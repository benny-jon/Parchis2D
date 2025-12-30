using UnityEngine;

[CreateAssetMenu(fileName = "SpriteLibrary", menuName = "Parchis/SpriteLibrary")]
public class SpriteLibrary : ScriptableObject
{
    [SerializeField] public Sprite[] diceFaces;
    [SerializeField] public Color[] playersColorPrimary;
    [SerializeField] public Color[] playersColorSecondary;
    [SerializeField] public Sprite normalHighlightBackground;
    [SerializeField] public Sprite leftCornerHighlightBackground;

    [SerializeField] public Sprite rightCornerHighlightBackground;
    [SerializeField] public Sprite homeHighlightBackground;

    public Sprite GetDiceFace(int diceValue)
    {
        if (diceValue < 1 || diceValue > 6) return null;
        return diceFaces[diceValue - 1];
    }
}
