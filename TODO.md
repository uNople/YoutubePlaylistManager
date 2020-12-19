Next:
- Write more tests
- Decide on update strategy
    - What happens if a title of a playlist changes, description of video, etc
    - What happens on deletion
    - What do/don't we read from the cache/disk to avoid getting too much data
    - How do we indicate a 'full update' vs an incremental
- Move from JSON on disk -> Sqlite on disk
    - Create some kind of store/stores (rather than accessing YoutubeCleanupToolDbContext directly)
- Add searching
    - Add indexes for searches and things

After?
- Create some kind of UI for what I have (Razer/express?)
- Re-architect program
    - Have some kind of 'SearchableField' attribute for later

Future features:
- Make a UI to modify my playlists easier than through youtube
- Cache thumbnails for playlists and videos (once it's in sqlite)
- build a search (make sure we're saving all the data we would want to search on)
- build some kind of auto-categorizer (put liked videos in specific playlist if they match things)

Final (countdown) goal
- Some kind of tool to manage my youtube videos / playlists / likes quicker than on the website
- Ability to cache titles/other info (and not clear them when videos get deleted)
- With the Ability to differentiate between music and youtube videos