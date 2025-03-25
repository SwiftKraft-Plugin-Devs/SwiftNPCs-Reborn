using InventorySystem.Items;
using UnityEngine;

namespace SwiftNPCs.Features.Personalities
{
    public class NPCPersonalityHuman : NPCPersonalityBase
    {
        public Vector3 TargetLastPosition { get; set; }

        public override void Begin() => TargetLastPosition = Core.Position;

        public override void End() { }

        public override void Tick()
        {
            if (GetWeapon(out ItemBase item, out _))
            {
                if (IsThreat || Core.HasTarget)
                {
                    Core.ItemUser.CanUse = true;
                    Core.Inventory.EquipItem(item);
                }
                else if (Core.Inventory.CurrentItem.Category == ItemCategory.Firearm || Core.Inventory.CurrentItem.Category == ItemCategory.SpecialWeapon || Core.Inventory.CurrentItem.Category == ItemCategory.Grenade)
                    Core.Inventory.UnequipItem();
            }

            if (Core.HasTarget)
                TargetLastPosition = Core.Target.HitPosition;
            else if ((TargetLastPosition - Core.Position).sqrMagnitude > 1f && Core.Pathfinder.Destination != TargetLastPosition)
                Core.Pathfinder.Destination = TargetLastPosition;
        }
    }
}
