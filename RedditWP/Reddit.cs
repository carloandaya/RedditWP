using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace RedditWP
{
    public partial class Reddit
    {
        #region Constant Urls

        private const string SslLoginUrl = "https://ssl.reddit.com/api/login";        
        private const string LoginUrl = "/api/login/username";
        private const string UserInfoUrl = "/user/{0}/about.json";
        private const string MeUrl = "/api/me.json";
        private const string SubredditAboutUrl = "/r/{0}/about.json";
        private const string ComposeMessageUrl = "/api/compose";
        private const string RegisterAccountUrl = "/api/register";
        private const string GetThingUrl = "/by_id/{0}.json";
        private const string GetCommentUrl = "/r/{0}/comments/{1}/foo/{2}.json";

        #endregion

        #region Static Variables

        static Reddit()
        {
            UserAgent = "";
            EnableRateLimit = true;
            RootDomain = "www.reddit.com";
        }

        /// <summary>
        /// Additional values to append to the default RedditSharp user agent.
        /// </summary>
        public static string UserAgent { get; set; }
        /// <summary>
        /// It is strongly advised that you leave this enabled. Reddit bans excessive
        /// requests with extreme predjudice.
        /// </summary>
        public static bool EnableRateLimit { get; set; }
        /// <summary>
        /// The root domain RedditSharp uses to address Reddit.
        /// www.reddit.com by default
        /// </summary>
        public static string RootDomain { get; set; }

        private static DateTime lastRequest = DateTime.MinValue;

        #endregion

        /// <summary>
        /// The authenticated user for this instance.
        /// </summary>
        public AuthenticatedUser User { get; set; }

        internal JsonSerializerSettings JsonSerializerSettings { get; set; }

        private CookieContainer Cookies { get; set; }

        private string AuthCookie { get; set; }

        public Reddit()
        {
            JsonSerializerSettings = new JsonSerializerSettings();
            JsonSerializerSettings.CheckAdditionalContent = false;
            JsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
        }

        public async Task<AuthenticatedUser> LogIn(string username, string password, bool useSsl = true)
        {
            Cookies = new CookieContainer();
            HttpClient client;
            StringContent content;
            HttpResponseMessage response;
            if (useSsl)
            {
                content = StringForPost(new
                {
                    user = username,
                    passwd = password,
                    api_type = "json"
                });
            }
            else
            {
                content = StringForPost(new
                {
                    user = username,
                    passwd = password,
                    api_type = "json",
                    op = "login"
                });
            }

            if (useSsl)
            {
                client = CreateClient(false);
                response = await client.PostAsync(SslLoginUrl, content);
            }                
            else
            {
                client = CreateClient();
                response = await client.PostAsync(LoginUrl, content);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseContent)["json"];
            if (json["errors"].Count() != 0)
                throw new Exception("Incorrect login.");
            await GetMe(); // assigns the authenticated user to the User variable
            return User;            
        }

        public async Task<RedditUser> GetUser(string name)
        {
            HttpClient client = CreateClient();
            var result = await client.GetStringAsync(string.Format(UserInfoUrl, name));
            var json = JObject.Parse(result);
            return new RedditUser(this, json);
        }

        public async Task<AuthenticatedUser> GetMe()
        {
            HttpClient client = CreateClient();
            var response = await client.GetAsync(MeUrl);
            var responseContent = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseContent);
            User = new AuthenticatedUser(this, json);
            return User;
        }

        public async Task<Subreddit> GetSubreddit(string name)
        {
            if (name.StartsWith("r/"))
                name = name.Substring(2);
            if (name.StartsWith("/r/"))
                name = name.Substring(3);
            return (Subreddit) await GetThing(string.Format(SubredditAboutUrl, name));
        }

        public async Task ComposePrivateMessage(string subject, string body, string to)
        {
            if (User == null)
                throw new Exception("User can not be null.");
            HttpClient client = CreateClient();
            StringContent content = StringForPost(new
            {
                api_type = "json",
                subject,
                text = body,
                to,
                uh = User.Modhash
            });
            var response = await client.PostAsync(ComposeMessageUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();            
            // TODO: Error
        }

        /// <summary>
        /// Registers a new Reddit user
        /// </summary>
        /// <param name="userName">The username for the new account.</param>
        /// <param name="passwd">The password for the new account.</param>
        /// <param name="email">The optional recovery email for the new account.</param>
        /// <returns>The newly created user account</returns>
        public async Task<AuthenticatedUser> RegisterAccount(string userName, string passwd, string email = "")
        {
            HttpClient client = CreateClient();

            //var request = CreatePost(RegisterAccountUrl);
            StringContent content = StringForPost(new
            {
                api_type = "json",
                email = email,
                passwd = passwd,
                passwd2 = passwd,
                user = userName
            });
            var response = await client.PostAsync(RegisterAccountUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();            
            var json = JObject.Parse(responseContent);
            return new AuthenticatedUser(this, json);
            // TODO: Error
        }

        public async Task<Thing> GetThingByFullname(string fullname)
        {
            HttpClient client = CreateClient();
            var data = await client.GetStringAsync(GetThingUrl);            
            var json = JToken.Parse(data);
            return Thing.Parse(this, json["data"]["children"][0]);
        }

        public async Task<Comment> GetComment(string subreddit, string name, string linkName)
        {
            try
            {
                if (linkName.StartsWith("t3_"))
                    linkName = linkName.Substring(3);
                if (name.StartsWith("t1_"))
                    name = name.Substring(3);
                HttpClient client = CreateClient();
                var data = await client.GetStringAsync(string.Format(GetCommentUrl, subreddit, linkName, name));                
                var json = JToken.Parse(data);
                return Thing.Parse(this, json[1]["data"]["children"][0]) as Comment;
            }
            catch (WebException e)
            {
                return null;
            }
        }

        #region Helpers
        // Will probably need to do a lot of work here to convert to WP8 calls
        
        protected internal HttpWebRequest CreateRequest(string url, string method, bool prependDomain = true)
        {
            // creating this function for completion; will remove after conversion
            // to asynchronous model
            while (EnableRateLimit && (DateTime.Now - lastRequest).TotalSeconds < 2) ;
            lastRequest = DateTime.Now;
            HttpWebRequest request;
            if (prependDomain)
                request = (HttpWebRequest)WebRequest.Create(string.Format("http://{0}{1}", RootDomain, url));
            else
                request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = Cookies;
            request.Method = method;
            request.UserAgent = UserAgent;
            return request;
        }

        protected internal String CreateURL(string url, bool prependDomain = true)
        {
            throw new NotImplementedException();
        }

        protected internal HttpWebRequest CreateGet(string url, bool prependDomain = true)
        {
            return CreateRequest(url, "GET", prependDomain);
        }

        protected internal HttpWebRequest CreatePost(string url, bool prependDomain = true)
        {
            var request = CreateRequest(url, "POST", prependDomain);
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        protected internal string GetResponseString(Stream stream)
        {            
            string data = new StreamReader(stream).ReadToEnd();
            stream.Close();
            return data;
        }

        protected async internal Task<Thing> GetThing(string url, bool prependDomain = true)
        {
            HttpClient client = CreateClient(prependDomain);
            string data = await client.GetStringAsync(url);
            var json = JToken.Parse(data);
            return Thing.Parse(this, json);            
        }

        protected internal void WritePostBody(Stream stream, object data, params string[] additionalFields)
        {
            var type = data.GetType();
            var properties = type.GetProperties();
            string value = String.Empty;
            foreach (var property in properties)
            {
                var entry = Convert.ToString(property.GetValue(data, null));
                value += property.Name + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            for (int i = 0; i < additionalFields.Length; i += 2)
            {
                var entry = Convert.ToString(additionalFields[i + 1]);
                if (entry == null)
                    entry = String.Empty;
                value += additionalFields[i] + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            value = value.Remove(value.Length - 1); // Remove trailing &
            byte[] raw = Encoding.UTF8.GetBytes(value);
            stream.Write(raw, 0, raw.Length);
            stream.Close();
        }

        protected internal ByteArrayContent ByteArrayForPost(object data, params string[] additionalFields)
        {
            var type = data.GetType();
            var properties = type.GetProperties();
            string value = String.Empty;
            foreach (var property in properties)
            {
                var entry = Convert.ToString(property.GetValue(data, null));
                value += property.Name + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            for (int i = 0; i < additionalFields.Length; i += 2)
            {
                var entry = Convert.ToString(additionalFields[i + 1]);
                if (entry == null)
                    entry = String.Empty;
                value += additionalFields[i] + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            value = value.Remove(value.Length - 1); // Remove trailing &
            byte[] raw = Encoding.UTF8.GetBytes(value);
            return new ByteArrayContent(raw);
        }

        protected internal StringContent StringForPost(object data, params string[] additionalFields)
        {
            var type = data.GetType();
            var properties = type.GetProperties();
            string value = String.Empty;
            foreach (var property in properties)
            {
                var entry = Convert.ToString(property.GetValue(data, null));
                value += property.Name + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            for (int i = 0; i < additionalFields.Length; i += 2)
            {
                var entry = Convert.ToString(additionalFields[i + 1]);
                if (entry == null)
                    entry = String.Empty;
                value += additionalFields[i] + "=" + HttpUtility.UrlEncode(entry).Replace(";", "%3B").Replace("&", "%26") + "&";
            }
            value = value.Remove(value.Length - 1); // Remove trailing &
            return new StringContent(value, Encoding.UTF8, "application/x-www-form-urlencoded");
        }

        protected internal HttpClient CreateClient(bool prependDomain = true)
        {
            while (EnableRateLimit && (DateTime.Now - lastRequest).TotalSeconds < 2) ; // Rate limiting
            lastRequest = DateTime.Now;
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = Cookies;            
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("user-agent", UserAgent + " - with RedditWP" );
            if (prependDomain)
                client.BaseAddress = new Uri(string.Format("http://{0}", RootDomain));            
            return client;
        }

        protected internal static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        #endregion
    }
}
