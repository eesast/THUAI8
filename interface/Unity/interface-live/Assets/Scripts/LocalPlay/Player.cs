using UnityEngine;
using System;
#if !UNITY_WEBGL
using Grpc.Core;
using Protobuf;

class Player : MonoBehaviour
{
    public AvailableService.AvailableServiceClient client;
    public int characterId = 0, teamId = 0;
    private bool gameStarted;
    private CharacterControl characterControl;

    public async void Start()
    {
        Channel channel = new("127.0.0.1:8888", ChannelCredentials.Insecure);
        // Wait for 30s.
        await channel.ConnectAsync(DateTime.UtcNow.AddSeconds(30));
        client = new AvailableService.AvailableServiceClient(channel);
        CharacterMsg playerInfo = new()
        {
            CharacterId = characterId,
            CharacterType = teamId == 0 ? CharacterType.TangSeng : CharacterType.JiuLing,
            TeamId = teamId,
            SideFlag = teamId
        };
        var call = client.AddCharacter(playerInfo);
        Debug.Log($"Trying to join game as {(teamId == 0 ? "buddhists" : "monsters")}...");
        while (await call.ResponseStream.MoveNext())
        {
            var currentGameInfo = call.ResponseStream.Current;
            print(currentGameInfo);
            if (currentGameInfo.GameState == GameState.GameStart) break;
        }
        gameStarted = true;
        client.Move(new MoveMsg()
        {
            CharacterId = characterId,
            TeamId = teamId,
            Angle = 0,
            TimeInMilliseconds = 0
        });
    }

    void Update()
    {
        if (characterId != 0 && gameStarted && characterControl == null)
        {
            characterControl = CoreParam.charactersG[teamId * 6 + characterId - 1].GetComponent<CharacterControl>();
            characterControl.client = client;
        }
    } 
}
#else
// Not Implemented
class Spectator : MonoBehaviour
{
    public void Start()
    {
        throw new NotImplementedException("Spectator/LocalPlay mode is not implemented in this build.");
    }
}
#endif