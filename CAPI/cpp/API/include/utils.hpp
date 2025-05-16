#pragma once
#ifndef UTILS_HPP
#define UTILS_HPP
#endif
#include <cstdint>
#include <cmath>
#include <map>
#include <vector>
#include <utility>
#include "Message2Clients.pb.h"
#include "Message2Server.pb.h"
#include "MessageType.pb.h"

#include "structures.h"

#undef GetMessage
#undef SendMessage
#undef PeekMessage

// 用于将THUAI8的类转换为Protobuf的类
namespace AssistFunction
{
    constexpr int32_t numOfGridPerCell = 1000;

    [[nodiscard]] constexpr inline int32_t GridToCell(int32_t grid) noexcept
    {
        return grid / numOfGridPerCell;
    }

    [[nodiscard]] constexpr inline int32_t GridToCell(double grid) noexcept
    {
        return int32_t(grid) / numOfGridPerCell;
    }

    inline bool HaveView(int32_t x, int32_t y, int32_t newX, int32_t newY, int32_t viewRange, std::vector<std::vector<THUAI8::PlaceType>>& map)
    {
        double deltaX = newX - x;
        double deltaY = newY - y;
        double distance = std::pow(deltaX, 2) + std::pow(deltaY, 2);
        THUAI8::PlaceType myPlace = map[GridToCell(x)][GridToCell(y)];
        THUAI8::PlaceType newPlace = map[GridToCell(newX)][GridToCell(newY)];
        if (newPlace == THUAI8::PlaceType::Bush && myPlace != THUAI8::PlaceType::Bush)
            return false;
        if (distance > std::pow(viewRange, 2))
            return false;
        int32_t divide = int32_t(std::max(std::abs(deltaX), std::abs(deltaY)) / 100);
        if (divide == 0)
            return true;
        double dx = deltaX / divide;
        double dy = deltaY / divide;
        double myX = double(x);
        double myY = double(y);
        if (newPlace == THUAI8::PlaceType::Bush && myPlace == THUAI8::PlaceType::Bush)
            for (int32_t i = 0; i < divide; i++)
            {
                myX += dx;
                myY += dy;
                if (map[GridToCell(myX)][GridToCell(myY)] != THUAI8::PlaceType::Bush)
                    return false;
            }
        else
            for (int32_t i = 0; i < divide; i++)
            {
                myX += dx;
                myY += dy;
                if (map[GridToCell(myX)][GridToCell(myY)] == THUAI8::PlaceType::Barrier)
                    return false;
            }
        return true;
    }
}  // namespace AssistFunction

namespace Proto2THUAI8
{
    // 用于将Protobuf中的枚举转换为THUAI8的枚举
    inline std::map<protobuf::GameState, THUAI8::GameState> gameStateDict{
        {protobuf::GameState::NULL_GAME_STATE, THUAI8::GameState::NullGameState},
        {protobuf::GameState::GAME_START, THUAI8::GameState::GameStart},
        {protobuf::GameState::GAME_RUNNING, THUAI8::GameState::GameRunning},
        {protobuf::GameState::GAME_END, THUAI8::GameState::GameEnd},
    };
    inline std::map<protobuf::MessageOfObj::MessageOfObjCase, THUAI8::MessageOfObj> messageOfObjDict{
        {protobuf::MessageOfObj::MessageOfObjCase::kCharacterMessage, THUAI8::MessageOfObj::CharacterMessage},
        {protobuf::MessageOfObj::MessageOfObjCase::kTeamMessage, THUAI8::MessageOfObj::TeamMessage},
        {protobuf::MessageOfObj::MessageOfObjCase::kBarracksMessage, THUAI8::MessageOfObj::BarracksMessage},
        {protobuf::MessageOfObj::MessageOfObjCase::kTrapMessage, THUAI8::MessageOfObj::TrapMessage},
        {protobuf::MessageOfObj::MessageOfObjCase::kEconomyResourceMessage, THUAI8::MessageOfObj::EconomyResourceMessage},
        {protobuf::MessageOfObj::MessageOfObjCase::kAdditionResourceMessage, THUAI8::MessageOfObj::AdditionResourceMessage},
        {protobuf::MessageOfObj::MessageOfObjCase::kSpringMessage, THUAI8::MessageOfObj::SpringMessage},
        {protobuf::MessageOfObj::MessageOfObjCase::kFarmMessage, THUAI8::MessageOfObj::FarmMessage},
        {protobuf::MessageOfObj::MessageOfObjCase::kMapMessage, THUAI8::MessageOfObj::MapMessage},
        {protobuf::MessageOfObj::MessageOfObjCase::kNewsMessage, THUAI8::MessageOfObj::NewsMessage},
    };

    inline std::map<protobuf::PlaceType, THUAI8::PlaceType> placeTypeDict{
        {protobuf::PlaceType::NULL_PLACE_TYPE, THUAI8::PlaceType::NullPlaceType},
        {protobuf::PlaceType::HOME, THUAI8::PlaceType::Home},
        {protobuf::PlaceType::SPACE, THUAI8::PlaceType::Space},
        {protobuf::PlaceType::BARRIER, THUAI8::PlaceType::Barrier},
        {protobuf::PlaceType::BUSH, THUAI8::PlaceType::Bush},
        {protobuf::PlaceType::ECONOMY_RESOURCE, THUAI8::PlaceType::EconomyResource},
        {protobuf::PlaceType::ADDITION_RESOURCE, THUAI8::PlaceType::AdditionResource},
        {protobuf::PlaceType::CONSTRUCTION, THUAI8::PlaceType::Construction},
        {protobuf::PlaceType::TRAP, THUAI8::PlaceType::Trap},
    };

