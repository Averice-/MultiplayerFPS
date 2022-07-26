using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class BaseGameMode : GameMode
    {

        private const int playersToStart = 1;

        private bool hasStarted = false;
        private int playersReady = 0;
        private int playersAlive = 0;

        private float spawnTime = 3f;

        public override void Start(){
            base.Start();
        }

        public override void Update(){
            base.Update();

            if( !hasStarted && playersReady >= playersToStart ){
                hasStarted = true;
                RoundStart();             
            }

            #if SERVER
            
            #endif
        }

        public override void RoundStart(){
            base.RoundStart();
            playersAlive = 0;

            #if SERVER
                foreach( Player ply in Player.GetAll() ){
                    ply.Spawn(new Vector3(0f, 1f, 0f), Quaternion.identity);
                }
            #endif

            Debug.Log($"Started Round[{gameName}]");
        }

        public override void PlayerJoined(Player player){
            Debug.Log($"Player[{player.id}][{player.name}] connected to the server.");

            #if SERVER
                if( hasStarted ){
                    Spawn(player);
                }
            #endif
            playersReady++;
        }

        public override void PlayerLeft(Player player){
            playersReady--;
        }

        public override void OnPlayerSpawn(Player player){
            playersAlive++;
            #if SERVER
                player.Give("weapon_m4a1");
                player.Give("weapon_pistol");
            #else
                if( player.isLocalPlayer )
                    Cursor.lockState = CursorLockMode.Locked;
            #endif
        }

        public override void OnPlayerDeath(Player player){
            playersAlive--;
            #if SERVER
                NetworkManager.Instance.StartCoroutine(Respawn(player));
            #endif
        }

        #if SERVER

            IEnumerator Respawn(Player player){

                yield return new WaitForSeconds(spawnTime);
                Spawn(player);

            }

        #endif

    }

}
