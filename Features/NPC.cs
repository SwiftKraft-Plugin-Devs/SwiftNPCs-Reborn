using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using Mirror;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using UnityEngine;

namespace SwiftNPCs.Features
{
    public class NPC
    {
        public const string DefaultName = "Bot";
        public NPCCore Core { get; private set; }
        public ReferenceHub ReferenceHub { get; private set; }
        public Player WrapperPlayer => Player.Get(ReferenceHub);
        public PlayerRoleBase RoleBase => ReferenceHub.roleManager.CurrentRole;

        public bool Respawnable;
        public bool Disposed => ReferenceHub == null;

        public Vector3 Position
        {
            get
            {
                if (RoleBase is not IFpcRole fpcRole)
                {
                    return Vector3.zero;
                }

                return fpcRole.FpcModule.Position;
            }
            set
            {
                ReferenceHub.TryOverridePosition(value);
            }
        }

        public Room Room => Room.GetRoomAtPosition(Position);

        public FacilityZone Zone => Room?.Zone ?? FacilityZone.None;

        public NPC(Vector3 position, string name = DefaultName, RoleTypeId role = RoleTypeId.Spectator, RoleSpawnFlags spawnFlags = RoleSpawnFlags.AssignInventory) : base()
        {
            ReferenceHub refHub = DummyUtils.SpawnDummy(name);
            ReferenceHub = refHub;
            NPCManager.AllNPCs.Add(this);
            PlayerEvents.Death += OnDeath;
            Core = refHub.gameObject.AddComponent<NPCCore>();
            Core.Setup(this);
            ReferenceHub.roleManager.ServerSetRole(role, RoleChangeReason.LateJoin, spawnFlags);
            Core.Position = position;
            Core.SetupComponents();
            Timing.CallDelayed(0.05f, () =>
            {
                ReferenceHub.serverRoles.RefreshLocalBadgeVisibility(ServerRoles.BadgeVisibilityPreferences.Hidden);
            });
        }

        public virtual void Destroy()
        {
            PlayerEvents.Death -= OnDeath;
            NPCManager.AllNPCs.Remove(this);
            NetworkServer.Destroy(ReferenceHub.gameObject);
        }

        protected virtual void OnDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player.ReferenceHub != ReferenceHub)
                return;

            if (!Respawnable)
                Destroy();
        }
    }
}
