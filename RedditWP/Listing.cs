using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace RedditWP
{
    public class Listing<T> : IEnumerable<T> where T : Thing
    {
        private Reddit Reddit { get; set; }
        private String Url { get; set; }

        internal Listing(Reddit reddit, String url)
        {
            Reddit = reddit;
            Url = url;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ListingEnumerator<T>(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class ListingEnumerator<T> : IEnumerator<T> where T : Thing
        {
            private Listing<T> Listing { get; set; }
            private int CurrentPageIndex { get; set; }
            private string After { get; set; }
            private string Before { get; set; }
            private Thing[] CurrentPage { get; set; }

            public ListingEnumerator(Listing<T> listing)
            {
                Listing = listing;
                CurrentPageIndex = 0;
            }

            public T Current
            {
                get
                {
                    return (T)CurrentPage[CurrentPageIndex - 1];
                }
            }

            private void FetchNextPage()
            {

            }

            object IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }

            public bool MoveNext()
            {
                throw new NotImplementedException();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
