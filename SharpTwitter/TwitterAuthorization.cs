using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
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
    /// Attempts to sign in to Twitter. Cookie can be reused for interaction with Twitter
    /// </summary>
    public sealed class TwitterAuthorization : CookieContainer, IDisposable
    {
        private readonly NetworkCredential _credentials;
        private readonly HttpClient _client;

        public bool SignedIn { get; private set; }
        public string AuthenticityToken { get; private set; }

        public TwitterAuthorization(NetworkCredential creds)
        {
            _credentials = creds;

            _client = new HttpClient(new HttpClientHandler
            {
                CookieContainer = this
            }, true);
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(TwitterUtil.UserAgent);
        }

        /// <summary>
        /// Fetches the authenticity token for the current Twitter session
        /// </summary>
        /// <returns>The authenticity token for the current Twitter session</returns>
        private async Task<string> FetchAuthenticityTokenAsync()
        {
            const string tokenPattern = ".*?(\"authenticity_token\").*?(\".*?\")";
            var pageSource = await _client.GetStringAsync(new Uri(TwitterUtil.TwitterHomeUrl));

            var match = Regex.Match(pageSource, tokenPattern);

            if (!match.Success)
                throw new WebException("Could not fetch authenticity token.");

            return (AuthenticityToken = match.Groups[2].Value.Trim('"'));
        }

        /// <summary>
        /// Attempts signing in to Twitter
        /// </summary>
        /// <returns>A boolean indicating success or failure</returns>
        public async Task<bool> TrySignInAsync()
        {
            using (var message = new HttpRequestMessage(HttpMethod.Post, new Uri(TwitterUtil.TwitterHomeUrl + "/sessions")))
            {
                message.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "session[username_or_email]", _credentials.UserName },
                    { "session[password]", _credentials.Password },
                    { "return_to_ssl", "true" },
                    { "scribe_log", string.Empty },
                    { "redirect_after_login", "/" },
                    { "authenticity_token", await FetchAuthenticityTokenAsync() }
                });

                message.Content.Headers.ContentType = new MediaTypeHeaderValue(TwitterUtil.ContentType);

                var response = await _client.SendAsync(message);
                var responseContent = await response.Content.ReadAsStringAsync();

                return (SignedIn = responseContent.IndexOf("js-username-field", StringComparison.OrdinalIgnoreCase) == -1);
            }
        }

        /// <summary>
        /// Disposes the HttpClient instance. Cookie can still be used afterwards
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
