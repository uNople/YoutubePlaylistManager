Next:
- Add basic searching (title/description)
    - Add indexes for searches and things
- Write tests
- Handle deletion of a video - preserve whatever data we can locally
- Refactor GetAndCachedata so we're not repeating too much code
- Handle unicode text

After
- Re-architect program - does what I have make sense
- Create some kind of UI for what I have (Razer/express?)

Future features:
- Make a UI to modify my playlists easier than through youtube
- Cache thumbnails for playlists and videos (once it's in sqlite)
- build a search (make sure we're saving all the data we would want to search on)
- build some kind of auto-categorizer (put liked videos in specific playlist if they match things)
- Handle secret revocation - eg, how do we prompt the client for reauth via google again

Final (countdown) goal
- Some kind of tool to manage my youtube videos / playlists / likes quicker than on the website
- Ability to cache titles/other info (and not clear them when videos get deleted)
- With the Ability to differentiate between music and youtube videos