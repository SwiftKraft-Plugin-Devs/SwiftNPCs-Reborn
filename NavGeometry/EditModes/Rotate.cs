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

            if (p.Room == null || !hasHit || !hit.collider.gameObject.name.Contains("(NavGeometry)") || !hit.collider.gameObject.TryGetComponent(out AdminToys.PrimitiveObjectToy comp))
                return default;

            editingPlayer = p;
            currentEdit = PrimitiveObjectToy.Get(comp);
            PrimitiveObjectToy toy = currentEdit;
            Quaternion original = currentEdit.Rotation;
            normal = hit.normal;
            currentEdit.IsStatic = false;

            void undo()
            {
                PrimitiveObjectToy t = currentEdit;
                currentEdit = null;
                Timing.CallDelayed(0.1f, () => t.IsStatic = true);
                editingPlayer = null;
                toy.Rotation = original;
            }

            return new(undo);
        }

        public override void Tick()
        {
            if (currentEdit == null || editingPlayer == null)
                return;

            // Define the plane
            Plane surfacePlane = new(normal, currentEdit.Position);

            // Ray from the camera
            Ray aimRay = new(editingPlayer.Camera.position, editingPlayer.Camera.forward);

            if (!surfacePlane.Raycast(aimRay, out float enter))
                return;

            // The target point where the player is aiming on the plane
            Vector3 aimPointOnPlane = aimRay.GetPoint(enter);

            // Direction the player is aiming on the plane
            Vector3 targetDirection = (aimPointOnPlane - currentEdit.Position);
            targetDirection = Vector3.ProjectOnPlane(targetDirection, normal).normalized;

            if (targetDirection.sqrMagnitude < 0.001f)
                return;

            // Current forward direction of the object projected onto the same plane
            Vector3 currentForward = Vector3.ProjectOnPlane(currentEdit.Transform.forward, normal).normalized;

            if (currentForward.sqrMagnitude < 0.001f)
                return;

            // Calculate signed angle between current forward and target direction on the plane
            float angle = Vector3.SignedAngle(currentForward, targetDirection, normal);

            // Create a rotation that rotates around the surface normal
            Quaternion additiveRotation = Quaternion.AngleAxis(angle, normal);

            // Apply it additively
            currentEdit.Rotation = additiveRotation * currentEdit.Rotation;
        }
    }
}
