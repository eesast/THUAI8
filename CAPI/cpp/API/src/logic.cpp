#include "logic.h"
#include "structures.h"
#include <grpcpp/grpcpp.h>
#include <spdlog/spdlog.h>
#include <spdlog/sinks/basic_file_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <functional>
#include "utils.hpp"
#include "Communication.h"
#include <memory>
#undef GetMessage
#undef SendMessage
#undef PeekMessage

extern const bool asynchronous;

Logic::Logic(int32_t pID, int32_t tID, THUAI8::PlayerType pType, THUAI8::CharacterType cType, bool side_flag) :
    playerID(pID),
    teamID(tID),
    playerType(pType),
    CharacterType(cType),
    side_flag(side_flag)
{
    currentState = &state[0];
    bufferState = &state[1];
    currentState->gameInfo = std::make_shared<THUAI8::GameInfo>();
    currentState->mapInfo = std::make_shared<THUAI8::GameMap>();
    bufferState->gameInfo = std::make_shared<THUAI8::GameInfo>();
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
    logger->debug("Called GetSelfInfo");
    return currentState->characterSelf;
}

std::shared_ptr<const THUAI8::Team> Logic::TeamGetSelfInfo() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called TeamGetSelfInfo");
    return this->currentState->teamSelf;
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
        return THUAI8::PlaceType::NullPlaceType;
    }
    logger->debug("Called GetPlaceType");
    return currentState->gameMap[cellX][cellY];
}

std::optional<THUAI8::EconomyResourceState> Logic::GetEnconomyResourceState(int32_t cellX, int32_t cellY) const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetEconomyResourceState");
    auto pos = THUAI8::cellxy_t(cellX, cellY);
    auto it = currentState->mapInfo->economyResource.find(pos);
    if (it != currentState->mapInfo->economyResource.end())
    {
        return THUAI8::EconomyResourceState(currentState->mapInfo->economyResource[pos]);
    }
    else
    {
        logger->warn("EconomyResource not found");
        return std::nullopt;
    }
}

std::optional<std::pair<int32_t, int32_t>> Logic::GetAdditionResourceState(int32_t cellX, int32_t cellY) const
{
    std::unique_lock<std::mutex> lock(mtxState);
    logger->debug("Called GetAdditionResourceState");
    auto pos = THUAI8::cellxy_t(cellX, cellY);
    auto it = currentState->mapInfo->additionResource.find(pos);
    if (it != currentState->mapInfo->additionResource.end())
    {
        return it->second;  // 直接返回存储的pair<int64_t, int32_t>
    }
    else
    {
        logger->warn("AdditionResource not found");
        return std::nullopt;
    }
}

/* std::optional<THUAI8::ConstructionState> Logic::GetConstructionState(int32_t cellX, int32_t cellY) const
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
}*/

int32_t Logic::GetEnergy() const
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

bool Logic::Common_Attack(int64_t playerID, int64_t teamID, int64_t attacked_playerID, int64_t attacked_teamID)
{
    logger->debug("Called Attack");
    return pComm->Common_Attack(playerID, teamID, attacked_playerID, attacked_playerID);
}

