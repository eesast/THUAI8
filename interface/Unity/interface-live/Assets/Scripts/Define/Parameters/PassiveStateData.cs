using UnityEngine;
using Protobuf;
[CreateAssetMenu]
public class PassiveStateData : ScriptableObject
{
    public CharacterState state;
    public Sprite sprite;
}