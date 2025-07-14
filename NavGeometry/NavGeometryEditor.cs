using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace SwiftNPCs.NavGeometry
{
    public static class NavGeometryEditor
    {
        public readonly static LayerMask GeoLayers = LayerMask.GetMask("Default", "Door", "Glass", "Fence");

        public readonly static List<Editor> Editors = [];

        private static void OnShootingWeapon(PlayerShootingWeaponEventArgs ev)
        {
            for (int i = 0; i < Editors.Count; i++)
                Editors[i].ShootingWeapon(ev);
        }

        private static void OnAimedWeapon(PlayerAimedWeaponEventArgs ev)
        {
            for (int i = 0; i < Editors.Count; i++)
                Editors[i].AimedWeapon(ev);
        }

        private static void OnReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
        {
            for (int i = 0; i < Editors.Count; i++)
                Editors[i].ReloadingWeapon(ev);
        }

        private static void Tick()
        {
            for (int i = 0; i < Editors.Count; i++)
                Editors[i].Tick();
        }

        public static void Enable()
        {
            PlayerEvents.ShootingWeapon += OnShootingWeapon;
            PlayerEvents.AimedWeapon += OnAimedWeapon;
            PlayerEvents.ReloadingWeapon += OnReloadingWeapon;

            StaticUnityMethods.OnFixedUpdate += Tick;
        }

        public static void Disable()
        {
            PlayerEvents.ShootingWeapon -= OnShootingWeapon;
            PlayerEvents.AimedWeapon -= OnAimedWeapon;
            PlayerEvents.ReloadingWeapon -= OnReloadingWeapon;

            StaticUnityMethods.OnFixedUpdate -= Tick;
        }

        public static void GiveEditor(Player player)
        {
            Item it = player.AddItem(ItemType.GunCOM18);
            Logger.Info(it.Serial);
            Editors.Add(new(it.Serial));
        }

        public static void RegisterEditMode<T>() where T : EditModeBase, new() => Editor.RegisterEditMode<T>();

        public class Editor
        {
            private static readonly HashSet<Type> EditModeTypes = [];

            public readonly ushort ItemSerial;
            public readonly List<EditModeBase> EditModes = [];
            public readonly Stack<EditAction> Actions = [];

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
                    if (!t.IsAbstract)
                    {
                        EditModeBase b = (EditModeBase)Activator.CreateInstance(t);
                        b.Init(this);
                        EditModes.Add(b);
                        Logger.Info(b.GetType());
                    }
            }

            public static void RegisterEditMode<T>() where T : EditModeBase, new() => EditModeTypes.Add(typeof(T));

            public void Tick()
            {
                if (EditModes.Count > 0)
                    EditModes[CurrentMode].Tick();
            }

            public void ShootingWeapon(PlayerShootingWeaponEventArgs ev)
            {
                if (ev.FirearmItem.Serial != ItemSerial)
                    return;

                ev.IsAllowed = false;

                if (EditModes.Count <= 0)
                    return;

                Actions.Push(EditModes[CurrentMode].Action(ev.Player, Physics.Raycast(ev.Player.Camera.position, ev.Player.Camera.forward,
                    out RaycastHit _hit, 5f, GeoLayers, QueryTriggerInteraction.Ignore), _hit));
            }

            public void AimedWeapon(PlayerAimedWeaponEventArgs ev)
            {
                if (ev.FirearmItem.Serial != ItemSerial || !ev.Aiming)
                    return;

                CurrentMode++;
            }

            public void ReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
            {
                if (ev.FirearmItem.Serial != ItemSerial)
                    return;

                ev.IsAllowed = false;
                if (Actions.Count > 0)
                    Actions.Pop().Undo?.Invoke();
            }
        }

        public abstract class EditModeBase
        {
            public Editor Parent { get; private set; }

            public void Init(Editor parent) => Parent = parent;

            public abstract EditAction Action(Player p, bool hasHit, RaycastHit hit);
            public abstract void Tick();
        }

        public struct EditAction(Action undo)
        {
            public Action Undo = undo;
        }
    }
}
