using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(fileName = "GlobalConstants", menuName = "Parchis/GlobalConstants")]
public class GlobalConstants : ScriptableObject
{
    [SerializeField] public Constant[] constantsByLocale;
    
    public string GetPrivacyPolicyURL()
    {
        return GetActiveConstants().privacyPolicyUrl;
    }

    public string GetGameRulesURL()
    {
        return GetActiveConstants().spanishGameRulesUrl;
    }

    private Constant GetActiveConstants()
    {
        var localeCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        Debug.Log($"Current Locale: {localeCode}");
        foreach (var constants in constantsByLocale)
        {
            if (constants.locale == localeCode)
            {
                return constants;
            }
        }

        return constantsByLocale[0];
    }

    [Serializable]
    public struct Constant
    {
        [SerializeField] public string locale;
        [SerializeField] public string privacyPolicyUrl;
        [SerializeField] public string spanishGameRulesUrl;
    }
}
