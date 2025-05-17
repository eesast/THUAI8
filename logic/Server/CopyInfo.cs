using GameClass.GameObj;
using GameClass.GameObj.Areas;
using Preparation.Utility;
using Protobuf;
using Utility = Preparation.Utility;

namespace Server
{
    public static class CopyInfo
    {
        public static MessageOfObj? Auto(GameObj gameObj, long time)
        {
            if (gameObj.IsRemoved == true)
                return null;
            switch (gameObj.Type)
            {
                case GameObjType.CHARACTER:
                    return Character((Character)gameObj, time);
                case GameObjType.ECONOMY_RESOURCE:
                    return EconomyResource((E_Resource)gameObj);
                case GameObjType.ADDITIONAL_RESOURCE:
                    return AdditionResource((A_Resource)gameObj);
                case GameObjType.CONSTRUCTION:
                    Construction construction = (Construction)gameObj;
                    if (construction.ConstructionType == Utility.ConstructionType.BARRACKS)
                        return Barracks(construction);
                    else if (construction.ConstructionType == Utility.ConstructionType.SPRING)
                        return Spring(construction);
                    else if (construction.ConstructionType == Utility.ConstructionType.FARM)
                        return Farm(construction);
                    return null;
                case GameObjType.TRAP:
                    return Traps(gameObj);
                default: return null;
            }
        }

        public static MessageOfObj? Auto(MessageOfNews news)
        {
            MessageOfObj objMsg = new()
            {
                NewsMessage = news
            };
            return objMsg;
        }
        private static MessageOfObj? Base(Base player, long time)
        {
            MessageOfObj msg = new()
            {
                TeamMessage = new()
                {
                    TeamId = player.TeamID,
                    PlayerId = player.PlayerID,
                    Score = player.MoneyPool.Score,
                    Energy = player.MoneyPool.Money,
                }
            };
            return msg;
        }

        public static MessageOfObj? Auto(Base @base, long time)
        {
            return Base(@base, time);
        }
        private static MessageOfObj? Character(Character player, long time)
        {
            MessageOfObj msg = new()
            {
                CharacterMessage = new()
                {
                    Guid = player.ID,

                    TeamId = player.TeamID,
                    PlayerId = player.PlayerID,

                    CharacterType = Transformation.CharacterTypeToProto(player.CharacterType),

                    CharacterActiveState = Transformation.CharacterStateToProto(player.CharacterState1),

                    IsBlind = player.blind,
                    BlindTime = player.BlindTime,
                    IsStunned = player.stunned,
                    StunnedTime = player.StunnedTime,
                    IsInvisible = !player.visible,
                    InvisibleTime = player.InvisibleTime,
                    IsBurned = player.burned,
                    BurnedTime = player.BurnedTime,
                    HarmCut = player.HarmCut,
                    HarmCutTime = player.HarmCutTime,

                    CharacterPassiveState = Transformation.CharacterStateToProto(player.CharacterState2),

                    X = player.Position.x,
                    Y = player.Position.y,

                    FacingDirection = player.FacingDirection.Angle(),
                    Speed = player.MoveSpeed,
                    ViewRange = player.ViewRange,

                    CommonAttack = (int)player.AttackPower,
                    // 待修改，Character.cs中没有CommonAttackCD
                    CommonAttackCd = (int)(1 / player.ATKFrequency),
                    CommonAttackRange = (int)player.AttackSize,

                    SkillAttackCd = player.skillCD,

                    EconomyDepletion = player.EconomyDepletion,
                    KillScore = (int)player.GetCost(),

                    Hp = (int)player.HP,

                    // 待修改，Shield要分两类
                    ShieldEquipment = (int)player.Shield, // 护盾装备
                    ShoesEquipment = (int)player.Shoes, // 加成值
                    ShoesTime = player.ShoesTime, // 包含所有速度加成的时间
                    IsPurified = player.Purified,
                    PurifiedTime = player.PurifiedTime,
                    IsBerserk = player.IsBerserk,
                    BerserkTime = player.BerserkTime,

                    AttackBuffNum = (int)player.CrazyManNum,
                    AttackBuffTime = player.CrazyManTime,
                    SpeedBuffTime = player.QuickStepTime,
                    VisionBuffTime = player.WideViewTime,
                }
            };
            return msg;
        }

        private static MessageOfObj EconomyResource(E_Resource economyresource)
        {
            MessageOfObj msg = new()
            {
                EconomyResourceMessage = new()
                {
                    EconomyResourceState = Transformation.EconomyResourceStateToProto(economyresource.ERstate),
                    EconomyResourceType = Transformation.EconomyResourceTypeToProto(economyresource.EResourceType),

                    X = economyresource.Position.x,
                    Y = economyresource.Position.y,

                    Process = 100 - (int)economyresource.HP,
                    Id = 0,
                }
            };
            return msg;
        }

        private static MessageOfObj AdditionResource(A_Resource additionResource)
        {
            MessageOfObj msg = new()
            {
                AdditionResourceMessage = new()
                {
                    AdditionResourceType = (AdditionResourceType)additionResource.AResourceType,
                    AdditionResourceState = (Protobuf.AdditionResourceState)additionResource.ARstate,

                    X = additionResource.Position.x,
                    Y = additionResource.Position.y,

                    Hp = (int)additionResource.HP,
                    Id = 0,
                }
            };
            //   Debugger.Output(additionResource, additionResource.Place.ToString()+" "+additionResource.Position.ToString());
            return msg;
        }

        private static MessageOfObj Barracks(Construction construction)
        {
            MessageOfObj msg = new()
            {
                BarracksMessage = new()
                {
                    X = construction.Position.x,
                    Y = construction.Position.y,

                    Hp = (int)construction.HP,

                    TeamId = construction.TeamID,
                    Id = 0,
                }
            };
            return msg;
        }

        private static MessageOfObj Spring(Construction construction)
        {
            MessageOfObj msg = new()
            {
                SpringMessage = new()
                {
                    X = construction.Position.x,
                    Y = construction.Position.y,

                    Hp = (int)construction.HP,

                    TeamId = construction.TeamID,
                    Id = 0,
                }
            };
            return msg;
        }

        private static MessageOfObj Farm(Construction construction)
        {
            MessageOfObj msg = new()
            {
                FarmMessage = new()
                {
                    X = construction.Position.x,
                    Y = construction.Position.y,

                    Hp = (int)construction.HP,

                    TeamId = construction.TeamID,
                    Id = 0,
                }
            };
            return msg;
        }

        private static MessageOfObj Traps(GameObj trap)
        {
            MessageOfObj msg = new()
            {
                TrapMessage = new()
                {
                    TrapType = trap switch
                    {
                        HOLE _ => Protobuf.TrapType.Hole,
                        Cage _ => Protobuf.TrapType.Cage,
                    },

                    X = trap.Position.x,
                    Y = trap.Position.y,

                    //Hp = (int)trap.HP,            ����û��HP

                    TeamId = trap switch
                    {
                        HOLE t => t.TeamID,
                        Cage c => c.TeamID,
                    },
                    Id = 0,
                }
            };
            return msg;
        }
    }
}