    inline std::map<protobuf::ShapeType, THUAI8::ShapeType> shapeTypeDict{
        {protobuf::ShapeType::NULL_SHAPE_TYPE, THUAI8::ShapeType::NullShapeType},
        {protobuf::ShapeType::CIRCLE, THUAI8::ShapeType::Circle},
        {protobuf::ShapeType::SQUARE, THUAI8::ShapeType::Square},
    };

    inline std::map<protobuf::PlayerType, THUAI8::PlayerType> playerTypeDict{
        {protobuf::PlayerType::NULL_PLAYER_TYPE, THUAI8::PlayerType::NullPlayerType},
        {protobuf::PlayerType::CHARACTER, THUAI8::PlayerType::Character},
        {protobuf::PlayerType::TEAM, THUAI8::PlayerType::Team},
    };

    inline std::map<protobuf::CharacterType, THUAI8::CharacterType> characterTypeDict{
        {protobuf::CharacterType::NULL_CHARACTER_TYPE, THUAI8::CharacterType::NullCharacterType},
        {protobuf::CharacterType::TangSeng, THUAI8::CharacterType::TangSeng},
        {protobuf::CharacterType::SunWukong, THUAI8::CharacterType::SunWukong},
        {protobuf::CharacterType::ZhuBajie, THUAI8::CharacterType::ZhuBajie},
        {protobuf::CharacterType::ShaWujing, THUAI8::CharacterType::ShaWujing},
        {protobuf::CharacterType::BaiLongma, THUAI8::CharacterType::BaiLongma},
        {protobuf::CharacterType::Monkid, THUAI8::CharacterType::Monkid},
        {protobuf::CharacterType::JiuLing, THUAI8::CharacterType::JiuLing},
        {protobuf::CharacterType::HongHaier, THUAI8::CharacterType::HongHaier},
        {protobuf::CharacterType::NiuMowang, THUAI8::CharacterType::NiuMowang},
        {protobuf::CharacterType::TieShan, THUAI8::CharacterType::TieShan},
        {protobuf::CharacterType::ZhiZhujing, THUAI8::CharacterType::ZhiZhujing},
        {protobuf::CharacterType::Pawn, THUAI8::CharacterType::Pawn},
    };

    inline std::map<protobuf::CharacterState, THUAI8::CharacterState> characterStateDict{
        {protobuf::CharacterState::NULL_CHARACTER_STATE, THUAI8::CharacterState::NullCharacterState},
        {protobuf::CharacterState::IDLE, THUAI8::CharacterState::Idle},
        {protobuf::CharacterState::HARVESTING, THUAI8::CharacterState::Harvesting},
        {protobuf::CharacterState::ATTACKING, THUAI8::CharacterState::Attacking},
        {protobuf::CharacterState::SKILL_CASTING, THUAI8::CharacterState::SkillCasting},
        {protobuf::CharacterState::CONSTRUCTING, THUAI8::CharacterState::Constructing},
        {protobuf::CharacterState::MOVING, THUAI8::CharacterState::Moving},
        {protobuf::CharacterState::BLIND, THUAI8::CharacterState::Blind},
        {protobuf::CharacterState::KNOCKED_BACK, THUAI8::CharacterState::KnockedBack},
        {protobuf::CharacterState::STUNNED, THUAI8::CharacterState::Stunned},
        {protobuf::CharacterState::INVISIBLE, THUAI8::CharacterState::Invisible},
        {protobuf::CharacterState::HEALING, THUAI8::CharacterState::Healing},
        {protobuf::CharacterState::BERSERK, THUAI8::CharacterState::Berserk},
        {protobuf::CharacterState::BURNED, THUAI8::CharacterState::Burned},
        {protobuf::CharacterState::DECEASED, THUAI8::CharacterState::Deceased},
    };

    inline std::map<protobuf::EconomyResourceType, THUAI8::EconomyResourceType> economyResourceTypeDict{
        {protobuf::EconomyResourceType::NULL_ECONOMY_RESOURCE_TYPE, THUAI8::EconomyResourceType::NullEconomyResourceType},
        {protobuf::EconomyResourceType::SMALL_ECONOMY_RESOURCE, THUAI8::EconomyResourceType::SmallEconomyResource},
        {protobuf::EconomyResourceType::MEDIUM_ECONOMY_RESOURCE, THUAI8::EconomyResourceType::MediumEconomyResource},
        {protobuf::EconomyResourceType::LARGE_ECONOMY_RESOURCE, THUAI8::EconomyResourceType::LargeEconomyResource},
    };

