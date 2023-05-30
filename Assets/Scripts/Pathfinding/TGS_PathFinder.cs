using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;

public class TGS_PathFinder : MonoBehaviour
{
    public TerrainGridSystem tgs;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartPathfindingAtFrameEnd());
    }

    private IEnumerator StartPathfindingAtFrameEnd()
    {
        yield return new WaitForEndOfFrame();
        tgs.pathFindingUseDiagonals = false;
        List<int> path = tgs.FindPath(15, 18602);
        Debug.Log(path.Count);

        foreach (int cellIndex in path)
        {
            tgs.CellSetColor(cellIndex, Color.magenta);
        }
    }
}
