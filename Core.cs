using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using SwiftNPCs.Commands;
using SwiftNPCs.Features;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Logger = LabApi.Features.Console.Logger;

namespace SwiftNPCs
{
    public class Core : Plugin
    {
        public static LayerMask NavMeshLayers = LayerMask.GetMask("Default");

        public static Version CurrentVersion = new(2, 0, 0, 0);

        public static List<IEvents> Events = [];

        public static Core Instance { get; private set; }

        public override string Name => "SwiftNPCs Reborn";

        public override string Description => "A vanilla friendly NPCs mod for LabAPI.";

        public override string Author => "SwiftKraft";

        public override Version Version => CurrentVersion;

        public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);

        public override LoadPriority Priority => LoadPriority.Highest;

        public static NavMeshSurface NavMesh;

        public override void Enable()
        {
            Instance = this;
            NPCParentCommand.SetPrompt();

            ServerEvents.MapGenerated += OnMapGenerated;
            PlayerEvents.ShotWeapon += OnPlayerShotWeapon;
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

        private void OnMapGenerated(MapGeneratedEventArgs ev)
        {
            NavMesh = new GameObject("NavMesh").AddComponent<NavMeshSurface>();

            NavMesh.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            NavMesh.collectObjects = CollectObjects.All;
            NavMesh.buildHeightMesh = true;
            NavMesh.layerMask = NavMeshLayers;
            NavMesh.BuildNavMesh();

            Logger.Info("NavMesh Built! ");
        }

        public override void Disable()
        {
            foreach (IEvents e in Events)
                e.Unsubscribe();

            NPCManager.RemoveAll();
            ServerEvents.MapGenerated -= OnMapGenerated;
            PlayerEvents.ShotWeapon -= OnPlayerShotWeapon;
        }
    }

    public interface IEvents
    {
        void Unsubscribe();
    }

    public abstract class EventClassBase : IEvents
    {
        public EventClassBase() => Core.Events.Add(this);

        public abstract void Unsubscribe();

        public void Remove() => Core.Events.Remove(this);
    }
}
