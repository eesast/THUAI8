#include <memory>
#include "AI.h"
#include "API.h"

#undef GetMessage
#undef SendMessage
#undef PeekMessage

#define PI 3.14159265358979323846

std::future<bool> CharacterAPI::SendTextMessage(int32_t toID, std::string message)
{
    return std::async(std::launch::async, [=, message = std::move(message)]()
                      { return logic.Send(toID, std::move(message), false); });
}

std::future<bool> TeamAPI::SendTextMessage(int32_t toID, std::string message)
{
    return std::async(std::launch::async, [=, message = std::move(message)]()
                      { return logic.Send(toID, std::move(message), false); });
}

std::future<bool> CharacterAPI::SendBinaryMessage(int32_t toID, std::string message)
{
    return std::async(std::launch::async, [=, message = std::move(message)]()
                      { return logic.Send(toID, std::move(message), true); });
}

std::future<bool> TeamAPI::SendBinaryMessage(int32_t toID, std::string message)
{
    return std::async(std::launch::async, [=, message = std::move(message)]()
                      { return logic.Send(toID, std::move(message), true); });
}

bool CharacterAPI::HaveMessage()
{
    return logic.HaveMessage();
}

bool TeamAPI::HaveMessage()
{
    return logic.HaveMessage();
}

std::pair<int32_t, std::string> CharacterAPI::GetMessage()
{
    return logic.GetMessage();
}

std::pair<int32_t, std::string> TeamAPI::GetMessage()
{
    return logic.GetMessage();
}

int32_t CharacterAPI::GetFrameCount() const
{
    return logic.GetCounter();
}

int32_t TeamAPI::GetFrameCount() const
{
    return logic.GetCounter();
}

bool CharacterAPI::Wait()
{
    if (logic.GetCounter() == -1)
        return false;
    else
        return logic.WaitThread();
}

bool TeamAPI::Wait()
{
    if (logic.GetCounter() == -1)
        return false;
    else
        return logic.WaitThread();
}

std::future<bool> CharacterAPI::EndAllAction()
{
    return std::async(std::launch::async, [this]()
                      { return logic.EndAllAction(); });
}

std::future<bool> TeamAPI::EndAllAction()
{
    return std::async(std::launch::async, [this]()
                      { return logic.EndAllAction(); });
}

std::vector<std::shared_ptr<const THUAI8::Character>> CharacterAPI::GetCharacters() const
{
    return logic.GetCharacters();
}

std::vector<std::shared_ptr<const THUAI8::Character>> TeamAPI::GetCharacters() const
{
    return logic.GetCharacters();
}

std::vector<std::shared_ptr<const THUAI8::Character>> CharacterAPI::GetEnemyCharacters() const
{
    return logic.GetEnemyCharacters();
}

std::vector<std::shared_ptr<const THUAI8::Character>> TeamAPI::GetEnemyCharacters() const
{
    return logic.GetEnemyCharacters();
}

std::vector<std::vector<THUAI8::PlaceType>> CharacterAPI::GetFullMap() const
{
    return logic.GetFullMap();
}

std::vector<std::vector<THUAI8::PlaceType>> TeamAPI::GetFullMap() const
{
    return logic.GetFullMap();
}

THUAI8::PlaceType CharacterDebugAPI::GetPlaceType(int32_t cellX, int32_t cellY) const
{
    return logic.GetPlaceType(cellX, cellY);
}

THUAI8::PlaceType TeamAPI::GetPlaceType(int32_t cellX, int32_t cellY) const
{
    return logic.GetPlaceType(cellX, cellY);
}

std::optional<THUAI8::ConstructionState> CharacterAPI::GetConstructionState(int32_t cellX, int32_t cellY) const
{
    return logic.GetConstructionState(cellX, cellY);
}

std::optional<THUAI8::ConstructionState> TeamAPI::GetConstructionState(int32_t cellX, int32_t cellY) const
{
    return logic.GetConstructionState(cellX, cellY);
}

std::optional<THUAI8::EconomyResource> CharacterAPI::GetEconomyResourceState(int32_t cellX, int32_t cellY) const
{
    return logic.GetEconomyResourceState(cellX, cellY);
}

std::optional<THUAI8::EconomyResource> TeamAPI::GetEconomyResourceState(int32_t cellX, int32_t cellY) const
{
    return logic.GetEconomyResourceState(cellX, cellY);
}

std::optional<THUAI8::AdditionResource> CharacterAPI::GetAdditionResourceState(int32_t cellX, int32_t cellY) const
{
    return logic.GetAdditionResourceState(cellX, cellY);
}

std::optional<THUAI8::AdditionResource> TeamAPI::GetAdditionResourceState(int32_t cellX, int32_t cellY) const
{
    return logic.GetAdditionResourceState(cellX, cellY);
}

std::optional<THUAI8::Trap> CharacterAPI::GetTrapState(int32_t cellX, int32_t cellY) const
{
    return logic.GetTrapState(cellX, cellY);
}

std::optional<THUAI8::Trap> TeamAPI::GetTrapState(int32_t cellX, int32_t cellY) const
{
    return logic.GetTrapState(cellX, cellY);
}

std::shared_ptr<const THUAI8::GameInfo> CharacterAPI::GetGameInfo() const
{
    return logic.GetGameInfo();
}

