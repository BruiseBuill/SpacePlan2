using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BF;

public class Grid
{
    public Vector2Int intPos;
    public List<MainShipCP> ships = new List<MainShipCP>();
}
public class ViewGridMap
{
    public int[,] viewWeight;
    public void Initialize(int x,int y)
    {
        viewWeight = new int[x, y];
    }
}
public class MapManager : Single<MapManager>
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float mapHight;
    [SerializeField] float mapWidth;
    //
    [SerializeField] float gridWidth;
    [SerializeField] int gridSumX;
    [SerializeField] int gridSumY;
    //由格点到物体
    Grid[,] grids;
    //由物体到格点
    Dictionary<int, Vector2Int> dictionary = new Dictionary<int, Vector2Int>();
    //
    [SerializeField] ViewGridMap[] viewMaps;
    //
    Vector3 aimPos;
    [SerializeField] float stepDistance;
    [SerializeField] float randomDistance;
    //
    Vector3 startPos;
    Vector3 randomOffset;
    //
    List<PlantBDCP> plantBDCPs = new List<PlantBDCP>(); 

    protected void Awake()
    {
        Initialize();
    }
    void Initialize()
    {
        mapWidth = spriteRenderer.sprite.bounds.size.x;
        mapHight = spriteRenderer.sprite.bounds.size.y;

        gridSumX = Mathf.CeilToInt(mapWidth * 0.5f / gridWidth) * 2;
        gridSumY = Mathf.CeilToInt(mapHight * 0.5f / gridWidth) * 2;
        grids = new Grid[gridSumX, gridSumY];
    }
    public void Register(Vector3 pos, int instanceID)
    {
        dictionary.Add(instanceID, PosToGridPos(pos));
    }
    public void RedoRegister(int instanceID)
    {
        dictionary.Remove(instanceID);
    }
    public void RefreshPos(Vector3 pos,int instanceID)
    {
        Vector2Int a, b;
        a = dictionary[instanceID];
        b = PosToGridPos(pos);
        if (a.x != b.x || a.y != b.y)
        {
            dictionary[instanceID] = b;
        }
    }
    Vector2Int PosToGridPos(Vector3 pos)
    {
        Vector2Int a = Vector2Int.zero;
        a.x = Mathf.FloorToInt(pos.x / gridWidth) + gridSumX / 2;
        a.y = Mathf.FloorToInt(pos.y / gridWidth) + gridSumY / 2;
        return a;
    }
    /*
    public Vector3 ReturnMoveTarget()
    {
        return new Vector3(Random.Range(-mapWidth * 0.5f, mapWidth * 0.5f), Random.Range(-mapHight * 0.5f, mapHight * 0.5f),0);
    }*/
    public List<Vector3> ReturnMoveTarget(Vector3 pos,string league)
    {
        int i;
        if (plantBDCPs.Count == 0)
        {
            var a = FindObjectsOfType<PlantBDCP>();
            for ( i= 0; i < a.Length; i++)
            {
                if (a[i].enabled)
                    plantBDCPs.Add(a[i]);
            }
        }
        i = Random.Range(0, plantBDCPs.Count);
        while (plantBDCPs[i].CompareTag(league))
        {
            i++;
            i %= plantBDCPs.Count;
        }
        aimPos = plantBDCPs[i].ReturnRandomBuildingPos();
        List<Vector3> pathPointList = new List<Vector3>();
        startPos = pos;
        while((startPos - aimPos).sqrMagnitude > stepDistance * stepDistance)
        {
            randomOffset.Set(Random.Range(-randomDistance, randomDistance), Random.Range(-randomDistance, randomDistance), 0);
            startPos += (aimPos - startPos).normalized * stepDistance + randomOffset;
            pathPointList.Add(startPos);
        }
        pathPointList.Add(aimPos);
        return pathPointList;
    }

}
