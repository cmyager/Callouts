using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Callouts
{
    public class Register
    {


        [Command("Register"), Description("Register")]
        public async Task Registration(CommandContext ctx)
        {
            // TODO
            // This has to be here to let it register as a command
        }
    }
}
