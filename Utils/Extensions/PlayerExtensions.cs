using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftNPCs.Utils.Extensions
{
    public static class PlayerExtensions
    {
        public static Elevator GetElevator(this Player player)
        {
            foreach (Elevator e in Elevator.List.Where(e => e.WorldSpaceBounds.Contains(player.Position)))
                return e;
            return null;
        }
    }
}
