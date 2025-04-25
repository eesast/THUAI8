using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ToolTipController : MonoBehaviour
{
    public static ToolTipController Instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipTextComponent;
    public Vector3 offset = new Vector3(100, -100, 0);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowTooltip(string text, Vector3 position)
    {
        tooltipTextComponent.text = text;
        tooltipPanel.transform.position = position + offset;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}