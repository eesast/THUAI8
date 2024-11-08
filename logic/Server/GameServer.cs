using GameClass.GameObj;
using GameClass.GameObj.Map;
using GameClass.MapGenerator;
using Gaming;
using Newtonsoft.Json;
using Playback;
using Preparation.Utility;
using Protobuf;
using System.Collections.Concurrent;
using Timothy.FrameRateTask;
using System.Net.Http.Json;
using System.Collections;

namespace Server
{
    public class ContestResult
    {
        public string status;
        public double[] scores;
    }
    partial class GameServer:ServerBase
    {
        private readonly ConcurrentDictionary<long, (SemaphoreSlim, SemaphoreSlim)> semaDict0 = new(); //for spectator and team0 player
        private readonly ConcurrentDictionary<long, (SemaphoreSlim, SemaphoreSlim)> semaDict1 = new();
        // private object semaDictLock = new();
        protected readonly ArgumentOptions options;
        private readonly HttpSender httpSender;
        private readonly object gameLock = new();
        private MessageToClient currentGameInfo = new();
        private readonly MessageOfObj currentMapMsg = new();
        private readonly object newsLock = new();
        private readonly List<MessageOfNews> currentNews = [];
        private readonly SemaphoreSlim endGameSem = new(0);
        protected readonly Game game;
        private readonly uint spectatorMinPlayerID = 2023;
        public int playerNum;
        public int TeamCount => options.TeamCount;
        protected long[][] communicationToGameID; // 通信用的ID映射到游戏内的ID，0指向队伍1，1指向队伍2，通信中0为大本营，1-5为船
        private readonly object messageToAllClientsLock = new();
        public static readonly long SendMessageToClientIntervalInMilliseconds = 50;
        private readonly MessageWriter? mwr = null;
        private readonly object spectatorJoinLock = new();

        public void StartGame()
        {
            if(game.GameMap.Timer.IsGaming) return;
            foreach(var team in communicationToGameID)
            {
                foreach(var id in team)
                {
                    if(if == GameObj.invalidID) return;//如果有未初始化的玩家，不开始游戏
                }
            }
            GameServerLogging.logger.ConsoleLog("Game starts!");
            CreatStartFiles();
            game.StarGame((int)options.GameTimeInSecond * 1000);
            Thread.Sleep(1);
            new Thread(() =>
            {
                bool flag = true;
                new  FramerateTaskExecutor<int>
                {
                    () => game.GameMap.Timer.IsGaming,
                    ()=>
                    {
                        if(flag)
                        {
                            ReportGame(GameState.Start);
                            falg = false;
                        }
                        else ReportGame(GameState.Running);
                    },
                    SendMessageToClientIntervalInMilliseconds,
                    () =>
                    {
                        ReportGame(GameState.GameEnd);  // 最后发一次消息，唤醒发消息的线程，防止发消息的线程由于有概率处在 Wait 状态而卡住
                        OnGameEnd();
                        return 0;
                    }
                }.Start();
            })
            {
                IsBackground = true
            }.Start();
        }
        
        public void CreateStartFile()
        {
            if (options.StartLockFile != DefaultArgumentOptions.FileName)
            {
                using var _ = File.Create(options.StartLockFile);
                GameServerLogging.logger.ConsoleLog("Successfully Created StartLockFile!");
            }
        }

        public override void WaitForEnd()
        {
            endGameSem.Wait();
            mwr?.Dispose();
        }

        private void SaveGameResult(string path)
        {
            Dictionary<string, int> result = [];
            int[] score = GetScore();
            result.Add("Disciple Team", score[0]);
            result.Add("Monsters Team", score[1]);
            JsonSerializer serializer = new();
            using StreamWriter sw = new(path);
            using JsonTextWriter writer = new(sw);
            serializer.Serialize(writer, result);

        }

        protected void SendGameResult(int[] scores, bool crashed = false)		// 天梯的 Server 给网站发消息记录比赛结果
        {
            string? url2 = Environment.GetEnvironmentVariable("FINISH_URL");
            if (url2 == null)
            {
                GameServerLogging.logger.ConsoleLog("Null FINISH_URL!");
                return;
            }
            else
            {
                httpSender.Url = url2;
                httpSender.Token = options.Token;
            }
            string state = crashed ? "Crashed" : "Finished";
            httpSender?.SendHttpRequest(scores, state).Wait();
        }

