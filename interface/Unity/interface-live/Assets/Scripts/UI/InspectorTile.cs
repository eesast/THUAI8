using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InspectorTile : SingletonMono<InspectorTile>
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI infoText;
    private TileInteract tile;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (tile == null) return;
        infoText.text = $"位置: ({tile.pos.Item1}, {tile.pos.Item2})\n";
        switch (tile.tileType)
        {
            case TileInteract.TileType.Barracks:
                titleText.text = "兵营";
                infoText.text +=
                    $"队伍 ID: {CoreParam.barracks[tile.pos].TeamId}\n" +
                    $"HP: {tile.GetHP()} / {tile.GetMaxHP()}\n";
                break;
            case TileInteract.TileType.Farm:
                titleText.text = "农田";
                infoText.text +=
                    $"队伍 ID: {CoreParam.farms[tile.pos].TeamId}\n" +
                    $"HP: {tile.GetHP()} / {tile.GetMaxHP()}\n";
                break;
            case TileInteract.TileType.EconomyResource:
                titleText.text = "经济资源";
                infoText.text +=
                    $"开采进度: {CoreParam.economyResources[tile.pos].Process} / 100\n";
                break;
            case TileInteract.TileType.AdditionResource:
                titleText.text = "加成资源";
                infoText.text +=
                    $"类型: {ParaDefine.Instance.GetData(CoreParam.additionResources[tile.pos].AdditionResourceType).name}\n" +
                    $"HP: {tile.GetHP()} / {tile.GetMaxHP()}\n";
                break;
            case TileInteract.TileType.Trap:
                titleText.text = "陷阱";
                infoText.text +=
                    $"队伍 ID: {CoreParam.traps[tile.pos].TeamId}\n" +
                    $"类型: {ParaDefine.Instance.GetData(CoreParam.traps[tile.pos].TrapType).name}\n";
                break;
        }
        titleText.text += "状态";
    }

    public void SetTile(TileInteract tileInteract)
    {
        tile = tileInteract;
    }

    public void Toggle(bool isShow)
    {
        animator.SetBool("Show", isShow);
    }
}
