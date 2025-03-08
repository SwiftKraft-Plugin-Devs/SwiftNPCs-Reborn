using InventorySystem.Items;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Logger = LabApi.Features.Console.Logger;
using Random = UnityEngine.Random;

namespace SwiftNPCs.Features.ItemBehaviors
{
    public abstract class ItemBehaviorBase
    {
        public ItemBase Item { get; set; }

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
                            Logger.Info($"Adding Behavior \"{type.FullName}\" to Item \"{it.GetName()}\"");
                        }
                    }
                }
            }
        }

        public abstract void Tick();

        public abstract void Begin();

        public abstract void End();

        public static implicit operator ItemBehaviorBase(ItemBase item)
        {
            if (!CorrespondingItemBehaviors.TryGetValue(item.ItemTypeId, out List<Type> types))
                return null;

            ItemBehaviorBase bb = (ItemBehaviorBase)Activator.CreateInstance(types[Random.Range(0, types.Count)]);
            bb.Item = item;
            return bb;
        }
        public static implicit operator ItemBehaviorBase(Item item) => item.Base;

        public static implicit operator ItemBase(ItemBehaviorBase behavior) => behavior.Item;
    }

    public abstract class ItemBehaviorBase<T> : ItemBehaviorBase where T : ItemBase
    {
        public new T Item { get => base.Item is T t ? t : null; set => base.Item = value; }

        public static implicit operator T(ItemBehaviorBase<T> behavior) => behavior.Item;
    }
}
