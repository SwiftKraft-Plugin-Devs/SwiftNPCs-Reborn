using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MapGeneration.RoomConnectors;
using MEC;
using SwiftNPCs.Commands;
using SwiftNPCs.Features;
using SwiftNPCs.NavGeometry;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Logger = LabApi.Features.Console.Logger;
using Object = UnityEngine.Object;

namespace SwiftNPCs
{
    public class Core : Plugin
    {
        public static LayerMask NavMeshLayers = LayerMask.GetMask("Default", "InvisibleCollider", "Fence");

        public static Version CurrentVersion = new(2, 0, 0, 0);

        public static Core Instance { get; private set; }

        public override string Name => "SwiftNPCs Reborn";

        public override string Description => "A vanilla friendly NPCs mod for LabAPI.";

        public override string Author => "SwiftKraft";

        public override Version Version => CurrentVersion;

        public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);

        public override LoadPriority Priority => LoadPriority.Highest;

        public static NavMeshSurface NavMeshSurface = null;

        public static readonly List<PrimitiveObjectToy> TemporaryBlockouts = [];

        public override void Enable()
        {
            Instance = this;
            NPCParentCommand.SetPrompt();

            ServerEvents.MapGenerated += MapGenerated;
            PlayerEvents.ShotWeapon += OnPlayerShotWeapon;

            //HarmonyManager.Enable();
        }

        private static void BlockoutConnectorMeshes(SpawnableRoomConnector connector, params string[] bannedKeywords)
        {
            Logger.Info("Connector spawned: ");
            foreach (MeshCollider go in connector.GetComponentsInChildren<MeshCollider>())
                if (!ContainsName(go.name, bannedKeywords))
                {
                    PrimitiveObjectToy t = PrimitiveObjectToy.Create(go.bounds.center, Quaternion.identity, go.bounds.size, networkSpawn: false);
                    t.Base.NetworkPrimitiveType = PrimitiveType.Cube;
                    t.Spawn();
                    TemporaryBlockouts.Add(t);
                    Logger.Info(go.name);
                }
        }

        private static void RemoveBlockouts()
        {
            foreach (PrimitiveObjectToy toy in TemporaryBlockouts)
                toy.Destroy();
        }

        private static bool ContainsName(string target, params string[] cont)
        {
            foreach (string c in cont)
                if (target.Contains(c))
                    return true;
            return false;
        }

        private void MapGenerated(MapGeneratedEventArgs ev)
        {
            Timing.CallDelayed(0.25f, static () =>
            {
                NavGeometryManager.LoadNavGeometry();
                foreach (SpawnableRoomConnector conn in Object.FindObjectsByType<SpawnableRoomConnector>(FindObjectsSortMode.None))
                    BlockoutConnectorMeshes(conn, "Doorframe", "Doors", "Collider");
                BuildNavMesh();
                NavGeometryManager.RemoveNavGeometry();
                RemoveBlockouts();
            });
        }

        internal void OnPlayerShotWeapon(PlayerShotWeaponEventArgs ev)
        {
            RaycastHit[] arr = Physics.RaycastAll(ev.Player.Camera.position, ev.Player.Camera.forward, 20f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            string text = "Hits:";
            foreach (RaycastHit hit in arr)
                text += "\n" + hit.distance + "m - " + hit.transform.gameObject.name + " - " + LayerMask.LayerToName(hit.transform.gameObject.layer);

            Logger.Info(text);
            ev.Player.SendHint(text);
        }

        public static void BuildNavMesh()
        {
            if (NavMeshSurface != null)
            {
                NavMeshSurface.RemoveData();
                Object.Destroy(NavMeshSurface.gameObject);
            }

            NavMeshSurface = new GameObject("NavMesh").AddComponent<NavMeshSurface>();

            NavMeshBuildSettings settings = NavMeshSurface.GetBuildSettings();

            settings.agentClimb = 0.21f;
            settings.agentHeight = 0.83f;
            settings.agentSlope = 45f;
            settings.agentRadius = 0.25f;

            NavMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            NavMeshSurface.collectObjects = CollectObjects.All;
            NavMeshSurface.layerMask = NavMeshLayers;
            NavMeshSurface.overrideVoxelSize = true;
            NavMeshSurface.voxelSize = 0.05f;
            NavMeshSurface.overrideTileSize = true;
            NavMeshSurface.tileSize = 128;

            NavMeshSurface.BuildNavMesh();

            Logger.Info("NavMesh Built! ");
        }

        public override void Disable()
        {
            NPCManager.RemoveAll();
            ServerEvents.MapGenerated -= MapGenerated;
            PlayerEvents.ShotWeapon -= OnPlayerShotWeapon;

            //HarmonyManager.Disable();
        }
    }
}