bool Logic::Skill_Attack(int64_t playerID, int64_t teamID, double angle)
{
    logger->debug("Called SkillAttack");
    return pComm->Skill_Attack(playerID, teamID, angle);
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
/* bool Logic::Recycle(int32_t playerID, int32_t targetID)
{
    logger->debug("Called Recycle");
    return pComm->Recycle(playerID, targetID);
}*/

bool Logic::Produce(int64_t playerID, int64_t teamID)
{
    logger->debug("Called Produce");
    return pComm->Produce(playerID, teamID);
}

bool Logic::Move(int64_t teamID, int64_t characterID, int32_t moveTimeInMilliseconds, double angle)
{
    logger->debug("Called Move");
    return pComm->Move(teamID, characterID, moveTimeInMilliseconds, angle);
}

/*bool Logic::Rebuild(THUAI8::ConstructionType constructionType)
{
    logger->debug("Called Rebuild");
    return pComm->Rebuild(playerID, teamID, constructionType);
}*/

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
            pComm->AddPlayer(playerID, teamID, CharacterType, side_flag);
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
            if (Proto2THUAI8::messageOfObjDict[item.message_of_obj_case()] == THUAI8::MessageOfObj::CharacterMessage && item.character_message().player_id() == playerID && item.character_message().team_id() == teamID)
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
                {
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

                        if (AssistFunction::HaveView(x, y, item.character_message().x(), item.character_message().y(), viewRange, bufferState->gameMap) && !item.character_message().is_invisible())
                        {
                            std::shared_ptr<THUAI8::Character> Character = Proto2THUAI8::Protobuf2THUAI8Character(item.character_message());
                            bufferState->characters.push_back(Character);
                            logger->debug("Load Character!");
                        }
                    break;
                }
            case THUAI8::MessageOfObj::BarracksMessage:
                {
                    if (item.barracks_message().team_id() == teamID || AssistFunction::HaveView(x, y, item.barracks_message().x(), item.barracks_message().y(), viewRange, bufferState->gameMap))
                    {
                        auto pos = THUAI8::cellxy_t(
                            AssistFunction::GridToCell(item.barracks_message().x()),
                            AssistFunction::GridToCell(item.barracks_message().y())
                        );
                        if (bufferState->mapInfo->barracksState.count(pos) == 0)
                        {
                            // bufferState->mapInfo->barracksState.emplace(pos, std::pair(item.barracks_message().team_id(), item.barracks_message().hp()));
                            bufferState->mapInfo->barracksState.emplace(
                                std::piecewise_construct,
                                std::forward_as_tuple(pos.first, pos.second),
                                std::forward_as_tuple(
                                    static_cast<int64_t>(item.barracks_message().team_id()),
                                    static_cast<int32_t>(item.barracks_message().hp())
                                )
                            );
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
                }
            case THUAI8::MessageOfObj::SpringMessage:
                {
                    if (item.spring_message().team_id() == teamID || AssistFunction::HaveView(x, y, item.spring_message().x(), item.spring_message().y(), viewRange, bufferState->gameMap))
                    {
                        auto pos = THUAI8::cellxy_t(
                            AssistFunction::GridToCell(item.spring_message().x()),
                            AssistFunction::GridToCell(item.spring_message().y())
                        );
                        if (bufferState->mapInfo->springState.count(pos) == 0)
                        {
                            // bufferState->mapInfo->springState.emplace(pos, std::pair(item.spring_message().team_id(), item.spring_message().hp()));
                            bufferState->mapInfo->springState.emplace(
                                std::piecewise_construct,
                                std::forward_as_tuple(pos.first, pos.second),  // 构造键 cellxy_t{pos.first, pos.second}
                                std::forward_as_tuple(item.spring_message().team_id(), item.spring_message().hp())
                            );
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
                }
            case THUAI8::MessageOfObj::FarmMessage:
                {
                    if (item.farm_message().team_id() == teamID || AssistFunction::HaveView(x, y, item.farm_message().x(), item.farm_message().y(), viewRange, bufferState->gameMap))
                    {
                        auto pos = THUAI8::cellxy_t(
                            AssistFunction::GridToCell(item.farm_message().x()),
                            AssistFunction::GridToCell(item.farm_message().y())
                        );
                        if (bufferState->mapInfo->farmState.count(pos) == 0)
                        {
                            // bufferState->mapInfo->farmState.emplace(pos, std::pair(item.farm_message().team_id(), item.farm_message().hp()));
                            bufferState->mapInfo->farmState.emplace(
                                std::piecewise_construct,
                                std::forward_as_tuple(pos.first, pos.second),  // 构造键 cellxy_t{pos.first, pos.second}
                                std::forward_as_tuple(item.farm_message().team_id(), item.farm_message().hp())
                            );
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
                }
            case THUAI8::MessageOfObj::TrapMessage:
                {
                    // 待定
                    if (item.trap_message().team_id() == teamID || AssistFunction::HaveView(x, y, item.trap_message().x(), item.trap_message().y(), viewRange, bufferState->gameMap) && currentState->characterSelf->visionBuffTime > 0)
                    {
                        auto pos = THUAI8::cellxy_t(
                            AssistFunction::GridToCell(item.trap_message().x()),
                            AssistFunction::GridToCell(item.trap_message().y())
                        );
                        if (bufferState->mapInfo->trapState.count(pos) == 0)
                        {
                            bufferState->mapInfo->trapState.emplace(
                                std::piecewise_construct,
                                std::forward_as_tuple(pos.first, pos.second),            // 构造键 cellxy_t{pos.first, pos.second}
                                std::forward_as_tuple(item.trap_message().team_id(), 0)  // 构造值 {team_id}
                            );

                            if (item.trap_message().team_id() == teamID)
                                logger->debug("Load Trap!");
                            else
                                logger->debug("Load EnemyTrap!");
                        }
                        else
                        {
                            bufferState->mapInfo->trapState[pos].second = item.trap_message().team_id();
                            if (item.trap_message().team_id() == teamID)
                                logger->debug("Update Trap!");
                            else
                                logger->debug("Update EnemyTrap!");
                        }
                    }
                    break;
                }
            case THUAI8::MessageOfObj::EconomyResourceMessage:
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.economy_resource_message().x()),
                        AssistFunction::GridToCell(item.economy_resource_message().y())
                    );
                    if (bufferState->mapInfo->economyResource.count(pos) == 0)
                    {
                        // bufferState->mapInfo->economyResource.emplace(pos, item.economy_resource_message().process());
                        bufferState->mapInfo->economyResource.emplace(
                            std::piecewise_construct,
                            std::forward_as_tuple(pos.first, pos.second),  // 构造键 cellxy_t{pos.first, pos.second}
                            std::forward_as_tuple(item.economy_resource_message().process())
                        );
                        logger->debug("Load EconomyResource!");
                    }
                    else
                    {
                        bufferState->mapInfo->economyResource[pos] = item.economy_resource_message().process();
                        logger->debug("Update EconomyResource!");
                    }
                    break;
                }
            case THUAI8::MessageOfObj::AdditionResourceMessage:
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.addition_resource_message().x()),
                        AssistFunction::GridToCell(item.addition_resource_message().y())
                    );
                    if (bufferState->mapInfo->additionResource.count(pos) == 0)
                    {
                        // bufferState->mapInfo->additionResource.emplace(pos, std::pair(item.addition_resource_message().hp(), item.addition_resource_message().addition_resource_type()));
                        //   显式将枚举转换为整数
                        bufferState->mapInfo->additionResource.emplace(
                            std::piecewise_construct,
                            std::forward_as_tuple(pos.first, pos.second),
                            std::forward_as_tuple(
                                static_cast<int32_t>(item.addition_resource_message().hp()),
                                static_cast<int32_t>(item.addition_resource_message().addition_resource_type())  // 枚举转 int
                            )
                        );
                        logger->debug("Load AdditionResource!");
                    }
                    else
                    {
                        bufferState->mapInfo->additionResource[pos].first = item.addition_resource_message().hp();
                        bufferState->mapInfo->additionResource[pos].second = item.addition_resource_message().addition_resource_type();
                        logger->debug("Update AdditionResource!");
                    }
                    break;
                }
            case THUAI8::MessageOfObj::NewsMessage:
                {
                    auto& news = item.news_message();
                    if (news.to_id() == playerID && news.team_id() == teamID)
                    {
                        if (Proto2THUAI8::newsTypeDict[news.news_case()] == THUAI8::NewsType::TextMessage)
                        {
                            // 显式指定 pair 的模板参数类型（假设 key 为 int32_t，value 为 std::string）
                            messageQueue.emplace(std::pair<int32_t, std::string>(static_cast<int32_t>(news.from_id()), news.text_message()));
                            logger->debug("Load Text News!");
                        }
                        else if (Proto2THUAI8::newsTypeDict[news.news_case()] == THUAI8::NewsType::BinaryMessage)
                        {
                            // 显式指定 pair 的模板参数类型（假设 key 为 int32_t，value 为 std::string）
                            messageQueue.emplace(std::pair<int32_t, std::string>(static_cast<int32_t>(news.from_id()), news.binary_message()));
                            logger->debug("Load Binary News!");
                        }
                        else
                            logger->error("Unknown NewsType!");
                    }
                    break;
                }
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
                {
                    if (item.character_message().team_id() != teamID && HaveOverView(item.character_message().x(), item.character_message().y()) && !item.character_message().is_invisible())
                    {
                        std::shared_ptr<THUAI8::Character> Character = Proto2THUAI8::Protobuf2THUAI8Character(item.character_message());
                        bufferState->enemyCharacters.push_back(Character);
                        logger->debug("Load EnemyCharacter!");
                    }
                    else if (item.character_message().team_id() == teamID && playerID != item.character_message().player_id())
                    {
                        std::shared_ptr<THUAI8::Character> Character = Proto2THUAI8::Protobuf2THUAI8Character(item.character_message());
                        bufferState->characters.push_back(Character);
                        logger->debug("Load Character!");
                    }
                    else if (item.character_message().team_id() == teamID && playerID == item.character_message().player_id())
                    {
                        bufferState->characterSelf = Proto2THUAI8::Protobuf2THUAI8Character(item.character_message());
                        logger->debug("Load Self Character!");
                    }
                    break;
                }
            case THUAI8::MessageOfObj::BarracksMessage:
                {
                    if (item.barracks_message().team_id() == teamID || HaveOverView(item.barracks_message().x(), item.barracks_message().y()))
                    {
                        auto pos = THUAI8::cellxy_t(
                            AssistFunction::GridToCell(item.barracks_message().x()),
                            AssistFunction::GridToCell(item.barracks_message().y())
                        );
                        if (bufferState->mapInfo->barracksState.count(pos) == 0)
                        {
                            // bufferState->mapInfo->barracksState.emplace(pos, std::pair(item.barracks_message().team_id(), item.barracks_message().hp()));
                            bufferState->mapInfo->barracksState.emplace(
                                std::piecewise_construct,
                                std::forward_as_tuple(pos.first, pos.second),
                                std::forward_as_tuple(
                                    static_cast<int64_t>(item.barracks_message().team_id()),
                                    static_cast<int32_t>(item.barracks_message().hp())
                                )
                            );
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
                }
            case THUAI8::MessageOfObj::SpringMessage:
                {
                    if (item.spring_message().team_id() == teamID || HaveOverView(item.spring_message().x(), item.spring_message().y()))
                    {
                        auto pos = THUAI8::cellxy_t(
                            AssistFunction::GridToCell(item.spring_message().x()),
                            AssistFunction::GridToCell(item.spring_message().y())
                        );
                        if (bufferState->mapInfo->springState.count(pos) == 0)
                        {
                            // bufferState->mapInfo->springState.emplace(pos, std::pair(item.spring_message().team_id(), item.spring_message().hp()));
                            bufferState->mapInfo->springState.emplace(
                                std::piecewise_construct,
                                std::forward_as_tuple(pos.first, pos.second),  // 构造键 cellxy_t{pos.first, pos.second}
                                std::forward_as_tuple(item.spring_message().team_id(), item.spring_message().hp())
                            );
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
                }
            case THUAI8::MessageOfObj::FarmMessage:
                {
                    if (item.farm_message().team_id() == teamID || HaveOverView(item.farm_message().x(), item.farm_message().y()))
                    {
                        auto pos = THUAI8::cellxy_t(
                            AssistFunction::GridToCell(item.farm_message().x()),
                            AssistFunction::GridToCell(item.farm_message().y())
                        );
                        if (bufferState->mapInfo->farmState.count(pos) == 0)
                        {
                            // bufferState->mapInfo->farmState.emplace(pos, std::pair(item.farm_message().team_id(), item.farm_message().hp()));
                            bufferState->mapInfo->farmState.emplace(
                                std::piecewise_construct,
                                std::forward_as_tuple(pos.first, pos.second),  // 构造键 cellxy_t{pos.first, pos.second}
                                std::forward_as_tuple(item.farm_message().team_id(), item.farm_message().hp())
                            );
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
                }
            case THUAI8::MessageOfObj::TrapMessage:
                {
                    if (item.trap_message().team_id() == teamID || HaveOverTrapView(item.trap_message().x(), item.trap_message().y()))
                    {
                        auto pos = THUAI8::cellxy_t(
                            AssistFunction::GridToCell(item.trap_message().x()),
                            AssistFunction::GridToCell(item.trap_message().y())
                        );
                        if (bufferState->mapInfo->trapState.count(pos) == 0)
                        {
                            bufferState->mapInfo->trapState.emplace(
                                std::piecewise_construct,
                                std::forward_as_tuple(pos.first, pos.second),            // 构造键 cellxy_t{pos.first, pos.second}
                                std::forward_as_tuple(item.trap_message().team_id(), 0)  // 构造值 {team_id}
                            );

                            if (item.trap_message().team_id() == teamID)
                                logger->debug("Load Trap!");
                            else
                                logger->debug("Load EnemyTrap!");
                        }
                        else
                        {
                            bufferState->mapInfo->trapState[pos].second = item.trap_message().team_id();
                            if (item.trap_message().team_id() == teamID)
                                logger->debug("Update Trap!");
                            else
                                logger->debug("Update EnemyTrap!");
                        }
                    }
                    break;
                }
            case THUAI8::MessageOfObj::EconomyResourceMessage:
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.economy_resource_message().x()),
                        AssistFunction::GridToCell(item.economy_resource_message().y())
                    );
                    if (bufferState->mapInfo->economyResource.count(pos) == 0)
                    {
                        // bufferState->mapInfo->economyResource.emplace(pos, item.economy_resource_message().process());
                        bufferState->mapInfo->economyResource.emplace(
                            std::piecewise_construct,
                            std::forward_as_tuple(pos.first, pos.second),                     // 构造键 cellxy_t{pos.first, pos.second}
                            std::forward_as_tuple(item.economy_resource_message().process())  // 构造值 {team_id}
                        );
                        logger->debug("Load EconomyResource!");
                    }
                    else
                    {
                        bufferState->mapInfo->economyResource[pos] = item.economy_resource_message().process();
                        logger->debug("Update EconomyResource!");
                    }
                    break;
                }
            case THUAI8::MessageOfObj::AdditionResourceMessage:
                {
                    auto pos = THUAI8::cellxy_t(
                        AssistFunction::GridToCell(item.addition_resource_message().x()),
                        AssistFunction::GridToCell(item.addition_resource_message().y())
                    );
                    if (bufferState->mapInfo->additionResource.count(pos) == 0)
                    {
                        // bufferState->mapInfo->additionResource.emplace(pos, std::pair(item.addition_resource_message().hp(), item.addition_resource_message().addition_resource_type()));
                        //   显式将枚举转换为整数
                        bufferState->mapInfo->additionResource.emplace(
                            std::piecewise_construct,
                            std::forward_as_tuple(pos.first, pos.second),
                            std::forward_as_tuple(
                                static_cast<int32_t>(item.addition_resource_message().hp()),
                                static_cast<int32_t>(item.addition_resource_message().addition_resource_type())  // 枚举转 int
                            )
                        );
                        logger->debug("Load AdditionResource!");
                    }
                    else
                    {
                        bufferState->mapInfo->additionResource[pos].first = item.addition_resource_message().hp();
                        bufferState->mapInfo->additionResource[pos].second = item.addition_resource_message().addition_resource_type();
                        logger->debug("Update AdditionResource!");
                    }
                    break;
                }
            case THUAI8::MessageOfObj::NewsMessage:
                {
                    auto& news = item.news_message();
                    if (news.to_id() == playerID && news.team_id() == teamID)
                    {
                        if (Proto2THUAI8::newsTypeDict[news.news_case()] == THUAI8::NewsType::TextMessage)
                        {
                            messageQueue.emplace(std::pair<int32_t, std::string>(static_cast<int32_t>(news.from_id()), news.text_message()));

                            logger->debug("Load Text News!");
                        }
                        else if (Proto2THUAI8::newsTypeDict[news.news_case()] == THUAI8::NewsType::BinaryMessage)
                        {
                            messageQueue.emplace(std::pair<int32_t, std::string>(static_cast<int32_t>(news.from_id()), news.binary_message()));

                            logger->debug("Load Binary News!");
                        }
                        else
                            logger->error("Unknown NewsType!");
                    }
                    break;
                }
            case THUAI8::MessageOfObj::NullMessageOfObj:
            default:
                break;
        }
    }
}
void Logic::LoadBuffer(const protobuf::MessageToClient& message)
{
    // 将消息读入到buffer中
    {
        std::lock_guard<std::mutex> lock(mtxBuffer);

        // 清空原有信息
        bufferState->characters.clear();
        bufferState->enemyCharacters.clear();
        bufferState->guids.clear();
        bufferState->allGuids.clear();
        logger->info("Buffer cleared!");
        // 读取新的信息
        for (const auto& obj : message.obj_message())
            if (Proto2THUAI8::messageOfObjDict[obj.message_of_obj_case()] == THUAI8::MessageOfObj::CharacterMessage)
            {
                bufferState->allGuids.push_back(obj.character_message().guid());
                if (obj.character_message().team_id() == teamID)
                    bufferState->guids.push_back(obj.character_message().guid());
            }
        bufferState->gameInfo = Proto2THUAI8::Protobuf2THUAI8GameInfo(message.all_message());
        LoadBufferSelf(message);
        if (playerType == THUAI8::PlayerType::Character && !bufferState->characterSelf)
        {
            logger->info("exit for nullSelf");
            return;
        }
        for (const auto& item : message.obj_message())
            LoadBufferCase(item);
    }
    if (asynchronous)
    {
        {
            std::lock_guard<std::mutex> lock(mtxState);
            std::swap(currentState, bufferState);
            counterState = counterBuffer;
            logger->info("Update State!");
        }
        freshed = true;
    }
    else
    {
        bufferUpdated = true;
    }
    counterBuffer++;
    // 唤醒其他线程
    cvBuffer.notify_one();
}
void Logic::Update() noexcept
{
    if (!asynchronous)
    {
        std::unique_lock<std::mutex> lock(mtxBuffer);
        // 缓冲区被更新之后才可以使用
        cvBuffer.wait(lock, [this]()
                      { return bufferUpdated; });
        {
            std::lock_guard<std::mutex> stateLock(mtxState);
            std::swap(currentState, bufferState);
            counterState = counterBuffer;
        }
        bufferUpdated = false;
        logger->info("Update State!");
    }
}
void Logic::Wait() noexcept
{
    freshed = false;
    {
        std::unique_lock<std::mutex> lock(mtxBuffer);
        cvBuffer.wait(lock, [this]()
                      { return freshed.load(); });
    }
}

void Logic::UnBlockAI()
{
    {
        std::lock_guard<std::mutex> lock(mtxAI);
        AIStart = true;
    }
    cvAI.notify_one();
}

int32_t Logic::GetCounter() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    return counterState;
}

