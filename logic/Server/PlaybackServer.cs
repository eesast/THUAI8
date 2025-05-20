using Grpc.Core;
using Microsoft.Extensions.Logging;
using Playback;
using Protobuf;
using System.Collections.Concurrent;
using Timothy.FrameRateTask;

namespace Server
{
    class PlaybackServer(ArgumentOptions options) : ServerBase
    {
        protected readonly ArgumentOptions options = options;
        private long[] teamScore = [];
        private readonly ConcurrentDictionary<long, (SemaphoreSlim, SemaphoreSlim)> semaDict = new();
        // private object semaDictLock = new();
        private MessageToClient? currentGameInfo = new();
        private MessageOfObj currentMapMsg = new();
        private const uint spectatorMinPlayerID = 2023;
        // private List<uint> spectatorList = new List<uint>();
        public int TeamCount => options.TeamCount;
        private readonly object spectatorJoinLock = new();
        protected object spectatorLock = new();
        protected bool isSpectatorJoin = false;
        protected bool IsSpectatorJoin
        {
            get
            {
                lock (spectatorLock)
                    return isSpectatorJoin;
            }

            set
            {
                lock (spectatorLock)
                    isSpectatorJoin = value;
            }
        }
        private bool IsGaming { get; set; } = true;
        private int[] finalScore = [];
        public int[] FinalScore
        {
            get
            {
                return finalScore;
            }
        }
        public override int[] GetMoney() => [];
        public override int[] GetScore() => FinalScore;

