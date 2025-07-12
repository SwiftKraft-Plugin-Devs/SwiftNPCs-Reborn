using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SwiftNPCs.Utils
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
            roleType = roleName.GetRoleFromString();
            return roleType != RoleTypeId.None;
        }

        public static void InvokePrivate(this object obj, string methodName, params object[] parameters)
        {
            MethodInfo method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(obj, parameters);
        }

        public static T GetPrivateFieldValue<T>(this object target, string fieldName)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));

            Type type = target.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            object value = field.GetValue(target);

            return field == null || value is not T t
                ? throw new ArgumentException($"Field '{fieldName}' not found in type {type.FullName}, or might be a type mismatch of variable.")
                : t;
        }

        public static bool TryGetComponentInParent<T>(this Component comp, out T component) where T : Component
        {
            component = comp.GetComponentInParent<T>();
            return component != null;
        }
    }
}
