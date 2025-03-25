using InventorySystem;
using InventorySystem.Items;

namespace SwiftNPCs.Features.Components
{
    public class NPCInventory : NPCComponent
    {
        public Inventory Inventory => Core.NPC.WrapperPlayer.Inventory;

        public ItemBase CurrentItem => Inventory.CurInstance;

        #region Overrides

        public override void Begin() { }

        public override void Tick() { }

        #endregion

        #region HasItem

        public bool HasItem(out ItemBase item, out ushort slot, params ItemCategory[] categories)
        {
            foreach (ItemCategory category in categories)
                if (HasItem(category, out item, out slot))
                    return true;

            item = null;
            slot = ushort.MaxValue;
            return false;
        }

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

        public void EquipItem(ItemBase item) => EquipItem(item.ItemSerial);
        public void EquipItem(ushort itemSerial) => Inventory.ServerSelectItem(itemSerial);
        public void UnequipItem() => EquipItem(0);

        #endregion
    }
}
