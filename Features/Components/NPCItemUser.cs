using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using NetworkManagerUtils.Dummies;
using SwiftNPCs.Features.ItemBehaviors;
using System.Collections.Generic;

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

        public override void Begin() => PlayerEvents.ChangedItem += ChangedItem;

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

            List<DummyAction> list2 = DummyActionCollector.ServerGetActions(Core.NPC.ReferenceHub);
            foreach (DummyAction da in list2)
                Logger.Info(da.Name + ": " + da.Action.Method.Name + ", " + da.Action.Target);

            CurrentItemBehavior = ev.NewItem == null ? null : this.GetBehavior(ev.NewItem);
        }
    }
}
