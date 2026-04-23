using System;
using System.Threading;
using System.Windows;

namespace KustoTimeGrab;

public partial class App : Application
{
    private static Mutex? _singleInstance;
    private TrayApp? _tray;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _singleInstance = new Mutex(initiallyOwned: true,
            name: @"Local\KustoTimeGrab.SingleInstance", out var createdNew);

        if (!createdNew)
        {
            Logger.Log("another instance is already running; exiting");
            Shutdown();
            return;
        }

        _tray = new TrayApp();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _singleInstance?.ReleaseMutex();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}