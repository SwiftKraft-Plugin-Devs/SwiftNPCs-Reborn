using LabApi.Features.Wrappers;
using SwiftNPCs.Features.Components;
using SwiftNPCs.Utils.Structures;
using UnityEngine;

namespace SwiftNPCs.Features.Personalities
{
    public class NPCPersonalityFollow : NPCPersonalityBase
    {
        public const string DataID = "FollowTarget";

        public Data CurrentData;

        public Player FollowTarget
        {
            get => HasFollowTarget ? CurrentData.FollowTarget : null;
            set
            {
                if (CurrentData == null && !Core.TryGetData(DataID, out CurrentData))
                    Core.TryAddData(DataID, out CurrentData);

                if (CurrentData != null)
                    CurrentData.FollowTarget = value;
            }
        }

        public bool HasFollowTarget => CurrentData != null && CurrentData.FollowTarget != null && CurrentData.FollowTarget.ReferenceHub != null;

        public bool InFollowRange => HasFollowTarget && (CurrentData.FollowTarget.Position - Core.Position).sqrMagnitude < FollowRange * FollowRange;

        public float FollowRange = 2.5f;

        readonly Timer followUpdate = new(0.5f);

        public class Data
        {
            public Player FollowTarget { get; set; }
        }

        public override void Init(NPCCore core)
        {
            base.Init(core);
            if (!Core.TryGetData(DataID, out CurrentData))
                Core.TryAddData(DataID, out CurrentData);
        }

        public override void Tick()
        {
            if (Core.HasTarget)
            {
                Core.SetPersonality(new NPCPersonalityHumanCombat());
                return;
            }

            if (!FollowTarget.IsAlive || FollowTarget.IsEnemy(WrapperPlayer))
            {
                FollowTarget = null;
                Core.SetPersonality(new NPCPersonalityWanderHuman());
                return;
            }

            if (!HasFollowTarget)
                return;

            followUpdate.Tick(Time.fixedDeltaTime);

            if (InFollowRange)
            {
                Core.Pathfinder.Stop();
                Core.Pathfinder.LookAtWaypoint = false;
                Core.Motor.WishLookPosition = FollowTarget.Position;
                return;
            }

            if (followUpdate.Ended)
            {
                UpdateDestination();
                followUpdate.Reset();
            }

            if (Core.Pathfinder.IsAtDestination && !InFollowRange)
                UpdateDestination();
        }

        public override void Begin() { }

        public override void End() { }

        public void UpdateDestination()
        {
            Core.Pathfinder.Destination = CurrentData.FollowTarget.Position;
            Core.Pathfinder.LookAtWaypoint = true;
        }
    }
}
