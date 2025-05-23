using InventorySystem.Items.Firearms.Modules;
using LabApi.Features.Console;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace SwiftNPCs.Features.ItemBehaviors
{
    [ItemBehavior(ItemType.GunA7, ItemType.GunAK, ItemType.GunCom45, ItemType.GunCOM15, ItemType.GunCOM18, ItemType.GunFSP9, ItemType.GunE11SR, ItemType.GunLogicer, ItemType.GunFRMG0, ItemType.GunCrossvec)]
    public class AutomaticFirearmBehavior : FirearmBehaviorBase<AutomaticActionModule>
    {
        public IReloaderModule Reloader { get; private set; }
        public IPrimaryAmmoContainerModule Ammo { get; private set; }

        public bool CanShoot = true;

        readonly Timer tacticalReloadTimer = new(4f);

        public override void Begin()
        {
            if (Item.TryGetModule(out IReloaderModule mod1))
                Reloader = mod1;
            if (Item.TryGetModule(out IPrimaryAmmoContainerModule mod2))
                Ammo = mod2;
        }

        public override void End() { }

        public override void Tick()
        {
            if (!Reloader.IsReloading)
            {
                if (User.Core.HasTarget && User.Core.Scanner.HasLOS(User.Core.Target, out Vector3 sight))
                {
                    User.Core.Motor.WishLookPosition = sight;
                    Shoot();

                    tacticalReloadTimer.Reset();
                }
                else if (Ammo.AmmoStored < Ammo.AmmoMax)
                {
                    tacticalReloadTimer.Tick(Time.fixedDeltaTime);

                    if (tacticalReloadTimer.Ended)
                    {
                        tacticalReloadTimer.Reset();
                        Reload();
                    }
                }
            }

            if (Ammo.AmmoStored <= 0)
                Reload();
        }

        public override void Shoot()
        {
            if (!CanShoot)
                return;

            Item.DummyEmulator.AddEntry(ActionName.Shoot, false);
        }

        public override void Reload()
        {
            if (Reloader.IsReloading)
                return;

            Item.DummyEmulator.AddEntry(ActionName.Reload, true);
        }
    }
}
