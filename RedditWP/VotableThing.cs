using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;

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

        /// <summary>
        /// State information for Vote async call
        /// </summary>
        private class VotableThingState
        {
            public HttpWebRequest AsyncRequest { get; set; }
            public HttpWebResponse AsyncResponse { get; set; }
            public VoteType VoteType { get; set; }
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
        public bool Saved { get; set; }

        /// <summary>
        /// True if the logged in user has upvoted this.
        /// False if they have not. 
        /// Null if they have not cast a vote.
        /// </summary>
        public bool? Liked { get; set; }

        public async Task Upvote()
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                dir = 1,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(VoteUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            // TODO: check the data for success or failure
            Liked = true;
        }

        public async Task Downvote()
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                dir = -1,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(VoteUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            // TODO: check the data for success or failure
            Liked = false; 
        }

        public async Task Save()
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(SaveUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Saved = true;
        }

        public async Task Unsave()
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(UnsaveUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Saved = false;
        }

        public async Task ClearVote()
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                dir = 0,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(VoteUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
        }

        public async Task Vote(VoteType type)
        {
            HttpClient client = Reddit.CreateClient();
            StringContent content = Reddit.StringForPost(new
            {
                dir = (int)type,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            var response = await client.PostAsync(VoteUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Liked = null;
        }
        
    }
    
}
