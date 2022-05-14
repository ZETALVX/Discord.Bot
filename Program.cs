using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TutorialBot
{
    class Program
    {   
        //variables
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        public CommandService _commands;
        private IServiceProvider _services;

        private SocketGuild guild;

        //log channel info
        private ulong LogChannelID;
        private SocketTextChannel LogChannel;
        
        //run bot connection
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = "TOKEN";

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            await Task.Delay(-1);

            //now the bot is online
        }

        //client log
        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        //Register Commands Async
        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        // Read Input and get Output ( RECEIVE AND DO SOMETHING )
        public async Task HandleCommandAsync(SocketMessage arg)
        {   
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            var channel = _client.GetChannel(LogChannelID) as SocketTextChannel;         

            //console log with message received and user info
            Console.WriteLine("-------------\nUser:  " + message.Author.Username + " with ID  " + message.Author.Id +
                                                           "\nWrite:" +
                                                           "\n" + message.ToString());

            //return (exit and do nothing) if author of message is the bot
            if (message.Author.IsBot) return;

            int argPos = 0;

            //set command / for COMMAND actions
            if (message.HasStringPrefix("/", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
                if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);
            }

            //I usually transform text input in lower text ( A -> a ) to facilitate the reading of the text
            var text = message.ToString().ToLower();

            //Create a new channel with the log from the bot
            if (text == "/create log")
            {
                ulong messageChannelId = message.Channel.Id;
                var textChannel = _client.GetChannel(messageChannelId) as SocketTextChannel;
                guild = _client.GetGuild(textChannel.Category.GuildId);
                var newChannel= await guild.CreateTextChannelAsync("log");

                //SET ROLES OF THE CHANNEL LOG
                //bot role (higher) - AllowAll (show and send messages) 
                await newChannel.AddPermissionOverwriteAsync(guild.GetRole(967098609982124032), OverwritePermissions.AllowAll(newChannel));
                //admins role - "modify" to set single actions role - in this case, to show channel but with sendMessages: PermValue.Deny (can't send messages)
                await newChannel.AddPermissionOverwriteAsync(guild.GetRole(967417101558112381), OverwritePermissions.DenyAll(newChannel).Modify(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow, sendMessages: PermValue.Deny));
                //users role - users can't see the log channel - DENY ALL
                await newChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, OverwritePermissions.DenyAll(newChannel));

                //log channel created - notify with text
                LogChannelID = newChannel.Id;
                await textChannel.SendMessageAsync("Channel created - Open it to see the Bot's log.");
                await newChannel.SendMessageAsync("Channel created - Here you can see the Bot's log.");
            }

            //log massage in log channel with action of users and text
            if (channel != null)
             {
                await channel.SendMessageAsync("-------------\nUser:  " + message.Author.Username + " with ID  " + message.Author.Id +
                                                           "\nWrite:" +
                                                           "\n" + message.ToString());
             }

            //read input and do something

            //answer to the user
            switch (text)
            {
                case "hi bot": 
                    await message.Channel.SendMessageAsync("Hello "+ message.Author.Username+"!"); 
                    break;
                case "hello bot":
                    await message.Channel.SendMessageAsync("Hello " + message.Author.Username + "!");
                    break;
            }
            //block and delete words
            if (text.Contains("bad word") || message.ToString().ToLower().Contains("badword"))
            {
                await message.Channel.SendMessageAsync("You can't say that things!\n\nMessage removed from the chat.");
                await message.DeleteAsync();
                //log in the log channel with the action
                await channel.SendMessageAsync("ACTION: Bot removed the message for BAD WORDS");
            }
        }
    }
}
