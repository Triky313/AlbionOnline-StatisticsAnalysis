AlbionOnline - Statistics Analysis
===================
[![Github All Releases](https://img.shields.io/github/v/release/Triky313/AlbionOnline-StatisticsAnalysis)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases)
[![Github All Releases](https://img.shields.io/github/downloads/Triky313/AlbionOnline-StatisticsAnalysis/total.svg)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases) 
[![CodeFactor](https://www.codefactor.io/repository/github/triky313/albiononline-statisticsanalysis/badge/main)](https://www.codefactor.io/repository/github/triky313/albiononline-statisticsanalysis/overview/main)
[![Build + Tests](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/actions/workflows/pr-build-and-unit-tests.yml/badge.svg)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/actions/workflows/dotnet-desktop.yml)
![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/Triky313/AlbionOnline-StatisticsAnalysis/latest?color=AF3B7F)
[![GitHub issues](https://img.shields.io/github/issues/Triky313/AlbionOnline-StatisticsAnalysis)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/issues)
[![Discord](https://img.shields.io/discord/772406813438115891?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat)](https://discord.gg/Wv5RWehbrU)
[![Donate](https://img.shields.io/badge/paypal-donate-1e477a)](https://www.paypal.com/donate/?hosted_button_id=N6T3CWXYNGHKC)
[![Support me on Patreon](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3DTriky313%26type%3Dpatrons&style=flat)](https://patreon.com/Triky313)

A tool to easily read auction house data with a loot logger, damage meter, dungeon tracker, dungeon entry timer, crafting calculator, map history and player information

<p align="center" align='right'>
  <img src="https://user-images.githubusercontent.com/14247773/147143464-c36d0cba-dddb-4b34-bd2e-11e3f65e3289.png" data-canonical-src="https://user-images.githubusercontent.com/14247773/147143464-c36d0cba-dddb-4b34-bd2e-11e3f65e3289.png" width="400" height="400" />
</p>

## Getting Started

### Prerequisites & Installation
- You need **Windows 10** or higher
- Install **.NET 7.0 Desktop Runtime** (v7.0.9 or higher) [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-7.0.9-windows-x64-installer) (To start the tool)
- Installing **Npcap** Free Edition (v1.6 or higher) [here](https://npcap.com/#download) (For Game tracking)

**Download the Statistics Analysis Tool**
- [**DOWNLOAD**](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases/download/v6.3.3/StatisticsAnalysis-AlbionOnline-v6.3.3-x64.zip)
Unzip the `.zip` file and start `StatisticsAnalysisTool.exe` with a double click. You may not be able to see the `.exe`. Don't worry, usually it's the file with the icon.

![tool_dir](https://user-images.githubusercontent.com/14247773/170473306-4dcc629e-384e-41b2-ada8-657cabe1b472.png)


## Is This Allowed
[Debate](https://forum.albiononline.com/index.php/Thread/124819-Regarding-3rd-Party-Software-and-Network-Traffic-aka-do-not-cheat-Update-16-45-U/)  
[Clarified](https://forum.albiononline.com/index.php/Thread/153238-DPS-METER/#:~:text=As%20noted%20on%20the%20GitHub,to%20use%20it%20without%20concern.)  

- [x] Only monitors
- [x] Does not modify our game client
- [x] Does not track players that are not within the player's view
- [x] Does not have an overlay to the game

## FAQ
### Which operating system is supported?
✅ Windows 10 and later

❌ Windows XP, Vista, 7 and 8 are not supported!

❌ Linux is currently not supported!

❌ Mac is currently not supported!

### Can I use the tool with Geforce Now
No, unfortunately this is not technically possible.

### Can I use the tool with ExitLag or VPN?
Yes, VPN or ExitLag can generally be used. 
In most cases this should work without problems. In the event of problems, it can help if you deactivate network filtering in the tool settings. 
If that doesn't help, a proxy redirect can help.

### How fast does my internet need to be?
An internet connection with at least 1M/bit (256KB/s) download rate.

### Can I use the tool even if the game is not started
Yes, but not all features are available. 
It is only important that you set the game server from automatic to one of your choice in the settings. Otherwise the tool does not know for which server it should load data.

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

[Patreon - Triky313](https://www.patreon.com/triky313)

<img src="https://user-images.githubusercontent.com/14247773/166248069-3211a206-b475-4e83-860b-e5c51b9554bf.png" data-canonical-src="https://www.patreon.com/triky313" width="40" height="40" />

[PayPal - Triky313](https://www.paypal.com/donate/?hosted_button_id=N6T3CWXYNGHKC)

<img src="https://user-images.githubusercontent.com/14247773/201472890-33a0ed70-7ef8-4804-aa84-46f0a84f3168.png" width="100" height="100" />
