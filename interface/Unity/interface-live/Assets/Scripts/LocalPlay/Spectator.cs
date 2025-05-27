using UnityEngine;
using System;
#if !UNITY_WEBGL || UNITY_EDITOR
using Grpc.Core;
using Protobuf;

class Spectator : SingletonMono<Spectator>
{
    public AvailableService.AvailableServiceClient client;

    public async void Start()
    {
        int characterId = 5000, teamId = 0;
        Debug.Log("Trying to connect server...");
        Channel channel = new("127.0.0.1:8888", ChannelCredentials.Insecure);
        // Wait for 30s.
        await channel.ConnectAsync(DateTime.UtcNow.AddSeconds(30));
        client = new AvailableService.AvailableServiceClient(channel);
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
#else
// WebGL下使用直播接口实现观战，Spectator类不实现任何功能，为避免编辑器中场景数据绑定异常而保留定义。
class Spectator : SingletonMono<Spectator> {}
#endif