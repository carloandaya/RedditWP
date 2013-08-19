using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RedditWP
{
    public class SubredditStyle
    {
        private const string UploadImageUrl = "/api/upload_sr_img";
        private const string UpdateCssUrl = "/api/subreddit_stylesheet";

        private Reddit Reddit { get; set; }

        public SubredditStyle(Reddit reddit, Subreddit subreddit)
        {
            Reddit = reddit;
            Subreddit = subreddit;
        }

        public SubredditStyle(Reddit reddit, Subreddit subreddit, JToken json)
            : this(reddit, subreddit)
        {
            throw new NotImplementedException();
        }

        public string CSS { get; set; }
        public List<SubredditImage> Images { get; set; }
        public Subreddit Subreddit { get; set; }

        public async Task UpdateCss()
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                op = "save",
                stylesheet_content = CSS,
                uh = Reddit.User.Modhash,
                api_type = "json",
                r = Subreddit.Name
            });
            var response = await client.PostAsync(UpdateCssUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(responseContent);
        }

        public void UploadImage(string name, ImageType imageType, byte[] file)
        {
            throw new NotImplementedException();
        }
    }

    public enum ImageType
    {
        PNG, 
        JPEG
    }
}
