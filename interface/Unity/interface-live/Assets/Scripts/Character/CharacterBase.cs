using System;
using Protobuf;
using UnityEngine;
using TMPro;
using Spine.Unity;
using UnityEngine.UI;

public class CharacterBase : MonoBehaviour
{
    public long ID;
    public CharacterType characterType;
    public MessageOfCharacter message => CoreParam.characters[ID];
    bool GetDeceased() => message.Hp <= 0 || message.CharacterActiveState == CharacterState.Deceased;
    public int maxHp => ParaDefine.GetInstance().GetMaxHp(characterType);
    private Transform hpBar;
    private TextMeshPro hpText;
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
        hpBar.localScale = new Vector3(ratio, 1, 1);
        hpText.text = text;
        if (globalHpBar != null)
        {
            globalHpBar.value = ratio;
            globalHpText.text = "HP: " + text;
        }
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        hpBar = transform.Find("HpBar").Find("HpBarFillWrapper");
        hpText = transform.Find("HpBar").Find("HpBarText").GetComponent<TextMeshPro>();
        if (characterType == CharacterType.TangSeng)
        {
            globalHpBar = RenderManager.GetInstance().hpBarBud;
            globalHpText = RenderManager.GetInstance().hpBud;
        }
        if (characterType == CharacterType.JiuLing)
        {
            globalHpBar = RenderManager.GetInstance().hpBarMon;
            globalHpText = RenderManager.GetInstance().hpMon;
        }

        stateIcons = transform.Find("StateIcons");

        visual = transform.GetComponentInChildren<SkeletonMecanim>().transform;
        if (visual == null) visual = transform.Find("Appearance");
        visualScaleInitial = visual.localScale;

    }

    void Update()
    {
        UpdateHpBar();
        switch (message.CharacterPassiveState)
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

        if (message.CharacterActiveState != CharacterState.NullCharacterState)
        {
            foreach (Transform icon in stateIcons)
                icon.gameObject.SetActive(icon.name == message.CharacterActiveState.ToString());
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