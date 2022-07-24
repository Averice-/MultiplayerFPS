using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

    public class Player
    {

        public static Dictionary<ushort, Player> playerList = new Dictionary<ushort, Player>();

        public ushort id;
        public NetworkedPlayer playerObject;
        public string name;

        public bool isLocalPlayer = false;

        // Gameplay variables.
        public bool isAlive = false;
        public bool lastPrimaryAttackMessage = false;
        public bool lastSecondaryAttackMessage = false;
        public EquipmentManager equipment;
        public Health health;
        public AnimationHandler handler;

        public Player(ushort id, string name = "nigger"){

            this.id = id;
            this.name = name;

            #if SERVER

                BroadcastPlayers(id);
                
                Message message = Message.Create(MessageSendMode.reliable, MessageID.PlayerJoined);
                message.AddUShort(id);
                message.AddString(name);

                NetworkManager.GameServer.Server.SendToAll(message);

            #else

                if( NetworkManager.GameClient.Client.Id == id ){
                    this.isLocalPlayer = true;
                }

            #endif

            playerList.Add(id, this);

        }

        #if SERVER

            public static void BroadcastPlayers(ushort id){

                foreach(KeyValuePair<ushort, Player> player in playerList){

                    Message message = Message.Create(MessageSendMode.reliable, MessageID.PlayerJoined);
                    message.Add(player.Key);
                    message.Add(player.Value.name);

                    NetworkManager.GameServer.Server.Send(message, id);

                    if( player.Value.isAlive && player.Key != id ){

                        Message spawnMessage = Message.Create(MessageSendMode.reliable, MessageID.PlayerSpawned);
                        spawnMessage.AddUShort(player.Key);
                        spawnMessage.AddUInt(player.Value.playerObject.id);

                        NetworkManager.GameServer.Server.Send(spawnMessage, id);

                    }

                }

            }

            public void Spawn(Vector3 position, Quaternion rotation){

                playerObject = (NetworkedPlayer)NetworkedEntity.Create("ClientPlayer", position, rotation, this.id);

                handler = playerObject.GetComponent<AnimationHandler>();

                equipment = playerObject.GetComponent<EquipmentManager>();
                equipment.SetOwner(this);

                health = playerObject.GetComponent<Health>();
                health.OnHealthZero.AddListener(PlayerKilledPlayer);
                health.OnTakeDamage.AddListener(PlayerTakeDamage);

                isAlive = true;

                Message message = Message.Create(MessageSendMode.reliable, MessageID.PlayerSpawned);
                message.AddUShort(id);
                message.AddUInt(playerObject.id);

                NetworkManager.GameServer.Server.SendToAll(message);

                GameMode.Game.OnPlayerSpawn(this);
            }


            [MessageHandler((ushort)MessageID.PlayerReady)]
            public static void PlayerReady(ushort from, Message message){

                NetworkedEntity.Broadcast(from); // all spawned networked entities.
                Player newPlayer = new Player(from, message.GetString()); // all connected players will be broadcast to me.
                GameMode.Game.PlayerJoined(newPlayer); // call gamemode playerjoined for functionality.
                EquipmentManager.Broadcast(from); // all player weapons and selected weapon

                //playerList[from].Spawn(new Vector3(0f, 1f, 0f), Quaternion.identity); // TEMP
            }

            public static void NetworkGiveMessage(Player player, string equipmentName){

                Message message = Message.Create(MessageSendMode.reliable, MessageID.PlayerGiveItem);

                message.AddUShort(player.id);
                message.AddString(equipmentName);

                NetworkManager.GameServer.Server.SendToAll(message);

            }

        #endif

        #if !SERVER

            [MessageHandler((ushort)MessageID.PlayerJoined)]
            public static void ReceivePlayer(Message message){
                Player joinedPlayer = new Player(message.GetUShort(), message.GetString());
                GameMode.Game.PlayerJoined(joinedPlayer);
            }

            [MessageHandler((ushort)MessageID.PlayerGiveItem)]
            public static void PlayerGiveItem(Message message){
                Player playerToGive = GetById(message.GetUShort());
                if( playerToGive != null ){
                    playerToGive.Give(message.GetString());
                }
            }

            [MessageHandler((ushort)MessageID.PlayerSpawned)]
            public static void PlayerSpawned(Message message){

                Player spawnedPlayer = GetById(message.GetUShort());

                spawnedPlayer.playerObject = (NetworkedPlayer)NetworkedEntity.GetEntityById(message.GetUInt());

                spawnedPlayer.handler = spawnedPlayer.playerObject.GetComponent<AnimationHandler>();
                if( spawnedPlayer.isLocalPlayer )
                    InputController.Instance.SetAnimationHandler(spawnedPlayer.handler);

                spawnedPlayer.equipment = spawnedPlayer.playerObject.GetComponent<EquipmentManager>();
                spawnedPlayer.equipment.SetOwner(spawnedPlayer);

                spawnedPlayer.health = spawnedPlayer.playerObject.GetComponent<Health>();
                spawnedPlayer.health.OnHealthZero.AddListener(spawnedPlayer.PlayerKilledPlayer);
                spawnedPlayer.health.OnTakeDamage.AddListener(spawnedPlayer.PlayerTakeDamage);


                GameMode.Game.OnPlayerSpawn(spawnedPlayer);
                spawnedPlayer.isAlive = true;

                if( spawnedPlayer.isLocalPlayer ){
                    CameraController.SetPlayer(spawnedPlayer);
                    // Setup camera.
                }
            }

            [MessageHandler((ushort)MessageID.PlayerPrimaryAttacked)]
            public static void PlayerPrimaryAttacked(Message message){
                Player player = GetById(message.GetUShort());
                bool isAttacking = message.GetByte() == (byte)1;
                player.PrimaryAttack(isAttacking);
            }
            

        #endif

        public void PlayerTakeDamage(Player attacker, float damage){
            Debug.Log($"Player[{id}] damaged[{damage}] by Player[{attacker.id}]");
        }

        public void PlayerKilledPlayer(Player attacker){
            GameMode.Game.OnPlayerDeath(this);
            if( attacker != null )
                GameMode.Game.PlayerKilledPlayer(this, attacker);

            Kill();
        }

        public static Player GetById(ushort id){
            if( playerList.ContainsKey(id) ){
                return playerList[id];
            }
            return null;
        }

        public static List<Player> GetAll(){
            return playerList.Values.ToList();
        }

        public void Give(string item){

            GameObject newItem = (GameObject)NetworkManager.Instantiate(Resources.Load($"Equipment/{item}"), playerObject.transform);
            EquipmentItem equipmentItem = newItem.GetComponent(item) as EquipmentItem;
            equipment.GiveItem(equipmentItem);
            Debug.Log($"Giving player[{id}] item[{item}]");
            #if SERVER
                NetworkGiveMessage(this, item);
            #endif

        }

        public void PrimaryAttack(bool isAttacking = true){
            if( !isAlive )
                return;

            if( isAttacking == lastPrimaryAttackMessage )
                return;

            if( equipment != null ){
                EquipmentItem equipmentItem = equipment.GetEquippedItem();
                if( equipmentItem != null ){   
                    lastPrimaryAttackMessage = isAttacking;
                    equipmentItem.OnPrimaryAttack(isAttacking);
                    #if SERVER
                        Message networkAttack = Message.Create(MessageSendMode.reliable, MessageID.PlayerPrimaryAttacked);
                        networkAttack.AddUShort(id);
                        networkAttack.AddByte(isAttacking ? (byte)1 : (byte)0);

                        NetworkManager.GameServer.Server.SendToAll(networkAttack, id);
                    #endif
                }
            }
        }

        public void Kill(){

            #if !SERVER
                if( this.isLocalPlayer ){
                    CameraController.Detach();
                }
            #endif

            playerObject.DestroyNetworkedEntity();
            playerObject = null;
            equipment = null;
            health = null;
            isAlive = false;
            lastPrimaryAttackMessage = false;
            lastSecondaryAttackMessage = false;
            
        }

        
    }

}
