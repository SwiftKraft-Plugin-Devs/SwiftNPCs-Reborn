﻿using LabApi.Features.Wrappers;
using PlayerRoles;
using SwiftNPCs.Features.Targettables;
using SwiftNPCs.Utils.Structures;
using UnityEngine;

namespace SwiftNPCs.Features.Components
{
    public class NPCScanner : NPCComponent
    {
        public static LayerMask SightLayers = LayerMask.GetMask("Default", "Door");
        public static LayerMask CollisionLayers = LayerMask.GetMask("Default", "Glass", "Door");

        public float FacilityRange = 40f;
        public float SurfaceRange = 70f;

        public float CurrentRange => Core.NPC.WrapperPlayer.Zone == MapGeneration.FacilityZone.Surface ? SurfaceRange : FacilityRange;

        private readonly Timer timer = new(0.5f);

        public override void Begin() { }

        public override void Tick()
        {
            timer.Tick(Time.fixedDeltaTime);
            if (timer.Ended)
            {
                timer.Reset();
                Search();
            }
        }

        public void Search()
        {
            if (Core.Target != null && (!HasLOS(Core.Target, out _) || Core.Target is TargetablePlayer p && (!Core.NPC.WrapperPlayer.IsEnemy(p.Target) || (p.HitPosition - Core.Position).sqrMagnitude > CurrentRange * CurrentRange)))
                Core.Target = null;

            foreach (Player player in Player.List)
                if ((player.Position - Core.Position).sqrMagnitude <= CurrentRange * CurrentRange && (Core.Target == null || (player.Position - Core.Position).sqrMagnitude < (Core.Target.HitPosition - Core.Position).sqrMagnitude) && HasLOS(player, out _) && Core.NPC.WrapperPlayer.IsEnemy(player))
                    Core.Target = new TargetablePlayer(player);
        }

        public bool HasLOS(Player player, out Vector3 sight, bool collision = false) => CheckLOS(Core.NPC.WrapperPlayer.Camera.position, out sight, collision, player.Camera.position, player.Position);

        public bool HasLOS(TargetableBase t, out Vector3 sight, bool collision = false) => CheckLOS(Core.NPC.WrapperPlayer.Camera.position, out sight, collision, t.CriticalPosition, t.HitPosition);

        public static bool CheckLOS(Vector3 cam, out Vector3 sight, bool collision = false, params Vector3[] others)
        {
            foreach (Vector3 pos in others)
                if (!Physics.Linecast(cam, pos, collision ? CollisionLayers : SightLayers, QueryTriggerInteraction.Ignore))
                {
                    sight = pos;
                    return true;
                }
            sight = default;
            return false;
        }
    }

    public static class EnemyCheck
    {
        public static bool IsEnemy(this Player player, Player other) =>
                other != null
                && other.ReferenceHub != null
                && other.IsAlive
                && !other.IsDisarmed
                && player.IsAlive
                && player.Role.GetFaction() != other.Role.GetFaction()
                && (!player.IsDisarmed
                || (player.DisarmedBy != null
                && player.DisarmedBy.Role.GetFaction() == other.Role.GetFaction()))
                && ((other.Role != RoleTypeId.ClassD
                && other.Role != RoleTypeId.Scientist)
                || (other.CurrentItem != null
                && (other.CurrentItem.Category == ItemCategory.Firearm
                || other.CurrentItem.Category == ItemCategory.SpecialWeapon)));
    }
}
