using System;
using System.Collections;
using System.Collections.Generic;
using Protobuf;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    public PlayerTeam team;
    public GameObject bar;
    public SpriteRenderer barFill;
    public TMPro.TextMeshPro text;
    public Func<int> getHp;
    public Func<int> getMaxHp;

    public void Start()
    {
        barFill.color = ParaDefine.Instance.teamColors[(int)team];
    }

    public void Update()
    {
        if (getHp == null) return;
        int hp = getHp(), maxHp = getMaxHp();
        float ratio = Mathf.Clamp01((float)hp / maxHp);
        bar.transform.localScale = new Vector3(ratio, 1, 1);
        text.text = $"{hp} / {maxHp}";
    }
}
