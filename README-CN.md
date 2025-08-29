AlbionOnline - 统计分析工具
===================

[![GitHub release (with filter)](https://img.shields.io/github/v/release/Triky313/AlbionOnline-StatisticsAnalysis?style=for-the-badge&labelColor=1E2126&color=0C637F)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases)
[![GitHub all releases downloads](https://img.shields.io/github/downloads/Triky313/AlbionOnline-StatisticsAnalysis/total?style=for-the-badge&labelColor=1E2126&color=EF476F)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases)[![Build + Tests Workflow Status (with event)](https://img.shields.io/github/actions/workflow/status/Triky313/AlbionOnline-StatisticsAnalysis/pr-build-and-unit-tests.yml?style=for-the-badge&label=%F0%9F%9B%A0%EF%B8%8F%20Build%20%2B%20Unit%20tests&labelColor=1E2126&color=09C3A5)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/actions/workflows/pr-build-and-unit-tests.yml)
![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/Triky313/AlbionOnline-StatisticsAnalysis/latest?style=for-the-badge&labelColor=1E2126&color=09C3A5)
[![GitHub issues](https://img.shields.io/github/issues/Triky313/AlbionOnline-StatisticsAnalysis?style=for-the-badge&labelColor=1E2126&color=FBAF69)](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/issues)
[![CodeFactor](https://www.codefactor.io/repository/github/triky313/albiononline-statisticsanalysis/badge/main?style=for-the-badge&labelColor=1E2126&color=0CB0A9)](https://www.codefactor.io/repository/github/triky313/albiononline-statisticsanalysis/overview/main)
[![Discord](https://img.shields.io/discord/772406813438115891?style=for-the-badge&logo=discord&logoColor=7289da&label=discord&labelColor=1E2126&color=7289da)](https://discord.gg/Wv5RWehbrU)
[![Donate PAYPAL](https://img.shields.io/badge/paypal-donate-1e477a?style=for-the-badge&labelColor=1E2126&color=1e477a)](https://www.paypal.com/donate/?hosted_button_id=N6T3CWXYNGHKC)
[![Support me on Patreon](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3DTriky313%26type%3Dpatrons&style=for-the-badge)](https://patreon.com/Triky313)

一个方便读取拍卖行数据并包含战利品拾取记录、伤害统计、地下城追踪、地下城关闭计时器、制作计算器、地图历史记录和玩家信息的工具

<p align="center" align='right'>
  <img src="https://user-images.githubusercontent.com/14247773/147143464-c36d0cba-dddb-4b34-bd2e-11e3f65e3289.png" data-canonical-src="https://user-images.githubusercontent.com/14247773/147143464-c36d0cba-dddb-4b34-bd2e-11e3f65e3289.png" width="400" height="400" />
</p>



## 开始使用

### 前置条件与安装

- 您需要 **Windows 10** 或更高版本
- 安装 **.NET 9.0 Desktop Runtime** (v9.0.0 或更高版本) [此处](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-9.0.0-windows-x64-installer) (用于启动工具)

**下载 Statistics Analysis 工具**

- [**下载**](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases/download/v7.5.3/StatisticsAnalysis-AlbionOnline-v7.5.3-x64.zip)
  解压 `.zip` 文件并双击启动 `StatisticsAnalysis.exe`。您可能看不到 `.exe` 扩展名。不用担心，通常是带有图标的那个文件。

  [最新版本](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/releases/latest)

![工具目录](https://user-images.githubusercontent.com/14247773/170473306-4dcc629e-384e-41b2-ada8-657cabe1b472.png)


### 通过 Socket 进行追踪

如果通过 socket 使用追踪功能，工具只需以管理员身份启动即可完全工作。

### 通过 Npcap 进行追踪

作为替代方案，有 Npcap 变体，为此必须安装 Npcap，但工具不再需要以管理员身份启动。
此外，还为 VPN 用户提供了过滤功能，允许进行 IP 和端口过滤。

https://npcap.com/ (通常最新版本应该可以工作！)

## 使用这个工具会不会导致封号

[讨论](https://forum.albiononline.com/index.php/Thread/124819-Regarding-3rd-Party-Software-and-Network-Traffic-aka-do-not-cheat-Update-16-45-U/) (原始链接),  [澄清](https://forum.albiononline.com/index.php/Thread/153238-DPS-METER/#:~:text=As%20noted%20on%20the%20GitHub,to%20use%20it%20without%20concern.) (2023 年社区经理简短答复)

✅ 仅监视

✅ 不修改我们的游戏客户端

✅ 不追踪不在玩家视野内的玩家

✅ 没有游戏内覆盖层

## 常见问题解答

### 支持哪些操作系统？

✅ Windows 10 及更高版本

❌ 不支持 Windows XP, Vista, 7 和 8！

❌ 目前不支持 Linux！

❌ 目前不支持 Mac！

### 我可以在 Geforce Now 上使用该工具吗？

不可以，这在技术上目前无法实现。

### 我可以在使用 ExitLag 或 VPN 或 游戏加速器时使用该工具吗？

可以，如果您使用 Npcap 追踪，通常可以使用 VPN 或 ExitLag。

注意：如果使用游戏加速器，请使用路由加速模式，雷神加速器为模式9

### 我的网速需要多快？

至少需要 1M/bit (256KB/s) 下载速度的互联网连接。

### 游戏未启动时我也可以使用该工具吗？

可以，但并非所有功能都可用。
重要的是，您需要在设置中将游戏服务器从自动设置为您选择的服务器。否则工具不知道应该为哪个服务器加载数据。

## WIKI

更多信息可以在 [Wiki](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis/wiki) 页面上找到。

## 工具网站

[triky313.github.io/AlbionOnline-StatisticsAnalysis](https://triky313.github.io/AlbionOnline-StatisticsAnalysis/)

## 捐赠

这个项目自 2019 年 6 月就已存在。我几乎每周都会投入 10+ 小时在这个项目上，并且我非常热爱它。这就是为什么我经常没有时间玩 Albion Online。如果您想支持这个项目，只需捐赠一些游戏物品或在 Patreon 上捐赠即可。

[Patreon - Triky313](https://www.patreon.com/triky313)

<img src="https://user-images.githubusercontent.com/14247773/166248069-3211a206-b475-4e83-860b-e5c51b9554bf.png" data-canonical-src="https://www.patreon.com/triky313" width="40" height="40" />

[PayPal - Triky313](https://www.paypal.com/donate/?hosted_button_id=N6T3CWXYNGHKC)

<img src="https://user-images.githubusercontent.com/14247773/201472890-33a0ed70-7ef8-4804-aa84-46f0a84f3168.png" width="100" height="100" />

## 贡献者

<!-- readme: contributors -start -->

<table>
<tr>
    <td align="center">
        <a href="https://github.com/Triky313">
            <img src="https://avatars.githubusercontent.com/u/14247773?v=4" width="50;" alt="Triky313"/>
            <br />
            <sub><b>Aaron Schultz</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/arabinos">
            <img src="https://avatars.githubusercontent.com/u/115917138?v=4" width="50;" alt="arabinos"/>
            <br />
            <sub><b>Marcin Wieczorek</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/bluenyx">
            <img src="https://avatars.githubusercontent.com/u/96876?v=4" width="50;" alt="bluenyx"/>
            <br />
            <sub><b>SeoheeKhang</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/isnullibi">
            <img src="https://avatars.githubusercontent.com/u/100205074?v=4" width="50;" alt="isnullibi"/>
            <br />
            <sub><b>Null</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ewersonmssilva">
            <img src="https://avatars.githubusercontent.com/u/26557729?v=4" width="50;" alt="ewersonmssilva"/>
            <br />
            <sub><b>Ewerson</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/NuberuSH">
            <img src="https://avatars.githubusercontent.com/u/45773746?v=4" width="50;" alt="NuberuSH"/>
            <br />
            <sub><b>Dani Tallón</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/kkkingim">
            <img src="https://avatars.githubusercontent.com/u/22095496?v=4" width="50;" alt="kkkingim"/>
            <br />
            <sub><b>Vagitus</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Faeeth">
            <img src="https://avatars.githubusercontent.com/u/37340968?v=4" width="50;" alt="Faeeth"/>
            <br />
            <sub><b>Faeeth</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Me1onSeed">
            <img src="https://avatars.githubusercontent.com/u/81557800?v=4" width="50;" alt="Me1onSeed"/>
            <br />
            <sub><b>瓜子</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/PurpleGale">
            <img src="https://avatars.githubusercontent.com/u/90148755?v=4" width="50;" alt="PurpleGale"/>
            <br />
            <sub><b>Null</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/taco0603">
            <img src="https://avatars.githubusercontent.com/u/19679024?v=4" width="50;" alt="taco0603"/>
            <br />
            <sub><b>Null</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/1027603857">
            <img src="https://avatars.githubusercontent.com/u/38471268?v=4" width="50;" alt="1027603857"/>
            <br />
            <sub><b>Null</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/acelan">
            <img src="https://avatars.githubusercontent.com/u/71646?v=4" width="50;" alt="acelan"/>
            <br />
            <sub><b>AceLan Kao</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ivanmaxlogiudice">
            <img src="https://avatars.githubusercontent.com/u/3275920?v=4" width="50;" alt="ivanmaxlogiudice"/>
            <br />
            <sub><b>Iván Máximiliano, Lo Giudice</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/devsurimlee">
            <img src="https://avatars.githubusercontent.com/u/53467957?v=4" width="50;" alt="devsurimlee"/>
            <br />
            <sub><b>Surim Lee</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/lx78WyY0J5">
            <img src="https://avatars.githubusercontent.com/u/84735589?v=4" width="50;" alt="lx78WyY0J5"/>
            <br />
            <sub><b>Null</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/rdayltx">
            <img src="https://avatars.githubusercontent.com/u/82792422?v=4" width="50;" alt="rdayltx"/>
            <br />
            <sub><b>DayLight</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/zenion">
            <img src="https://avatars.githubusercontent.com/u/4081449?v=4" width="50;" alt="zenion"/>
            <br />
            <sub><b>Josh Stout</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/Kukkimonsuta">
            <img src="https://avatars.githubusercontent.com/u/737093?v=4" width="50;" alt="Kukkimonsuta"/>
            <br />
            <sub><b>Lukáš Novotný</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Dibort">
            <img src="https://avatars.githubusercontent.com/u/7732931?v=4" width="50;" alt="Dibort"/>
            <br />
            <sub><b>Null</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/dyj0816">
            <img src="https://avatars.githubusercontent.com/u/40786887?v=4" width="50;" alt="dyj0816"/>
            <br />
            <sub><b>Redmeier</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/mleen4">
            <img src="https://avatars.githubusercontent.com/u/63968148?v=4" width="50;" alt="mleen4"/>
            <br />
            <sub><b>Michael</b></sub>
        </a>
    </td></tr>
</table>

<!-- readme: contributors -end -->