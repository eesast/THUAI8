using System;
using System.Collections;
using System.Collections.Generic;
using Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SideBarItem : MonoBehaviour
{
    [NonSerialized] public SideBar sideBar;
    [NonSerialized] public CharacterInfo info;
    public Image avatar, hpBar;
    public TextMeshProUGUI hpText;

    void Start()
    {
        avatar.sprite = info.data.avatar;
    }

    void Update()
    {
        if (info.message == null) return;
        hpBar.fillAmount = info.message.Hp / (float)info.data.maxHp;
        hpText.text = $"{info.message.Hp}/{info.data.maxHp}";
    }

    public void OnClick()
    {
        PlayerControl.Instance.SelectedObject = info.characterControl;
        Camera.main.GetComponent<CameraControl>().cameraMode = CameraControl.CameraMode.Follow;
    }
}
