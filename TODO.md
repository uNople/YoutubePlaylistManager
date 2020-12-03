Next:
- Continue the sisyphus-like grind of checking if there's some kind of data that tells us if it's music or video
- Compare playlistData and video, see if it's worth keeping data from playlistData (like order in playlist)
- Make sure with the linqpad reporting side we're getting tags / searchable data for everything

After?
- Write tests, have decent coverage
- Re-architect program
    - Have some kind of 'SearchableField' attribute for later
- Add Sqlite - not saving JSON to disk -> EF core
- Create some kind of UI for what I have

Future features:
- Make a UI to modify my playlists easier than through youtube
- Cache thumbnails for playlists and videos (once it's in sqlite)
- build a search
- build some kind of auto-categorizer (put liked videos in specific playlist if they match things)

Final (countdown) goal
- Some kind of tool to manage my youtube videos / playlists / likes quicker than on the website
- With the Ability to differentiate between music and youtube videos