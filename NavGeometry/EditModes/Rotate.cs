using LabApi.Features.Wrappers;
using UnityEngine;

namespace SwiftNPCs.NavGeometry.EditModes
{
    public class Rotate : NavGeometryEditor.EditModeBase
    {
        public override string Name => "Rotate";

        PrimitiveObjectToy currentEdit;

        public override NavGeometryEditor.EditAction Action(Player p, bool hasHit, RaycastHit hit)
        {
            if (currentEdit != null)
            {
                currentEdit = null;
                return default;
            }

            if (p.Room == null || !hasHit || !hit.collider.gameObject.name.Contains("(NavGeometry)") || !hit.collider.gameObject.TryGetComponent(out AdminToys.PrimitiveObjectToy comp))
                return default;

            currentEdit = PrimitiveObjectToy.Get(comp);
            

            void undo()
            {

            }
        }

        public override void Tick()
        {

        }
    }
}