    inline std::map<protobuf::AdditionResourceType, THUAI8::AdditionResourceType> additionResourceTypeDict{
        {protobuf::AdditionResourceType::NULL_ADDITION_RESOURCE_TYPE, THUAI8::AdditionResourceType::NullAdditionResourceType},
        {protobuf::AdditionResourceType::LIFE_POOL1, THUAI8::AdditionResourceType::LIFE_POOL1},
        {protobuf::AdditionResourceType::LIFE_POOL2, THUAI8::AdditionResourceType::LIFE_POOL2},
        {protobuf::AdditionResourceType::LIFE_POOL3, THUAI8::AdditionResourceType::LIFE_POOL3},
        {protobuf::AdditionResourceType::CRAZY_MAN1, THUAI8::AdditionResourceType::CRAZY_MAN1},
        {protobuf::AdditionResourceType::CRAZY_MAN2, THUAI8::AdditionResourceType::CRAZY_MAN2},
        {protobuf::AdditionResourceType::CRAZY_MAN3, THUAI8::AdditionResourceType::CRAZY_MAN3},
        {protobuf::AdditionResourceType::QUICK_STEP, THUAI8::AdditionResourceType::QUICK_STEP},
        {protobuf::AdditionResourceType::WIDE_VIEW, THUAI8::AdditionResourceType::WIDE_VIEW},
    };

    inline std::map<protobuf::EconomyResourceState, THUAI8::EconomyResourceState> economyResourceStateDict{
        {protobuf::EconomyResourceState::NULL_ECONOMY_RESOURCE_STSTE, THUAI8::EconomyResourceState::NullEconomyResourceState},
        {protobuf::EconomyResourceState::HARVESTABLE, THUAI8::EconomyResourceState::Harvestable},
        {protobuf::EconomyResourceState::BEING_HARVESTED, THUAI8::EconomyResourceState::BeingHarvested},
        {protobuf::EconomyResourceState::HARVESTED, THUAI8::EconomyResourceState::Harvested},
    };

    inline std::map<protobuf::AdditionResourceState, THUAI8::AdditionResourceState> additionResourceStateDict{
        {protobuf::AdditionResourceState::NULL_ADDITION_RESOURCE_STATE, THUAI8::AdditionResourceState::NullAdditionResourceState},
        {protobuf::AdditionResourceState::BEATABLE, THUAI8::AdditionResourceState::Beatable},
        {protobuf::AdditionResourceState::BEING_BEATEN, THUAI8::AdditionResourceState::BeingBeaten},
        {protobuf::AdditionResourceState::BEATEN, THUAI8::AdditionResourceState::Beaten},
    };

    inline std::map<protobuf::EquipmentType, THUAI8::EquipmentType> equipmentTypeDict{
        {protobuf::EquipmentType::NULL_EQUIPMENT_TYPE, THUAI8::EquipmentType::NullEquipmentType},
        {protobuf::EquipmentType::SMALL_HEALTH_POTION, THUAI8::EquipmentType::SmallHealthPotion},
        {protobuf::EquipmentType::MEDIUM_HEALTH_POTION, THUAI8::EquipmentType::MediumHealthPotion},
        {protobuf::EquipmentType::LARGE_HEALTH_POTION, THUAI8::EquipmentType::LargeHealthPotion},
        {protobuf::EquipmentType::SMALL_SHIELD, THUAI8::EquipmentType::SmallShield},
        {protobuf::EquipmentType::MEDIUM_SHIELD, THUAI8::EquipmentType::MediumShield},
        {protobuf::EquipmentType::LARGE_SHIELD, THUAI8::EquipmentType::LargeShield},
        {protobuf::EquipmentType::SPEEDBOOTS, THUAI8::EquipmentType::Speedboots},
        {protobuf::EquipmentType::PURIFICATION_POTION, THUAI8::EquipmentType::PurificationPotion},
        {protobuf::EquipmentType::INVISIBILITY_POTION, THUAI8::EquipmentType::InvisibilityPotion},
        {protobuf::EquipmentType::BERSERK_POTION, THUAI8::EquipmentType::BerserkPotion},
    };

    inline std::map<protobuf::ConstructionType, THUAI8::ConstructionType> constructionTypeDict{
        {protobuf::ConstructionType::NULL_CONSTRUCTION_TYPE, THUAI8::ConstructionType::NullConstructionType},
        {protobuf::ConstructionType::BARRACKS, THUAI8::ConstructionType::Barracks},
        {protobuf::ConstructionType::SPRING, THUAI8::ConstructionType::Spring},
        {protobuf::ConstructionType::FARM, THUAI8::ConstructionType::Farm},
    };

    inline std::map<protobuf::TrapType, THUAI8::TrapType> trapTypeDict{
        {protobuf::TrapType::NULL_TRAP_TYPE, THUAI8::TrapType::NullTrapType},
        {protobuf::TrapType::HOLE, THUAI8::TrapType::Hole},
        {protobuf::TrapType::CAGE, THUAI8::TrapType::Cage},
    };

    inline std::map<protobuf::MessageOfNews::NewsCase, THUAI8::NewsType> newsTypeDict{
        {protobuf::MessageOfNews::NewsCase::NEWS_NOT_SET, THUAI8::NewsType::NullNewsType},
        {protobuf::MessageOfNews::NewsCase::kTextMessage, THUAI8::NewsType::TextMessage},
        {protobuf::MessageOfNews::NewsCase::kBinaryMessage, THUAI8::NewsType::BinaryMessage},
    };

    inline std::map<protobuf::PlayerTeam, THUAI8::PlayerTeam> playerTeam{
        {protobuf::PlayerTeam::NULL_TEAM, THUAI8::PlayerTeam::NullTeam},
        {protobuf::PlayerTeam::BUDDHISTS_TEAM, THUAI8::PlayerTeam::BuddhistsTeam},
        {protobuf::PlayerTeam::MONSTERS_TEAM, THUAI8::PlayerTeam::MonstersTeam},
    };

