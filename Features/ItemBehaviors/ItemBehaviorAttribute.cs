using System;

namespace SwiftNPCs.Features.ItemBehaviors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ItemBehaviorAttribute(params ItemType[] type) : Attribute
    {
        public readonly ItemType[] ItemTypes = type;
    }
}
