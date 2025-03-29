#include <optional>
#include <string>
#include "AI.h"
#include "API.h"
#include "utils.hpp"
#include "structures.h"

#undef GetMessage
#undef SendMessage
#undef PeekMessage

#define PI 3.14159265358979323846

CharacterDebugAPI::CharacterDebugAPI(ILogic& logic, bool file, bool print, bool warnOnly, int32_t CharacterID) :
    logic(logic)
{
    std::string fileName = "logs/api-" + std::to_string(playerID) + "-log.txt";
    auto fileLogger = std::make_shared<spdlog::sinks::basic_file_sink_mt>(fileName, true);
    auto printLogger = std::make_shared<spdlog::sinks::stdout_color_sink_mt>();
    std::string pattern = "[api " + std::to_string(playerID) + "] [%H:%M:%S.%e] [%l] %v";
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

int32_t ChracterDebugAPI::GetFrameCount() const
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

std::futrue<bool> CharacterDebugAPI::Move(int64_t timeInMilliseconds, double angleInRadian)
{
    logger->info("Move: time = {}ms, angle = {}rad, called at {}ms", timeInMilliseconds, angleInRadian, Time::TimeSinceStart(startPoint));
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

std::future<bool> CharacterDebugAPI::Skill_Attack(double angleInRadian)
{
    logger->info("Skill_Attack: angle = {}rad, called at {}ms", angleInRadian, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.SkillAttack(angleInRadian);
                        if (!result)
                            logger->warn("Skill_Attack: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

std::future<bool> CharacterDebugAPI::Common_Attack(double angleInRadian)
{
    logger->info("Common_Attack: angle = {}rad, called at {}ms", angleInRadian, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.CommonAttack(angleInRadian);
                        if (!result)
                            logger->warn("Common_Attack: failed at {}ms", Time::TimeSinceStart(startPoint));
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
std::future<bool> CharacterDebugAPI::Harvest()
{
    logger->info("Harvest: called at {}ms", Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.Harvest();
                        if (!result)
                            logger->warn("Harvest: failed at {}ms", Time::TimeSinceStart(startPoint));
                        return result; });
}

std::future<bool> CharacterDebugAPI::Rebuild(THUAI8::ConstructionType constructionType)
{
    logger->info("Rebuild: constructionType = {}, called at {}ms", constructionType, Time::TimeSinceStart(startPoint));
    return std::async(std::launch::async, [=]()
                      { auto result = logic.Rebuild(constructionType);
                        if (!result)
                            logger->warn("Rebuild: failed at {}ms", Time::TimeSinceStart(startPoint));
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

std::optional<THUAI8::PlaceType> CharacterDebugAPI::GetPlaceType(int32_t cellX, int32_t cellY) const
{
    logger->info("GetPlaceType: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetPlaceType(cellX, cellY);
    if (!result)
        logger->warn("GetPlaceType: failed at {}ms", Time::TimeSinceStart(startPoint));
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

std::optional<THUAI8::AdditionResourceState> CharacterDebugAPI::GetAdditionResourceState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetAdditionResourceState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetAdditionResourceState(cellX, cellY);
    if (!result)
        logger->warn("GetAdditionResourceState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::optional<THUAI8::ConstructionState> CharacterDebugAPI::GetConstructionState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetConstructionState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetConstructionState(cellX, cellY);
    if (!result)
        logger->warn("GetConstructionState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

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
    auto result = logic.GetSelfInfo();
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

std::CharacterDebugAPI::Print(std::string str) const
{
    logger->info(str);
}

// facing direction存疑
std::CharacterDebugAPI::PrintCharacter() const
{
    for (const auto& Character : logic.GetCharacters())
    {
        logger->info("******Character Info******");
        logger->info("type={}, characterID={}, GUID={}, x={}, y={}", THUAI8::characterTypeDict[Character->characterType], Character->characterID, Character->guid, Character->x, Character->y);
        logger->info("state={},speed={}, view range={},facing direction={}", THUAI8::characterStateDict[Character->characterState], Character->speed, Character->viewRange, Character->facingDirection);
        logger->info("************************\n");
    }
}

std::CharacterDebugAPI::PrintSelfInfo() const
{
    auto self = logic.CharacterGetSelfInfo();
    logger->info("******Self Info******");
    logger->info("type={}, characterID={}, GUID={}, x={}, y={}", THUAI8::characterTypeDict[self->characterType], self->characterID, self->guid, self->x, self->y);
    logger->info("state={},speed={}, view range={},facing direction={}", THUAI8::characterStateDict[self->characterState], self->speed, self->viewRange, self->facingDirection);
    logger->info("************************\n");
}

std::future<bool> CharacterDebugAPI::EndAllAction()
{
    return std::async(std::launch::async, [this]()
                      { return logic.EndAllAction(); });
}

TeamDebugAPI::TeamDebugAPI(ILogic& logic, bool file, bool print, bool warnOnly, int32_t playerID) :
    logic(logic)
{
    std::string fileName = "logs/api-" + std::to_string(playerID) + "-log.txt";
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

std::optional<THUAI8::PlaceType> TeamDebugAPI::GetPlaceType(int32_t cellX, int32_t cellY) const
{
    logger->info("GetPlaceType: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetPlaceType(cellX, cellY);
    if (!result)
        logger->warn("GetPlaceType: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::optional<THUAI8::EconomyResourceState> TeamDebugAPI::GetEnconomyResourceState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetEnconomyResourceState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetEnconomyResourceState(cellX, cellY);
    if (!result)
        logger->warn("GetEnconomyResourceState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::optional<THUAI8::AdditionResourceState> TeamDebugAPI::GetAdditionResourceState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetAdditionResourceState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetAdditionResourceState(cellX, cellY);
    if (!result)
        logger->warn("GetAdditionResourceState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

std::optional<THUAI8::ConstructionState> TeamDebugAPI::GetConstructionState(int32_t cellX, int32_t cellY) const
{
    logger->info("GetConstructionState: cellX = {}, cellY = {}, called at {}ms", cellX, cellY, Time::TimeSinceStart(startPoint));
    auto result = logic.GetConstructionState(cellX, cellY);
    if (!result)
        logger->warn("GetConstructionState: failed at {}ms", Time::TimeSinceStart(startPoint));
    return result;
}

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

std::shared_ptr<const THUAI8::Character> TeamDebugAPI::GetSelfInfo() const
{
    logger->info("GetSelfInfo: called at {}ms", Time::TimeSinceStart(startPoint));
    auto result = logic.GetSelfInfo();
    if (result == nullptr)
        logger->warn("GetSelfInfo: failed at {}ms", Time::TimeSinceStart(startPoint));
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

void ShipDebugAPI::Play(IAI& ai)
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