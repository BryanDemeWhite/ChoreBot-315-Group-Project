using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Webhook;
using Discord.Commands;


namespace ChoreBot
{
    
    //The partial class contianing all our commands
    public static partial class CMDs
    {

        //   public static string menuAsync()
        //  { 

        //    return "$menu";
        // }
        //DEPRECATED
        public static Embed balanceAsync(IUser user)
        {
            var discImage = user.GetAvatarUrl();
            // var discImage = Context.Message.Author.GetAvatarUrl();//
            //   var discID = Context.Message.Author.Id;
            var discID = user.Id;
            var UNick = (user as SocketGuildUser).Nickname;
            DBConnection db = new DBConnection();


            var rpoint = db.runQuery("select points from users where discord_id='" + discID + "';");
            rpoint.Read();
            var points = rpoint.GetInt32(0);
           // Console.Write(discID);


            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(UNick + "'s Balance").WithColor(Color.DarkGreen);
            builder.WithThumbnailUrl(discImage);
            // builder.AddField("User:", user.Mention);
            builder.AddField("Points:", points);


            return builder.Build();

        }
        //DEPRECATED
        public static async Task<Embed> takechoreAsync(string choreName,IUser user)
        {
            var discID = user.Id;

            DBConnection db = new DBConnection();
            var rchore = db.runQuery("select id from chores where name='" + choreName + "';");

            rchore.Read();
            var choreID = rchore.GetValue(0);

            rchore.Close();

            var r = db.runQuery("UPDATE users SET chores = array_append(chores, " + choreID + ") where discord_id = " + discID + "::text;");

            string gif = await GIPHY.getRandomGIF(choreName, 5);

            var UNick = (user as SocketGuildUser).Nickname;
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(UNick + " has been assigned the chore: " + choreName).WithColor(Color.Green);
            builder.WithImageUrl(gif);
          //  await ReplyAsync("", false, builder.Build());

            // await Context.Channel.SendMessageAsync(Context.User.Mention + " you have been assigned the Chore: " + choreName);

            //TODO: make add chore remove the chore from the global list (since you have taken that chore)


            return builder.Build();

        }
        //DEPRECATED
        public static Embed createAccountAsync(IUser user, ulong guild)
        {
            var discID = user.Id;

            //var discName = user.Username;
            var discGuild = guild;
            DBConnection db = new DBConnection();

            var rchore = db.runQuery("insert into users (discord_id,guild,points) Values ('" + discID + "','" + discGuild + "',0) on conflict (discord_id) where (discord_id='" + discID + "') do nothing ;");
           
            var UNick = (user as SocketGuildUser).Nickname;
            var discImage = user.GetAvatarUrl();
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(UNick + "'s Account Has Been Created").WithColor(Color.DarkGreen);
            builder.WithThumbnailUrl(discImage);
            // builder.AddField("User:", user.Mention);
           


            return builder.Build();
            //await Context.Channel.SendMessageAsync("Chore Account Created for " + Context.User.Mention);

        }
        //DEPRECATED
        public static string toggleTTS(IUser u, bool tts)
        {
            DBConnection db = new DBConnection();
            if (tts)
            {
                if (db.runCommand($"update users set tts=false where discord_id='{u.Id}';") < 1)
                {
                    throw new Exception($"failed to disable tts for {u.Username}");
                }
                return $"{u.Username} disabled Text to Speech";
            }
            else
            {
                if (db.runCommand($"update users set tts=true where discord_id='{u.Id}';") < 1)
                {
                    throw new Exception($"failed to enable tts for {u.Username}");
                }
                return $"{u.Username} enabled Text to Speech";
            }

        }
    }
}