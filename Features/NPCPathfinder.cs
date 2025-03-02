using LabApi.Features.Wrappers;
using UnityEngine;
using UnityEngine.AI;
using Logger = LabApi.Features.Console.Logger;

namespace SwiftNPCs.Features
{
    public class NPCPathfinder : NPCComponent
    {
        public NPCMotor Motor { get; private set; }

        public bool LookAtWaypoint = true;

        public Vector3 Destination
        {
            get => _destination;
            set
            {
                _destination = !NavMesh.SamplePosition(value, out NavMeshHit hit, 2f, NavMesh.AllAreas) ? value : hit.position;
                Path.UpdatePath();
            }
        }
        Vector3 _destination;

        public float RepathTimer = 1f;
        float repathTimer;
        public NPCPath Path { get; private set; }

        public override void Begin()
        {
            Path = new(this);
            Motor = Core.GetNPCComponent<NPCMotor>();
            Destination = Core.Position;
        }

        public override void Tick()
        {
            Destination = Player.Get(2).Position;

            Logger.Info("Destination: " + Destination + ", path corners: " + Path.CurrentPath.corners.Length + ", current: " + Path.Current);

            repathTimer -= Time.fixedDeltaTime;
            if (repathTimer <= 0f)
            {
                Path.UpdatePath();
                repathTimer = RepathTimer;
            }
            else
                Path.UpdateWaypoint();

            Vector3 waypoint = Path.GetCurrentWaypoint();
            Vector3 pos = Core.Position;
            waypoint.y = 0f;
            pos.y = 0f;
            Vector3 direction = (waypoint - pos).normalized;

            Motor.WishMoveDirection = direction;
            if (LookAtWaypoint)
                Motor.WishLookDirection = direction;
        }

        public class NPCPath(NPCPathfinder parent, float waypointRadius = 1f)
        {
            public readonly NPCPathfinder Parent = parent;

            public readonly NavMeshPath CurrentPath = new();

            public float WaypointRadius = waypointRadius;

            public bool Ended { get; private set; }

            public int Current
            {
                get => current;
                private set => current = CurrentPath.corners.Length <= 0 ? 0 : Mathf.Clamp(value, 0, CurrentPath.corners.Length - 1);
            }
            private int current;

            public void UpdatePath()
            {
                NavMesh.SamplePosition(Parent.Core.Position, out NavMeshHit hit, 2f, NavMesh.AllAreas);
                NavMesh.CalculatePath(hit.position, Parent.Destination, NavMesh.AllAreas, CurrentPath);
                Current = 0;
                Ended = false;
                UpdateWaypoint();
            }

            public void UpdateWaypoint()
            {
                if (Ended)
                    return;

                if (Current >= CurrentPath.corners.Length - 1)
                {
                    Ended = true;
                    return;
                }

                if ((GetCurrentWaypoint() - Parent.Core.Position).sqrMagnitude < WaypointRadius * WaypointRadius)
                    Current++;
            }

            public Vector3 GetCurrentWaypoint() => 
                (CurrentPath.corners.Length - 1 <= Current 
                && (Parent.Destination - Parent.Core.Position).sqrMagnitude 
                    > WaypointRadius * WaypointRadius) 
                || CurrentPath.corners.Length <= 0
                    ? Parent.Destination
                    : CurrentPath.corners[Current];
        }
    }
}
