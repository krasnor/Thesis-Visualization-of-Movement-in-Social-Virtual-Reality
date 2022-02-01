using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MovementOrder
{
    [System.Serializable]
    public enum MovmentOrderType
    {
        WAIT_UNTIL_NOTIFY,
        DELAY,
        INSTANT
    }
    [System.Serializable]
    public enum RotationBehavior
    {
        NONE = 0,
        AlignWithWaypoint = 1,
        AlignWithNextWaypoint = 2
    }

    public Transform MovementTarget = null;
    public MovmentOrderType MovementType = MovmentOrderType.WAIT_UNTIL_NOTIFY;
    //[Tooltip("If MovementType set to delay.")]
    public float DelayTime = 2.0f;
    [Tooltip("Defines how Avatar should be rotated while he is waiting (delay or notify)")]
    public RotationBehavior AvatarRotationBehavior = RotationBehavior.NONE;

    public MovementOrder(Transform a_target, MovmentOrderType a_movementType, float a_delayTime, RotationBehavior a_avatarRotationBehavior)
    {
        MovementTarget = a_target;
        MovementType = a_movementType;
        DelayTime = a_delayTime;
        AvatarRotationBehavior = a_avatarRotationBehavior;
    }

    //public MovementOrder() { }
}
