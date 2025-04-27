﻿using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using System;
using System.Threading;

namespace Preparation.Interface
{
    public interface IMovable : IGameObj
    {
        public XY FacingDirection { get; set; }
        object ActionLock { get; }
        public AtomicInt MoveSpeed { get; }
        public AtomicBool CanMove { get; }
        public AtomicBool IsMoving { get; }
        public bool IsAvailableForMove { get; }
        public long StateNum { get; }
        public Semaphore ThreadNum { get; }
        public long MovingSetPos(XY moveVec, long stateNum);
        public bool WillCollideWith(IGameObj? targetObj, XY nextPos, bool collideWithWormhole = false)  // 检查下一位置是否会和目标物碰撞
        {
            if (targetObj == null)
                return false;
            if (!targetObj.IsRigid(collideWithWormhole) || targetObj.ID == ID)
                return false;

            if (IgnoreCollideExecutor(targetObj) || targetObj.IgnoreCollideExecutor(this))
                return false;

            if (targetObj.Shape == ShapeType.CIRCLE)
            {
                return XY.DistanceCeil3(nextPos, targetObj.Position) < targetObj.Radius + Radius;
            }
            else  // Square
            {
                long deltaX = Math.Abs(nextPos.x - targetObj.Position.x), deltaY = Math.Abs(nextPos.y - targetObj.Position.y);
                if (deltaX >= targetObj.Radius + Radius || deltaY >= targetObj.Radius + Radius)
                    return false;
                if (deltaX < targetObj.Radius || deltaY < targetObj.Radius)
                    return true;
                else
                    return ((long)(deltaX - targetObj.Radius) * (deltaX - targetObj.Radius)) + ((long)(deltaY - targetObj.Radius) * (deltaY - targetObj.Radius)) <= (long)Radius * (long)Radius;
            }
        }
    }
}