    // 用于将Protobuf中的类转换为THUAI8的类
    inline std::shared_ptr<THUAI8::Character> Protobuf2THUAI8Character(const protobuf::MessageOfCharacter& CharacterMsg)
    {
        auto character = std::make_shared<THUAI8::Character>();
        // 直接访问MessageOfCharacter的字段
        character->guid = CharacterMsg.guid();
        character->teamID = CharacterMsg.team_id();
        character->playerID = CharacterMsg.player_id();
        character->characterType = characterTypeDict.at(CharacterMsg.character_type());
        character->characterActiveState = characterStateDict.at(CharacterMsg.character_active_state());

        character->isBlind = CharacterMsg.is_blind();
        character->blindTime = CharacterMsg.blind_time();
        character->isStunned = CharacterMsg.is_stunned();
        character->stunnedTime = CharacterMsg.stunned_time();
        character->isInvisible = CharacterMsg.is_invisible();
        character->invisibleTime = CharacterMsg.invisible_time();
        character->isBurned = CharacterMsg.is_burned();
        character->burnedTime = CharacterMsg.burned_time();

        character->harmCut = CharacterMsg.harm_cut();
        character->harmCutTime = CharacterMsg.harm_cut_time();
        character->characterPassiveState = characterStateDict.at(CharacterMsg.character_passive_state());

        character->x = CharacterMsg.x();
        character->y = CharacterMsg.y();
        character->facingDirection = CharacterMsg.facing_direction();
        character->speed = CharacterMsg.speed();
        character->viewRange = CharacterMsg.view_range();

        character->commonAttack = CharacterMsg.common_attack();
        character->commonAttackCD = CharacterMsg.common_attack_cd();
        character->commonAttackRange = CharacterMsg.common_attack_range();
        character->skillAttackCD = CharacterMsg.skill_attack_cd();

        character->economyDepletion = CharacterMsg.economy_depletion();
        character->killScore = CharacterMsg.kill_score();
        character->hp = CharacterMsg.hp();

        character->shieldEquipment = CharacterMsg.shield_equipment();
        character->shoesEquipment = CharacterMsg.shoes_equipment();
        character->shoesTime = CharacterMsg.shoes_time();

        character->isPurified = CharacterMsg.is_purified();
        character->purifiedTime = CharacterMsg.purified_time();
        character->isBerserk = CharacterMsg.is_berserk();
        character->berserkTime = CharacterMsg.berserk_time();

        character->attackBuffNum = CharacterMsg.attack_buff_num();
        character->attackBuffTime = CharacterMsg.attack_buff_time();
        character->speedBuffTime = CharacterMsg.speed_buff_time();
        character->visionBuffTime = CharacterMsg.vision_buff_time();

        return character;
    }

    inline std::shared_ptr<THUAI8::Team> Protobuf2THUAI8Team(const protobuf::MessageOfTeam& TeamMsg)
    {
        auto team = std::make_shared<THUAI8::Team>();
        team->teamID = TeamMsg.team_id();
        team->playerID = TeamMsg.player_id();
        team->score = TeamMsg.score();
        team->energy = TeamMsg.energy();
        return team;
    }

    inline std::shared_ptr<THUAI8::GameInfo> Protobuf2THUAI8GameInfo(const protobuf::MessageOfAll& GameInfoMsg)
    {
        auto gameInfo = std::make_shared<THUAI8::GameInfo>();
        gameInfo->gameTime = GameInfoMsg.game_time();
        gameInfo->buddhistsTeamScore = GameInfoMsg.buddhists_team_score();
        gameInfo->buddhistsTeamEconomy = GameInfoMsg.buddhists_team_economy();
        gameInfo->buddhistsHeroHP = GameInfoMsg.buddhists_hero_hp();
        gameInfo->monstersTeamScore = GameInfoMsg.monsters_team_score();
        gameInfo->monstersTeamEconomy = GameInfoMsg.monsters_team_economy();
        gameInfo->monstersHeroHP = GameInfoMsg.monsters_hero_hp();
        return gameInfo;
    }

    inline std::shared_ptr<THUAI8::Trap> Protobuf2THUAI8Trap(const protobuf::MessageOfTrap& TrapMsg)
    {
        auto trap = std::make_shared<THUAI8::Trap>();
        trap->trapType = trapTypeDict.at(TrapMsg.trap_type());
        trap->x = TrapMsg.x();
        trap->y = TrapMsg.y();
        trap->teamID = TrapMsg.team_id();
        trap->id = TrapMsg.id();
        return trap;
    }

    inline std::shared_ptr<THUAI8::EconomyResource> Protobuf2THUAI8EconomyResource(const protobuf::MessageOfEconomyResource& EconomyResourceMsg)
    {
        auto economyResource = std::make_shared<THUAI8::EconomyResource>();
        economyResource->economyResourceType = economyResourceTypeDict.at(EconomyResourceMsg.economy_resource_type());
        //economyResource->economyResourceState = economyResourceStateDict.at(EconomyResourceMsg.economy_resource_state());
        //economyResource->x = EconomyResourceMsg.x();
        //economyResource->y = EconomyResourceMsg.y();
        return economyResource;
    }

    inline std::shared_ptr<THUAI8::AdditionResource> Protobuf2THUAI8AdditionResource(const protobuf::MessageOfAdditionResource& AdditionResourceMsg)
    {
        auto additionResource = std::make_shared<THUAI8::AdditionResource>();
        additionResource->additionResourceType = additionResourceTypeDict.at(AdditionResourceMsg.addition_resource_type());
        //additionResource->additionResourceState = additionResourceStateDict.at(AdditionResourceMsg.addition_resource_state());
        //additionResource->x = AdditionResourceMsg.x();
        //additionResource->y = AdditionResourceMsg.y();
        return additionResource;
    }

