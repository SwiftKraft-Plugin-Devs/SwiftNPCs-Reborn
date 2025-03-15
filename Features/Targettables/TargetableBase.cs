using LabApi.Features.Wrappers;
using System;
using UnityEngine;

namespace SwiftNPCs.Features.Targettables
{
    public abstract class TargetableBase : IComparable<TargetableBase>
    {
        public NPCCore NPC { get; set; }

        public virtual float PriorityScore => Priority * PriorityWeight + Distance * DistanceWeight;

        public virtual float PriorityWeight => 5f;
        public virtual float DistanceWeight => 1f;

        public abstract int Priority { get; }
        public virtual float Distance => Vector3.Distance(NPC.Position, PivotPosition);

        public abstract Vector3 HitPosition { get; }

        public virtual Vector3 CriticalPosition => HitPosition;

        public virtual Vector3 PivotPosition => HitPosition;

        public virtual int CompareTo(TargetableBase other) => PriorityScore.CompareTo(other.PriorityScore);
    }

    public abstract class TargetableBase<T> : TargetableBase
    {
        public TargetableBase() { }

        public TargetableBase(T target) : this() => Target = target;

        public T Target { get; set; }
    }
}
