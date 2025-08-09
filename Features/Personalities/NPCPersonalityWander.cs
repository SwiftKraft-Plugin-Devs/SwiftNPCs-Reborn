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

        public float MaxWanderTimer = 15f;
        public float MinWanderTimer = 8f;

        public float MaxWaitTimer = 4f;
        public float MinWaitTimer = 2.5f;

        public float MaxLookTimer = 2f;
        public float MinLookTimer = 1.5f;

        readonly Timer wanderTimer = new(15f);
        readonly Timer waitTimer = new(1f);
        readonly Timer lookTimer = new(0.5f);

        public override void Begin() => Core.Pathfinder.OnStuck += OnPathfinderStuck;

        public override void End() => Core.Pathfinder.OnStuck -= OnPathfinderStuck;

        public override void Tick()
        {
            WanderLoop();

            if (Core.HasTarget && IsThreat)
                Core.SetPersonality(CombatPersonality);
        }

        private void OnPathfinderStuck() => SelectRoom();

        private void SelectRoom()
        {
            Room r = Room.List.Where((r) => r != null && r.Base != null && !r.IsDestroyed && r.Zone == WrapperPlayer.Zone).ToList().GetRandom();

            if (r != null)
            {
                Vector3 rand = Random.insideUnitSphere * 3f;
                rand.y = 0f;
                Core.Pathfinder.Destination = r.Transform.position + rand;
                Core.Pathfinder.LookAtWaypoint = true;
                wanderTimer.Reset(Random.Range(MinWanderTimer, MaxWanderTimer));
                waitTimer.Reset(Random.Range(MinWaitTimer, MaxWaitTimer));
            }
        }

        protected virtual void WanderLoop()
        {
            wanderTimer.Tick(DeltaTime);

            if (Core.HasTarget || !Core.Pathfinder.IsAtDestination && !wanderTimer.Ended)
                return;

            waitTimer.Tick(DeltaTime);

            if (!Core.Pathfinder.IsAtDestination)
                Core.Pathfinder.Stop();

            if (waitTimer.Ended)
                SelectRoom();
            else
                Core.LookAround(lookTimer, MinLookTimer, MaxLookTimer);
        }
    }
}
