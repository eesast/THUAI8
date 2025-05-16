using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : SingletonMono<PlayerControl>
{
    public LayerMask interactableLayer;
    Collider2D raycaster;
    public List<InteractBase> tobeSelectedInt, selectedInt;
    public bool selectingAll;
    public List<InteractControl.InteractOption> enabledInteract;
    public InteractControl.InteractOption selectedOption;
    public float longClickTime, longClickTimer;
    public Vector2 clickPnt, cameraPos;

    // Update is called once per frame
    void Update()
    {
        // testInput();
        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (InteractBase i in selectedInt)
            {
                i.selected = false;
            }
            selectedInt.Clear();
        }
        if (Input.GetMouseButtonDown(0))
        {
            longClickTimer = longClickTime;
            cameraPos = Camera.main.transform.position;
            clickPnt = Input.mousePosition;
        }
        longClickTimer -= Time.deltaTime;
        if (longClickTimer < 0)
            longClickTimer = 0;
        CheckInteract();
        UpdateInteractList();
        Interact();
        // ShipAttack();
    }
    void TestInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
            selectedOption = InteractControl.InteractOption.Produce;
        if (Input.GetKeyDown(KeyCode.C))
            selectedOption = InteractControl.InteractOption.ConstructBarracks;
    }
    void CheckInteract()
    {
        for (int i = 0; i < selectedInt.Count; i++)
            if (!selectedInt[i])
            {
                selectedInt.Remove(selectedInt[i]);
            }
        try
        {
            raycaster = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), interactableLayer);
        }
        catch
        {
            raycaster = null;
        }
        if (raycaster)
        {
            Debug.Log("raycasthit");
            if (!selectingAll)
            {
                InteractBase intObj = raycaster.GetComponentInParent<InteractBase>();
                intObj.tobeSelected = true;
                if (!tobeSelectedInt.Contains(intObj))
                {
                    tobeSelectedInt.Add(intObj);
                }
                if (Input.GetMouseButtonDown(0))
                {
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {

                        foreach (InteractBase i in selectedInt)
                        {
                            i.selected = false;
                        }
                        selectedInt.Clear();
                    }
                    intObj.tobeSelected = false;
                    tobeSelectedInt.Remove(intObj);
                    intObj.selected = true;
                    if (!selectedInt.Contains(intObj))
                    {
                        selectedInt.Add(intObj);
                    }
                }
                if (Input.GetMouseButtonDown(1))
                {
                    // CharacterMove(raycaster.transform.position);
                }
            }
        }
        else
        {
            if (!selectingAll)
            {
                foreach (InteractBase i in tobeSelectedInt)
                {
                    i.tobeSelected = false;
                }
                tobeSelectedInt.Clear();
                // Debug.Log("clear" + tobeSelectedInt.Count);
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (Input.GetMouseButtonUp(0) && longClickTimer > 0)
                    {
                        // CharacterAttack();
                    }
                    if (Input.GetMouseButton(0))
                    {
                        Camera.main.transform.position = ((Vector3)cameraPos - Camera.main.ScreenToWorldPoint(Input.mousePosition) + (Vector3)Camera.main.ScreenToWorldPoint(clickPnt));
                        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10);
                    }
                }
                if (Input.GetMouseButtonDown(1))
                {
                    // CharacterMove(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    // CharacterSkill();
                }
            }
        }
    }
    void UpdateInteractList()
    {
        if (selectedInt.Count > 0)
        {
            // Debug.Log(selectedInt[0].interactType + "   " + InteractControl.GetInstance().interactOptions[selectedInt[0].interactType]);
            enabledInteract = new List<InteractControl.InteractOption>(InteractControl.GetInstance().interactOptions[selectedInt[0].interactType]);
            // Debug.Log(InteractControl.GetInstance().interactOptions[InteractControl.InteractType.Base].Count);
            foreach (InteractBase interactBase in selectedInt)
            {
                for (int i = 0; i < enabledInteract.Count; i++)
                    if (!InteractControl.GetInstance().interactOptions[interactBase.interactType].Contains(enabledInteract[i]))
                    {
                        enabledInteract.Remove(enabledInteract[i]);
                    }
            }
        }
        else
        {
            if (enabledInteract.Count > 0)
                enabledInteract.Clear();
        }
    }
    void Interact()
    {
        foreach (InteractBase interactBase in selectedInt)
        {
            interactBase.interactOption = selectedOption;
        }
        selectedOption = InteractControl.InteractOption.None;
    }
    /*void CharacterMove(Vector2 movePos)
    {
        foreach (InteractBase interactBase in selectedInt)
        {
            interactBase.enableMove = true;
            interactBase.moveOption = movePos;
        }
    }
    void CharacterAttack()
    {
        foreach (InteractBase interactBase in selectedInt)
        {
            interactBase.attackOption = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }
    void CharacterSkill()
    {
        foreach (InteractBase interactBase in selectedInt)
        {
            interactBase.attackOption = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }
    public void ButtonInteract(InteractControl.InteractOption option)
    {
        selectedOption = option;
    } */
}