using System;
using System.Collections;
using System.Collections.Generic;
using Protobuf;
using TMPro;
using UnityEngine;

public class SideBar : MonoBehaviour
{
    static readonly Dictionary<PlayerTeam, CharacterType[]> avilableCharacters = new()
    {
        { PlayerTeam.BuddhistsTeam, new[] { CharacterType.SunWukong, CharacterType.ZhuBajie, CharacterType.ShaWujing, CharacterType.BaiLongma, CharacterType.Monkid } },
        { PlayerTeam.MonstersTeam, new[] { CharacterType.HongHaier, CharacterType.NiuMowang, CharacterType.TieShan, CharacterType.ZhiZhujing, CharacterType.Pawn } }
    };
    public PlayerTeam team;
    public GameObject itemTemplate;
    public TMP_Dropdown addButton;
    List<CharacterInfo> currentCharacters = new();
    List<CharacterType> addableCharacters = new();

    void Start()
    {
        addableCharacters = new List<CharacterType>(avilableCharacters[team]);
    }

    public void AddItem(CharacterInfo info)
    {
        SideBarItem item = Instantiate(itemTemplate, transform).GetComponent<SideBarItem>();
        item.ID = info.ID;
        item.sideBar = this;
        item.gameObject.SetActive(true);
        addButton.transform.parent.SetAsLastSibling();
        currentCharacters.Add(info);
        if (currentCharacters.Count == 6)
            addButton.transform.parent.gameObject.SetActive(false);
    }
    public void ManualAdd(int index)
    {
        // Create new Player
        int teamId = (int)team - 1;
        CharacterType type = addableCharacters[index];
        if (ParaDefine.Instance.GetData(type).cost < CoreParam.teams[teamId].Energy)
        {
            Players.Instance.GetMainClient(teamId).CreatCharacter(new CreatCharacterMsg
            {
                CharacterType = type,
                TeamId = teamId,
                BirthpointIndex = 0
            });
            addButton.options.RemoveAt(index);
            addableCharacters.RemoveAt(index);
        }
    }

}
