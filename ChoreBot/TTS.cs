using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ChoreBot
{
    public static class TTS
    {
        public static bool doTTS(IUser user)
        {
            DBConnection db = new DBConnection();
            bool tts = false;
            var rtts = db.runQuery($"select tts from users where discord_id='{user.Id}';");
            if (rtts.Read()) { tts = rtts.GetBoolean(0); }
            return tts;
        }

        public static async void readEmbed(Embed e, IUserMessage m)
        {
            string tempPost = unpackEmbed(e);
            IUserMessage me = await m.ReplyAsync(tempPost, isTTS: true);
            //me.DeleteAsync();
        }

        public static async void readSelectMenu(SelectMenuComponent c, IUserMessage m)
        {
            string rawmenu = "";
            foreach (var o in c.Options)
            {
                rawmenu += o.Label + "\n";
            }

            IUserMessage me = await m.ReplyAsync(rawmenu, isTTS: true);
        }

        public static string unpackEmbed(Embed e, char delim = '\n')
        {
            string output = e.Title.Replace("\n", " ") + delim + delim;
            foreach (var f in e.Fields)
            {
                output += f.Name.Replace("\n", " ");
                output += delim;
                output += f.Value.Replace("\n", " ");
                output += delim;
                output += delim;
            }
            return output;
        }

    }
}
