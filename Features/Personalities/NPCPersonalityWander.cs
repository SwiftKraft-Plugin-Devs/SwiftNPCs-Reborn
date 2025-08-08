using LabApi.Features.Wrappers;
using SwiftNPCs.Utils.Extensions;
using SwiftNPCs.Utils.Structures;
using System.Collections.Generic;
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

        private void LookAround()
        {
            lookTimer.Tick(Time.fixedDeltaTime);

            if (lookTimer.Ended)
            {
                Core.Pathfinder.LookAtWaypoint = false;
                Vector3 dir;
                if (Random.Range(0f, 1f) < 0.5f && Core.Scanner.TryGetFriendlies(out List<Player> players))
                {
                    Player p = players.GetRandom();
                    dir = p.Position - Core.NPC.WrapperPlayer.Camera.position;
                }
                else
                {
                    Vector2 rand = Random.insideUnitCircle;
                    dir = new(rand.x, 0f, rand.y);

                    if (dir.sqrMagnitude == 0f)
                        dir = Vector3.one * (Random.Range(0, 1f) < 0.5f ? -1f : 1f);
                }

                Core.Motor.WishLookDirection = dir;
                lookTimer.Reset(Random.Range(MinLookTimer, MaxLookTimer));
            }
        }

        protected virtual void WanderLoop()
        {
            wanderTimer.Tick(Time.fixedDeltaTime);

            if (Core.HasTarget || !Core.Pathfinder.IsAtDestination && !wanderTimer.Ended)
                return;

            waitTimer.Tick(Time.fixedDeltaTime);

            if (!Core.Pathfinder.IsAtDestination)
                Core.Pathfinder.Stop();

            if (waitTimer.Ended)
                SelectRoom();
            else
                LookAround();
        }
    }
}
