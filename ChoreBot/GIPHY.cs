using System;
using ByteDev.Giphy;
using ByteDev.Giphy.Request;
using ByteDev.Giphy.Response;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;


namespace ChoreBot
{
    public static class GIPHY
    {
        public const string GIPHYAPIKEY = "GIPHY Key Here";

        /// <summary>
        /// Aquires a random GIF based on the search term.
        /// </summary>
        /// <param name="searchTerm">The term that will be passed to Giphy's serveres</param>
        /// <param name="range">The limit placed on the search term, which the resultant image will be randomly selected form. Smaller numbers are generally more specific.</param>
        /// <returns>A ByteDev <see cref="SearchResponse"> SearchResponse </see> containing the output GIF.</returns>
        public static async Task<string> getRandomGIF(string searchTerm, int range)
        {
            GiphyApiClient client = new GiphyApiClient(new HttpClient());
            SearchRequest request = new SearchRequest(GIPHYAPIKEY) { Query = searchTerm, Limit = range };
            SearchResponse response = await client.SearchAsync(request);
            Random r = new Random(DateTime.Now.Millisecond);
            int index = r.Next(response.Gifs.Count());
            string url = "";
            if (response.Gifs.Count() > 0)
            {
                url = response.Gifs.ToList()[index].Images.Original.Url.ToString();
            }
#if DEBUG
            Console.WriteLine("-----------GIF API CALL----------");
            Console.WriteLine($"Search Term was {searchTerm}, and range was {range}");
            Console.WriteLine($"index was {index} out of {response.Gifs.Count()}");
            Console.WriteLine($"GIF URL was: {url}");
#endif
            return url;

        }
    }
}
