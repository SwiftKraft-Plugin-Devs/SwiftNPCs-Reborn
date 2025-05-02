using CommandSystem.Commands.RemoteAdmin.Dummies;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Mirror;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SwiftNPCs.Features
{
    public class NPC
    {
        public readonly NetworkConnection Connection;
        public readonly Player WrapperPlayer;
        public readonly NPCCore Core;
        public ReferenceHub ReferenceHub => WrapperPlayer.ReferenceHub;
        public PlayerRoleBase RoleBase => WrapperPlayer.RoleBase;

        public bool Respawnable;

        public NPC(Vector3 position, RoleTypeId role = RoleTypeId.Spectator, RoleSpawnFlags spawnFlags = RoleSpawnFlags.AssignInventory) : base()
        {
            ReferenceHub refHub = DummyUtils.SpawnDummy("Bot");
            Connection = refHub.connectionToServer;
            Player.TryGet(refHub.gameObject, out WrapperPlayer);
            NPCManager.AllNPCs.Add(this);
            PlayerEvents.Death += OnDeath;
            WrapperPlayer.SetRole(role, RoleChangeReason.LateJoin, spawnFlags);
            Core = refHub.gameObject.AddComponent<NPCCore>();
            Core.Setup(this);
            WrapperPlayer.Position = position;
        }

        public virtual void Destroy()
        {
            PlayerEvents.Death -= OnDeath;
            NPCManager.AllNPCs.Remove(this);
            NetworkServer.Destroy(ReferenceHub.gameObject);
        }

        protected virtual void OnDeath(PlayerDeathEventArgs ev)
        {
            if (ev.Player != WrapperPlayer)
                return;

            if (!Respawnable)
                Destroy();
        }
    }
}
