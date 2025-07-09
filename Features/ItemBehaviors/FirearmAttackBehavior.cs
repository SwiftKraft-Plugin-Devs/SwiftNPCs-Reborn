using InventorySystem.Items.Firearms.Modules;
using SwiftNPCs.Utils.Structures;
using UnityEngine;

namespace SwiftNPCs.Features.ItemBehaviors
{
    [ItemBehavior(ItemType.GunA7, ItemType.GunAK, ItemType.GunCom45, ItemType.GunCOM15, ItemType.GunCOM18, ItemType.GunFSP9, ItemType.GunE11SR, ItemType.GunLogicer, ItemType.GunFRMG0, ItemType.GunCrossvec, ItemType.GunRevolver)]
    public class FirearmAttackBehavior : FirearmBehaviorBase
    {
        public IReloaderModule Reloader { get; private set; }
        public IPrimaryAmmoContainerModule Ammo { get; private set; }

        public bool CanShoot = true;

        public bool Attacking
        {
            get => attacking;
            set
            {
                if (attacking == value)
                    return;

                attacking = value;
                if (attacking)
                {
                    Item.DummyEmulator.AddEntry(ActionName.Shoot, false);
                    Item.DummyEmulator.AddEntry(ActionName.Zoom, false);
                }
                else
                {
                    Item.DummyEmulator.RemoveEntry(ActionName.Shoot);
                    Item.DummyEmulator.RemoveEntry(ActionName.Zoom);
                }
            }
        }

        private readonly Timer tacticalReloadTimer = new(4f);
        private bool attacking;

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
                if (User.Core.HasTarget && User.Core.Scanner.HasLOS(User.Core.Target, out Vector3 sight, true))
                {
                    User.Core.Motor.WishLookPosition = sight;
                    Shoot();

                    tacticalReloadTimer.Reset();
                }
                else
                    Attacking = false;

                if (Ammo.AmmoStored < Ammo.AmmoMax)
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

            Attacking = true;
        }

        public override void Reload()
        {
            if (Reloader.IsReloading)
                return;

            Attacking = false;
            Item.DummyEmulator.AddEntry(ActionName.Reload, true);
        }
    }
}
