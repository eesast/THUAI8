﻿using ProtoPlace = Protobuf.PlaceType;
using ProtoCharacterType = Protobuf.CharacterType;
using ProtoCharacterState = Protobuf.CharacterState;
using ProtoEconomyResourceState = Protobuf.EconomyResourceState;
using ProtoEconomyResourceType = Protobuf.EconomyResourceType;
using ProtoEquipmentType = Protobuf.EquipmentType;
using ProtoConstructionType = Protobuf.ConstructionType;
using ProtoAdditionalResourceType = Protobuf.AdditionResourceType;
using ProtoAdditionalResourceState = Protobuf.AdditionResourceState;
using ProtoTrapType = Protobuf.TrapType;
namespace Preparation.Utility;
public static class Transformation
{
    public static ProtoPlace PlaceTypeToProto(PlaceType placeType) => placeType switch
    {
        PlaceType.SPACE => ProtoPlace.Space,
        PlaceType.BARRIER => ProtoPlace.Barrier,
        PlaceType.BUSH => ProtoPlace.Bush,
        PlaceType.ADDITION_RESOURCE => ProtoPlace.AdditionResource,
        PlaceType.ECONOMY_RESOURCE => ProtoPlace.EconomyResource,
        PlaceType.CONSTRUCTION => ProtoPlace.Construction,
        PlaceType.TRAP => ProtoPlace.Trap,
        PlaceType.HOME => ProtoPlace.Home,
        PlaceType.NULL_PLACE_TYPE => ProtoPlace.NullPlaceType,
        _ => ProtoPlace.NullPlaceType
    };
    public static PlaceType PlaceTypeFromProto(ProtoPlace placeType) => placeType switch
    {
        ProtoPlace.Space => PlaceType.SPACE,
        ProtoPlace.Barrier => PlaceType.BARRIER,
        ProtoPlace.Bush => PlaceType.BUSH,
        ProtoPlace.AdditionResource => PlaceType.ADDITION_RESOURCE,
        ProtoPlace.EconomyResource => PlaceType.ECONOMY_RESOURCE,
        ProtoPlace.Construction => PlaceType.CONSTRUCTION,
        ProtoPlace.Trap => PlaceType.TRAP,
        ProtoPlace.Home => PlaceType.HOME,
        ProtoPlace.NullPlaceType => PlaceType.NULL_PLACE_TYPE,
        _ => PlaceType.NULL_PLACE_TYPE
    };
    public static ProtoCharacterType CharacterTypeToProto(CharacterType charactertype) => charactertype switch
    {
        CharacterType.TangSeng => ProtoCharacterType.TangSeng,
        CharacterType.SunWukong => ProtoCharacterType.SunWukong,
        CharacterType.ZhuBajie => ProtoCharacterType.ZhuBajie,
        CharacterType.ShaWujing => ProtoCharacterType.ShaWujing,
        CharacterType.BaiLongma => ProtoCharacterType.BaiLongma,
        CharacterType.Monkid => ProtoCharacterType.Monkid,
        CharacterType.JiuLing => ProtoCharacterType.JiuLing,
        CharacterType.HongHaier => ProtoCharacterType.HongHaier,
        CharacterType.NiuMowang => ProtoCharacterType.NiuMowang,
        CharacterType.TieShan => ProtoCharacterType.TieShan,
        CharacterType.ZhiZhujing => ProtoCharacterType.ZhiZhujing,
        CharacterType.Pawn => ProtoCharacterType.Pawn,
        _ => ProtoCharacterType.NullCharacterType
    };
    public static CharacterType CharacterTypeFromProto(ProtoCharacterType charactertype) => charactertype switch
    {
        ProtoCharacterType.TangSeng => CharacterType.TangSeng,
        ProtoCharacterType.SunWukong => CharacterType.SunWukong,
        ProtoCharacterType.ZhuBajie => CharacterType.ZhuBajie,
        ProtoCharacterType.ShaWujing => CharacterType.ShaWujing,
        ProtoCharacterType.BaiLongma => CharacterType.BaiLongma,
        ProtoCharacterType.Monkid => CharacterType.Monkid,
        ProtoCharacterType.JiuLing => CharacterType.JiuLing,
        ProtoCharacterType.HongHaier => CharacterType.HongHaier,
        ProtoCharacterType.NiuMowang => CharacterType.NiuMowang,
        ProtoCharacterType.TieShan => CharacterType.TieShan,
        ProtoCharacterType.ZhiZhujing => CharacterType.ZhiZhujing,
        ProtoCharacterType.Pawn => CharacterType.Pawn,
        _ => CharacterType.Null
    };
    public static ProtoCharacterState CharacterStateToProto(CharacterState characterstate) => characterstate switch
    {
        CharacterState.NULL_CHARACTER_STATE => ProtoCharacterState.NullCharacterState,
        CharacterState.ATTACKING => ProtoCharacterState.Attacking,
        CharacterState.IDLE => ProtoCharacterState.Idle,
        CharacterState.BERSERK => ProtoCharacterState.Berserk,
        CharacterState.HARVESTING => ProtoCharacterState.Harvesting,
        CharacterState.HEALING => ProtoCharacterState.Healing,
        CharacterState.CONSTRUCTING => ProtoCharacterState.Constructing,
        CharacterState.SKILL_CASTING => ProtoCharacterState.SkillCasting,
        CharacterState.MOVING => ProtoCharacterState.Moving,
        CharacterState.BLIND => ProtoCharacterState.Blind,
        CharacterState.BURNED => ProtoCharacterState.Burned,
        CharacterState.INVISIBLE => ProtoCharacterState.Invisible,
        CharacterState.KNOCKED_BACK => ProtoCharacterState.KnockedBack,
        CharacterState.STUNNED => ProtoCharacterState.Stunned,
        CharacterState.DECEASED => ProtoCharacterState.Deceased,
        _ => ProtoCharacterState.NullCharacterState
    };
    public static CharacterState CharacterStateFromProto(ProtoCharacterState characterstate) => characterstate switch
    {
        ProtoCharacterState.NullCharacterState => CharacterState.NULL_CHARACTER_STATE,
        ProtoCharacterState.Attacking => CharacterState.ATTACKING,
        ProtoCharacterState.Idle => CharacterState.IDLE,
        ProtoCharacterState.Berserk => CharacterState.BERSERK,
        ProtoCharacterState.Harvesting => CharacterState.HARVESTING,
        ProtoCharacterState.Healing => CharacterState.HEALING,
        ProtoCharacterState.Constructing => CharacterState.CONSTRUCTING,
        ProtoCharacterState.SkillCasting => CharacterState.SKILL_CASTING,
        ProtoCharacterState.Moving => CharacterState.MOVING,
        ProtoCharacterState.Blind => CharacterState.BLIND,
        ProtoCharacterState.Burned => CharacterState.BURNED,
        ProtoCharacterState.Invisible => CharacterState.INVISIBLE,
        ProtoCharacterState.KnockedBack => CharacterState.KNOCKED_BACK,
        ProtoCharacterState.Stunned => CharacterState.STUNNED,
        ProtoCharacterState.Deceased => CharacterState.DECEASED,
        _ => CharacterState.NULL_CHARACTER_STATE
    };
    public static ProtoEconomyResourceState EconomyResourceStateToProto(EconomyResourceState ERstate) => ERstate switch
    {
        EconomyResourceState.HARVESTABLE => ProtoEconomyResourceState.Harvestable,
        EconomyResourceState.BEING_HARVESTED => ProtoEconomyResourceState.BeingHarvested,
        EconomyResourceState.HARVESTED => ProtoEconomyResourceState.Harvested,
        EconomyResourceState.NULL_ECONOMY_RESOURCE_STATE => ProtoEconomyResourceState.NullEconomyResourceStste,
        _ => ProtoEconomyResourceState.NullEconomyResourceStste
    };
    public static EconomyResourceState EconomyResourceStateFromProto(ProtoEconomyResourceState ERstate) => ERstate switch
    {
        ProtoEconomyResourceState.Harvestable => EconomyResourceState.HARVESTABLE,
        ProtoEconomyResourceState.BeingHarvested => EconomyResourceState.BEING_HARVESTED,
        ProtoEconomyResourceState.Harvested => EconomyResourceState.HARVESTED,
        ProtoEconomyResourceState.NullEconomyResourceStste => EconomyResourceState.NULL_ECONOMY_RESOURCE_STATE,
        _ => EconomyResourceState.NULL_ECONOMY_RESOURCE_STATE
    };
    public static ProtoEconomyResourceType EconomyResourceTypeToProto(EconomyResourceType ERtype) => ERtype switch
    {
        EconomyResourceType.NULL_ECONOMY_RESOURCE_TYPE => ProtoEconomyResourceType.NullEconomyResourceType,
        EconomyResourceType.SMALL_ECONOMY_RESOURCE => ProtoEconomyResourceType.SmallEconomyResource,
        EconomyResourceType.MEDIUM_ECONOMY_RESOURCE => ProtoEconomyResourceType.MediumEconomyResource,
        EconomyResourceType.LARGE_ECONOMY_RESOURCE => ProtoEconomyResourceType.LargeEconomyResource,
        _ => ProtoEconomyResourceType.NullEconomyResourceType
    };
    public static EconomyResourceType EconomyResourceTypeFromProto(ProtoEconomyResourceType ERtype) => ERtype switch
    {
        ProtoEconomyResourceType.NullEconomyResourceType => EconomyResourceType.NULL_ECONOMY_RESOURCE_TYPE,
        ProtoEconomyResourceType.SmallEconomyResource => EconomyResourceType.SMALL_ECONOMY_RESOURCE,
        ProtoEconomyResourceType.MediumEconomyResource => EconomyResourceType.MEDIUM_ECONOMY_RESOURCE,
        ProtoEconomyResourceType.LargeEconomyResource => EconomyResourceType.LARGE_ECONOMY_RESOURCE,
        _ => EconomyResourceType.NULL_ECONOMY_RESOURCE_TYPE
    };
    public static ProtoEquipmentType EquipmentTypeToProto(EquipmentType equipmenttype) => equipmenttype switch
    {
        EquipmentType.NULL_EQUIPMENT_TYPE => ProtoEquipmentType.NullEquipmentType,
        EquipmentType.SMALL_HEALTH_POTION => ProtoEquipmentType.SmallHealthPotion,
        EquipmentType.MEDIUM_HEALTH_POTION => ProtoEquipmentType.MediumHealthPotion,
        EquipmentType.LARGE_HEALTH_POTION => ProtoEquipmentType.LargeHealthPotion,
        EquipmentType.SMALL_SHIELD => ProtoEquipmentType.SmallShield,
        EquipmentType.LARGE_SHIELD => ProtoEquipmentType.LargeShield,
        EquipmentType.MEDIUM_SHIELD => ProtoEquipmentType.MediumShield,
        EquipmentType.SPEEDBOOTS => ProtoEquipmentType.Speedboots,
        EquipmentType.PURIFICATION_POTION => ProtoEquipmentType.PurificationPotion,
        EquipmentType.INVISIBILITY_POTION => ProtoEquipmentType.InvisibilityPotion,
        EquipmentType.BERSERK_POTION => ProtoEquipmentType.BerserkPotion,
        _ => ProtoEquipmentType.NullEquipmentType
    };
    public static EquipmentType EquipmentTypeFromProto(ProtoEquipmentType equipmenttype) => equipmenttype switch
    {
        ProtoEquipmentType.NullEquipmentType => EquipmentType.NULL_EQUIPMENT_TYPE,
        ProtoEquipmentType.SmallHealthPotion => EquipmentType.SMALL_HEALTH_POTION,
        ProtoEquipmentType.MediumHealthPotion => EquipmentType.MEDIUM_HEALTH_POTION,
        ProtoEquipmentType.LargeHealthPotion => EquipmentType.LARGE_HEALTH_POTION,
        ProtoEquipmentType.SmallShield => EquipmentType.SMALL_SHIELD,
        ProtoEquipmentType.LargeShield => EquipmentType.LARGE_SHIELD,
        ProtoEquipmentType.MediumShield => EquipmentType.MEDIUM_SHIELD,
        ProtoEquipmentType.Speedboots => EquipmentType.SPEEDBOOTS,
        ProtoEquipmentType.PurificationPotion => EquipmentType.PURIFICATION_POTION,
        ProtoEquipmentType.InvisibilityPotion => EquipmentType.INVISIBILITY_POTION,
        ProtoEquipmentType.BerserkPotion => EquipmentType.BERSERK_POTION,
        _ => EquipmentType.NULL_EQUIPMENT_TYPE
    };
    public static ProtoConstructionType ConstructionToProto(ConstructionType constructiontype) => constructiontype switch
    {
        ConstructionType.NULL_CONSTRUCTION_TYPE => ProtoConstructionType.NullConstructionType,
        ConstructionType.FARM => ProtoConstructionType.Farm,
        ConstructionType.SPRING => ProtoConstructionType.Spring,
        ConstructionType.BARRACKS => ProtoConstructionType.Barracks,
        _ => ProtoConstructionType.NullConstructionType
    };
    public static ConstructionType ConstructionFromProto(ProtoConstructionType constructiontype) => constructiontype switch
    {
        ProtoConstructionType.NullConstructionType => ConstructionType.NULL_CONSTRUCTION_TYPE,
        ProtoConstructionType.Farm => ConstructionType.FARM,
        ProtoConstructionType.Spring => ConstructionType.SPRING,
        ProtoConstructionType.Barracks => ConstructionType.BARRACKS,
        _ => ConstructionType.NULL_CONSTRUCTION_TYPE
    };
    public static ProtoTrapType TrapTypeToProto(ConstructionType traptype) => traptype switch
    {
        ConstructionType.HOLE => ProtoTrapType.Hole,
        ConstructionType.CAGE => ProtoTrapType.Cage,
        _ => ProtoTrapType.NullTrapType
    };
    public static ConstructionType TrapTypeFromProto(ProtoTrapType traptype) => traptype switch
    {
        ProtoTrapType.Hole => ConstructionType.HOLE,
        ProtoTrapType.Cage => ConstructionType.CAGE,
        _ => ConstructionType.NULL_CONSTRUCTION_TYPE
    };
    public static ProtoAdditionalResourceType AResourceToProto(A_ResourceType ARtype) => ARtype switch
    {
        A_ResourceType.NULL => ProtoAdditionalResourceType.NullAdditionResourceType,
        A_ResourceType.LIFE_POOL1 => ProtoAdditionalResourceType.LifePool1,
        A_ResourceType.LIFE_POOL2 => ProtoAdditionalResourceType.LifePool2,
        A_ResourceType.LIFE_POOL3 => ProtoAdditionalResourceType.LifePool3,
        A_ResourceType.CRAZY_MAN1 => ProtoAdditionalResourceType.CrazyMan1,
        A_ResourceType.CRAZY_MAN2 => ProtoAdditionalResourceType.CrazyMan2,
        A_ResourceType.CRAZY_MAN3 => ProtoAdditionalResourceType.CrazyMan3,
        A_ResourceType.QUICK_STEP => ProtoAdditionalResourceType.QuickStep,
        A_ResourceType.WIDE_VIEW => ProtoAdditionalResourceType.WideView,
        _ => ProtoAdditionalResourceType.NullAdditionResourceType
    };
    public static A_ResourceType AResourceFromProto(ProtoAdditionalResourceType ARtype) => ARtype switch
    {
        ProtoAdditionalResourceType.NullAdditionResourceType => A_ResourceType.NULL,
        ProtoAdditionalResourceType.LifePool1 => A_ResourceType.LIFE_POOL1,
        ProtoAdditionalResourceType.LifePool2 => A_ResourceType.LIFE_POOL2,
        ProtoAdditionalResourceType.LifePool3 => A_ResourceType.LIFE_POOL3,
        ProtoAdditionalResourceType.CrazyMan1 => A_ResourceType.CRAZY_MAN1,
        ProtoAdditionalResourceType.CrazyMan2 => A_ResourceType.CRAZY_MAN2,
        ProtoAdditionalResourceType.CrazyMan3 => A_ResourceType.CRAZY_MAN3,
        ProtoAdditionalResourceType.QuickStep => A_ResourceType.QUICK_STEP,
        ProtoAdditionalResourceType.WideView => A_ResourceType.WIDE_VIEW,
        _ => A_ResourceType.NULL
    };
    public static ProtoAdditionalResourceState AResourceStateToProto(AdditionResourceState State) => State switch
    {
        AdditionResourceState.NULL_ADDITION_RESOURCE_STATE => ProtoAdditionalResourceState.NullAdditionResourceState,
        AdditionResourceState.BEATABLE => ProtoAdditionalResourceState.Beatable,
        AdditionResourceState.BEING_BEATEN => ProtoAdditionalResourceState.BeingBeaten,
        AdditionResourceState.BEATEN => ProtoAdditionalResourceState.Beaten,
        _ => ProtoAdditionalResourceState.NullAdditionResourceState
    };
    public static AdditionResourceState AResourceStateFromProto(ProtoAdditionalResourceState State) => State switch
    {
        ProtoAdditionalResourceState.NullAdditionResourceState => AdditionResourceState.NULL_ADDITION_RESOURCE_STATE,
        ProtoAdditionalResourceState.Beatable => AdditionResourceState.BEATABLE,
        ProtoAdditionalResourceState.BeingBeaten => AdditionResourceState.BEING_BEATEN,
        ProtoAdditionalResourceState.Beaten => AdditionResourceState.BEATEN,
        _ => AdditionResourceState.NULL_ADDITION_RESOURCE_STATE
    };
}