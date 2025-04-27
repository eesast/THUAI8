using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Preparation.Utility;
namespace Preparation.Interface
{
    public interface IShop
    {
        public int Cost { get; }
    }
    public interface ILifeMedicine : IShop
    {
        public int Recovery { get; }
        public EquipmentType RecoveryType { get; }
    }
    public interface IShield : IShop
    {
        public int ShieldHP { get; }
        public EquipmentType ShieldType { get; }
    }
    public interface IShoes : IShop
    {
        public int ShoesSpeed { get; }
        public EquipmentType ShoesType { get; }
    }
    public interface IPurification : IShop
    {
        public int ContinueTime { get; }
        public EquipmentType equipmentType { get; }
    }
    public interface IInvisible : IShop
    {
        public int ContinueTime { get; }
        public EquipmentType equipmentType { get; }
    }
    public interface ICrazy : IShop
    {
        public int ContinueTime { get; }
        public double PowerImprove { get; }
        public int SpeedImprove { get; }
        public double AttackFrequencyImprove { get; }
        public EquipmentType equipmentType { get; }
    }
}
