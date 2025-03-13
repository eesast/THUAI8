using GameClass.GameObj;
using Gaming;
using Grpc.Core;
using Preparation.Utility;
using Utility = Preparation.Utility;
using Protobuf;

namespace Server
{
    partial class GameServer : ServerBase
    {
        private int playerCountNow = 0;
        protected object spectatorLock = new();
        protected bool isSpectatorJoin = false;
        protected bool IsSpectatorJoin
        {
            get
            {
                lock (spectatorLock) return isSpectatorJoin;
            }

            set
            {
                lock (spectatorLock)
                    isSpectatorJoin = value;
            }
        }
        public override Task<BoolRes> TryConnection(IDMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY TryConnection: Player {request.PlayerId} from Team {request.TeamId}");
            var onConnection = new BoolRes();
            lock (gameLock)
            {
                if (0 <= request.PlayerId && request.PlayerId < playerNum)
                {
                    onConnection.ActSuccess = true;
                    GameServerLogging.logger.ConsoleLog($"TryConnection: {onConnection.ActSuccess}");
                    return Task.FromResult(onConnection);
                }
            }
            onConnection.ActSuccess = false;
            GameServerLogging.logger.ConsoleLogDebug("END TryConnection");
            return Task.FromResult(onConnection);
        }

        #region 游戏开局调用一次的服务

