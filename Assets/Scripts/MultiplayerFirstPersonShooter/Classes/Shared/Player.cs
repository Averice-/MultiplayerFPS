using System.Collections;
using System.Collections.Generic;
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
        public bool isAlive = true;

        // Temp
        public Transform hand;

        public EquipmentManager equipment;

        public Player(ushort id, string name = "nigger"){

            this.id = id;
            this.name = name;

            #if SERVER

                BroadcastPlayers(id);
                
                Message message = Message.Create(MessageSendMode.reliable, MessageID.PlayerJoined);
                message.AddUShort(id);
                message.AddString(name);

                NetworkManager.GameServer.Server.SendToAll(message);

                //Spawn(new Vector3(0f, 1f, 0f), Quaternion.identity);

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

                }

            }

            public void Spawn(Vector3 position, Quaternion rotation){
                playerObject = (NetworkedPlayer)NetworkedEntity.Create("ClientPlayer", position, rotation, this.id);
                equipment = playerObject.GetComponent<EquipmentManager>();

                Message message = Message.Create(MessageSendMode.reliable, MessageID.PlayerSpawned);
                message.AddUShort(id);
                message.AddUInt(playerObject.id);

                NetworkManager.GameServer.Server.SendToAll(message);

                GameMode.Game.OnPlayerSpawn(this);
            }

            public void Kill(){
                playerObject.DestroyNetworkedEntity();
            }

            [MessageHandler((ushort)MessageID.PlayerReady)]
            public static void PlayerReady(ushort from, Message message){
                Player newPlayer = new Player(from, message.GetString());
                NetworkedEntity.Broadcast(from);
                GameMode.Game.PlayerJoined(newPlayer);
                playerList[from].Spawn(new Vector3(0f, 1f, 0f), Quaternion.identity); // TEMP
            }

            public static void NetworkGiveMessage(Player player, string equipmentName){

                Message message = Message.Create(MessageSendMode.reliable, MessageID.PlayerGiveItem);

                message.AddUShort(player.id);
                message.AddString(equipmentName);

                NetworkManager.GameServer.Server.SendToAll(message);
                Debug.Log("Give message sent.");

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
                spawnedPlayer.equipment = spawnedPlayer.playerObject.GetComponent<EquipmentManager>();
                GameMode.Game.OnPlayerSpawn(spawnedPlayer);
                if( spawnedPlayer.isLocalPlayer ){
                    // Setup camera.
                }
            }

            [MessageHandler((ushort)MessageID.PlayerPrimaryAttacked)]
            public static void PlayerPrimaryAttacked(Message message){
                Player player = GetById(message.GetUShort());
                EquipmentItem playerEquippedItem = player.equipment.GetEquippedItem();
                if( playerEquippedItem != null ){
                    playerEquippedItem.OnPrimaryAttack();
                }
            }

            

        #endif


        public static Player GetById(ushort id){
            if( playerList.ContainsKey(id) ){
                return playerList[id];
            }
            return null;
        }

        // Already created item.
        public void Give(EquipmentItem item){
            equipment.GiveItem(item);

            #if SERVER
                NetworkGiveMessage(this, item.equipmentName);
            #endif
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

        
    }

}
