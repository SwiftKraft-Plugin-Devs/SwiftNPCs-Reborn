using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using SwiftNPCs.Features.ItemBehaviors;

namespace SwiftNPCs.Features.Components
{
    public class NPCItemUser : NPCComponent
    {
        public ItemBehaviorBase CurrentItemBehavior
        {
            get => _currentItemBehavior;
            set
            {
                _currentItemBehavior?.End();
                _currentItemBehavior = value;
                _currentItemBehavior?.Begin();
            }
        }
        ItemBehaviorBase _currentItemBehavior;

        public bool CanUse = true;

        public override void Begin()
        {
            PlayerEvents.ChangedItem += ChangedItem;
            CurrentItemBehavior = this.GetRandomBehavior(Core.Inventory.CurrentItem);
        }

        public override void Tick()
        {
            if (CanUse)
                CurrentItemBehavior?.Tick();
        }

        public override void Close()
        {
            base.Close();
            PlayerEvents.ChangedItem -= ChangedItem;
        }

        public virtual void ChangedItem(PlayerChangedItemEventArgs ev)
        {
            if (ev.Player != Core.NPC.WrapperPlayer)
                return;

            CurrentItemBehavior = this.GetRandomBehavior(ev.NewItem);
        }
    }
}
