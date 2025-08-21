using InventorySystem.Items.Firearms;

namespace SwiftNPCs.Features.ItemBehaviors
{
    public abstract class FirearmBehaviorBase : ItemBehaviorBase<Firearm>
    {
        public abstract void Shoot();
        public abstract void Reload();
    }
}
