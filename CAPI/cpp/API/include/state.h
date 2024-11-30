#pragma once
#ifndef STATE_H
#define STATE_H

#include <vector>
#include <array>
#include <map>
#include <memory>

#include "structures.h"

#undef GetMessage
#undef SendMessage
#undef PeekMessage

// 存储场上的状态
struct State
{
    std::shared_ptr<THUAI8::Character> characterSelf;
    std::shared_ptr<THUAI8::Team> teamSelf;
    std::vector<std::shared_ptr<THUAI8::Character>> characters;
    std::vector<std::shared_ptr<THUAI8::Character>> enemyCharacters;
    std::vector<std::vector<THUAI8::PlaceType>> gameMap;
    std::shared_ptr<THUAI8::GameMap> mapInfo;
    std::shared_ptr<THUAI8::GameInfo> gameInfo;
    std::vector<int64_t> guids;
    std::vector<int64_t> allGuids;
};

#endif