using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using PlayerRoles.FirstPersonControl;
using UnityEngine;

namespace SwiftNPCs.Features
{
    public class NPCMotor : NPCComponent
    {
        public static LayerMask DoorCheckLayers = LayerMask.GetMask("Default", "Door");

        public Vector3 WishMoveDirection { get; set; }

        public Quaternion WishLookRotation { get; set; }

        public Vector3 WishLookDirection
        {
            get => WishLookRotation * Vector3.forward;
            set
            {
                if (value != default)
                    WishLookRotation = Quaternion.LookRotation(value.normalized, Vector3.up);
            }
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
        public FpcMouseLook MouseLook { get; protected set; }

        public CharacterController CharacterController { get; protected set; }

        public float LookSpeed = 400f;

        public bool CanOpenDoors = true;

        public override void Begin()
        {
            if (Core.NPC.ReferenceHub.roleManager.CurrentRole is IFpcRole role)
            {
                Role = role;
                Motor = role.FpcModule.Motor;
                MouseLook = role.FpcModule.MouseLook;
                CharacterController = role.FpcModule.CharController;
            }
        }

        public override void Tick()
        {
            if (Motor == null)
                return;

            Move();
            Look();
        }

        public override void Frame() => Look();

        public virtual void Move()
        {
            CharacterController.Move(WishMoveDirection * (Time.fixedDeltaTime * Role.FpcModule.MaxMovementSpeed));
            Role.FpcModule.IsGrounded = CharacterController.isGrounded;

            if (CanOpenDoors && Physics.Raycast(Core.Position, CurrentLookRotation * Vector3.forward, out RaycastHit _hit, 3.63f, DoorCheckLayers) && _hit.transform.gameObject.TryGetComponent(out DoorVariant door))
                door.ServerInteract(Core.NPC.ReferenceHub, 0);
        }

        public virtual void Look()
        {
            CurrentLookRotation = Quaternion.RotateTowards(CurrentLookRotation, WishLookRotation, LookSpeed * Time.fixedDeltaTime);
            MouseLook.LookAtDirection(CurrentLookRotation * Vector3.forward);
            Core.transform.rotation = CurrentLookRotation;
        }
    }
}
