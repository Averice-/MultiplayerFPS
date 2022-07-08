using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class BaseGameMode : GameMode
    {

        private const int playersToStart = 1;

        private bool hasStarted = false;
        private int playersReady = 0;

        public override void Start(){
            base.Start();
        }

        public override void Update(){
            base.Update();

            if( !hasStarted && playersReady >= playersToStart ){
                hasStarted = true;
                RoundStart();             
            }
        }

        public override void RoundStart(){
            base.RoundStart();

            Debug.Log($"Started Round[{gameName}]");
        }

        public override void PlayerJoined(Player player){
            Debug.Log($"Player[{player.id}][{player.name}] connected to the server.");
            playersReady++;
        }

        public override void PlayerLeft(Player player){
            playersReady--;
        }

        public override void OnPlayerSpawn(Player player){
            // #if SERVER
            //     player.Give("weapon_m4a1");
            // #endif
        }

        // Need to work out when to give player weapons.. it seems EquipmentManager isn't there yet when we try!

    }

}