        protected double[] PullScore(double[] scores)
        {
            string? url2 = Environment.GetEnvironmentVariable("SCORE_URL");
            if (url2 != null)
            {
                httpSender.Url = url2;
                httpSender.Token = options.Token;
                double[] org = httpSender.GetLadderScore(scores).Result;
                if (org.Length == 0)
                {
                    GameServerLogging.logger.ConsoleLog("Error: No data returned from the web!");
                    return new double[0];
                }
                else
                {
                    double[] final = LadderCalculate(org, scores);
                    return final;
                }
            }
            else
            {
                GameServerLogging.logger.ConsoleLog("Null SCORE_URL Environment!");
                return new double[0];
            }
        }

        protected static double[] LadderCalculate(double[] oriScores, double[] competitionScores)
        {
            // 调整顺序，让第一项成为获胜者，便于计算
            bool scoresReverse = false; // 顺序是否需要交换
            if (competitionScores[0] < competitionScores[1])      // 第一项为落败者
                scoresReverse = true;
            else if (competitionScores[0] == competitionScores[1])// 平局
            {
                if (oriScores[0] == oriScores[1])
                // 完全平局，不改变天梯分数
                {
                    double[] Score = [0, 0];
                    return Score;
                }
                if (oriScores[0] > oriScores[1])
                    // 本次游戏平局，但一方天梯分数高，另一方天梯分数低，
                    // 需要将两者向中间略微靠拢，因此天梯分数低的定为获胜者
                    scoresReverse = true;
            }
            if (scoresReverse)// 如果需要换，交换两者的顺序
            {
                (competitionScores[0], competitionScores[1]) = (competitionScores[1], competitionScores[0]);
                (oriScores[0], oriScores[1]) = (oriScores[1], oriScores[0]);
            }

            const double normalDeltaThereshold = 100.0;                // 天梯分数差参数，天梯分差超过此阈值太多则增长缓慢
            const double correctParam = normalDeltaThereshold * 1.2;    // 修正参数
            const double winnerWeight = 4e-10;                           // 获胜者天梯得分权值
            const double loserWeight = 1.5e-10;                            // 落败者天梯得分权值
            const double scoreDeltaThereshold = 50000.0;                // 比赛得分参数，比赛得分超过此阈值太多则增长缓慢

            double[] resScore = [0, 0];
            double oriDelta = oriScores[0] - oriScores[1];                          // 天梯原分数差
            double competitionDelta = competitionScores[0] - competitionScores[1];  // 本次比赛分数差
            double normalOriDelta = oriDelta / normalDeltaThereshold;               // 标准化天梯原分数差
            double correctRate = oriDelta / correctParam;                           // 修正率，修正方向为缩小分数差
            double correct = 0.5 * (Math.Tanh((competitionDelta - scoreDeltaThereshold) / scoreDeltaThereshold
                                              - correctRate)
                                    + 1.0); // 分数修正
            resScore[0] = Math.Min(300, Math.Round(Math.Pow(competitionScores[0], 2)
                                                    * winnerWeight
                                                    * (1 - Math.Tanh(normalOriDelta))
                                                    * correct)); // 胜者所加天梯分)
            resScore[1] = Math.Max(-120, -Math.Round(Math.Pow(competitionDelta, 2)
                                                    * loserWeight
                                                    * (1 - Math.Tanh(normalOriDelta))
                                                    * correct)); // 败者所扣天梯分
            if (scoresReverse)// 顺序换回
                (resScore[0], resScore[1]) = (resScore[1], resScore[0]);
            return resScore;
        }

