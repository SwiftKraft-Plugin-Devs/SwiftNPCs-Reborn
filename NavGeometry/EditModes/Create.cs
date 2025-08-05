using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;

namespace SwiftNPCs.NavGeometry.EditModes
{
    public class Create : NavGeometryEditor.EditModeBase
    {
        Room editingRoom;
        PrimitiveObjectToy currentEdit;
        Player editingPlayer;
        Vector3 point;

        public override string Name => "Create Geometry";

        public override NavGeometryEditor.EditAction Action(Player p, bool hasHit, RaycastHit hit)
        {
            if (currentEdit != null)
            {
                editingPlayer = null;
                currentEdit.Base.NetworkPrimitiveFlags = AdminToys.PrimitiveFlags.Collidable | AdminToys.PrimitiveFlags.Visible;
                PrimitiveObjectToy t = currentEdit;
                currentEdit = null;
                if (editingRoom != null)
                    NavGeometryManager.SaveNavGeometry(editingRoom);
                editingRoom = null;
                point = default;
                Timing.CallDelayed(0.1f, () => t.IsStatic = true);
                return default;
            }

            Room r = Room.GetRoomAtPosition(hit.point);

            if (r == null || !hasHit)
                return default;

            PrimitiveObjectToy toy = NavGeometryManager.Spawn(r, p.Room.Position, hit.normal, Vector3.one);
            toy.GameObject.name += "(NavGeometry)";
            currentEdit = toy;
            currentEdit.IsStatic = false;
            currentEdit.Base.NetworkPrimitiveFlags = AdminToys.PrimitiveFlags.Visible;
            editingPlayer = p;
            editingRoom = r;
            point = hit.point;

            void undo()
            {
                toy.Destroy();
                if (r != null)
                    NavGeometryManager.SaveNavGeometry(r);
            }

            return new(undo);
        }

        public override void Tick()
        {
            if (currentEdit == null)
                return;

            ScaleCubeFromPointToPoint(currentEdit, point, !Physics.Raycast(editingPlayer.Camera.position, editingPlayer.Camera.forward,
                    out RaycastHit _hit, 5f, NavGeometryEditor.GeoLayers, QueryTriggerInteraction.Ignore) ? editingPlayer.Camera.position + editingPlayer.Camera.forward * 5f : _hit.point);
        }

        public static void ScaleCubeFromPointToPoint(PrimitiveObjectToy cube, Vector3 worldPointA, Vector3 worldPointB)
        {
            if (cube == null || cube.Base == null)
                return;

            // Get local axes in world space
            Vector3 localRight = cube.Transform.right;
            Vector3 localUp = cube.Transform.up;
            Vector3 localForward = cube.Transform.forward;

            // Vector from point A to B in world space
            Vector3 AB = worldPointB - worldPointA;

            // Project AB onto the cube’s local axes to determine scale along each axis
            float scaleX = Vector3.Dot(AB, localRight);
            float scaleY = Vector3.Dot(AB, localUp);
            float scaleZ = Vector3.Dot(AB, localForward);

            // Final scale (must be positive)
            Vector3 newScale = new(
                Mathf.Abs(scaleX),
                Mathf.Abs(scaleY),
                Mathf.Abs(scaleZ)
            );
            cube.Scale = newScale;

            // Determine which corner should be placed at point A
            Vector3 anchorOffsetLocal = new(
                -0.5f * Mathf.Sign(scaleX),
                -0.5f * Mathf.Sign(scaleY),
                -0.5f * Mathf.Sign(scaleZ)
            );

            // Scale the local offset to get the distance from center to corner
            Vector3 anchorOffsetWorld = cube.Transform.rotation * Vector3.Scale(anchorOffsetLocal, newScale);

            // Reposition so the correct corner is at point A
            cube.Position = worldPointA - anchorOffsetWorld;
        }
    }
}
