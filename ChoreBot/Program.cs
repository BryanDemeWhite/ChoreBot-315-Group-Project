using Discord;
using Discord.Net;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


namespace ChoreBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();
        private DiscordSocketClient discord;
        private CommandService command;
        private IServiceProvider service;


        public async Task RunBotAsync()
        {
            var config = new DiscordSocketConfig { MessageCacheSize = 10, AlwaysDownloadUsers = true  }; //I think this goes here
            discord = new DiscordSocketClient(config);
            // discord = new DiscordSocketClient();
            command = new CommandService();
            service = new ServiceCollection().AddSingleton(discord).AddSingleton(command).BuildServiceProvider();
      
            string botToken = "Discord Bot Token Here";
            
            //event subscription
            discord.Log += Log;
            discord.ButtonExecuted += MyButtonHandler;
            discord.SelectMenuExecuted += MyMenuHandler;
            
            

            discord.Ready += updateSlashCommands;
            discord.SlashCommandExecuted += SlashCommandHandler;
            
            await RegisterCommandAsync();

            await discord.LoginAsync(TokenType.Bot, botToken);
            await discord.StartAsync();
            await Task.Delay(-1);

        }

        //Not currently in use
        public async Task generateSlashCommands()
        {
            //generate a dictionary containing all unregistered commands
            var registeredcmds = await discord.GetGlobalApplicationCommandsAsync();
            //Use some LINQ magic to get a dictionary of unregistered commands
            //This prevents us from getting rate limited
            Dictionary<string, slashCMDs.CommandBody> unreg_cmds = slashCMDs.Commands.Where(pair => !registeredcmds.Any(c => c.Name == pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);

            //Register the new commands
            foreach (var c in unreg_cmds)
            {
                try
                { 
                    await discord.CreateGlobalApplicationCommandAsync(slashCMDs.generateCommand(c.Key, c.Value)); 
                }
                catch (Exception e) 
                { 
                    Console.WriteLine($"An exception: \"{e.Message}\", was thrown when adding command {c.Key}");
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public async Task updateSlashCommands()
        {
            List<ApplicationCommandProperties> appCommands = new List<ApplicationCommandProperties>();
            foreach (var command in slashCMDs.Commands)
            {
                appCommands.Add(slashCMDs.generateCommand(command.Key, command.Value));
            }
            try
            {
                await discord.BulkOverwriteGlobalApplicationCommandsAsync(appCommands.ToArray());
            } catch (Exception e)
            {
                Console.WriteLine($"An exception: \"{e.Message}\", was thrown when updating commands");
                Console.WriteLine(e.StackTrace);
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            ulong g_id = discord.Guilds.First(g => g.Channels.Any(c => c.Id == command.Channel.Id)).Id;
            await slashCMDs.HandleCMD(command, g_id);
        }
     

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandAsync()
        {
            discord.MessageReceived += HandleCommandAsync;
            await command.AddModulesAsync(Assembly.GetEntryAssembly(), service);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            int argPos = 0;

            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(discord, message);
            if (message == null)
                return;
            if (message.Author.IsBot) return;

            if (message.HasStringPrefix("$", ref argPos) || message.HasMentionPrefix(discord.CurrentUser, ref argPos))
            {

                var result = await command.ExecuteAsync(context, argPos, service);
                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
            }
        }
        public async Task MyButtonHandler(SocketMessageComponent component)
        {
            ulong g_id = discord.Guilds.First(g => g.Channels.Any(c => c.Id == component.Channel.Id)).Id;
            await component.DeferAsync();
            var instance3 = new OldCMDs();
            await instance3.ButtonHandler(component, g_id, discord);
     
        }
        public async Task MyMenuHandler(SocketMessageComponent component)
        {
            ulong g_id = discord.Guilds.First(g => g.Channels.Any(c => c.Id == component.Channel.Id)).Id;
            await component.DeferAsync();
            var instance4 = new OldCMDs();
            await instance4.MenuHandler(component, g_id);

           // var text = string.Join(", ", arg.Data.Values);
           // await arg.RespondAsync($"You have selected {text}");
        }
       
    }
}
