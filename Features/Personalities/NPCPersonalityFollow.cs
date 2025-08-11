using InventorySystem.Items;
using LabApi.Features.Wrappers;
using PlayerRoles.FirstPersonControl;
using SwiftNPCs.Features.Components;
using SwiftNPCs.Utils.Extensions;
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

        public float MaxLookTimer = 4f;
        public float MinLookTimer = 2f;

        readonly Timer followUpdate = new(0.5f);
        readonly Timer lookTimer = new();

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
            Core.Pathfinder.OnStuck += OnStuck;
        }

        public override void Tick()
        {
            if (Core.HasTarget)
            {
                Core.SetPersonality(new NPCPersonalityHumanCombat());
                return;
            }

            if (!HasFollowTarget)
                return;

            CheckFollowTarget();

            if (FollowTarget.CurrentItem != null
                && (FollowTarget.CurrentItem.Category == ItemCategory.SpecialWeapon
                || FollowTarget.CurrentItem.Category == ItemCategory.Firearm)
                && GetWeapon(out ItemBase item, out _))
                Core.Inventory.EquipItem(item);
            else if (IsCivilian)
                Core.Inventory.UnequipItem();

            followUpdate.Tick(DeltaTime);

            if (FollowTarget.RoleBase is IFpcRole role)
                Core.Motor.MoveState = (CurrentData.FollowTarget.Position - Core.Position).sqrMagnitude < SprintRange * SprintRange ? role.FpcModule.CurrentMovementState : PlayerMovementState.Sprinting;

            bool targetInElevator = FollowTarget.TryGetElevator(out Elevator elev);

            if (targetInElevator)
                CurrentFollowRange = 0.75f;

            if (InFollowRange && (!targetInElevator || (WrapperPlayer.TryGetElevator(out Elevator thisElev) && thisElev == elev)))
            {
                Core.Pathfinder.Stop();
                Core.Pathfinder.LookAtWaypoint = false;
                CurrentFollowRange = Random.Range(MinFollowRange, MaxFollowRange);
                Core.LookAround(lookTimer, MinLookTimer, MaxLookTimer);
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

        public override void Begin()
        {
            if (!HasFollowTarget)
                return;

            CheckFollowTarget();
        }

        public override void End() => Core.Pathfinder.OnStuck -= OnStuck;

        private void CheckFollowTarget()
        {
            if (FollowTarget.IsAlive && !FollowTarget.IsEnemy(WrapperPlayer))
                return;

            FollowTarget = null;
            Core.SetPersonality(new NPCPersonalityWanderHuman());
        }

        private void OnStuck() => Core.Motor.WishJump = true;

        public void UpdateDestination()
        {
            Core.Pathfinder.Destination = CurrentData.FollowTarget.Position;
            Core.Pathfinder.LookAtWaypoint = true;
        }
    }
}
