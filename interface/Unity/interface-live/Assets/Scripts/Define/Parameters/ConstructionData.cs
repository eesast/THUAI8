using UnityEngine;
using Protobuf;
[CreateAssetMenu]
public class ConstructionData : ScriptableObject
{
    public ConstructionType constructionType;
    public int maxHp;
    public int buildTimeInSeconds;
}   