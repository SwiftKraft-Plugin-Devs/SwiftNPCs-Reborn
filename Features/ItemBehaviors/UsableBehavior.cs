using InventorySystem.Items.Usables;

namespace SwiftNPCs.Features.ItemBehaviors
{
    public class UsableBehavior : ItemBehaviorBase<UsableItem>
    {
        bool started;

        public override void Begin() { }

        public override void End() { }

        public override void Tick()
        {
            if (!started && Item.CanStartUsing && !Item.IsUsing)
            {
                StartUsing();
                started = true;
            }
        }

        public void StartUsing() => UsableItemsController.ServerEmulateMessage(Item.ItemSerial, StatusMessage.StatusType.Start);

        public void CancelUsing() => UsableItemsController.ServerEmulateMessage(Item.ItemSerial, StatusMessage.StatusType.Cancel);
    }
}
