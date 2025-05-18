using System;
using System.Collections;
using System.Collections.Generic;
using Protobuf;
using UnityEngine;

public class ParaDefine : SingletonMono<ParaDefine>
{
    public CharacterData[] characterData;
    public ConstructionData[] constructionData;
    public TrapData[] trapData;
    public AdditionResourceData[] additionalResourceData;

    private Dictionary<CharacterType, CharacterData> characterDataDict;
    private Dictionary<ConstructionType, ConstructionData> constructionDataDict;
    private Dictionary<TrapType, TrapData> trapDataDict;
    private Dictionary<AdditionResourceType, AdditionResourceData> additionalResourceDataDict;

    void Start()
    {
        characterDataDict = new Dictionary<CharacterType, CharacterData>();
        constructionDataDict = new Dictionary<ConstructionType, ConstructionData>();
        trapDataDict = new Dictionary<TrapType, TrapData>();
        additionalResourceDataDict = new Dictionary<AdditionResourceType, AdditionResourceData>();

        foreach (var data in characterData)
            characterDataDict[data.characterType] = data;

        foreach (var data in constructionData)
            constructionDataDict[data.constructionType] = data;

        foreach (var data in trapData)
            trapDataDict[data.constructionType] = data;

        foreach (var data in additionalResourceData)
            additionalResourceDataDict[data.additionResourceType] = data;
    }


    public CharacterData GetData(CharacterType characterType)
    {
        return characterDataDict[characterType];
    }
    public ConstructionData GetData(ConstructionType constructionType)
    {
        return constructionDataDict[constructionType];
    }
    public AdditionResourceData GetData(AdditionResourceType additionResourceType)
    {
        return additionalResourceDataDict[additionResourceType];
    }

}