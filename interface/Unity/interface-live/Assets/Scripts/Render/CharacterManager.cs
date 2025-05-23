using System.Collections.Generic;
using UnityEngine;
using Protobuf;

public struct CharacterInfo
{
    public long ID;
    public long playerId;
    public long teamId;
    public CharacterType type;
    public CharacterBase _characterBase;
    public CharacterBase characterBase
    {
        get => _characterBase ??= CoreParam.charactersG[ID].GetComponent<CharacterBase>();
        set
        {
            _characterBase = value;
        }
    }
    private CharacterInteract _characterInteract;
    public CharacterInteract characterInteract
    {
        get => _characterInteract ??= characterBase.GetComponent<CharacterInteract>();
        set
        {
            _characterInteract = value;
        }
    }
    public readonly MessageOfCharacter message => CoreParam.characters.GetValueOrDefault(ID, null);
    public readonly CharacterData data => ParaDefine.Instance.GetData(type);
    public readonly bool deceased => message == null || message.CharacterActiveState == CharacterState.Deceased;
    public readonly int Hp => deceased ? 0 : message.Hp;
}


public class CharacterManager : SingletonMono<CharacterManager>
{
    public Dictionary<long, CharacterInfo> characterInfo = new();
    public SideBar[] sideBars = new SideBar[2];


    public void AddCharacter(CharacterInfo info)
    {
        if (!characterInfo.ContainsKey(info.ID))
            sideBars[info.teamId].AddItem(info);
        characterInfo[info.ID] = info;
        info.characterBase.ID = info.ID;
    }
}