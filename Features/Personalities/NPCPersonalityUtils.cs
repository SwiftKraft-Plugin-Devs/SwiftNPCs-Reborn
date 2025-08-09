using LabApi.Features.Wrappers;
using SwiftNPCs.Utils.Extensions;
using SwiftNPCs.Utils.Structures;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SwiftNPCs.Features.Personalities
{
    public static class NPCPersonalityUtils
    {
        public static void LookAround(this NPCCore core, Timer lookTimer, float min, float max)
        {
            lookTimer.Tick(NPCManager.DeltaTime);

            if (lookTimer.Ended)
            {
                core.Pathfinder.LookAtWaypoint = false;
                Vector3 dir;
                if (Random.Range(0f, 1f) < 0.5f && core.Scanner.TryGetFriendlies(out List<Player> players))
                {
                    Player p = players.GetRandom();
                    dir = p.Camera.position - core.NPC.WrapperPlayer.Camera.position;
                }
                else
                {
                    Vector2 rand = Random.insideUnitCircle;
                    dir = new(rand.x, 0f, rand.y);

                    if (dir.sqrMagnitude == 0f)
                        dir = Vector3.one * (Random.Range(0, 1f) < 0.5f ? -1f : 1f);
                }

                core.Motor.WishLookDirection = dir;
                lookTimer.Reset(Random.Range(min, max));
            }
        }
    }
}
