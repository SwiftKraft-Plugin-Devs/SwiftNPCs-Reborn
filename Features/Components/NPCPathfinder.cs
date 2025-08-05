using SwiftNPCs.Utils.Structures;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SwiftNPCs.Features.Components
{
    public class NPCPathfinder : NPCComponent
    {
        public NPCMotor Motor { get; private set; }

        public bool LookAtWaypoint = true;
        public bool LocalAvoidance = true;

        public float DestinationRange = 1f;

        public Vector3 Destination
        {
            get => _destination;
            set
            {
                if (value == Core.Position)
                {
                    _realDestination = value;
                    _destination = value;
                    return;
                }

                _realDestination = value;
                _destination = !NavMesh.SamplePosition(value, out NavMeshHit hit, 10f, NavMesh.AllAreas) ? value : hit.position;
            }
        }
        public Vector3 RealDestination => _realDestination;

        public Vector3 RequestedForce { get; set; }
        public float RequestDecayRate => Motor.MoveSpeed;

        Vector3 _destination;
        Vector3 _realDestination;

        public float RepathTimer = 0.3f;
        float repathTimer;

        readonly Timer stuckTimer = new(0.5f);
        readonly Timer unstuckTimer = new(0.5f);
        const float stuckDistance = 1.5f;
        const int maxStuckCounter = 3;
        const float stuckDistanceSqr = stuckDistance * stuckDistance;
        int stuckCounter = 0;
        Vector3 lastCheckPos;

        public event Action OnStuck;

        public NPCPath Path { get; private set; }

        public bool IsAtDestination { get; private set; }

        public void Stop() => Destination = Core.Position;

        public bool TrySetDestination(Vector3 destination, out Vector3 point)
        {
            bool notOnNavMesh = !NavMesh.SamplePosition(destination, out NavMeshHit hit, 10f, NavMesh.AllAreas);

            point = notOnNavMesh ? destination : hit.position;
            _destination = point;

            return notOnNavMesh;
        }

        public override void Begin()
        {
            Path = new(this);
            Motor = Core.GetNPCComponent<NPCMotor>();
            Destination = Core.Position;
        }

        public void StuckCheck()
        {
            stuckTimer.Tick(Time.fixedDeltaTime);

            if (stuckTimer.Ended)
            {
                stuckTimer.Reset();

                if ((lastCheckPos - Core.Position).sqrMagnitude <= stuckDistanceSqr)
                {
                    stuckCounter++;
                    if (stuckCounter > maxStuckCounter)
                    {
                        OnStuck?.Invoke();
                        stuckCounter = 0;
                        StuckAction();
                    }
                }
                else
                    stuckCounter = 0;

                lastCheckPos = Core.Position;
            }
        }

        private void StuckAction()
        {
            Motor.WishMoveDirection = (Motor.WishMoveDirection * -2f + Random.insideUnitSphere).normalized;
            unstuckTimer.Reset();
        }

        public override void Tick()
        {
            //Logger.Info("Destination: " + Destination + ", path corners: " + Path.Waypoints.Count + ", current: " + Path.Current);
            //Destination = Player.Get(2).Position;

            IsAtDestination = (Destination - Core.Position).sqrMagnitude <= DestinationRange * DestinationRange;
            RequestedForce = Vector3.MoveTowards(RequestedForce, Vector3.zero, RequestDecayRate * Time.fixedDeltaTime);

            if (IsAtDestination)
            {
                if (RealDestination != Core.Position)
                {
                    Destination = Core.Position;
                    Motor.WishMoveDirection = RequestedForce;
                }
                return;
            }

            unstuckTimer.Tick(Time.fixedDeltaTime);

            if (unstuckTimer.Ended)
            {
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
                Vector3 direction = (waypoint - pos + (LocalAvoidance ? CalculateAvoidance(0.35f) + RequestedForce : Vector3.zero)).normalized;

                Motor.WishMoveDirection = direction;
                if (LookAtWaypoint)
                    Motor.WishLookDirection = direction;

                StuckCheck();
            }
        }

        public Vector3 CalculateAvoidance(float avoidRadius)
        {
            Vector3 avoidance = Vector3.zero;
            Vector3 myPosition = Core.Position;
            var myZone = Core.NPC.Zone;

            var allNPCs = NPCManager.AllNPCs;
            int count = allNPCs.Count;
            for (int i = 0; i < count; i++)
            {
                var npc = allNPCs[i];
                if (npc == Core.NPC || npc.Zone != myZone)
                    continue;

                Vector3 offset = myPosition - npc.Position;
                float sqrDist = offset.sqrMagnitude;

                if (sqrDist < avoidRadius * avoidRadius && sqrDist > 0.0001f)
                {
                    float dist = Mathf.Sqrt(sqrDist);
                    Vector3 repulsion = offset / (dist * dist);
                    avoidance += repulsion;
                    npc.Core.Pathfinder.RequestedForce -= repulsion * 0.5f;
                }
            }

            return avoidance;
        }

        public class NPCPath(NPCPathfinder parent, float waypointRadius = 2f)
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
