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
    // 角色
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

    struct Gyuumao
    {
        SCCI int32_t maxHp = 300;
        SCCI int32_t common_attack_power = 20;
        SCCI int32_t attackRange = 2;
        SCCI int32_t Cost = 4000;
    };

    struct Princess_Iron_Fan
    {
        SCCI int32_t maxHp = 150;
        SCCI int32_t common_attack_power = 10;
        SCCI int32_t attackRange = 5;
        SCCI int32_t Cost = 3000;
    };

    struct Spider
    {
        SCCI int32_t maxHp = 150;
        SCCI int32_t common_attack_power = 10;
        SCCI int32_t attackRange = 5;
        SCCI int32_t Cost = 3000;
    };
    // 模块

    struct resumption  // 生命之泉
    {
        SCCI int32_t recovery1 = 50;
        SCCI int32_t recovery2 = 100;
        SCCI int32_t recovery3 = 150;
        SCCI int32_t score1 = 2000;  // 一级生命之泉分数
        SCCI int32_t score2 = 3000;  // 二级生命之泉分数
        SCCI int32_t score3 = 4000;  // 三级生命之泉分数
        SCCI int32_t maxHp1 = 200;   // 一级生命之泉守关怪物血量
        SCCI int32_t maxHp2 = 300;   // 二级生命之泉守关怪物血量
        SCCI int32_t maxHp3 = 400;   // 三级生命之泉守关怪物血量
        SCCI int32_t attack = 10;    // 生命之泉守关怪物攻击力
    };

    struct Attack_Boost  // 狂战士之力
    {
        SCCI int32_t attack_boost1 = 10;
        SCCI int32_t attack_boost2 = 15;
        SCCI int32_t attack_boost3 = 20;
        SCCI int32_t time1 = 30;     // 一级狂战士之力持续时间
        SCCI int32_t time2 = 45;     // 二级狂战士之力持续时间
        SCCI int32_t time3 = 60;     // 三级狂战士之力持续时间
        SCCI int32_t score1 = 4000;  // 一级狂战士之力分数
        SCCI int32_t score2 = 5000;  // 二级狂战士之力分数
        SCCI int32_t score3 = 6000;  // 三级狂战士之力分数
        SCCI int32_t maxHp1 = 400;   // 一级狂战士之力守关怪物血量
        SCCI int32_t maxHp2 = 500;   // 二级狂战士之力守关怪物血量
        SCCI int32_t maxHp3 = 600;   // 三级狂战士之力守关怪物血量
        SCCI int32_t attack1 = 10;   // 一级狂战士之力守关怪物攻击力
        SCCI int32_t attack2 = 15;   // 二级狂战士之力守关怪物攻击力
        SCCI int32_t attack3 = 20;   // 三级狂战士之力守关怪物攻击力
    };

    struct Speed_Boost
    {
        SCCI int32_t speed_boost = 500;
        SCCI int32_t time = 60;
        SCCI int32_t score = 3000;
        SCCI int32_t maxHp = 300;
        SCCI int32_t attack = 10;
    };

    struct View_Boost
    {
        SCCI int32_t time = 60;
        SCCI int32_t score = 3000;
        SCCI int32_t maxHp = 300;
        SCCI int32_t attack = 10;
    };

    struct Barracks
    {
        SCCI int32_t maxHp = 600;
        SCCI int32_t Cost = 10000;
        SCCI int32_t sabotage_score = 6000;
        SCCI int32_t time_cost = 15;
    };

    struct Spring
    {
        SCCI int32_t maxHp = 300;
        SCCI int32_t Cost = 8000;
        SCCI int32_t sabotage_score = 3000;
        SCCI int32_t time_cost = 10;
    };

    struct Farm
    {
        SCCI int32_t maxHp = 400;
        SCCI int32_t Cost = 8000;
        SCCI int32_t sabotage_score = 3000;
        SCCI int32_t time_cost = 10;
    };

    struct Hole
    {
        SCCI int32_t cost = 1000;
        SCCI int32_t time = 5;
        SCCI int32_t attack = 20;
        SCCI int32_t continous_time = 5;
    };

    struct Cage
    {
        SCCI int32_t cost = 1000;
        SCCI int32_t time = 5;
        SCCI int32_t continous_time = 30;
    };
    // 商店商品
    struct blood_vial
    {
        SCCI int32_t cost1 = 1500;
        SCCI int32_t cost2 = 3000;
        SCCI int32_t cost3 = 4500;
        SCCI int32_t recovery1 = 50;
        SCCI int32_t recovery2 = 100;
        SCCI int32_t recovery3 = 150;
    };

    struct Shield
    {
        SCCI int32_t cost1 = 2000;
        SCCI int32_t cost2 = 3500;
        SCCI int32_t cost3 = 5000;
        SCCI int32_t defence1 = 50;
        SCCI int32_t defence2 = 100;
        SCCI int32_t defence3 = 150;
    };

    struct Speed_Shoes
    {
        SCCI int32_t speed_boost = 500;
        SCCI int32_t time = 60;
        SCCI int32_t cost = 1500;
    };

    struct purification_medicine
    {
        SCCI int32_t cost = 2000;
        SCCI int32_t time = 30;
    };

    struct invisibility
    {
        SCCI int32_t cost = 4000;
        SCCI int32_t time = 10;
    };

    struct berserk
    {
        SCCI int32_t cost = 10000;
        SCCI int32_t time = 30;
        SCCI int32_t attack_boost = 1.2;        // 注意这是提升的倍数
        SCCI int32_t speed_boost = 300;         // 注意这是直接叠加
        SCCI int32_t attack_freq_boost = 1.25;  // 注意这是提升的倍数
    };
}  // namespace Constants
#endif