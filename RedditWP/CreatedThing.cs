using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RedditWP
{
    public class CreatedThing : Thing
    {
        private Reddit Reddit { get; set; }

        public CreatedThing(Reddit reddit, JToken json)
            : base(json)
        {
            Reddit = reddit;
            JsonConvert.PopulateObject(json["data"].ToString(), this, reddit.JsonSerializerSettings);
        }

        [JsonProperty("created")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime Created { get; set; }
    }
}
