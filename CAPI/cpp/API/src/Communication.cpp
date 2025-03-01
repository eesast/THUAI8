#include "Communication.h"
#include "utils.hpp"
#include "structures.h"
#include <thread>
#include <mutex>
#include <condition_variable>

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

bool Communication::Move(int32_t speed, int32_t playerID, int32_t teamID, int64_t time, double angle)
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
    auto request = THUAI82Proto::
}