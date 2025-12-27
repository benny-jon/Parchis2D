using System;
using System.IO;
using UnityEngine;

public class ReplayWriter : IDisposable
{
    public static readonly int FORMAT_VERSION = 1;

    private readonly StreamWriter _writer;

    public ReplayWriter(string fileName)
    {
        var folder = Path.Combine(Application.persistentDataPath, "replays");
        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, fileName);
        _writer = new StreamWriter(path, append: false) { AutoFlush = true };

        Debug.Log($"Replay file: {path}");
    }

    public void WriteHeader(int seed, string rulesType)
    {
        _writer.WriteLine(JsonUtility.ToJson(new ReplayHeader
        {
            v = FORMAT_VERSION,
            rules = rulesType,
            seed = seed,

        }));
    }

    public void WriteMove(int turnIndex, int playerIndex, int pieceIndex, int moveOptionIndex, int bonusAmount)
    {

        _writer.WriteLine(JsonUtility.ToJson(new MoveReplayEvent
        {
            turn = turnIndex,
            player = playerIndex,
            piece = pieceIndex,
            moveOption = moveOptionIndex,
            bonus = bonusAmount,
        }));
    }

    public void WriteRoll(int turnIndex, int playerIndex, int dice1, int dice2)
    {
        _writer.WriteLine(JsonUtility.ToJson(new RollReplayEvent
        {
            turn = turnIndex,
            player = playerIndex,
            d1 = dice1,
            d2 = dice2,
        }));
    }

    public void Dispose() => _writer?.Dispose();

    public class ReplayHeader
    {
        public int v;
        public string rules;
        public int seed;
    }

    public class MoveReplayEvent
    {
        public string e = "move";
        public int turn;
        public int player;
        public int piece;
        public int moveOption;
        public int bonus;
    }

    public class RollReplayEvent
    {
        public string e = "roll";
        public int turn;
        public int player;
        public int d1;
        public int d2;
    }
}
