﻿using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using SwiftNPCs.Features.Components;
using SwiftNPCs.Features.Personalities;
using SwiftNPCs.Features.Targettables;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwiftNPCs.Features
{
    public class NPCCore : MonoBehaviour
    {
        public const float InteractionDistance = 3f;
        public const float DoorDotMinimum = 0.1f;

        public NPCPersonalityBase Personality { get; private set; }

        public NPC NPC { get; private set; }

        public readonly List<NPCComponent> Components = [];

        public Vector3 Position { get => NPC.Position; set => NPC.Position = value; }

        public TargetableBase Target;

        public NPCMotor Motor { get; private set; }
        public NPCPathfinder Pathfinder { get; private set; }
        public NPCItemUser ItemUser { get; private set; }
        public NPCScanner Scanner { get; private set; }
        public NPCInventory Inventory { get; private set; }

        public bool HasTarget => Target != null;

        public bool Initialized { get; private set; }

        protected virtual void Update()
        {
            if (!Initialized)
                return;

            foreach (NPCComponent component in Components)
                component.Frame();
        }

        protected virtual void FixedUpdate()
        {
            if (!Initialized)
                return;

            foreach (NPCComponent component in Components)
                component.Tick();
            Personality?.Tick();
        }

        protected virtual void OnDestroy()
        {
            foreach (NPCComponent component in Components)
                component.Close();
        }

        public void Setup(NPC npc)
        {
            NPC = npc;
        }

        public void SetupComponents()
        {
            Inventory = AddNPCComponent<NPCInventory>();
            Motor = AddNPCComponent<NPCMotor>();
            Pathfinder = AddNPCComponent<NPCPathfinder>();
            ItemUser = AddNPCComponent<NPCItemUser>();
            Scanner = AddNPCComponent<NPCScanner>();
            Initialized = true;
        }

        public void RemovePersonality()
        {
            Personality?.End();
            Personality = null;
        }

        public void SetPersonality(NPCPersonalityBase personality)
        {
            if (personality == null)
            {
                RemovePersonality();
                return;
            }

            personality.Init(this);
            Personality?.End();
            Personality = personality;
            personality.Begin();
        }

        public T SetPersonality<T>() where T : NPCPersonalityBase, new()
        {
            T personality = new();
            SetPersonality(personality);
            return personality;
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
            if (door is CheckpointDoor checkpoint || door.TryGetComponentInParent(out checkpoint))
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
