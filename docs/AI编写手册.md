# THUAI8西游真经劫 AI编写手册

## 目录

- [前言](#前言)

- [下载和准备](#下载和准备)

- [游戏机制简介](#游戏机制简介)
- [AI编写基础](#ai编写基础)
  - [Python AI编写](#python-ai编写)
  - [C++ AI编写](#c-ai编写)

- [API接口详解](#api接口详解)
  - [接口体系结构](#接口体系结构)
  - [基本信息获取](#基本信息获取)
  - [角色控制](#角色控制)
  - [资源与建筑](#资源与建筑)
  - [战斗与技能](#战斗与技能)
  - [完整接口列表](#完整接口列表)

- [调试技巧](#调试技巧)
  - [本地调试](#本地调试)
  - [在线调试](#在线调试)

- [示例代码](#示例代码)
  - [Python示例](#python示例)
  - [C++示例](#c示例)

- [常见问题](#常见问题)

## 前言

欢迎参加THUAI8西游真经劫比赛！本手册将指导你如何编写AI以参与这场激动人心的对抗。在开始编写AI之前，建议你首先阅读游戏规则文档，了解游戏的基本机制和胜利条件。

西游真经劫游戏要求你编写7个角色(包括home)的AI代码，分别控制己方阵营的角色（取经团队或妖怪阵营）。通过资源采集、建筑建造、角色技能和团队配合，争取获得更高的得分并击败对手。

## 下载和准备

1. 首先，你需要下载THUAI8游戏包。游戏包包含以下内容：

   待定
   
2. 准备开发环境：

   - Python：确保安装了Python 3.8或更高版本
   - C++：建议使用Visual Studio 2019或更高版本（Windows）或GCC 9.0以上（Linux/MacOS）
3. 环境配置：

   - Python：执行 `CAPI/python/generate_proto.cmd`（Windows）或 `CAPI/python/generate_proto.sh`（Linux/MacOS）生成必要的通信文件
   - C++：使用Visual Studio打开 `CAPI/cpp/CAPI.sln`项目

## 游戏机制简介

THUAI8西游真经劫是一个双方对抗的策略游戏，基本规则如下：

- 游戏采用Bo2赛制，双方轮流扮演【取经团队】和【妖怪】阵营
- 每个阵营有6个角色，包括1个主角（【唐僧】或【九灵元圣】）和5个普通角色
- 每个阵营初始有一个兵营home（角色出生点）
- 游戏地图包含各种区域：空地、障碍物、草丛、建筑和资源点
- 角色可以采集资源、建造建筑、购买装备和攻击敌方角色
- 游戏时长10分钟，7分钟时进入【决战期】
- 游戏结束条件：
  - 【唐僧】被击杀或【九灵元圣】被击杀
  - 时间达到10分钟

得分计算：剩余经济*10 + 击杀敌方角色或摧毁敌方建筑得分 + 获取资源得分

详细规则请参考 `docs/THUAI8游戏规则.md`文档。

## AI编写基础

THUAI8提供了Python和C++两种语言的SDK，你可以选择其中一种进行AI编写。

### Python AI编写

Python AI的入口文件为 `CAPI/python/AI.py`，你需要修改以下两个主要方法：

1. `TeamPlay`方法：控制主角（【唐僧】或【九灵元圣】，PlayerID为0）
2. `CharacterPlay`方法：控制普通角色（其他角色，PlayerID为1-5）

基本结构如下：

```python
def TeamPlay(self, api: ITeamAPI) -> None:
    # 控制主角的代码
    pass

def CharacterPlay(self, api: ICharacterAPI) -> None:
    # 根据PlayerID控制不同角色
    player_id = self.__playerID
    if player_id == 1:
        # 控制第一个普通角色
        pass
    elif player_id == 2:
        # 控制第二个普通角色
        pass
    # ...以此类推
```

### C++ AI编写

C++ AI的入口文件为 `CAPI/cpp/API/src/AI.cpp`，同样包含两个主要方法（通过函数重载实现）：

1. `play`方法（接收ITeamAPI参数）：控制主角
2. `play`方法（接收ICharacterAPI参数）：控制普通角色

基本结构如下：

```cpp
void AI::play(ITeamAPI& api)
{
    // 控制主角的代码
}

void AI::play(ICharacterAPI& api)
{
    // 根据PlayerID控制不同角色
    int playerID = this->playerID;
    if (playerID == 1)
    {
        // 控制第一个普通角色
    }
    else if (playerID == 2)
    {
        // 控制第二个普通角色
    }
    // ...以此类推
}
```

## API接口详解

### 接口体系结构

#### 基础接口架构
- `IAPI` - 基础API接口，提供通用功能
- `ILogic` - 游戏逻辑接口，处理底层游戏机制
- `IAI` - AI接口，AI类需要实现的接口
- `IGameTimer` - 游戏计时器接口，控制游戏时序

#### 角色控制接口
- `ICharacterAPI` - 普通角色控制接口（玩家ID: 1-5）
- `ITeamAPI` - 主角/团队控制接口（玩家ID: 0）

#### 实现类
- `CharacterAPI` - 普通角色API实现
- `TeamAPI` - 主角/团队API实现
- `CharacterDebugAPI`/`TeamDebugAPI` - 调试版API实现

### 基本信息获取

获取游戏基本信息的API：

```python
# 获取自身信息
self_info = api.GetSelfInfo()

# 获取己方角色信息
characters = api.GetCharacters()

# 获取敌方角色信息
enemies = api.GetEnemyCharacters()

# 获取地图信息
map_info = api.GetFullMap()

# 获取当前帧数
frame_count = api.GetFrameCount()

# 获取当前经济值
energy = api.GetEnergy()

# 获取当前得分
score = api.GetScore()

# 检查某个位置是否在视野内
has_view = api.HaveView(target_x, target_y)

# 获取某个格子的类型
place_type = api.GetPlaceType(cell_x, cell_y)

# 获取经济资源状态
resource_state = api.GetEconomyResourceState(cell_x, cell_y)

# 获取加成资源状态
addition_state = api.GetAdditionResourceState(cell_x, cell_y)

# 获取建筑状态
construction_state = api.GetConstructionState(cell_x, cell_y)
```

对应的C++ API：

```cpp
// 获取自身信息
auto selfInfo = api.GetSelfInfo();  // 返回指向角色信息的智能指针

// 获取己方角色信息
auto characters = api.GetCharacters();  // 返回角色指针的vector

// 获取敌方角色信息
auto enemies = api.GetEnemyCharacters();  // 返回敌方角色指针的vector

// 获取地图信息
auto mapInfo = api.GetFullMap();  // 返回二维格子类型数组

// 获取当前帧数
int frameCount = api.GetFrameCount();

// 获取当前经济值和得分
int energy = api.GetEnergy();
int score = api.GetScore();

// 检查是否在视野范围内
bool inView = api.HaveView(targetX, targetY);  // 传入格子坐标

// 获取格子类型
auto placeType = api.GetPlaceType(cellX, cellY);

// 获取资源状态(返回std::optional)
auto economyResource = api.GetEconomyResourceState(cellX, cellY);
auto additionResource = api.GetAdditionResourceState(cellX, cellY);

// 获取建筑状态
auto construction = api.GetConstructionState(cellX, cellY);
```

### 角色控制

控制角色移动和操作的API：

```python
# 向指定方向移动
# speed: 移动速度，单位为像素/秒，范围在0-5000之间
# time_in_milliseconds: 移动时间，单位为毫秒
# angle_in_radian: 移动角度，单位为弧度，0表示向下，π/2表示向右，π表示向上，3π/2表示向左
api.Move(speed, time_in_milliseconds, angle_in_radian)

# 向四个基本方向移动
api.MoveRight(speed, time_in_milliseconds)  # 向右移动
api.MoveLeft(speed, time_in_milliseconds)   # 向左移动
api.MoveUp(speed, time_in_milliseconds)     # 向上移动
api.MoveDown(speed, time_in_milliseconds)   # 向下移动

# 等待下一帧
api.Wait()

# 结束所有动作
api.EndAllAction()

# 坐标转换辅助函数
grid_pos = api.CellToGrid(cell_pos)  # 格子坐标转为像素坐标
cell_pos = api.GridToCell(grid_pos)  # 像素坐标转为格子坐标
```

对应的C++ API：

```cpp
// 向指定方向移动
// speed: 移动速度(像素/秒, 0-5000)
// timeInMilliseconds: 持续时间(毫秒)
// angleInRadian: 移动角度(弧度), 0表示向下，π/2向右，π向上，3π/2向左
api.Move(speed, timeInMilliseconds, angleInRadian);

// 向四个基本方向移动
api.MoveRight(speed, timeInMilliseconds);  // 向右移动(π/2方向)
api.MoveLeft(speed, timeInMilliseconds);   // 向左移动(3π/2方向)
api.MoveUp(speed, timeInMilliseconds);     // 向上移动(π方向)
api.MoveDown(speed, timeInMilliseconds);   // 向下移动(0方向)

// 等待下一帧
api.Wait();

// 结束所有动作
api.EndAllAction();

// 坐标转换函数
int gridPos = IAPI::CellToGrid(cellPos);  // 格子坐标转像素坐标(中心点)
int cellPos = IAPI::GridToCell(gridPos);  // 像素坐标转格子坐标
```

### 资源与建筑

资源采集和建筑相关API：

```python
# 开采资源
api.Harvest()

# 建造建筑
api.Construct(construction_type)

# 修复建筑
api.Rebuild(construction_type)

# 团队控制中的额外API（仅在TeamPlay中可用）
# 召唤角色
api.BuildCharacter(character_type, birth_index)

# 安装装备
api.InstallEquipment(player_id, equipment_type)

# 回收角色
api.Recycle(player_id)
```

对应的C++ API：

```cpp
// 采集资源 - 角色站在资源点上时调用
api.Harvest();

// 建造建筑 - 在当前位置建造指定类型的建筑
// constructionType: 建筑类型枚举
api.Construct(constructionType);

// 修复建筑 - 站在建筑旁边时修复
// constructionType: 需要修复的建筑类型
api.Rebuild(constructionType);

// 团队控制API(仅在TeamPlay中可用)
// 召唤角色 - 创建新角色
// characterType: 角色类型枚举
// birthIndex: 出生点索引
api.BuildCharacter(characterType, birthIndex);

// 为角色安装装备
// playerID: 目标角色ID
// equipmentType: 装备类型枚举
api.InstallEquipment(playerID, equipmentType);

// 回收角色 - 回收角色并获得部分经济返还
// playerID: 要回收的角色ID
api.Recycle(playerID);
```

### 战斗与技能

战斗和技能相关API：

```python
# 普通攻击
api.Common_Attack(attacked_player_id)

# 使用技能
api.Skill_Attack(attacked_player_id)

# 恢复
api.Recover(recoverType)
```

对应的C++ API：

```cpp
// 普通攻击 - 对指定ID的玩家进行普通攻击
// attackedPlayerID: 目标玩家ID
api.Common_Attack(attackedPlayerID);

// 技能攻击 - 对指定ID的玩家使用技能攻击
// attackedPlayerID: 目标玩家ID
api.Skill_Attack(attackedPlayerID);

// 恢复生命值 - 使用恢复物品恢复生命
// recoverType: 恢复类型枚举(小、中、大型恢复)
api.Recover(recoverType);
```

### 完整接口列表

#### 通用IAPI接口
```python
# 消息通信
SendTextMessage(toPlayerID, message) -> Future[bool]  # 发送文本消息
SendBinaryMessage(toPlayerID, message) -> Future[bool]  # 发送二进制消息
HaveMessage() -> bool  # 检查是否有消息
GetMessage() -> Tuple[int, str]  # 获取消息(发送者ID, 消息内容)

# 游戏状态
GetFrameCount() -> int  # 获取当前帧数
Wait() -> bool  # 等待下一帧
EndAllAction() -> Future[bool]  # 终止所有动作

# 地图与信息获取
GetCharacters() -> List[Character]  # 获取己方角色列表
GetEnemyCharacters() -> List[Character]  # 获取敌方角色列表
GetFullMap() -> List[List[PlaceType]]  # 获取地图信息
GetGameInfo() -> GameInfo  # 获取游戏信息
GetPlaceType(cellX, cellY) -> PlaceType  # 获取指定位置类型
GetEconomyResourceState(cellX, cellY) -> Optional[EconomyResourceState]  # 获取经济资源状态
GetAdditionResourceState(cellX, cellY) -> Optional[AdditionResourceState]  # 获取加成资源状态
GetConstructionState(cellX, cellY) -> Optional[ConstructionState]  # 获取建筑状态
GetPlayerGUIDs() -> List[int]  # 获取玩家GUID列表
GetEnergy() -> int  # 获取当前经济值
GetScore() -> int  # 获取当前得分

# 调试
Print(string) -> None  # 打印调试信息
PrintCharacter() -> None  # 打印角色信息
PrintTeam() -> None  # 打印团队信息
PrintSelfInfo() -> None  # 打印自身信息

# 坐标转换
CellToGrid(cell) -> int  # 格子坐标转为像素坐标(中心点)
GridToCell(grid) -> int  # 像素坐标转为格子坐标
```

#### ICharacterAPI接口(普通角色)
```python
# 继承IAPI的所有方法，另外提供：

# 角色信息
GetSelfInfo() -> Character  # 获取自身角色信息

# 移动控制
Move(speed, timeInMilliseconds, angleInRadian) -> Future[bool]  # 向指定方向移动
MoveRight(speed, timeInMilliseconds) -> Future[bool]  # 向右移动
MoveUp(speed, timeInMilliseconds) -> Future[bool]  # 向上移动
MoveLeft(speed, timeInMilliseconds) -> Future[bool]  # 向左移动
MoveDown(speed, timeInMilliseconds) -> Future[bool]  # 向下移动

# 攻击与技能
Skill_Attack(attackedPlayerID) -> Future[bool]  # 技能攻击
Common_Attack(attackedPlayerID) -> Future[bool]  # 普通攻击
Recover(recoverType) -> Future[bool]  # 恢复生命值

# 资源与建筑
Harvest() -> Future[bool]  # 采集资源
Rebuild(constructionType) -> Future[bool]  # 修复建筑
Construct(constructionType) -> Future[bool]  # 建造建筑

# 视野检查
HaveView(targetX, targetY) -> bool  # 检查目标位置是否在视野内
```

#### ITeamAPI接口(主角)
```python
# 继承IAPI的所有方法，另外提供：

# 队伍信息
GetSelfInfo() -> Team  # 获取团队信息

# 装备与角色管理
InstallEquipment(playerID, equipmentType) -> Future[bool]  # 为角色安装装备
Recycle(playerID) -> Future[bool]  # 回收角色
BuildCharacter(characterType, birthIndex) -> Future[bool]  # 创建角色
```

#### C++接口
```cpp
// ICharacterAPI(普通角色)接口
std::future<bool> Move(int32_t speed, int64_t timeInMilliseconds, double angleInRadian);  // 向指定方向移动
std::future<bool> MoveRight(int32_t speed, int64_t timeInMilliseconds);  // 向右移动
std::future<bool> MoveUp(int32_t speed, int64_t timeInMilliseconds);  // 向上移动
std::future<bool> MoveLeft(int32_t speed, int64_t timeInMilliseconds);  // 向左移动
std::future<bool> MoveDown(int32_t speed, int64_t timeInMilliseconds);  // 向下移动
std::future<bool> Skill_Attack(int64_t attackedPlayerID);  // 技能攻击
std::future<bool> Common_Attack(int64_t attackedPlayerID);  // 普通攻击
std::future<bool> Recover(int64_t recoverType);  // 恢复生命值
std::future<bool> Harvest();  // 采集资源
std::future<bool> Rebuild(THUAI8::ConstructionType constructionType);  // 修复建筑
std::future<bool> Construct(THUAI8::ConstructionType constructionType);  // 建造建筑
std::shared_ptr<const THUAI8::Character> GetSelfInfo() const;  // 获取自身信息
bool HaveView(int32_t targetX, int32_t targetY) const;  // 检查视野

// ITeamAPI(主角)接口
std::shared_ptr<const THUAI8::Team> GetSelfInfo() const;  // 获取团队信息
std::future<bool> InstallEquipment(int32_t playerID, THUAI8::EquipmentType equipmentType);  // 为角色安装装备
std::future<bool> Recycle(int32_t playerID);  // 回收角色
std::future<bool> BuildCharacter(THUAI8::CharacterType characterType, int32_t birthIndex);  // 创建角色

// 继承自IAPI的通用接口(两种角色均可使用)
std::future<bool> SendTextMessage(int32_t toPlayerID, std::string message);  // 发送文本消息
std::future<bool> SendBinaryMessage(int32_t toPlayerID, std::string message);  // 发送二进制消息
bool HaveMessage();  // 检查是否有消息
std::pair<int32_t, std::string> GetMessage();  // 获取消息(发送者ID, 内容)
int32_t GetFrameCount() const;  // 获取当前帧数
bool Wait();  // 等待下一帧
std::future<bool> EndAllAction();  // 终止所有动作
std::vector<std::shared_ptr<const THUAI8::Character>> GetCharacters() const;  // 获取己方角色
std::vector<std::shared_ptr<const THUAI8::Character>> GetEnemyCharacters() const;  // 获取敌方角色
std::vector<std::vector<THUAI8::PlaceType>> GetFullMap() const;  // 获取地图信息
THUAI8::PlaceType GetPlaceType(int32_t cellX, int32_t cellY) const;  // 获取格子类型
std::optional<THUAI8::EconomyResourceState> GetEconomyResourceState(int32_t cellX, int32_t cellY) const;  // 获取经济资源
std::optional<THUAI8::AdditionResourceState> GetAdditionResourceState(int32_t cellX, int32_t cellY) const;  // 获取加成资源
std::optional<THUAI8::ConstructionState> GetConstructionState(int32_t cellX, int32_t cellY) const;  // 获取建筑状态
int32_t GetEnergy() const;  // 获取经济值
int32_t GetScore() const;  // 获取得分
void Print(std::string str) const;  // 打印调试信息
```

## 调试技巧

### 本地调试

使用本地调试工具测试你的AI：

1. 编译安装启动器：

   - 编译 `installer`项目或使用预编译版本
   - 下载完整游戏包
2. 启动本地调试：

   - 打开启动器，切换到 `Debug`选项卡
   - 配置 `Server`参数：设置端口、团队数和角色数量
   - 启动 `Server`
   - 配置 `Client`参数：设置IP（本地为127.0.0.1）、团队ID、角色ID等
   - 启动 `Client`
3. 日志输出：
   可以通过以下方式输出调试信息：

   ```python
   # Python中输出调试信息
   api.Print("调试信息")  # 使用SDK提供的接口
   import sys
   sys.stderr.write("调试信息\n")  # 使用标准错误流
   ```

   ```cpp
   // C++中输出调试信息
   api.Print("调试信息");  // 使用SDK提供的接口
   std::cerr << "调试信息" << std::endl;  // 使用标准错误流
   ```

### 在线调试

在提交到在线评测系统时，你可以：

1. 继续使用 `api.Print`或标准错误流输出调试信息
2. 在评测结束后查看调试输出
3. 通过回放功能分析游戏过程

**注意**：过多的调试输出可能导致缓冲区溢出，影响AI运行，请适量使用。

## 示例代码


>
> **代码结构说明**：
> - **类设计**：使用单一AI类实现所有角色的控制
> - **职责分离**：通过方法划分不同角色的行为逻辑
> - **决策模式**：基于简单条件判断的反应式决策
>
> **可优化方向**：
> - **策略优化**：添加更复杂的决策逻辑，如状态机或行为树
> - **团队协作**：实现角色间的信息共享和协同作战
> - **路径规划**：添加A*等寻路算法，优化移动策略
> - **战斗策略**：设计更高级的战术和目标选择算法
>
> **注意事项**：
>
> - 仅作为参考
> - 鼓励基于这些示例进行创新和优化
> - 示例中的参数和策略仅供参考，实际使用需根据游戏情况调整
>
> 祝各位参赛者取得好成绩！

### Python示例

```python
import PyAPI.structures as THUAI8
from PyAPI.Interface import ICharacterAPI, ITeamAPI, IAI
import math
import random

class AI(IAI):
    # [结构] 构造函数 - 初始化AI角色的基本属性
    # [优化提示] 可以添加更多状态变量，如目标位置、战术模式、已知资源点等
    def __init__(self, pID: int):
        self.__playerID = pID
        self.__last_skill_time = 0
    
    # [结构] 主角控制方法 - 团队指挥官的核心决策逻辑
    # [优化提示] 考虑添加全局战略决策，如经济模式、防守模式、进攻模式等
    def TeamPlay(self, api: ITeamAPI) -> None:
        """
        主角（唐僧或九灵元圣）的控制逻辑
        """
        # 获取自身信息
        self_info = api.GetSelfInfo()
        if self_info is None:
            return
        
        # 获取当前帧数和经济状况
        current_frame = api.GetFrameCount()
        energy = api.GetEnergy()
        score = api.GetScore()
    
        # 打印关键信息
        if current_frame % 300 == 0:
            api.Print(f"帧数: {current_frame}, 经济值: {energy}, 得分: {score}")
    
        # 1. 角色召唤
        # [优化提示] 根据战局情况动态决定角色组合，而不是固定角色
        self.__manage_characters(api, self_info, current_frame, energy)
    
        # 2. 移动逻辑
        # [优化提示] 添加地图分析，探索未知区域，避开危险区域
        self.__move_commander(api, self_info, current_frame)
    
        # 3. 等待下一帧
        api.Wait()
    
    # [结构] 角色管理方法 - 负责召唤和装备角色
    # [优化提示] 实现更复杂的装备分配策略，根据角色表现和任务动态分配资源
    def __manage_characters(self, api: ITeamAPI, self_info, current_frame, energy):
        """角色管理与召唤"""
        # 检查现有角色
        allies = api.GetCharacters()
        active_roles = {ally.playerID for ally in allies}
        
        # 早期阶段召唤角色
        # [优化提示] 考虑地图和对手特点，优化角色选择策略
        if current_frame < 3000 and energy >= 3000:
            if 1 not in active_roles:
                # 根据团队选择角色类型
                character_type = THUAI8.CharacterType.MonkyKing if self_info.teamID == 0 else THUAI8.CharacterType.HongHaier
                api.BuildCharacter(character_type, 0)
                api.Print(f"召唤角色 playerID: 1")
            elif 2 not in active_roles and energy >= 4000:
                character_type = THUAI8.CharacterType.Pigsy if self_info.teamID == 0 else THUAI8.CharacterType.Gyuumao
                api.BuildCharacter(character_type, 1)
                api.Print(f"召唤角色 playerID: 2")
        
        # 装备管理
        # [优化提示] 考虑角色类型和当前任务为其选择最适合的装备
        if current_frame % 1000 == 0 and energy >= 2000:
            for ally in allies:
                if ally.playerID > 0:  # 给普通角色装备
                    api.InstallEquipment(ally.playerID, THUAI8.EquipmentType.Shield)
                    api.Print(f"为角色 {ally.playerID} 安装装备")
                    break
    
    # [结构] 主角移动方法 - 控制主角的移动策略
    # [优化提示] 添加安全区域计算和风险评估，避免被敌方包围
    def __move_commander(self, api: ITeamAPI, self_info, current_frame):
        """主角移动策略"""
        # 获取敌方角色
        enemies = api.GetEnemyCharacters()
        
        # 根据是否有敌人决定移动策略
        # [优化提示] 可以添加战场态势分析，如敌方数量和分布，判断是躲避还是反击
        if enemies:
            # 找到最近的敌人
            nearest_enemy = min(enemies, key=lambda e: 
                (e.x - self_info.x)**2 + (e.y - self_info.y)**2)
            
            # 计算远离方向
            # [优化提示] 考虑地形因素，避开可能被困的死角
            angle = math.atan2(self_info.y - nearest_enemy.y, self_info.x - nearest_enemy.x)
            
            # 远离敌人
            api.EndAllAction()
            api.Move(2500, 1000, angle)  # 速度2500，持续1000毫秒，按计算的角度移动
            api.Print("远离敌人中")
        else:
            # 探索移动
            # [优化提示] 替换为更系统的探索算法，避免重复探索相同区域
            if current_frame % 200 < 50:
                api.MoveRight(2000, 500)
            elif current_frame % 200 < 100:
                api.MoveUp(2000, 500)
            elif current_frame % 200 < 150:
                api.MoveLeft(2000, 500)
            else:
                api.MoveDown(2000, 500)
        
        # 定期恢复
        # [优化提示] 根据血量和威胁程度动态决定恢复时机
        if current_frame % 1000 == 0:
            api.Recover(THUAI8.RecoverType.Small)
        
    # [结构] 普通角色控制方法 - 不同角色的行为逻辑入口
    # [优化提示] 考虑添加角色状态机或行为树，实现更复杂的决策逻辑
    def CharacterPlay(self, api: ICharacterAPI) -> None:
        """
        普通角色的控制逻辑
        """
        # 获取自身信息
        self_info = api.GetSelfInfo()
        if self_info is None:
            return
        
        # 获取当前帧数
        current_frame = api.GetFrameCount()
        
        # 打印角色状态
        if current_frame % 100 == 0:
            api.Print(f"角色 {self.__playerID} 运行中, HP: {self_info.hp}")
        
        # 针对不同角色类型执行不同策略
        # [优化提示] 可以基于角色能力和当前战局动态分配任务，而非固定角色职责
        if self.__playerID == 1:  # 资源收集者
            self.__resource_collector(api, self_info)
        elif self.__playerID == 2:  # 建造者
            self.__builder(api, self_info, current_frame)
        elif self.__playerID in {3, 4, 5}:  # 战斗角色
            self.__fighter(api, self_info, current_frame)
        
        api.Wait()  # 等待下一帧
    
    # [结构] 资源收集者方法 - 负责寻找和采集资源
    # [优化提示] 添加资源价值评估和记忆功能，优先采集高价值资源
    def __resource_collector(self, api: ICharacterAPI, self_info):
        """资源收集者"""
        # 当前位置
        current_cell_x = api.GridToCell(self_info.x)
        current_cell_y = api.GridToCell(self_info.y)
        
        # 检查周围是否有经济资源
        # [优化提示] 可以持续记录和更新资源位置，减少重复搜索
        for dx in range(-1, 2):
            for dy in range(-1, 2):
                check_x, check_y = current_cell_x + dx, current_cell_y + dy
                
                # 如果有资源且在视野内
                # [优化提示] 添加资源价值评估，优先采集高价值资源
                if api.HaveView(check_x, check_y):
                    resource_state = api.GetEconomyResourceState(check_x, check_y)
                    if resource_state is not None:
                        # 开始采集
                        api.Harvest()
                        return
        
        # 如果周围没有资源，随机移动寻找资源
        # [优化提示] 替换为系统化的探索策略，如螺旋探索或分区搜索
        angle = random.uniform(0, 2 * math.pi)
        api.Move(2500, 1000, angle)
    
    # [结构] 建造者方法 - 负责修复和建造建筑
    # [优化提示] 添加建筑价值评估和位置优化
    def __builder(self, api: ICharacterAPI, self_info, current_frame):
        """建造者"""
        # 当前位置
        current_cell_x = api.GridToCell(self_info.x)
        current_cell_y = api.GridToCell(self_info.y)
        
        # 检查周围是否有需要修复的己方建筑
        # [优化提示] 建立建筑状态记录，优先修复重要或严重受损的建筑
        for dx in range(-2, 3):
            for dy in range(-2, 3):
                check_x, check_y = current_cell_x + dx, current_cell_y + dy
                if api.HaveView(check_x, check_y):
                    construction = api.GetConstructionState(check_x, check_y)
                    if construction is not None and construction.teamID == self_info.teamID and construction.hp < construction.maxHp * 0.7:
                        # 修复建筑
                        api.Rebuild(construction.constructionType)
                        api.Print("修复建筑中")
                        return
        
        # 检查是否可以建造建筑
        # [优化提示] 添加位置评估算法，选择战略价值高的建造位置
        if api.GetPlaceType(current_cell_x, current_cell_y) == THUAI8.PlaceType.Land:
            # 根据游戏阶段选择建筑类型
            # [优化提示] 根据团队经济状况和战略需求选择建筑类型
            if current_frame < 3000:  # 早期
                api.Construct(THUAI8.ConstructionType.Farm)
                api.Print("建造农场中")
            else:  # 中后期
                api.Construct(THUAI8.ConstructionType.Trap)
                api.Print("建造陷阱中")
            return
        
        # 随机移动寻找建造位置
        # [优化提示] 实现有目标的移动，寻找战略要地进行建造
        angle = random.uniform(0, 2 * math.pi)
        api.Move(2000, 800, angle)
    
    # [结构] 战斗角色方法 - 负责战斗和攻击敌人
    # [优化提示] 添加战术决策和目标优先级评估
    def __fighter(self, api: ICharacterAPI, self_info, current_frame):
        """战斗角色"""
        # 获取敌方角色
        enemies = api.GetEnemyCharacters()
        
        # 检查自身生命值
        # [优化提示] 考虑周围队友和敌人情况，综合决定是否撤退或继续战斗
        low_health = self_info.hp < self_info.maxHp * 0.4
        
        # 如果生命值低，进行恢复
        if low_health:
            api.Recover(THUAI8.RecoverType.Small)
            api.Print("血量低，进行恢复")
            return
        
        # 如果有敌人
        if enemies:
            # 找到最近的敌人
            # [优化提示] 目标选择可基于多种因素，如敌人血量、距离、威胁程度等
            nearest_enemy = min(enemies, key=lambda e: 
                (e.x - self_info.x)**2 + (e.y - self_info.y)**2)
            
            # 计算距离
            distance = ((nearest_enemy.x - self_info.x)**2 + (nearest_enemy.y - self_info.y)**2)**0.5
            
            if distance <= self_info.attackRange * 1000:
                # 在攻击范围内，进行攻击
                # [优化提示] 考虑技能冷却和效果，选择最佳攻击时机
                api.Common_Attack(nearest_enemy.playerID)
                api.Print(f"攻击目标 {nearest_enemy.playerID}")
                
                # 技能攻击
                # [优化提示] 根据敌人类型和战局选择最合适的技能
                if current_frame - self.__last_skill_time > 2000:  # 技能冷却时间
                    api.Skill_Attack(nearest_enemy.playerID)
                    self.__last_skill_time = current_frame
            else:
                # 不在攻击范围内，向敌人移动
                # [优化提示] 考虑地形和障碍物，选择最优接近路径
                angle = math.atan2(nearest_enemy.y - self_info.y, nearest_enemy.x - self_info.x)
                api.Move(2500, 1000, angle)  # 速度2500，持续1000毫秒，向敌人方向移动
        else:
            # 没有敌人，随机移动
            # [优化提示] 可以巡逻关键区域或配合队友行动，而不是随机移动
            angle = random.uniform(0, 2 * math.pi)
            api.Move(2000, 1000, angle)
```

### C++示例

```cpp
#include "AI.h"
#include <cmath>
#include <random>
#include <algorithm>

// [结构] AI类成员变量 - 存储AI决策所需的状态信息
// [优化提示] 可以添加更多状态变量，如已知资源点、战术模式等
private:
    int playerID;
    int lastSkillTime;
    std::random_device rd;
    std::mt19937 gen;

// [结构] AI类构造函数 - 初始化基本属性
// [优化提示] 可以初始化更多的策略相关变量
AI::AI(int playerID)
    : playerID(playerID), lastSkillTime(0), gen(rd())
{
}

// [结构] 主角控制方法 - 团队指挥官的行为决策
// [优化提示] 考虑添加全局战略决策，如经济发展或猛攻策略
void AI::play(ITeamAPI& api)
{
    // 获取自身信息
    auto selfInfo = api.GetSelfInfo();
    if (selfInfo == nullptr)
        return;
    
    // 获取当前帧数和经济状况
    int currentFrame = api.GetFrameCount();
    int energy = api.GetEnergy();
    int score = api.GetScore();
  
    // 打印关键信息
    if (currentFrame % 300 == 0)
    {
        api.Print("帧数: " + std::to_string(currentFrame) + 
                 ", 经济值: " + std::to_string(energy) + 
                 ", 得分: " + std::to_string(score));
    }
  
    // 角色召唤逻辑
    // [优化提示] 根据当前战局和队伍情况动态调整召唤策略
    ManageCharacters(api, selfInfo, currentFrame, energy);
  
    // 主角移动逻辑
    // [优化提示] 添加战场态势分析，优化移动决策
    MoveCommander(api, selfInfo, currentFrame);
  
    // 等待下一帧
    api.Wait();
}

// [结构] 角色管理方法 - 负责召唤和装备角色
// [优化提示] 实现更复杂的角色选择和装备策略
void AI::ManageCharacters(ITeamAPI& api, std::shared_ptr<const THUAI8::Character> selfInfo, 
                         int currentFrame, int energy)
{
    // 检查现有角色
    auto allies = api.GetCharacters();
    std::set<int> activeRoles;
  
    for (const auto& ally : allies)
    {
        activeRoles.insert(ally->playerID);
    }
  
    // 早期阶段召唤角色
    // [优化提示] 考虑地图和对手特点，选择最适合的角色组合
    if (currentFrame < 3000 && energy >= 3000)
    {
        if (activeRoles.find(1) == activeRoles.end())
        {
            THUAI8::CharacterType characterType = 
                selfInfo->teamID == 0 ? THUAI8::CharacterType::MonkeyKing : THUAI8::CharacterType::Honghaier;
            api.BuildCharacter(characterType, 0);
            api.Print("召唤角色 playerID: 1");
        }
        else if (activeRoles.find(2) == activeRoles.end() && energy >= 4000)
        {
            THUAI8::CharacterType characterType = 
                selfInfo->teamID == 0 ? THUAI8::CharacterType::Pigsy : THUAI8::CharacterType::Gyuumao;
            api.BuildCharacter(characterType, 1);
            api.Print("召唤角色 playerID: 2");
        }
    }
  
    // 装备管理
    // [优化提示] 根据角色类型和战局需求为角色选择最适合的装备
    if (currentFrame % 1000 == 0 && energy >= 2000)
    {
        for (const auto& ally : allies)
        {
            if (ally->playerID > 0)  // 给普通角色装备
            {
                api.InstallEquipment(ally->playerID, THUAI8::EquipmentType::Shield);
                api.Print("为角色 " + std::to_string(ally->playerID) + " 安装装备");
                break;
            }
        }
    }
}

// [结构] 主角移动方法 - 控制主角的移动策略
// [优化提示] 添加安全区域计算和风险评估
void AI::MoveCommander(ITeamAPI& api, std::shared_ptr<const THUAI8::Character> selfInfo, int currentFrame)
{
    // 获取敌方角色
    auto enemies = api.GetEnemyCharacters();
    
    // 根据是否有敌人决定移动策略
    // [优化提示] 添加战场态势分析，做出更明智的移动决策
    if (!enemies.empty())
    {
        // 找到最近的敌人
        auto nearestEnemy = *std::min_element(enemies.begin(), enemies.end(),
            [selfInfo](const auto& a, const auto& b) {
                double distA = std::pow(a->x - selfInfo->x, 2) + std::pow(a->y - selfInfo->y, 2);
                double distB = std::pow(b->x - selfInfo->x, 2) + std::pow(b->y - selfInfo->y, 2);
                return distA < distB;
            });
        
        // 计算远离方向
        // [优化提示] 考虑障碍物和地形，避开可能被困的死角
        double angle = std::atan2(selfInfo->y - nearestEnemy->y, selfInfo->x - nearestEnemy->x);
        
        // 远离敌人
        api.EndAllAction();
        api.Move(2500, 1000, angle);  // 速度2500，持续1000毫秒，按计算的角度移动
        api.Print("远离敌人中");
    }
    else
    {
        // 探索移动
        // [优化提示] 实现有系统的探索算法，提高地图探索效率
        if (currentFrame % 200 < 50)
            api.MoveRight(2000, 500);
        else if (currentFrame % 200 < 100)
            api.MoveUp(2000, 500);
        else if (currentFrame % 200 < 150)
            api.MoveLeft(2000, 500);
        else
            api.MoveDown(2000, 500);
    }
    
    // 定期恢复
    // [优化提示] 根据当前血量和威胁程度动态决定恢复时机
    if (currentFrame % 1000 == 0)
    {
        api.Recover(THUAI8::RecoverType::Small);
    }
}

// [结构] 普通角色控制方法 - 不同角色的行为逻辑入口
// [优化提示] 考虑使用状态机或行为树实现更复杂的行为决策
void AI::play(ICharacterAPI& api)
{
    // 获取自身信息
    auto selfInfo = api.GetSelfInfo();
    if (selfInfo == nullptr)
        return;
    
    // 获取当前帧数
    int currentFrame = api.GetFrameCount();
    
    // 打印角色状态
    if (currentFrame % 100 == 0)
    {
        api.Print("角色 " + std::to_string(this->playerID) + 
                 " 运行中, HP: " + std::to_string(selfInfo->hp));
    }
    
    // 针对不同角色类型执行不同策略
    // [优化提示] 根据战局动态分配角色任务，而非固定角色职责
    if (this->playerID == 1)  // 资源收集者
        ResourceCollector(api, selfInfo);
    else if (this->playerID == 2)  // 建造者
        Builder(api, selfInfo, currentFrame);
    else if (this->playerID >= 3 && this->playerID <= 5)  // 战斗角色
        Fighter(api, selfInfo, currentFrame);
    
    api.Wait();  // 等待下一帧
}

// [结构] 资源收集者方法 - 负责寻找和采集资源
// [优化提示] 添加资源记忆和价值评估，提高采集效率
void AI::ResourceCollector(ICharacterAPI& api, std::shared_ptr<const THUAI8::Character> selfInfo)
{
    // 当前位置
    int currentCellX = api.GridToCell(selfInfo->x);
    int currentCellY = api.GridToCell(selfInfo->y);
    
    // 检查周围是否有经济资源
    // [优化提示] 维护已知资源位置的记录，减少重复搜索
    for (int dx = -1; dx <= 1; dx++)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            int checkX = currentCellX + dx;
            int checkY = currentCellY + dy;
            
            // 如果有资源且在视野内
            // [优化提示] a 对资源进行价值评估，优先采集高价值资源
            if (api.HaveView(checkX, checkY))
            {
                auto resourceState = api.GetEconomyResourceState(checkX, checkY);
                if (resourceState.has_value())
                {
                    // 开始采集
                    api.Harvest();
                    return;
                }
            }
        }
    }
    
    // 如果周围没有资源，随机移动寻找资源
    // [优化提示] 实现有系统的资源探索策略，如螺旋探索或分区搜索
    std::uniform_real_distribution<> dis(0, 2 * 3.14159);
    double angle = dis(gen);
    api.Move(2500, 1000, angle);  // 速度2500，持续1000毫秒，按随机角度移动
}

// [结构] 建造者方法 - 负责修复和建造建筑
// [优化提示] 添加建筑位置评估和建筑类型选择策略
void AI::Builder(ICharacterAPI& api, std::shared_ptr<const THUAI8::Character> selfInfo, int currentFrame)
{
    // 当前位置
    int currentCellX = api.GridToCell(selfInfo->x);
    int currentCellY = api.GridToCell(selfInfo->y);
    
    // 检查周围是否有需要修复的己方建筑
    // [优化提示] 建立建筑状态记录，优先修复重要建筑
    for (int dx = -2; dx <= 2; dx++)
    {
        for (int dy = -2; dy <= 2; dy++)
        {
            int checkX = currentCellX + dx;
            int checkY = currentCellY + dy;
            if (api.HaveView(checkX, checkY))
            {
                auto construction = api.GetConstructionState(checkX, checkY);
                if (construction.has_value() && construction->teamID == selfInfo->teamID && 
                    construction->hp < construction->maxHp * 0.7)
                {
                    // 修复建筑
                    api.Rebuild(construction->constructionType);
                    api.Print("修复建筑中");
                    return;
                }
            }
        }
    }
    
    // 检查是否可以建造建筑
    // [优化提示] 评估位置战略价值，选择最优建造位置
    if (api.GetPlaceType(currentCellX, currentCellY) == THUAI8::PlaceType::Land)
    {
        // 根据游戏阶段选择建筑类型
        // [优化提示] 根据团队经济状况和战略需求选择建筑类型
        if (currentFrame < 3000)  // 早期
        {
            api.Construct(THUAI8::ConstructionType::Farm);
            api.Print("建造农场中");
        }
        else  // 中后期
        {
            api.Construct(THUAI8::ConstructionType::Trap);
            api.Print("建造陷阱中");
        }
        return;
    }
    
    // 随机移动寻找建造位置
    // [优化提示] 向战略要地移动，而非随机移动
    std::uniform_real_distribution<> dis(0, 2 * 3.14159);
    double angle = dis(gen);
    api.Move(2000, 800, angle);  // 速度2000，持续800毫秒，按随机角度移动
}

// [结构] 战斗角色方法 - 负责战斗和攻击敌人
// [优化提示] 添加战术决策和目标优先级评估
void AI::Fighter(ICharacterAPI& api, std::shared_ptr<const THUAI8::Character> selfInfo, int currentFrame)
{
    // 获取敌方角色
    auto enemies = api.GetEnemyCharacters();
    
    // 检查自身生命值
    // [优化提示] 综合考虑周围队友和敌人情况，决定是否撤退
    bool lowHealth = selfInfo->hp < selfInfo->maxHp * 0.4;
    
    // 如果生命值低，进行恢复
    if (lowHealth)
    {
        api.Recover(THUAI8::RecoverType::Small);
        api.Print("血量低，进行恢复");
        return;
    }
    
    // 如果有敌人
    if (!enemies.empty())
    {
        // 找到最近的敌人
        // [优化提示] 目标选择可基于多种因素，如敌人血量、职责和威胁程度
        auto nearestEnemy = *std::min_element(enemies.begin(), enemies.end(),
            [selfInfo](const auto& a, const auto& b) {
                double distA = std::pow(a->x - selfInfo->x, 2) + std::pow(a->y - selfInfo->y, 2);
                double distB = std::pow(b->x - selfInfo->x, 2) + std::pow(b->y - selfInfo->y, 2);
                return distA < distB;
            });
        
        // 计算距离
        double distance = std::sqrt(std::pow(nearestEnemy->x - selfInfo->x, 2) + 
                                  std::pow(nearestEnemy->y - selfInfo->y, 2));
        
        if (distance <= selfInfo->attackRange * 1000)
        {
            // 在攻击范围内，进行攻击
            // [优化提示] 考虑攻击时机，避免无效攻击
            api.Common_Attack(nearestEnemy->playerID);
            api.Print("攻击目标 " + std::to_string(nearestEnemy->playerID));
            
            // 技能攻击
            // [优化提示] 根据敌人类型和战局选择合适的技能和时机
            if (currentFrame - this->lastSkillTime > 2000)  // 技能冷却时间
            {
                api.Skill_Attack(nearestEnemy->playerID);
                this->lastSkillTime = currentFrame;
            }
        }
        else
        {
            // 不在攻击范围内，向敌人移动
            // [优化提示] 考虑地形和障碍物，实现智能寻路
            double angle = std::atan2(nearestEnemy->y - selfInfo->y, nearestEnemy->x - selfInfo->x);
            api.Move(2500, 1000, angle);  // 速度2500，持续1000毫秒，向敌人方向移动
        }
    }
    else
    {
        // 没有敌人，随机移动
        // [优化提示] 实现目标巡逻或配合团队行动，而非随机移动
        std::uniform_real_distribution<> dis(0, 2 * 3.14159);
        double angle = dis(gen);
        api.Move(2000, 1000, angle);  // 速度2000，持续1000毫秒，按随机角度移动
    }
}
```

## 常见问题

**Q: 如何确定角色的位置？**  
A: 使用`api.GetSelfInfo()`获取角色位置，位置是以像素为单位的坐标 (x, y)。

**Q: 如何计算两点间距离？**  
A: 使用欧几里得距离公式：`distance = sqrt((x1-x2)^2 + (y1-y2)^2)`。

**Q: 如何判断是否可以攻击到敌人？**  
A: 检查敌人是否在角色的攻击范围内：`distance <= self_info.attackRange * 1000`。

**Q: 为什么我的AI在本地运行正常，提交后却失败？**  
A: 可能原因包括：
   - 代码中存在无限循环
   - 调试输出过多导致缓冲区溢出
   - 未考虑到某些边界情况

**Q: 如何优化AI性能？**  
A: 
   - 减少不必要的API调用
   - 避免复杂计算导致的超时
   - 使用高效的寻路和决策算法

---

祝你在THUAI8西游真经劫比赛中取得好成绩！如有更多问题，欢迎在比赛交流群中讨论。 
```
