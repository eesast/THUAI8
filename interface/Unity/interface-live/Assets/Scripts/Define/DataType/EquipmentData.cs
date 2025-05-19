using UnityEngine;
using Protobuf;
[CreateAssetMenu]
public class EquipmentData : ScriptableObject
{
    public EquipmentType equipmentType;
    public int cost;
    public int duration;
    public string equipmentName;
    public Sprite sprite;
}