using System.Collections.Generic;

namespace SwiftNPCs.Features
{
    public static class NPCManager
    {
        public static readonly List<NPC> AllNPCs = [];

        public static void RemoveAll()
        {
            NPC[] npcs = [.. AllNPCs];
            foreach (NPC npc in npcs)
                npc.Destroy();
        }
    }
}