std::vector<int64_t> Logic::GetPlayerGUIDs() const
{
    std::unique_lock<std::mutex> lock(mtxState);
    return currentState->guids;
}

bool Logic::TryConnection()
{
    logger->info("Try to connect to server...");
    return pComm->TryConnection(playerID, teamID);
}

bool Logic::HaveView(int32_t x, int32_t y, int32_t newX, int32_t newY, int32_t viewRange, std::vector<std::vector<THUAI8::PlaceType>>& map) const
{
    std::unique_lock<std::mutex> lock(mtxState);
    return AssistFunction::HaveView(x, y, newX, newY, viewRange, map);
}

void Logic::Main(CreateAIFunc createAI, std::string IP, std::string port, bool file, bool print, bool warnOnly, bool side_flag)
{
    // 建立日志组件
    auto fileLogger = std::make_shared<spdlog::sinks::basic_file_sink_mt>(fmt::format("logs/logic-{}-{}-log.txt", playerID, teamID), true);
    auto printLogger = std::make_shared<spdlog::sinks::stdout_color_sink_mt>();
    std::string pattern = "[logic] [%H:%M:%S.%e] [%l] %v";
    fileLogger->set_pattern(pattern);
    printLogger->set_pattern(pattern);
    if (file)
        fileLogger->set_level(spdlog::level::debug);
    else
        fileLogger->set_level(spdlog::level::off);
    if (print)
        printLogger->set_level(spdlog::level::info);
    else
        printLogger->set_level(spdlog::level::off);
    if (warnOnly)
        printLogger->set_level(spdlog::level::warn);
    logger = std::make_unique<spdlog::logger>("logicLogger", spdlog::sinks_init_list{fileLogger, printLogger});

    logger->flush_on(spdlog::level::warn);
    // 打印当前的调试信息
    logger->info("TeamID={}", teamID);
    logger->info("*********Basic Info*********");
    logger->info("asynchronous: {}", asynchronous);
    logger->info("server: {}:{}", IP, port);
    if (playerType == THUAI8::PlayerType::Character)
        logger->info("Character ID: {}", playerID);
    logger->info("player team: {}", THUAI8::playerTeamDict[playerTeam]);
    logger->info("****************************");

    // 建立与服务器之间通信的组件
    pComm = std::make_unique<Communication>(IP, port);

    // 构造timer
    if (playerType == THUAI8::PlayerType::Character)
    {
        if (!file && !print)
            timer = std::make_unique<CharacterAPI>(*this);
        else
            timer = std::make_unique<CharacterDebugAPI>(*this, file, print, warnOnly, playerID, teamID);
    }
    else
    {
        if (!file && !print)
            timer = std::make_unique<TeamAPI>(*this);
        else
            timer = std::make_unique<TeamDebugAPI>(*this, file, print, warnOnly, playerID, teamID);
    }

    // 构造AI线程
    auto AIThread = [&]()
    {
        try
        {
            {
                std::unique_lock<std::mutex> lock(mtxAI);
                cvAI.wait(lock, [this]()
                          { return AIStart; });
            }
            auto ai = createAI(playerID);

            while (AILoop)
            {
                if (asynchronous)
                {
                    Wait();
                    timer->StartTimer();
                    timer->Play(*ai);
                    timer->EndTimer();
                }
                else
                {
                    Update();
                    timer->StartTimer();
                    timer->Play(*ai);
                    timer->EndTimer();
                }
            }
        }
        catch (const std::exception& e)
        {
            std::cerr << "C++ Exception: " << e.what() << std::endl;
        }
        catch (...)
        {
            std::cerr << "Unknown Exception!" << std::endl;
        }
    };

    // 连接服务器
    if (TryConnection())
    {
        logger->info("Connect to the server successfully, AI thread will be started.");
        tAI = std::thread(AIThread);
        if (tAI.joinable())
        {
            logger->info("Join the AI thread!");
            // 首先开启处理消息的线程
            ProcessMessage();
            tAI.join();
        }
    }
    else
    {
        AILoop = false;
        logger->error("Connect to the server failed, AI thread will not be started.");
        return;
    }
}