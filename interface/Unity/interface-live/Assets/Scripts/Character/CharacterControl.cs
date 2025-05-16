// Used for LocalPlay mode
using System;
using Protobuf;
using UnityEngine;
#if !UNITY_WEBGL
[RequireComponent(typeof(CharacterBase))]
class CharacterControl : InteractBase
{
    public AvailableService.AvailableServiceClient client;
    public CharacterBase characterBase;
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
            AttackRange = 1, // TODO
            AttackedCharacterId = target.ID,
            AttackedTeam = target.message.TeamId,
        });
    }
    public void CastSkill()
    {
        client.Cast(new CastMsg
        {
            CharacterId = characterBase.message.PlayerId,
            TeamId = characterBase.message.TeamId,
            SkillId = 0, // TODO
            CastedCharacterId = { },
            AttackRange = 1, // TODO
            X = characterBase.message.X,
            Y = characterBase.message.Y,
            Angle = 1, // TODO
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
#else
class CharacterControl: InteractBase
{
    public void Start()
    {
        // Debug.LogWarning("LocalPlay mode is not implemented in this build.");
    }
}
#endif