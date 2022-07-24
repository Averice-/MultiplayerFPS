using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class AnimationHandler : MonoBehaviour
    {

        public Animator animator;

        string[] weaponAnimBools = new string[] {
            "EquipAssaultRifle",
            "EquipHandgun"
        };

        public void EquipAnim(string animName){

            for( int i = 0; i < weaponAnimBools.Length; i++ ){
                animator.SetBool(weaponAnimBools[i], animName == weaponAnimBools[i]);
                // if( weaponAnimBools[i] == animName ){
                //     animator.SetBool(animName, true);
                //     continue;
                // }

                // animator.SetBool(weaponAnimBools[i], false);
            }
            
        }
    }

}
