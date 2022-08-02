using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ShardStudios {

    [RequireComponent(typeof(AudioSource))]
    public class MovementSounds : MonoBehaviour
    {

        #if !SERVER

            public AudioSource audioSource;
            public AudioClip[] footstepSounds;
            void Start(){
                //audioSource = GetComponent<AudioSource>();
            }

            public AudioClip GetFootstepSound(){
                int index = Random.Range(0, footstepSounds.Length);
                return footstepSounds[index];
            }

            public void Footstep(float volume)
            {      
                audioSource.volume = volume;
                
                audioSource.PlayOneShot(GetFootstepSound());
            }
        
        #endif

    }

}
