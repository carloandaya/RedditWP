using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RedditWP
{
    public static class Extensions
    {
        public static T ValueOrDefault<T>(this IEnumerable<JToken> enumerable)
        {
            if (enumerable == null)
                return default(T);
            return enumerable.Value<T>();
        }
    }
}
