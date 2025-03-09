using LabApi.Features.Wrappers;
using System;
using UnityEngine;

namespace SwiftNPCs.Features.Targettables
{
    public abstract class TargetableBase : IComparable<TargetableBase>
    {
        public abstract int Priority { get; }

        public abstract Vector3 HitPosition { get; }

        public virtual Vector3 CriticalPosition => HitPosition;

        public virtual Vector3 PivotPosition => HitPosition;

        public virtual int CompareTo(TargetableBase other) => Priority.CompareTo(other.Priority);
    }

    public abstract class TargetableBase<T>(T target) : TargetableBase
    {
        public T Target { get; set; } = target;
    }
}
