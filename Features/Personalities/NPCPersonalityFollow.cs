using LabApi.Features.Wrappers;
using PlayerRoles.FirstPersonControl;
using SwiftNPCs.Features.Components;
using SwiftNPCs.Utils.Structures;
using UnityEngine;

namespace SwiftNPCs.Features.Personalities
{
    public class NPCPersonalityFollow : NPCPersonalityBase
    {
        public const string DataID = "FollowTarget";

        public Data CurrentData;

        public Player FollowTarget
        {
            get => HasFollowTarget ? CurrentData.FollowTarget : null;
            set
            {
                if (CurrentData == null && !Core.TryGetData(DataID, out CurrentData))
                    Core.TryAddData(DataID, out CurrentData);

                if (CurrentData != null)
                    CurrentData.FollowTarget = value;
            }
        }

        public bool HasFollowTarget => CurrentData != null && CurrentData.FollowTarget != null && CurrentData.FollowTarget.ReferenceHub != null;

        public bool InFollowRange => HasFollowTarget && (CurrentData.FollowTarget.Position - Core.Position).sqrMagnitude < CurrentFollowRange * CurrentFollowRange;

        public float CurrentFollowRange = 0.5f;

        public float MaxFollowRange = 4f;
        public float MinFollowRange = 2f;
        public float SprintRange = 15f;

        readonly Timer followUpdate = new(0.5f);

        public class Data
        {
            public Player FollowTarget { get; set; }
        }

        public override void Init(NPCCore core)
        {
            base.Init(core);
            if (!Core.TryGetData(DataID, out CurrentData))
                Core.TryAddData(DataID, out CurrentData);

            CurrentFollowRange = Random.Range(MinFollowRange, MaxFollowRange);
        }

        public override void Tick()
        {
            if (Core.HasTarget)
            {
                Core.SetPersonality(new NPCPersonalityHumanCombat());
                return;
            }

            if (!FollowTarget.IsAlive || FollowTarget.IsEnemy(WrapperPlayer))
            {
                FollowTarget = null;
                Core.SetPersonality(new NPCPersonalityWanderHuman());
                return;
            }

            if (!HasFollowTarget)
                return;

            followUpdate.Tick(Time.fixedDeltaTime);

            if (FollowTarget.RoleBase is IFpcRole role)
                Core.Motor.MoveState = (CurrentData.FollowTarget.Position - Core.Position).sqrMagnitude < SprintRange * SprintRange ? role.FpcModule.CurrentMovementState : PlayerMovementState.Sprinting;

            if (InFollowRange)
            {
                Core.Pathfinder.Stop();
                Core.Pathfinder.LookAtWaypoint = false;
                Core.Motor.WishLookPosition = FollowTarget.Camera.position;
                CurrentFollowRange = Random.Range(MinFollowRange, MaxFollowRange);
                return;
            }

            if (followUpdate.Ended)
            {
                UpdateDestination();
                followUpdate.Reset();
            }

            if (Core.Pathfinder.IsAtDestination && !InFollowRange)
                UpdateDestination();
        }

        public override void Begin() { }

        public override void End() { }

        public void UpdateDestination()
        {
            Core.Pathfinder.Destination = CurrentData.FollowTarget.Position;
            Core.Pathfinder.LookAtWaypoint = true;
        }
    }
}
