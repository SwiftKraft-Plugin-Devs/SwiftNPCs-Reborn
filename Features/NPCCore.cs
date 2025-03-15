using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Console;
using SwiftNPCs.Features.Components;
using SwiftNPCs.Features.Targettables;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace SwiftNPCs.Features
{
    public class NPCCore : MonoBehaviour
    {
        public const float InteractionDistance = 3f;
        public const float DoorDotMinimum = 0.1f;

        public NPC NPC { get; private set; }

        public readonly List<NPCComponent> Components = [];

        public Vector3 Position => NPC.WrapperPlayer.Position;

        public readonly List<TargetableBase> Targets = [];

        public NPCMotor Motor { get; private set; }
        public NPCPathfinder Pathfinder { get; private set; }
        public NPCItemUser ItemUser { get; private set; }
        public NPCScanner Scanner { get; private set; }

        public bool HasTarget => Targets.Count > 0;

        protected virtual void Update()
        {
            foreach (NPCComponent component in Components)
                component.Frame();
        }

        protected virtual void FixedUpdate()
        {
            foreach (NPCComponent component in Components)
                component.Tick();
        }

        protected virtual void OnDestroy()
        {
            foreach (NPCComponent component in Components)
                component.Close();
        }

        public void Setup(NPC npc)
        {
            NPC = npc;
            Motor = AddNPCComponent<NPCMotor>();
            Pathfinder = AddNPCComponent<NPCPathfinder>();
            ItemUser = AddNPCComponent<NPCItemUser>();
            Scanner = AddNPCComponent<NPCScanner>();
        }

        public void AddTarget<T1, T2>(T2 target) where T1 : TargetableBase<T2>, new()
        {
            foreach (TargetableBase targ in Targets)
                if (targ is TargetableBase<T2> ta && ta.Target.Equals(target))
                {
                    Targets.Sort();
                    return;
                }

            Logger.Info("A");

            T1 t = new() { Target = target, NPC = this };
            Targets.Add(t);
            Targets.Sort();
        }

        public T AddNPCComponent<T>() where T : NPCComponent, new()
        {
            T t = new()
            {
                Core = this
            };

            Components.Add(t);
            t.Begin();
            return t;
        }

        public T GetNPCComponent<T>() where T : NPCComponent => Components.FirstOrDefault(t => t is T) as T;

        public bool TryGetNPCComponent<T>(out T component) where T : NPCComponent
        {
            component = GetNPCComponent<T>();
            return component != null;
        }

        public bool TrySetDoor(DoorVariant door, bool state)
        {
            if (door is Interactables.Interobjects.CheckpointDoor checkpoint || door.TryGetComponentInParent(out checkpoint))
                door = checkpoint;

            float st = door.GetExactState();
            if (door.NetworkTargetState == state || (st > 0f && st < 1f))
                return false;

            if (door.NetworkTargetState != state)
                door.ServerInteract(NPC.ReferenceHub, 0);

            return true;
        }

        public DoorVariant GetDoor(out bool inVision)
        {
            DoorVariant door = null;
            float doorDist = Mathf.Infinity;
            foreach (DoorVariant d in DoorVariant.AllDoors)
            {
                if (d is BasicNonInteractableDoor)
                    continue;

                float dist = (d.transform.position - Position).sqrMagnitude;
                if (dist <= InteractionDistance * InteractionDistance && (door == null || dist < doorDist))
                {
                    door = d;
                    doorDist = dist;
                }
            }

            inVision = door == null || GetDotProduct(door.transform.position) >= DoorDotMinimum;
            return door;
        }

        public bool TryGetDoor(out DoorVariant door, out bool inVision)
        {
            door = GetDoor(out inVision);
            return door != null;
        }

        public float GetDotProduct(Vector3 position) => Vector3.Dot(Motor.CurrentLookRotation * Vector3.forward, (position - transform.position).normalized);
    }
}
