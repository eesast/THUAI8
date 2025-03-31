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

Logic::Logic(int32_t pID, int32_t tID, THUAI8::PlayerType pType, THUAI8::CharacterType cType) :
    playerID(pID),
    teamID(tID),
    playerType(pType),
    CharacterType(cType)
{
    currentState = &state[0];
    bufferState = &state[1];
    currentState->gameInfo = std::make_shared<THUAI8::GameInfo>();
    currentState->mapInfo = std::make_shared<THUAI8::GameMap>();
    bufferState->gameInfo = std::make_shared<THUAI8::GameInfo>();
    bufferState->mapInfo = std::make_shared<THUAI8::GameMap>();
    if (teamID == 0)
        playerTeam = THUAI8::PlayerTeam::Red;
    else if (teamID == 1)
        playerTeam = THUAI8::PlayerTeam::Blue;
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

std::vector<std::shared_ptr<const THUAI8::Character>> Logic::GetEnemySCharacters() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    std::vector<std::shared_ptr<const THUAI8::Character>> temp(currentState->enemyCharacters.begin(), currentState->enemyCharacters.end());
    logger->debug("Called GetEnemyCharacters");
    return temp;
}

std::shared_ptr<const THUAI8::Character> Logic::GetSelfInfo() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetSelfInfo");
    return currentState->characterSelf;
}

// std::shared_ptr<const THUAI8::Team> Logic::TeamGetSelfInfo() const
// {
//     std::unique_lock<std::mutex> lock(mtxState);
//     logger->debug("Called TeamGetSelfInfo");
//     return currentState->teamSelf;
// }

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
        return THUAI8::PlaceType::NullPlaceType;
           
    }
    logger->debug("Called GetPlaceType");
    return currentState->gameMap[cellX][cellY];
}

std::optional<THUAI8::EconomyResourceState> Logic::GetEconomyResourceState(int32_t cellX, int32_t cellY) const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetEconomyResourceState");
    auto pos = THUAI8::cellxy_t(cellX, cellY);
    auto it = currentState->mapInfo->economyResourceState.find(pos);
    if (it != currentState->mapInfo->economyResourceState.end())
    {
        return std::make_optional<THUAI8::EconomyResourceState>(currentState->mapInfo->economyResourceState[pos]);
    }
    else
           
        {
            logger->warn("EconomyResource not found");
            return std::nullopt;
        }
}

std::optional<THUAI8::AdditionResourceState> Logic::GetAdditionResourceState(int32_t cellX, int32_t cellY) const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetAdditionResourceState");
    auto pos = THUAI8::cellxy_t(cellX, cellY);
    auto it = currentState->mapInfo->additionResourceState.find(pos);
    if (it != currentState->mapInfo->additionResourceState.end())
           
        {
            return std::make_optional<THUAI8::AdditionResourceState>(currentState->mapInfo->additionResourceState[pos]);
        }
    else
           
        {
            logger->warn("AdditionResource not found");
            return std::nullopt;
        }
}

std::optional<THUAI8::ConstructionState> Logic::GetConstructionState(int32_t cellX, int32_t cellY) const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetConstructionState");
    auto pos = THUAI8::cellxy_t(cellX, cellY);
    auto it = currentState->mapInfo->barracksState.find(pos);
    auto it2 = currentState->mapInfo->springState.find(pos);
    auto it3 = currentState->mapInfo->farmState.find(pos);
    if (it != currentState->mapInfo->barracksState.end())
    {
        return std::make_optional<THUAI8::ConstructionState>(currentState->mapInfo->barracksState[pos], THUAI8::ConstructionType::Barracks);
    }
    else if (it2 != currentState->mapInfo->springState.end())
        return std::make_optional<THUAI8::ConstructionState>(currentState->mapInfo->springState[pos], THUAI8::ConstructionType::Spring);
    else if (it3 != currentState->mapInfo->farmState.end())
        return std::make_optional<THUAI8::ConstructionState>(currentState->mapInfo->farmState[pos], THUAI8::ConstructionType::Farm);
    else
           
        {
            logger->warn("Construction not found");
            return std::nullopt;
        }
}

