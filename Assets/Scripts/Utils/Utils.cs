using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    #region Vectors

    /// <summary>
    /// Set a given point to the given y value
    /// </summary>
    /// <param name="point"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector3 PojectOnY(this Vector3 point, float y)
    {
        return new Vector3(point.x, y, point.z);
    }

    /// <summary>
    /// Returns the point exactly between point a and b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Vector3 GetMidpoint(Vector3 a, Vector3 b)
    {
        return 0.5f * (a + b);
    }

    /// <summary>
    /// Moves a point along a given vector about a given distance
    /// </summary>
    /// <param name="position">start point</param>
    /// <param name="distance">distance to start point</param>
    /// <returns></returns>
    public static Vector3 CrawlOverY(Vector3 position, Vector3 direction, float distance)
    {
        return position + (direction.normalized * distance);
    }


    #endregion Vectors

    #region Comparer
    public static bool AlmostEqualOrLess(this float a, float b, float tollerance = 0.01f)
    {
        return Mathf.Abs(a - b) <= tollerance;
    }

    public static bool AlmostEqualOrGreater(this float a, float b, float tollerance = 0.01f)
    {
        return Mathf.Abs(a - b) >= tollerance;
    }

    public static bool AlmostEqual(this float a, float b, float tollerance = 0.01f)
    {
        return
            a.AlmostEqualOrLess(b, tollerance)
            && a.AlmostEqualOrGreater(b, tollerance);
    }

    #endregion Comparer

    #region Usefull algorithms

    /// <summary>
    /// Combines every item in the given collection with each other
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">Items to combine</param>
    /// <param name="combiLength">determines how many elements are in a combination</param>
    /// <param name="isFirstIteration">Ignore this</param>
    /// <returns>A list with all possible item combinations, where each combination consists of the determined number of elements</returns>
    public static List<List<T>> GetAllCombinations<T>(List<T> collectionToCombine, int combiLength, bool isFirstIteration = true)
    {
        List<T> list = new List<T>(collectionToCombine);
        if (list.Count == 0) { return new List<List<T>>(); }

        T erstes = list[0]; // b
        List<T> erstesRaus = list;
        erstesRaus.RemoveAt(0); // Leere Liste

        List<List<T>> combinationenOhneErstes = new List<List<T>>();

        if (erstesRaus.Count != 0)
        {
            combinationenOhneErstes = GetAllCombinations(erstesRaus, combiLength, false);  // eine list mit 2 listen, die eine hat nur b die andere nur ba
        }
        else
        {
            List<List<T>> toReturn = new List<List<T>>();
            List<T> nurErstes = new List<T>();
            nurErstes.Add(erstes);
            toReturn.Add(nurErstes);
            return toReturn;
        }

        List<List<T>> combinationenJeweilsMitGeaddetemErsten = new List<List<T>>();

        foreach (List<T> eineCombiOhneErstes in combinationenOhneErstes)
        {
            List<T> kopieZumAddenVonErstes = new List<T>(eineCombiOhneErstes);
            kopieZumAddenVonErstes.Add(erstes);
            combinationenJeweilsMitGeaddetemErsten.Add(kopieZumAddenVonErstes);
        }

        List<List<T>> mergeBeideListen = new List<List<T>>();
        mergeBeideListen.AddRange(combinationenOhneErstes);
        mergeBeideListen.AddRange(combinationenJeweilsMitGeaddetemErsten);

        List<T> nurErstes2 = new List<T>();
        nurErstes2.Add(erstes);
        mergeBeideListen.Add(nurErstes2);

        if (isFirstIteration)
        {
            List<List<T>> toReturn = new List<List<T>>();
            for (int combiIndex = 0; combiIndex < mergeBeideListen.Count; combiIndex++)
            {
                if (mergeBeideListen[combiIndex].Count == combiLength)
                {
                    toReturn.Add(mergeBeideListen[combiIndex]);
                }
            }

            return toReturn;
        }

        return mergeBeideListen;
    }

    #endregion

    #region EmptyObjects

    /// <summary>
    /// Creates a new empty GameObject with the components in the list
    /// </summary>
    /// <param name="components">Components to add to the new empty GameObject</param>
    /// <returns>Empty GameObject with the given components</returns>
    public static GameObject SpawnEmptyObjectWithComponents<T>(string name, List<T> components) where T : Component
    {
        GameObject emptyGameObject = new GameObject(name);

        try
        {
            // Faster than foreach
            for (int x = 0; x < components.Count; x++)
            {
                emptyGameObject.AddComponent(components[x].GetType());
            }
        }
        catch (System.Exception e)
        { Debug.LogError($"Cant add component to new empty object: {e.Message}"); }

        return emptyGameObject;
    }

    /// <summary>
    /// Creates a new empty GameObject with the given component
    /// </summary>
    /// <param name="component">Component to add to the new empty GameObject</param>
    /// <returns>Empty GameObject with the given component</returns>
    public static GameObject SpawnEmptyObjectWithComponents<T>(string name, T component) where T : Component
    {
        List<T> componentAsList = new List<T>() { component };
        return SpawnEmptyObjectWithComponents(name, componentAsList);
    }

    #endregion EmptyObjekts

    #region ChildObjects

    /// <summary>
    /// Returns a list with each component of the in the list parameter given component from all childs, cild of childs etc. from the given parent
    /// </summary>
    /// <typeparam name="T">Compoenent to collect</typeparam>
    /// <param name="parent">Highest parent</param>
    /// <param name="foundComponents">List to define the component typ that should be collected</param>
    /// <returns></returns>
    public static List<T> GetComonentFromAllChilds<T>(Transform parent, List<T> foundComponents) where T : Component
    {
        // Check if the parent from the initial GetAllChildsRenderer call need so be in this list
        if (foundComponents.Count == 0)
        {
            if (parent.gameObject.GetComponent<T>() != null)
            {
                foundComponents.Add(parent.gameObject.GetComponent<T>());
            }
        }

        for (int childIndex = 0; childIndex < parent.childCount; childIndex++)
        {
            Transform child = parent.GetChild(childIndex);

            if (child.gameObject.GetComponent<T>() != null)
            {
                foundComponents.Add(child.gameObject.GetComponent<T>());
            }

            GetComonentFromAllChilds(child, foundComponents);
        }

        return foundComponents;
    }

    #endregion

    #region Helper

    public static void SmoothLookAt(Vector3 selfPos, Vector3 targetPos, Transform rotationPoint, float rotationSpeed)
    {
        Vector3 dir = targetPos - selfPos;
        if (dir == Vector3.zero) { return; }
        Quaternion lookRota = Quaternion.LookRotation(dir);
        Vector3 rota = Quaternion.Lerp(rotationPoint.rotation, lookRota, Time.deltaTime * rotationSpeed).eulerAngles;
        rotationPoint.rotation = Quaternion.Euler(0f, rota.y, 0f);
    }

    public static void SmoothLookAt(Transform self, Transform target, Transform rotationPoint, float rotationSpeed)
    {
        SmoothLookAt(self.position, target.position, rotationPoint, rotationSpeed);
    }

    public static void SmoothLookAt(Transform self, Vector3 targetPos, Transform rotationPoint, float rotationSpeed)
    {
        SmoothLookAt(self.position, targetPos, rotationPoint, rotationSpeed);
    }

    public static void SmoothLookAt(Vector3 selfPos, Transform target, Transform rotationPoint, float rotationSpeed)
    {
        SmoothLookAt(selfPos, target.position, rotationPoint, rotationSpeed);
    }

    #endregion Helper
}
