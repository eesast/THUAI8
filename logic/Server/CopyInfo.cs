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

                    CharacterActiveState = Transformation.CharacterStateToProto(player.CharacterActiveState),

                    // 待修改，被动状态用CharacterStateType还是bool
                    BlindState = (player.blind) ? Protobuf.CharacterState.BLIND : Protobuf.CharacterState.NULL_CHARACTER_STATE,
                    BlindTime = (double)player.BlindTime, // 待修改，时间是否应该用double
                    // 待修改，Character.cs中没有knockedback
                    KnockbackState = (player.knockedback) ? Protobuf.CharacterState.KNOCKED_BACK : Protobuf.CharacterState.NULL_CHARACTER_STATE,
                    KnockbackTime = (double)player.KnockedBackTime,
                    StunnedState = (player.stunned) ? Protobuf.CharacterState.STUNNED : Protobuf.CharacterState.NULL_CHARACTER_STATE,
                    StunnedTime = (double)player.StunnedTime,
                    InvisibleState = (player.visible) ? Protobuf.CharacterState.NULL_CHARACTER_STATE : Protobuf.CharacterState.INVISIBLE,
                    // 待修改，Character.cs中没有InvisibleTime
                    InvisibleTime = (double)player.InvisibleTime,
                    HealingState = (player.healing) ? Protobuf.CharacterState.HEALING : Protobuf.CharacterState.NULL_CHARACTER_STATE,
                    HealingTime = (double)player.HealingTime,
                    BerserkState = (player.crazyman) ? Protobuf.CharacterState.BERSERK : Protobuf.CharacterState.NULL_CHARACTER_STATE,
                    BerserkTime = (double)CrazyManTime,
                    BurnedState = (player.burned) ? Protobuf.CharacterState.BURNED : Protobuf.CharacterState.NULL_CHARACTER_STATE,
                    BurnedTime = (double)BurnedTime,
                    DeceasedState = (player.deceased) ? Protobuf.CharacterState.DECEASED : Protobuf.CharacterState.NULL_CHARACTER_STATE,

                    X = player.Position.x,
                    Y = player.Position.y,

                    FacingDirection = player.FacingDirection.Angle(),
                    Speed = player.MoveSpeed,
                    ViewRange = player.ViewRange,

                    CommonAttack = (int)player.AttackPower,
                    // 待修改，Character.cs中没有CommonAttackCD
                    CommonAttackCD = (double)player.AttackCD,
                    CommonAttackRange = (int)player.AttackSize,

                    SkillAttackCD = (double)player.skillCD,

                    EconomyDepletion = player.EconomyDepletion,
                    KillScore = (int)player.GetCost(),

                    HP = (int)player.HP,

                    // 待修改，Character.cs中没有区分ShieldEquipment\ShoesEquipment类型
                    Shield = player.Shield,
                    Shoes = player.Shoes,

                    // 
                    //AttackBuff = 
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

                    X = economyresource.Position.x,
                    Y = economyresource.Position.y,

                    Process = (int)economyresource.HP,      //����������
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
                        Trap _ => Protobuf.TrapType.Hole,
                        Cage _ => Protobuf.TrapType.Cage,
                    },

                    X = trap.Position.x,
                    Y = trap.Position.y,

                    //Hp = (int)trap.HP,            ����û��HP

                    TeamId = trap switch
                    {
                        Trap t => t.TeamID,
                        Cage c => c.TeamID,
                    }
                }
            };
            return msg;
        }
    }
}
