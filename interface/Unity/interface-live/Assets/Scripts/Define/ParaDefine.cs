using System;
using System.Collections;
using System.Collections.Generic;
using Protobuf;
using UnityEngine;

public class ParaDefine : SingletonMono<ParaDefine>
{
    public Color[] teamColors;
    public CharacterData[] characterData;
    public ConstructionData[] constructionData;
    public TrapData[] trapData;
    public AdditionResourceData[] additionalResourceData;
    public EquipmentData[] equipmentData;

    private Dictionary<CharacterType, CharacterData> characterDataDict = new();
    private Dictionary<ConstructionType, ConstructionData> constructionDataDict = new();
    private Dictionary<TrapType, TrapData> trapDataDict = new();
    private Dictionary<AdditionResourceType, AdditionResourceData> additionalResourceDataDict = new();
    private Dictionary<EquipmentType, EquipmentData> equipmentDataDict = new();

    void Start()
    {
        foreach (var data in characterData)
            characterDataDict[data.characterType] = data;

        foreach (var data in constructionData)
            constructionDataDict[data.constructionType] = data;

        foreach (var data in trapData)
            trapDataDict[data.constructionType] = data;

        foreach (var data in additionalResourceData)
            additionalResourceDataDict[data.additionResourceType] = data;

        foreach (var data in equipmentData)
            equipmentDataDict[data.equipmentType] = data;
    }


    public CharacterData GetData(CharacterType characterType)
    {
        return characterDataDict[characterType];
    }
    public ConstructionData GetData(ConstructionType constructionType)
    {
        return constructionDataDict[constructionType];
    }
    public TrapData GetData(TrapType trapType)
    {
        return trapDataDict[trapType];
    }
    public AdditionResourceData GetData(AdditionResourceType additionResourceType)
    {
        return additionalResourceDataDict[additionResourceType];
    }
    public EquipmentData GetData(EquipmentType equipmentType)
    {
        return equipmentDataDict[equipmentType];
    }

}