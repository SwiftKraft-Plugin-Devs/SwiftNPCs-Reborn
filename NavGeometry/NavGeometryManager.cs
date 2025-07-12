using LabApi.Features.Wrappers;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using MapGeneration;
using SwiftNPCs.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Utf8Json;

namespace SwiftNPCs.NavGeometry
{
    public static class NavGeometryManager
    {
        public static string DirectoryPath => Path.Combine(Core.Instance.GetConfigDirectory(false).FullName, "Navigation Geometry");

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

        public struct SavedRoom
        {
            public FacilityZone Zone;
            public RoomShape Shape;
            public RoomName Name;

            public SavedPrimitive[] Primitives;

            public static implicit operator SavedRoom(Room room) => new() { Zone = room.Zone, Shape = room.Shape, Name = room.Name, Primitives = NavGeometry.ContainsKey(room) ? [.. Convert(NavGeometry[room])] : null };
        }

        public static List<SavedPrimitive> Convert(List<PrimitiveObjectToy> toys)
        {
            List<SavedPrimitive> res = [];
            for (int i = 0; i < toys.Count; i++)
                res[i] = toys[i];
            return res;
        }

        public static PrimitiveObjectToy Spawn(Room room, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (!NavGeometry.ContainsKey(room))
                NavGeometry.Add(room, []);

            PrimitiveObjectToy toy = PrimitiveObjectToy.Create(pos, rot, scale, networkSpawn: false);
            toy.Base.NetworkPrimitiveType = PrimitiveType.Cube;
            toy.Spawn();
            NavGeometry[room].Add(toy);
            return toy;
        }

        public static void SaveNavGeometry(Room room)
        {
            if (!NavGeometry.ContainsKey(room))
                return;

            SavedRoom ro = room;

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            File.WriteAllBytes(Path.Combine(DirectoryPath, ro.Zone + "." + ro.Shape + "." + ro.Name + ".json"), JsonSerializer.Serialize(ro));
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

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            string path = Path.Combine(DirectoryPath, room.Zone + "." + room.Shape + "." + room.Name + ".json");

            if (!File.Exists(path))
                return;

            SavedRoom ro = JsonSerializer.Deserialize<SavedRoom>(File.ReadAllText(path));

            List<PrimitiveObjectToy> prims = [];

            foreach (SavedPrimitive prim in ro.Primitives)
                prims.Add(SavedPrimitive.Spawn(prim));

            NavGeometry.Add(room, prims);
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
