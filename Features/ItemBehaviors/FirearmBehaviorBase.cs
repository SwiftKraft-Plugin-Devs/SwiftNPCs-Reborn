using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;

namespace SwiftNPCs.Features.ItemBehaviors
{
    public abstract class FirearmBehaviorBase<T> : ItemBehaviorBase<Firearm> where T : ModuleBase
    {
        public T Module { get; private set; }
        public override void Begin()
        {
            if (Item.TryGetModule(out T mod0))
                Module = mod0;
        }

        public abstract void Shoot();

        public abstract void Reload();
    }
}
