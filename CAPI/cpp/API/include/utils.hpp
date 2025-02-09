#pragma once
#ifndef UTILS_HPP
#define UTILS_HPP

#include <cstdint>
#include <cmath>
#include <map>
#include <vector>
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

    inline bool HaveView(int32_t x, int32_t y, int32_t newX, int32_t newY, int32_t viewRange, std::vector<std::vector<THUAI7::PlaceType>>& map)
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
        {protobuf::PlayerType::Buddhists, THUAI8::PlayerType::Buddhists},
        {protobuf::PlayerType::Monsters, THUAI8::PlayerType::Monsters},
    };

    inline std::map<protobuf::CharacterType, THUAI8::CharacterType> characterTypeDict{
        {protobuf::CharacterType::NULL_CHARACTER_TYPE, THUAI8::CharacterType::NullCharacterType},
        {protobuf::CharacterType::CAMP1_CHARACTER1, THUAI8::CharacterType::Camp1Character1},
        {protobuf::CharacterType::CAMP1_CHARACTER2, THUAI8::CharacterType::Camp1Character2},
        {protobuf::CharacterType::CAMP1_CHARACTER3, THUAI8::CharacterType::Camp1Character3},
        {protobuf::CharacterType::CAMP1_CHARACTER4, THUAI8::CharacterType::Camp1Character4},
        {protobuf::CharacterType::CAMP1_CHARACTER5, THUAI8::CharacterType::Camp1Character5},
        {protobuf::CharacterType::CAMP1_CHARACTER6, THUAI8::CharacterType::Camp1Character6},
        {protobuf::CharacterType::CAMP2_CHARACTER1, THUAI8::CharacterType::Camp2Character1},
        {protobuf::CharacterType::CAMP2_CHARACTER2, THUAI8::CharacterType::Camp2Character2},
        {protobuf::CharacterType::CAMP2_CHARACTER3, THUAI8::CharacterType::Camp2Character3},
        {protobuf::CharacterType::CAMP2_CHARACTER4, THUAI8::CharacterType::Camp2Character4},
        {protobuf::CharacterType::CAMP2_CHARACTER5, THUAI8::CharacterType::Camp2Character5},
        {protobuf::CharacterType::CAMP2_CHARACTER6, THUAI8::CharacterType::Camp2Character6},
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
    };

    inline std::map<protobuf::EconomyResourceType, THUAI8::EconomyResourceType> economyResourceTypeDict{
        {protobuf::EconomyResourceType::NULL_ECONOMY_RESOURCE_TYPE, THUAI8::EconomyResourceType::NullEconomyResourceType},
        {protobuf::EconomyResourceType::ECONOMY_RESOURCE_TYPE, THUAI8::EconomyResourceType::EconomyResource},
    };

    inline std::map<protobuf::AdditionResourceType, THUAI8::AdditionResourceType> additionResourceTypeDict{
        {protobuf::AdditionResourceType::NULL_ADDITION_RESOURCE_TYPE, THUAI8::AdditionResourceType::NullAdditionResourceType},
        {protobuf::AdditionResourceType::SMALL_ADDITION_RESOURCE1, THUAI8::AdditionResourceType::SmallAdditionResource1},
        {protobuf::AdditionResourceType::MEDIUM_ADDITION_RESOURCE1, THUAI8::AdditionResourceType::MediumAdditionResource1},
        {protobuf::AdditionResourceType::LARGE_ADDITION_RESOURCE1, THUAI8::AdditionResourceType::LargeAdditionResource1},
        {protobuf::AdditionResourceType::SMALL_ADDITION_RESOURCE2, THUAI8::AdditionResourceType::SmallAdditionResource2},
        {protobuf::AdditionResourceType::MEDIUM_ADDITION_RESOURCE2, THUAI8::AdditionResourceType::MediumAdditionResource2},
        {protobuf::AdditionResourceType::LARGE_ADDITION_RESOURCE2, THUAI8::AdditionResourceType::LargeAdditionResource2},
        {protobuf::AdditionResourceType::ADDITION_RESOURCE3, THUAI8::AdditionResourceType::AdditionResource3},
        {protobuf::AdditionResourceType::ADDITION_RESOURCE4, THUAI8::AdditionResourceType::AdditionResource4},
    };

    inline std::map<protobuf::EconomyResourceState, THUAI8::EconomyResourceState> economyResourceStateDict{
        {protobuf::EconomyResourceState::NULL_ECONOMY_RESOURCE_STATE, THUAI8::EconomyResourceState::NullEconomyResourceState},
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

    inline std::map<protobuf::NewsType, THUAI8::NewsType> newsTypeDict{
        {protobuf::NewsType::NULL_NEWS_TYPE, THUAI8::NewsType::NullNewsType},
        {protobuf::NewsType::TEXT, THUAI8::NewsType::Text},
        {protobuf::NewsType::BINARY, THUAI8::NewsType::Binary},
    };

    inline std::map<protobuf::PlayerTeam, THUAI8::PlayerTeam> playerTeam{
        {protobuf::PlayerTeam::NULL_PLAYER_TEAM, THUAI8::PlayerTeam::NullPlayerTeam},
        {protobuf::PlayerTeam::BUDDHISTS_TEAM, THUAI8::PlayerTeam::BuddhistsTeam},
        {protobuf::PlayerTeam::MONSTERS_TEAM, THUAI8::PlayerTeam::MonstersTeam},
    };

    // 用于将Protobuf中的类转换为THUAI8的类
    inline std::shared_ptr<THUAI8::Character> Protobuf2THUAI8Character(const protobuf::MessageOfCharacter& CharacterMsg)
    {
        auto character = std::make_shared<THUAI8::Character>();
        character->characterType = characterTypeDict.at(CharacterMsg.character().characterType());
        character->hp = CharacterMsg.character().hp();
        character->teamID = CharacterMsg.character().teamid();
        character->playerID = CharacterMsg.character().playerid();
        character->x = CharacterMsg.character().x();
        character->y = CharacterMsg.character().y();
        character->state = characterStateDict.at(CharacterMsg.character().state());
        character->equipmentType = equipmentTypeDict.at(CharacterMsg.character().equipmenttype());
        return character;
    }

    inline std::shared_ptr<THUAI8::Team> Protobuf2THUAI8Team(const protobuf::MessageOfTeam& TeamMsg)
    {
        auto team = std::make_shared<THUAI8::Team>();
        team->teamID = TeamMsg.team().teamid();
        team->playerID = TeamMsg.team().playerid();
        team->score = TeamMsg.team().score();
        team->energy = TeamMsg.team().energy();
        return team;
    }

    inline std::shared_ptr<THUAI8::GameInfo> Protobuf2THUAI8GameInfo(const protobuf::MessageOfGameInfo& GameInfoMsg)
    {
        auto gameInfo = std::make_shared<THUAI8::GameInfo>();
        gameInfo->gameState = gameStateDict.at(GameInfoMsg.gameinfo().gamestate());
        gameInfo->time = GameInfoMsg.gameinfo().time();
        gameInfo->placeType = placeTypeDict.at(GameInfoMsg.gameinfo().placetype());
        return gameInfo;
    }

    inline std::shared_ptr<THUAI8::Trap> Protobuf2THUAI8Trap(const protobuf::MessageOfTrap& TrapMsg)
    {
        auto trap = std::make_shared<THUAI8::Trap>();
        trap->trapType = trapTypeDict.at(TrapMsg.trap().traptype());
        trap->x = TrapMsg.trap().x();
        trap->y = TrapMsg.trap().y();
        trap->teamID = TrapMsg.trap().teamid();
        trap->id = TrapMsg.trap().id();
        return trap;
    }

    inline std::shared_ptr<THUAI8::EconomyResource> Protobuf2THUAI8EconomyResource(const protobuf::MessageOfEconomyResource& EconomyResourceMsg)
    {
        auto economyResource = std::make_shared<THUAI8::EconomyResource>();
        economyResource->economyResourceType = economyResourceTypeDict.at(EconomyResourceMsg.economyresource().economyresourcetype());
        economyResource->economyResourceState = economyResourceStateDict.at(EconomyResourceMsg.economyresource().economyresourcestate());
        economyResource->x = EconomyResourceMsg.economyresource().x();
        economyResource->y = EconomyResourceMsg.economyresource().y();
        return economyResource;
    }

    inline std::shared_ptr<THUAI8::AdditionResource> Protobuf2THUAI8AdditionResource(const protobuf::MessageOfAdditionResource& AdditionResourceMsg)
    {
        auto additionResource = std::make_shared<THUAI8::AdditionResource>();
        additionResource->additionResourceType = additionResourceTypeDict.at(AdditionResourceMsg.additionresource().additionresourcetype());
        additionResource->additionResourceState = additionResourceStateDict.at(AdditionResourceMsg.additionresource().additionresourcestate());
        additionResource->x = AdditionResourceMsg.additionresource().x();
        additionResource->y = AdditionResourceMsg.additionresource().y();
        return additionResource;
    }

    inline std::shared_ptr<THUAI8::ConstructionState> Protobuf2THUAI8ConstructionState(const protobuf::MessageOfConstructionState& ConstructionStateMsg)
    {
        auto constructionState = std::make_shared<THUAI8::ConstructionState>();
        constructionState->teamID = ConstructionStateMsg.constructionstate().teamid();
        constructionState->hp = ConstructionStateMsg.constructionstate().hp();
        constructionState->constructionType = constructionTypeDict.at(ConstructionStateMsg.constructionstate().constructiontype());
        return constructionState;
    }

    inline std::shared_ptr<THUAI8::GameMap> Protobuf2THUAI8GameMap(const protobuf::MessageOfGameMap& GameMapMsg)
    {
        auto gameMap = std::make_shared<THUAI8::GameMap>();
        for (const auto& barracks : GameMapMsg.gamemap().barracksstate())
        {
            gameMap->barracksState[{barracks.first.x(), barracks.first.y()}] = {barracks.second.teamid(), barracks.second.hp()};
        }
        for (const auto& spring : GameMapMsg.gamemap().springstate())
        {
            gameMap->springState[{spring.first.x(), spring.first.y()}] = {spring.second.teamid(), spring.second.hp()};
        }
        for (const auto& farm : GameMapMsg.gamemap().farmstate())
        {
            gameMap->farmState[{farm.first.x(), farm.first.y()}] = {farm.second.teamid(), farm.second.hp()};
        }
        for (const auto& trap : GameMapMsg.gamemap().trapstate())
        {
            gameMap->trapState[{trap.first.x(), trap.first.y()}] = {trap.second.teamid(), trap.second.hp()};
        }
        for (const auto& economyResource : GameMapMsg.gamemap().economyresource())
        {
            gameMap->economyResource[{economyResource.first.x(), economyResource.first.y()}] = economyResource.second;
        }
        for (const auto& additionResource : GameMapMsg.gamemap().additionresource())
        {
            gameMap->additionResource[{additionResource.first.x(), additionResource.first.y()}] = additionResource.second;
        }
        return gameMap;
    }
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
        {THUAI8::PlayerTeam::NullTeam, protobuf::PlayerTeam::NULL_PLAYER_TEAM},
        {THUAI8::PlayerTeam::BuddhistsTeam, protobuf::PlayerTeam::BUDDHISTS_TEAM},
        {THUAI8::PlayerTeam::MonstersTeam, protobuf::PlayerTeam::MONSTERS_TEAM},
    };

    inline std::map<THUAI8::CharacterType, protobuf::CharacterType> characterTypeDict{
        {THUAI8::CharacterType::NullCharacterType, protobuf::CharacterType::NULL_CHARACTER_TYPE},
        {THUAI8::CharacterType::Camp1Character1, protobuf::CharacterType::CAMP1_CHARACTER1},
        {THUAI8::CharacterType::Camp1Character2, protobuf::CharacterType::CAMP1_CHARACTER2},
        {THUAI8::CharacterType::Camp1Character3, protobuf::CharacterType::CAMP1_CHARACTER3},
        {THUAI8::CharacterType::Camp1Character4, protobuf::CharacterType::CAMP1_CHARACTER4},
        {THUAI8::CharacterType::Camp1Character5, protobuf::CharacterType::CAMP1_CHARACTER5},
        {THUAI8::CharacterType::Camp1Character6, protobuf::CharacterType::CAMP1_CHARACTER6},
        {THUAI8::CharacterType::Camp2Character1, protobuf::CharacterType::CAMP2_CHARACTER1},
        {THUAI8::CharacterType::Camp2Character2, protobuf::CharacterType::CAMP2_CHARACTER2},
        {THUAI8::CharacterType::Camp2Character3, protobuf::CharacterType::CAMP2_CHARACTER3},
        {THUAI8::CharacterType::Camp2Character4, protobuf::CharacterType::CAMP2_CHARACTER4},
        {THUAI8::CharacterType::Camp2Character5, protobuf::CharacterType::CAMP2_CHARACTER5},
        {THUAI8::CharacterType::Camp2Character6, protobuf::CharacterType::CAMP2_CHARACTER6},
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

    inline std::mao<THUAI8::CharacterState, protobuf::CharacterState> characterStateDict{
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
    };

    inline std::map<THUAI8::EconomyResourceType, protobuf::EconomyResourceType> economyResourceTypeDict{
        {THUAI8::EconomyResourceType::NullEconomyResourceType, protobuf::EconomyResourceType::NULL_ECONOMY_RESOURCE_TYPE},
        {THUAI8::EconomyResourceType::SmallEconomyResource, protobuf::EconomyResourceType::SMALL_ECONOMY_RESOURCE},
        {THUAI8::EconomyResourceType::MediumEconomyResource, protobuf::EconomyResourceType::MEDIUM_ECONOMY_RESOURCE},
        {THUAI8::EconomyResourceType::LargeEconomyResource, protobuf::EconomyResourceType::LARGE_ECONOMY_RESOURCE},
    };

    inline std::map<THUAI8::AdditionResourceType, protobuf::AdditionResourceType> additionResourceTypeDict{
        {THUAI8::AdditionResourceType::NullAdditionResourceType, protobuf::AdditionResourceType::NULL_ADDITION_RESOURCE_TYPE},
        {THUAI8::AdditionResourceType::SmallAdditionResource1, protobuf::AdditionResourceType::SMALL_ADDITION_RESOURCE1},
        {THUAI8::AdditionResourceType::MediumAdditionResource1, protobuf::AdditionResourceType::MEDIUM_ADDITION_RESOURCE1},
        {THUAI8::AdditionResourceType::LargeAdditionResource1, protobuf::AdditionResourceType::LARGE_ADDITION_RESOURCE1},
        {THUAI8::AdditionResourceType::SmallAdditionResource2, protobuf::AdditionResourceType::SMALL_ADDITION_RESOURCE2},
        {THUAI8::AdditionResourceType::MediumAdditionResource2, protobuf::AdditionResourceType::MEDIUM_ADDITION_RESOURCE2},
        {THUAI8::AdditionResourceType::LargeAdditionResource2, protobuf::AdditionResourceType::LARGE_ADDITION_RESOURCE2},
        {THUAI8::AdditionResourceType::AdditionResource3, protobuf::AdditionResourceType::ADDITION_RESOURCE3},
        {THUAI8::AdditionResourceType::AdditionResource4, protobuf::AdditionResourceType::ADDITION_RESOURCE4},
    };

    inline std::map<THUAI8::EconomyResourceState, protobuf::EconomyResourceState> economyResourceStateDict{
        {THUAI8::EconomyResourceState::NullEconomyResourceState, protobuf::EconomyResourceState::NULL_ECONOMY_RESOURCE_STATE},
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
        {THUAI8::NewsType::Text, protobuf::NewsType::TEXT},
        {THUAI8::NewsType::Binary, protobuf::NewsType::BINARY},
    };

    inline std::map<THUAI8::MessageOfObj, protobuf::MessageOfObj> messageOfObjDict{
        {THUAI8::MessageOfObj::NullMessageOfObj, protobuf::MessageOfObj::NULL_MESSAGE_OF_OBJ},
        {THUAI8::MessageOfObj::CharacterMessage, protobuf::MessageOfObj::CHARACTER_MESSAGE},
        {THUAI8::MessageOfObj::BarracksMessage, protobuf::MessageOfObj::BARRACKS_MESSAGE},
        {THUAI8::MessageOfObj::SpringMessage, protobuf::MessageOfObj::SPRING_MESSAGE},
        {THUAI8::MessageOfObj::FarmMessage, protobuf::MessageOfObj::FARM_MESSAGE},
        {THUAI8::MessageOfObj::TrapMessage, protobuf::MessageOfObj::TRAP_MESSAGE},
        {THUAI8::MessageOfObj::EconomyResourceMessage, protobuf::MessageOfObj::ECONOMY_RESOURCE_MESSAGE},
        {THUAI8::MessageOfObj::AdditionResourceMessage, protobuf::MessageOfObj::ADDITION_RESOURCE_MESSAGE},
        {THUAI8::MessageOfObj::MapMessage, protobuf::MessageOfObj::MAP_MESSAGE},
        {THUAI8::MessageOfObj::TeamMessage, protobuf::MessageOfObj::TEAM_MESSAGE},
        {THUAI8::MessageOfObj::NewsMessage, protobuf::MessageOfObj::NEWS_MESSAGE},
    };

    inline protobuf::MoveMsg THUAI8MoveMsg2ProtobufMoveMsg(int64_t character, double angle, int64_t time, int64_t team)
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
        IDMsg.set_player_id(playerID);
        IDMsg.set_team_id(teamID);
        return IDMsg;
    }

    inline protobuf::EquipMsg THUAI82ProtobufEquipMsg(int64_t character_id, int64_t team_id, THUAI8::EquipmentType equipment_type)
    {
        protobuf::EquipMsg equipMsg;
        equipMsg.set_character_id(character_id);
        equipMsg.set_team_id(team_id);
        equipMsg.set_equipment_type(equipment_type);
        return equipMsg;
    }

    inline protobuf::CreatCharacterMsg(int64_t team_id, THUAI8::CharacterType character_type, int32_t birthpoint_index)
    {
        protobuf::CreatCharacterMsg creatCharacterMsg;
        creatCharacterMsg.set_team_id(team_id);
        creatCharacterMsg.set_character_type(character_type);
        creatCharacterMsg.set_birthpoint_index(birthpoint_index);
        return creatCharacterMsg;
    }

    inline protobuf::ConstructMsg(int64_t character_id, int64_t team_id, THUAI8::ConstructionType construction_type, int32_t x, int32_t y)
    {
        protobuf::ConstructMsg constructMsg;
        constructMsg.set_character_id(character_id);
        constructMsg.set_team_id(team_id);
        constructMsg.set_construction_type(construction_type);
        return constructMsg;
    }

    inline protobuf::CharacterMsg(int64_t character_id, int64_t team_id, THUAI8::CharacterType character_type)
    {
        protobuf::CharacterMsg characterMsg;
        characterMsg.set_character_id(character_id);
        characterMsg.set_team_id(team_id);
        characterMsg.set_character_type(character_type);
        return characterMsg;
    }

    inline protobuf::CastMsg(int64_t character_id, int64_t skill_id, int64_t team_id, int32_t attack_range, int32_t x, int32_t y, double angle)
    {
        protobuf::CastMsg castMsg;
        castMsg.set_character_id(character_id);
        castMsg.set_skill_id(skill_id);
        castMsg.set_team_id(team_id);
        castMsg.set_attack_range(attack_range);
        castMsg.set_x(x);
        castMsg.set_y(y);
        castMsg.set_angle(angle);
        return castMsg;
    }

    inline protobuf::AttackMsg(int64_t character_id, int64_t team_id, int64_t attacked_character_id, int32_t attack_range)
    {
        protobuf::AttackMsg attackMsg;
        attackMsg.set_character_id(character_id);
        attackMsg.set_team_id(team_id);
        attackMsg.set_attacked_character_id(attacked_character_id);
        attackMsg.set_attack_range(attack_range);
        return attackMsg;
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
#endif