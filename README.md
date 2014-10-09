sharp-twitter
=============

.NET library for easy interaction with Twitter

#### Usage ####

```cs
var creds = new NetworkCredential("User", "Pass);

using (var auth = new TwitterAuthorization(creds))
{
	if (await auth.TrySignInAsync())
	{
		using (var service = new TwitterService(auth))
		{
			await service.PostTweetAsync("Hello world!");
		}
	}
}
```

#### Supported ####
> Sending tweets
> Retweeting tweets
> Following users
