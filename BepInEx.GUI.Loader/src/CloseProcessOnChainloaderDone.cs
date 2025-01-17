﻿using System.Diagnostics;
using BepInEx.Logging;

namespace BepInEx.GUI.Loader;

public class CloseProcessOnChainloaderDone : ILogListener
{
    private bool _disposed;

    private Process _process;

    public LogLevel LogLevelFilter { get => LogLevel.All; }

    public CloseProcessOnChainloaderDone(Process process) => _process = process;

    public void Dispose()
    {
        _disposed = true;
    }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (_disposed)
        {
            return;
        }

        if (eventArgs.Data.ToString() == "Chainloader startup complete" && eventArgs.Level.Equals(LogLevel.Message))
        {
            if (Config.CloseWindowWhenGameLoadedConfig.Value)
            {
                EntryPoint.Log.LogInfo("Closing BepInEx.GUI");
                KillBepInExGUIProcess();
            }
        }
    }

    private void KillBepInExGUIProcess()
    {
        try
        {
            _process.Kill();
        }
        catch (Exception e)
        {
            EntryPoint.Log.LogError($"Error while trying to kill BepInEx GUI Process: {e}");
        }
        finally
        {
            SendLogToClientSocket.Instance.Dispose();
            Dispose();
        }
    }
}
