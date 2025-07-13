using System.Collections.Generic;

namespace SwiftNPCs.Features.Spawners
{
    public static class SpawnerManager
    {
        public static readonly List<NPCSpawner> Spawners = [];

        public static void Enable()
        {
            StaticUnityMethods.OnFixedUpdate += Tick;
        }

        public static void Tick()
        {
            for (int i = 0; i < Spawners.Count; i++)
                Spawners[i].Tick();
        }

        public static void Disable()
        {
            StaticUnityMethods.OnFixedUpdate -= Tick;
        }
    }
}
