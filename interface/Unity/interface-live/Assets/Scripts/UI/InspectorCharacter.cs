using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Protobuf;
using TMPro;

public class InspectorCharacter : SingletonMono<InspectorCharacter>
{
    public Image equipmentImage;
    public Text equipmentName;
    public TextMeshProUGUI infoText;
    private Animator animator;
    private CharacterInfo? characterInfo;

    readonly Dictionary<CharacterState, string> activeStateTranslation = new()
        {
            { CharacterState.NullCharacterState, "空置" },
            { CharacterState.Idle, "空置" },
            { CharacterState.Harvesting, "开采" },
            { CharacterState.Attacking, "攻击" },
            { CharacterState.SkillCasting, "释放技能" },
            { CharacterState.Constructing, "建造" },
            { CharacterState.Moving, "移动" }
        };
    readonly Dictionary<CharacterState, string> passiveStateTranslation = new()
        {
            { CharacterState.NullCharacterState, "无" },
            { CharacterState.Idle, "无" },
            { CharacterState.Blind, "致盲" },
            { CharacterState.KnockedBack, "击退" },
            { CharacterState.Stunned, "定身" },
            { CharacterState.Invisible, "隐身" },
            { CharacterState.Healing, "治疗" },
            { CharacterState.Berserk, "狂暴" },
            { CharacterState.Burned, "灼烧" },
            { CharacterState.Deceased, "死亡" }
        };

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (characterInfo is CharacterInfo info)
        {
            infoText.text =
                $"玩家 ID: {info.playerId}      " +
                $"队伍 ID: {info.teamId}\n" +
                $"角色类型: {info.data.characterName}\n";
            if (info.deceased)
            {
                infoText.text += "角色已死亡";
            }
            else
            {
                infoText.text +=
                    $"位置: ({info.message.X}, {info.message.Y})\n" +
                    $"HP: {info.message.Hp}/{info.data.maxHp}\n" +
                    $"主动状态: {activeStateTranslation[info.message.CharacterActiveState]}\n" +
                    $"被动状态: {passiveStateTranslation[info.message.CharacterPassiveState]}\n";
            }
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
        animator.SetBool("Show", isShow);
    }
}