    /* inline std::shared_ptr<THUAI8::ConstructionState> Protobuf2THUAI8ConstructionState(const protobuf::MessageOfConstructionState& ConstructionStateMsg)
    {
        auto constructionState = std::make_shared<THUAI8::ConstructionState>();
        constructionState->teamID = ConstructionStateMsg.constructionstate().teamid();
        constructionState->hp = ConstructionStateMsg.constructionstate().hp();
        constructionState->constructionType = constructionTypeDict.at(ConstructionStateMsg.constructionstate().constructiontype());
        return constructionState;
    }*/

    // ?
    // inline std::shared_ptr<THUAI8::GameMap> Protobuf2THUAI8GameMap(const protobuf::MessageOfGameMap& GameMapMsg)
    // {
    //     auto gameMap = std::make_shared<THUAI8::GameMap>();
    //     for (const auto& barracks : GameMapMsg.gamemap().barracksstate())
    //     {
    //         gameMap->barracksState[{barracks.first.x(), barracks.first.y()}] = {barracks.second.teamid(), barracks.second.hp()};
    //     }
    //     for (const auto& spring : GameMapMsg.gamemap().springstate())
    //     {
    //         gameMap->springState[{spring.first.x(), spring.first.y()}] = {spring.second.teamid(), spring.second.hp()};
    //     }
    //     for (const auto& farm : GameMapMsg.gamemap().farmstate())
    //     {
    //         gameMap->farmState[{farm.first.x(), farm.first.y()}] = {farm.second.teamid(), farm.second.hp()};
    //     }
    //     for (const auto& trap : GameMapMsg.gamemap().trapstate())
    //     {
    //         gameMap->trapState[{trap.first.x(), trap.first.y()}] = {trap.second.teamid(), trap.second.hp()};
    //     }
    //     for (const auto& economyResource : GameMapMsg.gamemap().economyresource())
    //     {
    //         gameMap->economyResource[{economyResource.first.x(), economyResource.first.y()}] = economyResource.second;
    //     }
    //     for (const auto& additionResource : GameMapMsg.gamemap().additionresource())
    //     {
    //         gameMap->additionResource[{additionResource.first.x(), additionResource.first.y()}] = additionResource.second;
    //     }
    //     return gameMap;
    // }
}  // namespace Proto2THUAI8
// 辅助函数，用于将proto信息转换为THUAI8的信息
namespace THUAI8Proto
{
    // 用于将THUAI8的枚举转换为Protobuf的枚举
    inline std::map<THUAI8::GameState, protobuf::GameState> gameStateDict{
        {THUAI8::GameState::NullGameState, protobuf::GameState::NULL_GAME_STATE},
        {THUAI8::GameState::GameStart, protobuf::GameState::GAME_START},
        {THUAI8::GameState::GameRunning, protobuf::GameState::GAME_RUNNING},
        {THUAI8::GameState::GameEnd, protobuf::GameState::GAME_END},
    };

    inline std::map<THUAI8::PlaceType, protobuf::PlaceType> placeTypeDict{
        {THUAI8::PlaceType::NullPlaceType, protobuf::PlaceType::NULL_PLACE_TYPE},
        {THUAI8::PlaceType::Home, protobuf::PlaceType::HOME},
        {THUAI8::PlaceType::Space, protobuf::PlaceType::SPACE},
        {THUAI8::PlaceType::Barrier, protobuf::PlaceType::BARRIER},
        {THUAI8::PlaceType::Bush, protobuf::PlaceType::BUSH},
        {THUAI8::PlaceType::EconomyResource, protobuf::PlaceType::ECONOMY_RESOURCE},
        {THUAI8::PlaceType::AdditionResource, protobuf::PlaceType::ADDITION_RESOURCE},
        {THUAI8::PlaceType::Construction, protobuf::PlaceType::CONSTRUCTION},
        {THUAI8::PlaceType::Trap, protobuf::PlaceType::TRAP},
    };

    inline std::map<THUAI8::ShapeType, protobuf::ShapeType> shapeTypeDict{
        {THUAI8::ShapeType::NullShapeType, protobuf::ShapeType::NULL_SHAPE_TYPE},
        {THUAI8::ShapeType::Circle, protobuf::ShapeType::CIRCLE},
        {THUAI8::ShapeType::Square, protobuf::ShapeType::SQUARE},
    };

    inline std::map<THUAI8::PlayerTeam, protobuf::PlayerTeam> playerTeamDict{
        {THUAI8::PlayerTeam::NullTeam, protobuf::PlayerTeam::NULL_TEAM},
        {THUAI8::PlayerTeam::BuddhistsTeam, protobuf::PlayerTeam::BUDDHISTS_TEAM},
        {THUAI8::PlayerTeam::MonstersTeam, protobuf::PlayerTeam::MONSTERS_TEAM},
    };

