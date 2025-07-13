using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SwiftNPCs.NavGeometry
{
    public static class NavGeometryEditor
    {
        public readonly static LayerMask GeoLayers = LayerMask.GetMask("Default", "Door", "Glass", "Fence");

        public readonly static List<Editor> Editors = [];

        private static void OnShotWeapon(PlayerShotWeaponEventArgs ev)
        {
            for (int i = 0; i < Editors.Count; i++)
                Editors[i].ShotWeapon(ev);
        }

        public static void Enable()
        {
            PlayerEvents.ShotWeapon += OnShotWeapon;
        }

        public static void Disable()
        {
            PlayerEvents.ShotWeapon -= OnShotWeapon;
        }

        public static void GiveEditor(Player player)
        {
            Item it = player.AddItem(ItemType.GunCOM18);
            Editors.Add(new(it.Serial));
        }

        public static void RegisterEditMode<T>() where T : EditModeBase, new() => Editor.RegisterEditMode<T>();

        public class Editor
        {
            private static readonly HashSet<Type> EditModeTypes = [];

            public readonly ushort ItemSerial;
            public readonly List<EditModeBase> EditModes = [];
            public readonly Stack<EditAction> Actions = [];
            public readonly Stack<EditAction> Undid = [];

            public int CurrentMode
            {
                get => currentMode;
                set
                {
                    if (value < 0 || EditModes.Count <= 0)
                        return;

                    currentMode = value % EditModes.Count;
                }
            }
            private int currentMode;

            public Editor(ushort serial)
            {
                ItemSerial = serial;
                foreach (Type t in EditModeTypes)
                    if (t.IsAssignableFrom(typeof(EditModeBase)) && !t.IsAbstract)
                        EditModes.Add((EditModeBase)Activator.CreateInstance(t, this));
            }

            public static void RegisterEditMode<T>() where T : EditModeBase, new() => EditModeTypes.Add(typeof(T));

            public void ShotWeapon(PlayerShotWeaponEventArgs ev)
            {
                if (ev.FirearmItem.Serial != ItemSerial || EditModes.Count <= 0
                    || !Physics.Raycast(ev.Player.Camera.position, ev.Player.Camera.forward,
                    out RaycastHit _hit, 5f, GeoLayers, QueryTriggerInteraction.Ignore))
                    return;

                Actions.Push(EditModes[CurrentMode].Action(ev.Player, _hit));
                Undid.Clear();
            }
        }

        public abstract class EditModeBase(Editor parent)
        {
            public readonly Editor Parent = parent;
            public abstract EditAction Action(Player p, RaycastHit hit);
        }

        public struct EditAction(Action undo, Action redo)
        {
            public Action Undo = undo;
            public Action Redo = redo;
        }
    }
}
