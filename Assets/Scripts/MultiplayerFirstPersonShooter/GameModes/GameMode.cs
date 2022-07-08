using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public abstract class GameMode {

        private static GameMode _GameMode;
        public static GameMode Game {
            get => _GameMode;
            set {
                if( _GameMode != null ){
                    _GameMode.Shutdown();
                }
                _GameMode = value;
            }
        }

        public string gameName = "GameMode";
        public float roundTime = 0f; // 0 is infinite;
        public int roundLimit = 0; // 0 is infinite;
        public int roundsToWin = 0; // 0 is infinite;
        

        public static void SetGameMode(GameMode gameMode){
            Game = gameMode;
            Game.Start();
        }

        public virtual void Start(){
            Debug.Log($"GameMode[{gameName}] Started.");
        }

        public virtual void Tick(){}
        public virtual void Update(){}
        public virtual void Shutdown(){
            Debug.Log($"GameMode[{gameName}] Shutdown.");
        }

        public virtual void RoundStart(){}
        public virtual void RoundEnd(){}
        public virtual void RoundLimitReached(){}
        public virtual void RoundWinLimitReach(){}

        public virtual void OnPlayerSpawn(Player player){}
        public virtual void OnPlayerDeath(Player player){}
        public virtual void PlayerKilledPlayer(Player victim, Player killer){}
        public virtual void PlayerJoined(Player player){}
        public virtual void PlayerLeft(Player player){}

    }

}
