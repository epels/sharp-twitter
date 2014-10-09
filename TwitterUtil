using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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

namespace SharpTwitter
{
    /// <summary>
    /// Utility class for Twitter
    /// </summary>
    public static class TwitterUtil
    {
        public const string TwitterHomeUrl = "https://twitter.com";
        public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:32.0) Gecko/20100101 Firefox/32.0";
        public const string ContentType = "application/x-www-form-urlencoded";

        public static Dictionary<string, string> UserIdCache { get; private set; }

        /// <summary>
        /// Asynchronously fetches a user's profile ID. 
        /// Automatically caches ID's and returns those if available.
        /// </summary>
        /// <param name="username">the username to check</param>
        /// <returns>the user profile ID</returns>
        public static async Task<string> FetchUserIdAsync(string username)
        {
            const string userIdPattern = "(\"profile_user\").*?(\"id_str\").*?(\".*?\")";

            if (UserIdCache == null)
            {
                UserIdCache = new Dictionary<string, string>();
            }
            else
            {
                string userId;

                if (UserIdCache.TryGetValue(username, out userId))
                {
                    return userId;
                }
            }

            using (var client = new HttpClient())
            {
                var profileUrl = string.Format("{0}/{1}", TwitterHomeUrl, username);
                var responseContent = await client.GetStringAsync(new Uri(profileUrl));

                var match = Regex.Match(responseContent.Replace("&quot;", "\""), userIdPattern);

                if ((!match.Success) || (responseContent.IndexOf("profile_user", StringComparison.OrdinalIgnoreCase) == -1))
                    throw new WebException("Could not fetch user ID.");

                var userId = match.Groups[3].Value.Trim('"');

                UserIdCache.Add(username, userId);
                return userId;
            }
        }
    }
}
