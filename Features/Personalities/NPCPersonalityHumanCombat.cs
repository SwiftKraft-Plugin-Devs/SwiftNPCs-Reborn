using InventorySystem.Items;
using SwiftNPCs.Utils.Structures;
using UnityEngine;

namespace SwiftNPCs.Features.Personalities
{
    public class NPCPersonalityHumanCombat : NPCPersonalityBase
    {
        public virtual NPCPersonalityBase ExitPersonality => Core.PreviousPersonality;
        public virtual NPCPersonalityBase DefaultExitPersonality => new NPCPersonalityWanderHuman();

        public Vector3 TargetLastPosition { get; set; }

        public float StrafeRange = 5f;
        public float StrafeTimeMin = 0.5f;
        public float StrafeTimeMax = 2f;
        public float BackOffDistance = 3f;
        public float PushDistance = 16f;

        public bool CanChase = true;

        bool chasing;

        readonly Timer strafeTimer = new();

        public override void Begin() => TargetLastPosition = Core.Position;

        public override void End() { }

        public override void Tick()
        {
            if (!IsThreat)
            {
                Core.SetPersonality(ExitPersonality == null || ExitPersonality == this ? DefaultExitPersonality : ExitPersonality);
                return;
            }

            Core.Motor.MoveState = chasing ? PlayerRoles.FirstPersonControl.PlayerMovementState.Sprinting : PlayerRoles.FirstPersonControl.PlayerMovementState.Walking;

            CheckTargetLoop();
            WeaponLoop();
        }

        private void CheckTargetLoop()
        {
            if (Core.HasTarget)
            {
                TargetLastPosition = Core.Target.HitPosition;
                float sqrDist = (Core.Target.PivotPosition - Core.Position).sqrMagnitude;
                if (sqrDist < BackOffDistance * BackOffDistance)
                    Core.Pathfinder.Destination = -Core.NPC.ReferenceHub.transform.forward;
                else if (sqrDist >= PushDistance * PushDistance)
                    Core.Pathfinder.Destination = TargetLastPosition;
            }

            if (Core.CanAttackTarget)
            {
                chasing = false;
                strafeTimer.Tick(DeltaTime);
                if (strafeTimer.Ended)
                {
                    strafeTimer.Reset(Random.Range(StrafeTimeMin, StrafeTimeMax));
                    Vector2 random = Random.insideUnitCircle * StrafeRange;
                    Vector3 rand = new(random.x, 0f, random.y);
                    Core.Pathfinder.Destination = Core.Position + rand;
                }
            }
            else if (CanChase && (TargetLastPosition - Core.Position).sqrMagnitude > 4f && Core.Pathfinder.RealDestination != TargetLastPosition)
            {
                Core.Pathfinder.Destination = TargetLastPosition;
                chasing = true;
            }

            if (chasing && Core.Pathfinder.IsAtDestination)
                chasing = false;

            if (!Core.HasTarget && !chasing)
                Core.SetPersonality(ExitPersonality == null || ExitPersonality == this ? DefaultExitPersonality : ExitPersonality);
        }

        private void WeaponLoop()
        {
            if (GetWeapon(out ItemBase item, out _))
            {
                if (!IsArmed && (IsThreat || Core.HasTarget))
                {
                    Core.ItemUser.CanUse = true;
                    Core.Inventory.EquipItem(item);
                }
                else if (IsCivilian && !Core.HasTarget && !chasing)
                    Core.Inventory.UnequipItem();
            }
        }
    }
}
