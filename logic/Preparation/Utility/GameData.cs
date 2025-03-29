using Preparation.Utility.Value;
using System;
namespace Preparation.Utility
{
    public static class GameData
    {
        public const int NumOfStepPerSecond = 2500;         // 每秒行走基础步数.由于移速buff的存在，角色的具体移动速度会发生变化，相应代码需调整
        public const int FrameDuration = 50;                // 每帧时长
        public const int CheckInterval = 10;                // 检查间隔
        public const uint GameDurationInSecond = 60 * 10;   // 游戏时长
        public const int LimitOfStopAndMove = 15;           // 停止和移动的最大间隔
        public const int ProduceSpeedPerSecond = 200;       // 每秒生产值

        public const int TolerancesLength = 3;
        public const int AdjustLength = 3;
        //character cost
        public const int TangSengcost = 0;
        public const int SunWukongcost = 5000;
        public const int ZhuBajiecost = 4000;
        public const int ShaWujingcost = 3000;
        public const int BaiLongmacost = 4000;
        public const int Monkidcost = 1000;

        public const int JiuLingcost = 0;
        public const int HongHaiercost = 5000;
        public const int NiuMowangcost = 4000;
        public const int TieShancost = 3000;
        public const int ZhiZhujingcost = 3000;
        public const int Pawncost = 1000;

        //character property
        public const int TangSengHP = 1000;
        public const int SunWukongHP = 200;
        public const int ZhuBajieHP = 300;
        public const int ShaWujingHP = 150;
        public const int BaiLongmaHP = 150;
        public const int MonkidHP = 50;
        public const int JiuLingHP = 1000;
        public const int HongHaierHP = 200;
        public const int NiuMowangHP = 300;
        public const int TieShanHP = 150;
        public const int ZhiZhujingHP = 150;
        public const int PawnHP = 50;

        public const int TangSengATKsize = 0;
        public const int SunWukongATKsize = 1000;
        public const int ZhuBajieATKsize = 2000;
        public const int ShaWujingATKsize = 5000;
        public const int BaiLongmaATKsize = 5000;
        public const int MonkidATKsize = 1000;
        public const int JiuLingATKsize = 0;
        public const int HongHaierATKsize = 1000;
        public const int NiuMowangATKsize = 2000;
        public const int TieShanATKsize = 5000;
        public const int ZhiZhujingATKsize = 5000;
        public const int PawnATKsize = 1000;

        public const int TangSengATKpower = 0;
        public const int SunWukongATKpower = 30;
        public const int ZhuBajieATKpower = 20;
        public const int ShaWujingATKpower = 10;
        public const int BaiLongmaATKpower = 10;
        public const int MonkidATKpower = 5;
        public const int JiuLingATKpower = 0;
        public const int HongHaierATKpower = 25;
        public const int NiuMowangATKpower = 20;
        public const int TieShanATKpower = 10;
        public const int ZhiZhujingATKpower = 10;
        public const int PawnATKpower = 5;

        public const int NumOfPosGridPerCell = 1000;    // 每格的【坐标单位】数
        public const int MapLength = 50000;             // 地图长度
        public const int MapRows = 50;                  // 行数
        public const int MapCols = 50;                  // 列数

        public const int Viewrange = 8000;

        public const int SkillRange1 = 6000;            //技能释放范围，适用除“龙腾四海”外的范围型技能
        public const int SkillRange2 = 10000;           //技能释放范围，适用“龙腾四海”
        public const long SunWukongSkillATK = 50;
        public const long BaiLongmaSkillATK = 20;
        public const long HongHaierSkillATK = 15;
        public const long NiuMowangShield = 100;
        public const long TieShanSkillATK = 20;
        public const long ZhiZhujingSkillATK = 10;

        public const int CharacterRadius = 400;
        public const int AResourceRadius = 400;
        public static XY GetCellCenterPos(int x, int y)  // 求格子的中心坐标
            => new(x * NumOfPosGridPerCell + NumOfPosGridPerCell / 2,
                   y * NumOfPosGridPerCell + NumOfPosGridPerCell / 2);
        public static int PosGridToCellX(XY pos)  // 求坐标所在的格子的x坐标
            => pos.x / NumOfPosGridPerCell;
        public static int PosGridToCellY(XY pos)  // 求坐标所在的格子的y坐标
            => pos.y / NumOfPosGridPerCell;
        public static CellXY PosGridToCellXY(XY pos)  // 求坐标所在的格子的xy坐标
            => new(PosGridToCellX(pos), PosGridToCellY(pos));

