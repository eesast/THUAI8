#include "Communication.h"
#include "utils.hpp"
#include "structures.h"
#include <thread>
#include <mutex>
#include <condition_variable>
#include <memory>

#undef GetMessage
#undef SendMessage
#undef PeekMessage

using grpc::ClientContext;

Communication::Communication(std::string sIP, std::string sPort)
{
    std::string aim = sIP + ':' + sPort;
    auto channel = grpc::CreateChannel(aim, grpc::InsecureChannelCredentials());
    THUAI8Stub = protobuf::AvailableService::NewStub(channel);
}

bool Communication::Move(int64_t teamID, int64_t characterID, int32_t moveTimeInMilliseconds, double angle)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit || counterMove >= moveLimit)
            return false;
        counter++;
        counterMove++;
    }

    protobuf::MoveRes moveResult;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufMoveMsg(teamID, characterID, moveTimeInMilliseconds, angle);
    auto status = THUAI8Stub->Move(&context, request, &moveResult);
    if (status.ok())
    {
        return moveResult.act_success();
    }
    else
    {
        return false;
    }
}

bool Communication::Send(int32_t playerID, int32_t toPlayerID, int32_t teamID, std::string message, bool binary)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit)
            return false;
        counter++;
    }
    protobuf::BoolRes sendMessageResult;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufSendMsg(playerID, toPlayerID, teamID, std::move(message), binary);
    auto status = THUAI8Stub->Send(&context, request, &sendMessageResult);
    if (status.ok())
        return sendMessageResult.act_success();

    else
        return false;
}

bool Communication::EndAllAction(int32_t playerID, int32_t teamID)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit || counterMove >= moveLimit)
            return false;
        counter++;
        counterMove++;
    }
    protobuf::BoolRes endAllActionsResult;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufIDMsg(playerID, teamID);
    auto status = THUAI8Stub->EndAllAction(&context, request, &endAllActionsResult);
    if (status.ok())
        return endAllActionsResult.act_success();
    else
        return false;
}

bool Communication::Recover(int32_t playerID, int64_t recover, int32_t teamID)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit || counterMove >= moveLimit)
            return false;
        counter++;
        counterMove++;
    }
    protobuf::BoolRes recoverResult;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufRecoverMsg(playerID, recover, teamID);
    auto status = THUAI8Stub->Recover(&context, request, &recoverResult);
    if (status.ok())
        return recoverResult.act_success();
    else
        return false;
}

bool Communication::Produce(int64_t playerID, int64_t teamID)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit || counterMove >= moveLimit)
            return false;
        counter++;
        counterMove++;
    }
    protobuf::BoolRes produceResult;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufIDMsg(playerID, teamID);
    auto status = THUAI8Stub->Produce(&context, request, &produceResult);
    if (status.ok())
        return produceResult.act_success();
    else
        return false;
}

bool Communication::Construct(int32_t playerID, int32_t teamID, THUAI8::ConstructionType constructionType)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit || counterMove >= moveLimit)
            return false;
        counter++;
        counterMove++;
    }
    protobuf::BoolRes constructResult;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufConstructMsg(playerID, teamID, constructionType);
    auto status = THUAI8Stub->Construct(&context, request, &constructResult);
    if (status.ok())
        return constructResult.act_success();
    else
        return false;
}

bool Communication::InstallEquipment(int32_t playerID, int32_t teamID, THUAI8::EquipmentType equipmentType)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit || counterMove >= moveLimit)
            return false;
        counter++;
        counterMove++;
    }
    protobuf::BoolRes installEquipmentResult;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufEquipMsg(playerID, teamID, equipmentType);
    auto status = THUAI8Stub->Equip(&context, request, &installEquipmentResult);
    if (status.ok())
        return installEquipmentResult.act_success();
    else
        return false;
}

/* bool Communication::Rebuild(int32_t playerID, int32_t teamID, THUAI8::ConstructionType constructionType)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit || counterMove >= moveLimit)
            return false;
        counter++;
        counterMove++;
    }
    protobuf::BoolRes rebuildResult;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufRebuildMsg(playerID, teamID, constructionType);
    auto status = THUAI8Stub->Rebuild(&context, request, &rebuildResult);
    if (status.ok())
        return rebuildResult.act_success();
    else
        return false;
}*/

