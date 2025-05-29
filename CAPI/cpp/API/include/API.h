#pragma once
#ifndef API_H
#define API_H

#ifdef _MSC_VER
#pragma warning(disable : 4996)
#endif

#include "Message2Server.pb.h"
#include "Message2Clients.pb.h"
#include "MessageType.pb.h"
#include "Services.grpc.pb.h"
#include "Services.pb.h"
#include <future>
#include <iostream>
#include <vector>
#include <optional>

#include <spdlog/spdlog.h>
#include <spdlog/sinks/basic_file_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>

#include "structures.h"

#undef GetMessage
#undef SendMessage
#undef PeekMessage

const constexpr int32_t numOfGridPerCell = 1000;

class IAI;

class ILogic
{
    // API中依赖Logic的部分

public:
    // 获取服务器发来的消息
    [[nodiscard]] virtual std::vector<std::shared_ptr<const THUAI8::Character>> GetCharacters() const = 0;
    [[nodiscard]] virtual std::vector<std::shared_ptr<const THUAI8::Character>> GetEnemyCharacters() const = 0;
    [[nodiscard]] virtual std::shared_ptr<const THUAI8::Character> CharacterGetSelfInfo() const = 0;
    [[nodiscard]] virtual std::shared_ptr<const THUAI8::Team> TeamGetSelfInfo() const = 0;
    [[nodiscard]] virtual std::vector<std::vector<THUAI8::PlaceType>> GetFullMap() const = 0;
    [[nodiscard]] virtual std::shared_ptr<const THUAI8::GameInfo> GetGameInfo() const = 0;
    [[nodiscard]] virtual std::vector<int64_t> GetPlayerGUIDs() const = 0;
    [[nodiscard]] virtual THUAI8::PlaceType GetPlaceType(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::EconomyResource> GetEconomyResourceState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::AdditionResource> GetAdditionResourceState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::ConstructionState> GetConstructionState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::Trap> GetTrapState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual int32_t GetEnergy() const = 0;
    [[nodiscard]] virtual int32_t GetScore() const = 0;

    // 供IAPI使用的操作相关的公共部分
    virtual bool Send(int32_t toPlayerID, std::string message, bool binary) = 0;
    virtual bool HaveMessage() = 0;
    virtual std::pair<int32_t, std::string> GetMessage() = 0;
    virtual bool WaitThread() = 0;
    virtual int32_t GetCounter() const = 0;
    virtual bool EndAllAction() = 0;

    // ICharacterAPI使用的部分
    virtual bool Move(int64_t moveTimeInMilliseconds, double angle) = 0;
    virtual bool Recover(int64_t recover) = 0;
    virtual bool Produce(int64_t playerID, int64_t teamID) = 0;
    // virtual bool Rebuild(THUAI8::ConstructionType constructionType) = 0;
    virtual bool Construct(THUAI8::ConstructionType constructionType) = 0;
    virtual bool ConstructTrap(THUAI8::TrapType trapType) = 0;
    virtual bool Skill_Attack(int64_t teamID, int64_t playerID, double angle) = 0;
    virtual bool Common_Attack(int64_t teamID, int64_t playerID, int64_t attacked_teamID, int64_t attacked_playerID) = 0;
    virtual bool AttackConstruction(int64_t playerID, int64_t teamID) = 0;
    virtual bool AttackAdditionResource(int64_t playerID, int64_t teamID) = 0;
    [[nodiscard]] virtual bool HaveView(int32_t x, int32_t y, int32_t newX, int32_t newY, int32_t viewRange, std::vector<std::vector<THUAI8::PlaceType>>& map) const = 0;

    // Team使用的部分
    // virtual bool Recycle(int32_t playerID, int32_t targetID) = 0;
    virtual bool InstallEquipment(int32_t playerID, THUAI8::EquipmentType equipmentType) = 0;
    virtual bool BuildCharacter(THUAI8::CharacterType CharacterType, int32_t birthIndex) = 0;
};

class IAPI
{
public:
    // 选手可执行的操作，应当保证所有函数的返回值都应当为std::future，例如下面的移动函数：
    // 指挥本角色进行移动，`timeInMilliseconds` 为移动时间，单位为毫秒；`angleInRadian` 表示移动的方向，单位是弧度，使用极坐标——竖直向下方向为 x 轴，水平向右方向为 y 轴
    // 发送信息、接受信息，注意收消息时无消息则返回(-1,"")
    virtual std::future<bool> SendTextMessage(int32_t toPlayerID, std::string) = 0;
    virtual std::future<bool> SendBinaryMessage(int32_t toPlayerID, std::string) = 0;
    [[nodiscard]] virtual bool HaveMessage() = 0;
    [[nodiscard]] virtual std::pair<int32_t, std::string> GetMessage() = 0;

