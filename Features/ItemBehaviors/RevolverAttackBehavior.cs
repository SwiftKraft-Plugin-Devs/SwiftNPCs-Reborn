using InventorySystem.Items.Firearms.Modules;
using SwiftNPCs.Utils.Structures;

namespace SwiftNPCs.Features.ItemBehaviors
{
    [ItemBehavior(ItemType.GunRevolver)]
    public class RevolverAttackBehavior : FirearmAttackBehavior
    {
        public override bool Attacking
        {
            get => base.Attacking;
            set
            {
                if (attacking == value)
                    return;

                attacking = value;
            }
        }

        DoubleActionModule doubleAction;
        readonly Timer doubleActionTimer = new();
        bool doubling;

        public override void Begin()
        {
            base.Begin();
            Item.TryGetSubcomponent(out doubleAction);

            if (doubleAction == null)
                return;

            doubleActionTimer.Reset(doubleAction.DoubleActionTime);
        }

        public override void Tick()
        {
            base.Tick();

            if (Attacking && doubleAction != null)
            {
                if (doubleAction.Cocked)
                {
                    doubleAction.Fire(null);
                    //SendCmd(DoubleActionModule.MessageType.CmdShoot);
                }
                else if (!doubling)
                {
                    doubling = true;
                    //SendCmd(DoubleActionModule.MessageType.StartPulling);
                }
                else
                {
                    doubleActionTimer.Tick(DeltaTime);
                    if (doubleActionTimer.Ended && doubling)
                    {
                        doubling = false;
                        doubleAction.Fire(null);
                        //SendCmd(DoubleActionModule.MessageType.CmdShoot);
                    }
                }

            }
        }

        //void SendCmd(DoubleActionModule.MessageType msg, Action<NetworkWriter> writerFunc = null)
        //{
        //    NetworkWriter writer;
        //    using (new AutosyncCmd(Item.ItemId, out writer))
        //    {
        //        writer.WriteSubheader(msg);
        //        writerFunc?.Invoke(writer);
        //    }
        //    doubleAction.InvokePrivate(nameof(doubleAction.ServerProcessCmd), new NetworkReader(writer));
        //}
    }
}
