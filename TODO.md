Next/Current:
- Prototype UI in WPF
  - Wire up 'update playlist parts'
    - Extra permissions for the app in the google dev console <-- do this - cause the api side is working
	- Wire up 'Remove from playlist' call
  - Link to channel / video so we can open in YouTube
 - Wire up FilterType.Uncategorized

After:
- Create more permanent UI in cv++ languages/frameworks
- Blazor/express
- [project reunion](https://github.com/microsoft/ProjectReunion)

Future features:
- Cache thumbnails for playlists and videos (once it's in sqlite)
- build some kind of auto-categorizer (put liked videos in specific playlist if they match things)

Final (countdown) goal
- Some kind of tool to manage my YouTube videos / playlists / likes quicker than on the website
- Ability to cache titles/other info (and not clear them when videos get deleted)
- With the Ability to differentiate between music and YouTube videos

Deferred
- Refactor YouTubeApi so it uses IAsyncEnumerable (refactoring to make slightly more responsive)
- Instead of wrapping HttpClient, use HttpClientFactory and create a messagehandler that we can mock out instead
	https://www.nocture.dk/2013/05/21/csharp-unit-testing-classes-with-httpclient-dependence-using-autofixture/
	https://stackoverflow.com/questions/54227487/how-to-mock-the-new-httpclientfactory-in-net-core-2-1-using-moq
- Get EF Logging working in the console somewhere in the wpf app, and the console app
- Remove recursion in models - do it a more proper way (like just have the IDs, and read from the DB again)