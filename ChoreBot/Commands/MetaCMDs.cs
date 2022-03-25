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
        public static string ping()
        {
            return "pong :ping_pong:";
        }

        //TODO: Add parameter checking
        public static Embed help()
        {
            EmbedBuilder builder = new EmbedBuilder();
            //Implement?
            //builder.WithTitle("Commands:").WithColor(Color.Purple)
            //    .AddField("**ping**:", " Play a quick game of ping-pong with ChoreBot")
            //    .AddField("**about**:", "  Gives a brief overview of ChoreBot")
            //    .AddField("**createChore**:", "  Creates a chore that is available to all users")
            //    .AddField("**deleteChore**:", "  Removes a particualar chore from the global chore list")
            //    .AddField("**listChores**:", "  Lists all global chores")
            //    .AddField("**CreateAccount**:", "  Create a personal ID to track chores")
            //    .AddField("**addChore**:", "  Creates a chore available to user only")
            //    .AddField("**choreComplete**:", "  User is congratulated, and that personal chore is removed")
            //    .AddField("**myChores**:", "  Display the user's points and current chores")

            //    .AddField("**balance**:", "  (can be removed/ not needed) Display the user's points")
            //.AddField("**debugAccount**:", "  Reset points to 0 (set points to 0 for users with null points) and give user guildID");

            builder.WithTitle("Help:").WithColor(Color.Purple)
                //.AddField("**ChoreBot Summary:**", "ChoreBot helps people coordinate their chores and tasks.\n" +
                //    "Type **/Menu** into the message bar to open the menu, and then click the **Create Account** button to verify that you are ready to use the bot. " +
                //    "Use the buttons in the menu to add chores, assign chores, complete, and review completed chores. " +
                //    "When you complete a chore, you are rewarded points that can be used to purchase perks with the chore list. " +
                //    "Check the Scoreboard to see who is the ChoreChampion of the server!\n" +
                //    "If you need help, either type **/Help** into the message bar, or click the **Help** button in the menu.")
                .AddField("**Slash Commands:**", "These are commands you type into the message bar to interact with ChoreBot.")
                .AddField("**/menu**", "Displays the menu dialog with the buttons used to interact with ChoreBot")
                .AddField("**/help**", "Displays this help message")
                .AddField("**/about**", "Displays a summary of the ChoreBot's Functionality")
                .AddField("**/newchore**","Allows the user to create and assign a new chore! The \"name\" parameter takes the name of the chore. " +
                    "The \"description\" parameter allows you the options of defining a description for this chore. " +
                    "The \"difficulty\" parameter sets the difficulty of the chore (easy/medium/hard). " +
                    "The \"assigned\" parameter allows you to spend 25 points to assign this chore to another. " +
                    "All of these parameters autofill.")
                .AddField("\x00b6","----------------------------------------------------------")

                .AddField("**Menu Buttons**", "Click these buttons in the menu to use their functionality")
                .AddField("**My Chores**", "Displays the list of chores that are assigned to your from this server")
                .AddField("**List All Chores**", "Displays the list of all chores in the server, each with their own description, difficulty, and who it is assigned to")
                .AddField("**Balance**", "Displays how many points you have currently")
                .AddField("**Take Chore**", "Use the dropdown menu to choose a chore from the list of all chores to assign to yourself and add to your personal list")
                .AddField("**Complete Chore**", "Use the message bar to type out the information about the new chore, including the name of task, task description, difficulty level, and who the chore is assigned to, separating each part with a “|”.\n" +
                    "The difficulty level is a number that describes how much work the task requires. The valid values are 1(easy), 2(medium), and 3(hard).\n" +
                    "Example: 'Wash Dishes | Load and Start the Dishwasher | 2'\n")
                .AddField("**Review Chore**", "Use the dropdown menu to choose a chore to finalize as complete and award points")
                .AddField("**Unassign Chore**", "Use the dropdown menu to unassign a chore from yourself to take it off of your list by spending 10 points")
                .AddField("**Scoreboard**", "Displays the current point balances of each account in the server in order of who as the most points")
                .AddField("**Language**", "Use the dropdown menu to choose a language for the bot to output when you use it")
                .AddField("**TTS**", "Turns on or turns off automatic Text-To-Speech reading of the bot's replies")
                .AddField("**Help**", "Displays this help message")
                .AddField("**Create Account**", "Creates an account for a new user of the ChoreBot associated with this server");



            return builder.Build();

        }

        public static string about()
        {
            return "ChoreBot helps people coordinate their chores and tasks.\n" +
                    "Type **/Menu** into the message bar to open the menu, and then click the **Create Account** button to verify that you are ready to use the bot. " +
                    "Use the buttons in the menu to add chores, assign chores, complete, and review completed chores. " +
                    "When you complete a chore, you are rewarded points that can be used to purchase perks with the chore list. " +
                    "Check the Scoreboard to see who is the ChoreChampion of the server!\n" +
                    "If you need help, either type **/Help** into the message bar, or click the **Help** button in the menu.";

        }

        //DEPRECATED
        public static async Task<string> gif(string text)
        {
            string gif = await GIPHY.getRandomGIF(text, 5);
            if (gif == "") { gif = "No GIF Found :sob:"; }
            return gif;
        }
    }
}