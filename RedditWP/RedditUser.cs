using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditWP
{
    public class RedditUser : Thing
    {
        private const string OverviewUrl = "/user/{0}.json";
        private const string CommentsUrl = "/user/{0}/comments.json";
        private const string LinksUrl = "/user/{0}/submitted.json";
        private const string SubscribedSubredditsUrl = "/subreddits/mine.json";

        public RedditUser(Reddit reddit, JToken json)
            : base(json)
        {
            Reddit = reddit;
        }

        [JsonIgnore]
        protected Reddit Reddit { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("is_gold")]
        public bool HasGold { get; set; }
        
        [JsonProperty("is_mod")]
        public bool IsModerator { get; set; }
        
        [JsonProperty("comment_karma")]
        public int CommentKarma { get; set; }

        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Created { get; set; }

        public Listing<Comment> GetComments()
        {

        }

    }
}
