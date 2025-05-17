using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if !UNITY_WEBGL
public class PlayerControl : SingletonMono<PlayerControl>
{
    public LayerMask interactableLayer;
    Collider2D raycaster;
    public InteractBase tobeSelectedInt, selectedInt;
    private CharacterControl selectedCharacter => selectedInt as CharacterControl;
    public List<InteractControl.InteractOption> enabledInteract;
    public InteractControl.InteractOption selectedOption;
    public float longClickTime, longClickTimer;
    public Vector2 mousePos, cameraPos;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && selectedInt != null)
        {
            selectedInt.selected = false;
            selectedInt = null;
        }

        if (Input.GetMouseButtonDown(0))
        {
            longClickTimer = longClickTime;
            cameraPos = Camera.main.transform.position;
        }
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

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
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (tobeSelectedInt is CharacterControl tobeSelectedCharacter)
                    {
                        selectedCharacter?.Attack(tobeSelectedCharacter.characterBase);
                    }

                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                selectedCharacter?.Move(mousePos);
            }
            if (Input.GetKeyDown(KeyCode.Q))
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
}
#else
class PlayerControl : MonoBehaviour
{
    public void Start()
    {
        throw new System.NotImplementedException("PlayerControl/LocalPlay mode is not implemented in this build.");
    }
}
#endif