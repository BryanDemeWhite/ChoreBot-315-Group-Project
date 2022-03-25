using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using ByteDev.Giphy;
using System.Net.Http;
using ByteDev.Giphy.Request;
using Discord.WebSocket;

namespace ChoreBot
{
    public static partial class slashCMDs
    {
        public static ApplicationCommandProperties generateCommand(string name, CommandBody cb)
        {
            var command = new SlashCommandBuilder();
            command.WithName(name);
            command.WithDescription(cb.description);
            foreach (var p in cb.parameters.Where(p => p.required))
            {
                command.AddOption(p.name, p.type, p.description, required: p.required);
            }
            foreach (var o in cb.options.Where(o => o.required))
            {
                var op = new SlashCommandOptionBuilder();
                op.WithName(o.name);
                op.WithDescription(o.description);
                op.WithRequired(o.required);
                int i = 0;
                foreach (var c in o.choices)
                {
                    op.AddChoice(c, i);
                    i++;
                }
                op.WithType(ApplicationCommandOptionType.Integer);
                command.AddOption(op);
            }
            foreach (var p in cb.parameters.Where(p => !p.required))
            {
                command.AddOption(p.name, p.type, p.description, required: p.required);
            }
            foreach (var o in cb.options.Where(o => !o.required))
            {
                var op = new SlashCommandOptionBuilder();
                op.WithName(o.name);
                op.WithDescription(o.description);
                op.WithRequired(o.required);
                int i = 0;
                foreach (var c in o.choices)
                {
                    op.AddChoice(c, i);
                    i++;
                }
                op.WithType(ApplicationCommandOptionType.Integer);
                command.AddOption(op);
            }
            return command.Build();
        }

        public struct CommandBody
        {
            public string description;
            public List<parameter> parameters;
            public List<option> options;
            
            public struct parameter
            {
                public string name;
                public string description;
                public ApplicationCommandOptionType type;
                public bool required;
            }

            public struct option
            {
                public string name;
                public string description;
                public bool required;
                public List<string> choices;
            }
        }

        public static Dictionary<string, CommandBody> Commands = new Dictionary<string, CommandBody>
        {
            //Add new commands here
            //Format:
            /*
             {
                "commandname",
                new CommandBody {
                    description = "desc",
                    parameters = new() {
                        new CommandBody.parameter {
                            name = "paramname",
                            description = "paradesc",
                            type = ApplicationCommandOptionType.<type>,
                            required = false
                        }
                    },
                    options = new() {
                        new CommandBody.option
                        {
                            name = "optionname",
                            required = false,
                            choices = new() 
                            {
                                "choice1", "choice2", "etc"
                            }
                        }
                    }
                }
             }
            */
            {
                "ping",
                new CommandBody {
                    description = "Checks if the bot is functioning",
                    parameters = new() {},
                    options = new() {},
                }
            },
            {
                "help",
                new CommandBody
                {
                    description = "List out information about contained commands",
                    parameters = new() {},
                    options = new() { }
                }
            },
            {
                "about",
                new CommandBody {
                    description = "Lists off information about choreBot",
                    parameters = new() {},
                    options = new() {},
                }
            },
            //{
            //    "gif",
            //    new CommandBody {
            //        description = "DEBUG: generates a gif based on a search term",
            //        options = new() {},
            //        parameters = new()
            //        {
            //            new CommandBody.parameter
            //            {
            //                name = "term",
            //                description = "The gif's search term",
            //                required = true,
            //                type = ApplicationCommandOptionType.String,
            //            }
            //        }
            //    }
            //},
            //{
            //    "listchores",
            //    new CommandBody
            //    {
            //        description = "Lists out a set of chores",
            //        parameters = new()
            //        {
            //            new CommandBody.parameter
            //            {
            //                name = "user",
            //                description = "Whose chores you want listed",
            //                required = false,
            //                type = ApplicationCommandOptionType.User
            //            }
            //        },
            //        options = new() {},
            //    }
            //},
            //{
            //    "balance",
            //    new CommandBody
            //    {
            //        description = "Gets a current point balance",
            //        parameters = new()
            //        {
            //            new CommandBody.parameter
            //            {
            //                name = "user",
            //                description = "The user whose point balance you're viewing",
            //                required = true,
            //                type = ApplicationCommandOptionType.User
            //            }
            //        },
            //        options = new() {},
            //    }
            //},


            //{
            //    "debugaccount",
            //    new CommandBody
            //    {
            //        description = "desc",
            //         parameters = new()
            //        {
            //            new CommandBody.parameter
            //            {
            //                name = "user",
            //                description = "desc",
            //                required = false,
            //                type = ApplicationCommandOptionType.User
            //            }
            //        },
            //        options = new() {},
            //    }
            //},
              {
                "menu",
                new CommandBody
                {
                    description = "Opens your menu to access commands via buttons",
                     parameters = new()
                    {

                    },
                    options = new() {},
                }
            },

            {
                "newchore",
                new CommandBody
                {
                    description = "creates a new chore",
                    parameters = new()
                    {
                        new CommandBody.parameter
                        {
                            name = "name",
                            description = "The name of the chore",
                            required = true,
                            type = ApplicationCommandOptionType.String
                        },
                        new CommandBody.parameter
                        {
                            name = "description",
                            description = "A description of the chore",
                            required = false,
                            type = ApplicationCommandOptionType.String
                        },
                        new CommandBody.parameter
                        {
                            name = "assigned",
                            description = "The User the chore is assigned to (leave blank to make chore public)",
                            required = false,
                            type = ApplicationCommandOptionType.User
                        },
                    },
                    options = new()
                    {
                        new CommandBody.option
                        {
                            name = "difficulty",
                            description = "How hard is this chore?",
                            required = true,
                            choices = new()
                            {
                                "easy", "medium", "hard"
                            }
                        }
                    }
                }
            },

            //{
            //    "choredone",
            //    new CommandBody
            //    {
            //        description = "Marks a chore as finished",
            //        parameters = new()
            //        {
            //            new CommandBody.parameter
            //            {
            //                name = "chore",
            //                description = "The chore you're marking complete",
            //                required = true,
            //                type = ApplicationCommandOptionType.String
            //            }
            //        },
            //        options = new() {},
            //    }
            //},
            //{
            //    "cancelchore",
            //    new CommandBody
            //    {
            //        description = "Cancels and deletes a chore from the list (different from completion)",
            //        parameters = new()
            //        {
            //            new CommandBody.parameter
            //            {
            //                name = "chore",
            //                description = "The chore you're deleting",
            //                required = true,
            //                type = ApplicationCommandOptionType.String
            //            }
            //        },
            //        options = new() {},
            //    }
            //},
            //{
            //    "toggletts",
            //    new CommandBody
            //    {
            //        description = "Enables or disables Text to Speech option fro bot output.",
            //        parameters = new() { },
            //        options = new() { },
            //    }
            //}
        };

    }
}