using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Globalization;
using System;
using Discord;

namespace ChoreBot
{
    public static class Translate
    {
        static HttpClient client = new HttpClient();

        /// <summary>
        /// The available languages in the LibreTranslate API
        /// See: <see href="https://github.com/LibreTranslate/LibreTranslate"> LibreTranslate </see>
        /// </summary>
        public enum lang
        {
            English,
            Arabic,
            Chinese,
            French,
            German,
            Hindi,
            Indonesian,
            Irish,
            Italien,
            Japanese,
            Korean,
            Polish,
            Portugese,
            Russian,
            Spanish,
            Turkish,
            Vietnemese
        }

        static Dictionary<lang, string> getLangCode = new Dictionary<lang, string>()
        {
            { lang.Arabic, "ar" },
            { lang.Chinese, "zh" },
            { lang.English, "en" },
            { lang.French, "fr" },
            { lang.German, "de" },
            { lang.Hindi, "hi" },
            { lang.Indonesian, "id" },
            { lang.Irish, "ga" },
            { lang.Italien, "it" },
            { lang.Japanese, "ja" },
            { lang.Korean, "ko" },
            { lang.Polish, "pl" },
            { lang.Portugese, "pt" },
            { lang.Russian, "ru" },
            { lang.Spanish, "es" },
            { lang.Turkish, "tr" },
            { lang.Vietnemese, "vi" },
        };

        /// <summary>
        /// Translates the source string into the selected language
        /// </summary>
        /// <param name="source">The source string</param>
        /// <param name="l">The language as taken from the <see cref="lang">lang</see> enum</param>
        /// <returns>The translated text</returns>
        public static async Task<string> TranslateText(string source, lang l)
        {
            var response = await client.PostAsync($"https://translate.mentality.rip/translate?q={source}&source=en&target={getLangCode[l]}&format=text", new ByteArrayContent(new byte[0]));
            string result = await response.Content.ReadAsStringAsync();
            return fixUnicode(result.Substring(19, result.Length - 22));
        }
        
        static string fixUnicode(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }

        public static lang getLang(IUser user)
        {
            DBConnection db = new DBConnection();
            var rlang = db.runQuery($"select lang from users where discord_id='{user.Id}';");
            if (rlang.Read())
            {
                return (lang)rlang.GetInt32(0);
            } else
            {
                return lang.English;
            }
        }

        public static async Task<Embed> TranslateEmbed(Embed e, lang l)
        {
            try
            {
                string rawembed = TTS.unpackEmbed(e);
                var trans = (await TranslateText(rawembed, l)).Split(@"\n", StringSplitOptions.RemoveEmptyEntries);
                var builder = e.ToEmbedBuilder();
                int i = 0;
                builder.Title = trans[i]; i++;
                foreach (var f in builder.Fields)
                {
                    f.Name = trans[i]; i++;
                    f.Value = trans[i]; i++;
                }
                //for (int i = 1; i < trans.Length; i += 2)
                //{
                //    builder.Fields[i / 2 + 1].Name = trans[i];
                //    builder.Fields[i / 2 + 1].Value = trans[i + 1];
                //}

                return builder.Build();
            }
            catch
            {
                Console.WriteLine("Could Not tranlsate text!!!");
                return e;
            }
        }
    }
}
