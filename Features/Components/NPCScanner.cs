using LabApi.Features.Wrappers;
using PlayerRoles;
using SwiftNPCs.Features.Targettables;
using UnityEngine;

namespace SwiftNPCs.Features.Components
{
    public class NPCScanner : NPCComponent
    {
        public static LayerMask SightLayers = LayerMask.GetMask("Default");

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
            foreach (Player p in Player.List)
                if ((p.Position - Core.Position).sqrMagnitude <= CurrentRange * CurrentRange && HasLOS(p, out _) && Core.NPC.WrapperPlayer.IsEnemy(p))
                    Core.AddTarget<TargetablePlayer, Player>(p);
            Core.Targets.RemoveAll((t) => !HasLOS(t, out _) || t is TargetablePlayer p && (!Core.NPC.WrapperPlayer.IsEnemy(p.Target) || (p.HitPosition - Core.Position).sqrMagnitude > CurrentRange * CurrentRange));
        }

        public bool HasLOS(Player player, out Vector3 sight) => CheckLOS(Core.NPC.WrapperPlayer.Camera.position, out sight, player.Camera.position, player.Position);

        public bool HasLOS(TargetableBase t, out Vector3 sight) => CheckLOS(Core.NPC.WrapperPlayer.Camera.position, out sight, t.CriticalPosition, t.HitPosition);

        public static bool CheckLOS(Vector3 cam, out Vector3 sight, params Vector3[] others)
        {
            foreach (Vector3 pos in others)
                if (!Physics.Linecast(cam, pos, SightLayers, QueryTriggerInteraction.Ignore))
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
                other.IsAlive
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
