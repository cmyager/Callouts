using BungieSharper.Entities;
using BungieSharper.Entities.Destiny.Responses;
using BungieSharper.Entities.User;
using Callouts.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.Linq;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System;
namespace Callouts
{
    // This one is a bit weird to think out so I am winging it and making it very server specific
    public class RoleManager
    {
        private class Role
        {
            public readonly string Name;
            public readonly Permissions Permissions;
            public readonly DiscordColor Color;
            public readonly bool Hoist = true;
            public readonly bool Mentionable = true;
            public Role(string name, DiscordColor color, Permissions permissions)
            {
                Name = name;
                Color = color;
                Permissions = permissions;
            }
        }
        private readonly string GuestPassDeliminator = "_GUESTPASS_";
        private readonly string AdminRoleName = "Admin";
        private readonly string MemberRoleName = "Member";
        private readonly string GuestRoleName = "Guest";
        private readonly string EveryoneRoleName = "@everyone";

        private readonly List<Role> Roles = new();
        private readonly DiscordClient Client;
        public RoleManager(DiscordClient client)
        {
            Client = client;
            Roles.Add(new Role(AdminRoleName, DiscordColor.Green, Permissions.Administrator));
            Roles.Add(new Role(MemberRoleName, DiscordColor.DarkRed, Permissions.ChangeNickname
                                                                     | Permissions.AccessChannels
                                                                     | Permissions.CreateInstantInvite
                                                                     | Permissions.AddReactions
                                                                     | Permissions.SendMessages
                                                                     | Permissions.UseExternalEmojis
                                                                     | Permissions.UseVoice
                                                                     | Permissions.MentionEveryone
                                                                     | Permissions.ReadMessageHistory
                                                                     | Permissions.AttachFiles
                                                                     | Permissions.Speak
                                                                     | Permissions.UseSlashCommands
                                                                     | Permissions.UseVoiceDetection));
            Roles.Add(new Role(GuestRoleName, DiscordColor.HotPink, Permissions.ChangeNickname
                                                                    | Permissions.AccessChannels
                                                                    | Permissions.SendMessages
                                                                    | Permissions.UseVoice
                                                                    | Permissions.Speak
                                                                    | Permissions.UseVoiceDetection));
            Roles.Add(new Role(EveryoneRoleName, DiscordColor.None, Permissions.None));
            client.GuildAvailable += CreateRequiredRolesOnJoin;
            client.GuildCreated += CreateRequiredRolesOnJoin;
            client.ComponentInteractionCreated += GuestRollRequested;
        }

        private async Task CreateRequiredRolesOnJoin(DiscordClient sender, GuildCreateEventArgs e)
        {
            foreach (Role role in Roles)
            {
                KeyValuePair<ulong, DiscordRole> existingRole = e.Guild.Roles.Where(p => p.Value.Name == role.Name).FirstOrDefault();
                if (existingRole.Value == null)
                {
                    await e.Guild.CreateRoleAsync(role.Name, role.Permissions, role.Color, role.Hoist, role.Mentionable);
                }
                else if (existingRole.Value.Name == AdminRoleName)
                {
                    // Skip messing with admin
                }
                else if (existingRole.Value.Permissions != role.Permissions)
                {
                    await existingRole.Value.ModifyAsync(role.Name, role.Permissions, role.Color, role.Hoist, role.Mentionable);
                }
            }
        }

        public async Task PostWelcomeMessage(DiscordChannel channel)
        {
            var pinnedMessages = await channel.GetPinnedMessagesAsync();
            if (!pinnedMessages.Any())
            {
                var e = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Blue,

                    Title = "Welcome to The Clan Without A Plan!",
                    Footer = new DiscordEmbedBuilder.EmbedFooter()
                    {
                        Text = "Visit our website to link your bungie account to use all of the bots cool feature."
                    }
                };
                e.AddField("Rules",
                    "Please follow these so we can all have fun.");
                e.AddField("Be Respectful",
                    "You must respect all users regardless of your liking towards them.\nTreat others the way that you want to be treated.");
                e.AddField("No Spamming",
                    "Don’t send a lot of small messages right after each other.\nDo not disrupt the chat by spamming.");
                e.AddField("No Pornographic / Adult / Other NSFW material",
                    "Any posting of inappropriate material will result in an immediate removal from the server and from the clan.\nThere will be no warnings.");
                e.AddField("No Advertisements",
                    "Only post content that is relevant and adds value to the game.");
                e.AddField("No offensive profile pictures",
                    "You will be asked to change your picture if the Admins deem it to be inappropriate.");
                List<DiscordComponent> buttons = new()
                {
                    new DiscordLinkButtonComponent("https://theclanwithoutaplan.com", "Link Bungie.net Account"),
                    new DiscordButtonComponent(ButtonStyle.Primary, $"{channel.GuildId}{GuestPassDeliminator}", "Use Guest Roll")
                };
                var builder = new DiscordMessageBuilder();
                builder.AddEmbed(e);
                builder.AddComponents(buttons);
                await (await channel.SendMessageAsync(builder)).PinAsync();
            }
        }

        private async Task GuestRollRequested(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            // Only ack if it is an event button
            if (e.Interaction.Data.CustomId.Contains(GuestPassDeliminator))
            {
                // Respond so it doesn't get mad
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
                if (!member.Roles.Any())
                {
                    KeyValuePair<ulong, DiscordRole> GuestRole = e.Guild.Roles.Where(p => p.Value.Name == GuestRoleName).FirstOrDefault();
                    if (GuestRole.Value != null)
                    {
                        await member.GrantRoleAsync(GuestRole.Value, "User requested");
                    }
                }
            }
        }

        public async Task UpdateMemberRole(ulong userId, bool delete = false)
        {
            foreach ((ulong _, DiscordGuild guild) in Client.Guilds)
            {
                DiscordMember member = await guild.GetMemberAsync(userId);
                if (member != null)
                {
                    KeyValuePair<ulong, DiscordRole> MemberRole = guild.Roles.Where(p => p.Value.Name == MemberRoleName).FirstOrDefault();
                    if (MemberRole.Value == null)
                    {
                        // the member role doesn't exist. This shouldn't happen
                    }
                    else if (delete == true)
                    {
                        // Remove the member role
                        await member.RevokeRoleAsync(MemberRole.Value, "User unlinked account");
                    }
                    else
                    {
                        // add the member role
                        await member.GrantRoleAsync(MemberRole.Value, "User linked account");
                    }
                }

            }
        }

        public async Task ClearGuestRolls()
        {
            foreach ((ulong _, DiscordGuild guild) in Client.Guilds)
            {
                KeyValuePair<ulong, DiscordRole> GuestRole = guild.Roles.Where(p => p.Value.Name == GuestRoleName).FirstOrDefault();
                if (GuestRole.Value != null)
                {
                    foreach ((ulong _, DiscordMember member) in guild.Members)
                    {
                        if (member.Roles.Contains(GuestRole.Value))
                        {
                            await member.RevokeRoleAsync(GuestRole.Value, "Automatic removal");
                        }
                    }
                }
            }
        }
    }
}
