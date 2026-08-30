# Patch Notes - August 30, 2026

This update improves tracking reliability, gives you more control over which features are active, and expands player, trade, combat, and loot analysis.

## Highlights

- Added a redesigned Player Information view with server-specific searches for the America, Asia, and Europe servers, improved search feedback, and player avatars.
- Added full French localization across the application.
- Added separate tracking controls for Trade Monitoring, crafting costs, player trades, dungeons, logging, mob loot, player loot, kills, and the Loot Comparator.
- Added player loot and combat events to loot analysis, including kill and death counts and clearer loot-status summaries for each player.
- Added crafting costs to Trade Monitoring and improved the detection of purchases made directly from merchants.

## Improvements

- The updater now shows clearer download and installation progress and provides better feedback when an update fails.
- Tracking now detects incoming Albion Online game data more clearly and can automatically recover after network or adapter changes.
- Improved support for large or fragmented game-data packets, reducing the chance of missed tracking events.
- Gathering sessions now store the character name, making sessions easier to identify when playing multiple characters.
- Static Dungeon reports now only include chests that were actually opened.
- Item icons refresh automatically after their images become available.
- Current-map information now includes a clearer zone-type icon.
- Damage Meter abilities now use more reliable names and icons, including fallbacks for missing images.
- Ability tooltips and descriptions now support the original bold and color formatting from the game.
- Logging and Loot Comparator filters received convenient all-on/all-off controls and a cleaner, more consistent layout.
- Removed the unused session timer from Live Stats.

## Fixes

- Improved network recovery to prevent tracking from remaining inactive after connection changes.
- Improved Player Information searches by preventing outdated results from replacing newer searches and by clearly showing when a search is running.
- Improved Trade Monitoring accuracy when classifying instant merchant purchases.
- Improved the reliability of spell matching for damage and healing events.
- Improved loot summaries and exported comparison data when kills, deaths, or different loot states are present.

_Included commits: `7230f1502fc8b063fcfe97eef941d2bf9fee74a1` through `d370bee321146f848706586bc41f9665a9844e71`._
