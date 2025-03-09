using LabApi.Features.Wrappers;
using UnityEngine;

namespace SwiftNPCs.Features.Targettables
{
    public class TargetablePlayer(Player target) : TargetableBase<Player>(target)
    {
        public override int Priority => !Target.IsAlive ? -1 : Target.IsSCP ? 2 : 1;

        public override Vector3 HitPosition => Target.Position;

        public override Vector3 CriticalPosition => Target.Camera.position;
    }
}
