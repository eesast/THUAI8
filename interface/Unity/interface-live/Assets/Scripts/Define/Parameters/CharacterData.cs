using UnityEngine;
using Protobuf;
[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    public CharacterType characterType;
    public int maxHp;
    public int attackRange;
    public int skillRange;
    public Sprite avatar;
    public string characterName;
}