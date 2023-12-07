using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEditor.EventSystems;
using UnityEngine.EventSystems;

public static class UI_Utils
{
    public static LTDescr ScrollInDirection(RectTransform objectToMove, Vector2 directionNormal, float distance, float timeInSeconds)
    {
        Vector2 destination = objectToMove.anchoredPosition + (directionNormal * distance);

        LeanTween.moveX(objectToMove, destination.x, timeInSeconds);
        return LeanTween.moveY(objectToMove, destination.y, timeInSeconds);
    }

    public static LTDescr SwitchPositions(RectTransform objectToSwitchWith, RectTransform objectToSwitch, float timeInSeconds)
    {
        Vector2 posA = objectToSwitch.anchoredPosition;
        Vector2 posB = objectToSwitchWith.anchoredPosition;

        LeanTween.move(objectToSwitch, posB, timeInSeconds);
        return LeanTween.move(objectToSwitchWith, posA, timeInSeconds);
    }
}
