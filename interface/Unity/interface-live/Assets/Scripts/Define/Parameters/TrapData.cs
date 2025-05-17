using UnityEngine;
using Protobuf;
[CreateAssetMenu]
public class TrapData : ScriptableObject
{
    public TrapType constructionType;
    public int maxHp;
    public int buildTimeInSeconds;
}