using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniwari.DAL.Constants;

public static class Paths
{
    public static string StateFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aniwari\\", "state.dat");
    public static string TorrentCacheDirPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aniwari\\", "cache\\");
    public static string SettingsFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aniwari\\", "settings.json");
    public static string ArchiveDirPath => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
}
