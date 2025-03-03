using System;
using Protobuf;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
public class CharacterBase : MonoBehaviour
{
    //public long ID;
    public CharacterType characterType;
    public CharacterState characterState;
    public PlayerTeam TeamId => characterType switch
    {
        CharacterType.Camp1Character1 => PlayerTeam.BuddhistsTeam,
        CharacterType.Camp1Character2 => PlayerTeam.BuddhistsTeam,
        CharacterType.Camp1Character3 => PlayerTeam.BuddhistsTeam,
        CharacterType.Camp1Character4 => PlayerTeam.BuddhistsTeam,
        CharacterType.Camp1Character5 => PlayerTeam.BuddhistsTeam,
        CharacterType.Camp1Character6 => PlayerTeam.BuddhistsTeam,

        CharacterType.Camp2Character1 => PlayerTeam.MonstersTeam,
        CharacterType.Camp2Character2 => PlayerTeam.MonstersTeam,
        CharacterType.Camp2Character3 => PlayerTeam.MonstersTeam,
        CharacterType.Camp2Character4 => PlayerTeam.MonstersTeam,
        CharacterType.Camp2Character5 => PlayerTeam.MonstersTeam,
        CharacterType.Camp2Character6 => PlayerTeam.MonstersTeam,

        _ => PlayerTeam.NullTeam
    };

    public long Id => ((int)TeamId - 1) * 4 + ((int)characterType - 1);
    public int CurrentHp => CoreParam.characters[Id].Hp;
    public int MaxHp => ParaDefine.GetInstance().GetMaxHp(characterType);
    private Transform hpBar;
    private TextMeshPro hpText;
    private Animator animator;

    void UpdateHpBar()
    {
        hpBar.localScale = new Vector3(Mathf.Clamp01((float)CurrentHp / MaxHp), 1, 1);
        hpText.text = $"{CurrentHp} / {MaxHp}";
    }

    void Start()
    {
        hpBar = transform.Find("HpBar").Find("HpBarFillWrapper");
        hpText = transform.GetComponentInChildren<TextMeshPro>();
        animator = GetComponent<Animator>();

        UpdateHpBar();
    }

    void Update()
    {
        UpdateHpBar();
        switch (characterState)
        {
            case CharacterState.Idle:
                animator.SetBool("Moving", false);
                break;
            case CharacterState.Moving:
                animator.SetBool("Moving", true);
                break;
            case CharacterState.Attacking:
                animator.SetTrigger("Attack");
                break;
        }
        if (CurrentHp <= 0)
        {
            animator.SetTrigger("Die");
        }
    }
}