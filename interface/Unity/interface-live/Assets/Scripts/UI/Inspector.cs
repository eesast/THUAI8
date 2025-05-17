using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Protobuf;
using TMPro;

public class Inspector : SingletonMono<Inspector>
{
    public Image equipmentImage;
    public Text equipmentName;
    public TextMeshProUGUI infoText;
    private Animator animator;
    private CharacterInfo? characterInfo;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (characterInfo is CharacterInfo info)
        {
            infoText.text =
                $"Player ID: {info.playerId}\n" +
                $"Team ID: {info.teamId}\n" +
                $"Type: {info.type}\n" +
                $"Hp: {info.message.Hp}/{info.data.maxHp}\n" +
                $"State1: {info.message.CharacterActiveState}\n" +
                $"State2: {info.message.CharacterPassiveState}\n";
        }
    }

    public void ShowEquipment(EquipmentType equipmentType)
    {
        EquipmentData data = ParaDefine.Instance.GetData(equipmentType);
        equipmentImage.sprite = data.sprite;
        equipmentName.text = data.equipmentName;
    }

    public void SetCharacter(CharacterInfo? info)
    {
        characterInfo = info;
    }

    public void Toggle(bool isShow)
    {
        if (isShow)
        {
            animator.SetTrigger("Appear");
        }
        else
        {
            animator.SetTrigger("Disappear");
        }
    }

    public void Toggle()
    {
        animator.SetTrigger("Toggle");
    }
}
