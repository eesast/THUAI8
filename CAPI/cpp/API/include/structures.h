#pragma once
#ifndef STRUCTURES_H
#define STRUCTURES_H
#define FMT_ENABLE_ENUM_IMPLICIT
#include <cstdint>
#include <array>
#include <map>
#include <vector>
#include <string>
#include <fmt/format.h>
#undef GetMessage
#undef SendMessage
#undef PeekMessage

namespace THUAI8
{
    
    // 游戏状态
    enum class GameState : unsigned char
    {
        NullGameState = 0,
        GameStart = 1,
        GameRunning = 2,
        GameEnd = 3,
    };
    // 所有NullXXXType均为错误类型，其余为可能出现的正常类型

    // 位置标志
    enum class PlaceType : unsigned char
    {
        NullPlaceType = 0,
        Home = 1,
        Space = 2,
        Barrier = 3,
        Bush = 4,
        EconomyResource = 5,
        AdditionResource = 6,
        Construction = 7,
        Trap = 8,
    };

    // 形状标志
    enum class ShapeType : unsigned char
    {
        NullShapeType = 0,
        Circle = 1,
        Square = 2,
    };

    enum class PlayerTeam : unsigned char
    {
        NullTeam = 0,
        BuddhistsTeam = 1,
        MonstersTeam = 2,
    };

    enum class PlayerType : unsigned char
    {
        NullPlayerType = 0,
        Character = 1,
        Team = 2,
    };

    enum class CharacterType : unsigned char
    {
        NullCharacterType = 0,

        TangSeng = 1,
        SunWukong = 2,
        ZhuBajie = 3,
        ShaWujing = 4,
        BaiLongma = 5,
        Monkid = 6,

        JiuLing = 7,
        HongHaier = 8,
        NiuMowang = 9,
        TieShan = 10,
        ZhiZhujing = 11,
        Pawn = 12,
    };


    enum class EquipmentType : unsigned char
    {
        NullEquipmentType = 0,

        SmallHealthPotion = 1,
        MediumHealthPotion = 2,
        LargeHealthPotion = 3,

        SmallShield = 4,
        MediumShield = 5,
        LargeShield = 6,

        Speedboots = 7,
        PurificationPotion = 8,
        InvisibilityPotion = 9,
        BerserkPotion = 10,
    };

    enum class CharacterState : unsigned char
    {
        NullCharacterState = 0,

        Idle = 1,
        Harvesting = 2,
        Attacking = 3,
        SkillCasting = 4,
        Constructing = 5,
        Moving = 6,

        Blind = 7,
        KnockedBack = 8,
        Stunned = 9,
        Invisible = 10,
        Healing = 11,
        Berserk = 12,
        Burned = 13,
    };

    enum class CharacterBuffType : unsigned char
    {
        NullCharacterBuffType = 0,

        AttackBuff1 = 1,
        AttackBuff2 = 2,
        AttackBuff3 = 3,
        DefenseBuff = 4,
        SpeedBuff = 5,
        VisionBuff = 6,
    };

    enum class EconomyResourceType : unsigned char
    {
        NullEconomyResourceType = 0,

        SmallEconomyResource = 1,
        MediumEconomyResource = 2,
        LargeEconomyResource = 3,
    };

    enum class AdditionResourceType : unsigned char
    {
        NullAdditionReourceType = 0,

        LIFE_POOL1 = 1,
        LIFE_POOL2 = 2,
        LIFE_POOL3 = 3,

        CRAZY_MAN1 = 4,
        CRAZY_MAN2 = 5,
        CRAZY_MAN3 = 6,

        QUICK_STEP = 7,

        WIDE_VIEW = 8,
    };

    enum class EconomyResourceState : unsigned char
    {
        NullEconomyResourceState = 0,
        Harvestable = 1,
        BeingHarvested = 2,
        Harvested = 3,
    };
    enum class AdditionResourceState
    {
        NullAdditionResourceState = 0,
        Beatable = 1,
        BeingBeaten = 2,
        Beaten = 3,
    };

    enum class ConstructionType : unsigned char
    {
        NullConstructionType = 0,
        Barracks = 1,
        Spring = 2,
        Farm = 3,
    };

    enum class TrapType
    {
        NullTrapType = 0,
        Hole = 1,
        Cage = 2,
    };

