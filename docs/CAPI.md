# 接口一览
```
    //通用接口
    //发送信息、接受信息，注意收消息是无消息则返回-1和空“string”
    virtual std::future<bool> SendTextMessage(int32_t toPlayerID, std::string) = 0;
    virtual std::future<bool> SendBinaryMessage(int32_t toPlayerID, std::string) = 0;
    [[nodiscard]] virtual bool HaveMessage() = 0;
    [[nodiscard]] virtual std::pair<int32_t, std::string> GetMessage() = 0;

    //获取游戏目前所进行的帧数
    [[nodiscard]] virtual int32_t GetFrameCount() const = 0;

    //等待下一帧
    virtual bool Wait() = 0;
    virtual std::future<bool> EndAllAction() = 0;
    [[nodiscard]] virtual std::vector<std::shared_ptr<const THUAI8::Character>> GetCharacters() const = 0;
    [[nodiscard]] virtual std::vector<std::shared_ptr<const THUAI8::Character>> GetEnemyCharacters() const = 0;
    [[nodiscard]] virtual std::vector<std::vector<THUAI8::PlaceType>> GetFullMap() const = 0;
    [[nodiscard]] virtual std::shared_ptr<const THUAI8::GameInfo> GetGameInfo() const = 0;
    [[nodiscard]] virtual THUAI8::PlaceType GetPlaceType(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::EconomyResourceState> GetEnconomyResourceState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::AdditionResourceState> GetAdditionResourceState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::optional<THUAI8::ConstructionState> GetConstructionState(int32_t cellX, int32_t cellY) const = 0;
    [[nodiscard]] virtual std::vector<int64_t> GetPlayerGUIDs() const = 0;
    [[nodiscard]] virtual int32_t GetEnergy() const = 0;
    [[nodiscard]] virtual int32_t GetScore() const = 0;

    //控制角色进行移动
    virtual std::future<bool> Move(int64_t timeInMilliseconds, double angleInRadian) = 0;
    //向特定方向移动
    virtual std::future<bool> MoveRight(int64_t timeInMilliseconds) = 0;
    virtual std::future<bool> MoveUp(int64_t timeInMilliseconds) = 0;
    virtual std::future<bool> MoveLeft(int64_t timeInMilliseconds) = 0;
    virtual std::future<bool> MoveDown(int64_t timeInMilliseconds) = 0;
    virtual std::future<bool> Skill_Attack(double angleInRadian) = 0;
    virtual std::future<bool> Common_Attack(int64_t attackedPlayerID) = 0;
    virtual std::future<bool> Recover(int64_t recover) = 0;
    virtual std::future<bool> Harvest() = 0;
    virtual std::future<bool> Rebuild(THUAI8::ConstructionType constructionType) = 0;
    virtual std::future<bool> Construct(THUAI8::ConstructionType constructionType) = 0;
    virtual std::shared_ptr<const THUAI8::Character> GetSelfInfo() const = 0;
    virtual bool HaveView(int32_t targetX, int32_t targetY) const = 0;

    [[nodiscard]] virtual std::shared_ptr<const THUAI8::Team> GetSelfInfo() const = 0;
    virtual std::future<bool> InstallEquipment(int32_t playerID, THUAI8::EquipmentType equipmenttype) = 0;
    virtual std::future<bool> Recycle(int32_t playerID) = 0;
    virtual std::future<bool> BuildCharacter(THUAI8::CharacterType CharacterType, int32_t birthIndex) = 0;

     // 获取指定格子中心的坐标
    [[nodiscard]] static inline int32_t CellToGrid(int32_t cell) noexcept
    {
        return cell * numOfGridPerCell + numOfGridPerCell / 2;
    }

    // 获取指定坐标点所位于的格子的 X 序号
    [[nodiscard]] static inline int32_t GridToCell(int32_t grid) noexcept
    {
        return grid / numOfGridPerCell;
    }

    // 用于DEBUG的输出函数，选手仅在开启Debug模式的情况下可以使用

    virtual void Print(std::string str) const = 0;
    virtual void PrintCharacter() const = 0;
    virtual void PrintTeam() const = 0;
    virtual void PrintSelfInfo() const = 0;
