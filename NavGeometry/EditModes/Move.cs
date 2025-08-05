using AdminToys;
using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace SwiftNPCs.NavGeometry.EditModes
{
    public class Move : NavGeometryEditor.EditModeBase
    {
        public override string Name => "Move";

        Player editingPlayer;
        PrimitiveObjectToy moving;
        float dist;

        public override NavGeometryEditor.EditAction Action(Player p, bool hasHit, RaycastHit hit)
        {
            if (moving != null)
            {
                moving.IsStatic = true;
                moving = null;
                editingPlayer = null;
                return default;
            }    

            if (!hasHit
                || !hit.collider.gameObject.name.Contains("(NavGeometry)")
                || !hit.collider.gameObject.TryGetComponent(out AdminToys.PrimitiveObjectToy toy))
                return default;

            editingPlayer = p;
            moving = PrimitiveObjectToy.Get(toy);
            moving.IsStatic = false;
            dist = (editingPlayer.Camera.position - moving.Position).magnitude;
            Vector3 originalPos = moving.Position;
            PrimitiveObjectToy t = moving;

            void undo()
            {
                t.Position = originalPos;
                t.IsStatic = false;
                moving = null;
                editingPlayer = null;
                Timing.CallDelayed(0.1f, () => t.IsStatic = true);
            }

            return new(undo);
        }

        public override void Tick()
        {
            if (moving != null)
            {
                Vector3 pos = editingPlayer.Camera.position + editingPlayer.Camera.forward * dist;
                moving.Position = pos;
            }
        }
    }
}
