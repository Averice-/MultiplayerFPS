using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

    public class Player
    {

        public static Dictionary<ushort, Player> playerList = new Dictionary<ushort, Player>();

        public ushort id;
        public GameObject playerObject;
        public string name;

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
                
                playerObject = NetworkedEntity.Create("ClientPlayer", position, rotation, this.id);
            }

            public void Kill(){
                playerObject.GetComponent<NetworkedEntity>().DestroyNetworkedEntity();
            }

            [MessageHandler((ushort)MessageID.PlayerReady)]
            public static void PlayerReady(ushort from, Message message){
                playerList[from].Spawn(new Vector3(0f, 1f, 0f), Quaternion.identity);
            }

        #endif

        #if !SERVER

            [MessageHandler((ushort)MessageID.PlayerJoined)]
            public static void ReceivePlayer(Message message){
                new Player(message.GetUShort(), message.GetString());
            }

            

        #endif

        
    }

}
