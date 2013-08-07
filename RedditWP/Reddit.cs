using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        private const string RegisterAccountIrl = "/api/register";

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

        public AuthenticatedUser LogIn(string username, string password, bool useSsl = true)
        {
            throw new NotImplementedException();
        }

        public RedditUser GetUser(string name)
        {
            throw new NotImplementedException();
        }

        public AuthenticatedUser GetMe()
        {
            throw new NotImplementedException();
        }

        public Subreddit GetSubreddit(string name)
        {
            throw new NotImplementedException();
        }

        public void ComposePrivateMessage(string subject, string body, string to)
        {
            throw new NotImplementedException();
        }

        public AuthenticatedUser RegisterAccount(string userName, string passwd, string email = "")
        {
            throw new NotImplementedException();
        }

        #region Helpers
        // Will probably need to do a lot of work here to convert to WP8 calls


        private static DateTime lastRequest = DateTime.MinValue;
        protected internal HttpWebRequest CreateRequest(string url, string method, bool prependDomain = true)
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

        protected internal string GetReponseString(Stream stream)
        {
            throw new NotImplementedException();
        }

        protected internal Thing GetThing(string url, bool prependDomain = true)
        {
            throw new NotImplementedException();
        }

        protected internal void WritePostBody(Stream stream, object data, params string[] additionalFields)
        {
            throw new NotImplementedException();
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
