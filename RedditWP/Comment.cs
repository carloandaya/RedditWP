using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;


namespace RedditWP
{
    public class Comment : VotableThing
    {
        private const string CommentUrl = "/api/comment";
        private const string DistinguishUrl = "/api/distinguish";
        private const string EditUserTextUrl = "/api/editusertext";
        private const string RemoveUrl = "/api/remove";

        private Comment returnComment;

        private class CommentState
        {
            public HttpWebRequest AsyncRequest { get; set; }
            public HttpWebResponse AsyncResponse { get; set; }
            public Object ParameterValue { get; set; }            
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

        public Comment Reply(string message)
        {
            if (Reddit.User == null)
                // RedditSharp used an AuthenticationException 
                // but it's not available for Windows Phone
                throw new Exception("No user logged in.");

            CommentState commentState = new CommentState();
            var request = Reddit.CreatePost(CommentUrl);            
            commentState.AsyncRequest = request;
            commentState.ParameterValue = message;            

            IAsyncResult replyRequestAR = request.BeginGetRequestStream(new AsyncCallback(ReplyRequest), commentState);
            IAsyncResult replyResponseAR = request.BeginGetResponse(new AsyncCallback(ReplyResponse), commentState);

            return returnComment;            
        }

        private void ReplyRequest(IAsyncResult ar)
        {
            CommentState commentState = (CommentState)ar.AsyncState;
            HttpWebRequest request = commentState.AsyncRequest;
            Stream stream = request.EndGetRequestStream(ar);
            Reddit.WritePostBody(stream, new
            {
                text = (String)commentState.ParameterValue, 
                thing_id = FullName,
                uh = Reddit.User.Modhash,
                api_type = "json"
                //r = Subreddit
            });
        }

        private void ReplyResponse(IAsyncResult ar)
        {
            CommentState commentState = (CommentState)ar.AsyncState;
            HttpWebRequest request = commentState.AsyncRequest;
            commentState.AsyncResponse = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(commentState.AsyncResponse.GetResponseStream());
            var json = JObject.Parse(data);
            returnComment = new Comment(Reddit, json["json"]["data"]["things"][0]);
        }

        public void Distinguish(DistinguishType distinguishType)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");
            var request = Reddit.CreatePost(DistinguishUrl);
            CommentState commentState = new CommentState();
            commentState.AsyncRequest = request;
            commentState.ParameterValue = distinguishType;            
            request.BeginGetRequestStream(new AsyncCallback(DistinguishRequest), commentState);
        }

        private void DistinguishRequest(IAsyncResult ar)
        {
            CommentState commentState = (CommentState)ar.AsyncState;
            DistinguishType distinguishType = (DistinguishType)commentState.ParameterValue;
            HttpWebRequest request = commentState.AsyncRequest;
            Stream stream = request.EndGetRequestStream(ar);

            string how;
            switch (distinguishType)
            {
                case DistinguishType.Admin:
                    how = "admin";
                    break;
                case DistinguishType.Moderator:
                    how = "yes";
                    break;
                case DistinguishType.None:
                    how = "no";
                    break;
                default:
                    how = "special";
                    break;
            }

            Reddit.WritePostBody(stream, new
            {
                how,
                id = Id, 
                uh = Reddit.User.Modhash
            });

            request.BeginGetResponse(new AsyncCallback(DistinguishResponse), commentState);
        }

        private void DistinguishResponse(IAsyncResult ar)
        {
            CommentState commentState = (CommentState)ar.AsyncState;
            HttpWebRequest request = commentState.AsyncRequest;
            commentState.AsyncResponse = (HttpWebResponse)request.EndGetResponse(ar);

            var data = Reddit.GetResponseString(commentState.AsyncResponse.GetResponseStream());
            var json = JObject.Parse(data);
            if (json["jquery"].Count(i => i[0].Value<int>() == 11 && i[1].Value<int>() == 12) == 0)
                throw new Exception("You are not permitted to distinguish this comment.");
        }

        public void EditText(string newText)
        {
            if (Reddit.User == null)
                throw new Exception("No user logged in.");

            var request = Reddit.CreatePost(EditUserTextUrl);
            CommentState commentState = new CommentState();
            commentState.AsyncRequest = request;
            commentState.ParameterValue = newText;

            request.BeginGetRequestStream(new AsyncCallback(EditTextRequest), commentState);
        }

        private void EditTextRequest(IAsyncResult ar)
        {
            CommentState commentState = (CommentState)ar.AsyncState;
            HttpWebRequest request = commentState.AsyncRequest;
            Stream stream = request.EndGetRequestStream(ar);

            Reddit.WritePostBody(stream, new
            {
                api_type = "json",
                text = (String)commentState.ParameterValue,
                thing_id = FullName,
                uh = Reddit.User.Modhash
            });

            request.BeginGetResponse(new AsyncCallback(EditTextResponse), commentState);
        }

        private void EditTextResponse(IAsyncResult ar)
        {
            CommentState commentState = (CommentState)ar.AsyncState;
            HttpWebRequest request = commentState.AsyncRequest;
            commentState.AsyncResponse = (HttpWebResponse)request.EndGetResponse(ar);

            var data = Reddit.GetResponseString(commentState.AsyncResponse.GetResponseStream());
            JToken json = JToken.Parse(data);
            if (json["json"].ToString().Contains("\"errors\": []"))
                Body = (String)commentState.ParameterValue;
            else
                throw new Exception("Error editing text.");
        }
    }

    public enum DistinguishType
    {
        Moderator,
        Admin, 
        Special,
        None
    }
}