    enum class MessageOfObj : unsigned char
    {
        NullMessageOfObj = 0,
        CharacterMessage = 1,
        BarracksMessage = 2,
        SpringMessage = 3,
        FarmMessage = 4,
        TrapMessage = 5,
        EconomyResourceMessage = 6,
        AdditionResourceMessage = 7,
        MapMessage = 8,
        NewsMessage = 9,
        TeamMessage = 10,
    };

    enum class NewsType : unsigned char
    {
        NullNewsType = 0,
        TextMessage = 1,
        BinaryMessage = 2,
    };

    struct Character
    {
        int64_t guid;

        int64_t teamID;
        int64_t playerID;

        CharacterType characterType;

        CharacterState characterActiveState;

        bool isBlind;
        long blindTime;
        bool isStunned;
        long stunnedTime;
        bool isInvisible;
        long invisibleTime;
        bool isBurned;
        long burnedTime;
        double harmCut;
        long harmCutTime;

        CharacterState characterPassiveState;

        int32_t x;
        int32_t y;

        double facingDirection;
        int32_t speed;
        int32_t viewRange;

        int32_t commonAttack;
        int64_t commonAttackCD;
        int32_t commonAttackRange;

        int64_t skillAttackCD;

        int32_t economyDepletion;
        int32_t killScore;

        int32_t hp;

        int32_t shieldEquipment;
        // int32_t shild;
        int32_t shoesEquipment;
        int64_t shoesTime;
        bool isPurified;
        int64_t purifiedTime;
        bool isBerserk;
        int64_t berserkTime;

        int32_t attackBuffNum;
        int64_t attackBuffTime;
        int64_t speedBuffTime;
        int64_t visionBuffTime;
    };

    struct Team
    {
        int64_t teamID;
        int64_t playerID;
        int64_t score;
        int64_t energy;
    };

    struct Trap
    {
        TrapType trapType;

        int32_t x;
        int32_t y;

        int64_t teamID;
        int32_t id;
    };

    struct EconomyResource
    {
        EconomyResourceType economyResourceType;
        EconomyResourceState economyResourceState;

        int32_t x;
        int32_t y;

        int32_t process;

        int32_t id;
    };

    struct AdditionResource
    {
        AdditionResourceType additionResourceType;
        AdditionResourceState additionResourceState;

        int32_t x;
        int32_t y;

        int32_t hp;

        int32_t id;
    };

    /* struct ConstructionState
    {
        int64_t teamID;
        int32_t hp;
        ConstructionType constructionType;
        ConstructionState(std::pair<int64_t, int32_t> teamHP, ConstructionType type) :
            teamID(teamHP.first),
            hp(teamHP.second),
            constructionType(type)
        {
        }
    };*/

    // struct BombedBullet
    // {
    //     BulletType bulletType,
    //     int32_t x,
    //     int32_t y,
    //     double facingDirection,
    //     int64_t mappingID,
    //     double bombRange,
    // };

    using cellxy_t = std::pair<int32_t, int32_t>;

    struct GameMap
    {
        // x,y,id,hp
        std::map<cellxy_t, std::pair<int32_t, int32_t>> barracksState;
        std::map<cellxy_t, std::pair<int32_t, int32_t>> springState;
        std::map<cellxy_t, std::pair<int32_t, int32_t>> farmState;
        std::map<cellxy_t, std::pair<int32_t, int32_t>> trapState;
        std::map<cellxy_t, int32_t> economyResource;
        std::map<cellxy_t, std::pair<int32_t, int32_t>> additionResource;
    };

    struct GameInfo
    {
        int32_t gameTime;
        int32_t buddhistsTeamScore;
        int32_t buddhistsTeamEconomy;
        int32_t buddhistsHeroHP;
        int32_t monstersTeamScore;
        int32_t monstersTeamEconomy;
        int32_t monstersHeroHP;
    };

    // 仅供DEBUG使用，名称可改动
    // 还没写完，后面待续

    inline std::map<GameState, std::string> gameStateDict{
        {GameState::NullGameState, "NullGameState"},
        {GameState::GameStart, "GameStart"},
        {GameState::GameRunning, "GameRunning"},
        {GameState::GameEnd, "GameEnd"},
    };

