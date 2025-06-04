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
    public GameObject skillFX;
    private Slider globalHpBar;
    private TextMeshProUGUI globalHpText;
    private Animator animator;
    private Transform stateIcons;
    private Transform visual;
    private Vector3 visualScaleInitial;
    private CharacterState lastState;
    private int lastHp;
    private long lastSkillTime = long.MaxValue;

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
        hpBar.getHp = () => message?.Hp ?? 0;
        hpBar.getMaxHp = () => maxHp;
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
                if (lastState != CharacterState.Attacking)
                    animator.SetTrigger("Attack");
                break;
            case CharacterState.Harvesting:
                animator.SetTrigger("Attack");
                break;
            case CharacterState.SkillCasting:
                if (lastState != CharacterState.SkillCasting)
                    animator.SetTrigger("CastSkill");
                break;
        }
        lastState = message.CharacterActiveState;
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

        // Temporary patch for Server failing to send Attacking and SkillCasting state:
        // When getting attacked (HP decreased), manually guess possible attacker
        // according to the harm value and the attacking range 
        int harm = lastHp - message.Hp;
        if (harm > 0)
        {
            bool flag = false;
            foreach (var (id, message) in CoreParam.characters)
            {
                if (id == ID) continue;

                int atk = message.CommonAttack;
                int range = message.CommonAttackRange;

                if (message.IsBurned && atk == 15) return;

                int sqDist = (int)Mathf.Pow(message.X - this.message.X, 2) + (int)Mathf.Pow(message.Y - this.message.Y, 2);
                if (atk == harm && sqDist <= range * range)
                {
                    CharacterManager.Instance.characterInfo[id].characterBase.ManualSetAttack();
                    flag = true;
                    break;
                }
            }
            /*if (!flag && harm == 50)
            {
                foreach (var (id, message) in CoreParam.characters)
                {
                    if (message.CharacterType == CharacterType.SunWukong
                     && CoreParam.currentFrame.AllMessage.GameTime - message.SkillAttackCd <= 100)
                        CharacterManager.Instance.characterInfo[id].characterBase.ManualSetCastSkill(transform);
                }

            }*/

        }
        lastHp = message.Hp;

        if (message.SkillAttackCd > lastSkillTime)
            ManualSetCastSkill();
        lastSkillTime = message.SkillAttackCd;
    }

    void OnDestroy()
    {
        if (globalHpBar != null)
        {
            globalHpBar.value = 0;
            globalHpText.text = "HP: 0 / " + maxHp;
        }
    }

    public void ManualSetAttack()
    {
        animator.SetTrigger("Attack");
    }

    public void ManualSetCastSkill(Transform target = null)
    {
        animator.SetTrigger("CastSkill");
        GameObject skillFXObj = null;
        if (skillFX != null) skillFXObj = Instantiate(skillFX, transform);
        if (characterType == CharacterType.SunWukong)
        {
            skillFXObj.transform.LookAt(target);
        }
        Destroy(skillFXObj, 15);
    }
}