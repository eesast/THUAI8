using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractControl : Singleton<InteractControl>
{
    public enum InteractType
    {
        NoneType,
        TangSeng,
        JiuLing,
        Character,
        CharacterSP
    }

    public enum InteractOption
    {
        None,
        Produce,
        ConstructBarracks,
        ConstructFarm,
        ConstructTrap,
        ConstructCage
    }
    public readonly Dictionary<InteractType, List<InteractOption>> interactOptions = new Dictionary<InteractType, List<InteractOption>>(){
        {InteractType.NoneType,
            null},
        {InteractType.Character,
            new List<InteractOption>{
                InteractOption.Produce,
            }},
        {InteractType.CharacterSP,
            new List<InteractOption>{
                InteractOption.ConstructBarracks,
                InteractOption.ConstructFarm,
                InteractOption.ConstructTrap,
                InteractOption.ConstructCage
            }},
    };
    public readonly Dictionary<InteractOption, string> textDic = new Dictionary<InteractOption, string>()
    {
        {InteractOption.None, ""},
        {InteractOption.Produce, "采集资源"},
        {InteractOption.ConstructBarracks, "建造兵营"},
        {InteractOption.ConstructFarm, "建造农场"},
        {InteractOption.ConstructTrap, "建造坑洞陷阱"},
        {InteractOption.ConstructCage, "建造牢笼陷阱"}
    };
    public readonly Dictionary<InteractOption, string> textCost = new Dictionary<InteractOption, string>()
    {
        {InteractOption.None, ""},
        {InteractOption.Produce, ""},
        {InteractOption.ConstructBarracks, "＄10000"},
        {InteractOption.ConstructFarm, "＄8000"},
        {InteractOption.ConstructTrap, "＄1000"},
        {InteractOption.ConstructCage, "＄1000"}
    };
}