int32_t Logic::GetEconomy() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetEconomy");
    if (playerTeam == THUAI8::PlayerTeam::BuddhistsTeam)
        return currentState->gameInfo->buddhistsTeamEconomy;
    else if (playerTeam == THUAI8::PlayerTeam::MonstersTeam)
        return currentState->gameInfo->monstersTeamEconomy;
    else
           
        {
            logger->warn("Invalid playerTeam");
            return -1;
        }
}

int32_t Logic::GetScore() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetScore");
    if (playerTeam == THUAI8::PlayerTeam::BuddhistsTeam)
        return currentState->gameInfo->buddhistsTeamScore;
    else if (playerTeam == THUAI8::PlayerTeam::MonstersTeam)
        return currentState->gameInfo->monstersTeamScore;
    else
           
        {
            logger->warn("Invalid playerTeam");
            return -1;
        }
}

std::shared_ptr<const THUAI8::GameInfo> Logic::GetGameInfo() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetGameInfo");
    return currentState->gameInfo;
}

//
bool Logic::Move(int64_t time, double angle)
{
    logger->debug("Called Move");
    return pComm->Move(playerID, teamID, time, angle);
}

bool Logic::Send(int32_t toID, std::string message, bool binary)
{
    logger->debug("Called SendMessage");
    return pComm->Send(playerID, toID, teamID, std::move(message), binary);
}

bool Logic::HaveMessage()
{
    logger->debug("Called HaveMessage");
    return !messageQueue.empty();
}

std::pair<int32_t, std::string> Logic::GetMessage()
{
    logger->debug("Called GetMessage");
    auto msg = messageQueue.tryPop();
    if (msg.has_value())
        return msg.value();
    else
           
        {
            logger->warn("No message");
            return std::pair(-1, std::string(""));
        }
}

bool Logic::Common_Attack(int32_t attacked_playerID, int32_t attacked_teamID)
{
    logger->debug("Called Attack");
    return pComm->Attack(attacked_playerID, attacked_teamID);
}

bool Logic::Skill_Attack(int32_t attacked_playerID, int32_t attacked_teamID)
{
    logger->debug("Called SkillAttack");
    return pComm->SkillAttack(attacked_playerID, attacked_teamID);
}

bool Logic::Recover(int64_t recover)
{
    logger->debug("Called Recover");
    return pComm->Recover(playerID, recover, teamID);
}

bool Logic::Construct(THUAI8::ConstructionType constructiontype)
{
    logger->debug("Called Construct");
    return pComm->Construct(playerID, teamID, constructiontype);
}

bool Logic::BuildCharacter(THUAI8::CharacterType CharacterType, int32_t birthIndex)
{
    logger->debug("Called BuildCharacter");
    return pComm->BuildCharacter(teamID, CharacterType, birthIndex);
}

// 等待完成
// bool Logic::Recycle(int32_t targetID)
// {
//     logger->debug("Called Recycle");
//     return pComm->Recycle(targetID, teamID);
// }

// bool Logic::Produce()
// {
//     logger->debug("Called Produce");
//     return pComm->Produce(playerID, teamID);
// }

bool Logic::Rebuild(THUAI8::ConstructionType constructionType)
{
    logger->debug("Called Rebuild");
    return pComm->Rebuild(playerID, teamID, constructionType);
}

bool Logic::InstallEquipment(int32_t playerID, THUAI8::EquipmentType equipmentType)
{
    logger->debug("Called InstallEquipment");
    return pComm->InstallEquipment(playerID, teamID, equipmentType);
}

bool Logic::EndAllAction()
{
    logger->debug("Called EndAllAction");
    return pComm->EndAllAction(playerID, teamID);
}

bool Logic::WaitThread()
{
    if (asynchronous)
        Wait();
    return true;
}

