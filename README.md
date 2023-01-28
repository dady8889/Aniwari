<h1 align="center">Aniwari</h2>

<h4 align="center">
<b>Anime schedule with a torrent client and MyAnimeList integration</b>
</h4>

<p align="center">
<a href="#about">About</a> •
<a href="#features">Features</a> •
<a href="#installing">Installing</a> •
<a href="https://github.com/dady8889/Aniwari/wiki/Frequently-Asked-Questions">FAQ</a> •
<a href="https://github.com/dady8889/Aniwari/releases">Download</a>
</ul>

<p align="center" width="100%">
<img width="66%" src="./doc/intro.gif" alt="Showcase" />
</p>

## **About**

Aniwari is an application aimed to ease watching currently airing anime in the season.
You can often find yourself in situations where you open a schedule + MAL + Nyaa (or any other service) and then switch back and forth between them, which can get annoying. Aniwari provides a way to integrate all these services into one. The application has been designed with responsiveness and touch-screens in mind.

<b>The only supported platform is currently Windows 10+ (.NET 7)</b>

## **Features**

### **Schedule**
<ul>
<li>List airing anime for each day</li>
<li>Automatic conversion from Japan to Local time</li>
<li>Click on an anime to view more information</li>
<li>Choose which anime you are planning to watch</li>
<li>View multiple days at once in fullscreen</li>
</ul>

### **Watchlist**
<ul>
<li>List all anime you are watching</li>
<li>Click on an anime to list all aired episodes</li>
<li>Force a number of episodes (in case of irregular schedule) <a href="https://github.com/dady8889/Aniwari/wiki/Number-of-episodes">Read more</a></li>
<li>Search for a torrent on Nyaa <a href="https://github.com/dady8889/Aniwari/wiki/Searching-torrents">Read more</a></li>
<li>Set a preferred search string <a href="https://github.com/dady8889/Aniwari/wiki/Searching-torrents">Read more</a></li>
<li>Track the torrent's status</li>
<li>Opens your video player to watch an episode</li>
<li>Delete a downloaded episode</li>
</ul>

### **MyAnimeList**
<ul>
<li>Log into your MAL <a href="https://github.com/dady8889/Aniwari/wiki/MyAnimeList">Read more</a></li>
<li>Import your watching list</li>
<li>Automatically update your watched episodes</li>
<li>Automatically add new anime to your MAL</li>
</ul>

### **Torrents**

<ul>
<li>Fully integrated within Watchlist</li>
<li>Set maximum seeding/downloading speed</li>
<li>Set maximum seed ratio</li>
<li>Automatic save and resume on restart</li>
<li>Choose archive location</li>
</ul>

### **Visuals**

<ul>
<li>White mode / Dark mode</li>
<li>Custom theme color</li>
<li>Automatically generated accent colors</li>
<li>Custom background</li>
<li>Responsive layout</li>
<li>Touch friendly</li>
<li>Heads up notifications</li>
<li>Anime titles language preference (Japanese, English, Romanized)</li>
</ul>

### **Updates**
<ul>
<li>Check for updates in Settings</li>
<li>Automatic install and restart</li>
</ul>

## **Installing**

Go to the <a href="https://github.com/dady8889/Aniwari/releases">Download</a> page and download the latest zip package, then unpack it into a folder anywhere you want.

Aniwari needs the .NET 7 framework to work, however if you don't have it installed it *should* give you a link to download it when you try to open the app. (Updated Windows 10+ installations have newest .NET automatically, so it should not be a problem)

Application preferences are saved into the `%appdata%\Aniwari` folder.

<b>(Installer is work in progress)</b>

## **Bugs**

The following bugs has been found and cannot be fixed, because they are in the framework itself.
- Windows 10 19044 - Scrolling while scroll lock is OFF doesn't work - apparently turning scroll lock ON fixes the issue

## **Building and Development**

Aniwari is made in **C# MAUI Blazor**. You need to have **Visual Studio 2022** with specific MAUI workload installed.

We are using a fork of **Onova** for updates, so currently you have to publish it locally before the first build.
I recommend to use the **publish script** in the solution folder first, and then you can build and debug without any problems. Just call `.\publish.ps1` in the Package Manager Console.

No other dependencies should be necessary, just keep in mind there are projects listed as **git submodules**.

## **Open source**
The following open source projects have been used:
<ul>
<li><a href="https://github.com/Ervie/jikan.net">Jikan.NET</a></li>
<li><a href="https://github.com/Jirubizu/NyaaWrapper">NyaaWrapper</a></li>
<li><a href="https://github.com/Tyrrrz/Onova">Onova</a></li>
<li><a href="https://github.com/alanmcgovern/monotorrent">MonoTorrent</a></li>
</ul>

Aniwari is licensed under the MIT license.
