using LabApi.Features.Wrappers;
using UnityEngine;

namespace SwiftNPCs.Features.Targettables
{
    public class TargetablePlayer : TargetableBase<Player>
    {
        public override int Priority => !Target.IsAlive ? -1 : Target.IsSCP ? 2 : 1;

        public override Vector3 HitPosition => Target.Position;

        public override Vector3 CriticalPosition => Target.Camera.position;

        public TargetablePlayer() : base() { }

        public TargetablePlayer(Player target) : base(target) => Target = target;
    }
}
