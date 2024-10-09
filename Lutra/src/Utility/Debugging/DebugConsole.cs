using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ImGuiNET;
using Lutra.Input;

namespace Lutra.Utility.Debugging;

public class DebugConsole : IDisposable, IAsyncDisposable
{
    public static DebugConsole Instance;
    public static Key SummonKey = Key.Grave;
    public static Key PopSceneKey = Key.Escape;

    public bool IsOpen = false;
    private readonly List<ConsoleLogEntry> Entries = [];
    private readonly Dictionary<string, MethodInfo> Commands = [];
    private readonly Dictionary<Type, object> TypeInstances = [];
    private readonly HashSet<string> EnabledGroups = [];
    private readonly List<string> CommandBuffer = [];

    private string CurrentInput = "";
    private bool ScrollToBottom = false;
    private bool FileLoggingEnabled = false;
    private FileStream LogFileHandle;

    private const BindingFlags BINDING_FLAGS = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
    private static readonly List<string> _whitelist = ["lutra"];

    private DebugConsole()
    {
    }

    public static void WhitelistAssemblyForDebugCommands(string contains)
    {
        _whitelist.Add(contains.ToLower());
    }

    // TODO: Document the fact that NativeAot will disable auto-registration of `DebugCommand`s!
    [Conditional("DEBUG")]
    [UnconditionalSuppressMessage("Aot", "IL2026", Justification = "The dynamic code is not reachable with AOT")]
    public static void Initialize()
    {
        Instance = new();

        if (RuntimeFeature.IsDynamicCodeSupported)
        {
            Instance.RegisterCommands();
        }
    }

    [Conditional("DEBUG")]
    public static void EnableLogToFile(string filePath)
    {
        Instance.FileLoggingEnabled = true;
        Instance.LogFileHandle = File.OpenWrite(filePath);
    }

    [Conditional("DEBUG")]
    public static void HandleInput()
    {
        if (InputManager.KeyPressed(SummonKey))
        {
            Instance.IsOpen = !Instance.IsOpen;
        }

        if (InputManager.KeyPressed(PopSceneKey))
        {
            if (Game.Instance.SceneSystem.Stack.Count > 1)
            {
                Game.Instance.RemoveScene();
            }
            else
            {
                Game.Instance.GameLoop.Exit();
            }
        }
    }

    #region ImGui

