using BungieSharper.Entities.Destiny;
using BungieSharper.Entities.Destiny.HistoricalStats.Definitions;
using Callouts.Data;
using Callouts.DataContext;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Drawing;
using OpenQA.Selenium.Chrome;

namespace Callouts
{
    public class ReportManager
    {
        private readonly string ReportDeliminator = "_REPORT_";
        private readonly string ReportChannelName = "raid-reports";
        private readonly string ReportButtonText = "Get Raid Report";
        private readonly static string imageName = "{0}.png";


        private readonly IDbContextFactory<CalloutsContext> contextFactory;
        private readonly BungieService bungieService;
        private readonly ChannelManager channelManager;
        private readonly GuildManager guildManager;
        private readonly UserManager userManager;
        private readonly DiscordClient client;
        private readonly SchedulingService schedulingService;

        public ReportManager(IDbContextFactory<CalloutsContext> contextFactory,
                            BungieService bungieService,
                            ChannelManager channelManager,
                            GuildManager guildManager,
                            UserManager userManager,
                            DiscordClient client,
                            SchedulingService schedulingService)
        {
            this.contextFactory = contextFactory;
            this.bungieService = bungieService;
            this.channelManager = channelManager;
            this.guildManager = guildManager;
            this.userManager = userManager;
            this.client = client;
            this.schedulingService = schedulingService;
            client.Ready += OnReady;
            client.ComponentInteractionCreated += ComponentInteractionCreatedCallback;
            client.VoiceStateUpdated += OnVoiceStateUpdated;
        }

        /// <summary>
        /// OnReady
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            channelManager.AddRequiredChannel(ReportChannelName);
            return Task.CompletedTask;
        }

