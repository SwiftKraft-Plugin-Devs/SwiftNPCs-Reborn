﻿using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace SwiftNPCs.Features.ItemBehaviors
{
    [ItemBehavior(ItemType.GunA7, ItemType.GunAK, ItemType.GunCom45, ItemType.GunFSP9, ItemType.GunE11SR, ItemType.GunLogicer, ItemType.GunFRMG0, ItemType.GunCrossvec)]
    public class AutomaticFirearmBehavior : FirearmBehaviorBase<AutomaticActionModule>
    {
        public bool CanShoot => !Item.AnyModuleBusy(Module) && Module.Cocked && !Module.BoltLocked;

        private readonly Timer curTimer = new(0f);

        public override void Begin()
        {
            base.Begin();
            curTimer.MaxTime = 1f / Module.DisplayCyclicRate;
        }

        public override void End() { }

        public override void Tick()
        {
            curTimer.Tick(Time.fixedDeltaTime);

            if (User.Core.HasTarget)
            {
                User.Core.Motor.WishLookPosition = User.Core.Targets[0].HitPosition;
                Shoot();
            }
        }

        public override void Shoot()
        {
            if (!CanShoot || !curTimer.Ended)
                return;

            curTimer.Reset();
            Module.InvokePrivate(nameof(Module.ServerShoot), [null]);
        }
    }
}
