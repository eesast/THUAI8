#include <vector>
#include <thread>
#include <array>
#include <map>
#include "AI.h"
#include "constants.h"
// 注意不要使用conio.h，Windows.h等非标准库
// 为假则play()期间确保游戏状态不更新，为真则只保证游戏状态在调用相关方法时不更新，大致一帧更新一次
extern const bool asynchronous = false;

// 选手需要依次将player1到player4的角色类型在这里定义
extern const std::array<THUAI8::CharacterType, 12> CharacterTypeDict = {
    THUAI8::CharacterType::Camp1Character1,
    THUAI8::CharacterType::Camp1Character2,
    THUAI8::CharacterType::Camp1Character3,
    THUAI8::CharacterType::Camp1Character4,
    THUAI8::CharacterType::Camp1Character5,
    THUAI8::CharacterType::Camp1Character6,
    
    THUAI8::CharacterType::Camp2Character1,
    THUAI8::CharacterType::Camp2Character2,
    THUAI8::CharacterType::Camp2Character3,
    THUAI8::CharacterType::Camp2Character4,
    THUAI8::CharacterType::Camp2Character5,
    THUAI8::CharacterType::Camp2Character6,
};

// 可以在AI.cpp内部声明变量与函数

void AI::play(IShipAPI& api)
{
    if (this->playerID == 1)
    {
        // player1的操作
    }
    else if (this->playerID == 2)
    {
        // player2的操作
    }
    else if (this->playerID == 3)
    {
        // player3的操作
    }
    else if (this->playerID == 4)
    {
        // player4的操作
    }
}

void AI::play(ITeamAPI& api)  // 默认team playerID 为0
{
    // player0的操作
}
