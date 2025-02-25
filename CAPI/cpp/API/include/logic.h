#pragma once

#ifndef LOGIC_H
#define LOGIC_H

#ifdef _MSC_VER
#pragma warning(disable : 4996)
#endif

#include <iostream>
#include <vector>
#include <optional>
#include <thread>
#include <mutex>
#include <condition_variable>
#include <atomic>
#include <queue>

#include <spdlog/spdlog.h>
#include <spdlog/sinks/basic_file_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>

#include "Message2Server.pb.h"
#include "Message2Clients.pb.h"
#include "MessageType.pb.h"
#include "Services.grpc.pb.h"
#include "Services.pb.h"
#include "API.h"
#include "AI.h"
#include "structures.h"
#include "state.h"
#include "Communication.h"
#include "ConcurrentQueue.hpp"

#undef GetMessage
#undef SendMessage
#undef PeekMessage

class Logic : public ILogic
{
private:
    //日志组件
    std::unique_ptr<spdlog::logger> logger;

    //通信组件
    std::unique_ptr<Communication> pComm;

    // ID
    THUAI8::PlayerType playerType;
    int32_t playerID;
    int32_t teamID;
    THUAI8::PlayerTeam playerTeam;
    THUAI8::CharacterType CharacterType;
    std::unique_ptr<IGameTimer> timer;
    std::thread tAI;  // 用于运行AI的线程

    mutable std::mutex mtxAI;
    mutable std::mutex mtxState;
    mutable std::mutex mtxBuffer;

    std::condition_variable cvBuffer;
    std::condition_variable cvAI;

    //信息队列
    ConcurrentQueue<std::pair<int64_t, std::string>> messageQueue;

    //存储状态，分别是现在的状态和缓冲区的状态。
    State state[2];
    State* currentState;
    State* bufferState;

    //保存缓冲区数
    int32_t counterState = 0;
    int32_t counterBuffer = 0;

    TUHAI8::GameState gameState = THUAI8::GameState::NullGameState;

    //是否应该执行player()
    std::atomic_bool AILoop = true;

    // buffer是否更新完毕
    bool bufferUpdated = false;

    //是否应当启动AI
    bool AIStart = false;

    // asynchronous = true 时控制内容更新的变量
    std::atomic_bool freshed = false;

    //提供给API使用的函数

    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetCharacters() const;
    [[nodiscard]] std::vector<std::shared_ptr<const THUAI8::Character>> GetEnemyCharacters() const;
    [[nodiscard]] std::vector<std::vector<THUAI8::PlaceType>> GetFullMap() const;
    [[nodiscard]] std::shared_ptr<const THUAI8::GameInfo> GetGameInfo() const;
    [[nodiscard]] THUAI8::PlaceType GetPlaceType(int32_t cellX, int32_t cellY) const;
    [[nodiscard]] std::optional<THUAI8::EconomyResourceState> GetEnconomyResourceState(int32_t cellX, int32_t cellY) const;
    [[nodiscard]] std::optional<THUAI8::AdditionResourceState> GetAdditionResourceState(int32_t cellX, int32_t cellY) const;
    [[nodiscard]] std::optional<THUAI8::ConstructionState> GetConstructionState(int32_t cellX, int32_t cellY) const;
    [[nodiscard]] std::vector<int64_t> GetPlayerGUIDs() const;
    [[nodiscard]] int32_t GetEnergy() const;
    [[nodiscard]] int32_t GetScore() const;
    [[nodiscard]] std::shared_ptr<const THUAI8::Character> GetSelfInfo() const;

    //供IAPI是使用的操作相关的部分
    bool Send(int32_t toPlayerID, std::string message, bool binary);
    bool HaveMessage();
    std::pair<int32_t, std::string> GetMessage();
    bool WaitThread();
    int32_t GetCounter() const;
    bool EndAllAction();

    // ICharacterAPI使用的部分
    bool Move(int32_t speed, int64_t timeInMilliseconds, double angleInRadian) = 0;
    //向特定方向移动
    bool MoveRight(int32_t speed, int64_t timeInMilliseconds);
    bool MoveUp(int32_t speed, int64_t timeInMilliseconds);
    bool MoveLeft(int32_t speed, int64_t timeInMilliseconds);
    bool MoveDown(int32_t speed, int64_t timeInMilliseconds);
    bool Skill_Attack(double angleInRadian);
    bool Common_Attack(double angleInRadian);
    bool Recover(int64_t recover);
    bool Produce();
    bool Rebuild(THUAI8::ConstructionType constructionType);
    bool Construct(THUAI8::ConstructionType constructionType);
    [[nodiscard]] bool HaveView(int32_t targetX, int32_t targetY) const;

    // ITeamAPI
    bool InstallEquipment(int32_t playerID, THUAI8::EquipmentType equipmenttype);
    bool Recycle(int32_t playerID);
    bool BuildCharacter(THUAI8::CharacterType CharacterType, int32_t birthIndex);

    bool TryConnection();
    void ProcessMessage();

    //将信息加载到buffer
    void LoadBufferSelf(const protobuf::MessageToClient& message);
    void LoadBufferCase(const protobuf::MessageOfObj& item);
    void LoadBuffer(const protobuf::MessageToClient& message);

    //解锁AI线程
    void UnBlockAI();

    //更新状态
    void Update() noexcept;

    //等待
    void Wait() noexcept;

public:
    Logic(int32_t playerID, int32_t teamID, THUAI8::PlayerType playerType, THUAI8::CharacterType CharacterType);

    ~Logic()
    {
    }

    void Main(CreateAIFunc createAI, std::string IP, std::string port, bool file, bool print, bool warnOnly);
};

#endif