    // 获取游戏目前所进行的帧数
    [[nodiscard]] virtual int32_t GetFrameCount() const = 0;
    // 等待下一帧
    virtual bool Wait() = 0;
    virtual std::future<bool> EndAllAction() = 0;
    [[nodiscard]] virtual std::vector<std::shared_ptr<const THUAI8::Character>> GetCharacters() const = 0;
    [[nodiscard]] virtual std::vector<std::shared_ptr<const THUAI8::Character>> GetEnemyCharacters() const = 0;
    [[nodiscard]] virtual std::vector<std::vector<THUAI8::PlaceType>> GetFullMap() const = 0;
    [[nodiscard]] virtual std::shared_ptr<const THUAI8::GameInfo> GetGameInfo() const = 0;
    [[nodiscard]] virtual THUAI8::PlaceType GetPlaceType(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::EconomyResource> GetEconomyResourceState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::AdditionResource> GetAdditionResourceState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::ConstructionState> GetConstructionState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::Trap> GetTrapState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::vector<int64_t> GetPlayerGUIDs() const = 0;
    [[nodiscard]] virtual int32_t GetEnergy() const = 0;
    [[nodiscard]] virtual int32_t GetScore() const = 0;

    /*****选手可能用的辅助函数*****/

    // 获取指定格子中心的坐标
    [[nodiscard]] static inline int32_t CellToGrid(int32_t cell) noexcept
    {
        return cell * numOfGridPerCell + numOfGridPerCell / 2;
    }

    // 获取指定坐标点所位于的格子的 X 序号
    [[nodiscard]] static inline int32_t GridToCell(int32_t grid) noexcept
    {
        return grid / numOfGridPerCell;
    }

    // 用于DEBUG的输出函数，选手仅在开启Debug模式的情况下可以使用

    virtual void Print(std::string str) const = 0;
    virtual void PrintCharacter() const = 0;
    //    virtual void PrintTeam() const = 0;
    virtual void PrintSelfInfo() const = 0;
};

class ICharacterAPI : public IAPI
{
public:
    virtual std::future<bool> Move(int64_t moveTimeInMilliseconds, double angle) = 0;
    // 向特定方向移动
    virtual std::future<bool> MoveRight(int64_t timeInMilliseconds) = 0;
    virtual std::future<bool> MoveUp(int64_t timeInMilliseconds) = 0;
    virtual std::future<bool> MoveLeft(int64_t timeInMilliseconds) = 0;
    virtual std::future<bool> MoveDown(int64_t timeInMilliseconds) = 0;
    virtual std::future<bool> Skill_Attack(double angle) = 0;
    virtual std::future<bool> Common_Attack(int64_t attackedPlayerID) = 0;
    virtual std::future<bool> AttackConstruction() = 0;
    virtual std::future<bool> AttackAdditionResource() = 0;
    virtual std::future<bool> Recover(int64_t recover) = 0;
    virtual std::future<bool> Produce() = 0;
    // virtual std::future<bool> Rebuild(THUAI8::ConstructionType constructionType) = 0;
    virtual std::future<bool> Construct(THUAI8::ConstructionType constructionType) = 0;
    virtual std::future<bool> ConstructTrap(THUAI8::TrapType trapType) = 0;
    virtual std::shared_ptr<const THUAI8::Character> GetSelfInfo() const = 0;
    virtual bool HaveView(int32_t x, int32_t y, int32_t newX, int32_t newY, int32_t viewRange, std::vector<std::vector<THUAI8::PlaceType>>& map) const = 0;
};

class ITeamAPI : public IAPI
{
public:
    [[nodiscard]] virtual std::shared_ptr<const THUAI8::Team> GetSelfInfo() const = 0;
    virtual std::future<bool> InstallEquipment(int32_t playerID, THUAI8::EquipmentType equipmenttype) = 0;
    // virtual std::future<bool> Recycle(int32_t playerID, int32_t targetID) = 0;
    virtual std::future<bool> BuildCharacter(THUAI8::CharacterType CharacterType, int32_t birthIndex) = 0;
};

class IGameTimer
{
public:
    virtual ~IGameTimer() = default;
    virtual void StartTimer() = 0;
    virtual void EndTimer() = 0;
    virtual void Play(IAI& ai) = 0;
};

class CharacterAPI : public ICharacterAPI, public IGameTimer
{
public:
    CharacterAPI(ILogic& logic) :
        logic(logic)
    {
    }
    void StartTimer() override
    {
    }
    void EndTimer() override
    {
    }
    void Play(IAI& ai) override;

