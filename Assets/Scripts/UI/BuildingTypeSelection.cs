using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTypeSelection : MonoBehaviour
{
    private static bool isVisible = false;
    private static bool hasActivTween = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            Vector2 tweenDirection =
                (isVisible, hasActivTween) switch
                {
                    (false, false) => Vector2.up,
                    (true, false) => Vector2.down,
                    _ => Vector2.zero
                };

            if(tweenDirection == Vector2.zero)
            { return; }

            ToggleActivTweenStatus();

            UI_Utils
                .ScrollInDirection(this.gameObject.GetComponent<RectTransform>(), tweenDirection, 100, 0.5f)
                .setOnComplete(ToggleVisibilityAndTweenStatus);
        }
    }

    private static void ToggleVisibilityStatus()
    {
        isVisible = !isVisible;
    }

    private static void ToggleActivTweenStatus()
    {
        hasActivTween = !hasActivTween;
    }

    private static void ToggleVisibilityAndTweenStatus()
    {
        ToggleVisibilityStatus();
        ToggleActivTweenStatus();
    }
}
