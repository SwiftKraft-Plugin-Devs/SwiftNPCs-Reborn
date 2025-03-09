using LabApi.Features.Wrappers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Logger = LabApi.Features.Console.Logger;

namespace SwiftNPCs.Features.Components
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
                _realDestination = value;
                _destination = !NavMesh.SamplePosition(value, out NavMeshHit hit, 10f, NavMesh.AllAreas) ? value : hit.position;
            }
        }
        public Vector3 RealDestination => _realDestination;

        Vector3 _destination;
        Vector3 _realDestination;

        public float RepathTimer = 0.3f;
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
            //Logger.Info("Destination: " + Destination + ", path corners: " + Path.Waypoints.Count + ", current: " + Path.Current);

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

        public class NPCPath(NPCPathfinder parent, float waypointRadius = 1.5f)
        {
            public readonly NPCPathfinder Parent = parent;

            public readonly NavMeshPath CurrentPath = new();

            public readonly List<Vector3> Waypoints = [];

            public float WaypointRadius = waypointRadius;

            public bool Ended { get; private set; }

            public int Current
            {
                get => current;
                private set => current = Waypoints.Count <= 0 ? 0 : Mathf.Clamp(value, 0, Waypoints.Count - 1);
            }
            private int current;

            public void UpdatePath()
            {
                NavMesh.SamplePosition(Parent.Core.Position, out NavMeshHit hit, 2f, NavMesh.AllAreas);
                NavMesh.CalculatePath(hit.position, Parent.Destination, NavMesh.AllAreas, CurrentPath);
                Waypoints.Clear();
                Waypoints.AddRange(CurrentPath.corners);
                Waypoints.Add(Parent.RealDestination);
                Current = 0;
                Ended = false;
                UpdateWaypoint();
            }

            public void UpdateWaypoint()
            {
                if (Ended)
                    return;

                if (Current >= Waypoints.Count - 1)
                {
                    Ended = true;
                    return;
                }

                if ((GetCurrentWaypoint() - Parent.Core.Position).sqrMagnitude < WaypointRadius * WaypointRadius)
                    Current++;
            }

            public Vector3 GetCurrentWaypoint() =>
                Waypoints.Count <= 0
                    ? Parent.RealDestination
                    : Waypoints[Current];
        }
    }
}
