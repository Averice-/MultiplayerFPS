using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace ShardStudios {

    public class CameraController : MonoBehaviour
    {

        #if !SERVER

            private static CameraController _instance;
            public static CameraController Instance {
                get => _instance;
                private set {
                    if( _instance == null )
                        _instance = value;
                    else if ( _instance != value ){
                        Debug.Log($"{nameof(CameraController)} instance already exists, destroying duplicate!");
                        Destroy(value);
                    }
                }
            }

            Player player;
            CinemachineVirtualCamera virtualCamera;
            Transform playerEyePos;

            void Awake(){
                Instance = this;
            }

            void Start(){
                virtualCamera = GetComponent<CinemachineVirtualCamera>();
            }

            public static Transform GetCameraTransform(){
                return Instance.transform;
            }


            public static void SetPlayer(Player player){
                if( !player.isAlive )
                    return;

                Instance.playerEyePos = player.playerObject.transform.Find("EyePosition");
                Instance.player = player;

                Instance.transform.position = Instance.playerEyePos.position;
                Instance.transform.rotation = Instance.playerEyePos.rotation;
                Instance.transform.parent = Instance.playerEyePos;

            }

            public static void Detach(){
                Instance.transform.parent = null;
            }

        #endif
    }

}