        /// <summary>
        /// ComponentInteractionCreatedCallback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ComponentInteractionCreatedCallback(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            // Only ack if it is an report button
            if (e.Interaction.Data.CustomId.Contains(ReportDeliminator))
            {
                // Respond so it doesn't get mad
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                // Request report
                _ = RequestReport(e.User.Id, e.Guild.Id);
            }
        }

        /// <summary>
        /// OnVoiceStateUpdated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            // If anyone enters a raid channel setup a repeating raid report request
            // It's okay to do this multiple times.The scheduler throws away dupe channel requsts
            // No need to stop on channel exit. The scheduled task will stop if channel is empty
            if (e.Channel != null && e.Channel.Name.Contains("Raid"))
            {
                await RequestReport(null, e.Guild.Id, e.Channel.Id, true);
            }
        }

        public async Task RequestReport(ulong? userId, ulong? guildId=null, ulong? channelId = null, bool repeat=false)
        {
            // check if user is registered.
            // Consider telling them to register?
            // This could be a little better
            if (userId != null && await userManager.GetUserByUserId(userId.Value) == null)
            {
                return;
            }

            DateTimeOffset executionTime = DateTimeOffset.Now;
            if (repeat)
            {
                executionTime = executionTime.AddMinutes(5);
            }
            FetchReport reportRequest = new()
            {
                DiscordUserId = userId,
                GuildId = guildId,
                IsRepeating = repeat,
                ExecutionTime = executionTime,
                ChannelId = channelId,
                Id = 0
            };
            schedulingService.ScheduleTask(reportRequest);
        }

        /// <summary>
        /// GetNewestRaidId
        /// </summary>
        /// <param name="discordUserId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private async Task<long> GetNewestRaidId(ulong discordUserId, bool filter)
        {

            List<DestinyComponentType> components = new() { DestinyComponentType.Characters };
            User user = await userManager.GetUserByUserId(discordUserId);
            var destinyProfile = await bungieService.GetProfile(user.PrimaryPlatformId.Value, user.Platform, components);
            if (destinyProfile == null)
            {
                throw new Exception();
            }
            List<long> instanceIds = new() { };
            foreach (var character in destinyProfile.Characters.Data)
            {
                var activity = await bungieService.GetActivityHistory(character.Value, DestinyActivityModeType.Raid, 1);
                if (activity != null)
                {
                    long instanceId = activity.Activities.ElementAt(0).ActivityDetails.InstanceId;
                    bool completed = activity.Activities.ElementAt(0).Values
                        .Where(p => p.Key == "completed").ElementAt(0).Value.Basic.DisplayValue == "Yes";
                    if (filter)
                    {
                        DateTime period = activity.Activities.ElementAt(0).Period;
                        if ((DateTime.UtcNow - period).TotalDays > 7)
                        {
                            instanceId = 0;
                        }
                    }
                    if (completed)
                    {
                        instanceIds.Add(instanceId);
                    }
                }
            }
            instanceIds.Sort();
            return instanceIds.Last();
        }

        /// <summary>
        /// GetAttending
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="recentRaidId"></param>
        /// <returns></returns>
        private async Task<string> GetAttending(DiscordGuild guild, long recentRaidId)
        {
            string retval = "";
            // wrap this all in a try except so the report doesn't fail to post just because users are not mentioned
            try
            {
                var report = await bungieService.GetPostGameCarnageReport(recentRaidId);
                if (report != null)
                {
                    foreach (var player in report.Entries)
                    {
                        var userInfo = player.Player.DestinyUserInfo;
                        var user = await userManager.GetUserByPlatformId(userInfo.MembershipType, userInfo.MembershipId);
                        if (user != null)
                        {
                            if (await guild.GetMemberAsync(user.UserId) != null)
                            {
                                retval += $"<@{user.UserId}> ";
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return retval;
        }

        /// <summary>
        /// GetReport
        /// </summary>
        /// <param name="reportRequest"></param>
        /// <returns></returns>
        public async Task GetReport(ulong discordUserId, ulong? guildId, bool filter)
        {
            // Make this work for private messages. will have to rework how we setup the guild/channel.
            // Best way would probably be to extend message manager

            if (await userManager.GetUserByUserId(discordUserId) == null)
            {
                return;
            }

            string errorMessage = null;
            MemoryStream raidReportImage = null;
            DiscordGuild guild = await client.GetGuildAsync(guildId.Value);
            var channel = await channelManager.GetChannel(guild, ReportChannelName);
            try
            {
                errorMessage = "Could not get recent raids from Bungie API. Try again later.";
                long recentRaidId = await GetNewestRaidId(discordUserId, filter);

                string RaidReportimageName = String.Format(imageName, recentRaidId);

                // Get all messages attachment names from the channel
                var messagesAttachments = ((await channel.GetMessagesAsync()).SelectMany(a => a.Attachments)).Select(a => a.FileName);

                if (!messagesAttachments.Contains(RaidReportimageName))
                {
                    // Get the raid report
                    errorMessage = "Error getting the report";
                    raidReportImage = GetRaidReportFromWeb(recentRaidId);
                    if (raidReportImage == null)
                    {
                        throw new Exception();
                    }
                    // Add user mentions
                    string users = "";
                    if (guildId != null)
                    {
                        users = await GetAttending(guild, recentRaidId);
                    }
                    // Send the report
                    await PostReport(channel, raidReportImage, RaidReportimageName, users);
                }
            }
            catch (Exception)
            {
                if (errorMessage != null)
                {
                    await channel.SendMessageAsync(errorMessage);
                }
            }
            finally
            {
                await CleanChannel(guild);
            }
        }

        /// <summary>
        /// PostReport
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="report"></param>
        /// <param name="imageName"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        private async Task PostReport(DiscordChannel channel, MemoryStream report, string imageName, string users)
        {
            DiscordMessageBuilder message = new();
            message.Content = users;
            message.WithFile(imageName, report);
            // Post the report
            await channel.SendMessageAsync(message);
        }

        /// <summary>
        /// CleanChannel
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        public async Task CleanChannel(DiscordGuild guild)
        {
            var ReportsChannel = await channelManager.GetChannel(guild, ReportChannelName);
            DiscordMessage newestMessage = null;

            foreach (DiscordMessage message in (await ReportsChannel.GetMessagesAsync()))
            {
                if (message.Author != client.CurrentUser)
                {
                    // Delete messages that are not from the bot
                    await ReportsChannel.DeleteMessageAsync(message);
                }
                if (message.Author == client.CurrentUser && !message.Attachments.Any())
                {
                    // Delete messages that are from the bot, but are not reports
                    await ReportsChannel.DeleteMessageAsync(message);
                }
                else if (DateTime.UtcNow - message.Timestamp > TimeSpan.FromDays(30))
                {
                    // Delete messages that are older than a month
                    await ReportsChannel.DeleteMessageAsync(message);
                }
                else if (newestMessage == null)
                {
                    // save the newest message
                    newestMessage = message;
                    if (!message.Components.Any())
                    {
                        DiscordMessageBuilder addButtonMessage = new();
                        addButtonMessage.Content = message.Content;
                        var button = new DiscordButtonComponent(ButtonStyle.Primary, $"{message.Channel.GuildId}{ReportDeliminator}{message.ChannelId}", ReportButtonText);
                        addButtonMessage.AddComponents(new List<DiscordComponent>() { button });
                        await message.ModifyAsync(addButtonMessage, attachments: message.Attachments);
                    }
                }
                else if (message.Components.Any())
                {
                    // Remove buttons from other messages
                    DiscordMessageBuilder newMessage = new DiscordMessageBuilder();
                    newMessage.Content = message.Content;
                    await message.ModifyAsync(newMessage, attachments: message.Attachments);
                }
            }
        }

        /// <summary>
        /// GetRaidReportFromWeb
        /// </summary>
        /// <param name="recentRaidId"></param>
        /// <returns></returns>
        private MemoryStream GetRaidReportFromWeb(long recentRaidId)
        {
            MemoryStream retval = null;
            ChromeOptions options = new();
            options.AddArguments("headless");
            ChromeDriver webDriver = new(options);
            try
            {
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                // Set it to dark theme
                webDriver.Navigate().GoToUrl("https://raid.report/settings");
                webDriver.FindElementByXPath("//span[text()='Dark']").Click();

                // set the window up so the report is always the same size
                webDriver.Navigate().GoToUrl($"https://raid.report/pgcr/{recentRaidId}");
                webDriver.Manage().Window.Position = new Point(0, 0);
                webDriver.Manage().Window.Size = new Size(1024, 768);

                // The waits seem to hate me so sleep for 2 seconds to let everything load.
                System.Threading.Thread.Sleep(2000);

                // Wait till the table isloaded
                webDriver.FindElementByClassName("pgcr-table");

                // Wait till the image in the title card is loaded
                WebDriverWait driverWait = new(webDriver, TimeSpan.FromSeconds(30));
                driverWait.Until((driver) =>
                {
                    try
                    {
                        var ele = driver.FindElement(By.ClassName("destiny-card"));
                        return ele.Displayed ? ele : null;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return null;
                    }
                });

                // remove the ad. It might not be loaded so wrap it in an except just to be safe
                try
                {
                    ((IJavaScriptExecutor)webDriver)
                        .ExecuteScript("var element=arguments[0];element.parentNode.removeChild(element);",
                                       webDriver.FindElementByXPath("/html/body/div/div/main/div/div[1]"));
                }
                catch (Exception) { }

                // get the report
                byte[] image = ((ITakesScreenshot)webDriver.FindElement(By.ClassName("side-container"))).GetScreenshot().AsByteArray;
                retval = new MemoryStream(image);
            }
            catch (Exception) { }
            finally
            {
                webDriver.Quit();
            }
            return retval;
        }
    }
}
