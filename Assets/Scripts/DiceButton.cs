using UnityEngine;

public class DiceButton : Clickable2D
{
    public GameManager gameManager;

    public override void OnClickUp()
    {
        gameManager.OnDiceRollButton();
    }
}