    [Conditional("DEBUG")]
    public static unsafe void RenderDebugConsoleIMGUI()
    {
        if (Instance.IsOpen)
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500.0f, 500.0f), ImGuiCond.FirstUseEver);
            if (!ImGui.Begin("Debug Console", ref Instance.IsOpen))
            {
                // Dismiss/Close, so do buffered inputs
                Instance.ExecuteCommands();
                ImGui.End();
                return;
            }

            if (ImGui.BeginChild("ScrollingRegion", new System.Numerics.Vector2(0.0f, -40.0f), 0, ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(4, 1)); // Tightens spacing.
                foreach (var entry in Instance.Entries)
                {
                    var sanitisedString = entry.LogText.Replace("%", "%%"); // TODO: Perform other sanitisation checks here for printf() format.
                    ImGui.TextColored(ImGuiHelper.ImColorFromColor(entry.Color).Value, $"[{entry.Timestamp:G}] {sanitisedString}");
                }

                if (Instance.ScrollToBottom)
                {
                    ImGui.SetScrollHereY(1.0f);
                    Instance.ScrollToBottom = false;
                }

                ImGui.PopStyleVar();
                ImGui.EndChild();
            }

            ImGui.Separator();

            var inputFlags = ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.CallbackHistory;
            static int callback(ImGuiInputTextCallbackData* data)
            {
                return ImguiConsoleTextCallback(new ImGuiInputTextCallbackDataPtr(data), *data);
            }

            bool reclaimFocus = false;
            if (ImGui.InputText("", ref Instance.CurrentInput, 256, inputFlags, callback))
            {
                Instance.SendCommand(Instance.CurrentInput);
                Instance.CurrentInput = "";
                Instance.ScrollToBottom = true;
                reclaimFocus = true;
            }

            ImGui.SameLine();
            if (ImGui.SmallButton("Enter"))
            {
                Instance.SendCommand(Instance.CurrentInput);
                Instance.CurrentInput = "";
                Instance.ScrollToBottom = true;
            }

            ImGui.SameLine();
            if (ImGui.SmallButton("Save Log"))
            {
                Instance.CmdSave("savedLog.txt");
            }

            ImGui.SameLine();
            if (ImGui.SmallButton("Clear"))
            {
                Instance.CmdClear();
            }

            ImGui.SetItemDefaultFocus();
            if (reclaimFocus)
            {
                ImGui.SetKeyboardFocusHere(-1);
            }

            ImGui.End();
        }
    }

    private static unsafe int ImguiConsoleTextCallback(ImGuiInputTextCallbackDataPtr data, ImGuiInputTextCallbackData data_real)
    {
        switch (data.EventFlag)
        {
            case ImGuiInputTextFlags.CallbackCompletion:
                {
                    // Tab-Completion
                    var currentString = System.Text.Encoding.UTF8.GetString(data_real.Buf, data.BufTextLen);

                    var matches = Instance.GetActiveCommands().Keys.Where(cmd => cmd.StartsWith(currentString)).ToList();
                    if (matches.Count > 0)
                    {
                        // Only one match, so let's just use that.
                        if (matches.Count == 1)
                        {
                            data.DeleteChars(0, data.BufTextLen);
                            data.InsertChars(0, matches[0]);
                        }
                        else
                        {
                            // Multiple matches, so provide hints.
                            string matchString = "";
                            foreach (var match in matches)
                            {
                                matchString += match + ", ";
                            }
                            Instance.LogInfo($"Possible matches: {matchString}");
                        }
                    }
                    else
                    {
                        Instance.LogInfo($"No matches for {currentString}.");
                    }
                }
                break;
        }
        return 0;
    }

    #endregion

    [DebugCommand(alias: "help", help: "Displays help information.")]
    public void CmdHelp()
    {
        LogInfo("");

        var cmds = GetActiveCommands();

        var maxCommandNameLength = cmds.Max(c => c.Key.Length);
        var maxGroupNameLength = cmds.Max(c => GetDebugCommand(c.Key).Group);

        Instance.LogInfo("== Available Commands:");
        var cmdGroups = cmds.GroupBy(kv => GetDebugCommand(kv.Key).Group).OrderBy(kv => kv.Key);

        foreach (var group in cmdGroups)
        {
            if (group.Key != "")
            {
                LogInfo("");
                LogInfo($"= {group.Key}");
            }

            foreach (var cmd in group)
            {
                var attribute = GetDebugCommand(cmd.Value);
                var cmdstr = cmd.Key;
                if (attribute.Help != "")
                {
                    cmdstr = cmdstr.PadRight(maxCommandNameLength + 2, ' ');
                    cmdstr += ": ";
                    cmdstr += attribute.Help;
                }

                LogInfo(cmdstr);
            }
        }

        LogInfo("== End of Help.");
    }

    [DebugCommand(alias: "clear", help: "Clears the console.")]
    void CmdClear()
    {
        Entries.Clear();
        LogQuiet("Cleared.");
    }

    [DebugCommand(alias: "savelog", help: "Saves log to disk.")]
    void CmdSave(string filePath)
    {
        System.IO.File.WriteAllLines(filePath, Entries.Select((entry) => $"[{entry.Timestamp:G}] {entry.LogText}"));
        LogQuiet($"Saved to {filePath}");
    }

    public void Log(string str)
    {
        LogInternal(str, Color.White);
    }

    public void LogInfo(string str)
    {
        LogInternal(str, Color.CornflowerBlue);
    }

    public void LogQuiet(string str)
    {
        LogInternal(str, Color.Grey);
    }

    public void LogWarning(string str)
    {
        LogInternal(str, Color.Yellow);
    }

    public void LogError(string str)
    {
        LogInternal(str, Color.Red);
    }

    private void LogInternal(string str, Color? col = null)
    {
        if (col == null) col = Color.White;

        if (str.Contains('\n'))
        {
            var split = str.ToString().Split('\n');
            foreach (var s in split)
            {
                LogInternal(s, col);
            }
            return;
        }

        Entries.Add(new ConsoleLogEntry
        {
            LogText = str.ToString(),
            Color = col.Value,
            Timestamp = DateTime.Now
        });

        ScrollToBottom = true;

        LogToStdOutput(str);
        LogToFile(str);
    }

    public void LogToFile(string logText)
    {
        if (FileLoggingEnabled && LogFileHandle.CanWrite)
        {
            LogFileHandle.Write(System.Text.Encoding.UTF8.GetBytes(logText + "\n"));
            LogFileHandle.Flush();
        }
    }

    [Conditional("DEBUG")]
    public static void LogToStdOutput(string logText)
    {
        Console.WriteLine(logText);
    }

    // Looks through whitelisted assemblies to find DebugCommand tagged methods.
    [Conditional("DEBUG")]
    [RequiresUnreferencedCode("Uses Reflection.")]
    public void RegisterCommands()
    {
        Commands.Clear();
        TypeInstances.Clear();

        // Always include the entry point assembly by default
        var entryAssembly = Assembly.GetEntryAssembly();
        RegisterCommandsForAssembly(entryAssembly);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly == entryAssembly) continue;
            string name = assembly.GetName().FullName.ToLower();
            foreach (var whitelisted in _whitelist)
            {
                if (name.Contains(whitelisted))
                {
                    RegisterCommandsForAssembly(assembly);
                    break; // Move on to the next assembly after registering
                }
            }
        }
    }

    [RequiresUnreferencedCode("Uses Reflection.")]
    private void RegisterCommandsForAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BINDING_FLAGS))
            {
                if (method.IsDefined(typeof(DebugCommand), false))
                {
                    var key = method.Name.ToLower();
                    var attr = GetDebugCommand(method);
                    if (attr.Alias != "")
                    {
                        key = attr.Alias.ToLower();
                    }

                    if (attr.Group != "")
                    {
                        EnabledGroups.Add(attr.Group);
                    }

                    Commands.Add(key, method);

                    if (!method.IsStatic)
                    {
                        if (method.DeclaringType != typeof(DebugConsole))
                        {
                            if (!TypeInstances.ContainsKey(method.DeclaringType))
                            {
                                TypeInstances.Add(
                                    method.DeclaringType,
                                    Activator.CreateInstance(method.DeclaringType, null)
                                );
                            }
                        }
                    }
                }
            }
        }
    }

    static DebugCommand GetDebugCommand(MethodInfo methodInfo)
    {
        return (DebugCommand)methodInfo.GetCustomAttributes(typeof(DebugCommand), false)[0];
    }

    DebugCommand GetDebugCommand(string commandName)
    {
        return GetDebugCommand(Commands[commandName]);
    }

    Dictionary<string, MethodInfo> GetActiveCommands()
    {
        return Commands.Where(c =>
        {
            var attr = GetDebugCommand(c.Key);
            return EnabledGroups.Contains(attr.Group) || GetDebugCommand(c.Key).Group == "";
        }).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public void EnableCommandGroup(string group)
    {
        EnabledGroups.Add(group);
    }

    public void DisableCommandGroup(string group)
    {
        EnabledGroups.Remove(group);
    }

    public void SendCommand(string str)
    {
        str = str.Trim();
        LogQuiet("> " + str);

        var cmdName = ParseCommandName(str);

        if (GetActiveCommands().ContainsKey(cmdName))
        {
            CommandBuffer.Add(str);
            var attr = GetDebugCommand(cmdName);
            if (!attr.IsBuffered)
            {
                ExecuteCommand();
            }
        }
        else
        {
            LogError($"Command \"{str}\" not found.");
        }
    }

    private static string ParseCommandName(string str)
    {
        if (str.Contains(' '))
        {
            return str.Split(' ')[0].ToLower();
        }
        return str.ToLower();
    }

    void ExecuteCommands()
    {
        while (CommandBuffer.Count > 0)
        {
            ExecuteCommand(0);
        }
    }

    void ExecuteCommand(int index = -1)
    {
        if (index == -1) index = CommandBuffer.Count - 1;

        string cmd = CommandBuffer[index];
        //parse the string, when inside a quote replace space with something else
        bool inQuote = false;
        string parsedCmd = "";
        for (int i = 0; i < cmd.Length; i++)
        {
            char nextChar = cmd[i];
            if (cmd[i] == '"')
            {
                if (i > 0)
                {
                    if (cmd[i - 1] != '\\')
                    {
                        inQuote = !inQuote;
                    }
                }

            }

            if (inQuote)
            {
                if (cmd[i] == ' ')
                {
                    nextChar = (char)16;
                }
            }
            parsedCmd += nextChar;
        }

        string[] split = parsedCmd.Split(' ');

        string methodName = split[0].ToLower();
        string[] parameters = new string[split.Length - 1];

        //restore spaces
        for (int i = 1; i < split.Length; i++)
        {
            split[i] = split[i].Replace((char)16, ' ');
            if (split[i][0] == '"')
            {
                //get rid of quotes in string arguments
                split[i] = split[i].Replace("\\\"", "#z[");
                split[i] = split[i].Replace("\"", "");
                split[i] = split[i].Replace("#z[", "\"");
            }
            parameters[i - 1] = split[i];
        }

        bool usageMode = false;

        if (parameters.Length == 0)
        {
            if (Commands[methodName].GetParameters().Length > 0)
            {
                usageMode = true;
            }
        }

        if (Commands[methodName].GetParameters().Length != parameters.Length)
        {
            if (!usageMode)
            {
                LogError("Invalid amount of parameters.");
            }
        }

        if (Commands.TryGetValue(methodName, out MethodInfo methodInfo))
        {
            if (usageMode)
            {
                ShowUsage(methodName);
            }
            else if (methodInfo.GetParameters().Length == parameters.Length)
            {
                try
                {
                    Invoke(methodName, parameters);
                }
                catch (Exception ex)
                {
                    LogError(ex.Message);
                }
            }
        }

        CommandBuffer.RemoveAt(index);
    }

    private void Invoke(string methodName, string[] parameters)
    {
        var mi = Commands[methodName];

        object instance = null;
        if (!mi.IsStatic)
        {
            if (mi.DeclaringType == typeof(DebugConsole))
            {
                instance = this;
            }
            else
            {
                instance = TypeInstances[mi.DeclaringType];
            }
        }
        try
        {
            Commands[methodName].Invoke(instance, ParseParameters(Commands[methodName], parameters));
        }
        catch (Exception ex)
        {
            throw ex.InnerException;
        }

    }

    static object[] ParseParameters(MethodInfo methodInfo, string[] paramStrings)
    {
        var parsedParams = new object[paramStrings.Length];
        var paramsInfo = methodInfo.GetParameters();

        if (paramsInfo.Length != paramStrings.Length)
        {
            throw new ArgumentException("Invalid amount of parameters.");
        }

        var i = 0;
        foreach (var param in paramsInfo)
        {
            var ptype = param.ParameterType;
            if (ptype == typeof(float))
            {
                if (!float.TryParse(paramStrings[i].TrimEnd('f'), out float value))
                {
                    throw new ArgumentException(string.Format("Error parsing float for parameter {0}", i));
                }
                else
                {
                    parsedParams[i] = value;
                }
            }

            if (ptype == typeof(int))
            {
                if (!int.TryParse(paramStrings[i], out int value))
                {
                    throw new ArgumentException(string.Format("Error parsing int for parameter {0}", i));
                }
                else
                {
                    parsedParams[i] = value;
                }
            }

            if (ptype == typeof(bool))
            {
                if (!bool.TryParse(paramStrings[i], out bool value))
                {
                    throw new ArgumentException(string.Format("Error parsing bool for parameter {0}", i));
                }
                else
                {
                    parsedParams[i] = value;
                }
            }

            if (ptype == typeof(string))
            {
                parsedParams[i] = paramStrings[i];
            }
            i++;
        }

        return parsedParams;
    }

    private void ShowUsage(string methodName)
    {
        LogInfo(string.Format("== Command Usage: {0}", methodName));

        var helpStr = "";
        foreach (var param in Commands[methodName].GetParameters())
        {
            var ptype = ParameterTypeToString(param);
            helpStr += string.Format("({0}) {1}, ", ptype, param.Name);
        }
        helpStr = helpStr.TrimEnd(',', ' ');

        LogInfo(helpStr);

        var usageStr = GetDebugCommand(methodName).Usage;
        if (usageStr != "")
        {
            LogInfo("");
            LogInfo(usageStr);
        }

        LogInfo("");

        LogInfo(string.Format("== End of Usage Details", methodName));
    }

    private static string ParameterTypeToString(ParameterInfo param)
    {
        if (param.ParameterType == typeof(int))
        {
            return "int";
        }
        if (param.ParameterType == typeof(string))
        {
            return "string";
        }
        if (param.ParameterType == typeof(float))
        {
            return "float";
        }
        if (param.ParameterType == typeof(bool))
        {
            return "bool";
        }
        return "";
    }

    public void Dispose()
    {
        LogFileHandle?.Close();
        LogFileHandle?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (LogFileHandle != null) await LogFileHandle.DisposeAsync();
    }
}
