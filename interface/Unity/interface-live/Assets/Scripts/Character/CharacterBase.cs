using System;
using Protobuf;
using UnityEngine;
using TMPro;
using Unity.Collections;

[RequireComponent(typeof(Animator))]
public class CharacterBase : MonoBehaviour
{
    //public long ID;
    public CharacterType characterType;
    public CharacterState ActiveState => CoreParam.characters[Id].CharacterState1;
    public CharacterState PassiveState => CoreParam.characters[Id].CharacterState2;
    public PlayerTeam TeamId => (int)characterType switch
    {
        var x when x >= 1 && x <= 6 => PlayerTeam.BuddhistsTeam,
        var x when x >= 7 && x <= 12 => PlayerTeam.MonstersTeam,
        _ => PlayerTeam.NullTeam
    };
    bool Deceased => CurrentHp <= 0 || ActiveState == CharacterState.Deceased;

    public long Id => ((int)TeamId - 1) * 4 + ((int)characterType - 1);
    public int CurrentHp => CoreParam.characters[Id].Hp;
    public int MaxHp => ParaDefine.GetInstance().GetMaxHp(characterType);
    private Transform hpBar;
    private TextMeshPro hpText;
    private Animator animator;
    private Transform stateIcons;

    void UpdateHpBar()
    {
        hpBar.localScale = new Vector3(Mathf.Clamp01((float)CurrentHp / MaxHp), 1, 1);
        hpText.text = $"{CurrentHp} / {MaxHp}";
    }

    void Start()
    {
        animator = GetComponent<Animator>();

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
        switch (ActiveState)
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
        if (Deceased != animator.GetBool("Deceased"))
        {
            animator.SetBool("Deceased", Deceased);
            if (Deceased)
                animator.SetTrigger("Die");
        }

        if (PassiveState != CharacterState.NullCharacterState)
        {
            foreach (Transform icon in stateIcons)
                icon.gameObject.SetActive(icon.name == PassiveState.ToString());
        }
    }
}