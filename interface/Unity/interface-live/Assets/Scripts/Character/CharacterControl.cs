// Used for LocalPlay mode
using System;
using UnityEngine;
#if !UNITY_WEBGL
[RequireComponent(typeof(CharacterBase))]
class CharacterControl : InteractBase
{
    public CharacterBase characterBase;
    public void Start()
    {
        characterBase = GetComponent<CharacterBase>();
    }
    public void Attack()
    {

    }
    public void CastSkill()
    {

    }
    public void Move(Vector2 position)
    {

    }

}
#else
class CharacterControl: InteractBase
{
    public void Start()
    {
        // Debug.LogWarning("LocalPlay mode is not implemented in this build.");
    }
}
#endif