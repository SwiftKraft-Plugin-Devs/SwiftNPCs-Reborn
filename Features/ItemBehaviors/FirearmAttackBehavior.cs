using InventorySystem.Items.Firearms.Modules;
using SwiftNPCs.Utils.Structures;
using UnityEngine;

namespace SwiftNPCs.Features.ItemBehaviors
{
    [ItemBehavior(ItemType.GunA7, ItemType.GunAK, ItemType.GunCom45, ItemType.GunCOM15, ItemType.GunCOM18, ItemType.GunFSP9, ItemType.GunE11SR, ItemType.GunLogicer, ItemType.GunFRMG0, ItemType.GunCrossvec, ItemType.GunRevolver, ItemType.GunSCP127)]
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
                    Item.DummyEmulator.AddEntry(ActionName.Shoot, false);
                else
                    Item.DummyEmulator.RemoveEntry(ActionName.Shoot);
            }
        }

        public bool Aiming
        {
            get => aiming;
            set
            {
                if (aiming == value)
                    return;

                aiming = value;
                if (aiming)
                    Item.DummyEmulator.AddEntry(ActionName.Zoom, false);
                else
                    Item.DummyEmulator.RemoveEntry(ActionName.Zoom);
            }
        }

        public float MinSprayTime = 2f;
        public float MaxSprayTime = 4f;

        public float MinSprayCooldown = 0.2f;
        public float MaxSprayCooldown = 1f;

        public float HipfireDistance = 5f;

        private readonly Timer tacticalReloadTimer = new(4f);
        private readonly Timer sprayTimer = new(2.5f);
        private readonly Timer sprayCooldownTimer = new(1f);
        private bool attacking;
        private bool aiming;

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
                bool targetFar = !User.Core.TargetWithinDistance(HipfireDistance);
                Aiming = Attacking && targetFar;

                if (User.Core.HasTarget && User.Core.Scanner.HasLOS(User.Core.Target, out Vector3 sight, true))
                {
                    tacticalReloadTimer.Reset();
                    User.Core.Motor.WishLookPosition = sight;

                    if (targetFar)
                    {
                        sprayTimer.Tick(Time.fixedDeltaTime);

                        if (sprayCooldownTimer.Ended)
                        {
                            sprayTimer.Reset(Random.Range(MinSprayTime, MaxSprayTime));
                            sprayCooldownTimer.Reset(Random.Range(MinSprayCooldown, MaxSprayCooldown));
                        }
                        else if (sprayTimer.Ended)
                            sprayCooldownTimer.Tick(Time.fixedDeltaTime);
                        else
                            Shoot();
                    }
                    else
                        Shoot();
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
