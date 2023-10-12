using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicUnit : MonoBehaviour
{
    protected abstract void AddSelfToObjectCollection();

    protected abstract void RemoveSelfFromObjectCollection();

    protected abstract void OnDestroy();
}
