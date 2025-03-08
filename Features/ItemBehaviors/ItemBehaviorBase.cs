using InventorySystem.Items;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SwiftNPCs.Features.ItemBehaviors
{
    public abstract class ItemBehaviorBase
    {
        public static readonly Dictionary<ItemType, List<Type>> CorrespondingItemBehaviors = [];

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
                        {
                            if (!CorrespondingItemBehaviors.ContainsKey(it))
                                CorrespondingItemBehaviors.Add(it, []);
                            CorrespondingItemBehaviors[it].Add(type);
                        }
                    }
                }
            }
        }

        public abstract void Tick();

        public abstract void Begin();

        public abstract void End();

        public static implicit operator ItemBehaviorBase(ItemType item)
        {
            if (!CorrespondingItemBehaviors.TryGetValue(item, out List<Type> type))
                return null;

            ItemBehaviorBase bb = (ItemBehaviorBase)Activator.CreateInstance(type[Random.Range(0, type.Count)], [item]);
            return bb;
        }

        public static implicit operator ItemBehaviorBase(ItemBase item) => item.ItemTypeId;
        public static implicit operator ItemBehaviorBase(Item item) => item.Type;
    }

    public abstract class ItemBehaviorBase<T>(T item) : ItemBehaviorBase where T : ItemBase
    {
        public T Item { get; private set; } = item;

        public static implicit operator T(ItemBehaviorBase<T> behavior) => behavior.Item;
    }
}
