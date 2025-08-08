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

        public float MinSprayTime = 0.5f;
        public float MaxSprayTime = 1.5f;

        public float MinSprayCooldown = 0.2f;
        public float MaxSprayCooldown = 0.6f;

        public float HipfireDistance = Random.Range(3f, 6f);

        public float MaxInaccuracy = 1.5f;
        public float MinInaccuracyDistance = 7f;
        public float MaxInaccuracyDistance = 30f;

        public float ShootDotProduct = 0.75f;

        private readonly Timer tacticalReloadTimer = new(4f);
        private readonly Timer sprayTimer = new(2.5f);
        private readonly Timer sprayCooldownTimer = new(1f);
        private bool attacking;
        private bool aiming;

        private Vector3 offset;

        public override void Begin()
        {
            if (Item.TryGetModule(out IReloaderModule mod1))
                Reloader = mod1;
            if (Item.TryGetModule(out IPrimaryAmmoContainerModule mod2))
                Ammo = mod2;

            offset = Random.insideUnitSphere * MaxInaccuracy;
        }

        public override void End() { }

        public override void Tick()
        {
            if (!Reloader.IsReloading)
            {
                float targetDistance = User.Core.TargetDistance;
                bool targetFar = targetDistance >= HipfireDistance;
                Aiming = targetFar;

                if (User.Core.HasTarget && User.Core.Scanner.HasLOS(User.Core.Target, out Vector3 sight, true))
                {
                    tacticalReloadTimer.Reset();
                    User.Core.Motor.WishLookPosition = sight + Vector3.Lerp(default, offset, Mathf.InverseLerp(MinInaccuracyDistance, MaxInaccuracyDistance, targetDistance));

                    if (targetFar)
                    {
                        if (!sprayCooldownTimer.Ended)
                        {
                            Attacking = false;
                            sprayCooldownTimer.Tick(Time.fixedDeltaTime);
                        }
                        else
                        {
                            sprayTimer.Tick(Time.fixedDeltaTime);

                            if (sprayTimer.Ended)
                            {
                                sprayTimer.Reset(Random.Range(MinSprayTime, MaxSprayTime));
                                sprayCooldownTimer.Reset(Random.Range(MinSprayCooldown, MaxSprayCooldown));
                                offset = Random.insideUnitSphere * MaxInaccuracy;
                            }
                            else if (User.Core.GetDotProduct(sight) >= ShootDotProduct)
                                Shoot();
                        }
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
