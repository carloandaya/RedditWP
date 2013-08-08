using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditWP
{
    public class VotableThing : CreatedThing
    {
        public enum VoteType
        {
            Upvote = 1, 
            None = 0,
            Downvote = -1
        }

        private const string VoteUrl = "/api/vote";
        private const string SaveUrl = "/api/save";
        private const string UnsaveUrl = "/api/unsave";

        [JsonIgnore]
        private Reddit Reddit { get; set; }

        public VotableThing(Reddit reddit, JToken json)
            : base(reddit, json)
        {
            Reddit = reddit;
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
        }

        [JsonProperty("downs")]
        public int Downvotes { get; set; }

        [JsonProperty("ups")]
        public int Upvotes { get; set; }
    
        [JsonProperty("saved")]
        public int Saved { get; set; }

        /// <summary>
        /// True if the logged in user has upvoted this.
        /// False if they have not. 
        /// Null if they have not cast a vote.
        /// </summary>
        public bool? Liked { get; set; }

        public void Upvote()
        {
            var request = Reddit.CreatePost(VoteUrl);
            // TODO: Write a proper asynchronous call
            request.BeginGetRequestStream(new AsyncCallback(UpvoteRequest), request);
        }

        private void UpvoteRequest(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

        public void Downvote()
        {
            var request = Reddit.CreatePost(VoteUrl);
            // TODO: Write a proper asynchronous call
        }
    }
}