    inline std::map<THUAI8::CharacterType, protobuf::CharacterType> characterTypeDict{
        {THUAI8::CharacterType::NullCharacterType, protobuf::CharacterType::NULL_CHARACTER_TYPE},
        {THUAI8::CharacterType::TangSeng, protobuf::CharacterType::TangSeng},
        {THUAI8::CharacterType::SunWukong, protobuf::CharacterType::SunWukong},
        {THUAI8::CharacterType::ZhuBajie, protobuf::CharacterType::ZhuBajie},
        {THUAI8::CharacterType::ShaWujing, protobuf::CharacterType::ShaWujing},
        {THUAI8::CharacterType::BaiLongma, protobuf::CharacterType::BaiLongma},
        {THUAI8::CharacterType::Monkid, protobuf::CharacterType::Monkid},
        {THUAI8::CharacterType::JiuLing, protobuf::CharacterType::JiuLing},
        {THUAI8::CharacterType::HongHaier, protobuf::CharacterType::HongHaier},
        {THUAI8::CharacterType::NiuMowang, protobuf::CharacterType::NiuMowang},
        {THUAI8::CharacterType::TieShan, protobuf::CharacterType::TieShan},
        {THUAI8::CharacterType::ZhiZhujing, protobuf::CharacterType::ZhiZhujing},
        {THUAI8::CharacterType::Pawn, protobuf::CharacterType::Pawn},
    };

    inline std::map<THUAI8::EquipmentType, protobuf::EquipmentType> equipmentTypeDict{
        {THUAI8::EquipmentType::NullEquipmentType, protobuf::EquipmentType::NULL_EQUIPMENT_TYPE},
        {THUAI8::EquipmentType::SmallHealthPotion, protobuf::EquipmentType::SMALL_HEALTH_POTION},
        {THUAI8::EquipmentType::MediumHealthPotion, protobuf::EquipmentType::MEDIUM_HEALTH_POTION},
        {THUAI8::EquipmentType::LargeHealthPotion, protobuf::EquipmentType::LARGE_HEALTH_POTION},
        {THUAI8::EquipmentType::SmallShield, protobuf::EquipmentType::SMALL_SHIELD},
        {THUAI8::EquipmentType::MediumShield, protobuf::EquipmentType::MEDIUM_SHIELD},
        {THUAI8::EquipmentType::LargeShield, protobuf::EquipmentType::LARGE_SHIELD},
        {THUAI8::EquipmentType::Speedboots, protobuf::EquipmentType::SPEEDBOOTS},
        {THUAI8::EquipmentType::PurificationPotion, protobuf::EquipmentType::PURIFICATION_POTION},
        {THUAI8::EquipmentType::InvisibilityPotion, protobuf::EquipmentType::INVISIBILITY_POTION},
        {THUAI8::EquipmentType::BerserkPotion, protobuf::EquipmentType::BERSERK_POTION},
    };

    inline std::map<THUAI8::CharacterState, protobuf::CharacterState> characterStateDict{
        {THUAI8::CharacterState::NullCharacterState, protobuf::CharacterState::NULL_CHARACTER_STATE},
        {THUAI8::CharacterState::Idle, protobuf::CharacterState::IDLE},
        {THUAI8::CharacterState::Harvesting, protobuf::CharacterState::HARVESTING},
        {THUAI8::CharacterState::Attacking, protobuf::CharacterState::ATTACKING},
        {THUAI8::CharacterState::SkillCasting, protobuf::CharacterState::SKILL_CASTING},
        {THUAI8::CharacterState::Constructing, protobuf::CharacterState::CONSTRUCTING},
        {THUAI8::CharacterState::Moving, protobuf::CharacterState::MOVING},
        {THUAI8::CharacterState::Blind, protobuf::CharacterState::BLIND},
        {THUAI8::CharacterState::KnockedBack, protobuf::CharacterState::KNOCKED_BACK},
        {THUAI8::CharacterState::Stunned, protobuf::CharacterState::STUNNED},
        {THUAI8::CharacterState::Invisible, protobuf::CharacterState::INVISIBLE},
        {THUAI8::CharacterState::Healing, protobuf::CharacterState::HEALING},
        {THUAI8::CharacterState::Berserk, protobuf::CharacterState::BERSERK},
        {THUAI8::CharacterState::Burned, protobuf::CharacterState::BURNED},
        {THUAI8::CharacterState::Deceased, protobuf::CharacterState::DECEASED},
    };

    inline std::map<THUAI8::EconomyResourceType, protobuf::EconomyResourceType> economyResourceTypeDict{
        {THUAI8::EconomyResourceType::NullEconomyResourceType, protobuf::EconomyResourceType::NULL_ECONOMY_RESOURCE_TYPE},
        {THUAI8::EconomyResourceType::SmallEconomyResource, protobuf::EconomyResourceType::SMALL_ECONOMY_RESOURCE},
        {THUAI8::EconomyResourceType::MediumEconomyResource, protobuf::EconomyResourceType::MEDIUM_ECONOMY_RESOURCE},
        {THUAI8::EconomyResourceType::LargeEconomyResource, protobuf::EconomyResourceType::LARGE_ECONOMY_RESOURCE},
    };

