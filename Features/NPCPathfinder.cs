using LabApi.Features.Console;
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
                _destination = !NavMesh.SamplePosition(value, out NavMeshHit hit, 3f, NavMesh.AllAreas) ? value : hit.position;
                UpdatePath();
            }
        }
        Vector3 _destination;

        public float RepathTimer = 1f;
        float repathTimer;
        public readonly NPCPath Path = new();

        public override void Begin()
        {
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
                UpdatePath();
                repathTimer = RepathTimer;
            }
            else
                Path.UpdateWaypoint(Core.Position);

            Vector3 waypoint = Path.GetCurrentWaypoint(Core.Position);
            Vector3 pos = Core.Position;
            waypoint.y = 0f;
            pos.y = 0f;
            Vector3 direction = (waypoint - pos).normalized;

            Motor.WishMoveDirection = direction;
            if (LookAtWaypoint)
                Motor.WishLookDirection = direction;
        }

        public void UpdatePath() => Path.UpdatePath(Core.Position, Destination);

        public class NPCPath(float waypointRadius = 1f)
        {
            public readonly NavMeshPath CurrentPath = new();

            public float WaypointRadius = waypointRadius;

            public bool Ended { get; private set; }

            public int Current
            {
                get => current;
                private set => current = CurrentPath.corners.Length <= 0 ? 0 : Mathf.Clamp(value, 0, CurrentPath.corners.Length - 1);
            }
            private int current;

            public void UpdatePath(Vector3 current, Vector3 target)
            {
                NavMesh.SamplePosition(current, out NavMeshHit hit, 2f, NavMesh.AllAreas);
                NavMesh.CalculatePath(hit.position, target, NavMesh.AllAreas, CurrentPath);
                Current = 0;
                Ended = false;
                UpdateWaypoint(current);
            }

            public void UpdateWaypoint(Vector3 pos)
            {
                if (Ended)
                    return;

                if (Current >= CurrentPath.corners.Length - 1)
                {
                    Ended = true;
                    return;
                }

                if ((GetCurrentWaypoint(pos) - pos).sqrMagnitude < WaypointRadius * WaypointRadius)
                    Current++;
            }

            public Vector3 GetCurrentWaypoint(Vector3 d = default) => CurrentPath.corners.Length <= 0 ? d : CurrentPath.corners[Current];
        }
    }
}
