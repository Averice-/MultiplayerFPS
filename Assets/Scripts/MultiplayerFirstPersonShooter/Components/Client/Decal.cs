using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ShardStudios {

    public class Decal : MonoBehaviour
    {
        // Now using URP Decal Projector;
        [SerializeField] DecalProjector decalProjector;

        [SerializeField] float lifeTime = 5f;
        [SerializeField] float offsetMultiplier = 0.25f;
        float timeLeft = 0f;

        public void SetOffset(Vector2 offsetVector){
            decalProjector.uvBias = offsetVector * offsetMultiplier;
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
