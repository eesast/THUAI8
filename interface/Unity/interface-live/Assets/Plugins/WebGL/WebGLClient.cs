using System.Runtime.InteropServices;
using Google.Protobuf;
using Protobuf;

public class WebGLClient
{
    [DllImport("__Internal")]
    private static extern void SendToServer(string action, long id, byte[] bytesPtr, int size);
    [DllImport("__Internal")]
    private static extern void CreateClient(long teamId, long characterId, int characterType);
    private static void SendToServer(string action, long characterId, long teamId, byte[] bytes)
    {
        SendToServer(action, teamId * 6 + characterId - 1, bytes, bytes.Length);
    }

    public WebGLClient(long teamId, long characterId, CharacterType characterType)
    {
        CreateClient(teamId, characterId, (int)characterType);
    }


    public void CreatCharacter(CharacterMsg msg) => SendToServer("CreateCharacter", msg.CharacterId, msg.TeamId, msg.ToByteArray());
    public void Move(MoveMsg msg) => SendToServer("Move", msg.CharacterId, msg.TeamId, msg.ToByteArray());
    public void Attack(AttackMsg msg) => SendToServer("Attack", msg.CharacterId, msg.TeamId, msg.ToByteArray());
    public void Cast(CastMsg msg) => SendToServer("Cast", msg.CharacterId, msg.TeamId, msg.ToByteArray());
}