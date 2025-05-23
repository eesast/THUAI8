using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : SingletonMono<PlayerControl>
{
    public LayerMask interactableLayer;
    Collider2D raycaster;
    private InteractBase tobeSelectedInt, selectedInt;
    public CharacterInteract selectedCharacter => selectedInt as CharacterInteract;
    public TileInteract selectedTile => selectedInt as TileInteract;
    public InteractBase SelectedObject
    {
        get => selectedInt;
        set
        {
            if (value != null)
            {
                if (selectedInt != null)
                {
                    selectedInt.selected = false;
                    selectedInt = value;
                    OnChangeSelect();
                }
                else
                {
                    selectedInt = value;
                    OnBeginSelect();
                }
                value.tobeSelected = false;
                value.selected = true;
            }
            else
            {
                if (selectedInt != null)
                {
                    selectedInt.selected = false;
                    OnEndSelect();
                }
                selectedInt = null;
            }
        }
    }
    public InteractBase TobeSelectedObject
    {
        get => tobeSelectedInt;
        set
        {
            if (tobeSelectedInt != null)
                tobeSelectedInt.tobeSelected = false;
            tobeSelectedInt = value;
            if (value != null)
                value.tobeSelected = true;
        }
    }
    public List<InteractControl.InteractOption> enabledInteract;
    public InteractControl.InteractOption selectedOption;
    public float longClickTime, doubleClickTime;
    private float longClickTimer, doubleClickTimer;
    Vector2 mousePos;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            longClickTimer = longClickTime;
            if (doubleClickTimer > 0)
            {
                doubleClickTimer = 0;
                OnDoubleClick();
            }
            else
            {
                doubleClickTimer = doubleClickTime;
            }
        }
        longClickTimer -= Time.deltaTime;
        doubleClickTimer -= Time.deltaTime;
        if (longClickTimer < 0) longClickTimer = 0;
        if (doubleClickTimer < 0) doubleClickTimer = 0;

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CheckInteract();
        UpdateInteractList();
        Interact();
    }
    void CheckInteract()
    {
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
            InteractBase intObj = raycaster.GetComponentInParent<InteractBase>();
            TobeSelectedObject = intObj;
            if (Input.GetMouseButtonDown(0))
            {
                SelectedObject = intObj;
            }
            if (Input.GetMouseButtonDown(1))
            {
                selectedCharacter?.Move(raycaster.transform.position);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (tobeSelectedInt is CharacterInteract tobeSelectedCharacter)
                {
                    selectedCharacter?.Attack(tobeSelectedCharacter.characterBase);
                }
            }
        }
        else
        {
            TobeSelectedObject = null;

            if (Input.GetMouseButtonDown(0))
            {
                SelectedObject = null;
            }
            if (Input.GetMouseButtonDown(1))
            {
                selectedCharacter?.Move(mousePos);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                selectedCharacter?.CastSkill(mousePos);
            }
        }
    }
    void UpdateInteractList()
    {
        if (selectedInt?.interactType != null)
        {
            enabledInteract = InteractControl.Instance.interactOptions[selectedInt.interactType];
        }
        else
        {
            if (enabledInteract.Count > 0)
                enabledInteract.Clear();
        }
    }
    void Interact()
    {
        if (selectedInt == null)
            return;
        selectedInt.interactOption = selectedOption;
        selectedOption = InteractControl.InteractOption.None;
    }

    void OnBeginSelect()
    {
        if (selectedCharacter)
        {
            InspectorCharacter.Instance.Toggle(true);
            InspectorCharacter.Instance.SetCharacter(CharacterManager.Instance.characterInfo[selectedCharacter.ID]);
        }
        if (selectedTile)
        {
            // Inspector.Instance.Toggle(true);
            // Inspector.Instance.SetTile(selectedTile);
        }
    }
    void OnChangeSelect()
    {
        if (selectedCharacter)
        {
            InspectorCharacter.Instance.SetCharacter(CharacterManager.Instance.characterInfo[selectedCharacter.ID]);
        }
        if (selectedTile)
        {
            // Inspector.Instance.SetTile(selectedTile);
        }
        
    }
    void OnEndSelect()
    {
        if (selectedCharacter)
        {
            InspectorCharacter.Instance.Toggle(false);
        }
        if (selectedTile)
        {
            InspectorCharacter.Instance.Toggle(false);
        }
    }

    void OnDoubleClick()
    {
        if (selectedCharacter != null)
        {
            Camera.main.GetComponent<CameraControl>().cameraMode = CameraControl.CameraMode.Follow;
        }
    }
}