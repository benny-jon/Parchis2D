using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerDicePanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image dice1;
    [SerializeField] private Image dice2;
    [SerializeField] private GameObject scrim;
    [SerializeField] private GameObject timeToRollIndicator;

    [SerializeField] private SpriteLibrary spriteLibrary;

    public Action OnDiceClicked;

    private bool isActive = false;

    public void SetDice(int d1, int d2)
    {
        dice1.sprite = spriteLibrary.GetDiceFace(d1);
        dice2.sprite = spriteLibrary.GetDiceFace(d2);
    }

    public void SetDim(bool dim)
    {
        isActive = !dim;
        scrim.SetActive(dim);
        timeToRollIndicator?.SetActive(isActive);
    }

    public void SetTimeToRoll(bool timeToRoll)
    {
        timeToRollIndicator?.SetActive(timeToRoll);
    }

    // public override void OnClickUp()
    // {
    //     Debug.Log("Panel clicked " + gameObject.name);
    //     if (isActive)
    //     {
    //         OnDiceClicked?.Invoke();
    //     }
    // }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Panel clicked {gameObject.name}, active={isActive}");
        OnDiceClicked?.Invoke();
    }
}
