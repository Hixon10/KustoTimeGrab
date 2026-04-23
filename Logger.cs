using System;
using System.IO;

namespace KustoTimeGrab;

internal static class Logger
{
    private const long MaxBytes = 256 * 1024; // 256 KB rotation threshold

    public static string LogPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KustoTimeGrab", "KustoTimeGrab.log");

    private static readonly object _gate = new();

    static Logger()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
        }
        catch
        {
        }

        RotateIfLarge();
    }

    public static void Log(string line)
    {
        try
        {
            lock (_gate)
            {
                File.AppendAllText(LogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {line}{Environment.NewLine}");
            }
        }
        catch
        {
            /* logging must never throw */
        }
    }

    private static void RotateIfLarge()
    {
        try
        {
            var fi = new FileInfo(LogPath);
            if (fi.Exists && fi.Length > MaxBytes)
            {
                var bak = LogPath + ".old";
                if (File.Exists(bak)) File.Delete(bak);
                File.Move(LogPath, bak);
            }
        }
        catch
        {
        }
    }
}