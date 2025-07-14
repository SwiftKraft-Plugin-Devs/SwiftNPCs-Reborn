using LabApi.Features.Wrappers;
using LabApi.Loader;
using MapGeneration;
using MapGeneration.RoomConnectors;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Utf8Json;
using Logger = LabApi.Features.Console.Logger;

namespace SwiftNPCs.NavGeometry
{
    public static class NavGeometryManager
    {
        public static string DirectoryPath => Path.Combine(Core.Instance.GetConfigDirectory(false).FullName, "Navigation Geometry");

        public static readonly Dictionary<Room, List<PrimitiveObjectToy>> NavGeometry = [];

        public static readonly List<PrimitiveObjectToy> TemporaryBlockouts = [];

        public struct SavedPrimitive
        {
            public PrimitiveType Type;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;

            public static PrimitiveObjectToy Spawn(Room r, SavedPrimitive prim) => SpawnPrim(prim.Type, r.Rotation * prim.Position + r.Position, prim.Rotation * r.Rotation, prim.Scale);

            public static SavedPrimitive Convert(Room r, PrimitiveObjectToy toy) => new() { Type = toy.Type, Position = Quaternion.Inverse(r.Rotation) * (toy.Position - r.Position), Rotation = Quaternion.Inverse(r.Rotation) * toy.Rotation, Scale = toy.Scale };
        }

        public struct SavedRoom
        {
            public FacilityZone Zone;
            public RoomShape Shape;
            public RoomName Name;

            public SavedPrimitive[] Primitives;

            public static implicit operator SavedRoom(Room room) => new() { Zone = room.Zone, Shape = room.Shape, Name = room.Name, Primitives = NavGeometry.ContainsKey(room) ? [.. Convert(room, NavGeometry[room])] : null };
        }

        private static bool ContainsName(string target, params string[] cont)
        {
            foreach (string c in cont)
                if (target.Contains(c))
                    return true;
            return false;
        }

        public static void RemoveBlockouts()
        {
            foreach (PrimitiveObjectToy toy in TemporaryBlockouts)
                toy.Destroy();
        }

        public static void BlockoutConnectorMeshes(SpawnableRoomConnector connector, params string[] bannedKeywords)
        {
            Logger.Info("Connector spawned: ");
            foreach (MeshCollider go in connector.GetComponentsInChildren<MeshCollider>())
                if (!ContainsName(go.name, bannedKeywords))
                {
                    PrimitiveObjectToy t = SpawnPrim(PrimitiveType.Cube, go.bounds.center, Quaternion.identity, go.bounds.size);
                    TemporaryBlockouts.Add(t);
                    Logger.Info(go.name);
                }
        }

        public static void BlockoutConnectors()
        {
            foreach (SpawnableRoomConnector conn in Object.FindObjectsByType<SpawnableRoomConnector>(FindObjectsSortMode.None))
                BlockoutConnectorMeshes(conn, "Doorframe", "Doors", "Collider");
        }

        public static List<SavedPrimitive> Convert(Room r, List<PrimitiveObjectToy> toys)
        {
            if (toys.Count == 0) return [];

            List<SavedPrimitive> res = [];
            for (int i = 0; i < toys.Count; i++)
                if (toys[i] != null && toys[i].Base != null)
                    res.Add(SavedPrimitive.Convert(r, toys[i]));
            return res;
        }

        public static PrimitiveObjectToy SpawnPrim(PrimitiveType type, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            PrimitiveObjectToy toy = PrimitiveObjectToy.Create(pos, rot, scale, networkSpawn: false);
            toy.Base.NetworkPrimitiveType = type;
            toy.Color = new Color(1f, 0f, 0f, 0.85f);
            toy.Spawn();
            return toy;
        }

        public static PrimitiveObjectToy Spawn(Room room, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (room == null) return null;

            if (!NavGeometry.ContainsKey(room))
                NavGeometry.Add(room, []);

            PrimitiveObjectToy toy = SpawnPrim(PrimitiveType.Cube, room.Position + room.Rotation * pos, room.Rotation * rot, scale);
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

            SavedRoom ro = JsonSerializer.Deserialize<SavedRoom>(File.ReadAllBytes(path));

            List<PrimitiveObjectToy> prims = [];

            foreach (SavedPrimitive prim in ro.Primitives)
                prims.Add(SavedPrimitive.Spawn(room, prim));

            NavGeometry.Add(room, prims);

            Logger.Info("Loaded NavGeometry: " + room.Name);
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
