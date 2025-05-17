using System.Collections.Generic;
using UnityEngine;
using Protobuf;

public struct CharacterInfo
{
    public long ID;
    public long playerId;
    public long teamId;
    public CharacterType type;
    public CharacterBase characterBase;
    public CharacterControl characterControl;
    public readonly MessageOfCharacter message => CoreParam.characters.GetValueOrDefault(ID, null);
    public readonly CharacterData data => ParaDefine.Instance.GetData(type);
}


public class CharacterManager : SingletonMono<CharacterManager>
{
    public Dictionary<long, CharacterInfo> characterInfo = new();
    public SideBar[] sideBars = new SideBar[2];


    public void AddCharacter(CharacterInfo info)
    {
        characterInfo[info.ID] = info;
        info.characterBase.ID = info.ID;
        sideBars[info.teamId].AddItem(info);
    }
}