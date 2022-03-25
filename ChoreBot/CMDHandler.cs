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
    public static partial class slashCMDs
    {
        public static async Task HandleCMD(SocketSlashCommand command, ulong guild_id)
        {
            //Check if user has TTS enabled
            bool tts = TTS.doTTS(command.User);
            var lang = Translate.getLang(command.User);
            switch (command.Data.Name)
            {
                case "ping":
                    await command.RespondAsync(CMDs.ping(), isTTS: tts);
                    break;
                case "help":
                    Embed e = CMDs.help();
                    string outtext = "";
                    if (tts)
                    {
                        outtext = TTS.unpackEmbed(e);
                    }
                    if (lang != Translate.lang.English)
                    {
                        await command.DeferAsync();
                        e = await Translate.TranslateEmbed(e, lang);
                        await command.Channel.SendMessageAsync(outtext, embed: e, isTTS: tts);
                    }
                    else
                    {
                        await command.RespondAsync(outtext, new Embed[] { e }, isTTS: tts);
                    }
                    break;
                case "about":
                    string abouttxt = CMDs.about();
                    if (lang != Translate.lang.English)
                    {
                        await command.DeferAsync();
                        abouttxt = await Translate.TranslateText(abouttxt, lang);
                        await command.Channel.SendMessageAsync(abouttxt, isTTS: tts);
                    }
                    else
                    {
                        await command.RespondAsync(abouttxt, isTTS: tts);
                    }
                    break;
                case "menu":
                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithTitle("Menu Options:").WithColor(Color.Blue);
                    var buttonbuilder = new ComponentBuilder();
                    buttonbuilder.WithButton("My Chores", "ID:listchores");
                    buttonbuilder.WithButton("List All Chores", "ID:listallchores");
                    buttonbuilder.WithButton("View Balance", "ID:balancebutton");
                    buttonbuilder.WithButton("Take Chore", "ID:addchore");
                    buttonbuilder.WithButton("Complete Chore", "ID:completechore");
                    buttonbuilder.WithButton("Review Chores", "ID:review");
                    buttonbuilder.WithButton("Unassign Chore", "ID:unassign");
                    buttonbuilder.WithButton("Scoreboard", "ID:scoreboard");
                    buttonbuilder.WithButton("Language", "ID:language");
                    buttonbuilder.WithButton("TTS", "ID:tts");
                    buttonbuilder.WithButton("Help", "ID:help");
                    buttonbuilder.WithButton("About", "ID:about");
                    buttonbuilder.WithButton("Create Account", "ID:CreateAccount");
                    //Do TTS
                    Embed e3 = builder.Build();
                    string posttxt = "";
                    if (tts)
                    {
                        posttxt = TTS.unpackEmbed(e3);
                    }
                    if (lang != Translate.lang.English)
                    {
                        await command.DeferAsync();
                        e3 = await Translate.TranslateEmbed(e3, lang);
                        await command.Channel.SendMessageAsync(embed: e3, component: buttonbuilder.Build(), isTTS: tts);
                    }
                    else
                    {
                        await command.RespondAsync(text: posttxt, embed: e3, component: buttonbuilder.Build(), isTTS: tts);
                    }
                    break;
                case "newchore":
                    var cmdops = command.Data.Options.ToDictionary(o => o.Name, o => o);
                    string desc = cmdops.ContainsKey("description") ? (string)cmdops["description"].Value : "";
                    IUser assigned = cmdops.ContainsKey("assigned") ? (IUser)cmdops["assigned"].Value : null;
                    long diff = cmdops.ContainsKey("difficulty") ? (long)cmdops["difficulty"].Value : 0;
                    Embed e5 = await CMDs.newchore((string)cmdops["name"].Value, command.User, guild_id, desc, assigned, diff);
                    string otext = "";
                    if (tts)
                    {
                        otext = TTS.unpackEmbed(e5);
                    }
                    if (lang != Translate.lang.English)
                    {
                        await command.DeferAsync();
                        e5 = await Translate.TranslateEmbed(e5, lang);
                        await command.Channel.SendMessageAsync(embed: e5, isTTS: tts);
                    }
                    else
                    {
                        await command.RespondAsync(otext, embed: e5, isTTS: tts);
                    }
                    break;
                default:
                    await command.RespondAsync("If you're seeing this, the Discord API screwed up the global command registry again. This isn't our fault!", isTTS: tts);
                    break;


            }
        }
    }
}
