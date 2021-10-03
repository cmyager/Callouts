using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Callouts.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireBungieLink : CheckBaseAttribute
    {
        public async override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            bool retval = true;
            var user = await ctx.Services.GetRequiredService<UserManager>().GetUserByUserId(ctx.User.Id);

            if (user.BungieId == null)
            {
                using var messageManager = new MessageManager(ctx);
                // add button to the clan website
                await messageManager.SendPrivateMessage("You must have a linked Bungie.net account to use that command.");
                retval = false;
            }
            return retval;
        }
    }
}
