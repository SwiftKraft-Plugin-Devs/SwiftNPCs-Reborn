using InventorySystem.Items;
using LabApi.Features.Wrappers;
using SwiftNPCs.Utils.Extensions;
using System.Linq;
using UnityEngine;

namespace SwiftNPCs.Features.Personalities
{
    public class NPCPersonalityHuman : NPCPersonalityBase
    {
        public Vector3 TargetLastPosition { get; set; }

        public bool CanChase = true;

        bool chasing;

        public override void Begin() => TargetLastPosition = Core.Position;

        public override void End() { }

        public override void Tick()
        {
            CheckTargetLoop();

            WanderLoop();

            WeaponLoop();
        }

        private void WanderLoop()
        {
            if (!Core.HasTarget && !chasing && Core.Pathfinder.IsAtDestination)
            {
                Room r = Room.List.Where((r) => r != null && r.Base != null && !r.IsDestroyed && r.Zone == Core.NPC.WrapperPlayer.Zone).ToList().GetRandom();
                if (r != null)
                    Core.Pathfinder.Destination = r.Transform.position;
            }
        }

        private void CheckTargetLoop()
        {
            if (Core.HasTarget)
            {
                TargetLastPosition = Core.Target.HitPosition;
                chasing = false;
                if ((Core.Pathfinder.Destination - Core.Position).sqrMagnitude > 4f)
                    Core.Pathfinder.Destination = Core.Position;
            }
            else if (CanChase && (TargetLastPosition - Core.Position).sqrMagnitude > 4f && Core.Pathfinder.RealDestination != TargetLastPosition)
            {
                Core.Pathfinder.Destination = TargetLastPosition;
                chasing = true;
            }

            if (chasing && Core.Pathfinder.IsAtDestination)
                chasing = false;
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
