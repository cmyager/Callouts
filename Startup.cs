using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using BungieSharper.Client;
using Callouts.Data;
using Callouts.DataContext;
using Discord.OAuth2;
using DSharpPlus;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Plk.Blazor.DragDrop;
using System;
using System.IO;

namespace Callouts
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var dbPath = $"{path}{Path.DirectorySeparatorChar}callouts.db";

            // TODO: Remove this later on once it is closer to done
            dbPath = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}callouts.db";
            Console.WriteLine($"DbPath is {dbPath}");

            services.AddDbContextFactory<CalloutsContext>(options => options.UseSqlite($"Data Source={dbPath}"));

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = DiscordDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddDiscord(x =>
                {
                    x.AppId = Configuration["Discord:ClientId"];
                    x.AppSecret = Configuration["Discord:ClientSecret"];
                    x.SaveTokens = true;
                    x.Scope.Add("guilds");
                });

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();

            var bungiecfg = new BungieClientConfig
            {
                ApiKey = Configuration["Bungie:ApiKey"],
                OAuthClientId = uint.Parse(Configuration["Bungie:ClientId"]),
                OAuthClientSecret = Configuration["Bungie:ClientSecret"],
                RateLimit = byte.Parse(Configuration["Bungie:RateLimit"])
            };

            var cfg = new DiscordConfiguration
            {
                Token = Configuration["Discord:BotToken"],
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.All
            };

            // TODO: the cool way of registering things by their type instead of manually.
            services.AddSingleton(s => new DiscordClient(cfg));
            services.AddSingleton(s => new BungieService(bungiecfg));
            services.AddSingleton<AsyncExecutionService>();
            services.AddSingleton<SchedulingService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<GuildManager>();
            services.AddSingleton<UserManager>();
            services.AddSingleton<ChannelManager>();
            services.AddSingleton<EventManager>();
            services.AddSingleton<ReportManager>();
            services.AddSingleton<PeriodicTaskService>();
            // TODO: Add more singletons here

            services.AddHttpContextAccessor();
            services.AddBlazorDragDrop();
            services.AddBlazorise(options =>
            {
                options.ChangeTextOnKeyPress = true;
            })
                    .AddBootstrapProviders()
                    .AddFontAwesomeIcons();
            services.AddHostedService<BotService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseForwardedHeaders();
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