    std::future<bool> SendTextMessage(int32_t, std::string) override;
    std::future<bool> SendBinaryMessage(int32_t, std::string) override;
    [[nodiscard]] bool HaveMessage() override;
    [[nodiscard]] std::pair<int32_t, std::string> GetMessage() override;

    [[nodiscard]] int32_t GetFrameCount() const override;
    bool Wait() override;
    std::future<bool> EndAllAction() override;

    std::future<bool> Move(int64_t moveTimeInMilliseconds, double angle) override;
    std::future<bool> MoveRight(int64_t timeInMilliseconds) override;
    std::future<bool> MoveUp(int64_t timeInMilliseconds) override;
    std::future<bool> MoveLeft(int64_t timeInMilliseconds) override;
    std::future<bool> MoveDown(int64_t timeInMilliseconds) override;
    std::future<bool> Skill_Attack(double angle) override;
    std::future<bool> Common_Attack(int64_t attackedPlayerID) override;
    std::future<bool> AttackConstruction() override;
    std::future<bool> AttackAdditionResource() override;
    std::future<bool> Recover(int64_t recover) override;
    std::future<bool> Produce() override;
    // std::future<bool> Rebuild(THUAI8::ConstructionType constructionType) override;
    std::future<bool> Construct(THUAI8::ConstructionType constructionType) override;
    std::future<bool> ConstructTrap(THUAI8::TrapType trapType) override;
    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetCharacters() const override;
    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetEnemyCharacters() const override;
    [[nodiscard]] std::vector<std::vector<THUAI8::PlaceType>> GetFullMap() const override;
    [[nodiscard]] std::shared_ptr<const THUAI8::GameInfo> GetGameInfo() const override;
    [[nodiscard]] THUAI8::PlaceType GetPlaceType(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::EconomyResource> GetEconomyResourceState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::AdditionResource> GetAdditionResourceState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::ConstructionState> GetConstructionState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::Trap> GetTrapState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::vector<int64_t> GetPlayerGUIDs() const override;
    [[nodiscard]] int32_t GetEnergy() const override;
    [[nodiscard]] int32_t GetScore() const override;
    [[nodiscard]] std::shared_ptr<const THUAI8::Character> GetSelfInfo() const override;
    [[nodiscard]] bool HaveView(int32_t x, int32_t y, int32_t newX, int32_t newY, int32_t viewRange, std::vector<std::vector<THUAI8::PlaceType>>& map) const override;
    void Print(std::string str) const
    {
    }
    void PrintCharacter() const
    {
    }
    void PrintTeam() const
    {
    }
    void PrintSelfInfo() const
    {
    }

private:
    ILogic& logic;
};

class TeamAPI : public ITeamAPI, public IGameTimer
{
public:
    TeamAPI(ILogic& logic) :
        logic(logic)
    {
    }
    void StartTimer() override
    {
    }
    void EndTimer() override
    {
    }
    void Play(IAI& ai) override;

