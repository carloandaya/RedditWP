﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditWP
{
    public class Subreddit : Thing
    {
        private const string SubredditPostUrl = "/r/{0}.json";
        private const string SubredditNewUrl = "/r/{0}/new.json?sort=new";
        private const string SubscribeUrl = "/api/subscribe";
        private const string GetSettingsUrl = "/r/{0}/about/edit.json";
        private const string GetReducedSettingsUrl = "/r/{0}/about.json";
        private const string ModqueueUrl = "/r/{0}/about/modqueue.json";
        private const string UnmoderatedUrl = "/r/{0}/about/unmoderated.json";
        private const string FlairTemplateUrl = "/api/flairtemplate";
        private const string ClearFlairTemplatesUrl = "/api/clearflairtemplates";
        private const string SetUserFlairUrl = "/api/flair";
        private const string StylesheetUrl = "/r/{0}/about/stylesheet.json";
        private const string UploadImageUrl = "/api/upload_sr_img";
        private const string FlairSelectorUrl = "/api/flairselector";
        private const string AcceptModeratorInviteUrl = "/api/accept_moderator_invite";
        private const string LeaveModerationUrl = "/api/unfriend";
        private const string FrontPageUrl = "/.json";
        private const string SubmitLinkUrl = "/api/submit";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? Created { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("description_html")]
        public string DescriptionHTML { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        [JsonProperty("header_img")]
        public string HeaderImage { get; set; }
        [JsonProperty("header_title")]
        public string HeaderTitle { get; set; }
        [JsonProperty("over18")]
        public bool? NSFW { get; set; }
        [JsonProperty("public_description")]
        public string PublicDescription { get; set; }
        [JsonProperty("subscribers")]
        public int? Subscribers { get; set; }
        [JsonProperty("accounts_active")]
        public int? ActiveUsers { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonIgnore]
        public string Name { get; set; }

        /// <summary>
        /// This constructor only exists for internal use and serialization.
        /// You would be wise not to use it.
        /// </summary>
        public Subreddit()
            : base(null)
        {
        }

        protected internal Subreddit(Reddit reddit, JToken json)
            : base(json)
        {
            Reddit = reddit;
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
            Name = Url;
            if (Name.StartsWith("/r/"))
                Name = Name.Substring(3);
            if (Name.StartsWith("r/"))
                Name = Name.Substring(2);
            Name = Name.TrimEnd('/');
        }

        public static Subreddit GetRSlashAll(Reddit reddit)
        {
            var rSlashAll = new Subreddit
            {
                DisplayName = "/r/all",
                Title = "/r/all",
                Url = "/r/all",
                Name = "all",
                Reddit = reddit
            };
            return rSlashAll;
        }

        public static Subreddit GetFrontPage(Reddit reddit)
        {
            var frontPage = new Subreddit
            {
                DisplayName = "Front Page",
                Title = "reddit: the front page of the internet",
                Url = "/",
                Name = "/",
                Reddit = reddit
            };
            return frontPage;
        }

        public Listing<Post> GetPosts()
        {
            if (Name == "/")
                return new Listing<Post>(Reddit, "/.json");
            return new Listing<Post>(Reddit, string.Format(SubredditPostUrl, Name));
        }

        public Listing<Post> GetNew()
        {
            if (Name == "/")
                return new Listing<Post>(Reddit, "/new.json");
            return new Listing<Post>(Reddit, string.Format(SubredditNewUrl, Name));
        }

        public Listing<VotableThing> GetModQueue()
        {
            return new Listing<VotableThing>(Reddit, string.Format(ModqueueUrl, Name));
        }

        public Listing<Post> GetUnmoderatedLinks()
        {
            return new Listing<Post>(Reddit, string.Format(UnmoderatedUrl, Name));
        }

        public async Task Subscribe()
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                action = "sub", 
                sr = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(SubscribeUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            // Do something with the results or discard them
        }

        public async Task Unsubscribe()
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                action = "unsub",
                sr = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(SubscribeUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            // Do something with the results or discard them
        }

        public async Task<SubredditSettings> GetSettings()
        {
            bool getSettingsSucceeded;
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            try
            {
                HttpClient client = Reddit.CreateClient();
                var response = await client.GetAsync(string.Format(GetSettingsUrl, Name));
                var responseContent = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseContent);
                getSettingsSucceeded = true;
                return new SubredditSettings(this, Reddit, json);
            }
            catch // TODO: More specific catch
            {
                getSettingsSucceeded = false;
            }

            if (!getSettingsSucceeded)
            {
                // Do it unauthed
                HttpClient client = Reddit.CreateClient();
                var response = await client.GetAsync(string.Format(GetReducedSettingsUrl, Name));
                var responseContent = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseContent);
                return new SubredditSettings(this, Reddit, json);
            }
        }

        public async Task ClearFlairTemplates(FlairType flairType)
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                flair_type = flairType == FlairType.Link ? "LINK_FLAIR" : "USER_FLAIR",
                uh = Reddit.User.Modhash,
                r = Name
            });
            var response = await client.PostAsync(ClearFlairTemplatesUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
        }

        public async Task AddFlairTemplate(string cssClass, FlairType flairType, string text, bool userEditable)
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                css_class = cssClass,
                flair_type = flairType == FlairType.Link ? "LINK_FLAIR" : "USER_FLAIR",
                text = text, 
                text_editable = userEditable,
                uh = Reddit.User.Modhash,
                r = Name,
                api_type = "json"
            });
            var response = await client.PostAsync(FlairTemplateUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(responseContent);
        }

        public async Task SetUserFlair(string user, string cssClass, string text)
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                css_class = cssClass,
                text = text, 
                uh = Reddit.User.Modhash,
                r = Name, 
                name = user
            });
            var response = await client.PostAsync(SetUserFlairUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
        }

        public async Task<UserFlairTemplate[]> GetUserFlairTemplates()
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                name = Reddit.User.Name, 
                r = Name, 
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(FlairSelectorUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(responseContent);
            if (document.DocumentNode.Descendants("div").First().Attributes["error"] != null)
                throw new InvalidOperationException("This subreddit does not allow users to select flair.");
            var templateNodes = document.DocumentNode.Descendants("li");
            var list = new List<UserFlairTemplate>();
            foreach (var node in templateNodes)
            {
                list.Add(new UserFlairTemplate
                {
                    CssClass = node.Descendants("span").First().Attributes["class"].Value.Split(' ')[1],
                    Text = node.Descendants("span").First().InnerText
                });
            }
            return list.ToArray();
        }

        public async Task UploadHeaderImage(string name, ImageType imageType, byte[] file)
        {
            HttpClient client = Reddit.CreateClient();

        }
    }
}
