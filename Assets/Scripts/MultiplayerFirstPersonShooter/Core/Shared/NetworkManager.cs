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
        NetworkedEntitySpawned,
        UpdateTick,
        ClientSendInput,
        ReceiveSimulationState,
        PlayerJoined,
        PlayerReady,
        ReceiveOwnSimulationState,
        PlayerSpawned,
        PlayerGiveItem,
        PlayerPrimaryAttacked,
        PlayerChangeWeapon,
        PlayerUpdateSelectedWeapon,
        EntityTakeDamage
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

        public static uint tick = 0;
        public static bool connectedAndReady = false;

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

        public static void Ready(){
            connectedAndReady = true;
            GameMode.Game = new BaseGameMode(); // TEMPORARY, start this based on --gamemode or gServ.gameName;
            GameMode.Game.Start();
        }

        void FixedUpdate(){

            tick++;

            if( connectedAndReady )
                GameMode.Game.Tick();

            #if SERVER

                if( tick % 33 == 0){
                    SendTick();
                }

            #endif

        }

        void Update(){

            if( connectedAndReady )
                GameMode.Game.Update();

            #if SERVER
                GameServer.Tick();
            #else
                GameClient.Tick();
            #endif
            MasterConnection.Tick();

        }

        #if SERVER
            private static void SendTick(){
                Message message = Message.Create(MessageSendMode.unreliable, MessageID.UpdateTick);
                message.AddUInt(tick);

                GameServer.Server.SendToAll(message);
            }
        #else
            [MessageHandler((ushort)MessageID.UpdateTick)]
            private static void UpdateTick(Message message){
                tick = message.GetUInt();
            }
        #endif

        private void OnApplicationQuit(){
            if( connectedAndReady )
                GameMode.Game.Shutdown();

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