    std::future<bool> SendTextMessage(int32_t, std::string) override;
    std::future<bool> SendBinaryMessage(int32_t, std::string) override;
    [[nodiscard]] bool HaveMessage() override;
    [[nodiscard]] std::pair<int32_t, std::string> GetMessage() override;

    [[nodiscard]] int32_t GetFrameCount() const override;
    bool Wait() override;
    std::future<bool> EndAllAction() override;

    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetCharacters() const override;
    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetEnemyCharacters() const override;
    [[nodiscard]] std::vector<std::vector<THUAI8::PlaceType>> GetFullMap() const override;
    [[nodiscard]] std::shared_ptr<const THUAI8::GameInfo> GetGameInfo() const override;
    [[nodiscard]] THUAI8::PlaceType GetPlaceType(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::EconomyResource> GetEconomyResourceState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::AdditionResource> GetAdditionResourceState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::ConstructionState> GetConstructionState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::Trap> GetTrapState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::vector<int64_t> GetPlayerGUIDs() const override;
    [[nodiscard]] int32_t GetEnergy() const override;
    [[nodiscard]] int32_t GetScore() const override;
    [[nodiscard]] std::shared_ptr<const THUAI8::Team> GetSelfInfo() const override;
    std::future<bool> InstallEquipment(int32_t playerID, THUAI8::EquipmentType equipmenttype) override;
    // std::future<bool> Recycle(int32_t playerID, int32_t targetID) override;
    std::future<bool> BuildCharacter(THUAI8::CharacterType CharacterType, int32_t birthIndex) override;
    void Print(std::string str) const
    {
    }
    void PrintCharacter() const
    {
    }
    void PrintTeam() const
    {
    }
    void PrintSelfInfo() const
    {
    }

private:
    ILogic& logic;
};

class CharacterDebugAPI : public ICharacterAPI, public IGameTimer
{
public:
    CharacterDebugAPI(ILogic& logic, bool file, bool print, bool warnOnly, int32_t CharacterID, int32_t TeamID);
    void StartTimer() override;
    void EndTimer() override;
    void Play(IAI& ai) override;
    std::future<bool> SendTextMessage(int32_t, std::string) override;
    std::future<bool> SendBinaryMessage(int32_t, std::string) override;
    [[nodiscard]] bool HaveMessage() override;
    [[nodiscard]] std::pair<int32_t, std::string> GetMessage() override;
    bool Wait() override;
    [[nodiscard]] int32_t GetFrameCount() const override;
    std::future<bool> EndAllAction() override;

