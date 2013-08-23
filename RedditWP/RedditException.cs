using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWP
{
    public class RedditException
    {
        public RedditException(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
