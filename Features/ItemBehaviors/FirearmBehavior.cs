using InventorySystem.Items.Firearms;

namespace SwiftNPCs.Features.ItemBehaviors
{
    [ItemBehavior(ItemType.GunA7, ItemType.GunAK, ItemType.GunCom45, ItemType.GunFSP9, ItemType.GunE11SR, ItemType.GunRevolver, ItemType.GunLogicer, ItemType.GunFRMG0, ItemType.GunCrossvec, ItemType.GunShotgun)]
    public class FirearmBehavior(Firearm item) : ItemBehaviorBase<Firearm>(item)
    {
        public override void Begin()
        {

        }

        public override void End()
        {

        }

        public override void Tick()
        {

        }
    }
}
