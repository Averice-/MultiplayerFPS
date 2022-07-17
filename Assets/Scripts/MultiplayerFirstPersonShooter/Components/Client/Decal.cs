using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class Decal : MonoBehaviour
    {
        
        [SerializeField] MeshRenderer meshRenderer;

        Material decalMaterial;
        float lifeTime = 5f;
        float timeLeft = 0f;

        void Awake(){

            //#if UNITY_EDITOR
            //    decalMaterial = meshRenderer.sharedMaterial;
            //#else
                decalMaterial = meshRenderer.material;
            //#endif

        }

        public void SetOffset(Vector2 offsetVector){
            decalMaterial.mainTextureOffset = offsetVector * 0.25f;
        }

        public void MakeAlive(){
            this.gameObject.SetActive(true);
            timeLeft = lifeTime;
        }

        void Update(){
            timeLeft -= Time.deltaTime;
            if( timeLeft <= 0f ){
                this.gameObject.SetActive(false);
            }
        }

    }

}
