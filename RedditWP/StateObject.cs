using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RedditWP
{
    public class StateObject
    {
        public HttpWebRequest Request { get; set; }
        public HttpWebResponse Response { get; set; }
        public Object ParameterValue { get; set; }
    }
}
