using LabApi.Features.Wrappers;
using System.Linq;

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

        public static bool TryGetElevator(this Player player, out Elevator elev)
        {
            elev = player.GetElevator();
            return elev != null;
        }
    }
}
