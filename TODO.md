Next/Current:
- Prototype UI in WPF
  - Dark mode / prettify

Tidy-ups:
WPF:
- Somewhere to set options in the WPF app (eg, path to DB, what monitor to start on, what theme)
- Show dialogs on same screen as running app
- Improve performance of log updating the UI (ui freezes when updates are happening and we have a lot of messages)
- Create some icons (top-left as well as taskbar etc)

Console app:
- Ability to set the path to client_secret.json
- Ability to set the DB path

General stuff:
- Pass in cancellation token everywhere

After:
- Create more permanent UI in cv++ languages/frameworks
- Blazor/express
- [project reunion](https://github.com/microsoft/ProjectReunion)

Future features:
- Cache all resolutions of the thumbnails for playlists and videos (once it's in sqlite)
  - Also store resolution of the thumbnail so we can calculate aspect ratio etc correctly
- build some kind of auto-categorizer (put liked videos in specific playlist if they match things)
- regex search

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