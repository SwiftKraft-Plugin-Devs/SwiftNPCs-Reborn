using LabApi.Features.Wrappers;
using SwiftNPCs.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utf8Json;

namespace SwiftNPCs.NavGeometry
{
    public static class NavGeometryManager
    {
        public static readonly Dictionary<Room, List<PrimitiveObjectToy>> NavGeometry = [];

        public struct SavedPrimitive
        {
            public PrimitiveType Type;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;

            public static PrimitiveObjectToy Spawn(SavedPrimitive prim)
            {
                PrimitiveObjectToy toy = PrimitiveObjectToy.Create(prim.Position, prim.Rotation, prim.Scale, networkSpawn: false);
                toy.Base.NetworkPrimitiveType = prim.Type;
                toy.Spawn();
                return toy;
            }

            public static implicit operator SavedPrimitive(PrimitiveObjectToy toy) => new() { Type = toy.Type, Position = toy.Position, Rotation = toy.Rotation, Scale = toy.Scale };
        }

        public static List<SavedPrimitive> Convert(List<PrimitiveObjectToy> toys)
        {
            List<SavedPrimitive> res = [];
            for (int i = 0; i < toys.Count; i++)
                res[i] = toys[i];
            return res;
        }

        public static void SaveNavGeometry(Room room)
        {
            if (!NavGeometry.ContainsKey(room))
                return;

            JsonSerializer.Serialize(Convert(NavGeometry[room]));
        }

        public static void SaveNavGeometry()
        {
            foreach (Room r in NavGeometry.Keys)
                SaveNavGeometry(r);
        }

        public static void LoadNavGeometry(Room room)
        {
            if (NavGeometry.ContainsKey(room))
                RemoveNavGeometry(room);
        }

        public static void LoadNavGeometry()
        {
            foreach (Room r in Room.List.Where(static r => r != null && r.Base != null && r.Base.enabled && !r.IsDestroyed))
                LoadNavGeometry(r);
        }

        public static void RemoveNavGeometry(Room room)
        {
            if (!NavGeometry.ContainsKey(room))
                return;

            for (int i = 0; i < NavGeometry[room].Count; i++)
                NavGeometry[room][i].Destroy();

            NavGeometry.Remove(room);
        }

        public static void RemoveNavGeometry()
        {
            foreach (Room r in Room.List.Where(static r => r != null && r.Base != null && r.Base.enabled && !r.IsDestroyed))
                RemoveNavGeometry(r);
        }
    }
}
