using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class PlayerHud : MonoBehaviour
    {
        private static PlayerHud _instance;
        public static PlayerHud Instance {
            get => _instance;
            private set {
                if( _instance == null )
                    _instance = value;
                else if ( _instance != value ){
                    Debug.Log($"{nameof(PlayerHud)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        void Awake(){
            Instance = this;
        }

        public Transform playerHudCanvas;
        public Crosshair crosshair;

        public static Crosshair CreateCrosshair(string crosshairName = "CrosshairDefault"){

            if( Instance.crosshair != null ){
                Destroy(Instance.crosshair.gameObject);
                Instance.crosshair = null;
            }

            GameObject newCrosshair = (GameObject)Instantiate(Resources.Load($"HudElements/Crosshairs/{crosshairName}"), Instance.playerHudCanvas);
            Instance.crosshair = newCrosshair.GetComponent<Crosshair>();

            return Instance.crosshair;
            
        }

    }

}
