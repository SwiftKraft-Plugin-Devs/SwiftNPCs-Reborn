using InventorySystem;
using InventorySystem.Items;

namespace SwiftNPCs.Features.Components
{
    public class NPCInventory : NPCComponent
    {
        public Inventory Inventory => Core.NPC.WrapperPlayer.Inventory;

        #region Overrides

        public override void Begin() { }

        public override void Tick() { }

        #endregion

        #region HasItem

        public bool HasItem(ItemCategory category, out ItemBase item, out ushort slot)
        {
            foreach (ushort s in Inventory.UserInventory.Items.Keys)
            {
                if (Inventory.UserInventory.Items[s].Category == category)
                {
                    item = Inventory.UserInventory.Items[s];
                    slot = s;
                    return true;
                }
            }
            item = null;
            slot = ushort.MaxValue;
            return false;
        }

        public bool HasItem(ItemType type, out ItemBase item, out ushort slot)
        {
            foreach (ushort s in Inventory.UserInventory.Items.Keys)
            {
                if (Inventory.UserInventory.Items[s].ItemTypeId == type)
                {
                    item = Inventory.UserInventory.Items[s];
                    slot = s;
                    return true;
                }
            }
            item = null;
            slot = ushort.MaxValue;
            return false;
        }

        public bool HasItem(ItemBase itemBase, out ushort slot) => HasItem(itemBase.ItemSerial, out _, out slot);

        public bool HasItem(ushort itemSerial, out ItemBase item, out ushort slot)
        {
            foreach (ushort s in Inventory.UserInventory.Items.Keys)
            {
                if (Inventory.UserInventory.Items[s].ItemSerial == itemSerial)
                {
                    item = Inventory.UserInventory.Items[s];
                    slot = s;
                    return true;
                }
            }
            item = null;
            slot = ushort.MaxValue;
            return false;
        }

        #endregion
    }
}