    std::future<bool> Move(int64_t moveTimeInMilliseconds, double angle) override;
    std::future<bool> MoveRight(int64_t timeInMilliseconds) override;
    std::future<bool> MoveUp(int64_t timeInMilliseconds) override;
    std::future<bool> MoveLeft(int64_t timeInMilliseconds) override;
    std::future<bool> MoveDown(int64_t timeInMilliseconds) override;
    std::future<bool> Skill_Attack(double angle) override;
    std::future<bool> Common_Attack(int64_t attackedPlayerID) override;
    std::future<bool> AttackConstruction() override;
    std::future<bool> AttackAdditionResource() override;
    std::future<bool> Recover(int64_t recover) override;
    std::future<bool> Produce();
    // std::future<bool> Rebuild(THUAI8::ConstructionType constructionType) override;
    std::future<bool> Construct(THUAI8::ConstructionType constructionType) override;
    std::future<bool> ConstructTrap(THUAI8::TrapType trapType) override;

    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetCharacters() const override;
    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetEnemyCharacters() const override;
    [[nodiscard]] std::vector<std::vector<THUAI8::PlaceType>> GetFullMap() const override;
    [[nodiscard]] std::shared_ptr<const THUAI8::GameInfo> GetGameInfo() const override;
    [[nodiscard]] THUAI8::PlaceType GetPlaceType(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::EconomyResource> GetEconomyResourceState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::AdditionResource> GetAdditionResourceState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::ConstructionState> GetConstructionState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::Trap> GetTrapState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::vector<int64_t> GetPlayerGUIDs() const override;
    [[nodiscard]] int32_t GetEnergy() const override;
    [[nodiscard]] int32_t GetScore() const override;
    [[nodiscard]] std::shared_ptr<const THUAI8::Character> GetSelfInfo() const override;
    [[nodiscard]] bool HaveView(int32_t x, int32_t y, int32_t newX, int32_t newY, int32_t viewRange, std::vector<std::vector<THUAI8::PlaceType>>& map) const override;

    void Print(std::string str) const override;
    void PrintCharacter() const override;
    void PrintSelfInfo() const override;
    // void PrintTeam() const override;
    //{
    // }

private:
    std::chrono::system_clock::time_point startPoint;
    std::unique_ptr<spdlog::logger> logger;
    ILogic& logic;
};

class TeamDebugAPI : public ITeamAPI, public IGameTimer
{
public:
    TeamDebugAPI(ILogic& logic, bool file, bool print, bool warnOnly, int32_t PlayerID, int32_t TeamID);
    void StartTimer() override;
    void EndTimer() override;
    void Play(IAI& ai) override;

    std::future<bool> SendTextMessage(int32_t, std::string) override;
    std::future<bool> SendBinaryMessage(int32_t, std::string) override;
    [[nodiscard]] bool HaveMessage() override;
    [[nodiscard]] std::pair<int32_t, std::string> GetMessage() override;

    [[nodiscard]] int32_t GetFrameCount() const override;
    bool Wait() override;
    std::future<bool> EndAllAction() override;

    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetCharacters() const override;
    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetEnemyCharacters() const override;
    [[nodiscard]] std::vector<std::vector<THUAI8::PlaceType>> GetFullMap() const override;
    [[nodiscard]] std::shared_ptr<const THUAI8::GameInfo> GetGameInfo() const override;
    [[nodiscard]] THUAI8::PlaceType GetPlaceType(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::EconomyResource> GetEconomyResourceState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::AdditionResource> GetAdditionResourceState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::ConstructionState> GetConstructionState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::optional<THUAI8::Trap> GetTrapState(int32_t cellX, int32_t cellY) const override;
    [[nodiscard]] std::vector<int64_t> GetPlayerGUIDs() const override;
    [[nodiscard]] int32_t GetEnergy() const override;
    [[nodiscard]] int32_t GetScore() const override;
    [[nodiscard]] std::shared_ptr<const THUAI8::Team> GetSelfInfo() const override;
    std::future<bool> InstallEquipment(int32_t playerID, THUAI8::EquipmentType equipmenttype) override;
    // std::future<bool> Recycle(int32_t playerID, int32_t targetID) override;
    std::future<bool> BuildCharacter(THUAI8::CharacterType CharacterType, int32_t birthIndex) override;
    void Print(std::string str) const override;
    void PrintSelfInfo() const override;
    // TODO
    void PrintTeam() const
    {
    }
    void PrintCharacter() const
    {
    }

private:
    std::chrono::system_clock::time_point startPoint;
    std::unique_ptr<spdlog::logger> logger;
    ILogic& logic;
};

#endif