        private void OnGameEnd()
        {
            game.ClearAllLists();
            mwr?.Flush();
            if (options.ResultFileName != DefaultArgumentOptions.FileName)
                SaveGameResult(options.ResultFileName.EndsWith(".json")
                             ? options.ResultFileName
                             : options.ResultFileName + ".json");
            int[] scores = GetScore();
            double[] doubleArray = scores.Select(x => (double)x).ToArray();
            if (options.Mode == 2)
            {
                bool crash = false;
                doubleArray = PullScore(doubleArray);
                if (doubleArray.Length == 0)
                {
                    crash = true;
                    GameServerLogging.logger.ConsoleLog("Error: No data returned from the web!");
                }
                else
                    scores = doubleArray.Select(x => (int)x).ToArray();
                SendGameResult(scores, crash);
            }
            else if (options.Mode == 1)
            {
                int[] s = new int[2];
                if (scores[1] > scores[0])
                    s = [0, 2];
                else if (scores[1] == scores[0])
                    s = [1, 1];
                else
                    s = [2, 0];
                SendGameResult(s);
            }
            endGameSem.Release();
        }

        public void ReportGame(GameState gameState, bool requiredGaming = true)
        {
            var gameObjList = game.GetGameObj();
            currentGameInfo = new();
            lock (messageToAllClientsLock)
            {
                switch (gameState)
                {
                    case GameState.GameRunning:
                    case GameState.GameEnd:
                    case GameState.GameStart:
                        if (gameState == GameState.GameStart || IsSpectatorJoin)
                        {
                            currentGameInfo.ObjMessage.Add(currentMapMsg);
                            IsSpectatorJoin = false;
                        }
                        long time = Environment.TickCount64;
                        foreach (GameObj gameObj in gameObjList.Cast<GameObj>())
                        {
                            MessageOfObj? msg = CopyInfo.Auto(gameObj, time);
                            if (msg != null) currentGameInfo.ObjMessage.Add(msg);
                        }
                        foreach (Base team in game.TeamList)
                        {
                            MessageOfObj? msg = CopyInfo.Auto(team, time);
                            if (msg != null) currentGameInfo.ObjMessage.Add(msg);
                        }
                        lock (newsLock)
                        {
                            foreach (var news in currentNews)
                            {
                                MessageOfObj? msg = CopyInfo.Auto(news);
                                if (msg != null) currentGameInfo.ObjMessage.Add(msg);
                            }
                            currentNews.Clear();
                        }
                        currentGameInfo.GameState = gameState;
                        currentGameInfo.AllMessage = GetMessageOfAll(game.GameMap.Timer.NowTime());
                        mwr?.WriteOne(currentGameInfo);
                        break;
                    default:
                        break;
                }
            }
            lock (spectatorJoinLock)
            {
                foreach (var kvp in semaDict0)
                {
                    kvp.Value.Item1.Release();
                }
                foreach (var kvp in semaDict1)
                {
                    kvp.Value.Item1.Release();
                }
                // 若此时观战者加入，则死锁，所以需要 spectatorJoinLock

                foreach (var kvp in semaDict0)
                {
                    kvp.Value.Item2.Wait();
                }
                foreach (var kvp in semaDict1)
                {
                    kvp.Value.Item2.Wait();
                }
            }
        }

        private bool PlayerDeceased(int playerID)    //# 这里需要判断大本营deceased吗？
        {
            return game.GameMap.GameObjDict[GameObjType.Character].Cast<Character>()?.Find(
                character => character.PlayerID == playerID && character.CharacterState == CharacterStateType.Deceased
                ) != null;
        }

        public override int[] GetMoney()
        {
            int[] money = new int[2]; // 0代表RedTeam，1代表BlueTeam
            foreach (Base team in game.TeamList)
            {
                money[(int)team.TeamID] = (int)game.GetTeamMoney(team.TeamID);
            }
            return money;
        }

        public override int[] GetScore()
        {
            int[] score = new int[2]; // 0代表RedTeam，1代表BlueTeam
            foreach (Base team in game.TeamList)
            {
                score[(int)team.TeamID] = (int)game.GetTeamScore(team.TeamID);
            }
            return score;
        }

        private uint GetBirthPointIdx(long playerID)  // 获取出生点位置
        {
            return (uint)playerID + 1; // ID从0-8,出生点从1-9
        }

        private bool ValidPlayerID(long playerID)
        {
            if (playerID == 0 || (1 <= playerID && playerID <= options.ShipCount))
                return true;
            return false;
        }

