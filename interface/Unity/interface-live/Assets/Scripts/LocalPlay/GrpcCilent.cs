using Grpc.Core;
using Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

class GrpcClient
{
    public static AvailableService.AvailableServiceClient client;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitCilent()
    {
        int characterId = 0, teamId = 0;
        Channel channel = new("127.0.0.1:8888", ChannelCredentials.Insecure);
        var client = new AvailableService.AvailableServiceClient(channel);
        CharacterMsg playerInfo = new()
        {
            CharacterId = characterId,
            TeamId = teamId,
            CharacterType = teamId == 1 ? CharacterType.JiuLing : CharacterType.TangSeng,
            SideFlag = teamId
        };
        var call = client.AddCharacter(playerInfo);
        while (call.ResponseStream.MoveNext().Result)
        {
            var currentGameInfo = call.ResponseStream.Current;
            if (currentGameInfo.GameState == GameState.GameStart) break;
        }
    }

    public static void SendMessage()
    {
        BoolRes call = client.Attack(new AttackMsg()
        {
            CharacterId = 0,
            TargetId = 1,
            AttackType = AttackType.NormalAttack
        });
    }


}