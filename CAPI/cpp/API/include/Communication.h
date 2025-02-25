#ifndef COMMUNICATION_H
#define COMMUNICATION_H

#include "Message2Server.pb.h"
#include "Message2Clients.pb.h"
#include "MessageType.pb.h"
#include "Services.grpc.pb.h"
#include "Services.pb.h"
#include <grpcpp/grpcpp.h>
#include "structures.h"
#include <thread>
#include <mutex>
#include <condition_variable>
#include <queue>
#include <atomic>

#undef GetMessage
#undef SendMessage
#undef PeekMessage

class Logic;

class Communication
{
public:
    Communication(std::string sIP, std::string sPort);
    ~Communication() = default;
    bool TryConnection(int32_t playerID, int32_t teamID);
    protobuf::MessageToClient GetMessage2Client();
    void AddPlayer(int32_t playerID, int32_t teamID, THUAI8::CharacterType CharacterType);
    bool EndAllAction(int32_t playerID, int32_t teamID);
    // Character
    bool Move(int32_t speed, int32_t playerID, int32_t teamID, int64_t time, double angle);
    bool Recover(int32_t playerID, int64_t recover, int32_t teamID);
    bool Produce(int32_t playerID, int32_t teamID);
    bool Rebuild(int32_t playerID, int32_t teamID, THUAI8::ConstructionType constructionType);
    bool Construct(int32_t playerID, int32_t teamID, THUAI8::ConstructionType constructionType);
    bool Skill_Attack(int32_t playerID, int32_t teamID, double angle);
    bool Common_Attack(int32_t playerID, int32_t teamID, double angle);
    bool Send(int32_t playerID, int32_t toPlayerID, int32_t teamID, std::string message, bool binary);
    // Team
    bool InstallEquipment(int32_t playerID, int32_t teamID, THUAI8::EquipmentType equipmentType);
    bool BuildCharacter(int32_t teamID, THUAI8::CharacterType CharacterType, int32_t birthIndex);
    bool Recycle(int32_t playerID, int32_t teamID);

private:
    std::unique_ptr<protobuf::AvailableService::Stub> THUAI8Stub;
    bool haveNewMessage = false;
    protobuf::MessageToClient message2Client;
    std::mutex mtxMessage;
    std::mutex mtxLimit;
    int32_t counter{};
    int32_t counterMove{};
    static constexpr const int32_t limit = 50;
    static constexpr const int32_t moveLimit = 10;
    std::condition_variable cvMessage;
};

#endif