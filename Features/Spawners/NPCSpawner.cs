using PlayerRoles;
using SwiftNPCs.Features.Personalities;
using SwiftNPCs.Utils.Structures;
using System.Collections.Generic;
using UnityEngine;

namespace SwiftNPCs.Features.Spawners
{
    public class NPCSpawner
    {
        public readonly Vector3 Position;

        public readonly Timer Timer = new();

        public RoleTypeId Role;

        public int Limit = 5;
        readonly List<NPC> NPCs = [];

        public NPCSpawner(Vector3 position)
        {
            SpawnerManager.Spawners.Add(this);
            Position = position;
        }

        public void Tick()
        {
            Timer.Tick(Time.fixedDeltaTime);

            if (Timer.Ended)
            {
                Timer.Reset();
                Spawn();
            }
        }

        public void Spawn()
        {
            NPCs.RemoveAll(n => n.Disposed);
            if (NPCs.Count >= Limit)
                return;

            NPC npc = new(Position, role: Role);
            NPCs.Add(npc);
            npc.Core.SetPersonality<NPCPersonalityWanderHuman>();
        }
    }
}
