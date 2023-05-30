using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiElementPositionSwitch : MonoBehaviour
{
    [SerializeField] RectTransform uiElementToSwitch;
    [SerializeField] RectTransform uiElementToSwitchWith;

    public void TriggerUIPositionSwitch()
    {
        // Hier muss noch: Wenn einer der beiden grade in einem Tween ist. dann soll erstmal gewartet werden bis der zuende ist
        UI_Utils.SwitchPositions(uiElementToSwitchWith, uiElementToSwitch, 0.5f);
    }
}
