global using System;
global using System.Collections.Generic;
global using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using BepInEx.Configuration;
using System.Reflection;
using BepInEx.Logging;
using Mono.Cecil;
using BepInEx.Preloader.Core.Patching;

namespace BepInEx.GUI.Loader;


[PatcherPluginInfo("com.bepinex.gui", "BepInEx GUI", "1.0.0")]
public class EntryPoint : BasePatcher
{
    public static IEnumerable<string> TargetDLLs { get; } = Array.Empty<string>();

    public static ManualLogSource Log { get; private set; }

    public override void Initialize()
    {
        Log = base.Log;
        try
        {
            InitializeInternal();
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to initialize : ({e.GetType()}) {e.Message}{Environment.NewLine}{e}");
        }
    }

    private void InitializeInternal()
    {
        Loader.Config.Init(Paths.ConfigPath);

        var consoleConfig = (ConfigEntry<bool>)typeof(BepInPlugin).Assembly.
            GetType("BepInEx.ConsoleManager", true).
            GetField("ConfigConsoleEnabled",
            BindingFlags.Static | BindingFlags.Public).GetValue(null);

        if (consoleConfig.Value)
        {
            Log.LogInfo(GetLogOutputFilePath());
            Log.LogInfo("BepInEx regular console is enabled, aborting launch.");
        }
        else if (Loader.Config.EnableBepInExGUIConfig.Value)
        {
            FindAndLaunchGUI();
        }
        else
        {
            Log.LogInfo("Custom BepInEx.GUI is disabled in the config, aborting launch.");
        }
    }

    private string FindGUIExecutable()
    {
        foreach (var filePath in Directory.GetFiles(Paths.PatcherPluginPath, "*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(filePath);

            const string GuiFileName = "bepinex_gui";

            // No platform check because proton is used for RoR2 and it handles it perfectly anyway:
            // It makes the Process.Start still goes through proton and makes the bep gui
            // that was compiled for Windows works fine even in linux operating systems.

            if (fileName == $"{GuiFileName}.exe")
            {
                var versInfo = FileVersionInfo.GetVersionInfo(filePath);
                if (versInfo.FileMajorPart == 3)
                {
                    Log.LogInfo($"Found bepinex_gui v3 executable in {filePath}");
                    return filePath;
                }
            }
        }

        return null;
    }

    private void FindAndLaunchGUI()
    {
        Log.LogInfo("Finding and launching GUI");

        var executablePath = FindGUIExecutable();
        if (executablePath != null)
        {
            var freePort = FindFreePort();
            var process = LaunchGUI(executablePath, freePort);
            if (process != null)
            {
                Logger.Listeners.Add(new SendLogToClientSocket(freePort));
                Logger.Listeners.Add(new CloseProcessOnChainloaderDone(process));
            }
            else
            {
                Log.LogInfo("LaunchGUI failed");
            }
        }
        else
        {
            Log.LogInfo("bepinex_gui executable not found.");
        }
    }

    private int FindFreePort()
    {
        int port = 0;
        Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPEndPoint localEP = new(IPAddress.Any, 0);
            socket.Bind(localEP);
            localEP = (IPEndPoint)socket.LocalEndPoint;
            port = localEP.Port;
        }
        finally
        {
            socket.Close();
        }

        return port;
    }

    private Process LaunchGUI(string executablePath, int socketPort)
    {
        var processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = executablePath;
        processStartInfo.WorkingDirectory = Path.GetDirectoryName(executablePath);

        processStartInfo.Arguments =
            $"\"{typeof(Paths).Assembly.GetName().Version}\" " +
            $"\"{Paths.ProcessName}\" " +
            $"\"{Paths.GameRootPath}\" " +
            $"\"{GetLogOutputFilePath()}\" " +
            $"\"{Loader.Config.ConfigFilePath}\" " +
            $"\"{Process.GetCurrentProcess().Id}\" " +
            $"\"{socketPort}\"";

        return Process.Start(processStartInfo);
    }

    // diskLogListener.FullFilePath was removed in BepInEx 6
    private string GetLogOutputFilePath()
    {
        return "GAME_PATH\\BepInEx\\LogOutput.log";
    }
}
