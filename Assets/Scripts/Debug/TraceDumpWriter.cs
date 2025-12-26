using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class TraceDumpWriter
{
    public static void Dump(List<MoveTraceEntry> trace, string reason)
    {
        try
        {
            var sb = new StringBuilder();

            sb.AppendLine("==== PARCHIS TRACE DUMP ====");
            sb.AppendLine($"Time: {DateTime.Now}");
            sb.AppendLine($"Reason: {reason}");
            sb.AppendLine();

            sb.AppendLine("---- TRACE ----");
            foreach (var e in trace)
            {
                sb.AppendLine($"Move {e.phase} | Player={e.player} | Steps={e.stepsToMove} | {e.fromTile}->{e.toTile} | {e.tileType} | {e.note}");
            }

            sb.AppendLine();
            sb.AppendLine("---- END TRACE ----");
            sb.AppendLine();

            WriteToFile(sb.ToString());
        } catch (Exception e)
        {
            Debug.LogError("Failed to dump trace " + e);
        }
    }

    private static void WriteToFile(string content)
    {
        string folder = Path.Combine(Application.persistentDataPath, "trace_dump");
        Directory.CreateDirectory(folder);

        string file = Path.Combine(folder, $"trace_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        File.WriteAllText(file, content);

        Debug.Log($"Trace dump written to: {file}");
    }
}
