#pragma once
#ifndef CONSTANTS_H
#define CONSTANTS_H

#ifndef SCCI
#define SCCI static const constexpr inline
#endif

#undef GetMessage
#undef SendMessage
#undef PeekMessage
namespace Constants
{
    SCCI int32_t frameDuration = 50;  // 每帧毫秒数
    // 地图相关
    SCCI int32_t numOfGridPerCell = 1000;  // 单位坐标数
    SCCI int32_t rows = 50;                // 地图行数
    SCCI int32_t cols = 50;                // 地图列数
                                           // SCCI double robPercent =
    SCCI int32_t DestroyBarracksBonus = 6000;
    SCCI int32_t DestroySpringBonus = 3000;
    SCCI int32_t DestroyFarmBonus = 4000;
    // SCCI double recoverMultiplier = 1.2;
    // SCCI double recycleMultiplier = 0.5;
    //角色
    SCCI int32_t sizeofCharacter = 800;
    SCCI int32_t Speed = 2500;
    struct Monk
    {
        SCCI int32_t maxHp = 1000;
    };
    struct MonkeyKing
    {
        SCCI int32_t maxHp = 200;
        SCCI int32_t common_attack_power = 30;
        SCCI int32_t attackRange = 1;
        SCCI int32_t Cost = 5000;
    };
    struct Pigsy
    {
        SCCI int32_t maxHp = 300;
        SCCI int32_t common_attack_power = 20;
        SCCI int32_t attackRange = 2;
        SCCI int32_t Cost = 4000;
    };
    struct ShaWujing
    {
        SCCI int32_t maxHp = 150;
        SCCI int32_t common_attack_power = 10;
        SCCI int32_t attackRange = 5;
        SCCI int32_t Cost = 3000;
    };
    struct Whitedragonhorse
    {
        SCCI int32_t maxHp = 150;
        SCCI int32_t common_attack_power = 10;
        SCCI int32_t attackRange = 5;
        SCCI int32_t Cost = 4000;
    };
    struct JiuTouYuanSheng
    {
        SCCI int32_t maxHp = 1000;
    };
    struct Honghaier
    {
        SCCI int32_t maxHp = 200;
        SCCI int32_t common_attack_power = 25;
        SCCI int32_t attackRange = 1;
        SCCI int32_t Cost = 5000;
    };
    //模块

}  // namespace Constants