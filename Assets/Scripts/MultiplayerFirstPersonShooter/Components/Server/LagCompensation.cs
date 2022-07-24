using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class LagCompensation
    {

        #if SERVER

            static Dictionary<uint, SimulationState> originalStates = new Dictionary<uint, SimulationState>();
            static bool inCompensation = false;
            static int CACHE_SIZE = 132; // Must be identical to SimulatedObjectHandler;

            // Move the players back in time.
            public static void BeginCompensation(uint tick, uint frameDiff = 0){

                if( inCompensation ){
                    Debug.Log("Server already compensating.. returning.."); // Should never get to this.
                    return;
                }

                uint frame = frameDiff > 0 ? frameDiff : 0; 
                uint rewindTick = tick - 1 - frame;
                inCompensation = true;

                foreach(KeyValuePair<uint, SimulationState[]> simState in SimulatedObjectHandler.simulationStates){

                    
                    SimulationState newState = simState.Value[rewindTick % CACHE_SIZE]; // Previous state.
                    if( newState != null ){ // Shouldn't be, but I'm insane.

                        NetworkedEntity entity = NetworkedEntity.GetEntityById(simState.Key);
                        MovementController movementController = entity.GetComponent<MovementController>();

                        SimulationState originalState = new SimulationState {
                            position = entity.transform.position,
                            velocity = movementController.velocity,
                            rotation = movementController.GetLookAngle(),
                            tick = tick
                        };
                        originalStates.Add(simState.Key, originalState);

                        if( entity != null){
                            //entity.GetComponent<CharacterController>().enabled = false;
                            entity.transform.position = newState.position;
                            movementController.SetLookAngleFromVector2(newState.rotation);
                        }
                        

                    }
                }
            }

            // Move everyone back to their OG.
            public static void EndCompensation(){
                
                if( !inCompensation )
                    return;
                    
                foreach(KeyValuePair<uint, SimulationState> originalState in originalStates){

                    NetworkedEntity entity = NetworkedEntity.GetEntityById(originalState.Key);
                    if( entity != null ){
                        MovementController movementController = entity.GetComponent<MovementController>();

                        entity.transform.position = originalState.Value.position;
                        movementController.SetLookAngleFromVector2(originalState.Value.rotation);
                        //entity.GetComponent<CharacterController>().enabled = true;
                    }

                }

                originalStates.Clear();
                inCompensation = false;

            }

        #endif

    }

}
