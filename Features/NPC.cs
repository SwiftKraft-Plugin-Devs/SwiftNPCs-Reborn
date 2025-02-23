using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Mirror;
using NetworkManagerUtils;
using PlayerRoles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SwiftNPCs.Features
{
    public class NPC
    {
        public readonly NPCConnection Connection;
        public readonly Player WrapperPlayer;
        public readonly NPCCore Core;
        public ReferenceHub ReferenceHub => WrapperPlayer.ReferenceHub;
        public PlayerRoleBase RoleBase => WrapperPlayer.RoleBase;

        public bool Respawnable;

        public NPC(Vector3 position) : base()
        {
            GameObject playerBody = Object.Instantiate(NetworkManager.singleton.playerPrefab, position, Quaternion.identity);
            Connection = new();
            NetworkServer.AddPlayerForConnection(Connection, playerBody);
            Player.TryGet(playerBody, out WrapperPlayer);
            Core = playerBody.AddComponent<NPCCore>();
            NPCManager.AllNPCs.Add(this);
            PlayerEvents.Death += OnDeath;
            WrapperPlayer.DisplayName = "Bot " + WrapperPlayer.PlayerId;
        }

        public NPC(Vector3 position, RoleTypeId role, RoleSpawnFlags spawnFlags = RoleSpawnFlags.AssignInventory) : this(position) => WrapperPlayer.SetRole(role, RoleChangeReason.LateJoin, spawnFlags);

        public virtual void Destroy()
        {
            PlayerEvents.Death -= OnDeath;
            NPCManager.AllNPCs.Remove(this);
            NetworkServer.RemovePlayerForConnection(Connection, true);
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
