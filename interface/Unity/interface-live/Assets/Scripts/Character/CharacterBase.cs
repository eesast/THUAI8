using System;
using Protobuf;
using UnityEngine;
using TMPro;
using Unity.Collections;

public class CharacterBase : MonoBehaviour
{
    public long ID;
    public CharacterType characterType;
    public CharacterState GetActiveState() => CoreParam.characters[ID].CharacterActiveState;
    public CharacterState GetPassiveState() => CoreParam.characters[ID].CharacterPassiveState;
    public PlayerTeam GetTeamId() => (int)characterType switch
    {
        var x when x >= 1 && x <= 6 => PlayerTeam.BuddhistsTeam,
        var x when x >= 7 && x <= 12 => PlayerTeam.MonstersTeam,
        _ => PlayerTeam.NullTeam
    };
    bool GetDeceased() => GetCurrentHp() <= 0 || GetActiveState() == CharacterState.Deceased;
    public int GetCurrentHp() => CoreParam.characters[ID].Hp;
    public int GetMaxHp() => ParaDefine.GetInstance().GetMaxHp(characterType);
    private Transform hpBar;
    private TextMeshPro hpText;
    private Animator animator;
    private Transform stateIcons;

    void UpdateHpBar()
    {
        hpBar.localScale = new Vector3(Mathf.Clamp01((float)GetCurrentHp() / GetMaxHp()), 1, 1);
        hpText.text = $"{GetCurrentHp()} / {GetMaxHp()}";
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
        switch (GetActiveState())
        {
            case CharacterState.Idle:
                animator.SetBool("Moving", false);
                break;
            case CharacterState.Moving:
                animator.SetBool("Moving", true);
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

        if (GetPassiveState() != CharacterState.NullCharacterState)
        {
            foreach (Transform icon in stateIcons)
                icon.gameObject.SetActive(icon.name == GetPassiveState().ToString());
        }
    }
}