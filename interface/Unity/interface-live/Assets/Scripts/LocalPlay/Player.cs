using UnityEngine;
using System;
using Protobuf;
using System.Threading.Tasks;
using System.Threading;

#if !UNITY_WEBGL || UNITY_EDITOR
using Grpc.Core;
using Client = Protobuf.AvailableService.AvailableServiceClient;
#else
#pragma warning disable CS1998
using Client = WebGLClient;
#endif

class Players : SingletonMono<Players>
{
    public CharacterType[] buddhists, monsters;
    private CharacterInteract[] characterInteracts = new CharacterInteract[12];
    private bool gameStarted;
    private Client[] _clients;
    public Client GetClient(long ID)
    {
        if (0 <= ID && ID < 6) return _clients[ID + 1]; // Buddhists
        if (6 <= ID && ID < 12) return _clients[ID + 2]; // Monsters
        Debug.LogError($"Invalid client ID: {ID}");
        return null;
    }
    public Client GetMainClient(int teamId) => _clients[teamId * 7];

#if !UNITY_WEBGL || UNITY_EDITOR
    public async Task<Client> CreateClient(int teamId, int characterId, CharacterType characterType)
    {
        Channel channel = new("127.0.0.1:8888", ChannelCredentials.Insecure);
        await channel.ConnectAsync(DateTime.UtcNow.AddSeconds(30));
        var client = new Client(channel);
        var call = client.AddCharacter(new()
        {
            CharacterId = characterId,
            CharacterType = characterType,
            TeamId = teamId,
            SideFlag = teamId
        });
        Debug.Log($"Trying to join game as {characterType}...");
        while (await call.ResponseStream.MoveNext())
        {
            var currentGameInfo = call.ResponseStream.Current;
            if (currentGameInfo.GameState == GameState.GameStart) break;
        }
        return client;
    }
#else
    public Client CreateClient(int teamId, int characterId, CharacterType characterType)
    {
        Thread.Sleep(100000);
        return new(teamId, characterId, characterType);
    }
#endif

    public async void Start()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        Task<Client>[] clientTasks = new Task<Client>[14];
#else
        Client[] clientTasks = new Client[14];
#endif
        for (int teamId = 0; teamId < 2; teamId++)
        {
            long baseId = teamId * 7;
            {
                // 大本营和唐僧/九灵
                var type = teamId == 0 ? CharacterType.TangSeng : CharacterType.JiuLing;
                clientTasks[baseId] = CreateClient(teamId, 0, type);
                clientTasks[baseId + 1] = CreateClient(teamId, 1, type);
            }
            for (int characterId = 2; characterId <= 6; characterId++)
            {
                clientTasks[baseId + characterId] = CreateClient(
                    teamId, characterId, (teamId == 0 ? buddhists : monsters)[characterId - 2]);
            }
        }
#if !UNITY_WEBGL || UNITY_EDITOR
        _clients = await Task.WhenAll(clientTasks);
#else
        _clients = clientTasks;
#endif
    }

}