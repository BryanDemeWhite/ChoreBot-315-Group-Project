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
          //  [Command("myChores")]
        public async Task viewAccount(IUser user, IUserMessage messageContext)  //my chores button
        {
            var discImage = user.GetAvatarUrl();
            var discID = user.Id;
            DBConnection db = new DBConnection();
            string temp = db.runScalar($"select discord_id from users where discord_id='{discID}';");
            if (temp == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            var rchore = db.runQuery($"select array_agg(name) from chores inner join (select distinct unnest(chores) as cid from users where discord_id='{discID}') as t on t.cid = chores.id;");

            rchore.Read();
            object cListOut = rchore.GetValue(0);
            string[] choreList = {"No Chores"};
            if (!(cListOut is System.DBNull)) choreList = (string[])rchore.GetValue(0);

            var UNick = user.Username; //swapped to username
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(UNick+"'s Chores").WithColor(Color.Purple);
            builder.WithThumbnailUrl(discImage);
            //builder.AddField("User:", Context.Message.Author.Mention);
            


            builder.AddField("Current Chores:", string.Join(", ", choreList));
            //builder.AddField("User ID (debugging only):", discID);

            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", embed: e);

        }
       // [Command("balance")] // balance button
        public async Task viewbalance(IUser user,IUserMessage messageContext, ulong guild_id)
        {
            var discImage = user.GetAvatarUrl();
            var discID = user.Id;
            var UNick = user.Username; //swapped nickname for username

            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{discID}' and guild = '{guild_id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            var rpoint = db.runQuery($"select points from users where discord_id='{discID}' and guild='{guild_id}';");
            rpoint.Read();
            var points = rpoint.GetInt32(0);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(UNick +"'s Balance").WithColor(Color.DarkGreen);
            builder.WithThumbnailUrl(discImage);
            builder.AddField("Points:", points);

            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", false, e);

 
        }
       // [Command("CreateAccount")]// FOR GUILDS TO WORK, ALL QUERIES MUST CHECK FOR WHERE (DISCID AND GUILD) // create account button
        public async Task createAccount(IUser user, IUserMessage test, ulong discGuild)
        {
           var discID = user.Id;

            // var discName = Context.Message.Author.Username;
            //var discGuild = Context.Guild.Id;
            DBConnection db = new DBConnection();

           // var rcheck = db.runQuery("select * from users where discord_id= '" + discID + "' and guild = '" + discGuild + "';");

            //var rchore = db.runQuery("insert into users (discord_id,guild,points,tts,lang) Values ('" + discID + "','"+ discGuild + "',0,false,0) on conflict (discord_id) where (discord_id='" + discID + "') do nothing ;");
             if (db.runCommand("insert into users (discord_id,guild,points,tts,lang) Values ('" + discID + "','" + discGuild + "',15,false,0) on conflict (discord_id,guild) where (discord_id='" + discID + "' and guild='"+discGuild +"') do nothing ;") != 0) {
                string outtext = "Chore account created for ";
                if (lang != Translate.lang.English)
                {
                    outtext = await Translate.TranslateText(outtext, lang);
                }

                await test.ReplyAsync(outtext + user.Username, isTTS:tts);
            }
            else
            {
                string outtext = "You already have an account in this server ";
                if (lang != Translate.lang.English)
                {
                    outtext = await Translate.TranslateText(outtext, lang);
                }
                await test.ReplyAsync(outtext + user.Username, isTTS:tts);
            }
           // var rchore = db.runQuery("insert into users (discord_id,guild,points,tts,lang) Values ('" + discID + "','" + discGuild + "',15,false,0);");
        
            

        }
        
        public async Task addChore(IUser user,IUserMessage messageContext, string choreName, ulong guild_id)
        {
            var discID = user.Id;

            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{discID}';") == "")
            {
                string outtext = "You do not yet have an account. Please use the create account button to create one!";
                if (lang != Translate.lang.English)
                {
                    outtext = await Translate.TranslateText(outtext, lang);
                }
                await messageContext.ReplyAsync(outtext);
                return;
            }
            var rchore = db.runQuery($"select id from chores where name='{choreName}' and guild='{guild_id}';");

            rchore.Read();
            var choreID = rchore.GetValue(0);

            rchore.Close();

            db.runCommand($"UPDATE users SET chores = array_append(chores, {choreID}) where discord_id = '{discID}' and guild = '{guild_id}';");
            db.runCommand($"UPDATE users SET points = points + 5 where discord_id = '{discID}' and guild = '{guild_id}';"); //award points for taking a chore
            db.runCommand("UPDATE chores SET discord_id = '"+user.Id+"' where name=  '" +choreName+"';"); 

            string gif = await GIPHY.getRandomGIF(choreName, 5);

            var UNick = user.Username; //swapped Nickname for username
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(UNick + " has been assigned the chore: " + choreName).WithColor(Color.Green);
            builder.WithImageUrl(gif);

            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", false, e);
            


        }
        public async Task takechorenew(IUser user, IUserMessage messageContext, ulong guild_id)
        {

            EmbedBuilder builder1 = new EmbedBuilder();
            builder1.WithTitle("Chores:").WithColor(Color.Green);



            var menuBuilder = new SelectMenuBuilder();
            menuBuilder.WithPlaceholder("Select a chore to take");
            menuBuilder.WithCustomId("menu-2");
            menuBuilder.WithMinValues(1);
            menuBuilder.WithMaxValues(1);

            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{user.Id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            var rchore = db.runQuery($"select * from chores where discord_id is null and guild = '{guild_id}' and finished = false;");

            int count = 1;

            while (rchore.Read())
            {
                // builder.AddField((count + 1) + ". " + rchore.GetString(1) + " (Chore ID: " + rchore.GetInt32(0) + ")", "Description,Difficulty,Point Reward ");
                menuBuilder.AddOption(rchore.GetString(1), rchore.GetString(1), rchore.GetString(1));

                //  Console.WriteLine("ran times:" + count);

                // builder.AddField((count + 1) + ". " + rchore.GetString(1) + " (Chore ID: " + rchore.GetInt32(0) + ")", "----------------------------------------");

                count++;

            }

            var builder = new ComponentBuilder()
             .WithSelectMenu(menuBuilder);

            //Do TTS
            Embed e = builder1.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                SelectMenuComponent smc = menuBuilder.Build();
                TTS.readEmbed(e, messageContext);
                TTS.readSelectMenu(smc, messageContext);
            }
            await messageContext.ReplyAsync(embed: e, components: builder.Build());

        }
        public async Task completeChoreButton(IUser user, IUserMessage messageContext) //////////////////// COMPLETE CHORE BUTTON
        {
            var discID = user.Id;

            EmbedBuilder builder1 = new EmbedBuilder();
            builder1.WithTitle("Assigned Chores:").WithColor(Color.Green);

            var menuBuilder = new SelectMenuBuilder();
            menuBuilder.WithPlaceholder("Select the chore you have completed");
            menuBuilder.WithCustomId("menu-3");
            menuBuilder.WithMinValues(1);
            menuBuilder.WithMaxValues(1);

            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{discID}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            var rchore = db.runQuery($"select array_agg(name) from chores inner join (select distinct unnest(chores) as cid from users where discord_id='{discID}') as t on t.cid = chores.id;");

            rchore.Read();
            object cListOut = rchore.GetValue(0);
            string[] choreList = { "No Chores" };
            if (!(cListOut is System.DBNull)) choreList = (string[])rchore.GetValue(0);

            int count = 1;

            foreach (string item in choreList) // Loop through List with foreach
            {
                Console.WriteLine(item);
                menuBuilder.AddOption(item, item, item);

                count++;
            }
            var builder = new ComponentBuilder()
             .WithSelectMenu(menuBuilder);

            //Do TTS
            Embed e = builder1.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                SelectMenuComponent smc = menuBuilder.Build();
                TTS.readEmbed(e, messageContext);
                TTS.readSelectMenu(smc, messageContext);
            }
            await messageContext.ReplyAsync(embed: e, components: builder.Build());



        }

       // [Command("choreComplete")] 
        public async Task choreComplete(IUser user, IUserMessage messageContext, string choreName)
        {

            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{user.Id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            var r = db.runQuery("update chores set finished = true where name = '"+choreName+"';");

            string gif = await GIPHY.getRandomGIF(choreName, 5);
            var UNick = user.Username; //swapped out nickname
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(UNick + " has completed the chore: " + choreName).WithColor(Color.Green);
            builder.WithDescription("This chore is now under review (click the review button to see completed chores and mark them as done)");
            builder.WithImageUrl(gif);


            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", false, e);



        }
        public async Task completeChoreReply(IUserMessage messageContext)
        {
            await messageContext.ReplyAsync("Type the name of the chore you want to create (Chore Name | Description | Difficulty Value|Assigned To(takes 25 points)) and click New Chore, an example of a valid chore would be 'Do the dishes | Thoroughly clean the dishes in the kitchen sink with warm water and soap | 1| ' ");
        }
        public async Task language(IUser user, IUserMessage messageContext) ///////////////LANGUAGE BUTTON MENU
        {
            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{user.Id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }


            EmbedBuilder builder1 = new EmbedBuilder();
            builder1.WithTitle("Select a Language:").WithColor(Color.Green);

            var menuBuilder = new SelectMenuBuilder()
                .WithPlaceholder("Select an option")
                .WithCustomId("menu-1")
                .WithMinValues(1)
                .WithMaxValues(1)
                .AddOption("English", "English", " ")
                .AddOption("Arabic", "Arabic", " ")
                .AddOption("Chinese", "Chinese", " ")
                .AddOption("French", "French", " ")
                .AddOption("German", "German", " ")
                .AddOption("Hindi", "Hindi", " ")
                .AddOption("Indonesian", "Indonesian", " ")
                .AddOption("Irish", "Irish", " ")
                .AddOption("Italien", "Italien", " ")
                .AddOption("Japanese", "Japanese", " ")
                .AddOption("Korean", "Korean", " ")
                .AddOption("Polish", "Polish", " ")
                .AddOption("Portugese", "Portugese", " ")
                .AddOption("Russian", "Russian", " ")
                .AddOption("Spanish", "Spanish", " ")
                .AddOption("Turkish", "Turkish", " ")
                .AddOption("Vietnemese", "Vietnemese", " "); 

            var builder = new ComponentBuilder()
                .WithSelectMenu(menuBuilder);


            //Do TTS
            Embed e = builder1.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                SelectMenuComponent smc = menuBuilder.Build();
                TTS.readEmbed(e, messageContext);
                TTS.readSelectMenu(smc, messageContext);
            }
            await messageContext.ReplyAsync(embed: e, components: builder.Build());

        }
        // [Command("createChore")]///////////////////////CREATE CHORE BUTTON
        public async Task createChore(IUser user, IUserMessage messageContext, string choreName, string desc, string diff, string assigned, ulong guild_id, DiscordSocketClient client)
        {
            int diffint = Int32.Parse(diff);

            //TODO: make createchore take guild id*********************************

            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{user.Id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            // db.runCommand($"insert into chores(name, description, difficulty, guild) Values ('{choreName}', '{desc}', {diff}, '{guild_id}');");
            db.runCommand($"insert into chores(name, description, difficulty, guild) Values ('{choreName}', '{desc}', '{diffint}', '{guild_id}');");

            string gif = await GIPHY.getRandomGIF(choreName, 5);

            var UNick = user.Username;//swapped out nickname
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle(UNick + " created the chore: " + choreName).WithColor(Color.Blue);
            builder.WithImageUrl(gif);
            //await messageContext.ReplyAsync("", false, builder.Build());
            string assignedID = assigned;
            assignedID = assignedID.Replace("!", "");
            assignedID = assignedID.Replace("@", "");
            assignedID = assignedID.Replace(">", "");
            assignedID = assignedID.Replace("<", "");
            Console.WriteLine(">|" + assignedID + "|<");

            IUser name = await client.GetUserAsync(Convert.ToUInt64(assignedID));

            int points = 0;

            var rchore = db.runQuery($"select points from users where guild='{guild_id}' and discord_id='{user.Id}';");
            while (rchore.Read())
            {
                points = rchore.GetInt32(0);

            }
            if (assigned != "" && points < 25)
            {
                builder.WithDescription("YOU DO NOT HAVE ENOUGH POINTS: " + points + "/25");

            }
            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", false, e);


            if (assigned != "" && points >= 25)
            {
                await addChore(name, messageContext, choreName, guild_id);

            }
         

        }
        
        // [Command("listChores")] //////////////////////////////////////LIST CHORES BUTTON
        public async Task listChores(IUserMessage messageContext, ulong guild_id, DiscordSocketClient client)
        {
            EmbedBuilder builder = new EmbedBuilder();
            Npgsql.NpgsqlDataReader rchore;
            DBConnection db = new DBConnection();
            rchore = db.runQuery($"select * from chores where finished=false and guild='{guild_id}' limit 24;");

            int[] vChoreIndex = { };
            string[] vChoreName = { };

            int count = 0;
            builder.WithTitle("Chores:").WithColor(Color.Purple);
            while (rchore.Read() && count < 24)
            {
                string n = rchore.GetString(1);
                int id = rchore.GetInt32(0);
                string description = "----------------------------------------";
                //try { description = rchore.GetString(2) + "Difficulty: " + rchore.GetInt32(3).ToString() + "Assigned To" + rchore.GetString(6); } //THIS DOES NOT WORK IF ASSIGNED TO NO ONE, FIX THIS, AND FORMATE RIGHT
                //catch { description = "----------------------------------------"; }


                description = rchore.GetString(2) + "\nDifficulty: ";
                int diffValue = rchore.GetInt32(3);
                string diffString = "Not Set";
                if (diffValue == 1)
                {
                    diffString = "Easy";
                }
                else if (diffValue == 2)
                {
                    diffString = "Medium";
                }
                else if (diffValue == 3)
                {
                    diffString = "Hard";
                }

                description += diffString + "\nAssigned To: ";

                string AssignedString = "Unassigned";
                try
                {
                    IUser name = await client.GetUserAsync(Convert.ToUInt64(rchore.GetString(6)));
                    AssignedString = name.Username;
                }
                catch { }
                finally
                {
                    description += AssignedString;
                }

                if (description.Trim() == "")
                {
                    description = "----------------------------------------";
                }
                try
                {
                    builder.AddField($"{count + 1}. {n}", description);
                }
                catch
                {
                    Console.WriteLine($"Exception in listchore builder with chore name: {n} and id: {id} and count: {count}");
                    throw;
                }
                count++;
            }
            if (count == 24)
            {
                builder.AddField("And more", "..."); //figure out how many more?
            }

            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", false, e);
        }
        /////////////////////////////////////////////////////////////////////Review Chores Button / Menu
        public async Task reviewChores(IUser user, IUserMessage messageContext, ulong guild_id)
        {

            EmbedBuilder builder1 = new EmbedBuilder();
            builder1.WithTitle("Mark chore as complete and delete it:").WithColor(Color.Green);



            var menuBuilder = new SelectMenuBuilder();
            menuBuilder.WithPlaceholder("review a chore");
            menuBuilder.WithCustomId("menu-4");
            menuBuilder.WithMinValues(1);
            menuBuilder.WithMaxValues(1);

            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{user.Id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            var rchore = db.runQuery($"select * from chores where finished = true and guild='{guild_id}';");

            int count = 1;

            while (rchore.Read())
            {

                menuBuilder.AddOption(rchore.GetString(1), rchore.GetString(1), "Mark as complete and delete chore?");

                count++;
            }

            if (count == 1)
            {
                await messageContext.ReplyAsync("No chores to be reviewed currently...", tts);
                return;
            }

            var builder = new ComponentBuilder()
             .WithSelectMenu(menuBuilder);

            //Do TTS
            Embed e = builder1.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                SelectMenuComponent smc = menuBuilder.Build();
                TTS.readEmbed(e, messageContext);
                TTS.readSelectMenu(smc, messageContext);
            }
            await messageContext.ReplyAsync(embed: e, components: builder.Build());


        }

        public async Task deleteChoreReview(IUser user, IUserMessage messageContext, string choreName, ulong guild_id)
        {
            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{user.Id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            var rchore = db.runQuery("select discord_id from chores where name = '" + choreName + "';");
            var assigned_disc_id = "";
            while (rchore.Read())
            {
                assigned_disc_id = rchore.GetString(0);
                Console.WriteLine("THE ID OF THE CHORE COMPLETER 1:" + assigned_disc_id);


            }
            Console.WriteLine("THE ID OF THE CHORE COMPLETER 2:" +assigned_disc_id);

            rchore.Close();
               
            var diff = db.runQuery($"select difficulty from chores where name='{choreName}' and guild='{guild_id}';"); 
            int diffpoints = 0;
                 while (diff.Read())
            {
                diffpoints= diff.GetInt32(0);
                Console.WriteLine("DIFF LEVEL 1:"+ diffpoints);
            }
            diff.Close();
            Console.WriteLine("DIFF LEVEL 2:" + diffpoints);

            int numpoint = 0;
            if (diffpoints == 1){
                numpoint = 5;
            }
            if (diffpoints == 2)
            {
                numpoint = 10;

            }
            if (diffpoints == 3)
            {
                numpoint = 15;

            }
            db.runCommand($"select rem_chore('{choreName}', '{guild_id}');");

            db.runCommand($"update users set points=points+{numpoint} where discord_id='{assigned_disc_id}' and guild='{guild_id}';"); 

            db.runCommand("delete from chores where name = '" + choreName + "';");

            string gif = await GIPHY.getRandomGIF(choreName, 5);

            var UNick = (user as SocketGuildUser).Nickname;
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(UNick + " reviewed and marked as complete : " + choreName).WithColor(Color.Red);
            builder.WithImageUrl(gif);


            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", false, e);

        }
        public async Task scoreboard(IUserMessage messageContext, ulong guild_id, DiscordSocketClient client)
        {
            DBConnection db = new DBConnection();
            var amount = 0;
            var disc_id = "";

            var rchore = db.runQuery("select * from users where guild='" + guild_id + "' order by points desc;");
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Leaderboard:").WithColor(Color.Purple);

            IUser name;
            int count = 0;


            while (rchore.Read())
            {

                amount = rchore.GetInt32(4);
                disc_id = rchore.GetString(1);
                ulong id = Convert.ToUInt64(disc_id);
                name = await client.GetUserAsync(Convert.ToUInt64(disc_id));

                // IUser name = new IUser;
                builder.AddField($"{count + 1}. {name.Username}", "Points: " + amount);
                Console.WriteLine("scoreboard ran this many times :" + count);
                count++;


            }
            // await messageContext.ReplyAsync("", false, builder.Build());
            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", false, e);


        }
        public async Task unassignChore(IUser user, IUserMessage messageContext, ulong guild_id)
        {

            EmbedBuilder builder1 = new EmbedBuilder();
            builder1.WithTitle("Select a chore to unassign from yourself for 10 points:").WithColor(Color.Green);



            var menuBuilder = new SelectMenuBuilder();
            menuBuilder.WithPlaceholder("Unassign a Chore");
            menuBuilder.WithCustomId("menu-5");
            menuBuilder.WithMinValues(1);
            menuBuilder.WithMaxValues(1);

            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{user.Id}' and guild='{guild_id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            var rchore = db.runQuery($"select * from chores where discord_id='{user.Id}' and guild='{guild_id}';");

            int count = 1;

            while (rchore.Read())
            {

                menuBuilder.AddOption(rchore.GetString(1), rchore.GetString(1), "which chore would you like to Unassign?");

                count++;
            }
            
            if (count == 1)
            {
                await messageContext.ReplyAsync("You don't have any assigned chores", tts);
                return;
            }
            rchore.Close();
            //if (db.runCommand($"update users set points = points - 10 where points-10 > 0 and discord_id = '{user.Id}' and guild='{guild_id}';") == 0)
            //{
            //    await messageContext.ReplyAsync("You can't afford this! Do your chores!", tts);
            //    return;
            //}


            var builder = new ComponentBuilder()
             .WithSelectMenu(menuBuilder);

            //Do TTS
              Embed e = builder1.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                SelectMenuComponent smc = menuBuilder.Build();
                TTS.readEmbed(e, messageContext);
                TTS.readSelectMenu(smc, messageContext);
            }
                await messageContext.ReplyAsync(embed: e, components: builder.Build());

           // await messageContext.ReplyAsync(embed: builder1.Build(), components: builder.Build());


        }
        public async Task deleteChoreUnassigned(IUser user, IUserMessage messageContext, string choreName, ulong guild_id)
        {
            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{user.Id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            int numpoints = 0;
            var rchore = db.runQuery($"select points from users where guild='{guild_id}' and discord_id='{user.Id}';");
            while (rchore.Read())
            {
                numpoints = rchore.GetInt32(0);

            }
            rchore.Close();

            string gif = await GIPHY.getRandomGIF(choreName, 5);

            var UNick = (user as SocketGuildUser).Nickname;
            EmbedBuilder builder = new EmbedBuilder();
            if (numpoints >= 10)
            {

                db.runCommand($"select rem_chore('{choreName}', '{guild_id}');");
                db.runCommand($"UPDATE chores set discord_id=null where name = '{choreName}' and guild = '{guild_id}';");
                db.runCommand($"update users set points=points-10 where discord_id='{user.Id}' and guild='{guild_id}';");

                //db.runCommand("delete from chores where name = '" + choreName + "';");
                builder.WithTitle(UNick + " Has unassigned " + choreName + " for 10 points").WithColor(Color.Red);
                builder.WithImageUrl(gif);

            }
            else
            {
                builder.WithTitle(UNick + " You do not have enough points to unassign " + choreName+ " requires 10 points").WithColor(Color.Red);

            }
            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", false, e);
        }
        public async Task toggleTTSButton(IUser user, IUserMessage messageContext)
        {
            EmbedBuilder builder = new EmbedBuilder();
            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{user.Id}';") == "")
            {
                await messageContext.ReplyAsync("You do not yet have an account. Please use the create account button to create one!");
                return;
            }
            if (tts)
            {
                if (db.runCommand($"update users set tts=false where discord_id='{user.Id}';") < 1)
                {
                    throw new Exception($"failed to disable tts for {user.Username}");
                }
                builder.WithTitle($"{user.Username} disabled Text to Speech");
                //await messageContext.ReplyAsync($"{user.Username} disabled Text to Speech");
            }
            else
            {
                if (db.runCommand($"update users set tts=true where discord_id='{user.Id}';") < 1)
                {
                    throw new Exception($"failed to enable tts for {user.Username}");
                }
               // await messageContext.ReplyAsync($"{user.Username} enabled Text to Speech");
                builder.WithTitle($"{user.Username} enabled Text to Speech");

            }
            //Do TTS
            Embed e = builder.Build();
            if (lang != Translate.lang.English)
            {
                e = await Translate.TranslateEmbed(e, lang);
            }
            if (tts)
            {
                TTS.readEmbed(e, messageContext);
            }
            await messageContext.ReplyAsync("", false, e);

        }

    }
}