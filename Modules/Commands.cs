using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using System;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DIscordBotTest.Modules
{   
    //You can also add separately the commands you want on the bot. To do this,in addition to the main program code, we create a new class that we will call 'Commands'. To add commands so that the bot responds to them, do as I did below.
    public class Commands : ModuleBase<SocketCommandContext>
    {    
        [Command("commands")]
        public async Task Comandi()
        {
            await ReplyAsync("/comandi: Show avaible commands" +
                "\n.\n/create log: Create a news channel with the log of the bot" +
                "\n.\n/time: Show date and time");
        }

        [Command("time")]
        public async Task Time()
        {
            await ReplyAsync("Now is:\n"+ DateTime.Now);
        }  
    }
}
