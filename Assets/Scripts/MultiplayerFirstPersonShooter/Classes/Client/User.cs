using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

#if !SERVER

    public class User
    {

        // Saved Details
        public RegionID selectedRegion;
        public string username = "Averice";
        public string password;

        // Temp Details;
        public string game_ip;
        public ushort game_port;
        public ushort game_maxplayers;
        public ushort game_playercount;
        public string game_mapname;
        public string game_gamename;

        public bool connectedToGameServer = false;

        Client MasterServer;

        public void Init(){
            username = PlayerPrefs.GetString("username", "username");
            password = PlayerPrefs.GetString("password", "password");
            selectedRegion = (RegionID)PlayerPrefs.GetInt("region", 0);
        }

        public void SelectRegion(int region){
            selectedRegion = (RegionID)region;
            PlayerPrefs.Save();
        }

        public void SetUsernamePassword(string name, string pass){
            // hash this password;
            username = name;
            password = pass;
            PlayerPrefs.Save();
        }

        public void UserToMasterServer(){
            NetworkManager.MasterConnection.Connect();
            MasterServer = NetworkManager.MasterConnection.Client;

            Message message = Message.Create(MessageSendMode.reliable, MessageID.UserConnected);
            message.AddString(username);
            message.AddString(password);

            NetworkManager.MasterConnection.Client.Send(message);
        }

        [MessageHandler((ushort)MessageID.NewConnectionDetails)]
        private static void UserToGameServer(Message message){

            NetworkManager.User.game_ip = message.GetString();
            NetworkManager.User.game_port = message.GetUShort();
            NetworkManager.User.game_maxplayers = message.GetUShort();
            NetworkManager.User.game_playercount = message.GetUShort();
            NetworkManager.User.game_mapname = message.GetString();
            NetworkManager.User.game_gamename = message.GetString();

            NetworkManager.GameClient.Client.Connect($"{NetworkManager.User.game_ip}:{NetworkManager.User.game_port}");

        }

        public void RequestQuickServer(){
            Message message = Message.Create(MessageSendMode.reliable, MessageID.UserRequestQuickServer);
            MasterServer.Send(message);
        }

        public void Reset(){
            connectedToGameServer = false;
        }
        
    }

#endif

}
