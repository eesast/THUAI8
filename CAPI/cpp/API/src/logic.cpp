#include "logic.h"
#include "structures.h"
#include <grpcpp/grpcpp.h>
#include <spdlog/spdlog.h>
#include <spdlog/sinks/basic_file_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <functional>
#include "utils.hpp"
#include "Communication.h"

#undef GetMessage
#undef SendMessage
#undef PeekMessage

extern const bool asynchronous;

Logic::Logic(int32_t pID, int32_t tID, THUAI8::PlayerType pType, THUAI8::CharacterType sType) :
    playerID(pID),
    teamID(tID),
    playerType(pType),
    selfType(sType)
{
    currentState = &state[0];
    bufferState = &state[1];
    currentState->gameInfo = std::make_shared<THUAI8::GameInfo>();
    bufferState->gameInfo = std::make_shared<THUAI8::GameInfo>();
    currentState->mapInfo = std::make_shared<THUAI8::GameMap>();
    bufferState->mapInfo = std::make_shared<THUAI8::GameMap>();
    if (teamID == 0)
        playerTeam = THUAI8::PlayerTeam::BuddhistsTeam;
    else if (teamID == 1)
        playerTeam = THUAI8::PlayerTeam::MonstersTeam;
    else
        playerTeam = THUAI8::PlayerTeam::NullTeam;
}

std::vector<std::shared_ptr<const THUAI8::Character>> Logic::GetCharacters() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    std::vector<std::shared_ptr<const THUAI8::Character>> temp(currentState->characters.begin(), currentState->characters.end());
    logger->debug("Called GetCharacters");
    return temp;
}

std::vector<std::shared_ptr<const THUAI8::Character>> Logic::GetEnemyCharacters() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    std::vector<std::shared_ptr<const THUAI8::Character>> temp(currentState->enemyCharacters.begin(), currentState->enemyCharacters.end());
    logger->debug("Called GetEnemyCharacters");
    return temp;
}

std::shared_ptr<const THUAI8::Character> Logic::CharacterGetSelfInfo() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called CharacterGetSelfInfo");
    return currentState->characterSelf;
}

std::shared_ptr<const THUAI8::Team> Logic::TeamGetSelfInfo() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called TeamGetSelfInfo");
    return currentState->teamSelf;
}

std::vector<std::vector<THUAI8::PlaceType>> Logic::GetFullMap() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetFullMap");
    return currentState->gameMap;
}

THUAI8::PlaceType Logic::GetPlaceType(int32_t cellX, int32_t cellY) const
{
    std::unique_lock<std::mutex> lock(mtxState);
    if (cellX < 0 || uint64_t(cellX) >= currentState->gameMap.size() || cellY < 0 || uint64_t(cellY) >= currentState->gameMap[0].size())
    {
        logger->warn("Invalid position!");
        return THUAI7::PlaceType::NullPlaceType;
    }
    logger->debug("Called GetPlaceType");
    return currentState->gameMap[cellX][cellY];
}

std::optional<THUAI8::ConstructionState> Logic::GetConstructionState(int32_t cellX, int32_t cellY) const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetConstructionState");
    auto pos = THUAI8::cellxy_t(cellX, cellY);
    auto it = currentState->mapInfo->farmState.find(pos);
    auto it2 = currentState->mapInfo->trapState.find(pos);
    if (it != currentState->mapInfo->farmState.end())
    {
        return std::make_optional<THUAI8::ConstructionState>(currentState->mapInfo->farmState[pos], THUAI8::ConstructionType::Farm);
    }
    else if (it2 != currentState->mapInfo->trapState.end())
    {
        return std::make_optional<THUAI8::ConstructionState>(currentState->mapInfo->trapState[pos], THUAI8::ConstructionType::Trap);
    }
    else
    {
        logger->warn("Construction not found!");
        return std::nullopt;
    }
}

int32_t Logic::GetResourceState(int32_t cellX, int32_t cellY) const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetResourceState");
    auto pos = THUAI8::cellxy_t(cellX, cellY);
    auto it = currentState->mapInfo->economyResource.find(pos);
    if (it != currentState->mapInfo->economyResource.end())
    {
        return currentState->mapInfo->economyResource[pos];
    }
    else
    {
        logger->warn("Resource not found!");
        return -1;
    }
}

int32_t