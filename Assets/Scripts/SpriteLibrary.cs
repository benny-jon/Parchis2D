using UnityEngine;

[CreateAssetMenu(fileName = "SpriteLibrary", menuName = "Parchis/SpriteLibrary")]
public class SpriteLibrary : ScriptableObject
{
    [SerializeField] public Sprite[] diceFaces;
    [SerializeField] public Color[] playersColorPrimary;
    [SerializeField] public Color[] playersColorSecondary;

    public Sprite GetDiceFace(int diceValue)
    {
        if (diceValue < 1 || diceValue > 6) return null;
        return diceFaces[diceValue - 1];
    }
}
