#include <optional>
#include <string>
#include "AI.h"
#include "API.h"
#include "utils.hpp"
#include "structures.h"
#include <memory>
#undef GetMessage
#undef SendMessage
#undef PeekMessage

#define PI 3.14159265358979323846

CharacterDebugAPI::CharacterDebugAPI(ILogic& logic, bool file, bool print, bool warnOnly, int32_t CharacterID, int32_t TeamID) :
    logic(logic),
    logger(nullptr)  // 显式初始化 logger 为 nullptr
{
    std::string fileName = "logs/api-" + std::to_string(TeamID) + "-" + std::to_string(CharacterID) + "log.txt";
    auto fileLogger = std::make_shared<spdlog::sinks::basic_file_sink_mt>(fileName, true);
    auto printLogger = std::make_shared<spdlog::sinks::stdout_color_sink_mt>();
    std::string pattern = "[api " + std::to_string(TeamID) + std::to_string(CharacterID) + "] [%H:%M:%S.%e] [%l] %v";
    fileLogger->set_pattern(pattern);
    printLogger->set_pattern(pattern);
    if (file)
        fileLogger->set_level(spdlog::level::trace);
    else
        fileLogger->set_level(spdlog::level::off);
    if (print)
        printLogger->set_level(spdlog::level::info);
    else
        printLogger->set_level(spdlog::level::off);
    if (warnOnly)
        printLogger->set_level(spdlog::level::warn);
    logger = std::make_unique<spdlog::logger>("apiLogger", spdlog::sinks_init_list{fileLogger, printLogger});
    logger->flush_on(spdlog::level::warn);
}

void CharacterDebugAPI::StartTimer()
{
    startPoint = std::chrono::system_clock::now();
    std::time_t t = std::chrono::system_clock::to_time_t(startPoint);
    logger->info("=== AI.play() ===");
    logger->info("StartTimer: {}", std::ctime(&t));
}

void CharacterDebugAPI::EndTimer()
{
    logger->info("Time elapsed: {}ms", Time::TimeSinceStart(startPoint));
}

int32_t CharacterDebugAPI::GetFrameCount() const
{
    return logic.GetCounter();
}

