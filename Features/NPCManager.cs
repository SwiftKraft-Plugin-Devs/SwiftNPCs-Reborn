using LabApi.Features.Wrappers;
using System.Collections.Generic;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace SwiftNPCs.Features
{
    public static class NPCManager
    {
        public const int DeltaTimeCapacity = 15;
        public static readonly List<NPC> AllNPCs = [];

        public static float DeltaTime { get; private set; } = Time.fixedDeltaTime;
        public static int CurrentCapacity => Mathf.CeilToInt(AllNPCs.Count / (float)DeltaTimeCapacity);
        private static int TickProgress = 0;

        public static void Tick()
        {
            if (TickProgress >= CurrentCapacity)
                TickProgress = 0;

            for (int i = DeltaTimeCapacity * TickProgress; i < Mathf.Min(AllNPCs.Count, DeltaTimeCapacity * (TickProgress + 1)); i++)
            {
                if (i >= AllNPCs.Count) break;
                AllNPCs[i].Core.Tick();
            }

            TickProgress = (TickProgress + 1) % Mathf.Max(1, CurrentCapacity);
        }

        public static void RemoveAll()
        {
            for (int i = AllNPCs.Count - 1; i >= 0; i--)
                AllNPCs[i].Destroy();
            UpdateDeltaTime();
        }

        public static void Add(NPC npc)
        {
            AllNPCs.Add(npc);
            UpdateDeltaTime();
        }

        public static void UpdateDeltaTime() => DeltaTime = Time.fixedDeltaTime * CurrentCapacity;

        public static NPC GetNPC(this Player pl) => !pl.IsDummy ? null : AllNPCs.FirstOrDefault((n) => n.WrapperPlayer == pl, null);

        public static bool TryGetNPC(this Player pl, out NPC npc)
        {
            npc = pl.GetNPC();
            return npc != null;
        }

        public static NPC GetNPC(this ReferenceHub pl) => !pl.IsDummy ? null : AllNPCs.FirstOrDefault((n) => n.ReferenceHub == pl, null);

        public static bool TryGetNPC(this ReferenceHub pl, out NPC npc)
        {
            npc = pl.GetNPC();
            return npc != null;
        }
    }
}