        private MessageOfAll GetMessageOfAll(int time)
        {
            MessageOfAll msg = new()
            {
                GameTime = time;
            };
            int[] score = GetScore();
            msg.buddhists_team_score = score[0];
            msg.monsters_team_score = score[1];
            int[] economy = GetMoney();
            msg.buddhists_team_money = economy[0];
            msg.monsters_team_money = economy[1];
            return msg;
        }

        private MessageOfMap MapMsg()
        {
            MessageOfMap msgOfMap = new()
            {
                Height = game.GameMap.Height,
                Width = game.GameMap.Width
            };
            for(int i = 0; i < game.GameMap.Height; i++)
            {
                msgOfMap.Rows.Add(new MessageOfMap.Types.Rows());
                for(int j = 0; j<game.GameMap.Width; j++)
                {
                    msgOfMap.Rows[i].Cols.Add(Transformation.PlaceTypeToProto(game.GameMap.Map[i, j]));
                }
            }
            return msgOfMap;
        }

        public GameServer(ArgumentOptions options)
        {
            this.options = options;
            if(options.MapResource == DefaultArgumentOptions.MapResource)
                game = new(MapInfo.defaultMapStruct, options.TeamCount);
            else
            {
                if(options.MapResource.EndsWith(".txt"))
                {
                    try
                    {
                        uint[,] map = new uint[GameData.MapRows,GameData.MapCols];
                        string? lines;
                        int i =0.j=0;
                        using StreamReader sr = new(options.MapResource);
                        #region 读取地图
                        while(!sr.EndOfStream && i<GameData.MapRows)
                        {
                            if((line = sr.RewadLine()) != null)
                            {
                                string[] nums = lines.Split(' ');
                                foreach(string item nums)
                                {
                                    if(items.Length>1)
                                    map[i,j] = (uint)int.Parse(item);
                                    else
                                        map[i,j] = (unint)MapEncoder.Hex2Dec(char.Parse(item));
                                    j++;
                                    if(j>=GameData.MapCols)
                                    {
                                        j = 0;
                                        break;
                                    }
                                }
                                i++;
                            }
                        }
                        #endregion
                        game = new(new(GameData.MapRows, GameData.MapCols, map), options.TeamCount);
                    }
                    catch
                    {
                        game = new(MapInfo.defaultMapStruct, options.TeamCount);
                    }
                }
                else if(options.MapResource.EndWith(".map"))
                {
                    try
                    {
                        game = new(MapStruct.FromFile(options.MapResource), options.TeamCount);
                    }
                    catch
                    {
                        game = new(MapInfo.defaultMapStruct, options.TeamCount);
                    }
                }
                else
                {
                    game = new(MapInfo.defaultMapStruct, options.TeamCount);
                }
            }
            currentMapMsg = new() { MapMessage = MapMsg() };
            playerNum = options.CharacterCount;
            communicationToGameID = new long[TeamCount][];
            for(int i = 0; i < TeamCount; i++)
            {
                communicationToGameID[i] = new long[options.CharacterCount + 1];
            }

            for(int team = 0; team < TeamCount; team++)
            {
                communicationToGameID[team][0] = GameObj.invalidID;
                for(int i = 1; i <= options.CharacterCount; i++)
                {
                    communicationToGameID[team][i] = GameObj.invalidID;
                }
            }

            if(options.FileName != DefaultArgumentOptions.FileName)
            {
                try
                {
                    mwr = new(options.FileName, options.TeamCount, options.CharacterCount);
                }
                catch
                {
                    GameServerLogging.logger.Consolelog($"Error:Cannot create the playback file: {options.FileName}! ");
                }
            }
            string? token2 = Environment.GetEnvironmentVariable("TOKEN");
            if(token2 == null)
            {
                GameServerLogging.logger.ConsoleLog("Null TOKEN Environment!");
            }
            else
                options.Token = token2;
            if(options.Url != DefaultArgumentOptions.Url && options.Token != Token)
            {
                httpSender = new(options.Url, options.Token);
            }
            else
            {
                httpSender = new(DefaultArgumentOptions.Url, DefaultArgumentOptions.Token);
            }
        }
    }
}