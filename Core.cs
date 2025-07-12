using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MapGeneration.RoomConnectors;
using SwiftNPCs.Commands;
using SwiftNPCs.Features;
using SwiftNPCs.NavGeometry;
using System;
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

        public override void Enable()
        {
            Instance = this;
            NPCParentCommand.SetPrompt();

            ServerEvents.MapGenerated += MapGenerated;
            PlayerEvents.ShotWeapon += OnPlayerShotWeapon;

            //HarmonyManager.Enable();
        }

        private void Test(SpawnableRoomConnector connector)
        {
            Logger.Info("Connector spawned: ");
            foreach (Collider go in connector.GetComponentsInChildren<MeshCollider>())
                Logger.Info($"{go.name}, collider: {go}");
        }

        private void MapGenerated(MapGeneratedEventArgs ev)
        {
            NavGeometryManager.LoadNavGeometry();
            BuildNavMesh();
            NavGeometryManager.RemoveNavGeometry();

            foreach (SpawnableRoomConnector conn in Object.FindObjectsByType<SpawnableRoomConnector>(FindObjectsSortMode.None))
                Test(conn);
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
