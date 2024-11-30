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
    THUAI8::CharacterType::Monk,
    THUAI8::CharacterType::MonkeyKing,
    THUAI8::CharacterType::Pigsy,
    THUAI8::CharacterType::ShaWujing,
    THUAI8::CharacterType::Whitedragonhorse,
    THUAI8::CharacterType::JiuTouYuanSheng,
    THUAI8::CharacterType::Honghaier,
    THUAI8::CharacterType::Gyuumao,
    THUAI8::CharacterType::Princess_Iron_Fan,
    THUAI8::CharacterType::Spider,
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
    else if (this->playerID == 5)
    {
        // player5的操作
    }
}

void AI::play(ITeamAPI& api)  // 默认team playerID 为0
{
    // player0的操作
}
