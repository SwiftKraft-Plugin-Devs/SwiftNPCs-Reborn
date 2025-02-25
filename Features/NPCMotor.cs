using PlayerRoles.FirstPersonControl;
using UnityEngine;

namespace SwiftNPCs.Features
{
    public class NPCMotor : NPCComponent
    {
        public Vector3 WishMoveDirection
        {
            get => Motor != null ? Motor.MoveDirection : Vector3.zero;
            set
            {
                if (Motor != null)
                    Motor.MoveDirection = value;
            }
        }

        public Quaternion WishLookRotation { get; set; }

        public Vector3 WishLookDirection
        {
            get => WishLookRotation * Vector3.forward;
            set => WishLookRotation = Quaternion.LookRotation(value.normalized);
        }

        public Vector3 WishLookPosition
        {
            get => Core.NPC.WrapperPlayer.Camera.position + WishLookDirection;
            set => WishLookDirection = value - Core.NPC.WrapperPlayer.Camera.position;
        }

        public Quaternion CurrentLookRotation { get; protected set; }

        public IFpcRole Role { get; protected set; }

        public FpcMotor Motor { get; protected set; }

        public override void Begin()
        {
            if (Core.NPC.WrapperPlayer.RoleBase is IFpcRole role)
            {
                Role = role;
                Motor = role.FpcModule.Motor;
            }
        }

        public override void Tick()
        {
            WishMoveDirection = Core.transform.forward;
        }
    }
}
