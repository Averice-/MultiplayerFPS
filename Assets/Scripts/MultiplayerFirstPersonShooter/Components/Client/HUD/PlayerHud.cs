using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace ShardStudios {

    public class PlayerHud : MonoBehaviour
    {

        #if !SERVER

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

            private float hitLifeTime = 0f;

            public Transform playerHudCanvas;
            public AudioSource audioSource;

            [Header("Crosshair Settings")]
            public Crosshair crosshair;
            public GameObject hitMarkerImage;
            public float hitMarkerLifetime = 0.05f;

            [Header("Kill Feed Setup")]
            public GameObject killFeedPrefab;
            public GameObject killFeedObject;
            public GameObject killFeedMessage;

            [Space(10)]
            [Header("HUDSounds")]
            public AudioClip hitMarkerSound;


            public static Crosshair CreateCrosshair(string crosshairName = "CrosshairDefault"){

                if( Instance.crosshair != null ){
                    Destroy(Instance.crosshair.gameObject);
                    Instance.crosshair = null;
                }

                if( GameMode.Game.AllowCrosshairs ){
                    GameObject newCrosshair = (GameObject)Instantiate(Resources.Load($"HudElements/Crosshairs/{crosshairName}"), Instance.playerHudCanvas);
                    Instance.crosshair = newCrosshair.GetComponent<Crosshair>();

                    return Instance.crosshair;
                }

                return null;
                
            }

            public static void HitMarker(){
                if( GameMode.Game.AllowHitMarkers ){
                    Instance.audioSource.PlayOneShot(Instance.hitMarkerSound);
                    Instance.hitMarkerImage.SetActive(true);
                    Instance.hitLifeTime = Instance.hitMarkerLifetime;
                }
            }

            void Update(){
                if( hitLifeTime > 0 ){
                    hitLifeTime -= Time.deltaTime;
                    if( hitLifeTime <= 0 ){
                        hitMarkerImage.SetActive(false);
                    }
                }
            }

            public static void CreateKillFeed(){
                if( GameMode.Game.AllowKillFeed ){
                    Instance.killFeedObject = Instantiate(Instance.killFeedPrefab, Instance.playerHudCanvas);
                }
            }

            public static void CreateKillFeedMessage(Sprite weaponImage, string attacker, string victim){
                KillFeedMessage kfm = Instantiate(Instance.killFeedMessage, Instance.killFeedObject.transform).GetComponent<KillFeedMessage>();
                kfm.Setup(weaponImage, attacker, victim);
            }

        
        #endif
    }

}
