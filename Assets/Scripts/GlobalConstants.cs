using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalConstants", menuName = "Parchis/GlobalConstants")]
public class GlobalConstants : ScriptableObject
{
    [SerializeField] public Constant[] constantsByLocale;
    
    public string GetPrivacyPolicyURL()
    {
        // only 1 language is available so far
        return constantsByLocale[0].privacyPolicyUrl;
    }

    public string GetGameRulesURL()
    {
        // only 1 language is available so far
        return constantsByLocale[0].spanishGameRulesUrl;
    }

    [Serializable]
    public struct Constant
    {
        [SerializeField] public string locale;
        [SerializeField] public string privacyPolicyUrl;
        [SerializeField] public string spanishGameRulesUrl;
    }
}