        protected readonly object addPlayerLock = new();
        public override async Task AddPlayer(PlayerMsg request, IServerStreamWriter<MessageToClient> responseStream, ServerCallContext context)
        {
#if !DEBUG
            GameServerLogging.logger.ConsoleLog($"AddPlayer: Player {request.PlayerId} from Team {request.TeamId}");
#endif
            if (request.PlayerId >= spectatorMinPlayerID && options.NotAllowSpectator == false)
            {
                GameServerLogging.logger.ConsoleLogDebug($"TRY Add Spectator: Player {request.PlayerId}");
                // 观战模式
                lock (spectatorJoinLock)  // 具体原因见另一个上锁的地方
                {
                    if (semaDict0.TryAdd(request.PlayerId, (new SemaphoreSlim(0, 1), new SemaphoreSlim(0, 1))))
                    {
                        GameServerLogging.logger.ConsoleLog("A new spectator comes to watch this game");
                        IsSpectatorJoin = true;
                    }
                    else
                    {
                        GameServerLogging.logger.ConsoleLog($"Duplicated Spectator ID {request.PlayerId}");
                        return;
                    }
                }
                do
                {
                    semaDict0[request.PlayerId].Item1.Wait();
                    try
                    {
                        if (currentGameInfo != null)
                        {
                            var info = currentGameInfo.Clone();
                            for (int i = info.ObjMessage.Count - 1; i >= 0; i--)
                            {
                                if (info.ObjMessage[i].NewsMessage != null)
                                {
                                    info.ObjMessage.RemoveAt(i);
                                }
                            }
                            await responseStream.WriteAsync(info);
                            GameServerLogging.logger.ConsoleLog("Send!", false);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        if (semaDict0.TryRemove(request.PlayerId, out var semas))
                        {
                            try
                            {
                                semas.Item1.Release();
                                semas.Item2.Release();
                            }
                            catch
                            {
                            }
                            GameServerLogging.logger.ConsoleLog($"The spectator {request.PlayerId} exited");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        GameServerLogging.logger.ConsoleLog(ex.ToString());
                    }
                    finally
                    {
                        try
                        {
                            semaDict0[request.PlayerId].Item2.Release();
                        }
                        catch
                        {
                        }
                    }
                } while (game.GameMap.Timer.IsGaming);
                GameServerLogging.logger.ConsoleLogDebug("END Add Spectator");
                return;
            }
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Add Player: Player {request.PlayerId} from Team {request.TeamId}");
            if (game.GameMap.Timer.IsGaming)
                return;
            if (!ValidPlayerID(request.PlayerId))  //玩家id是否正确
                return;
            if (request.TeamId >= TeamCount)  //队伍只能是0，1
                return;
            if (communicationToGameID[request.TeamId][request.PlayerId] != GameObj.invalidID)  //是否已经添加了该玩家
                return;
            GameServerLogging.logger.ConsoleLogDebug("AddPlayer: Check Correct");
            lock (addPlayerLock)
            {
                Game.PlayerInitInfo playerInitInfo = new(request.TeamId, request.PlayerId, Transformation.CharacterTypeFromProto(request.CharacterType));
                long newPlayerID = game.AddPlayer(playerInitInfo);
                if (newPlayerID == GameObj.invalidID)
                {
                    GameServerLogging.logger.ConsoleLogDebug("FAIL AddPlayer");
                    return;
                }
                communicationToGameID[request.TeamId][request.PlayerId] = newPlayerID;
                var temp = (new SemaphoreSlim(0, 1), new SemaphoreSlim(0, 1));
                bool start = false;
                GameServerLogging.logger.ConsoleLog($"Player {request.PlayerId} from Team {request.TeamId} joins");
                lock (spectatorJoinLock)  // 为了保证绝对安全，还是加上这个锁吧
                {
                    if (request.TeamId == 0)
                    {
                        if (semaDict0.TryAdd(request.PlayerId, temp))
                        {
                            start = Interlocked.Increment(ref playerCountNow) == (playerNum * TeamCount);
                            GameServerLogging.logger.ConsoleLog($"PlayerCountNow: {playerCountNow}");
                            GameServerLogging.logger.ConsoleLog($"PlayerTotalNum: {playerNum * TeamCount}");
                        }
                    }
                    else if (request.TeamId == 1)
                    {
                        if (semaDict1.TryAdd(request.PlayerId, temp))
                        {
                            start = Interlocked.Increment(ref playerCountNow) == (playerNum * TeamCount);
                            GameServerLogging.logger.ConsoleLog($"PlayerCountNow: {playerCountNow}");
                            GameServerLogging.logger.ConsoleLog($"PlayerNum: {playerNum * TeamCount}");
                        }
                    }
                }
                if (start)
                {
                    GameServerLogging.logger.ConsoleLogDebug("Game Start");
                    StartGame();
                }
            }
            bool exitFlag = false;
            bool firstTime = true;
            do
            {
                if (request.TeamId == 0)
                    semaDict0[request.PlayerId].Item1.Wait();
                else if (request.TeamId == 1)
                    semaDict1[request.PlayerId].Item1.Wait();
                Character? character = game.GameMap.FindCharacterInPlayerID(request.TeamId, request.PlayerId);
                // if(character!=null)
                // {
                //     GameServerLogging.logger.ConsoleLog($"Character {request.PlayerId} exist! IsRemoved {character.IsRemoved}");
                // }
                // else{
                //     GameServerLogging.logger.ConsoleLog($"Character {request.PlayerId} null");
                // }
                if (!firstTime && request.PlayerId > 0 && (character == null || character.IsRemoved == true))
                {
                    // GameServerLogging.logger.ConsoleLog($"Cannot find character {request.PlayerId} from Team {request.TeamId}!");
                }
                else
                {
                    if (firstTime)
                        firstTime = false;
                    try
                    {
                        if (currentGameInfo != null && !exitFlag)
                        {
                            await responseStream.WriteAsync(currentGameInfo);
                            GameServerLogging.logger.ConsoleLog(
                                $"Send to Player{request.PlayerId} from Team {request.TeamId}!",
                                false);
                        }
                    }
                    catch
                    {
                        if (!exitFlag)
                        {
                            GameServerLogging.logger.ConsoleLog($"The client {request.PlayerId} exited");
                            exitFlag = true;
                        }
                    }
                }
                (request.TeamId == 0 ? semaDict0 : semaDict1)[request.PlayerId].Item2.Release();
            } while (game.GameMap.Timer.IsGaming);
        }

        public override Task<MessageOfMap> GetMap(NullRequest request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug($"GetMap: IP {context.Peer}");
            return Task.FromResult(MapMsg());
        }

        #endregion

        #region 游戏过程中玩家执行操作的服务

        #region 普通角色操作

        /*public override Task<BoolRes> Activate(ActivateMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug($"TRY Activate: Player {request.PlayerId} from Team {request.TeamId}");
            BoolRes boolRes = new();
            if (request.PlayerId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            // var gameID = communicationToGameID[request.TeamId][request.PlayerId];
            boolRes.ActSuccess = game.ActivateCharacter(request.TeamId, Transformation.CharacterTyprFromProto(request.CharacterType));
            if (!game.GameMap.Timer.IsGaming) boolRes.ActSuccess = false;
            GameServerLogging.logger.ConsoleLogDebug($"END Activate: {boolRes.ActSuccess}");
            return Task.FromResult(boolRes);
        }*/

        public override Task<MoveRes> Move(MoveMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Move: Player {request.PlayerId} from Team {request.TeamId}, " +
                $"TimeInMilliseconds: {request.TimeInMilliseconds}");
            MoveRes moveRes = new();
            if (request.PlayerId >= spectatorMinPlayerID)
            {
                moveRes.ActSuccess = false;
                return Task.FromResult(moveRes);
            }
            if (double.IsNaN(request.Angle))
            {
                moveRes.ActSuccess = false;
                return Task.FromResult(moveRes);
            }
            // var gameID = communicationToGameID[request.TeamId][request.PlayerId];
            moveRes.ActSuccess = game.MoveCharacter(
                request.TeamId, request.PlayerId,
                (int)request.TimeInMilliseconds, request.Angle);
            if (!game.GameMap.Timer.IsGaming)
                moveRes.ActSuccess = false;
            GameServerLogging.logger.ConsoleLogDebug($"END Move: {moveRes.ActSuccess}");
            return Task.FromResult(moveRes);
        }

        public override Task<BoolRes> Recover(RecoverMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Recover: Player {request.PlayerId} from Team {request.TeamId}");
            BoolRes boolRes = new();
            if (request.PlayerId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            // var gameID = communicationToGameID[request.TeamId][request.PlayerId];
            boolRes.ActSuccess = game.Recover(request.TeamId, request.PlayerId, request.Recover);
            GameServerLogging.logger.ConsoleLogDebug("END Recover");
            return Task.FromResult(boolRes);
        }

        public override Task<BoolRes> Harvest(IDMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Harvest: Player {request.PlayerId} from Team {request.TeamId}");
            BoolRes boolRes = new();
            if (request.PlayerId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            // var gameID = communicationToGameID[request.TeamId][request.PlayerId];
            boolRes.ActSuccess = game.Harvest(
                request.TeamId, request.PlayerId,
                Transformation.EcnomyResourceFromProto(request.EconomyResourceType));
            GameServerLogging.logger.ConsoleLogDebug("END Harvest");
            return Task.FromResult(boolRes);
        }

        public override Task<BoolRes> Rebuild(ConstructMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Rebuild: Player {request.PlayerId} from Team {request.TeamId}");
            BoolRes boolRes = new();
            if (request.PlayerId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            // var gameID = communicationToGameID[request.TeamId][request.PlayerId];
            boolRes.ActSuccess = game.Construct(
                request.TeamId, request.PlayerId,
                Transformation.ConstructionFromProto(request.ConstructionType));
            GameServerLogging.logger.ConsoleLogDebug("END Rebuild");
            return Task.FromResult(boolRes);
        }

        public override Task<BoolRes> Construct(ConstructMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Construct: Player {request.PlayerId} from Team {request.TeamId}");
            BoolRes boolRes = new();
            if (request.PlayerId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            // var gameID = communicationToGameID[request.TeamId][request.PlayerId];
            boolRes.ActSuccess = game.Construct(
                request.TeamId, request.PlayerId,
                Transformation.ConstructionFromProto(request.ConstructionType));
            GameServerLogging.logger.ConsoleLogDebug("END Construct");
            return Task.FromResult(boolRes);
        }

        public override Task<BoolRes> Equip(EquipMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Construct: Player {request.PlayerId} from Team {request.TeamId}");
            BoolRes boolRes = new();
            if (request.PlayerId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            // var gameID = communicationToGameID[request.TeamId][request.PlayerId];
            boolRes.ActSuccess = game.Equip(
                request.TeamId, request.PlayerId,
                Transformation.EquipmentFromProto(request.EquipmentType));
            GameServerLogging.logger.ConsoleLogDebug("END Equip");
            return Task.FromResult(boolRes);
        }

        public override Task<BoolRes> Attack(AttackMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Attack: Player {request.CharacterId} from Team {request.TeamId} attacking Player {request.AttackedCharacterId}");
            BoolRes boolRes = new();
            if (request.CharacterId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            if (request.AttackedCharacterId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            if (request.AttackRange <= 0)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            // var gameID = communicationToGameID[request.TeamId][request.PlayerId];
            boolRes.ActSuccess = game.Attack(
                request.TeamId, request.CharacterId,
                request.AttackRange, request.AttackedCharacterId);
            GameServerLogging.logger.ConsoleLogDebug("END Attack");
            return Task.FromResult(boolRes);
        }

        public override Task<BoolRes> Cast(CastMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Cast: Player {request.CharacterId} from Team {request.TeamId} casting Skill {request.SkillId}");
            BoolRes boolRes = new();
            if (request.CharacterId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            if (request.SkillId <= 0)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            if (request.AttackRange <= 0)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            boolRes.ActSuccess = game.CastSkill(
                request.TeamId, request.CharacterId, request.SkillId,
                request.CastedCharacterId.ToList(), request.AttackRange,
                request.X, request.Y, request.Angle);
            GameServerLogging.logger.ConsoleLogDebug("END Cast");
            return Task.FromResult(boolRes);
        }

        public override Task<BoolRes> Send(SendMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY Send: From Player {request.PlayerId} To Player {request.ToPlayerId} from Team {request.TeamId}");
            var boolRes = new BoolRes();
            if (request.PlayerId >= spectatorMinPlayerID || PlayerDeceased((int)request.PlayerId))
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            if (!ValidPlayerID(request.PlayerId)
                || !ValidPlayerID(request.ToPlayerId)
                || request.PlayerId == request.ToPlayerId)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            GameServerLogging.logger.ConsoleLogDebug($"Send: As {request.MessageCase}");
            switch (request.MessageCase)
            {
                case SendMsg.MessageOneofCase.TextMessage:
                    {
                        if (request.TextMessage.Length > 256)
                        {
                            GameServerLogging.logger.ConsoleLogDebug("Send: Text message string is too long!");
                            boolRes.ActSuccess = false;
                            return Task.FromResult(boolRes);
                        }
                        MessageOfNews news = new()
                        {
                            TextMessage = request.TextMessage,
                            FromId = request.PlayerId,
                            ToId = request.ToPlayerId,
                            TeamId = request.TeamId
                        };
                        lock (newsLock)
                        {
                            currentNews.Add(news);
                        }
                        GameServerLogging.logger.ConsoleLogDebug("Send: Text: " + news.TextMessage);
                        boolRes.ActSuccess = true;
                        GameServerLogging.logger.ConsoleLogDebug($"END Send");
                        return Task.FromResult(boolRes);
                    }
                case SendMsg.MessageOneofCase.BinaryMessage:
                    {
                        if (request.BinaryMessage.Length > 256)
                        {
                            GameServerLogging.logger.ConsoleLogDebug("Send: Binary message string is too long!");
                            boolRes.ActSuccess = false;
                            return Task.FromResult(boolRes);
                        }
                        MessageOfNews news = new()
                        {
                            BinaryMessage = request.BinaryMessage,
                            FromId = request.PlayerId,
                            ToId = request.ToPlayerId,
                            TeamId = request.TeamId
                        };
                        lock (newsLock)
                        {
                            currentNews.Add(news);
                        }
                        GameServerLogging.logger.ConsoleLogDebug($"BinaryMessageLength: {news.BinaryMessage.Length}");
                        boolRes.ActSuccess = true;
                        GameServerLogging.logger.ConsoleLogDebug($"END Send");
                        return Task.FromResult(boolRes);
                    }
                default:
                    {
                        boolRes.ActSuccess = false;
                        return Task.FromResult(boolRes);
                    }
            }
        }

        #endregion

        #region 核心角色操作

        public override Task<BoolRes> CreatCharacter(CreatCharacterMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY CreatCharacter: CharacterType {request.CharacterType} from Team {request.TeamId}");
            var activateCost = Transformation.CharacterTypeFromProto(request.CharacterType) switch
            {
                Utility.CharacterType.TangSeng => GameData.TangSengcost,
                Utility.CharacterType.SunWukong => GameData.SunWukongcost,
                Utility.CharacterType.ZhuBajie => GameData.ZhuBajiecost,
                Utility.CharacterType.ShaWujing => GameData.ShaWujingcost,
                Utility.CharacterType.BaiLongma => GameData.BaiLongmacost,
                Utility.CharacterType.Monkid => GameData.Monkidcost,

                Utility.CharacterType.JiuLing => GameData.JiuLingcost,
                Utility.CharacterType.HongHaier => GameData.HongHaiercost,
                Utility.CharacterType.NiuMowang => GameData.NiuMowangcost,
                Utility.CharacterType.TieShan => GameData.TieShancost,
                Utility.CharacterType.ZhiZhujing => GameData.ZhiZhujingcost,
                Utility.CharacterType.Pawn => GameData.Pawncost,

                _ => int.MaxValue
            };
            var teamMoneyPool = game.TeamList[(int)request.TeamId].MoneyPool;
            if (activateCost > teamMoneyPool.Money)
            {
                return Task.FromResult(new BoolRes { ActSuccess = false });
            }
            if (game.TeamList[(int)request.TeamId].Hero.HP <= 0)
            {
                return Task.FromResult(new BoolRes { ActSuccess = false });
            }
            BoolRes boolRes = new()
            {
                ActSuccess =
                    game.ActivateCharacter(
                        request.TeamId,
                        Transformation.CharacterTypeFromProto(request.CharacterType),
                        request.BirthpointIndex)
                    != GameObj.invalidID
            };
            if (boolRes.ActSuccess) teamMoneyPool.SubMoney(activateCost);
            GameServerLogging.logger.ConsoleLogDebug("END CreatCharacter");
            return Task.FromResult(boolRes);
        }

        public override Task<CreatCharacterRes> CreatCharacterRID(CreatCharacterMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY CreatCharacter: CharacterType {request.CharacterType} from Team {request.TeamId}");
            var activateCost = Transformation.CharacterTypeFromProto(request.CharacterType) switch
            {
                Utility.CharacterType.TangSeng => GameData.TangSengcost,
                Utility.CharacterType.SunWukong => GameData.SunWukongcost,
                Utility.CharacterType.ZhuBajie => GameData.ZhuBajiecost,
                Utility.CharacterType.ShaWujing => GameData.ShaWujingcost,
                Utility.CharacterType.BaiLongma => GameData.BaiLongmacost,
                Utility.CharacterType.Monkid => GameData.Monkidcost,

                Utility.CharacterType.JiuLing => GameData.JiuLingcost,
                Utility.CharacterType.HongHaier => GameData.HongHaiercost,
                Utility.CharacterType.NiuMowang => GameData.NiuMowangcost,
                Utility.CharacterType.TieShan => GameData.TieShancost,
                Utility.CharacterType.ZhiZhujing => GameData.ZhiZhujingcost,
                Utility.CharacterType.Pawn => GameData.Pawncost,

                _ => int.MaxValue
            };
            var teamMoneyPool = game.TeamList[(int)request.TeamId].MoneyPool;
            if (activateCost > teamMoneyPool.Money)
            {
                return Task.FromResult(new CreatCharacterRes { ActSuccess = false });
            }
            if (game.TeamList[(int)request.TeamId].Hero.HP <= 0)
            {
                return Task.FromResult(new CreatCharacterRes { ActSuccess = false });
            }
            var playerId = game.ActivateCharacter(
                request.TeamId,
                Transformation.CharacterTypeFromProto(request.CharacterType),
                request.BirthpointIndex);

            CreatCharacterRes creatCharacterRes = new()
            {
                ActSuccess = playerId != GameObj.invalidID,
                PlayerId = playerId
            };
            if (creatCharacterRes.ActSuccess) teamMoneyPool.SubMoney(activateCost);
            GameServerLogging.logger.ConsoleLogDebug("END CreatCharacterRID");
            return Task.FromResult(creatCharacterRes);
        }

        public override Task<BoolRes> EndAllAction(IDMsg request, ServerCallContext context)
        {
            GameServerLogging.logger.ConsoleLogDebug(
                $"TRY EndAllAction: Player {request.PlayerId} from Team {request.TeamId}");
            BoolRes boolRes = new();
            if (request.PlayerId >= spectatorMinPlayerID)
            {
                boolRes.ActSuccess = false;
                return Task.FromResult(boolRes);
            }
            // var gameID = communicationToGameID[request.TeamId][request.PlayerId];
            boolRes.ActSuccess = game.Stop(request.TeamId, request.PlayerId);
            GameServerLogging.logger.ConsoleLogDebug("END EndAllAction");
            return Task.FromResult(boolRes);
        }

        #endregion

        #endregion
    }
}