    inline std::map<CharacterType, std::string> characterTypeDict{
        {CharacterType::NullCharacterType, "NullCharacterType"},
        {CharacterType::TangSeng, "TangSeng"},
        {CharacterType::SunWukong, "SunWukong"},
        {CharacterType::ZhuBajie, "ZhuBajie"},
        {CharacterType::ShaWujing, "ShaWujing"},
        {CharacterType::BaiLongma, "BaiLongma"},
        {CharacterType::Monkid, "Monkid"},
        {CharacterType::JiuLing, "JiuLing"},
        {CharacterType::HongHaier, "HongHaier"},
        {CharacterType::NiuMowang, "NiuMowang"},
        {CharacterType::TieShan, "TieShan"},
        {CharacterType::ZhiZhujing, "ZhiZhujing"},
        {CharacterType::Pawn, "Pawn"},
    };

    inline std::map<CharacterState, std::string> characterStateDict{
        {CharacterState::NullCharacterState, "NullCharacterState"},
        {CharacterState::Idle, "Idle"},
        {CharacterState::Harvesting, "Harvesting"},
        {CharacterState::Attacking, "Attacking"},
        {CharacterState::SkillCasting, "SkillCasting"},
        {CharacterState::Constructing, "Constructing"},
        {CharacterState::Moving, "Moving"},
        {CharacterState::Blind, "Blind"},
        {CharacterState::KnockedBack, "KnockedBack"},
        {CharacterState::Stunned, "Stunned"},
        {CharacterState::Invisible, "Invisible"},
        {CharacterState::Healing, "Healing"},
        {CharacterState::Berserk, "Berserk"},
        {CharacterState::Burned, "Burned"},
    };

    inline std::map<PlayerTeam, std::string> playerTeamDict{
        {PlayerTeam::NullTeam, "NullTeam"},
        {PlayerTeam::BuddhistsTeam, "BuddhistsTeam"},
        {PlayerTeam::MonstersTeam, "MonstersTeam"},
    };

    inline std::map<PlaceType, std::string> placeTypeDict{
        {PlaceType::NullPlaceType, "NullPlaceType"},
        {PlaceType::Home, "Home"},
        {PlaceType::Space, "Space"},
        {PlaceType::Barrier, "Barrier"},
        {PlaceType::Bush, "Bush"},
        {PlaceType::EconomyResource, "EconomyResource"},
        {PlaceType::AdditionResource, "AdditionResource"},
        {PlaceType::Construction, "Construction"},
        {PlaceType::Trap, "Trap"},
    };

    inline std::map<EquipmentType, std::string> equipmentTypeDict{
        {EquipmentType::NullEquipmentType, "NullEquipmentType"},
        {EquipmentType::SmallHealthPotion, "SmallHealthPotion"},
        {EquipmentType::MediumHealthPotion, "MediumHealthPotion"},
        {EquipmentType::LargeHealthPotion, "LargeHealthPotion"},
        {EquipmentType::SmallShield, "SmallShield"},
        {EquipmentType::MediumShield, "MediumShield"},
        {EquipmentType::LargeShield, "LargeShield"},
        {EquipmentType::Speedboots, "Speedboots"},
        {EquipmentType::PurificationPotion, "PurificationPotion"},
        {EquipmentType::InvisibilityPotion, "InvisibilityPotion"},
        {EquipmentType::BerserkPotion, "BerserkPotion"},
    };

    inline std::map<ConstructionType, std::string> constructionDict{
        {ConstructionType::NullConstructionType, "NullConstructionType"},
        {ConstructionType::Barracks, "Barracks"},
        {ConstructionType::Spring, "Spring"},
        {ConstructionType::Farm, "Farm"},
    };

    inline std::map<EconomyResourceType, std::string> economyResourceTypeDict{
        {EconomyResourceType::NullEconomyResourceType, "NullEconomyResourceType"},
        {EconomyResourceType::SmallEconomyResource, "SmallEconomyResource"},
        {EconomyResourceType::MediumEconomyResource, "MediumEconomyResource"},
        {EconomyResourceType::LargeEconomyResource, "LargeEconomyResource"},
    };

