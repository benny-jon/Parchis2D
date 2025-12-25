using UnityEngine;

[CreateAssetMenu(fileName = "SpriteLibrary", menuName = "Parchis/SpriteLibrary")]
public class SpriteLibrary : ScriptableObject
{
    [SerializeField] public Sprite[] diceFaces;

    public Sprite GetDiceFace(int diceValue)
    {
        if (diceValue < 1 || diceValue > 6) return null;
        return diceFaces[diceValue - 1];
    }
}
