using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace SwiftNPCs.Features.ItemBehaviors
{
    [ItemBehavior(ItemType.GunA7, ItemType.GunAK, ItemType.GunCom45, ItemType.GunFSP9, ItemType.GunE11SR, ItemType.GunLogicer, ItemType.GunFRMG0, ItemType.GunCrossvec)]
    public class AutomaticFirearmBehavior(Firearm item) : FirearmBehaviorBase<AutomaticActionModule>(item)
    {
        public bool CanShoot => !Item.AnyModuleBusy(Module) && Module.Cocked && !Module.BoltLocked;

        private readonly Timer curTimer = new(0f);

        public override void Begin()
        {
            base.Begin();
            curTimer.MaxTime = Module.TimeBetweenShots;
        }

        public override void End() { }

        public override void Tick()
        {
            curTimer.Tick(Time.fixedDeltaTime);
        }

        public override void Shoot()
        {
            if (!CanShoot || !curTimer.Ended)
                return;

            curTimer.Reset();
            Module.ServerShoot(null);
        }
    }
}
