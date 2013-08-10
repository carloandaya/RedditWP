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
        public class VoteState
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

        public void Upvote()
        {
            var request = Reddit.CreatePost(VoteUrl);
            request.BeginGetRequestStream(new AsyncCallback(UpvoteRequest), request);
        }

        private void UpvoteRequest(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;

            Stream stream = request.EndGetRequestStream(ar);

            Reddit.WritePostBody(stream, new
            {
                dir = 1,
                id = FullName,
                uh = Reddit.User.Modhash
            });            

            request.BeginGetResponse(new AsyncCallback(UpvoteResponse), request);
        }

        private void UpvoteResponse(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(response.GetResponseStream());
            // TODO: check the data for success or failure
            Liked = true;
        }

        public void Downvote()
        {
            var request = Reddit.CreatePost(VoteUrl);
            request.BeginGetRequestStream(new AsyncCallback(DownvoteRequest), request);
        }

        private void DownvoteRequest(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;

            Stream stream = request.EndGetRequestStream(ar);

            Reddit.WritePostBody(stream, new 
            {
                dir = -1,
                id = FullName, 
                uh = Reddit.User.Modhash
            });

            request.BeginGetResponse(new AsyncCallback(DownvoteResponse), request);
        }

        private void DownvoteResponse(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(response.GetResponseStream());
            // TODO: check the data for success or failure
            Liked = false; 
        }

        public void Save()
        {
            var request = Reddit.CreatePost(SaveUrl);
            var stream = request.BeginGetRequestStream(new AsyncCallback(SaveRequest), request);
        }

        private void SaveRequest(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            Stream stream = request.EndGetRequestStream(ar);
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            request.BeginGetResponse(new AsyncCallback(SaveResponse), request);
        }

        private void SaveResponse(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(response.GetResponseStream());
            Saved = true;
        }

        public void Unsave()
        {
            var request = Reddit.CreatePost(UnsaveUrl);
            request.BeginGetRequestStream(new AsyncCallback(UnsaveRequest), request);
        }

        private void UnsaveRequest(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            Stream stream = request.EndGetRequestStream(ar);
            Reddit.WritePostBody(stream, new
            {
                id = FullName,
                uh = Reddit.User.Modhash
            });
            request.BeginGetResponse(new AsyncCallback(UnsaveResponse), request);
        }

        private void UnsaveResponse(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(response.GetResponseStream());
            Saved = false;
        }

        public void ClearVote()
        {
            var request = Reddit.CreatePost(VoteUrl);
            request.BeginGetRequestStream(new AsyncCallback(ClearVoteRequest), request);
        }

        private void ClearVoteRequest(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            Stream stream = request.EndGetRequestStream(ar);
            Reddit.WritePostBody(stream, new 
            {
                dir = 0,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            request.BeginGetResponse(new AsyncCallback(ClearVoteResponse), request);
        }

        private void ClearVoteResponse(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            var data = Reddit.GetResponseString(response.GetResponseStream());            
        }

        public void Vote(VoteType type)
        {
            VoteState voteState = new VoteState();
            var request = Reddit.CreatePost(VoteUrl);
            voteState.AsyncRequest = request;
            voteState.VoteType = type;
            request.BeginGetRequestStream(new AsyncCallback(VoteRequest), voteState);            
        }

        private void VoteRequest(IAsyncResult ar)
        {
            // get the state information
            VoteState voteState = (VoteState)ar.AsyncState;
            HttpWebRequest request = (HttpWebRequest)voteState.AsyncRequest;
            Stream stream = request.EndGetRequestStream(ar);
            Reddit.WritePostBody(stream, new
            {
                dir = (int)voteState.VoteType,
                id = FullName,
                uh = Reddit.User.Modhash
            });
            request.BeginGetResponse(new AsyncCallback(VoteResponse), voteState);
        }

        private void VoteResponse(IAsyncResult ar)
        {
            // get the state informatoin
            VoteState voteState = (VoteState)ar.AsyncState;
            HttpWebRequest request = (HttpWebRequest)voteState.AsyncRequest;

            // end the async request
            voteState.AsyncResponse = (HttpWebResponse)request.EndGetResponse(ar);            
            var data = Reddit.GetResponseString(voteState.AsyncResponse.GetResponseStream());
            Liked = null;
        }
        
    }
    
}
