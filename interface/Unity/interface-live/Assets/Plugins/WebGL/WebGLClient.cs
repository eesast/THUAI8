using System.Runtime.InteropServices;
using Google.Protobuf;
using Protobuf;

public class WebGLClient
{
    [DllImport("__Internal")]
    private static extern void SendToServer(string action, int id, byte[] bytesPtr, int size);
    [DllImport("__Internal")]
    private static extern void CreateClient(int teamId, int characterId, int characterType);
    private static void SendToServer(string action, long characterId, long teamId, byte[] bytes)
    {
        SendToServer(action, (int)(teamId * 7 + characterId), bytes, bytes.Length);
    }
    private readonly int teamId, characterId;
    private readonly CharacterType characterType;

    public WebGLClient(int teamId, int characterId, CharacterType characterType)
    {
        CreateClient(teamId, characterId, (int)characterType);
        this.teamId = teamId;
        this.characterId = characterId;
        this.characterType = characterType;
    }


    public void CreatCharacter(CreatCharacterMsg msg) => SendToServer("CreatCharacter", 0, msg.TeamId, msg.ToByteArray());
    public void Move(MoveMsg msg) => SendToServer("Move", msg.CharacterId, msg.TeamId, msg.ToByteArray());
    public void Attack(AttackMsg msg) => SendToServer("Attack", msg.CharacterId, msg.TeamId, msg.ToByteArray());
    public void Cast(CastMsg msg) => SendToServer("Cast", msg.CharacterId, msg.TeamId, msg.ToByteArray());
}