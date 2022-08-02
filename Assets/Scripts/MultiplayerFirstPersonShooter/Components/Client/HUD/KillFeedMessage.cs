using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShardStudios {

    public class KillFeedMessage : MonoBehaviour
    {
        #if !SERVER
            
            private float lifeRemaining;

            [Header("Kill Feed Message Components")]
            public TextMeshProUGUI attackerText;
            public TextMeshProUGUI victimText;
            public Image weaponImage;

            [Space(10)]
            [Header("Settings")]
            public float lifeTime = 3f;

            public void Setup(Sprite weapon, string attkr, string vctm){

                lifeRemaining = lifeTime;

                attackerText.text = attkr;
                victimText.text = vctm;
                weaponImage.sprite = weapon;

            }

            void Update(){
                lifeRemaining -= Time.deltaTime;

                if( lifeRemaining <= 0f ){
                    Destroy(this.gameObject);
                }
            }

        #endif
    }

}
