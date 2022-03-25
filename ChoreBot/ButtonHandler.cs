using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using GiphyDotNet.Model.Parameters;
using GiphyDotNet.Manager;
using ByteDev.Giphy;
using System.Net.Http;
using Discord.WebSocket;
using ByteDev.Giphy.Request;

//using ByteDev.Giphy.Request;

namespace ChoreBot
{
    //The partial class contianing all our commands

    public partial class OldCMDs : ModuleBase<SocketCommandContext>
    {
        //View Account
        bool tts = false;
        Translate.lang lang = Translate.lang.English;
      
        public async Task ButtonHandler(SocketMessageComponent component,ulong guild_id,DiscordSocketClient client)
        {
            tts = TTS.doTTS(component.User);
            lang = Translate.getLang(component.User);
            switch (component.Data.CustomId) // these IDs are set in the /menu command (CMDhandler)
            {
                case "ID:balancebutton":
                      await viewbalance(component.User, component.Message, guild_id);  
                    break;
                case "ID:listchores":
                    await viewAccount(component.User, component.Message);
                    break;
                case "ID:help":
                    await helpAsync(component.Message);
                    break;
                case "ID:language":
                    await language(component.User, component.Message);
                    break;
                case "ID:CreateAccount":
                    await createAccount(component.User,component.Message, guild_id);
                    break;
                case "ID:addchore":
                    await takechorenew(component.User, component.Message, guild_id);
                    break;
                case "ID:completechore":
                    await completeChoreButton(component.User, component.Message);
                    break;
                case "ID:listallchores":
                    await listChores(component.Message, guild_id, client);
                    break;
                case "ID:review":
                    await reviewChores(component.User, component.Message, guild_id);
                    break;
                case "ID:unassign":
                    await unassignChore(component.User, component.Message, guild_id);
                    break;
                case "ID:scoreboard":
                    await scoreboard(component.Message, guild_id, client);
                    break;
                case "ID:tts":
                    await toggleTTSButton(component.User,component.Message);
                    break;
                case "ID:about":
                    await aboutAsync(component.Message);
                    break;

            }
        }

        public async Task MenuHandler(SocketMessageComponent component, ulong guild_id)
        {

            bool itts = TTS.doTTS(component.User);
            var ilang = Translate.getLang(component.User);

            var text = string.Join(", ", component.Data.Values);

            var theId = string.Join(", ", component.Data.CustomId);

            var lang = Translate.lang.English;

            bool doLanguage = true;

            if (theId == "menu-2")//add chore
            {
                var instance1 = new OldCMDs();
                doLanguage = false;
                await instance1.addChore(component.User, component.Message, text, guild_id);
            }
            if (theId == "menu-3")//complete chore
            {
                var instance1 = new OldCMDs();
                doLanguage = false;
                await instance1.choreComplete(component.User, component.Message, text);
            }
            if (theId == "menu-4")//complete chore review
            {
                var instance1 = new OldCMDs();
                doLanguage = false;
                await instance1.deleteChoreReview(component.User, component.Message, text, guild_id);
            }
            if (theId == "menu-5")//complete chore review
            {
                var instance1 = new OldCMDs();
                doLanguage = false;
                await instance1.deleteChoreUnassigned(component.User, component.Message, text, guild_id);
            }

            switch (text) // for each translation menu button // TODO: change the cases to change the specified language to the users database entry
            {
                case "English":
                    lang = Translate.lang.English;
                    break;
                case "Arabic":
                    lang = Translate.lang.Arabic;
                    break;
                case "Chinese":
                    lang = Translate.lang.Chinese;
                    break;
                case "French":
                    lang = Translate.lang.French;
                    break;
                 case "German":
                    lang = Translate.lang.German;
                    break;
                case "Hindi":
                    lang = Translate.lang.Hindi;
                    break;
                case "Indonesian":
                    lang = Translate.lang.Indonesian;
                    break;
                case "Irish":
                    lang = Translate.lang.Irish;
                    break;
                case "Italien":
                    lang = Translate.lang.Italien;
                    break;
                case "Japanese":
                    lang = Translate.lang.Japanese;
                    break;
                case "Korean":
                    lang = Translate.lang.Korean;
                    break;
                case "Polish":
                    lang = Translate.lang.Polish;
                    break;
                case "Portugese":
                    lang = Translate.lang.Portugese;
                    break;
                case "Russian":
                    lang = Translate.lang.Russian;
                    break;
                case "Spanish":
                    lang = Translate.lang.Spanish;
                    break;
                case "Turkish":
                    lang = Translate.lang.Turkish;
                    break;
                case "Vietnemese":
                    lang = Translate.lang.Vietnemese;
                    break;
            }

            if (doLanguage)
            {
                DBConnection db = new DBConnection();
                db.runCommand($"UPDATE users set lang={(int)lang} where discord_id='{component.User.Id}' and guild='{guild_id}'");
                await component.Channel.SendMessageAsync(component.User.Username + " " + await Translate.TranslateText($"selected {lang}", lang));
            }


        }


    }
}