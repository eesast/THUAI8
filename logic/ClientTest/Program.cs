using Grpc.Core;
using Protobuf;

namespace ClientTest
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            // 从命令行参数解析 CharacterId 和 TeamId
            if (!int.TryParse(args[0], out int characterId)) {
                Console.WriteLine("Invalid CharacterId. Please provide a valid integer.");
                return Task.CompletedTask;
            }

            if (!int.TryParse(args[1], out int teamId)) {
                Console.WriteLine("Invalid TeamId. Please provide a valid integer.");
                return Task.CompletedTask;
            }
            Thread.Sleep(3000);
            Channel channel = new("127.0.0.1:8888", ChannelCredentials.Insecure);
            var client = new AvailableService.AvailableServiceClient(channel);
            CharacterMsg playerInfo = new()
            {
                CharacterId = characterId,
                TeamId = teamId,
                CharacterType = teamId == 0 ? CharacterType.JiuLing : CharacterType.TangSeng,
                SideFlag = 1 - teamId
            };
            var call = client.AddCharacter(playerInfo);
            MoveMsg moveMsg = new()
            {
                CharacterId = 0,
                TeamId = 0,
                TimeInMilliseconds = 100,
                Angle = 0
            };
            int tot = 0;
            /*while (call.ResponseStream.MoveNext().Result)
            {
                var currentGameInfo = call.ResponseStream.Current;
                if (currentGameInfo.GameState == GameState.GameStart) break;
            }*/
            while (true)
            {
                Thread.Sleep(50);
                //MoveRes boolRes = client.Move(moveMsg);
                //if (boolRes.ActSuccess == false) break;
                tot++;
                //if (tot % 10 == 0) moveMsg.Angle += 1;
            }
            return Task.CompletedTask;
        }
    }
}