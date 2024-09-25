using System.Collections.Generic;
using Lutra.Utility.Debugging;
using System.Linq;

namespace Lutra.Utility.Profiling;

public struct ProfileRecord
{
    public double TotalMilliseconds;
    public int CallCountThisFrame;
}

public static class Profiler
{
    public static bool Enabled = false;
    private static Dictionary<string, ProfileRecord> ProfilerMarkerTimingsMilliseconds = new();
    private static Dictionary<string, DateTime> ProfilerMarkerStartTimes = new();
    private static DateTime FrameStart;

    public static void StartProfilingMarker(string marker)
    {
        if (!Enabled) return;
        if (ProfilerMarkerStartTimes.ContainsKey(marker))
        {
            Util.LogWarning($"WARNING! StartProfilingMarker() called for marker '{marker}', but marker has already been started! Ignoring...");
            return;
        }
        ProfilerMarkerStartTimes[marker] = System.DateTime.UtcNow;
    }

    public static void EndProfilingMarker(string marker)
    {
        if (!Enabled) return;
        if (!ProfilerMarkerStartTimes.ContainsKey(marker))
        {
            Util.LogWarning($"WARNING! EndProfiling() called for marker '{marker}', but marker has not been started yet!");
            return;
        }
        if (!ProfilerMarkerTimingsMilliseconds.ContainsKey(marker))
        {
            ProfilerMarkerTimingsMilliseconds[marker] = new ProfileRecord
            {
                TotalMilliseconds = (System.DateTime.UtcNow - ProfilerMarkerStartTimes[marker]).TotalMilliseconds,
                CallCountThisFrame = 1
            };
        }
        else
        {
            var record = ProfilerMarkerTimingsMilliseconds[marker];
            record.TotalMilliseconds += (System.DateTime.UtcNow - ProfilerMarkerStartTimes[marker]).TotalMilliseconds;
            record.CallCountThisFrame++;
            ProfilerMarkerTimingsMilliseconds[marker] = record;
        }
        ProfilerMarkerStartTimes.Remove(marker);
    }


    [DebugCommand(alias: "tools.profiler", help: "Toggles recording and display of profiler markers.", group: "tools")]
    public static void ToggleProfiler()
    {
        Enabled = !Enabled;
    }

    public static void StartFrame()
    {
        if (!Enabled) return;
        FrameStart = System.DateTime.UtcNow;
    }

    public static void EndFrame()
    {
        if (!Enabled) return;
        var frameTimeMs = (System.DateTime.UtcNow - FrameStart).TotalMilliseconds;

        // Clean up any dangling markers.
        foreach (var marker in ProfilerMarkerStartTimes.Keys)
        {
            EndProfilingMarker(marker);
        }

        if (ProfilerMarkerTimingsMilliseconds.Count == 0)
        {
            // No profiling data this frame.
            return;
        }

        Util.LogInfo($"[PROFILING] Frame Time: {frameTimeMs}ms");

        foreach (var marker in ProfilerMarkerTimingsMilliseconds)
        {
            var markerPercent = (marker.Value.TotalMilliseconds / frameTimeMs);
            string progressMeter = "[";
            for (int i = 0; i < 30; i++)
            {
                if (i <= markerPercent * 30)
                {
                    progressMeter += "#";
                }
                else
                {
                    progressMeter += "-";
                }
            }
            progressMeter += "]";
            Util.LogInfo($"{progressMeter}|'{marker.Key}': {markerPercent * 100.0f}% - {marker.Value.TotalMilliseconds} ms total, {marker.Value.CallCountThisFrame} calls, {marker.Value.TotalMilliseconds / marker.Value.CallCountThisFrame} avg ms per call.");
        }

        ProfilerMarkerStartTimes.Clear();
        ProfilerMarkerTimingsMilliseconds.Clear();

    }

}