    inline std::map<THUAI8::AdditionResourceType, protobuf::AdditionResourceType> additionResourceTypeDict{
        {THUAI8::AdditionResourceType::NullAdditionResourceType, protobuf::AdditionResourceType::NULL_ADDITION_RESOURCE_TYPE},
        {THUAI8::AdditionResourceType::LIFE_POOL1, protobuf::AdditionResourceType::LIFE_POOL1},
        {THUAI8::AdditionResourceType::LIFE_POOL2, protobuf::AdditionResourceType::LIFE_POOL2},
        {THUAI8::AdditionResourceType::LIFE_POOL3, protobuf::AdditionResourceType::LIFE_POOL3},
        {THUAI8::AdditionResourceType::CRAZY_MAN1, protobuf::AdditionResourceType::CRAZY_MAN1},
        {THUAI8::AdditionResourceType::CRAZY_MAN2, protobuf::AdditionResourceType::CRAZY_MAN2},
        {THUAI8::AdditionResourceType::CRAZY_MAN3, protobuf::AdditionResourceType::CRAZY_MAN3},
        {THUAI8::AdditionResourceType::QUICK_STEP, protobuf::AdditionResourceType::QUICK_STEP},
        {THUAI8::AdditionResourceType::WIDE_VIEW, protobuf::AdditionResourceType::WIDE_VIEW},
    };

    inline std::map<THUAI8::EconomyResourceState, protobuf::EconomyResourceState> economyResourceStateDict{
        {THUAI8::EconomyResourceState::NullEconomyResourceState, protobuf::EconomyResourceState::NULL_ECONOMY_RESOURCE_STSTE},
        {THUAI8::EconomyResourceState::Harvestable, protobuf::EconomyResourceState::HARVESTABLE},
        {THUAI8::EconomyResourceState::BeingHarvested, protobuf::EconomyResourceState::BEING_HARVESTED},
        {THUAI8::EconomyResourceState::Harvested, protobuf::EconomyResourceState::HARVESTED},
    };

    inline std::map<THUAI8::AdditionResourceState, protobuf::AdditionResourceState> additionResourceStateDict{
        {THUAI8::AdditionResourceState::NullAdditionResourceState, protobuf::AdditionResourceState::NULL_ADDITION_RESOURCE_STATE},
        {THUAI8::AdditionResourceState::Beatable, protobuf::AdditionResourceState::BEATABLE},
        {THUAI8::AdditionResourceState::BeingBeaten, protobuf::AdditionResourceState::BEING_BEATEN},
        {THUAI8::AdditionResourceState::Beaten, protobuf::AdditionResourceState::BEATEN},
    };

    inline std::map<THUAI8::ConstructionType, protobuf::ConstructionType> constructionTypeDict{
        {THUAI8::ConstructionType::NullConstructionType, protobuf::ConstructionType::NULL_CONSTRUCTION_TYPE},
        {THUAI8::ConstructionType::Barracks, protobuf::ConstructionType::BARRACKS},
        {THUAI8::ConstructionType::Spring, protobuf::ConstructionType::SPRING},
        {THUAI8::ConstructionType::Farm, protobuf::ConstructionType::FARM},
    };

    inline std::map<THUAI8::TrapType, protobuf::TrapType> trapTypeDict{
        {THUAI8::TrapType::NullTrapType, protobuf::TrapType::NULL_TRAP_TYPE},
        {THUAI8::TrapType::Hole, protobuf::TrapType::HOLE},
        {THUAI8::TrapType::Cage, protobuf::TrapType::CAGE},
    };

    inline std::map<THUAI8::NewsType, protobuf::NewsType> newsTypeDict{
        {THUAI8::NewsType::NullNewsType, protobuf::NewsType::NULL_NEWS_TYPE},
        {THUAI8::NewsType::TextMessage, protobuf::NewsType::TEXT},
        {THUAI8::NewsType::BinaryMessage, protobuf::NewsType::BINARY},
    };

    inline std::map<THUAI8::MessageOfObj, protobuf::MessageOfObj::MessageOfObjCase> messageOfObjDict{
        {THUAI8::MessageOfObj::CharacterMessage, protobuf::MessageOfObj::MessageOfObjCase::kCharacterMessage},
        {THUAI8::MessageOfObj::BarracksMessage, protobuf::MessageOfObj::MessageOfObjCase::kBarracksMessage},
        {THUAI8::MessageOfObj::SpringMessage, protobuf::MessageOfObj::MessageOfObjCase::kSpringMessage},
        {THUAI8::MessageOfObj::FarmMessage, protobuf::MessageOfObj::MessageOfObjCase::kFarmMessage},
        {THUAI8::MessageOfObj::TrapMessage, protobuf::MessageOfObj::MessageOfObjCase::kTrapMessage},
        {THUAI8::MessageOfObj::EconomyResourceMessage, protobuf::MessageOfObj::MessageOfObjCase::kEconomyResourceMessage},
        {THUAI8::MessageOfObj::AdditionResourceMessage, protobuf::MessageOfObj::MessageOfObjCase::kAdditionResourceMessage},
        {THUAI8::MessageOfObj::MapMessage, protobuf::MessageOfObj::MessageOfObjCase::kMapMessage},
        {THUAI8::MessageOfObj::TeamMessage, protobuf::MessageOfObj::MessageOfObjCase::kNewsMessage},
        {THUAI8::MessageOfObj::NewsMessage, protobuf::MessageOfObj::MessageOfObjCase::kTeamMessage},
    };

    inline protobuf::MoveMsg THUAI82ProtobufMoveMsg(int64_t team, int64_t character, int64_t time, double angle)
    {
        protobuf::MoveMsg moveMsg;
        moveMsg.set_character_id(character);
        moveMsg.set_angle(angle);
        moveMsg.set_time_in_milliseconds(time);
        moveMsg.set_team_id(team);
        return moveMsg;
    }

