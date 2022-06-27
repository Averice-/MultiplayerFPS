using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RiptideNetworking;

namespace ShardStudios {

#if SERVER
    public class GameServer
    {

        public Server Server { get; private set; }

        public ushort maxClients = 32;
        public string serverIp = "127.0.0.1";
        public ushort serverPort = 7778;
        public string mapName = "SampleScene";
        public ushort playerCount = 0;

        Client MasterServer;

        public GameServer(){

            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++){
                if (args[i] == "--mapname" && args.Length > i + 1){
                    mapName = args[i + 1];
                }else if( args[i] == "--ip" && args.Length > i + 1){
                    serverIp = args[i + 1];
                }else if( args[i] == "--port" && args.Length > i + 1){
                    serverPort = (ushort)int.Parse(args[i + 1]);
                }else if( args[i] == "--maxclients" && args.Length > i + 1){
                    maxClients = (ushort)int.Parse(args[i + 1]);
                }else if( args[i] == "--region" && args.Length > i + 1){
                    NetworkManager.Instance.region = (RegionID)int.Parse(args[i + 1]);
                }
            }

        }

        public void Start(){

            MasterServer = NetworkManager.MasterConnection.Client;
            Server = new Server();
            Server.Start(serverPort, maxClients);

            Server.ClientConnected += ServerPlayerConnected;
            Server.ClientDisconnected += ServerPlayerDisconnected;

            // Connect to MasterServer
            NetworkManager.Instance.StartCoroutine(LoadMap(mapName));

        }

        public void ServerPlayerConnected(object sender, ServerClientConnectedEventArgs e){
            Message message = Message.Create(MessageSendMode.reliable, MessageID.GameServerAddPlayer);
            MasterServer.Send(message);
        }

        public void ServerPlayerDisconnected(object sender, ClientDisconnectedEventArgs e){
            Message message = Message.Create(MessageSendMode.reliable, MessageID.GameServerSubPlayer);
            MasterServer.Send(message);
        }

        IEnumerator LoadMap(string mapname){
            //Cleanup previous map things.
            AsyncOperation loading = SceneManager.LoadSceneAsync(mapname);
            while( !loading.isDone ){
                yield return null;
            }
            Message message = Message.Create(MessageSendMode.reliable, MessageID.GameServerMapChange);
            message.AddString(mapname);

            MasterServer.Send(message);
        }

        public void Tick(){
            Server.Tick();
        }

        public void Kill(){

            Server.Stop();
            Server.ClientDisconnected -= ServerPlayerDisconnected;
            Server.ClientConnected -= ServerPlayerConnected;

        }


    }

#endif
}
