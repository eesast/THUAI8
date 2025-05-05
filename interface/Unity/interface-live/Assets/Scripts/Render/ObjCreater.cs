using System;
using System.Collections;
using System.Collections.Generic;
using Protobuf;
using UnityEngine;

[Serializable]
public struct GameObjectList
{
    public List<GameObject> p;
}
public class ObjCreater : SingletonMono<ObjCreater>
{
    public List<GameObjectList> placeList;
    public GameObject[] characterList;
    public GameObject[] constructionList, trapType, economyResourceList, additionResourceList;
    public Transform mapRoot;
    public GameObject CreateObj(PlaceType placeType, Vector2 Pos, Quaternion? quaternion = null)
    {
        int enumValue = Convert.ToInt32(placeType);
        try
        {
            if (enumValue >= 2 && placeList[enumValue - 2].p.Count > 0)
            {
                Quaternion rotation = quaternion ?? Quaternion.identity;

                // Override rotation for space tiles
                if (placeType == PlaceType.Space)
                    rotation = Quaternion.Euler(0, 0, 90 * Tool.GetRandom(0, 4));

                return Instantiate(Tool.RandomSelect(placeList[enumValue - 2].p), Pos, rotation, mapRoot);
            }
        }
        catch (Exception e)
        {
            print(placeType);
        }
            return null;
        
    }

    public GameObject CreateObj(CharacterType characterType, Vector2 Pos, Quaternion? quaternion = null)
    {
        int enumValue = Convert.ToInt32(characterType);
        if (enumValue >= 1 && characterList[enumValue - 1])
            return Instantiate(characterList[enumValue - 1], Pos, quaternion ?? Quaternion.identity, mapRoot);
        return null;
    }

    public GameObject CreateObj(ConstructionType constructionType, Vector2 Pos)
    {
        int enumValue = Convert.ToInt32(constructionType);
        if (enumValue >= 1 && constructionList[enumValue - 1])
            return Instantiate(constructionList[enumValue - 1], Pos, Quaternion.identity, mapRoot);
        return null;
    }

    public GameObject CreateObj(TrapType trapType, Vector2 Pos)
    {
        int enumValue = Convert.ToInt32(trapType);
        if (enumValue >= 1 && this.trapType[enumValue - 1])
            return Instantiate(this.trapType[enumValue - 1], Pos, Quaternion.identity, mapRoot);
        return null;
    }

    public GameObject CreateObj(EconomyResourceType economyResourceType, Vector2 Pos)
    {
        int enumValue = Convert.ToInt32(economyResourceType);
        if (enumValue >= 1 && economyResourceList[enumValue - 1])
            return Instantiate(economyResourceList[enumValue - 1], Pos, Quaternion.identity, mapRoot);
        return null;
    }

    public GameObject CreateObj(AdditionResourceType additionResourceType, Vector2 Pos)
    {
        int enumValue = Convert.ToInt32(additionResourceType);
        if (enumValue >= 1 && additionResourceList[enumValue - 1])
            return Instantiate(additionResourceList[enumValue - 1], Pos, Quaternion.identity, mapRoot);
        return null;
    }
    void Start()
    {
        mapRoot = GameObject.Find("Map").transform;
    }
}