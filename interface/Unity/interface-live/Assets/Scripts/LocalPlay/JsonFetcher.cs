using Grpc.Core;
using Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

class JsonFetcher : MonoBehaviour
{
    public static AvailableService.AvailableServiceClient client;
    public static IAsyncStreamReader<MessageToClient> responseStream;

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public async void Start()
    {
        int characterId = 5000, teamId = 0;
        Debug.Log("Trying to connect server...");
        Channel channel = new("127.0.0.1:8888", ChannelCredentials.Insecure);
        // Wait for 30s.
        await channel.ConnectAsync(DateTime.UtcNow.AddSeconds(30));
        var client = new AvailableService.AvailableServiceClient(channel);
        Debug.Log("Successfully connected.");
        CharacterMsg playerInfo = new()
        {
            CharacterId = characterId,
            CharacterType = CharacterType.NullCharacterType,
            TeamId = teamId,
            SideFlag = teamId
        };
        var call = client.AddCharacter(playerInfo);
        Debug.Log("Trying to join game as spectator...");
        while (await call.ResponseStream.MoveNext())
        {
            var currentGameInfo = call.ResponseStream.Current;
            if (currentGameInfo.GameState == GameState.GameStart) break;
        }
        CoreParam.firstFrame = call.ResponseStream.Current;
        Debug.Log("Game started!");
        while (await call.ResponseStream.MoveNext())
        {
            CoreParam.frameQueue.Add(call.ResponseStream.Current);
            CoreParam.cnt++;
        }

    }


}