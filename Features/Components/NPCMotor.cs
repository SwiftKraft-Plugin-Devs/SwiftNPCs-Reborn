using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Handlers;
using PlayerRoles.FirstPersonControl;
using RelativePositioning;
using SwiftNPCs.Utils.Extensions;
using UnityEngine;

namespace SwiftNPCs.Features.Components
{
    public class NPCMotor : NPCComponent
    {
        public static LayerMask DoorCheckLayers = LayerMask.GetMask("Door");

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

        public bool WishJump { get => Motor.JumpController.WantsToJump; set => Motor.JumpController.WantsToJump = value; }

        public IFpcRole Role { get; protected set; }

        public PlayerMovementState MoveState { get => Role.FpcModule.CurrentMovementState; set => Role.FpcModule.CurrentMovementState = value; }

        public FpcMotor Motor { get; protected set; }
        public FpcMouseLook MouseLook { get; protected set; }

        public float LookTime = 0.5f;
        public float MoveSpeed = 30f;

        public bool CanOpenDoors = true;

        Vector3 lookVel;

        public override void Begin()
        {
            RefreshRole();
            PlayerEvents.ChangedRole += OnChangedRole;
        }

        public override void Tick()
        {
            if (Motor == null)
                return;

            Move();
            Look();
        }

        public override void Frame() => Look();

        public override void Close()
        {
            base.Close();
            PlayerEvents.ChangedRole -= OnChangedRole;
        }

        private void OnChangedRole(LabApi.Events.Arguments.PlayerEvents.PlayerChangedRoleEventArgs ev)
        {
            if (ev.Player != Core.NPC.WrapperPlayer)
                return;

            RefreshRole();
        }

        public void RefreshRole()
        {
            if (Core.NPC.ReferenceHub.roleManager.CurrentRole is IFpcRole role)
            {
                Role = role;
                Motor = role.FpcModule.Motor;
                MouseLook = role.FpcModule.MouseLook;
            }
        }

        public virtual void Move()
        {
            Motor.ReceivedPosition = new RelativePosition(Core.Position + WishMoveDirection * (MoveSpeed * Time.fixedDeltaTime));

            if (CanOpenDoors && WishMoveDirection != Vector3.zero && Core.TryGetDoor(out DoorVariant door, out bool inVision) && inVision && Vector3.Angle(door.transform.position - Core.Position, WishMoveDirection) <= 22.5f)
                Core.TrySetDoor(door, true);
        }

        public virtual void Look()
        {
            CurrentLookRotation = CurrentLookRotation.SmoothDamp(WishLookRotation, ref lookVel, LookTime);
            MouseLook.LookAtDirection(CurrentLookRotation * Vector3.forward);
        }
    }
}
