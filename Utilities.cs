using PlayerRoles;
using System;
using System.Collections.Generic;

namespace SwiftNPCs
{
    public static class Utilities
    {
        public static readonly Dictionary<string, RoleTypeId> Roles = [];

        static Utilities()
        {
            RoleTypeId[] roleTypeIds = Enum.GetValues(typeof(RoleTypeId)) as RoleTypeId[];
            foreach (RoleTypeId r in roleTypeIds)
                Roles.Add(r.ToString().ToLower(), r);
        }

        public static RoleTypeId GetRoleFromString(this string roleName) => !Roles.ContainsKey(roleName) ? RoleTypeId.None : Roles[roleName];
        public static bool TryGetRoleFromString(this string roleName, out RoleTypeId roleType)
        {
            roleType = GetRoleFromString(roleName);
            return roleType != RoleTypeId.None;
        }
    }
}
