using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace RedditWP
{
    public class Post : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string RemoveUrl = "/api/remove";
        private const string GetCommentsUrl = "/comments/{0}.json";
        private const string ApproveUrl = "/api/approve";
        private const string EditUserTextUrl = "/api/editusertext";

        private Comment returnComment;

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        public Post(Reddit reddit, JToken post)
            : base(reddit, post)
        {
            Reddit = reddit;
            JsonConvert.PopulateObject(post["data"].ToString(), this, reddit.JsonSerializerSettings);
        }

        [JsonProperty("author")]
        public string AuthorName { get; set; }

        /// <summary>
        /// The Author property returns a RedditUser object and is used 
        /// in some functions
        /// </summary>
        [JsonIgnore]
        public RedditUser Author
        {
            get
            {
                return Reddit.GetUser(AuthorName);
            }
        }
        [JsonProperty("approved_by")]
        public string ApprovedBy { get; set; }
        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }
        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }
        [JsonProperty("banned_by")]
        public string BannedBy { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
        [JsonProperty("edited")]
        public bool Edited { get; set; }
        [JsonProperty("is_self")]
        public bool IsSelfPost { get; set; }
        [JsonProperty("link_flair_css_class")]
        public string LinkFlairCssClass { get; set; }
        [JsonProperty("link_flair_text")]
        public string LinkFlairText { get; set; }
        [JsonProperty("num_comments")]
        public int CommentCount { get; set; }
        [JsonProperty("over_18")]
        public bool NSFW { get; set; }
        [JsonProperty("permalink")]
        public string Permalink { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("selftext")]
        public string SelfText { get; set; }
        [JsonProperty("selftext_html")]
        public string SelfTextHtml { get; set; }
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }
        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("num_reports")]
        public int? Reports { get; set; }

        public Comment Comment(string message)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            StateObject postState = new StateObject();
            var request = Reddit.CreatePost(CommentUrl);
            postState.Request = request;
            postState.ParameterValue = message;

            IAsyncResult commentRequestAR = request.BeginGetRequestStream(new AsyncCallback(CommentRequest), postState);
            IAsyncResult commentResponseAR = request.BeginGetResponse(new AsyncCallback(CommentResponse), postState);

            return returnComment;
        }

        private void CommentRequest(IAsyncResult ar)
        {
            StateObject postState = (StateObject)ar.AsyncState;
            HttpWebRequest request = postState.Request;
            Stream stream = request.EndGetRequestStream(ar);
            Reddit.WritePostBody(stream, new
            {
                text = (String)postState.ParameterValue,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
        }

        private void CommentResponse(IAsyncResult ar)
        {
            StateObject postState = (StateObject)ar.AsyncState;
            HttpWebRequest request = postState.Request;
            postState.Response = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(postState.Response.GetResponseStream());
            var json = JObject.Parse(data);
            var comment = json["jquery"].FirstOrDefault(i => i[0].Value<int>() == 18 && i[1].Value<int>() == 19);
            returnComment = new Comment(Reddit, comment[3][0][0]);
        }

        public void Approve()
        {
            var request = Reddit.CreatePost(ApproveUrl);
            request.BeginGetRequestStream(new AsyncCallback(ApproveRequest), request);
        }

        private void ApproveRequest(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            Stream stream = request.EndGetRequestStream(ar);
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            request.BeginGetResponse(new AsyncCallback(ApproveResponse), request);
        }

        private void ApproveResponse(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public void Remove()
        {
            var request = Reddit.CreatePost(RemoveUrl);
            request.BeginGetRequestStream(new AsyncCallback(RemoveRequest), request);
        }

        private void RemoveRequest(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            Stream stream = request.EndGetRequestStream(ar);
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                spam = true,
                uh = Reddit.User.Modhash
            });
            request.BeginGetResponse(new AsyncCallback(RemoveResponse), request);
        }

        private void RemoveResponse(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        public void RemoveSpam()
        {
            var request = Reddit.CreatePost(RemoveUrl);
            request.BeginGetRequestStream(new AsyncCallback(RemoveSpamRequest), request);
        }

        private void RemoveSpamRequest(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            Stream stream = request.EndGetRequestStream(ar);
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                spam = true,
                uh = Reddit.User.Modhash
            });
            request.BeginGetResponse(new AsyncCallback(RemoveSpamResponse), request);
        }

        private void RemoveSpamResponse(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(response.GetResponseStream());
        }

        // an attempt at using Task and System.Net.Http
        public async Task<Comment[]> GetComments()
        {
            var comments = new List<Comment>();
            // create a new HttpClient
            HttpClient client = new HttpClient();            
            string body = await client.GetStringAsync(string.Format(GetCommentsUrl, Id));
            var json = JArray.Parse(body);
            var postJson = json.Last()["data"]["children"];
            foreach (var comment in postJson)
            {
                comments.Add(new Comment(Reddit, comment));
            }
            return comments.ToArray();
        }

        public async Task EditText(string newText)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            if (!this.IsSelfPost)
                throw new Exception("Submission to edit is not a self-post.");
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://www.reddit.com");
            var content = Reddit.StringForPost(new {
                api_type = "json",
                text = newText,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(EditUserTextUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            JToken json = JToken.Parse(responseContent);
        }
    }
}
