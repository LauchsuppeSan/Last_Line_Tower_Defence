using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank: BasicUnit
{
    // Start is called before the first frame update
    void Start()
    {
        AddSelfToObjectCollection();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Add this game object as activ tower to the tower collection
    /// </summary>
    /// <exception cref="LLTDExeptions.ObjectCollectionException">"Is already in collection"</exception>
    protected override void AddSelfToObjectCollection()
    {
        if(ObjectCollections.Towers.Contains(this.gameObject))
        { throw new LLTDExeptions.ObjectCollectionException($"Tower collection already contains {this.gameObject}"); }
        else
        { ObjectCollections.Towers.Add(this.gameObject); }
    }

    /// <summary>
    /// Remove this game object from activ tower collection
    /// </summary>
    /// <exception cref="LLTDExeptions.ObjectCollectionException">Tower collection does not contain this game object</exception>
    protected override void RemoveSelfFromObjectCollection()
    {
        if (!ObjectCollections.Towers.Contains(this.gameObject))
        { throw new LLTDExeptions.ObjectCollectionException($"Tower collection does not contain {this.gameObject} to remove"); }
        else
        { ObjectCollections.Towers.Remove(this.gameObject); }
    }

    /// <summary>
    /// Called if this game object will be destroyed
    /// </summary>
    protected override void OnDestroy()
    {
        RemoveSelfFromObjectCollection();
    }
}
