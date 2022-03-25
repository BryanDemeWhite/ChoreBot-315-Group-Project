using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ChoreBot
{
    //The partial class contianing all our commands
    public static partial class CMDs
    {
        //DEPRECATED
        public static Embed listchores(IUser U = null)
        {

            EmbedBuilder builder = new EmbedBuilder();
            Npgsql.NpgsqlDataReader rchore;
            DBConnection db = new DBConnection();
            if (U == null)
            {
                rchore = db.runQuery("select * from chores where finished=false limit 24;");
            }
            else
            {
                rchore = db.runQuery($"select * from chores inner join (select distinct unnest(chores) as cid from users where discord_id='{U.Id}') as t on t.cid = chores.id where finished = false limit 24;");
            }
             
            int[] vChoreIndex = { };
            string[] vChoreName = { };

            int count = 0;
            builder.WithTitle("Chores:").WithColor(Color.Purple);
            while (rchore.Read() && count < 24)
            {
                string n = rchore.GetString(1);
                int id = rchore.GetInt32(0);
                string description = "----------------------------------------";
                try { description = rchore.GetString(2); }
                catch { description = "----------------------------------------"; }
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
            return builder.Build();
        }

        public static async Task<Embed> newchore(string choreName, IUser caller, ulong guild_id, string desc = "--------------------", IUser assigned = null, long diff = 0)
        {
            //Create Chore
            DBConnection db = new DBConnection();
            if (db.runScalar($"select discord_id from users where discord_id='{caller.Id}';") == "")
            {
                EmbedBuilder b = new EmbedBuilder();
                b.WithTitle("You must make an account before you can add a chore!").WithColor(Color.Red);
                return b.Build();
            }
            if (assigned != null )
            {
                if (db.runCommand($"update users set points = points - 25 where points-25 > 0 and discord_id = '{caller.Id}' and guild='{guild_id}';") == 0)
                {
                    EmbedBuilder b = new EmbedBuilder();
                    b.WithTitle("You don't have enough points to assign a chore to someone else!").WithColor(Color.Red);
                    return b.Build();
                }
            }
            db.runCommand($"insert into chores(name, description, difficulty, guild) Values ('{choreName}', '{desc}', {diff}, '{guild_id}');");

            string gif = await GIPHY.getRandomGIF(choreName, 5);

            var UNick = caller.Username;
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle(UNick + " created the chore: " + choreName).WithColor(Color.Blue);
            builder.WithImageUrl(gif);

            if (assigned != null)
            {
                builder.AddField("Assigned to", assigned.Username);
                var rchore = db.runQuery("select id from chores where name='" + choreName + "';");

                rchore.Read();
                var choreID = rchore.GetValue(0);

                rchore.Close();

                db.runCommand("UPDATE users SET chores = array_append(chores, " + choreID + ") where discord_id = " + assigned.Id + "::text;");
                db.runCommand($"UPDATE chores SET discord_id = '{assigned.Id}' where id = {choreID};");

            }
            return builder.Build();


        }
    }
}