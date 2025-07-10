using LabApi.Features.Wrappers;
using SwiftNPCs.Utils.Extensions;
using SwiftNPCs.Utils.Structures;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SwiftNPCs.Features.Personalities
{
    public abstract class NPCPersonalityWander : NPCPersonalityBase
    {
        public abstract NPCPersonalityBase CombatPersonality { get; }

        public float MaxWanderTimer = 20f;
        public float MinWanderTimer = 10f;

        public float MaxWaitTimer = 3f;
        public float MinWaitTimer = 1f;

        readonly Timer wanderTimer = new(15f);
        readonly Timer waitTimer = new(1f);

        public override void Begin() { }

        public override void End() { }

        public override void Tick()
        {
            WanderLoop();

            if (Core.HasTarget && IsThreat)
                Core.SetPersonality(CombatPersonality);
        }

        protected virtual void WanderLoop()
        {
            wanderTimer.Tick(Time.fixedDeltaTime);

            if (!Core.HasTarget && (Core.Pathfinder.IsAtDestination || wanderTimer.Ended))
            {
                waitTimer.Tick(Time.fixedDeltaTime);

                if (!Core.Pathfinder.IsAtDestination)
                    Core.Pathfinder.Stop();

                if (waitTimer.Ended)
                {
                    Room r = Room.List.Where((r) => r != null && r.Base != null && !r.IsDestroyed && r.Zone == Core.NPC.WrapperPlayer.Zone).ToList().GetRandom();
                    if (r != null)
                    {
                        Vector3 rand = Random.insideUnitSphere * 4f;
                        rand.y = 0f;
                        Core.Pathfinder.Destination = r.Transform.position + rand;
                        Core.Pathfinder.LookAtWaypoint = true;
                        wanderTimer.Reset(Random.Range(MinWanderTimer, MaxWanderTimer));
                        waitTimer.Reset(Random.Range(MinWaitTimer, MaxWaitTimer));
                    }
                }
            }
        }
    }
}
