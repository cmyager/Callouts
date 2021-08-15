using System;
using Microsoft.AspNetCore.Authentication;
using Discord.OAuth2;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.OAuth2
{
    public static class DiscordDefaults
    {
        public const string AuthenticationScheme = "Discord";
        public const string DisplayName = "Discord";

        public static readonly string AuthorizationEndpoint = "https://discordapp.com/api/oauth2/authorize";
        public static readonly string TokenEndpoint = "https://discordapp.com/api/oauth2/token";
        public static readonly string UserInformationEndpoint = "https://discordapp.com/api/users/@me";
    }
    public static class DiscordAuthenticationOptionsExtensions
    {
        public static AuthenticationBuilder AddDiscord(this AuthenticationBuilder builder)
            => builder.AddDiscord(DiscordDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddDiscord(this AuthenticationBuilder builder, Action<DiscordOptions> configureOptions)
            => builder.AddDiscord(DiscordDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddDiscord(this AuthenticationBuilder builder, string authenticationScheme, Action<DiscordOptions> configureOptions)
            => builder.AddDiscord(authenticationScheme, DiscordDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddDiscord(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<DiscordOptions> configureOptions)
            => builder.AddOAuth<DiscordOptions, DiscordHandler>(authenticationScheme, displayName, configureOptions);
    }
}