std::future<bool> CharacterDebugAPI::SendTextMessage(int32_t toID, std::string message)
{
    logger->info("SendTextMessage: toID = {}, message = {}, called at {}ms", toID, message, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=, message = std::move(message)]()
                      { auto result = logic.Send(toID, std::move(message), false);
                        if (!result)
                            logger->warn("SendTextMessage: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

std::future<bool> CharacterDebugAPI::SendBinaryMessage(int32_t toID, std::string message)
{
    logger->info("SendBinaryMessage: toID = {}, message = {}, called at {}ms", toID, message, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=, message = std::move(message)]()
                      { auto result = logic.Send(toID, std::move(message), true);
                        if (!result)
                            logger->warn("SendBinaryMessage: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

bool CharacterDebugAPI::HaveMessage()
{
    logger->info("HaveMessage: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.HaveMessage();
    if (!result)
        logger->warn("HaveMessage: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}
bool CharacterDebugAPI::HaveView(int32_t x, int32_t y, int32_t newX, int32_t newY, int32_t viewRange, std::vector<std::vector<THUAI8::PlaceType>>& map) const
{
    auto selfInfo = GetSelfInfo();
    return logic.HaveView(selfInfo->x, selfInfo->y, newX, newX, selfInfo->viewRange, map);
}

std::pair<int32_t, std::string> CharacterDebugAPI::GetMessage()
{
    logger->info("GetMessage: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetMessage();
    if (result.first == -1)
        logger->warn("GetMessage: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

bool CharacterDebugAPI::Wait()
{
    logger->info("Wait: called at {}ms", Time::TimeSinceStart(startPoint));
    if (logic.GetCounter() == -1)
        return false;
    else
        return logic.WaitThread();
}


std::future<bool> CharacterDebugAPI::Move(int64_t timeInMilliseconds, double angleInRadian)
{
    logger->info("Move: timeInMilliseconds = {}, angleInRadian = {}, called at {}ms", timeInMilliseconds, angleInRadian, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.Move(timeInMilliseconds, angleInRadian);
                        if (!result)
                            logger->warn("Move: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}


std::future<bool> CharacterDebugAPI::MoveDown(int64_t timeInMilliseconds)
{
    return Move(timeInMilliseconds, 0);  
}

std::future<bool> CharacterDebugAPI::MoveRight(int64_t timeInMilliseconds)
{
    return Move(timeInMilliseconds, PI * 0.5);  
}

std::future<bool> CharacterDebugAPI::MoveUp(int64_t timeInMilliseconds)
{
    return Move(timeInMilliseconds, PI);  
}

std::future<bool> CharacterDebugAPI::MoveLeft(int64_t timeInMilliseconds)
{
    return Move(timeInMilliseconds, PI * 1.5);
}

std::future<bool> CharacterDebugAPI::Skill_Attack(double angle)
{
    logger->info("Skill_Attack: player={}, teamID={}, called@{}ms", this->GetSelfInfo()->playerID, this->GetSelfInfo()->teamID, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      {
        auto result = logic.Skill_Attack(this->GetSelfInfo()->playerID,this->GetSelfInfo()->teamID,angle); // 改为传递玩家ID
        if (!result)
            logger->warn("Skill_Attack failed@{}ms", Time::TimeSinceStart(startPoint));
        return result; });
}

std::future<bool> CharacterDebugAPI::Common_Attack(int64_t attackedPlayerID)
{
    logger->info("characterID={}, teamID={}, Common_Attack: target={}, called@{}ms", this->GetSelfInfo()->playerID, this->GetSelfInfo()->teamID, attackedPlayerID, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      {
        auto result = logic.Common_Attack(this->GetSelfInfo()->teamID,this->GetSelfInfo()->playerID,1-this->GetSelfInfo()->teamID, attackedPlayerID); // 改为传递玩家ID
        if (!result)
            logger->warn("Common_Attack failed@{}ms", Time::TimeSinceStart(startPoint));
        return result; });
}

std::future<bool> CharacterDebugAPI::AttackConstruction(int64_t teamID, int64_t playerID)
{
    logger->info("AttackConstruction: teamID = {}, playerID = {}, called at {}ms", teamID, playerID, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.AttackConstruction(teamID, playerID);
                        if (!result)
                            logger->warn("AttackConstruction: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

std::future<bool> CharacterDebugAPI::AttackAdditionResource(int64_t teamID, int64_t playerID)
{
    logger->info("AttackAdditionResource: teamID = {}, playerID = {}, called at {}ms", teamID, playerID, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.AttackAdditionResource(teamID, playerID);
                        if (!result)
                            logger->warn("AttackAdditionResource: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

std::future<bool> CharacterDebugAPI::Recover(int64_t recover)
{
    logger->info("Recover: recover = {}, called at {}ms", recover, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.Recover(recover);
                        if (!result)
                            logger->warn("Recover: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}
std::future<bool> CharacterDebugAPI::Produce(int64_t playerID, int64_t teamID)
{
    logger->info("Harvest: called at {}ms", Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.Produce(playerID, teamID);
                        if (!result)
                            logger->warn("Harvest: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}


std::future<bool> CharacterDebugAPI::Construct(THUAI8::ConstructionType constructionType)
{
    logger->info("Construct: constructionType = {}, called at {}ms", constructionType, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.Construct(constructionType);
                        if (!result)
                            logger->warn("Construct: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

std::vector<std::shared_ptr<const THUAI8::Character>> CharacterDebugAPI::GetCharacters() const
{
    logger->info("GetCharacters: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetCharacters();
    if (result.empty())
        logger->warn("GetCharacters: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::vector<std::shared_ptr<const THUAI8::Character>> CharacterDebugAPI::GetEnemyCharacters() const
{
    logger->info("GetEnemyCharacters: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetEnemyCharacters();
    if (result.empty())
        logger->warn("GetEnemyCharacters: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::vector<std::vector<THUAI8::PlaceType>> CharacterDebugAPI::GetFullMap() const
{
    logger->info("GetFullMap: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetFullMap();
    if (result.empty())
        logger->warn("GetFullMap: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::shared_ptr<const THUAI8::GameInfo> CharacterDebugAPI::GetGameInfo() const
{
    logger->info("GetGameInfo: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetGameInfo();
    if (result == nullptr)
        logger->warn("GetGameInfo: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::optional<THUAI8::EconomyResourceState> CharacterDebugAPI::GetEnconomyResourceState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetEnconomyResourceState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetEnconomyResourceState(cellX, cellY);
    if (!result)
        logger->warn("GetEnconomyResourceState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::optional<std::pair<int32_t, int32_t>> CharacterDebugAPI::GetAdditionResourceState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetAdditionResourceState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetAdditionResourceState(cellX, cellY);
    if (!result)
        logger->warn("GetAdditionResourceState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}
std::optional<std::pair<int32_t, int32_t>> TeamDebugAPI::GetAdditionResourceState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetAdditionResourceState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetAdditionResourceState(cellX, cellY);
    if (!result)
        logger->warn("GetAdditionResourceState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}
/* std::optional<THUAI8::ConstructionState> CharacterDebugAPI::GetConstructionState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetConstructionState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetConstructionState(cellX, cellY);
    if (!result)
        logger->warn("GetConstructionState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}*/

std::vector<int64_t> CharacterDebugAPI::GetPlayerGUIDs() const
{
    logger->info("GetPlayerGUIDs: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetPlayerGUIDs();
    if (result.empty())
        logger->warn("GetPlayerGUIDs: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

int32_t CharacterDebugAPI::GetEnergy() const
{
    logger->info("GetEnergy: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetEnergy();
    if (result == -1)
        logger->warn("GetEnergy: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

int32_t CharacterDebugAPI::GetScore() const
{
    logger->info("GetScore: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetScore();
    if (result == -1)
        logger->warn("GetScore: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::shared_ptr<const THUAI8::Character> CharacterDebugAPI::GetSelfInfo() const
{
    logger->info("GetSelfInfo: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.CharacterGetSelfInfo();
    if (result == nullptr)
        logger->warn("GetSelfInfo: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

/*bool CharacterDebugAPI::HaveView(int32_t targetX, int32_t targetY) const
{
    logger->info("HaveView: targetX = {}, targetY = {}, called at {}ms", targetX, targetY, Time::TimeSinceStart(startPoint));
    auto result = logic.HaveView(targetX, targetY);
    if (!result)
        logger->warn("HaveView: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}*/

void CharacterDebugAPI::Print(std::string str) const
{
    logger->info(str);
}

// facing direction存疑
void CharacterDebugAPI::PrintCharacter() const
{
    for (const auto& Character : logic.GetCharacters())
    {
        logger->info("******Character Info******");

        // 确保字典返回值是 std::string
        std::string characterType = THUAI8::characterTypeDict.at(Character->characterType);

        // 确保成员存在并类型正确
        int characterID = Character->playerID;
        int guid = Character->guid;
        int x = Character->x;
        int y = Character->y;

        logger->info("type={}, characterID={}, GUID={}, x={}, y={}", characterType, characterID, guid, x, y);

        // 确保字典返回值是 std::string
        std::string characterActiveState = THUAI8::characterStateDict.at(Character->characterActiveState);
        std::string characterPassiveState = THUAI8::characterStateDict.at(Character->characterPassiveState);
        // 确保成员存在并类型正确
        int speed = Character->speed;
        int viewRange = Character->viewRange;
        double facingDirection = Character->facingDirection;

        logger->info("Activestate={}, PassiveState={}, speed={}, view range={}, facing direction={}", characterActiveState, characterPassiveState, speed, viewRange, facingDirection);
        logger->info("************************\n");
    }
}

void CharacterDebugAPI::PrintSelfInfo() const
{
    auto self = logic.CharacterGetSelfInfo();
    if (!self)  // 检查 self 是否为空指针
    {
        logger->warn("PrintSelfInfo: self is null");
        return;
    }

    logger->info("******Self Info******");

    // 确保字典返回值是 std::string
    std::string characterType = THUAI8::characterTypeDict.at(self->characterType);
    std::string characterActiveState = THUAI8::characterStateDict.at(self->characterActiveState);
    std::string characterPassiveState = THUAI8::characterStateDict.at(self->characterPassiveState);

    // 打印基本信息
    logger->info("type={}, characterID={}, teamID={}, GUID={}, x={}, y={}", characterType, self->playerID, self->teamID, self->guid, self->x, self->y);

    // 打印状态信息
    logger->info("activestate={}, passivestate={}, speed={}, view range={}, facing direction={}", characterActiveState, characterPassiveState, self->speed, self->viewRange, self->facingDirection);

    logger->info("************************\n");
}

std::future<bool> CharacterDebugAPI::EndAllAction()
{
    return std::async(std::launch::async, [this]()
                      { return logic.EndAllAction(); });
}

TeamDebugAPI::TeamDebugAPI(ILogic& logic, bool file, bool print, bool warnOnly, int32_t playerID, int32_t teamID) :
    logic(logic)
{
    std::string fileName = "logs/api-" + std::to_string(playerID) + "-" + std::to_string(teamID) + "-log.txt";
    auto fileLogger = std::make_shared<spdlog::sinks::basic_file_sink_mt>(fileName, true);
    auto printLogger = std::make_shared<spdlog::sinks::stdout_color_sink_mt>();
    std::string pattern = "[api" + std::to_string(playerID) + "] [%H:%M:%S.%e] [%l] %v";
    fileLogger->set_pattern(pattern);
    printLogger->set_pattern(pattern);
    if (file)
        fileLogger->set_level(spdlog::level::trace);
    else
        fileLogger->set_level(spdlog::level::off);
    if (print)
        printLogger->set_level(spdlog::level::info);
    else
        printLogger->set_level(spdlog::level::off);
    if (warnOnly)
        printLogger->set_level(spdlog::level::warn);
    logger = std::make_unique<spdlog::logger>("apiLogger", spdlog::sinks_init_list{fileLogger, printLogger});
}

void TeamDebugAPI::StartTimer()
{
    startPoint = std::chrono::system_clock::now();
    std::time_t t = std::chrono::system_clock::to_time_t(startPoint);
    logger->info("=== AI.play() ===");
    logger->info("StartTimer: {}", std::ctime(&t));
}

void TeamDebugAPI::EndTimer()
{
    logger->info("Time elapsed: {}ms", Time::TimeSinceStart(startPoint));
}

int32_t TeamDebugAPI::GetFrameCount() const
{
    return logic.GetCounter();
}

std::future<bool> TeamDebugAPI::SendTextMessage(int32_t toID, std::string message)
{
    logger->info("SendTextMessage: toID = {}, message = {}, called at {}ms", toID, message, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=, message = std::move(message)]()
                      { auto result = logic.Send(toID, std::move(message), false);
                        if (!result)
                            logger->warn("SendTextMessage: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

std::future<bool> TeamDebugAPI::SendBinaryMessage(int32_t toID, std::string message)
{
    logger->info("SendBinaryMessage: toID = {}, message = {}, called at {}ms", toID, message, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=, message = std::move(message)]()
                      { auto result = logic.Send(toID, std::move(message), true);
                        if (!result)
                            logger->warn("SendBinaryMessage: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

bool TeamDebugAPI::HaveMessage()
{
    logger->info("HaveMessage: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.HaveMessage();
    if (!result)
        logger->warn("HaveMessage: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::pair<int32_t, std::string> TeamDebugAPI::GetMessage()
{
    logger->info("GetMessage: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetMessage();
    if (result.first == -1)
        logger->warn("GetMessage: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

bool TeamDebugAPI::Wait()
{
    logger->info("Wait: called at {}ms", Time::TimeSinceStart(startPoint));
    if (logic.GetCounter() == -1)
        return false;
    else
        return logic.WaitThread();
}

std::vector<std::shared_ptr<const THUAI8::Character>> TeamDebugAPI::GetCharacters() const
{
    logger->info("GetCharacters: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetCharacters();
    if (result.empty())
        logger->warn("GetCharacters: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::vector<std::shared_ptr<const THUAI8::Character>> TeamDebugAPI::GetEnemyCharacters() const
{
    logger->info("GetEnemyCharacters: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetEnemyCharacters();
    if (result.empty())
        logger->warn("GetEnemyCharacters: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::vector<std::vector<THUAI8::PlaceType>> TeamDebugAPI::GetFullMap() const
{
    logger->info("GetFullMap: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetFullMap();
    if (result.empty())
        logger->warn("GetFullMap: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::shared_ptr<const THUAI8::GameInfo> TeamDebugAPI::GetGameInfo() const
{
    logger->info("GetGameInfo: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetGameInfo();
    if (result == nullptr)
        logger->warn("GetGameInfo: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

THUAI8::PlaceType TeamDebugAPI::GetPlaceType(int32_t cellX, int32_t cellY) const
{
    logger->info("GetPlaceType: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    THUAI8::PlaceType result = logic.GetPlaceType(cellX, cellY);  // 直接获取返回值
    if (result == THUAI8::PlaceType::NullPlaceType)               // 假设 Unknown 是一个表示失败的默认值
    {
        logger->warn("GetPlaceType: failed at {}ms", Time::TimeSinceStart(startPoint));
        throw std::runtime_error("GetPlaceType failed");  // 如果失败，抛出异常
    }
    return result;  // 返回实际值
}

std::optional<THUAI8::EconomyResourceState> TeamDebugAPI::GetEnconomyResourceState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetEnconomyResourceState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetEnconomyResourceState(cellX, cellY);
    if (!result)
        logger->warn("GetEnconomyResourceState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

/* std::optional<THUAI8::ConstructionState> TeamDebugAPI::GetConstructionState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetConstructionState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetConstructionState(cellX, cellY);
    if (!result)
        logger->warn("GetConstructionState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}*/

std::vector<int64_t> TeamDebugAPI::GetPlayerGUIDs() const
{
    logger->info("GetPlayerGUIDs: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetPlayerGUIDs();
    if (result.empty())
        logger->warn("GetPlayerGUIDs: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

int32_t TeamDebugAPI::GetEnergy() const
{
    logger->info("GetEnergy: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetEnergy();
    if (result == -1)
        logger->warn("GetEnergy: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

int32_t TeamDebugAPI::GetScore() const
{
    logger->info("GetScore: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetScore();
    if (result == -1)
        logger->warn("GetScore: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::shared_ptr<const THUAI8::Team> TeamDebugAPI::GetSelfInfo() const
{
    logger->info("GetSelfInfo: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.TeamGetSelfInfo();  // 调用正确的逻辑函数
    if (result == nullptr)
    {
        logger->warn("GetSelfInfo: failed at {}ms", Time::TimeSinceStart(startPoint));
    }
    return result;
}

std::future<bool> TeamDebugAPI::InstallEquipment(int32_t playerID, THUAI8::EquipmentType equipmenttype)
{
    logger->info("InstallEquipment: playerID = {}, equipmenttype = {}, called at {}ms", playerID, equipmenttype, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.InstallEquipment(playerID, equipmenttype);
                        if (!result)
                            logger->warn("InstallEquipment: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

// recycle暂时闲置
std::future<bool> TeamDebugAPI::BuildCharacter(THUAI8::CharacterType characterType, int32_t birthIndex)
{
    logger->info("BuildCharacter: characterType = {}, birthIndex = {}, called at {}ms", characterType, birthIndex, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.BuildCharacter(characterType, birthIndex);
                        if (!result)
                            logger->warn("BuildCharacter: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

void TeamDebugAPI::PrintSelfInfo() const
{
    auto Team = logic.TeamGetSelfInfo();
    logger->info("******Self Info******");
    logger->info("teamID={}, playerID={}, score={}, energy={}", Team->teamID, Team->playerID, Team->score, Team->energy);
    logger->info("*********************\n");
}

void CharacterDebugAPI::Play(IAI& ai)
{
    ai.play(*this);
}

void TeamDebugAPI::Play(IAI& ai)
{
    ai.play(*this);
}

void TeamDebugAPI::Print(std::string str) const
{
    logger->info(str);
}

std::future<bool> TeamDebugAPI::EndAllAction()
{
    return std::async(std::launch::async, [this]()
                      { return logic.EndAllAction(); });
}