using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AddSelfToTowerCollection();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        { Destroy(gameObject); }
    }

    private void OnDestroy()
    {
        RemoveSelfFromTowerCollection();
    }

    private void AddSelfToTowerCollection()
    {
        ObjectCollections.Towers.Add(this.gameObject);
    }

    private void RemoveSelfFromTowerCollection()
    {
        ObjectCollections.Towers.Remove(gameObject);
    }
}
