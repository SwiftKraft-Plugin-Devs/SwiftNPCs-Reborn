using InventorySystem.Items;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace SwiftNPCs.Features.Personalities
{
    public abstract class NPCPersonalityBase
    {
        public NPCCore Core { get; private set; }

        public RoleTypeId Role => Core.NPC.RoleBase.RoleTypeId;

        public Player WrapperPlayer => Core.NPC.WrapperPlayer;

        public bool IsMilitant => Role.GetTeam() == Team.FoundationForces || Role.GetTeam() == Team.ChaosInsurgency;
        public bool IsCivilian => Role.GetTeam() == Team.ClassD || Role.GetTeam() == Team.Scientists;
        public bool IsArmed => Core.Inventory.CurrentItem != null && (Core.Inventory.CurrentItem.Category == ItemCategory.Firearm || Core.Inventory.CurrentItem.Category == ItemCategory.SpecialWeapon || Core.Inventory.CurrentItem.Category == ItemCategory.Grenade);
        public bool HasWeapon => GetWeapon(out _, out _);
        public bool IsThreat => IsMilitant || IsArmed || WrapperPlayer.IsSCP;

        public bool GetWeapon(out ItemBase item, out ushort slot)
        {
            if (Core.Inventory != null)
                return Core.Inventory.HasItem(out item, out slot, ItemCategory.Firearm, ItemCategory.SpecialWeapon, ItemCategory.Grenade);

            item = null;
            slot = 0;
            return false;
        }

        public bool GetMedical(out ItemBase item, out ushort slot)
        {
            if (Core.Inventory != null)
                return Core.Inventory.HasItem(ItemCategory.Medical, out item, out slot)
            || Core.Inventory.HasItem(ItemCategory.SCPItem, out item, out slot)
            && item.ItemTypeId == ItemType.SCP500;

            item = null; 
            slot = 0; 
            return false;
        }

        public virtual void Init(NPCCore core) => Core = core;
        public abstract void Begin();
        public abstract void Tick();
        public abstract void End();
    }
}
