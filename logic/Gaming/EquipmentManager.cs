using GameClass.GameObj;
using Preparation.Utility;

namespace Gaming
{
    public partial class Game
    {
        private readonly EquipmentManager equipManager;
        private class EquipmentManager
        {
            public bool GetEquipment(Character character, EquipmentType EquipType)
            {
                return character.GetEquipments(EquipType);
            }
        }
    }
}