        public static bool IsInTheSameCell(XY pos1, XY pos2) => PosGridToCellXY(pos1) == PosGridToCellXY(pos2);
        public static bool PartInTheSameCell(XY pos1, XY pos2)
        {
            return Math.Abs((pos1 - pos2).x) < CharacterRadius + (NumOfPosGridPerCell / 2)
                && Math.Abs((pos1 - pos2).y) < CharacterRadius + (NumOfPosGridPerCell / 2);
        }
        public static bool ApproachToInteract(XY pos1, XY pos2)
        {
            return Math.Abs(PosGridToCellX(pos1) - PosGridToCellX(pos2)) <= 1
                && Math.Abs(PosGridToCellY(pos1) - PosGridToCellY(pos2)) <= 1;
        }
        public static bool ApproachToInteractInACross(XY pos1, XY pos2)
        {
            if (pos1 == pos2) return false;
            return (Math.Abs(PosGridToCellX(pos1) - PosGridToCellX(pos2))
                  + Math.Abs(PosGridToCellY(pos1) - PosGridToCellY(pos2))) <= 1;
        }
        public static bool IsInTheRange(XY pos1, XY pos2, int range)
        {
            return (pos1 - pos2).Length() <= range;
        }
        public static bool IsOnTheSameLine(XY pos1, XY pos2, double angle)//以pos1为基准，检测pos2是否在以pos1为端点、与x轴正方向呈angle角的射线上（逆时针为正方向）
        {
            double sinx = (pos2 - pos1).y / (pos2 - pos1).Length();
            double cosx = (pos2 - pos1).x / (pos2 - pos1).Length();
            if (Math.Abs(sinx - Math.Sin(angle)) < 0.01 && Math.Abs(cosx - Math.Cos(angle)) < 0.01)
            {
                return true;
            }
            else
                return false;
        }
        public static bool NeedCopy(GameObjType gameObjType)
        {
            return gameObjType != GameObjType.NULL &&
                   gameObjType != GameObjType.BARRIER &&
                   gameObjType != GameObjType.BUSH &&
                   gameObjType != GameObjType.ECONOMY_RESOURCE &&
                   gameObjType != GameObjType.ADDITIONAL_RESOURCE &&
                   gameObjType != GameObjType.CONSTRUCTION &&
                   gameObjType != GameObjType.TRAP &&
                   gameObjType != GameObjType.HOME &&
                    gameObjType != GameObjType.OUTOFBOUNDBLOCK;
        }
        public const int ConstructionHP = 1000;//建筑物的默认HP
        public const int BarracksHP = 600;
        public const int SpringHP = 300;
        public const int FarmHP = 400;
        public const int BarracksConstructSpeed = 40;//用血量/建造时间表示速度600/15
        public const int SpringConstructSpeed = 30;//300/10
        public const int FarmConstructSpeed = 40;//400/10
        public const int TrapConstructSpeed = 20;//100/5
        public const int CageConstructSpeed = 20;//100/5
        public const int TimerInterval = 1000;
        public const int TrapRange = 1;
        public const int TrapDamage = 20;
        public const int TrapTime = 5000;

        public static readonly XY PosNotInGame = new(1, 1);

        public const int ResourceHP = 10000;

        //加成资源
        public const int CrazyMan1HP = 400;
        public const int CrazyMan2HP = 500;
        public const int CrazyMan3HP = 600;
        public const int LifePool1HP = 200;
        public const int LifePool2HP = 300;
        public const int LifePool3HP = 400;
        public const int QuickStepHP = 300;
        public const int WideViewHP = 300;

        public const int CrazyMan1ATK = 10;
        public const int CrazyMan2ATK = 15;
        public const int CrazyMan3ATK = 20;
        public const int LifePoolATK = 10;
        public const int QuickStepATK = 10;
        public const int WideViewATK = 10;

        //装备
        public const int LifeMedicine1cost = 1500;
        public const int LifeMedicine2cost = 3000;
        public const int LifeMedicine3cost = 4500;
        public const int LifeMedicine1HP = 50;
        public const int LifeMedicine2HP = 100;
        public const int LifeMedicine3HP = 150;

        public const int Shield1cost = 2000;
        public const int Shield2cost = 3500;
        public const int Shield3cost = 5000;
        public const int Shield1 = 50;
        public const int Shield2 = 100;
        public const int Shield3 = 150;

        public const int ShoesCost = 1500;
        public const int ShoesSpeed = 500;

        public const int PurificationCost = 2000;
        public const int PurificationTime = 30000;

        public const int InvisibleCost = 4000;
        public const int InvisibleTime = 10000;

        public const int CrazyCost = 10000;
        public const int CrazyTime = 30000;
        public const double CrazyPower = 1.2;
        public const double CrazyATKFreq = 1.25;
        public const int CrazySpeed = 300;
        public const int ScoreFarmPerSecond = 100;
        public const int MaxCharacterNum = 1;
        public const int InitialMoney = 5000;
        public const int CharacterTotalNumMax = 6;
        public const double RecycleRate = 0.5;
    }
}
