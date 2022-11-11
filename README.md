AlbionOnline - Statistics Analysis
===================
[![Github All Releases](https://img.shields.io/github/v/release/Triky313/AlbionOnline-StatisticsAnalysis)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases)
[![Github All Releases](https://img.shields.io/github/downloads/Triky313/AlbionOnline-StatisticsAnalysis/total.svg)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases) 
[![CodeFactor](https://www.codefactor.io/repository/github/triky313/albiononline-statisticsanalysis/badge/main)](https://www.codefactor.io/repository/github/triky313/albiononline-statisticsanalysis/overview/main)
[![.NET Desktop](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/actions/workflows/dotnet-desktop.yml)
![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/Triky313/AlbionOnline-StatisticsAnalysis/latest?color=AF3B7F)
[![GitHub issues](https://img.shields.io/github/issues/Triky313/AlbionOnline-StatisticsAnalysis)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/issues)
[![Discord](https://img.shields.io/discord/772406813438115891?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat)](https://discord.gg/Wv5RWehbrU)
[![Donate](https://img.shields.io/badge/paypal-donate-1e477a)](https://www.paypal.me/schultzaaron)
[![Support me on Patreon](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3DTriky313%26type%3Dpatrons&style=flat)](https://patreon.com/Triky313)

A tool to easily read auction house data with a loot logger, damage meter, dungeon tracker, dungeon entry timer, crafting calculator, map history and player information

<p align="center" align='right'>
  <img src="https://user-images.githubusercontent.com/14247773/147143464-c36d0cba-dddb-4b34-bd2e-11e3f65e3289.png" data-canonical-src="https://user-images.githubusercontent.com/14247773/147143464-c36d0cba-dddb-4b34-bd2e-11e3f65e3289.png" width="400" height="400" />
</p>

## Getting Started

### Prerequisites & Installation
- You need **Windows 10** or higher
- Install **.NET 6.0 Desktop Runtime** (v6.0.5 or higher) [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.5-windows-x64-installer) (To start the tool)
- Installing **Npcap** Free Edition (v1.6 or higher) [here](https://npcap.com/#download) (For Game tracking)

**Download the Statistics Analysis Tool**
- [**DOWNLOAD**](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases/download/v5.14.2/StatisticsAnalysis-AlbionOnline-v5.14.2-x64.zip)
Unzip the `.zip` file and start `StatisticsAnalysisTool.exe` with a double click. You may not be able to see the `.exe`. Don't worry, usually it's the file with the icon.

![tool_dir](https://user-images.githubusercontent.com/14247773/170473306-4dcc629e-384e-41b2-ada8-657cabe1b472.png)


## Is This Allowed
https://forum.albiononline.com/index.php/Thread/124819-Regarding-3rd-Party-Software-and-Network-Traffic-aka-do-not-cheat-Update-16-45-U/

- [x] Only monitors
- [x] Does not modify our game client
- [x] Does not track players that are not within the player's view
- [x] Does not have an overlay to the game

## SETTINGS 

### ITEM LIST SOURCE
If the item list is outdated, you can change it yourself. Just change the "ITEM LIST SOURCE" URL for that. 

Another good source is https://github.com/broderickhyman/ao-bin-dumps

Or you extract the files yourself from the game. More information can be found here: https://github.com/broderickhyman/ao-id-extractor


## FAQ
### Which operating system is supported?
✅ Windows 10 and later

❌ Windows XP, Vista, 7 and 8 are not supported!

❌ Linux is currently not supported!

### Can I use the tool with ExitLag or VPN?
Unfortunately, **ExitLag is not supported**, but there are other VPN services that the tool works well with. There is no support from the developers for this. Validating it would be too time-consuming.

### How fast does my internet need to be?
An internet connection with at least 1M/bit (256KB/s) download rate.

### The tool cannot download the ItemList.json or Item.json, what to do?
If a button like the one in the screenshot appears and the automatic download of the files repeatedly does not work, the following can be done:

![try-download-again-button](https://user-images.githubusercontent.com/14247773/170475039-3739e5cd-5d02-41bf-a77d-f58290de75a3.png)

Download the file https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/formatted/items.json and rename it to ItemList.json.
and
Download this file https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/items.json 
Then put both files in the tools folder.

Then restart the tool and everything should work.


## INFORMATION

### Where does the price information come from?
From this project: [https://www.albion-online-data.com/](https://www.albion-online-data.com/)

### I want more current prices!
Download the [Albion online data client](https://www.albion-online-data.com/) and scan the auction houses.


## CONTRIBUTING

### Problem or question?
Create an issue [here](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/issues).

### Language translation

#### How it works
To add your language, you only need to create or adapt a file in the `Languages` directory.
The file must always have the language code. 

Example:
```
en-US.xml
```

You can find language codes here: https://www.andiamo.co.uk/resources/iso-language-codes/

#### Create file
Copy the en-US file and change everything, then all APP-CODES should be available. 
```
<translation name = "EVERY_3_DAYS">HERE YOUR TEXT</translation>
```

## This website
[triky313.github.io/AlbionOnline-StatisticsAnalysis](https://triky313.github.io/AlbionOnline-StatisticsAnalysis/)

## DONATIONS
This project has existed since June 2019. Almost every week I put +10 hours into this project and I love it. That's why I often don't have the time to play Albion Online. So if you want to make me happy and support this project, just donate a few items or donate on Patreon.

<img src="https://user-images.githubusercontent.com/14247773/166248069-3211a206-b475-4e83-860b-e5c51b9554bf.png" data-canonical-src="https://www.patreon.com/triky313" width="40" height="40" /> [Patreon - Triky313](https://www.patreon.com/triky313)
