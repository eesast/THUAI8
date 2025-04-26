#include <vector>
#include <thread>
#include <array>
#include <map>
#include <memory>
#include "AI.h"
#include "constants.h"
// 注意不要使用conio.h，Windows.h等非标准库
// 为假则play()期间确保游戏状态不更新，为真则只保证游戏状态在调用相关方法时不更新，大致一帧更新一次
extern const bool asynchronous = false;

// 选手需要依次将player1到player5的角色类型在这里定义
extern const std::array<THUAI8::CharacterType, 6> BuddhistsCharacterTypeDict = {
    THUAI8::CharacterType::TangSeng,
    THUAI8::CharacterType::SunWukong,
    THUAI8::CharacterType::ZhuBajie,
    THUAI8::CharacterType::ShaWujing,
    THUAI8::CharacterType::BaiLongma,
    THUAI8::CharacterType::Monkid,
};

extern const std::array<THUAI8::CharacterType, 6> MonstersCharacterTypeDict = {
    THUAI8::CharacterType::JiuLing,
    THUAI8::CharacterType::HongHaier,
    THUAI8::CharacterType::NiuMowang,
    THUAI8::CharacterType::TieShan,
    THUAI8::CharacterType::ZhiZhujing,
    THUAI8::CharacterType::Pawn,
};

// 可以在AI.cpp内部声明变量与函数

void AI::play(ICharacterAPI& api)
{
    if (this->playerID == 1)
    {
        // player1的操作
        api.PrintSelfInfo();
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
