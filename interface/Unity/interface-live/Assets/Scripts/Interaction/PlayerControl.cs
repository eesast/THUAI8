using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : SingletonMono<PlayerControl>
{
    public LayerMask interactableLayer;
    Collider2D raycaster;
    public InteractBase tobeSelectedInt, selectedInt;
    private CharacterControl selectedCharacter => selectedInt as CharacterControl;
    public List<InteractControl.InteractOption> enabledInteract;
    public InteractControl.InteractOption selectedOption;
    public float longClickTime, longClickTimer;
    public Vector2 clickPnt, cameraPos;

    // Update is called once per frame
    void Update()
    {
        // testInput();
        if (Input.GetKeyDown(KeyCode.E) && selectedInt != null)
        {
            selectedInt.selected = false;
            selectedInt = null;
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
            intObj.tobeSelected = true;
            tobeSelectedInt = intObj;
            if (Input.GetMouseButtonDown(0))
            {
                if (selectedInt != null)
                    selectedInt.selected = false;
                intObj.tobeSelected = false;
                intObj.selected = true;
                selectedInt = intObj;
            }
            if (Input.GetMouseButtonDown(1))
            {
                selectedCharacter?.Move(raycaster.transform.position);
            }
        }
        else
        {
            if (tobeSelectedInt != null)
                tobeSelectedInt.tobeSelected = false;
            tobeSelectedInt = null;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetMouseButtonUp(0) && longClickTimer > 0)
                {
                    // selectedCharacter?.Attack();
                }
                if (Input.GetMouseButton(0))
                {
                    Camera.main.transform.position = (Vector3)cameraPos - Camera.main.ScreenToWorldPoint(Input.mousePosition) + (Vector3)Camera.main.ScreenToWorldPoint(clickPnt);
                    Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -10);
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                selectedCharacter?.Move(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                selectedCharacter?.CastSkill();
            }
        }
    }
    void UpdateInteractList()
    {
        if (selectedInt?.interactType != null)
        {
            enabledInteract = new List<InteractControl.InteractOption>(InteractControl.Instance.interactOptions[selectedInt.interactType]);
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
}