using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LLTDExeptions
{
    public class ObjectCollectionException: System.Exception
    {
        public ObjectCollectionException(string msg) : base($"{msg}")
        {

        }
    }
}
