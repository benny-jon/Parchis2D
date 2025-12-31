using UnityEngine;

[CreateAssetMenu(fileName = "SpriteLibrary", menuName = "Parchis/SpriteLibrary")]
public class SpriteLibrary : ScriptableObject
{
    [SerializeField] public Sprite[] diceFaces;

    [Header("Colors")]
    [SerializeField] public Color[] playersColorPrimary;
    [SerializeField] public Color[] playersColorSecondary;

    [Header("Medals Sprite")]
    [SerializeField] private Sprite gold;
    [SerializeField] private Sprite silver;
    [SerializeField] private Sprite bronze;

    [Header("Moves Highlights")]
    [SerializeField] public Sprite normalHighlightBackground;
    [SerializeField] public Sprite leftCornerHighlightBackground;
    [SerializeField] public Sprite rightCornerHighlightBackground;
    [SerializeField] public Sprite homeHighlightBackground;

    public Sprite GetDiceFace(int diceValue)
    {
        if (diceValue < 1 || diceValue > 6) return null;
        return diceFaces[diceValue - 1];
    }

    public Sprite GetMedalSprite(Medal medal)
    {
        switch (medal)
        {
            case Medal.Gold: return gold;
            case Medal.Silver: return silver;
            case Medal.Bronze: return bronze;
            default: return null;
        }
    }
}
