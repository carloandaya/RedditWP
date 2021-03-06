﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;

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

            private async Task FetchNextPage()
            {
                var url = Listing.Url;
                if (After != null)
                {
                    if (url.Contains("?"))
                        url += "&after=" + After;
                    else
                        url += "?after=" + After;
                }
                HttpClient client = Listing.Reddit.CreateClient();
                var response = await client.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();
                var json = JToken.Parse(responseContent);
                if (json["kind"].ValueOrDefault<string>() != "Listing")
                    throw new FormatException("Reddit responded with an object that is not a listing.");
                Parse(json);
            }            

            private void Parse(JToken json)
            {
                var children = json["data"]["children"] as JArray;
                CurrentPage = new Thing[children.Count];
                for (int i = 0; i < CurrentPage.Length; i++)
                    CurrentPage[i] = Thing.Parse(Listing.Reddit, children[i]);
                After = json["data"]["after"].Value<string>();
                Before = json["data"]["before"].Value<string>();
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public async Task<bool> MoveNext()
            {
                if (CurrentPage == null)
                    await FetchNextPage();
                if (CurrentPageIndex >= CurrentPage.Length)
                {
                    if (After == null)
                        return false;
                    await FetchNextPage();
                    ResetCurrentPageIndex();
                }
                CurrentPageIndex++;
                return true;
            }

            private void ResetCurrentPageIndex()
            {
                CurrentPageIndex = 0;
            }

            public void Reset()
            {
                After = Before = null;
            }

            public void Dispose()
            {
                // ...
            }
        }
    }
}