void Logic::ProcessMessage()
{
    auto messageThread = [this]()
    {
        try
        {
            // TODO
            logger->info("Message thread start!");
            pComm->AddPlayer(playerID, teamID, PlayerType);
            while (gameState != THUAI8::GameState::GameEnd)
            {
                auto clientMsg = pComm->GetMessage2Client();
                // 在获得新消息之前阻塞
                logger->debug("Get message from server!");
                gameState = Proto2THUAI8::gameStateDict[clientMsg.game_state()];
                switch (gameState)
                {
                    case THUAI8::GameState::GameStart:
                        logger->info("Game Start!");
                        // 读取地图
                        for (const auto& item : clientMsg.obj_message())
                        {
                            if (Proto2THUAI8::messageOfObjDict[item.message_of_obj_case()] == THUAI8::MessageOfObj::MapMessage)
                            {
                                auto map = std::vector<std::vector<THUAI8::PlaceType>>();
                                auto& mapResult = item.map_message();
                                for (int32_t i = 0; i < item.map_message().rows_size(); i++)
                                {
                                    std::vector<THUAI8::PlaceType> row;
                                    for (int32_t j = 0; j < mapResult.rows(i).cols_size(); j++)
                                    {
                                        if (Proto2THUAI8::placeTypeDict.count(mapResult.rows(i).cols(j)) == 0)
                                            logger->error("Unknown place type!");
                                        row.push_back(Proto2THUAI8::placeTypeDict[mapResult.rows(i).cols(j)]);
                                    }
                                    map.push_back(std::move(row));
                                }
                                bufferState->gameMap = std::move(map);
                                currentState->gameMap = bufferState->gameMap;
                                logger->info("Map loaded!");
                                break;
                            }
                             
                        }
                        if (currentState->gameMap.empty())
                        {
                            logger->error("Map not loaded!");
                            throw std::runtime_error("Map not loaded!");
                        }
                        LoadBuffer(clientMsg);
                        AILoop = true;
                        UnBlockAI();
                        break;
                    case THUAI8::GameState::GameRunning:
                        LoadBuffer(clientMsg);
                        break;
                    default:
                        logger->debug("Unknown GameState!");
                        break;
                }
            }
            {
                std::lock_guard<std::mutex> lock(mtxBuffer);
                bufferUpdated = true;
                counterBuffer = -1;
            }
            cvBuffer.notify_one();
            logger->info("Game End!");
            AILoop = false;
        }
        catch (const std::exception& e)
        {
            std::cerr << "C++ Exception: " << e.what() << std::endl;
            AILoop = false;
        }
        catch (...)
        {
            std::cerr << "Unknown Exception!" << std::endl;
            AILoop = false;
        }
    };
        std::thread(messageThread).detach();
}

void Logic::LoadBufferSelf(const protobuf::MessageToClient& message)
{
    if (playerType == THUAI8::PlayerType::Character)
    {
        for (const auto& item : message.obj_message())
        {
            if (Proto2THUAI8::messageOfObjDict[item.message_of_obj_case()] == THUAI8::MessageOfObj::CharacterMessage && item.character_message().player_id() == playerID)
            {
                bufferState->characterSelf = Proto2THUAI8::Protobuf2THUAI8Character(item.character_message());
                bufferState->characters.push_back(bufferState->characterSelf);
                logger->debug("Load Self Character!");
            }
               
        }
    }
    else if (playerType == THUAI8::PlayerType::Team)
    {
        for (const auto& item : message.obj_message())
        {
            if (Proto2THUAI8::messageOfObjDict[item.message_of_obj_case()] == THUAI8::MessageOfObj::TeamMessage && item.team_message().team_id() == teamID)
            {
                bufferState->teamSelf = Proto2THUAI8::Protobuf2THUAI8Team(item.team_message());
                logger->debug("Load Self Team!");
            }
            else if (Proto2THUAI8::messageOfObjDict[item.message_of_obj_case()] == THUAI8::MessageOfObj::CharacterMessage && item.character_message().team_id() == teamID)
            {
                std::shared_ptr<THUAI8::Character> Character = Proto2THUAI8::Protobuf2THUAI8Character(item.character_message());
                bufferState->characters.push_back(Character);
                logger->debug("Load Character!");
            }
        }
    }
}

