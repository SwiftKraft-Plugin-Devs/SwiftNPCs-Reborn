using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;

namespace SwiftNPCs.NavGeometry.EditModes
{
    public class Rotate : NavGeometryEditor.EditModeBase
    {
        public override string Name => "Rotate";

        Player editingPlayer;
        PrimitiveObjectToy currentEdit;
        Vector3 normal;

        public override NavGeometryEditor.EditAction Action(Player p, bool hasHit, RaycastHit hit)
        {
            if (currentEdit != null)
            {
                editingPlayer = null;
                PrimitiveObjectToy t = currentEdit;
                currentEdit = null;
                Timing.CallDelayed(0.1f, () => t.IsStatic = true);
                return default;
            }

            if (!hasHit 
                || !hit.collider.gameObject.name.Contains("(NavGeometry)") 
                || !hit.collider.gameObject.TryGetComponent(out AdminToys.PrimitiveObjectToy comp))
                return default;

            editingPlayer = p;
            currentEdit = PrimitiveObjectToy.Get(comp);
            PrimitiveObjectToy toy = currentEdit;
            Quaternion original = currentEdit.Rotation;
            normal = hit.normal;
            currentEdit.IsStatic = false;

            void undo()
            {
                currentEdit = null;
                editingPlayer = null;
                toy.Rotation = original;
                toy.IsStatic = false;
                Timing.CallDelayed(0.1f, () => toy.IsStatic = true);
            }

            return new(undo);
        }

        public override void Tick()
        {
            if (currentEdit == null || editingPlayer == null)
                return;

            Plane surfacePlane = new(normal, currentEdit.Position);
            Ray aimRay = new(editingPlayer.Camera.position, editingPlayer.Camera.forward);

            if (!surfacePlane.Raycast(aimRay, out float enter))
                return;

            Vector3 aimPointOnPlane = aimRay.GetPoint(enter);
            Vector3 targetDirection = aimPointOnPlane - currentEdit.Position;
            targetDirection = Vector3.ProjectOnPlane(targetDirection, normal).normalized;

            if (targetDirection.sqrMagnitude < 0.001f)
                return;

            Vector3 currentForward = Vector3.ProjectOnPlane(currentEdit.Transform.forward, normal).normalized;

            if (currentForward.sqrMagnitude < 0.001f)
                return;

            float angle = Vector3.SignedAngle(currentForward, targetDirection, normal);
            Quaternion additiveRotation = Quaternion.AngleAxis(angle, normal);
            currentEdit.Rotation = additiveRotation * currentEdit.Rotation;
        }
    }
}
