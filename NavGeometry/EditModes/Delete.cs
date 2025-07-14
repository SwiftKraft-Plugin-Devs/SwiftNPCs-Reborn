using LabApi.Features.Wrappers;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace SwiftNPCs.NavGeometry.EditModes
{
    public class Delete : NavGeometryEditor.EditModeBase
    {
        public override string Name => "Delete Geometry";

        public override NavGeometryEditor.EditAction Action(Player p, bool hasHit, RaycastHit hit)
        {
            if (p.Room == null || !hasHit || !hit.collider.gameObject.name.Contains("(NavGeometry)") || !hit.collider.gameObject.TryGetComponent(out AdminToys.PrimitiveObjectToy comp))
                return default;

            PrimitiveObjectToy prim = PrimitiveObjectToy.Get(comp);
            Vector3 pos = prim.Position;
            Quaternion rot = prim.Rotation;
            Vector3 scale = prim.Scale;
            Room room = p.Room;
            prim.Destroy();

            return new(undo);

            void undo() => NavGeometryManager.Spawn(room, pos, rot, scale);
        }

        public override void Tick() { }
    }
}
