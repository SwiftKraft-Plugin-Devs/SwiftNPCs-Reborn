using InventorySystem.Items;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SwiftNPCs.Features.ItemBehaviors
{
    public abstract class ItemBehaviorBase
    {
        public static readonly Dictionary<ItemType, Type> CorrespondingItemBehaviors = [];

        static ItemBehaviorBase()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof(ItemBehaviorAttribute), false);
                    if (attribs != null && attribs.Length > 0)
                    {
                        ItemBehaviorAttribute attr = (ItemBehaviorAttribute)attribs[0];
                        foreach (ItemType it in attr.ItemTypes)
                            CorrespondingItemBehaviors.Add(it, type);
                    }
                }
            }
        }

        public abstract void Tick();

        public abstract void Begin();

        public abstract void End();

        public static implicit operator ItemBehaviorBase(ItemBase item)
        {
            if (!CorrespondingItemBehaviors.TryGetValue(item.ItemTypeId, out Type type))
                return null;

            ItemBehaviorBase bb = (ItemBehaviorBase)Activator.CreateInstance(type, [item]);
            return bb;
        }
    }

    public abstract class ItemBehaviorBase<T>(T item) : ItemBehaviorBase where T : ItemBase
    {
        public T Item { get; private set; } = item;

        public static implicit operator T(ItemBehaviorBase<T> behavior) => behavior.Item;
    }
}