void Logic::LoadBufferCase(const protobuf::MessageOfObj& item)
{
    if (playerType == THUAI8::PlayerType::Character)
    {
        int32_t x, y, viewRange;
        x = bufferState->characterSelf->x, y = bufferState->characterSelf->y, viewRange = bufferState->characterSelf->viewRange;
        switch (Proto2THUAI8::messageOfObjDict[item.message_of_obj_case()])
        {
            case THUAI8::MessageOfObj::CharacterMessage:
                if (teamID != item.character_message().team_id())
                {
                    if (AssistFunction::HaveView(x, y, item.character_message().x(), item.character_message().y(), viewRange, bufferState->gameMap))
                    {
                        std::shared_ptr<THUAI8::Character> Character = Proto2THUAI8::Protobuf2THUAI8Character(item.character_message());
                        bufferState->enemyCharacters.push_back(Character);
                        logger->debug("Load EnemyCharacter!");
                    }
                }
                else if (teamID == item.character_message().team_id() && playerID != item.character_message().player_id())
                {
                    std::shared_ptr<THUAI8::Character> Character = Proto2THUAI8::Protobuf2THUAI8Character(item.character_message());
                    bufferState->characters.push_back(Character);
                    logger->debug("Load Character!");
                }
                break;
            case THUAI8::MessageOfObj::BarracksMessage:
                if (item.barracks_message().team_id() == teamID || AssistFunction::HaveView(x, y, item.barracks_message().x(), item.barracks_message().y(), viewRange, bufferState->gameMap))
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.barracks_message().x()),
                        AssistFunction::GridToCell(item.barracks_message().y())
                    );
                    if (bufferState->mapInfo->barracksState.count(pos) == 0)
                    {
                        bufferState->mapInfo->barracksState.emplace(pos, std::pair(item.barracks_message().team_id(), item.barracks_message().hp()));
                        if (item.barracks_message().team_id() == teamID)
                            logger->debug("Load Barracks!");
                        else
                            logger->debug("Load EnemyBarracks!");
                    }
                    else
                    {
                        bufferState->mapInfo->barracksState[pos].first = item.barracks_message().team_id();
                        bufferState->mapInfo->barracksState[pos].second = item.barracks_message().hp();
                        if (item.barracks_message().team_id() == teamID)
                            logger->debug("Update Barracks!");
                        else
                            logger->debug("Update EnemyBarracks!");
                    }
                }
                break;
            case THUAI8::MessageOfObj::SpringMessage:
                if (item.spring_message().team_id() == teamID || AssistFunction::HaveView(x, y, item.spring_message().x(), item.spring_message().y(), viewRange, bufferState->gameMap))
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.spring_message().x()),
                        AssistFunction::GridToCell(item.spring_message().y())
                    );
                    if (bufferState->mapInfo->springState.count(pos) == 0)
                    {
                        bufferState->mapInfo->springState.emplace(pos, std::pair(item.spring_message().team_id(), item.spring_message().hp()));
                        if (item.spring_message().team_id() == teamID)
                            logger->debug("Load Spring!");
                        else
                            logger->debug("Load EnemySpring!");
                    }
                    else
                    {
                        bufferState->mapInfo->springState[pos].first = item.spring_message().team_id();
                        bufferState->mapInfo->springState[pos].second = item.spring_message().hp();
                        if (item.spring_message().team_id() == teamID)
                            logger->debug("Update Spring!");
                        else
                            logger->debug("Update EnemySpring!");
                    }
                }
                break;
            case THUAI8::MessageOfObj::FarmMessage:
                if (item.farm_message().team_id() == teamID || AssistFunction::HaveView(x, y, item.farm_message().x(), item.farm_message().y(), viewRange, bufferState->gameMap))
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.farm_message().x()),
                        AssistFunction::GridToCell(item.farm_message().y())
                    );
                    if (bufferState->mapInfo->farmState.count(pos) == 0)
                    {
                        bufferState->mapInfo->farmState.emplace(pos, std::pair(item.farm_message().team_id(), item.farm_message().hp()));
                        if (item.farm_message().team_id() == teamID)
                            logger->debug("Load Farm!");
                        else
                            logger->debug("Load EnemyFarm!");
                    }
                    else
                    {
                        bufferState->mapInfo->farmState[pos].first = item.farm_message().team_id();
                        bufferState->mapInfo->farmState[pos].second = item.farm_message().hp();
                        if (item.farm_message().team_id() == teamID)
                            logger->debug("Update Farm!");
                        else
                            logger->debug("Update EnemyFarm!");
                    }
                }
                break;
            case THUAI8::MessageOfObj::TrapMessage:
                // 待定
                if (item.trap_message().team_id() == teamID || AssistFunction::HaveView(x, y, item.trap_message().x(), item.trap_message().y(), viewRange, bufferState->gameMap) && currentState->characterSelf->visionBuffTime > 0)
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.trap_message().x()),
                        AssistFunction::GridToCell(item.trap_message().y())
                    );
                    if (bufferState->mapInfo->trapState.count(pos) == 0)
                    {
                        bufferState->mapInfo->trapState.emplace(pos, item.trap_message().team_id());
                        if (item.trap_message().team_id() == teamID)
                            logger->debug("Load Trap!");
                        else
                            logger->debug("Load EnemyTrap!");
                    }
                    else
                    {
                        bufferState->mapInfo->trapState[pos] = item.trap_message().team_id();
                        if (item.trap_message().team_id() == teamID)
                            logger->debug("Update Trap!");
                        else
                            logger->debug("Update EnemyTrap!");
                    }
                }
                break;
            case THUAI8::MessageOfObj::EconomyResourceMessage:
                auto pos = THUAI8::cellxy_t(
                    AssistFunction::GridToCell(item.economy_resource_message().x()),
                    AssistFunction::GridToCell(item.economy_resource_message().y())
                );
                if (bufferState->mapInfo->economyResourceState.count(pos) == 0)
                {
                    bufferState->mapInfo->economyResourceState.emplace(pos, item.economy_resource_message().hp());
                    logger->debug("Load EconomyResource!");
                }
                else
                {
                    bufferState->mapInfo->economyResourceState[pos] = item.economy_resource_message().hp();
                    logger->debug("Update EconomyResource!");
                }
                break;
            case THUAI8::MessageOfObj::AdditionResourceMessage:
                auto pos = THUAI8::cellxy_t(
                    AssistFunction::GridToCell(item.addition_resource_message().x()),
                    AssistFunction::GridToCell(item.addition_resource_message().y())
                );
                if (bufferState->mapInfo->additionResourceState.count(pos) == 0)
                {
                    bufferState->mapInfo->additionResourceState.emplace(pos, std::pair(item.addition_resource_message().hp(), item.addition_resource_message().addition_resource_type()));
                    logger->debug("Load AdditionResource!");
                }
                else
                {
                    bufferState->mapInfo->additionResourceState[pos].first = item.addition_resource_message().hp();
                    bufferState->mapInfo->additionResourceState[pos].second = item.addition_resource_message().addition_resource_type();
                    logger->debug("Update AdditionResource!");
                }
                break;
            case THUAI8::MessageOfObj::NewsMessage:
                auto& news = item.news_message();
                if (news.to_id() == playerID && news.team_id() == teamID)
                {
                    if (Proto2THUAI8::newsTypeDict[news.news_case()] == THUAI8::NewsType::TextMessage)
                    {
                        messageQueue.emplace(std::pair(news.from_id(), news.text_message()));
                        logger->debug("Load Text News!");
                    }
                    else if (Proto2THUAI8::newsTypeDict[news.news_case()] == THUAI8::NewsType::BinaryMessage)
                    {
                        messageQueue.emplace(std::pair(news.from_id(), news.binary_message()));
                        logger->debug("Load Binary News!");
                    }
                    else
                        logger->error("Unknown NewsType!");
                }
                break;
            case THUAI8::MessageOfObj::NullMessageOfObj:
            default:
                break;
        }
    }
    else if (playerType == THUAI8::PlayerType::Team)
    {
        auto HaveOverView = [&](int32_t targetX, int32_t targetY)
        {
            for (const auto& character : bufferState->characters)
            {
                if (AssistFunction::HaveView(character->x, character->y, targetX, targetY, character->viewRange, bufferState->gameMap))
                    return true;
            }
            return false;
        };
        auto HaveOverTrapView = [&](int32_t targetX, int32_t targetY)
        {
            for (const auto& character : bufferState->characters)
            {
                if (AssistFunction::HaveView(character->x, character->y, targetX, targetY, character->viewRange, bufferState->gameMap) && character->visionBuffTime > 0)
                    return true;
            }
            return false;
        };
        switch (Proto2THUAI8::messageOfObjDict[item.message_of_obj_case()])
        {
            case THUAI8::MessageOfObj::CharacterMessage:
                if (item.character_message().team_id() != teamID && HaveOverView(item.character_message().x(), item.character_message().y()))
                {
                    std::shared_ptr<THUAI8::Character> Character = Proto2THUAI8::Protobuf2THUAI8Character(item.character_message());
                    bufferState->enemyCharacters.push_back(Character);
                    logger->debug("Load EnemyCharacter!");
                }
                break;
            case THUAI8::MessageOfObj::BarracksMessage:
                if (item.barracks_message().team_id() == teamID || HaveOverView(item.barracks_message().x(), item.barracks_message().y()))
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.barracks_message().x()),
                        AssistFunction::GridToCell(item.barracks_message().y())
                    );
                    if (bufferState->mapInfo->barracksState.count(pos) == 0)
                    {
                        bufferState->mapInfo->barracksState.emplace(pos, std::pair(item.barracks_message().team_id(), item.barracks_message().hp()));
                        if (item.barracks_message().team_id() == teamID)
                            logger->debug("Load Barracks!");
                        else
                            logger->debug("Load EnemyBarracks!");
                    }
                    else
                    {
                        bufferState->mapInfo->barracksState[pos].first = item.barracks_message().team_id();
                        bufferState->mapInfo->barracksState[pos].second = item.barracks_message().hp();
                        if (item.barracks_message().team_id() == teamID)
                            logger->debug("Update Barracks!");
                        else
                            logger->debug("Update Enemy Barracks!");
                    }
                }
                break;
            case THUAI8::MessageOfObj::SpringMessage:
                if (item.spring_message().team_id() == teamID || HaveOverView(item.spring_message().x(), item.spring_message().y()))
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.spring_message().x()),
                        AssistFunction::GridToCell(item.spring_message().y())
                    );
                    if (bufferState->mapInfo->springState.count(pos) == 0)
                    {
                        bufferState->mapInfo->springState.emplace(pos, std::pair(item.spring_message().team_id(), item.spring_message().hp()));
                        if (item.spring_message().team_id() == teamID)
                            logger->debug("Load Spring!");
                        else
                            logger->debug("Load EnemySpring!");
                    }
                    else
                    {
                        bufferState->mapInfo->springState[pos].first = item.spring_message().team_id();
                        bufferState->mapInfo->springState[pos].second = item.spring_message().hp();
                        if (item.spring_message().team_id() == teamID)
                            logger->debug("Update Spring!");
                        else
                            logger->debug("Update EnemySpring!");
                    }
                }
                break;
            case THUAI8::MessageOfObj::FarmMessage:
                if (item.farm_message().team_id() == teamID || HaveOverView(item.farm_message().x(), item.farm_message().y()))
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.farm_message().x()),
                        AssistFunction::GridToCell(item.farm_message().y())
                    );
                    if (bufferState->mapInfo->farmState.count(pos) == 0)
                    {
                        bufferState->mapInfo->farmState.emplace(pos, std::pair(item.farm_message().team_id(), item.farm_message().hp()));
                        if (item.farm_message().team_id() == teamID)
                            logger->debug("Load Farm!");
                        else
                            logger->debug("Load EnemyFarm!");
                    }
                    else
                    {
                        bufferState->mapInfo->farmState[pos].first = item.farm_message().team_id();
                        bufferState->mapInfo->farmState[pos].second = item.farm_message().hp();
                        if (item.farm_message().team_id() == teamID)
                            logger->debug("Update Farm!");
                        else
                            logger->debug("Update EnemyFarm!");
                    }
                }
                break;
            case THUAI8::MessageOfObj::TrapMessage:
                if (item.trap_message().team_id() == teamID || HaveOverTrapView(item.trap_message().x(), item.trap_message().y()))
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.trap_message().x()),
                        AssistFunction::GridToCell(item.trap_message().y())
                    );
                    if (bufferState->mapInfo->trapState.count(pos) == 0)
                    {
                        bufferState->mapInfo->trapState.emplace(pos, item.trap_message().team_id());
                        if (item.trap_message().team_id() == teamID)
                            logger->debug("Load Trap!");
                        else
                            logger->debug("Load EnemyTrap!");
                    }
                    else
                    {
                        bufferState->mapInfo->trapState[pos] = item.trap_message().team_id();
                        if (item.trap_message().team_id() == teamID)
                            logger->debug("Update Trap!");
                        else
                            logger->debug("Update EnemyTrap!");
                    }
                }
                break;
            case THUAI8::MessageOfObj::EconomyResourceMessage:
                auto pos = THUAI8::cellxy_t(
                    AssistFunction::GridToCell(item.economy_resource_message().x()),
                    AssistFunction::GridToCell(item.economy_resource_message().y())
                );
                if (bufferState->mapInfo->economyResourceState.count(pos) == 0)
                {
                    bufferState->mapInfo->economyResourceState.emplace(pos, item.economy_resource_message().hp());
                    logger->debug("Load EconomyResource!");
                }
                else
                {
                    bufferState->mapInfo->economyResourceState[pos] = item.economy_resource_message().hp();
                    logger->debug("Update EconomyResource!");
                }
                break;
            case THUAI8::MessageOfObj::AdditionResourceMessage:
                auto pos = THUAI8::cellxy_t(
                    AssistFunction::GridToCell(item.addition_resource_message().x()),
                    AssistFunction::GridToCell(item.addition_resource_message().y())
                );
                if (bufferState->mapInfo->additionResourceState.count(pos) == 0)
                {
                    bufferState->mapInfo->additionResourceState.emplace(pos, std::pair(item.addition_resource_message().hp(), item.addition_resource_message().addition_resource_type()));
                    logger->debug("Load AdditionResource!");
                }
                else
                {
                    bufferState->mapInfo->additionResourceState[pos].first = item.addition_resource_message().hp();
                    bufferState->mapInfo->additionResourceState[pos].second = item.addition_resource_message().addition_resource_type();
                    logger->debug("Update AdditionResource!");
                }
                break;
            case THUAI8::MessageOfObj::NewsMessage:
                auto& news = item.news_message();
                if (news.to_id() == playerID && news.team_id() == teamID)
                {
                    if (Proto2THUAI8::newsTypeDict[news.news_case()] == THUAI8::NewsType::TextMessage)
                    {
                        messageQueue.emplace(std::pair(news.from_id(), news.text_message()));
                        logger->debug("Load Text News!");
                    }
                    else if (Proto2THUAI8::newsTypeDict[news.news_case()] == THUAI8::NewsType::BinaryMessage)
                    {
                        messageQueue.emplace(std::pair(news.from_id(), news.binary_message()));
                        logger->debug("Load Binary News!");
                    }
                    else
                        logger->error("Unknown NewsType!");
                }
                break;
            case THUAI8::MessageOfObj::NullMessageOfObj:
            default:
                break;
        }
    }
}
