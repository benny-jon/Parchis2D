using System;
using System.IO;
using UnityEngine;
using static ReplayWriter;


public interface IReplayGame
{
    void ApplyRoll(int player, int d1, int d2);

    void ApplyMove(int player, int piece, int moveOption);
}
public class ReplayPlayer: IDisposable
{
    public static int REPLAY_FORMAT_VERSION = 1;

    private readonly StreamReader _reader;
    private readonly IReplayGame _game;

    public ReplayHeader Header { get; private set; }
    public int EventsApplied { get; private set; }

    public ReplayPlayer(string filePath, IReplayGame game)
    {
        _reader = new StreamReader(filePath);
        _game = game;

        Header = ReadHeaderOrThrow();
    }

    private ReplayHeader ReadHeaderOrThrow()
    {
        string line;
        do
        {
            line = _reader.ReadLine();
            if (line == null) throw new Exception("Replay file is empty (missing header)");
        } while (string.IsNullOrWhiteSpace(line));

        var header = JsonUtility.FromJson<ReplayHeader>(line);
        if (header == null || header.v > REPLAY_FORMAT_VERSION)
        {
            throw new Exception("Your ReplayPlayer is older than the replay format version you are trying to load");
        }

        return header;
    }

    /// <summary>
    /// Applies the next replay event.
    /// </summary>
    /// <returns>Returns false when EOF</returns>
    public bool Step()
    {
        string line;
        while(true)
        {
            line = _reader.ReadLine();
            if (line == null) return false; // EOF
            if (!string.IsNullOrWhiteSpace(line)) break;
        }

        if (line.Contains("move"))
        {
            var replayEvent = JsonUtility.FromJson<MoveReplayEvent>(line);
            ApplyEvent(replayEvent);
            EventsApplied++;
            return true;
        }
        else if (line.Contains("roll"))
        {
            var replayEvent = JsonUtility.FromJson<RollReplayEvent>(line);
            ApplyEvent(replayEvent);
            EventsApplied++;
            return true;
        }
        
        throw new Exception($"Invalid replay event line: {line}");
    }

    private void ApplyEvent(RollReplayEvent replayEvent)
    {
        if (replayEvent.d1 < 1 || replayEvent.d1 > 6 || replayEvent.d2 < 1 || replayEvent.d2 > 6)
        {
            throw new Exception("Invalid dice values");
        }
        if (replayEvent.player < 0 || replayEvent.player > 3)
        {
            throw new Exception("Invalid player index");
        }

        Debug.Log("Apply " + JsonUtility.ToJson(replayEvent));
        _game.ApplyRoll(replayEvent.player, replayEvent.d1, replayEvent.d2);
    }

    private void ApplyEvent(MoveReplayEvent replayEvent)
    {
        if (replayEvent.player < 0 || replayEvent.player > 3)
        {
            throw new Exception("Invalid player index");
        }   
        if (replayEvent.piece < 0 || replayEvent.piece > 3)
        {
            throw new Exception("Invalid piece index");
        }

        Debug.Log("Apply " + JsonUtility.ToJson(replayEvent));
        _game.ApplyMove(replayEvent.player, replayEvent.piece, replayEvent.moveOption);
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}
