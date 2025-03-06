using InventorySystem.Items;

namespace SwiftNPCs.Features.ItemBehaviors
{
    public abstract class ItemBehaviorBase
    {
        public abstract void Tick();

        public abstract void Begin();

        public abstract void End();
    }

    public abstract class ItemBehaviorBase<T>(T item) where T : ItemBase
    {
        public T Item { get; private set; } = item;
    }
}
