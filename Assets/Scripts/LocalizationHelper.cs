using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizationHelper
{
    public string getForceStartMessage()
    {
        return GetString("force_start_message");
    }

    public string GetForceBreakBlockadeMessage(int playerIndex)
    {
        return GetString("force_break_blockade_message", GetPlayerName(playerIndex));
    }

    public string GetPenaltyMessage(int playerIndex)
    {
        return GetString("penalty_message", GetPlayerName(playerIndex));
    }

    public string GetGameOverMessage(int playerIndex)
    {
        return GetString("game_over", GetPlayerName(playerIndex));
    }

    public string GetCaptureBonusMessage(int playerIndex)
    {
        return GetString("capture_bonus_message", GetPlayerName(playerIndex));
    }

    public string GetCapturingBonusLostMessage(int playerIndex)
    {
        return GetString("capture_bonus_lost_message", GetPlayerName(playerIndex));
    }

    public string GetReachingHomeBonusMessage(int playerIndex)
    {
        return GetString("reaching_home_bonus_message", GetPlayerName(playerIndex));
    }

    public string GetReachingHomeBonusLostMessage()
    {
        return GetString("reaching_home_bonus_lost_message");
    }

    private string GetPlayerName(int playerIndex)
    {
        return GetString(GetPlayerKey(playerIndex));
    }

    private string GetPlayerKey(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0: return "yellow";
            case 1: return "blue";
            case 2: return "red";
            case 3: return "green";
        }
        return "anonymous";
    }

    private string GetString(string key, params object[] arguments)
    {
        if (arguments != null)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(key, arguments);
        }

        return LocalizationSettings.StringDatabase.GetLocalizedString(key);
    }
}
