using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;

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
            throw new NotImplementedException();
            //bool getSettingsSucceeded;            
            //if (Reddit.User == null)
            //    throw new Exception("No user logged in.");
            //try
            //{
            //    HttpClient client = Reddit.CreateClient();
            //    var response = await client.GetAsync(string.Format(GetSettingsUrl, Name));
            //    var responseContent = await response.Content.ReadAsStringAsync();
            //    var json = JObject.Parse(responseContent);
            //    getSettingsSucceeded = true;
            //    return new SubredditSettings(this, Reddit, json);
            //}
            //catch // TODO: More specific catch
            //{
            //    getSettingsSucceeded = false;
            //}

            //if (!getSettingsSucceeded)
            //{
            //    // Do it unauthed
            //    HttpClient client = Reddit.CreateClient();
            //    var response = await client.GetAsync(string.Format(GetReducedSettingsUrl, Name));
            //    var responseContent = await response.Content.ReadAsStringAsync();
            //    var json = JObject.Parse(responseContent);
            //    return new SubredditSettings(this, Reddit, json);
            //}
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
            throw new NotImplementedException();
        }

        public void UploadHeaderImage(string name, ImageType imageType, byte[] file)
        {
            // Need to understand what is going on with MultiPartForm
            // TODO: Finish this method
            throw new NotImplementedException();
        }

        public async Task<SubredditStyle> GetStylesheet()
        {
            HttpClient client = Reddit.CreateClient();
            var response = await client.GetAsync(string.Format(StylesheetUrl, Name));
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(responseContent);
            return new SubredditStyle(Reddit, this, json);
        }

        public async Task AcceptModeratorInvite()
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                api_type = "json", 
                uh = Reddit.User.Modhash,
                r = Name
            });
            var response = await client.PostAsync(AcceptModeratorInviteUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
        }

        public async Task RemoveModerator(string id)
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                api_type = "json", 
                uh = Reddit.User.Modhash,
                r = Name, 
                type = "moderator", 
                id
            });
            var response = await client.PostAsync(LeaveModerationUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
        }

        public override string ToString()
        {
            return "/r/" + DisplayName;
        }

        /// <summary>
        /// Submits a text post in the current subreddit using the logged-in user
        /// </summary>
        /// <param name="title">The title of the submission</param>
        /// <param name="text">The raw markdown of the submission</param>
        /// <returns></returns>
        public async Task<Post> SubmitTextPost(string title, string text)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                api_type = "json", 
                kind = "self", 
                sr = Title, 
                text = text, 
                title = title, 
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(SubmitLinkUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var json = JToken.Parse(responseString);
            return new Post(Reddit, json["json"]);
            // TODO: Error
        }

        /// <summary>
        /// Submits a link post in the current subreddit using the logged-in user
        /// </summary>
        /// <param name="title">The title of the submission</param>
        /// <param name="url">The url of the submission link</param>
        public async Task<Post> SubmitPost(string title, string url)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
                {
                    api_type = "json",
                    extension = "json",
                    kind = "link",
                    sr = Title,
                    title = title,
                    uh = Reddit.User.Modhash,
                    url = url
                });
            var response = await client.PostAsync(SubmitLinkUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();            
            var json = JToken.Parse(responseContent);
            return new Post(Reddit, json["json"]);
            // TODO: Error
        }
    }
}
