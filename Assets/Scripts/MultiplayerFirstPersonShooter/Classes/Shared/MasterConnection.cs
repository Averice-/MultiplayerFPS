using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {


    public class MasterConnection
    {
       
        public Client Client { get; private set; }
        public bool connectedToMaster = false;

        public void Start(){
            Client = new Client();
            Client.Connected += MasterConnected;
            Client.ConnectionFailed += MasterConnectionFailed;
            Client.Disconnected += MasterDisconnected;
        }

        public void Connect(){
            string regionIP = NetworkManager.Instance.regionIPAddress[(int)NetworkManager.Instance.region];
            Client.Connect($"{regionIP}:{NetworkManager.Instance.port}");

            #if SERVER

                GameServer gServ = NetworkManager.GameServer;
                Message message = Message.Create(MessageSendMode.reliable, MessageID.GameServerStartup);
                message.AddString(gServ.serverIp);
                message.AddUShort(gServ.serverPort);
                message.AddUShort(gServ.playerCount);
                message.AddUShort(gServ.maxClients);
                message.AddString(gServ.mapName);
                message.AddString(gServ.gameName);

                Client.Send(message);

                // Load map.

            #endif

        }

        public void Kill(){
            Client.Disconnect();
            Client.Connected -= MasterConnected;
            Client.ConnectionFailed -= MasterConnectionFailed;
            Client.Disconnected -= MasterDisconnected;
        }

        public void Tick(){
            Client.Tick();
        }

        public void MasterConnected(object sender, EventArgs e){
            Debug.Log("Connected to MasterServer!");
            connectedToMaster = true;
        }

        public void MasterConnectionFailed(object sender, EventArgs e){
            Debug.Log("Connection to MasterServer Failed!\nRetrying!");
            Connect();
        }

        public void MasterDisconnected(object sender, EventArgs e){
            Debug.Log("Disconnected from MasterServer!");
            connectedToMaster = false;
        }
    }

}