    inline protobuf::IDMsg THUAI82ProtobufIDMsg(int32_t playerID, int32_t teamID)
    {
        protobuf::IDMsg IDMsg;
        IDMsg.set_character_id(playerID);
        IDMsg.set_team_id(teamID);
        return IDMsg;
    }

    inline protobuf::SendMsg THUAI82ProtobufSendMsg(int32_t playerID, int32_t toPlayerID, int32_t teamID, std::string msg, bool binary)
    {
        protobuf::SendMsg sendMsg;
        if (binary)
            sendMsg.set_binary_message(std::move(msg));
        else
            sendMsg.set_text_message(std::move(msg));
        sendMsg.set_to_character_id(toPlayerID);
        sendMsg.set_character_id(playerID);
        sendMsg.set_team_id(teamID);
        return sendMsg;
    }

    inline protobuf::RecoverMsg THUAI82ProtobufRecoverMsg(int32_t playerID, int64_t recover, int32_t teamID)
    {
        protobuf::RecoverMsg RecoverMsg;
        RecoverMsg.set_character_id(playerID);
        RecoverMsg.set_recovered_hp(recover);
        RecoverMsg.set_team_id(teamID);
        return RecoverMsg;
    }

    inline protobuf::EquipMsg THUAI82ProtobufEquipMsg(int64_t character_id, int64_t team_id, THUAI8::EquipmentType equipment_type)
    {
        protobuf::EquipMsg equipMsg;
        equipMsg.set_character_id(character_id);
        equipMsg.set_team_id(team_id);
        equipMsg.set_equipment_type(protobuf::EquipmentType(equipment_type));
        return equipMsg;
    }

    inline protobuf::CreatCharacterMsg THUAI82ProtobufCreatCharacterMsg(int64_t team_id, THUAI8::CharacterType character_type, int32_t birthpoint_index)
    {
        protobuf::CreatCharacterMsg creatCharacterMsg;
        creatCharacterMsg.set_team_id(team_id);
        creatCharacterMsg.set_character_type(protobuf::CharacterType(character_type));
        creatCharacterMsg.set_birthpoint_index(birthpoint_index);
        return creatCharacterMsg;
    }

    inline protobuf::ConstructMsg THUAI82ProtobufConstructMsg(int64_t character_id, int64_t team_id, THUAI8::ConstructionType construction_type)
    {
        protobuf::ConstructMsg constructMsg;
        constructMsg.set_character_id(character_id);
        constructMsg.set_team_id(team_id);
        constructMsg.set_construction_type(protobuf::ConstructionType(construction_type));
        return constructMsg;
    }

    inline protobuf::ConstructTrapMsg THUAI82ProtobufConstructTrapMsg(int64_t character_id, int64_t team_id, THUAI8::TrapType trap_type)
    {
        protobuf::ConstructTrapMsg constructTrapMsg;
        constructTrapMsg.set_character_id(character_id);
        constructTrapMsg.set_team_id(team_id);
        constructTrapMsg.set_trap_type(protobuf::TrapType(trap_type));
        return constructTrapMsg;
    }

    inline protobuf::CharacterMsg THUAI82ProtobufCharacterMsg(int64_t character_id, int64_t team_id, THUAI8::CharacterType character_type)
    {
        protobuf::CharacterMsg characterMsg;
        characterMsg.set_character_id(character_id);
        characterMsg.set_team_id(team_id);
        characterMsg.set_character_type(protobuf::CharacterType(character_type));
        return characterMsg;
    }

    inline protobuf::CastMsg THUAI82ProtobufCastMsg(int64_t character_id, int64_t team_id, double attack_angle)
    {
        protobuf::CastMsg castMsg;
        castMsg.set_character_id(character_id);
        // castMsg.set_skill_id(skill_id);
        castMsg.set_team_id(team_id);
        castMsg.set_angle(attack_angle);
        return castMsg;
    }

    inline protobuf::AttackMsg THUAI82ProtobufAttackMsg(int64_t character_id, int64_t team_id, int64_t attacked_character_id, int64_t attacked_team_id)
    {
        protobuf::AttackMsg attackMsg;
        attackMsg.set_character_id(character_id);
        attackMsg.set_team_id(team_id);
        attackMsg.set_attacked_character_id(attacked_character_id);
        attackMsg.set_attacked_character_id(attacked_team_id);
        return attackMsg;
    }

    inline protobuf::AttackConstructionMsg THUAI82ProtobufAttackConstructionMsg(int64_t team_id, int64_t player_id)
    {
        protobuf::AttackConstructionMsg attackConstructionMsg;
        attackConstructionMsg.set_character_id(player_id);
        attackConstructionMsg.set_team_id(team_id);
        return attackConstructionMsg;
    }

    inline protobuf::AttackAdditionResourceMsg THUAI82ProtobufAttackAdditionResourceMsg(int64_t team_id, int64_t player_id)
    {
        protobuf::AttackAdditionResourceMsg attackAdditionResourceMsg;
        attackAdditionResourceMsg.set_character_id(player_id);
        attackAdditionResourceMsg.set_team_id(team_id);
        return attackAdditionResourceMsg;
    }

}  // namespace THUAI8Proto

namespace Time
{
    inline double TimeSinceStart(const std::chrono::system_clock::time_point& sp)
    {
        auto tp = std::chrono::system_clock::now();
        auto time_span = std::chrono::duration_cast<std::chrono::duration<double, std::milli>>(tp - sp);
        return time_span.count();
    }
}  // namespace Time
