﻿using InventorySystem.Items;
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
            if (Core.HasTarget)
            {
                TargetLastPosition = Core.Target.HitPosition;
                chasing = false;
                if ((Core.Pathfinder.Destination - Core.Position).sqrMagnitude > 9f)
                    Core.Pathfinder.Destination = Core.Position;
            }
            else if (CanChase && (TargetLastPosition - Core.Position).sqrMagnitude > 9f && Core.Pathfinder.RealDestination != TargetLastPosition)
            {
                Core.Pathfinder.Destination = TargetLastPosition;
                chasing = true;
            }

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
