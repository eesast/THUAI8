using UnityEngine;
using System;
#if !UNITY_WEBGL
using Grpc.Core;
using Protobuf;

class Player : MonoBehaviour
{
    public static Player buddhistsMain, monstersMain;
    public AvailableService.AvailableServiceClient client;
    public CharacterType characterType;
    public int characterId = 0, teamId = 0;
    private bool gameStarted;
    private long ID;
    private CharacterControl characterControl;

    public async void Start()
    {
        ID = teamId * 6 + characterId - 1;
        if (characterId == 0)
        {
            if (teamId == 0)
                buddhistsMain ??= this;
            else if (teamId == 1)
                monstersMain ??= this;
        }

        Channel channel = new("127.0.0.1:8888", ChannelCredentials.Insecure);
        // Wait for 30s.
        await channel.ConnectAsync(DateTime.UtcNow.AddSeconds(30));
        client = new AvailableService.AvailableServiceClient(channel);
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
            print(currentGameInfo);
            if (currentGameInfo.GameState == GameState.GameStart) break;
        }
        gameStarted = true;
    }

    void Update()
    {
        if (characterId != 0 && gameStarted && characterControl == null)
        {
            if (CoreParam.charactersG.TryGetValue(ID, out GameObject characterG))
            {
                characterControl = characterG.GetComponent<CharacterControl>();
                characterControl.client = client;
            }
        }
    }
}
#else
// Not Implemented
class Player : MonoBehaviour
{
    public void Start()
    {
        throw new NotImplementedException("Spectator/LocalPlay mode is not implemented in this build.");
    }
}
#endif