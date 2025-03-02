using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SwiftNPCs.Features
{
    public class NPCCore : MonoBehaviour
    {
        public NPC NPC { get; private set; }

        public readonly List<NPCComponent> Components = [];

        public Vector3 Position => NPC.WrapperPlayer.Position;

        public NPCMotor Motor { get; private set; }
        public NPCPathfinder Pathfinder { get; private set; }

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

        public void Setup(NPC npc)
        {
            NPC = npc;
            Motor = AddNPCComponent<NPCMotor>();
            Pathfinder = AddNPCComponent<NPCPathfinder>();
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
    }
}
