using PlayerRoles.FirstPersonControl;
using System.Data;
using UnityEngine;

namespace SwiftNPCs.Features
{
    public class NPCMotor : NPCComponent
    {
        public Vector3 WishMoveDirection { get; set; }

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

        public bool WishJump { get => Motor.WantsToJump; set => Motor.WantsToJump = value; }

        public IFpcRole Role { get; protected set; }

        public FpcMotor Motor { get; protected set; }

        public CharacterController CharacterController { get; protected set; }

        public override void Begin()
        {
            if (Core.NPC.ReferenceHub.roleManager.CurrentRole is IFpcRole role)
            {
                Role = role;
                Motor = role.FpcModule.Motor;
                CharacterController = role.FpcModule.CharController;
            }
        }

        public override void Tick()
        {
            if (Motor == null)
                return;

            Move();
        }

        public virtual void Move()
        {
            CharacterController.Move(WishMoveDirection * (Time.fixedDeltaTime * Role.FpcModule.MaxMovementSpeed));
            Role.FpcModule.IsGrounded = CharacterController.isGrounded;
        }
    }
}
