using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShardStudios {

    public enum RegionID : ushort {
        AU = 0
    }

    public enum MessageID : ushort {
        GameServerStartup = 1,
        UserConnected,
        GameServerMapChange,
        NewConnectionDetails,
        GameServerAddPlayer,
        GameServerSubPlayer,
        UserRequestQuickServer,
        NetworkedEntitySpawned
    }

    public class NetworkManager : MonoBehaviour
    {

        private static NetworkManager _instance;
        public static NetworkManager Instance {
            get => _instance;
            private set {
                if( _instance == null )
                    _instance = value;
                else if ( _instance != value ){
                    Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        public string[] regionIPAddress = new string[] {"127.0.0.1"};
        
        public static MasterConnection MasterConnection = new MasterConnection();
        public RegionID region = RegionID.AU;

        #if SERVER
            public static GameServer GameServer = new GameServer();
        #else
            public static GameClient GameClient = new GameClient();
            public static User User = new User();
        #endif

        public ushort port = 7777;
        

        void Awake(){

            #if SERVER
                Debug.Log("Server Starting!");
            #endif

            Instance = this;
            DontDestroyOnLoad(this.gameObject);

        }

        void Start(){

            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
            MasterConnection.Start();
            #if SERVER
                MasterConnection.Connect();
                GameServer.Start();
            #else
                GameClient.Start();
                // Temporary this'll be done when menu is made.
                User.Init();
                region = User.selectedRegion;
                User.UserToMasterServer();
                // End Temporary
            #endif


        }


        void Update(){

            #if SERVER
                GameServer.Tick();
            #else
                GameClient.Tick();
            #endif
            MasterConnection.Tick();

        }

        private void OnApplicationQuit(){

            #if SERVER
                GameServer.Kill();
            #else
                if( GameClient.IsConnected() ){
                    GameClient.Kill();
                }
            #endif
            MasterConnection.Kill();

        }



    }

}