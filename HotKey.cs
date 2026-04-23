using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace KustoTimeGrab;

/// <summary>Registers a process-wide hotkey via RegisterHotKey.</summary>
public sealed class HotKey : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private static int _nextId = 9000;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly int _id = System.Threading.Interlocked.Increment(ref _nextId);
    private readonly HwndSource _source;
    private readonly Action _callback;

    public HotKey(uint modifiers, uint key, Action callback)
    {
        _callback = callback;

        // Create a message-only window to receive WM_HOTKEY.
        var param = new HwndSourceParameters("KustoTimeGrab.HotKey")
        {
            WindowStyle = 0,
            ParentWindow = new IntPtr(-3) // HWND_MESSAGE
        };
        _source = new HwndSource(param);
        _source.AddHook(WndProc);

        if (!RegisterHotKey(_source.Handle, _id, modifiers, key))
            throw new InvalidOperationException("Failed to register hotkey (already in use?).");
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == _id)
        {
            _callback();
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterHotKey(_source.Handle, _id);
        _source.Dispose();
    }
}