        public override async Task AddCharacter(CharacterMsg request,
                                             IServerStreamWriter<MessageToClient> responseStream,
                                             ServerCallContext context)
        {
            PlaybackServerLogging.logger.LogInformation($"AddPlayer: {request.CharacterId}");
            if (request.CharacterId >= spectatorMinPlayerID && options.NotAllowSpectator == false)
            {
                // 观战模式
                lock (spectatorJoinLock) // 具体原因见另一个上锁的地方
                {
                    if (semaDict.TryAdd(request.CharacterId, (new SemaphoreSlim(0, 1), new SemaphoreSlim(0, 1))))
                    {
                        PlaybackServerLogging.logger.LogInformation("A new spectator comes to watch this game");
                        IsSpectatorJoin = true;
                    }
                    else
                    {
                        PlaybackServerLogging.logger.LogInformation($"Duplicated Spectator ID {request.CharacterId}");
                        return;
                    }
                }
                do
                {
                    semaDict[request.CharacterId].Item1.Wait();
                    try
                    {
                        if (currentGameInfo != null)
                        {
                            await responseStream.WriteAsync(currentGameInfo);
                            PlaybackServerLogging.logger.LogInformation("Send!");
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        if (semaDict.TryRemove(request.CharacterId, out var semas))
                        {
                            try
                            {
                                semas.Item1.Release();
                                semas.Item2.Release();
                            }
                            catch { }
                            PlaybackServerLogging.logger.LogInformation($"The spectator {request.CharacterId} exited");
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        PlaybackServerLogging.logger.LogInformation(e.ToString());
                    }
                    finally
                    {
                        try
                        {
                            semaDict[request.CharacterId].Item2.Release();
                        }
                        catch { }
                    }
                } while (IsGaming);
                return;
            }
        }

        public void ReportGame(MessageToClient? msg)
        {
            currentGameInfo = msg;
            if (currentGameInfo != null && currentGameInfo.GameState == GameState.GameStart)
            {
                currentMapMsg = currentGameInfo.ObjMessage[0];
            }

            if (currentGameInfo != null && IsSpectatorJoin)
            {
                currentGameInfo.ObjMessage.Add(currentMapMsg);
                IsSpectatorJoin = false;
            }

            foreach (var kvp in semaDict)
            {
                kvp.Value.Item1.Release();
            }

            foreach (var kvp in semaDict)
            {
                kvp.Value.Item2.Wait();
            }
        }

        public override void WaitForEnd()
        {
            try
            {
                if (options.ResultOnly)
                {
                    using (MessageReader mr = new(options.FileName))
                    {
                        PlaybackServerLogging.logger.LogInformation("Parsing playback file...");
                        teamScore = new long[mr.teamCount];
                        finalScore = new int[mr.teamCount];
                        int infoNo = 0;
                        object cursorLock = new();
                        var initialTop = Console.CursorTop;
                        var initialLeft = Console.CursorLeft;
                        while (true)
                        {
                            MessageToClient? msg = null;
                            for (int i = 0; i < mr.teamCount; ++i)
                            {
                                for (int j = 0; j < mr.playerCount; ++j)
                                {
                                    msg = mr.ReadOne();
                                    if (msg == null)
                                    {
                                        PlaybackServerLogging.logger.LogInformation(
                                            "The game doesn't come to an end because of timing up!");
                                        IsGaming = false;
                                        goto endParse;
                                    }

                                    lock (cursorLock)
                                    {
                                        var curTop = Console.CursorTop;
                                        var curLeft = Console.CursorLeft;
                                        Console.SetCursorPosition(initialLeft, initialTop);
                                        PlaybackServerLogging.logger.LogInformation(
                                            $"Parsing messages... Current message number: {infoNo}");
                                        Console.SetCursorPosition(curLeft, curTop);
                                    }

                                    if (msg != null)
                                    {
                                        //teamScore[i] = msg.TeamScore;
                                    }
                                }
                            }

                            ++infoNo;

                            if (msg == null)
                            {
                                PlaybackServerLogging.logger.LogInformation("No game information in this file!");
                                goto endParse;
                            }
                            if (msg.GameState == GameState.GameEnd)
                            {
                                PlaybackServerLogging.logger.LogInformation("Game over normally!");
                                finalScore[0] = msg.AllMessage.BuddhistsTeamScore;
                                finalScore[1] = msg.AllMessage.MonstersTeamScore;
                                goto endParse;
                            }
                        }
                    endParse:
                        PlaybackServerLogging.logger.LogInformation($"Successfully parsed {infoNo} informations!");
                    }
                }
                else
                {
                    long timeInterval = GameServer.SendMessageToClientIntervalInMilliseconds;
                    if (options.PlaybackSpeed != 1.0)
                    {
                        options.PlaybackSpeed = Math.Max(0.25, Math.Min(4.0, options.PlaybackSpeed));
                        timeInterval = (int)Math.Round(timeInterval / options.PlaybackSpeed);
                    }
                    using MessageReader mr = new(options.FileName);
                    teamScore = new long[mr.teamCount];
                    finalScore = new int[mr.teamCount];
                    int infoNo = 0;
                    object cursorLock = new();
                    var msgCurTop = Console.CursorTop;
                    var msgCurLeft = Console.CursorLeft;
                    var frt = new FrameRateTaskExecutor<int>
                    (
                        loopCondition: () => true,
                        loopToDo: () =>
                        {
                            MessageToClient? msg = null;

                            msg = mr.ReadOne();
                            if (msg == null)
                            {
                                PlaybackServerLogging.logger.LogInformation(
                                    "The game doesn't come to an end because of timing up!");
                                IsGaming = false;
                                ReportGame(msg);
                                return false;
                            }
                            ReportGame(msg);
                            lock (cursorLock)
                            {
                                var curTop = Console.CursorTop;
                                var curLeft = Console.CursorLeft;
                                Console.SetCursorPosition(msgCurLeft, msgCurTop);
                                PlaybackServerLogging.logger.LogInformation(
                                    $"Sending messages... Current message number: {infoNo}");
                                Console.SetCursorPosition(curLeft, curTop);
                            }
                            if (msg != null)
                            {
                                foreach (var item in msg.ObjMessage)
                                {
                                    if (item.TeamMessage != null)
                                        teamScore[item.TeamMessage.TeamId] = item.TeamMessage.Score;

                                }
                            }

                            ++infoNo;
                            if (msg == null)
                            {
                                PlaybackServerLogging.logger.LogInformation("No game information in this file!");
                                IsGaming = false;
                                ReportGame(msg);
                                return false;
                            }
                            if (msg.GameState == GameState.GameEnd)
                            {
                                PlaybackServerLogging.logger.LogInformation("Game over normally!");
                                IsGaming = false;
                                finalScore[0] = msg.AllMessage.BuddhistsTeamScore;
                                finalScore[1] = msg.AllMessage.MonstersTeamScore;
                                ReportGame(msg);
                                return false;
                            }
                            return true;
                        },
                        timeInterval: timeInterval,
                        finallyReturn: () => 0
                    )
                    { AllowTimeExceed = true, MaxTolerantTimeExceedCount = 5 };
                    PlaybackServerLogging.logger.LogInformation("The server is well prepared!");
                    PlaybackServerLogging.logger.LogInformation(
                        "Please MAKE SURE that you have opened all the clients to watch the game!");
                    PlaybackServerLogging.logger.LogInformation(
                        "If ALL clients have opened, press any key to start");
                    Console.ReadKey();

                    new Thread
                        (
                            () =>
                            {
                                var rateCurTop = Console.CursorTop;
                                var rateCurLeft = Console.CursorLeft;
                                lock (cursorLock)
                                {
                                    rateCurTop = Console.CursorTop;
                                    rateCurLeft = Console.CursorLeft;
                                    PlaybackServerLogging.logger.LogInformation(
                                        $"Send message to clients frame rate: {frt.FrameRate}");
                                }
                                while (!frt.Finished)
                                {
                                    lock (cursorLock)
                                    {
                                        var curTop = Console.CursorTop;
                                        var curLeft = Console.CursorLeft;
                                        Console.SetCursorPosition(rateCurLeft, rateCurTop);
                                        PlaybackServerLogging.logger.LogInformation(
                                            $"Send message to clients frame rate: {frt.FrameRate}");
                                        Console.SetCursorPosition(curLeft, curTop);
                                    }
                                    Thread.Sleep(1000);
                                }
                            }
                        )
                    { IsBackground = true }.Start();

                    lock (cursorLock)
                    {
                        msgCurLeft = Console.CursorLeft;
                        msgCurTop = Console.CursorTop;
                        PlaybackServerLogging.logger.LogInformation("Sending messages...");
                    }
                    frt.Start();
                }
            }
            finally
            {
                teamScore ??= [];
            }
        }
    }
}