// bool Communication::Common_Attack(int32_t playerID, int32_t teamID, double angle)
// {
//     {
//         std::lock_guard<std::mutex> lock(mtxLimit);
//         if (counter >= limit)
//             return false;
//         counter++;
//     }
//     protobuf::BoolRes commonAttackResult;
//     ClientContext context;
//     auto request = THUAI82Proto::THUAI82ProtobufCommonAttackMsg(playerID, teamID, angle);
//     auto status = THUAI8Stub->CommonAttack(&context, request, &commonAttackResult);
//     if (status.ok())
//         return commonAttackResult.act_success();
//     else
//         return false;
// }
// 普攻必中，angle改toplayerID

bool Communication::Common_Attack(int64_t playerID, int64_t teamID, int64_t attacked_playerID, int64_t attacked_teamID)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit)
            return false;
        counter++;
    }
    protobuf::BoolRes commonAttackResult;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufAttackMsg(playerID, teamID);
    auto status = THUAI8Stub->Attack(&context, request, &commonAttackResult);
    if (status.ok())
        return commonAttackResult.act_success();
    else
        return false;
}

bool Communication::BuildCharacter(int32_t teamID, THUAI8::CharacterType charactertype, int32_t birthIndex)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit)
            return false;
        counter++;
    }
    protobuf::BoolRes reply;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufCreatCharacterMsg(teamID, charactertype, birthIndex);
    auto status = THUAI8Stub->CreatCharacter(&context, request, &reply);
    if (status.ok())
        return reply.act_success();
    else
        return false;
}

/*/ bool Communication::Recycle(int32_t playerID, int32_t teamID)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit)
            return false;
        counter++;
    }
    protobuf::BoolRes reply;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufIDMsg(playerID, teamID);
    auto status = THUAI8Stub->(&context, request, &reply);
    if (status.ok())
        return reply.act_success();
    else
        return false;
}*/

// bool Communication::Skill_Attack(int32_t playerID, int32_t teamID, double angle)
// {
//     {
//         std::lock_guard<std::mutex> lock(mtxLimit);
//         if (counter >= limit)
//             return false;
//         counter++;
//     }
//     protobuf::BoolRes reply;
//     ClientContext context;
//     auto request = THUAI82Proto::THUAI82ProtobufSkillAttackMsg(playerID, teamID, angle);
//     auto status = THUAI8Stub->SkillAttack(&context, request, &reply);
//     if (status.ok())
//         return reply.act_success();
//     else
//         return false;
// }
// 技能必中，angle改toplayerID

// 待修改，不知道要不要toteamID
bool Communication::Skill_Attack(int64_t teamID, int64_t playerID, double angle)
{
    {
        std::lock_guard<std::mutex> lock(mtxLimit);
        if (counter >= limit)
            return false;
        counter++;
    }
    protobuf::BoolRes reply;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufCastMsg(playerID,teamID,angle);
    auto status = THUAI8Stub->Cast(&context, request, &reply);
    if (status.ok())
        return reply.act_success();
    else
        return false;
}

bool Communication::TryConnection(int32_t playerID, int32_t teamID)
{
    protobuf::BoolRes reply;
    ClientContext context;
    auto request = THUAI8Proto::THUAI82ProtobufIDMsg(playerID, teamID);
    auto status = THUAI8Stub->TryConnection(&context, request, &reply);
    if (status.ok())
        return true;
    else
        return false;
}

void Communication::AddPlayer(int32_t playerID, int32_t teamID, THUAI8::CharacterType charactertype)
{
    auto tMessage = [=]()
    {
        protobuf::CharacterMsg playerMsg = THUAI8Proto::THUAI82ProtobufCharacterMsg(playerID, teamID, charactertype);
        grpc::ClientContext context;
        auto MessageReader = THUAI8Stub->AddCharacter(&context, playerMsg);

        protobuf::MessageToClient buffer2Client;
        counter = 0;
        counterMove = 0;

        while (MessageReader->Read(&buffer2Client))
        {
            {
                std::lock_guard<std::mutex> lock(mtxMessage);
                message2Client = std::move(buffer2Client);
                haveNewMessage = true;
                {
                    std::lock_guard<std::mutex> lock(mtxLimit);
                    counter++;
                    counterMove = 0;
                }
            }
            cvMessage.notify_one();
        }
    };
    std::thread(tMessage).detach();
}

protobuf::MessageToClient Communication::GetMessage2Client()
{
    std::unique_lock<std::mutex> lock(mtxMessage);
    cvMessage.wait(lock, [this]()
                   { return haveNewMessage; });
    haveNewMessage = false;
    return message2Client;
}