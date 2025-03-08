using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;

namespace SwiftNPCs.Features.ItemBehaviors
{
    public abstract class FirearmBehaviorBase<T>(Firearm item) : ItemBehaviorBase<Firearm>(item) where T : ModuleBase
    {
        public AutomaticActionModule Module;

        public override void Begin() => Item.TryGetModule(out Module);

        public abstract void Shoot();
    }
}
