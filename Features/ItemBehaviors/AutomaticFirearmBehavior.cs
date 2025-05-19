using InventorySystem.Items.Firearms.Modules;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace SwiftNPCs.Features.ItemBehaviors
{
    [ItemBehavior(ItemType.GunA7, ItemType.GunAK, ItemType.GunCom45, ItemType.GunCOM15, ItemType.GunCOM18, ItemType.GunFSP9, ItemType.GunE11SR, ItemType.GunLogicer, ItemType.GunFRMG0, ItemType.GunCrossvec)]
    public class AutomaticFirearmBehavior : FirearmBehaviorBase<AutomaticActionModule>
    {
        public AnimatorReloaderModuleBase Reloader { get; private set; }
        public IPrimaryAmmoContainerModule Ammo { get; private set; }

        public bool CanShoot => !Item.AnyModuleBusy(Module) && Module.Cocked && !Module.BoltLocked;

        private readonly Timer curTimer = new(0f);
        private readonly Timer reloadTimer = new(0f);

        public override void Begin()
        {
            base.Begin();
            curTimer.MaxTime = 1f / Module.DisplayCyclicRate;

            if (Item.TryGetModule(out AnimatorReloaderModuleBase mod1, false))
                Reloader = mod1;
            if (Item.TryGetModule(out IPrimaryAmmoContainerModule mod2))
                Ammo = mod2;
        }

        public override void End() { }

        public override void Tick()
        {
            curTimer.Tick(Time.fixedDeltaTime);

            if (!Reloader.IsReloading && User.Core.HasTarget && User.Core.Scanner.HasLOS(User.Core.Target, out Vector3 sight))
            {
                User.Core.Motor.WishLookPosition = sight;
                Shoot();
            }

            if (Ammo.AmmoStored <= 0)
                Reload();
        }

        public override void Shoot()
        {
            if (!CanShoot || Item.PrimaryActionBlocked || !curTimer.Ended)
                return;

            curTimer.Reset();
            Module.InvokePrivate(nameof(Module.ServerShoot), [null]);
        }

        public override void Reload()
        {
            if (Reloader.IsReloading)
                return;

            Item.DummyEmulator.AddEntry(ActionName.Reload, true);
        }
    }
}
