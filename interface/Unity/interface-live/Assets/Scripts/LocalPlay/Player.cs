using UnityEngine;
using System;
using Protobuf;
using System.Threading.Tasks;
using System.Collections.Generic;

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

#if !UNITY_WEBGL || UNITY_EDITOR

    public async Task<Client> CreateClient(int teamId, int characterId, CharacterType characterType)
    {
        Channel channel = new("127.0.0.1:8888", ChannelCredentials.Insecure);
        await channel.ConnectAsync(DateTime.UtcNow.AddSeconds(30));
        var client = new Client(channel);
        CharacterMsg playerInfo = new()
        {
            CharacterId = characterId,
            CharacterType = characterType,
            TeamId = teamId,
            SideFlag = teamId
        };
        var call = client.AddCharacter(playerInfo);
        Debug.Log($"Trying to join game as {(teamId == 0 ? "buddhists" : "monsters")}...");
        while (await call.ResponseStream.MoveNext())
        {
            var currentGameInfo = call.ResponseStream.Current;
            if (currentGameInfo.GameState == GameState.GameStart) break;
        }
        return client;
    }
#else
    public async Task<Client> CreateClient(int teamId, int characterId, CharacterType characterType)
        => new(teamId, characterId, characterType);
#endif

    public async void Start()
    {
        Task<Client>[] clientTasks = new Task<Client>[14];
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
        _clients = await Task.WhenAll(clientTasks);
    }

}