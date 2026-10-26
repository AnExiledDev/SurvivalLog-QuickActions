using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using BepInEx.Logging;
using QuickActions.Core;

namespace QuickActions.Adapters;

/// <summary>
/// Writes to the BepInEx log and to an append-only file that survives restarts
/// (LogOutput.log and Player.log are both overwritten on the next launch). The file append runs on a pool thread, in
/// the order the lines came, so a line written from the game's frame never waits on the disk; the BepInEx log is
/// written at once, on the caller's thread.
/// Never throws: it is called from inside Load and the game's hooks, where an escaping exception would reach the game.
/// </summary>
internal sealed class QuickActionsLog
{
    private const long TrimAboveBytes = 1024 * 1024;
    private const int KeepBytes = 200 * 1024;

    private readonly ManualLogSource _bepinex;
    private readonly string _path;
    private readonly object _gate = new();
    private readonly ConcurrentQueue<string> _pending = new();
    private readonly WaitCallback _appendPending;
    private int _isAppending;

    public QuickActionsLog(ManualLogSource bepinex, string path)
    {
        _bepinex = bepinex;
        _path = path;
        _appendPending = AppendPending;
    }

    public void Write(string message)
    {
        try
        {
            if (HookFailure.Is(message)) _bepinex.LogError(message);
            else _bepinex.LogInfo(message);
        }
        catch (Exception)
        {
            // The file copy below still has a chance.
        }

        _pending.Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");

        if (Interlocked.CompareExchange(ref _isAppending, 1, 0) == 0) ThreadPool.UnsafeQueueUserWorkItem(_appendPending, null);
    }

    /// <summary>Past 1 MB only the newest ~200 KB of whole lines is kept.</summary>
    public void TrimIfLarge()
    {
        lock (_gate)
        {
            try
            {
                var file = new FileInfo(_path);

                if (!file.Exists || file.Length <= TrimAboveBytes) return;

                var bytes = File.ReadAllBytes(_path);
                var cut = bytes.Length - KeepBytes;
                var nextLine = Array.IndexOf(bytes, (byte)'\n', cut) + 1;

                File.WriteAllBytes(_path, bytes[(nextLine > 0 ? nextLine : cut)..]);
            }
            catch (Exception)
            {
                // An oversized log is harmless; failing to trim it must not stop the plugin loading.
            }
        }
    }

    /// <summary>
    /// One pool thread at a time empties the queue. A line queued just as it finishes finds the flag cleared and starts
    /// the next one, or is picked up by the recheck at the end of this one.
    /// </summary>
    private void AppendPending(object? _)
    {
        do
        {
            lock (_gate) AppendQueuedLines();

            Volatile.Write(ref _isAppending, 0);
        }
        while (!_pending.IsEmpty && Interlocked.CompareExchange(ref _isAppending, 1, 0) == 0);
    }

    private void AppendQueuedLines()
    {
        try
        {
            using var file = new StreamWriter(new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));

            while (_pending.TryDequeue(out var line)) file.Write(line);
        }
        catch (Exception)
        {
            // A locked or read-only file costs the file copy, never the plugin. The lines still queued are dropped, as
            // a line that failed to append always was, and the next line tries the file again.
            while (_pending.TryDequeue(out _))
            {
            }
        }
    }
}
