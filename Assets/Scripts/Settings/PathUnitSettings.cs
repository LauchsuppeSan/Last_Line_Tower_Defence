using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathUnitSettings
{
    [SerializeField]
    public float movementSpeed = 200;
    [SerializeField]
    public float rotationSpeed = 200;
    [SerializeField, Tooltip("In Cells")]
    public int attackRange = 3;
}
