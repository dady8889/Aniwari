using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniwari.DAL.Constants;

public static class Paths
{
    public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aniwari\\");
    public static string StateFilePath => Path.Combine(AppDataPath, "state.dat");
    public static string TorrentCacheDirPath => Path.Combine(AppDataPath, "cache\\");
    public static string SettingsFilePath => Path.Combine(AppDataPath, "settings.json");
    public static string ArchiveDirPath => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
}