std::shared_ptr<const THUAI8::GameInfo> TeamAPI::GetGameInfo() const
{
    return logic.GetGameInfo();
}

std::vector<int64_t> CharacterAPI::GetPlayerGUIDs() const
{
    return logic.GetPlayerGUIDs();
}

std::vector<int64_t> TeamAPI::GetPlayerGUIDs() const
{
    return logic.GetPlayerGUIDs();
}

std::shared_ptr<const THUAI8::Character> CharacterAPI::GetSelfInfo() const
{
    return logic.CharacterGetSelfInfo();
}

std::shared_ptr<const THUAI8::Team> TeamAPI::GetSelfInfo() const
{
    return logic.TeamGetSelfInfo();
}

int32_t CharacterAPI::GetScore() const
{
    return logic.GetScore();
}

int32_t TeamAPI::GetScore() const
{
    return logic.GetScore();
}
THUAI8::PlaceType CharacterAPI::GetPlaceType(int32_t cellX, int32_t cellY) const
{
    return logic.GetPlaceType(cellX, cellY);
}

int32_t CharacterAPI::GetEnergy() const
{
    return logic.GetEnergy();
}

int32_t TeamAPI::GetEnergy() const
{
    return logic.GetEnergy();
}


std::future<bool> CharacterAPI::Move(int64_t moveTimeInMilliseconds, double angle)
{
    return std::async(std::launch::async, [=]()
                      { return logic.Move(moveTimeInMilliseconds, angle); });
}


std::future<bool> CharacterAPI::MoveDown(int64_t timeInMilliseconds)
{
    return Move(timeInMilliseconds, 0);
}

std::future<bool> CharacterAPI::MoveRight(int64_t timeInMilliseconds)
{
    return Move(timeInMilliseconds, PI * 0.5);
}

std::future<bool> CharacterAPI::MoveUp(int64_t timeInMilliseconds)
{
    return Move(timeInMilliseconds, PI);
}

std::future<bool> CharacterAPI::MoveLeft(int64_t timeInMilliseconds)
{
    return Move(timeInMilliseconds, PI * 1.5);
}
std::future<bool> CharacterAPI::Common_Attack(int64_t attackedPlayerID)
{
    return std::async(std::launch::async, [=]()
                      { return logic.Common_Attack(this->GetSelfInfo()->playerID, this->GetSelfInfo()->teamID, 1 - this->GetSelfInfo()->teamID, attackedPlayerID); });
}

std::future<bool> CharacterAPI::Skill_Attack(double angle)
{
    return std::async(std::launch::async, [=]()
                      { return logic.Skill_Attack(this->GetSelfInfo()->playerID, this->GetSelfInfo()->teamID, angle); });
}

std::future <bool> CharacterAPI::AttackConstruction()
{
    return std::async(std::launch::async, [=]()
                      { return logic.AttackConstruction(this->GetSelfInfo()->playerID, this->GetSelfInfo()->teamID); });
}

std::future<bool> CharacterAPI::AttackAdditionResource()
{
    return std::async(std::launch::async, [=]()
                      { return logic.AttackAdditionResource(this->GetSelfInfo()->playerID, this->GetSelfInfo()->teamID); });
}
std::future<bool> CharacterAPI::Recover(int64_t recover)
{
    return std::async(std::launch::async, [=]()
                      { return logic.Recover(recover); });
}

std::future<bool> CharacterAPI::Produce()
{
    return std::async(std::launch::async, [=]()
                      { return logic.Produce(this->GetSelfInfo()->playerID, this->GetSelfInfo()->teamID); });
}

/* std::future<bool> CharacterAPI::Rebuild(THUAI8::ConstructionType constructionType)
{
    return std::async(std::launch::async, [=]()
                      { return logic.Rebuild(constructionType); });
}*/

std::future<bool> CharacterAPI::Construct(THUAI8::ConstructionType constructionType)
{
    return std::async(std::launch::async, [=]()
                      { return logic.Construct(constructionType); });
}

std::future<bool> CharacterAPI::ConstructTrap(THUAI8::TrapType trapType)
{
    return std::async(std::launch::async, [=]()
                      { return logic.ConstructTrap(trapType); });
}

bool CharacterAPI::HaveView(int32_t x, int32_t y, int32_t newX, int32_t newY, int32_t viewRange, std::vector<std::vector<THUAI8::PlaceType>>& map) const
{
    auto selfInfo = GetSelfInfo();
    return logic.HaveView(x, y, newX, newY, viewRange, map);
}

void CharacterAPI::Play(IAI& ai)
{
    ai.play(*this);
}

// Team独有
std::future<bool> TeamAPI::InstallEquipment(int32_t playerID, const THUAI8::EquipmentType equipmentType)
{
    return std::async(std::launch::async, [=]()
                      { return logic.InstallEquipment(playerID, equipmentType); });
}

/* std::future<bool> TeamAPI::Recycle(int32_t playerID, int32_t targetID)
{
    return std::async(std::launch::async, [=]()
                      { return logic.Recycle(playerID, targetID); });
}*/

std::future<bool> TeamAPI::BuildCharacter(THUAI8::CharacterType CharacterType, int32_t birthIndex)
{
    return std::async(std::launch::async, [=]()
                      { return logic.BuildCharacter(CharacterType, birthIndex); });
}

void TeamAPI::Play(IAI& ai)
{
    ai.play(*this);
}
