using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanelControl : MonoBehaviour
{
    public GameObject ShopPanel;

    public void ShopButtonClicked()
    {
        ShopPanel.SetActive(true);
    }
    public void ShopButtonExit()
    {
        ShopPanel.SetActive(false);
    }
}
