using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace SwiftNPCs.Features.Components
{
    public class NPCScanner : NPCComponent
    {
        public static LayerMask SightLayers = LayerMask.GetMask("Default");

        public override void Begin()
        {

        }

        public override void Tick()
        {

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