    inline std::map<AdditionResourceType, std::string> additionResourceTypeDict{
        {AdditionResourceType::NullAdditionReourceType, "NullAdditionReourceType"},
        {AdditionResourceType::LIFE_POOL1, "LIFE_POOL1"},
        {AdditionResourceType::LIFE_POOL2, "MediumAdditionResource1"},
        {AdditionResourceType::LIFE_POOL3, "LargeAdditionResource1"},
        {AdditionResourceType::CRAZY_MAN1, "SmallAdditionResource2"},
        {AdditionResourceType::CRAZY_MAN2, "MediumAdditionResource2"},
        {AdditionResourceType::CRAZY_MAN3, "LargeAdditionResource2"},
        {AdditionResourceType::QUICK_STEP, "AdditionResource3"},
        {AdditionResourceType::WIDE_VIEW, "AdditionResource4"},
    };

    inline std::map<EconomyResourceState, std::string> economyResourceStateDict{
        {EconomyResourceState::NullEconomyResourceState, "NullEconomyResourceState"},
        {EconomyResourceState::Harvestable, "Harvestable"},
        {EconomyResourceState::BeingHarvested, "BeingHarvested"},
        {EconomyResourceState::Harvested, "Harvested"},
    };

    inline std::map<AdditionResourceState, std::string> additionResourceStateDict{
        {AdditionResourceState::NullAdditionResourceState, "NullAdditionReourceType"},
        {AdditionResourceState::Beatable, "Beatable"},
        {AdditionResourceState::BeingBeaten, "BeingBeaten"},
        {AdditionResourceState::Beaten, "Beaten"},
    };

    inline std::map<TrapType, std::string> trapTypeDict{
        {TrapType::NullTrapType, "NullTrapType"},
        {TrapType::Hole, "Hole"},
        {TrapType::Cage, "Cage"},
    };

    inline std::map<MessageOfObj, std::string> messageOfObjDict{
        {MessageOfObj::NullMessageOfObj, "NullMessageOfObj"},
        {MessageOfObj::CharacterMessage, "CharacterMessage"},
        {MessageOfObj::BarracksMessage, "BarracksMessage"},
        {MessageOfObj::SpringMessage, "SpringMessage"},
        {MessageOfObj::FarmMessage, "FarmMessage"},
        {MessageOfObj::TrapMessage, "TrapMessage"},
        {MessageOfObj::EconomyResourceMessage, "EconomyResourceMessage"},
        {MessageOfObj::AdditionResourceMessage, "AdditionResourceMessage"},
        {MessageOfObj::MapMessage, "MapMessage"},
        {MessageOfObj::NewsMessage, "NewsMessage"},
        {MessageOfObj::TeamMessage, "TeamMessage"},
    };

    inline std::map<NewsType, std::string> newsTypeDict{
        {NewsType::NullNewsType, "NullNewsType"},
        {NewsType::TextMessage, "TextMessage"},
        {NewsType::BinaryMessage, "BinaryMessage"},
    };
    
}  // namespace THUAI8

namespace fmt
{
    template<>
    struct formatter<THUAI8::CharacterType> : formatter<std::string>
    {
        auto format(THUAI8::CharacterType type, format_context& ctx) const
        {
            auto it = THUAI8::characterTypeDict.find(type);
            formatter<std::string> stringFormatter;
            return stringFormatter.format(
                it != THUAI8::characterTypeDict.end() ? it->second : "UnknownCharacterType", ctx
            );
        }
    };

    template<>
    struct formatter<THUAI8::ConstructionType> : formatter<std::string>
    {
        auto format(THUAI8::ConstructionType type, format_context& ctx) const
        {
            auto it = THUAI8::constructionDict.find(type);
            formatter<std::string> stringFormatter;
            return stringFormatter.format(
                it != THUAI8::constructionDict.end() ? it->second : "UnknownConstructionType", ctx
            );
        }
    };

    template<>
    struct formatter<THUAI8::EquipmentType> : formatter<std::string>
    {
        auto format(THUAI8::EquipmentType type, format_context& ctx) const
        {
            auto it = THUAI8::equipmentTypeDict.find(type);
            formatter<std::string> stringFormatter;
            return stringFormatter.format(
                it != THUAI8::equipmentTypeDict.end() ? it->second : "UnknownEquipmentType", ctx
            );
        }
    };
}  // namespace fmt


#endif