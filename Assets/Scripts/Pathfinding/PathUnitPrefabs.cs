using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PathUnitPrefabs : MonoBehaviour
{
    // Instance of PathUnitPrefabs (variable shared by all instances)
    [HideInInspector]
    public static PathUnitPrefabs instance { get; private set; }

    [SerializeField]
    public GameObject mediumTankPathUnitPrefab;

    private void Awake()
    {
        // Prevents destruction at scene loading
        DontDestroyOnLoad(this.gameObject);

        // If there is already a value in instance that is not this instance destroy this instance
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public GameObject GetMediumTankPrefab()
    {
        return mediumTankPathUnitPrefab;
    }
}
