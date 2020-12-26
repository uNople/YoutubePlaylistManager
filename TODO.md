Next:
- Create some kind of UI for what I have (Razer/express?)

Future features:
- Cache thumbnails for playlists and videos (once it's in sqlite)
- build some kind of auto-categorizer (put liked videos in specific playlist if they match things)

Final (countdown) goal
- Some kind of tool to manage my YouTube videos / playlists / likes quicker than on the website
- Ability to cache titles/other info (and not clear them when videos get deleted)
- With the Ability to differentiate between music and YouTube videos

Deferred (probably not really needed)
- Refactor YouTubeApi so it uses IAsyncEnumerable (refactoring to make slightly more responsive)
- Handle secret revocation - eg, how do we prompt the client for reauth via google again