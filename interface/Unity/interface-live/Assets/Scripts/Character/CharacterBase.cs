using System;
using Protobuf;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    public CharacterType characterType;
    public int TeamID => (int)characterType <= 6 ? 1 : 2;
    public int maxHp, currentHp;
}