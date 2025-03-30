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

                    BlindState = (player.blind) ? Protobuf.CharacterState.Blind : Protobuf.CharacterState.NullCharacterState,
                    BlindTime = player.BlindTime,
                    // 待修改，Character.cs中没有knockedback
                    // KnockbackState = (player.knockedback) ? Protobuf.CharacterState.KnockedBack : Protobuf.CharacterState.NullCharacterState,
                    // KnockbackTime = player.KnockedBackTime,
                    StunnedState = (player.stunned) ? Protobuf.CharacterState.Stunned : Protobuf.CharacterState.NullCharacterState,
                    StunnedTime = player.StunnedTime,
                    InvisibleState = (player.visible) ? Protobuf.CharacterState.NullCharacterState : Protobuf.CharacterState.Invisible,
                    // 待修改，Character.cs中没有InvisibleTime
                    // InvisibleTime = (double)player.InvisibleTime,
                    // 貌似不需要治疗时间
                    // HealingState = (player.healing) ? Protobuf.CharacterState.Healing : Protobuf.CharacterState.NullCharacterState,
                    // HealingTime = (double)player.HealingTime,
                    // 待修改，crazyman不知道是buff还是药水
                    // BerserkState = (player.CrazyManNum == 1) ? Protobuf.CharacterState.Berserk : Protobuf.CharacterState.NullCharacterState,
                    // BerserkTime = CrazyManTime,
                    BurnedState = (player.burned) ? Protobuf.CharacterState.Burned : Protobuf.CharacterState.NullCharacterState,
                    BurnedTime = player.BurnedTime,
                    HarmCut = player.HarmCut,
                    HarmCutTime = player.HarmCutTime,
                    DeceasedState = (player.characterState2 == CharacterState.DECEASED) ? Protobuf.CharacterState.Deceased : Protobuf.CharacterState.NullCharacterState,

                    CharacterPassiveState = Transformation.CharacterStateToProto(player.CharacterState2),

                    X = player.Position.x,
                    Y = player.Position.y,

                    FacingDirection = player.FacingDirection.Angle(),
                    Speed = player.MoveSpeed,
                    ViewRange = player.ViewRange,

                    CommonAttack = (int)player.AttackPower,
                    // 待修改，Character.cs中没有CommonAttackCD
                    CommonAttackCd = 1 / player.ATKFrequency,
                    CommonAttackRange = (int)player.AttackSize,

                    SkillAttackCd = player.skillCD,

                    EconomyDepletion = player.EconomyDepletion,
                    KillScore = (int)player.GetCost(),

                    Hp = (int)player.HP,

                    // 待修改，Shield要分两类
                    ShieldEquipment = (int)player.Shield, // 加成值，只包含护盾装备
                    ShoesEquipment = (int)player.Shoes, // 加成值
                    ShoesEquipmentTime = player.QuickStepTime, // 包含所有速度加成的时间
                    // 待修改，Transformation缺东西
                    // PurificationEquipment = (player.Purified) ? Protobuf.EquipmentType.PurificationPotion : Protobuf.PurificationEquipmentType.NullEquipmentType,
                    PurificationEquipmentTime = player.PurifiedTime,
                    // 待修改，Character.cs没有隐身时间，没有狂暴药水
                    // InvisibilityEquipment = player.Invisibility,
                    // InvisibilityEquipmentTime = player.InsvisibilityTime,
                    // Berserk = player.CrazyManNum, // 数值，1~3表示等级，0表示没有
                    // BerserkTime = player.CrazyManTime,

                    // 待修改，Transformation缺东西
                    // AttackBuff = (player.CrazyManNum == 1) ? Protobuf.CharacterBuffType.AttackBuff1 : (player.CrazyManNum == 2) ? Protobuf.CharacterBuffType.AttackBuff2 : (player.CrazyManNum == 3) ? Protobuf.CharacterBuffType.AttackBuff3 : Protobuf.CharacterBuffType.NullAttackBuff,
                    AttackBuffTime = player.CrazyManTime,
                    // 待修改
                    SpeedBuffTime = player.QuickStepTime,
                    // VisionBuff = (player.CanSeeAll) ? Protobuf.CharacterBuffType.VisionBuff : Protobuf.CharacterBuffType.NullCharacterBuffType,
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
