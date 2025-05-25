// Used for LocalPlay mode
using System;
using Protobuf;
using UnityEngine;
[RequireComponent(typeof(CharacterBase))]
public class CharacterInteract : InteractBase
{
#if !UNITY_WEBGL || UNITY_EDITOR
    public AvailableService.AvailableServiceClient client => Players.Instance.GetClient(ID);
#else
    public WebGLClient client => Players.Instance.GetClient(ID);
#endif
    [NonSerialized] public CharacterBase characterBase;
    public long ID => characterBase.ID;
    public void Start()
    {
        characterBase = GetComponent<CharacterBase>();
    }
    public void Attack(CharacterBase target)
    {
        client.Attack(new AttackMsg
        {
            CharacterId = characterBase.message.PlayerId,
            TeamId = characterBase.message.TeamId,
            AttackRange = ParaDefine.Instance.GetData(characterBase.characterType).attackRange,
            AttackedCharacterId = target.message.PlayerId,
            AttackedTeam = target.message.TeamId,
        });
    }
    public void CastSkill(Vector2 position)
    {
        var (targetX, targetY) = Tool.UxyToGrid(position);
        float angle = Mathf.Atan2(targetY - characterBase.message.Y, targetX - characterBase.message.X);
        client.Cast(new CastMsg
        {
            CharacterId = characterBase.message.PlayerId,
            TeamId = characterBase.message.TeamId,
            SkillId = 0, // TO BE CONFIRMED
            CastedCharacterId = { }, // TODO
            AttackRange = ParaDefine.Instance.GetData(characterBase.characterType).skillRange,
            X = characterBase.message.X,
            Y = characterBase.message.Y,
            Angle = angle
        });
    }
    public void Move(Vector2 position)
    {
        var (targetX, targetY) = Tool.UxyToGrid(position);
        float angle = Mathf.Atan2(targetY - characterBase.message.Y, targetX - characterBase.message.X);
        float speed = characterBase.message.Speed;
        long time = Mathf.CeilToInt(Mathf.Sqrt(Mathf.Pow(targetX - characterBase.message.X, 2) + Mathf.Pow(targetY - characterBase.message.Y, 2)) * 1000 / speed);
        client.Move(new MoveMsg
        {
            CharacterId = characterBase.message.PlayerId,
            TeamId = characterBase.message.TeamId,
            Angle = angle,
            TimeInMilliseconds = time
        });
    }

}