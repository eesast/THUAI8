using System;
using System.Collections;
using System.Collections.Generic;
using Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SideBarItem : MonoBehaviour
{
    [NonSerialized] public SideBar sideBar;
    [NonSerialized] public long ID;
    private CharacterInfo info => CharacterManager.Instance.characterInfo[ID];
    private Button button;
    public Image avatar, deceasedMask, hpBar;
    public TextMeshProUGUI hpText;

    void Start()
    {
        avatar.sprite = info.data.avatar;
        button = GetComponentInChildren<Button>();
    }

    void Update()
    {
        hpBar.fillAmount = info.Hp / (float)info.data.maxHp;
        hpText.text = $"{info.Hp}/{info.data.maxHp}";
        deceasedMask.gameObject.SetActive(info.deceased);
        button.interactable = !info.deceased;
    }

    public void OnClick()
    {
        PlayerControl.Instance.SelectedObject = info.characterInteract;
        Camera.main.GetComponent<CameraControl>().cameraMode = CameraControl.CameraMode.Follow;
    }
}
