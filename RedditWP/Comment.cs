using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditWP
{
    public class Comment : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string DistinguishUrl = "/api/distinguish";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string RemoveUrl = "/api/remove";

        private class CommentState
        {
            public HttpWebRequest AsyncRequest { get; set; }
            public HttpWebResponse AsyncResponse { get; set; }
            public String Message { get; set; }
        }

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        public Comment(Reddit reddit, JToken json)
            : base(reddit, json)
        {
            var data = json["data"];
            JsonConvert.PopulateObject(data.ToString(), this, reddit.JsonSerializerSettings);
            Reddit = reddit;

            // Parse sub comments
            // TODO: Consider deserializing this properly
            var subComments = new List<Comment>();
            if (data["replies"] != null && data["replies"].Any())
            {
                foreach (var comment in data["replies"]["data"]["children"])
                    subComments.Add(new Comment(reddit, comment));
            }
            Comments = subComments.ToArray();
        }

        [JsonProperty("author")]
        public string Author { get; set; }
        
        [JsonProperty("banned_by")]
        public string BannedBy { get; set; }
        
        [JsonProperty("body")]
        public string Body { get; set; }
        
        [JsonProperty("body_html")]
        public string BodyHtml { get; set; }

        [JsonProperty("parent_id")]
        public string ParentId { get; set; }
        
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        [JsonProperty("approved_by")]
        public string ApprovedBy { get; set; }

        [JsonProperty("author_flair_css_class")]
        public string AuthorFlairCssClass { get; set; }

        [JsonProperty("author_flair_text")]
        public string AuthorFlairText { get; set; }

        [JsonProperty("gilded")]
        public int Gilded { get; set; }

        [JsonProperty("link_id")]
        public string LinkId { get; set; }

        [JsonProperty("link_title")]
        public string LinkTitle { get; set; }

        [JsonProperty("num_reports")]
        public int? NumReports { get; set; }

        [JsonProperty("distinguished")]
        [JsonConverter(typeof(DistinguishConverter))]
        public DistinguishType Distinguished { get; set; }

        [JsonIgnore]
        public Comment[] Comments { get; set; }

        public async Task<Comment> Reply(string message)
        {
            if (Reddit.User == null)
                // RedditSharp used an AuthenticationException 
                // but it's not available for Windows Phone
                throw new Exception("No user logged in.");
            var request = Reddit.CreatePost(CommentUrl);
            CommentState commentState = new CommentState();
            commentState.AsyncRequest = request;
            commentState.Message = message;
            Comment replyComment = await ReplyTask(commentState);
            return replyComment;
        }

        private void ReplyRequest(IAsyncResult ar)
        {

        }

        private Task<Comment> ReplyTask(CommentState state)
        {

        }
    }
}
