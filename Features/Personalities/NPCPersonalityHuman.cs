using InventorySystem.Items;
using UnityEngine;

namespace SwiftNPCs.Features.Personalities
{
    public class NPCPersonalityHuman : NPCPersonalityBase
    {
        public Vector3 TargetLastPosition { get; set; }

        public bool CanChase = true;

        public override void Begin() => TargetLastPosition = Core.Position;

        public override void End() { }

        public override void Tick()
        {
            if (GetWeapon(out ItemBase item, out _))
            {
                if (!IsArmed && (IsThreat || Core.HasTarget))
                {
                    Core.ItemUser.CanUse = true;
                    Core.Inventory.EquipItem(item);
                }
                else if (IsCivilian && !Core.HasTarget)
                    Core.Inventory.UnequipItem();
            }

            if (Core.HasTarget)
                TargetLastPosition = Core.Target.HitPosition;
            else if (CanChase && (TargetLastPosition - Core.Position).sqrMagnitude > 1f && Core.Pathfinder.Destination != TargetLastPosition)
                Core.Pathfinder.Destination = TargetLastPosition;
        }
    }
}
