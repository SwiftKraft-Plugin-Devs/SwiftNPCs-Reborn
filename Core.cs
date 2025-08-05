using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MEC;
using SwiftNPCs.Commands;
using SwiftNPCs.Features;
using SwiftNPCs.NavGeometry;
using SwiftNPCs.NavGeometry.EditModes;
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
            //PlayerEvents.ShotWeapon += OnPlayerShotWeapon;

            NavGeometryEditor.Enable();

            NavGeometryEditor.RegisterEditMode<Create>();
            NavGeometryEditor.RegisterEditMode<Delete>();
            NavGeometryEditor.RegisterEditMode<NavGeometry.EditModes.Rotate>();
            NavGeometryEditor.RegisterEditMode<Move>();

            //HarmonyManager.Enable();
        }

        private void MapGenerated(MapGeneratedEventArgs ev) => Timing.CallDelayed(0.25f, BuildNavMesh);

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
            NavGeometryManager.LoadNavGeometry();
            NavGeometryManager.BlockoutConnectors();

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
            settings.agentRadius = 0.22f;

            NavMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            NavMeshSurface.collectObjects = CollectObjects.All;
            NavMeshSurface.layerMask = NavMeshLayers;
            NavMeshSurface.overrideVoxelSize = true;
            NavMeshSurface.voxelSize = 0.05f;
            NavMeshSurface.overrideTileSize = true;
            NavMeshSurface.tileSize = 128;

            NavMeshSurface.BuildNavMesh();

            Logger.Info("NavMesh Built! ");

            Timing.CallDelayed(0.5f, () => {
                NavGeometryManager.RemoveNavGeometry();
                NavGeometryManager.RemoveBlockouts();
            });
        }

        public override void Disable()
        {
            NPCManager.RemoveAll();
            ServerEvents.MapGenerated -= MapGenerated;
            //PlayerEvents.ShotWeapon -= OnPlayerShotWeapon;

            NavGeometryEditor.Disable();

            //HarmonyManager.Disable();
        }
    }
}
