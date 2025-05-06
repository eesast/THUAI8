using System;
using Protobuf;
using UnityEngine;
using TMPro;

public class CharacterBase : MonoBehaviour
{
    public long ID;
    public CharacterType characterType;
    public MessageOfCharacter message => CoreParam.characters[ID];
    public PlayerTeam GetTeamId() => (int)characterType switch
    {
        var x when x >= 1 && x <= 6 => PlayerTeam.BuddhistsTeam,
        var x when x >= 7 && x <= 12 => PlayerTeam.MonstersTeam,
        _ => PlayerTeam.NullTeam
    };
    bool GetDeceased() => message.Hp <= 0 || message.CharacterActiveState == CharacterState.Deceased;
    public int maxHp => ParaDefine.GetInstance().GetMaxHp(characterType);
    private Transform hpBar;
    private TextMeshPro hpText;
    private Animator animator;
    private Transform stateIcons;

    void UpdateHpBar()
    {
        hpBar.localScale = new Vector3(Mathf.Clamp01((float)message.Hp / maxHp), 1, 1);
        hpText.text = $"{message.Hp} / {maxHp}";
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        hpBar = transform.Find("HpBar").Find("HpBarFillWrapper");
        hpText = transform.Find("HpBar").Find("HpBarText").GetComponent<TextMeshPro>();
        try
        {
            UpdateHpBar();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        stateIcons = transform.Find("StateIcons");
    }

    void Update()
    {
        UpdateHpBar();
        switch (message.CharacterActiveState)
        {
            case CharacterState.Idle:
                animator.SetBool("Running", false);
                break;
            case CharacterState.Moving:
                animator.SetBool("Running", true);
                break;
            case CharacterState.Attacking:
            case CharacterState.Harvesting:
                animator.SetTrigger("Attack");
                break;
            case CharacterState.SkillCasting:
                animator.SetTrigger("CastSkill");
                break;
        }
        bool deceased = GetDeceased();
        if (deceased != animator.GetBool("Deceased"))
        {
            animator.SetBool("Deceased", GetDeceased());
            if (deceased)
                animator.SetTrigger("Die");
        }

        if (message.CharacterPassiveState != CharacterState.NullCharacterState)
        {
            foreach (Transform icon in stateIcons)
                icon.gameObject.SetActive(icon.name == message.CharacterPassiveState.ToString());
        }

        if (message.FacingDirection > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (message.FacingDirection < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}