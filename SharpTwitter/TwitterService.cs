using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

	/*
     * Creator: Emile Pels
     * 
     * Contact: 
     * E-mail: emile.pels@hva.nl
     * 
     * Created: 10.09.2014
     */

namespace TwitterDemo.SharpTwitter
{
    /// <summary>
    /// Offers various methods to interact with Twitter
    /// </summary>
    public class TwitterService : IDisposable
    {
        private readonly HttpClient _client;
        private readonly TwitterAuthorization _cookie;
        private bool _disposed;

        /// <summary>
        /// Sets up the http client
        /// </summary>
        /// <param name="cookie">an object of AuthorizationCookie which is signed in to Twitter</param>
        public TwitterService(TwitterAuthorization cookie)
        {
            if (!(_cookie = cookie).SignedIn)
                throw new ArgumentException("The cookie must contain an active Twitter session.");

            _client = new HttpClient(new HttpClientHandler
            {
                CookieContainer = cookie
            }, true);
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(TwitterUtil.UserAgent);
        }

        /// <summary>
        /// Asynchronously posts URL encoded content to the specified URL
        /// </summary>
        /// <param name="content">the content to post as a dictionary of string, string</param>
        /// <param name="postUrl">the URL to post the content to</param>
        /// <returns>the server's response to the request</returns>
        private async Task<string> PostContentAsync(Dictionary<string, string> content, Uri postUrl)
        {
            using (var message = new HttpRequestMessage(HttpMethod.Post, postUrl))
            {
                message.Content = new FormUrlEncodedContent(content);
                message.Content.Headers.ContentType = new MediaTypeHeaderValue(TwitterUtil.ContentType);

                var response = await _client.SendAsync(message);
                var responseContent = await response.Content.ReadAsStringAsync();

                return responseContent;
            }
        }

        /// <summary>
        /// Asynchronously posts a tweet to the currently signed in Twitter account
        /// </summary>
        /// <param name="tweet">the content to post as a tweet</param>
        public async Task PostTweetAsync(string tweet)
        {
            if ((string.IsNullOrWhiteSpace(tweet)) || (tweet.Length > 140))
                throw new ArgumentException("Tweet can not be null, empty, whitespace or over 140 characters.");

            var content = new Dictionary<string, string>
            {
                {"authenticity_token", _cookie.AuthenticityToken},
                {"place_id", string.Empty},
                {"status", tweet},
                {"tagged_users", string.Empty}
            };

            await PostContentAsync(content, new Uri(TwitterUtil.TwitterHomeUrl + "/i/tweet/create"));
        }

        /// <summary>
        /// Asynchronously retweets a tweet
        /// </summary>
        /// <param name="tweetUrl">the URL of the tweet</param>
        public async Task RetweetAsync(Uri tweetUrl)
        {
            var absoluteUrl = tweetUrl.AbsoluteUri;
            long tweetId;

            if ((absoluteUrl.IndexOf("/status/", StringComparison.OrdinalIgnoreCase) == -1) ||
                (!long.TryParse(absoluteUrl.Substring(absoluteUrl.LastIndexOf('/') + 1), out tweetId)))
                throw new FormatException("The passed URL is not valid.");

            await RetweetAsync(tweetId);
        }

        /// <summary>
        /// Asynchronously retweets a tweet
        /// </summary>
        /// <param name="tweetId">the ID of the tweet</param>
        public async Task RetweetAsync(long tweetId)
        {
            var content = new Dictionary<string, string>
            {
                {"authenticity_token", _cookie.AuthenticityToken},
                {"id", tweetId.ToString(CultureInfo.InvariantCulture)}
            };

            await PostContentAsync(content, new Uri(TwitterUtil.TwitterHomeUrl + "/i/tweet/retweet"));
        }

        /// <summary>
        /// Asynchronously follows a user from the currently signed in Twitter account
        /// </summary>
        /// <param name="username">the user to follow. User ID will be fetched through TwitterUtil</param>
        public async Task FollowAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username can not be null, empty or whitespace.");

            var userId = await TwitterUtil.FetchUserIdAsync(username);

            var content = new Dictionary<string, string>
            {
                {"authenticity_token", _cookie.AuthenticityToken},
                {"challenges_passed", "false"},
                {"handles_challenges", "1"},
                {"inject_tweet", "false"},
                {"user_id", userId}
            };

            await PostContentAsync(content, new Uri(TwitterUtil.TwitterHomeUrl + "/i/user/follow"));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _client.Dispose();
                _cookie.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Disposes the http client instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
