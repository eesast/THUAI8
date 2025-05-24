using System;
using Protobuf;
using UnityEngine;
using TMPro;
using Spine.Unity;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterBase : MonoBehaviour
{
    public long ID = -1;
    public CharacterType characterType;
    public MessageOfCharacter message => CoreParam.characters.GetValueOrDefault(ID, null);
    bool GetDeceased() => message.Hp <= 0 || message.CharacterActiveState == CharacterState.Deceased;
    public int maxHp => ParaDefine.Instance.GetData(characterType).maxHp;
    private Slider globalHpBar;
    private TextMeshProUGUI globalHpText;
    private Animator animator;
    private Transform stateIcons;
    private Transform visual;
    private Vector3 visualScaleInitial;

    void UpdateHpBar()
    {
        float ratio = Mathf.Clamp01((float)message.Hp / maxHp);
        string text = $"{message.Hp} / {maxHp}";
        if (globalHpBar != null)
        {
            globalHpBar.value = ratio;
            globalHpText.text = "HP: " + text;
        }
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        var hpBar = GetComponentInChildren<HpBar>();
        hpBar.team = message.TeamId == 0 ? PlayerTeam.BuddhistsTeam : PlayerTeam.MonstersTeam;
        hpBar.getHp = () => message.Hp;
        hpBar.maxHp = maxHp;
        if (characterType == CharacterType.TangSeng)
        {
            globalHpBar = RenderManager.Instance.hpBarBud;
            globalHpText = RenderManager.Instance.hpBud;
        }
        if (characterType == CharacterType.JiuLing)
        {
            globalHpBar = RenderManager.Instance.hpBarMon;
            globalHpText = RenderManager.Instance.hpMon;
        }

        stateIcons = transform.Find("StateIcons");

        visual = transform.GetComponentInChildren<SkeletonMecanim>()?.transform;
        if (visual == null) visual = transform.Find("Appearance");
        visualScaleInitial = visual.localScale;

    }

    void Update()
    {
        if (message == null) return;
        UpdateHpBar();
        switch (message.CharacterActiveState)
        {
            case CharacterState.NullCharacterState:
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
            visual.localScale = visualScaleInitial;
        }
        else if (message.FacingDirection < 0)
        {
            visual.localScale = Vector3.Scale(visualScaleInitial, new Vector3(-1, 1, 1));
        }
    }
}