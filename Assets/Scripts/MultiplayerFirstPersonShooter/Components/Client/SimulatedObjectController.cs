using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

    public class SimulationState {
        public Vector3 position;
        public Vector3 velocity;
        public Quaternion rotation;
        public uint tick;
    }

    public class InputState {
        public Vector2 input;
        public Quaternion rotation; // rotation will not be server authoritative.
        public byte jumping;
        public uint tick;
    }


    [RequireComponent(typeof(NetworkedEntity))]
    public class SimulatedObjectController : MonoBehaviour
    {
    

        #if !SERVER

            InputController inputController;
            MovementController movementController;
            CharacterController characterController;
            NetworkedEntity networkedEntity;
            Player owner;

            private const int CACHE_SIZE = 1024;
            private SimulationState[] simulationStates = new SimulationState[CACHE_SIZE];

            private static SimulationState activeServerState;

            private InputState[] inputStates = new InputState[CACHE_SIZE];
            private InputState activeInputState;
            private uint lastReconciledFrame = 0;

            private Vector2 moveAxis;
            private Vector2 lookAxis;
            private bool isJumping = false;
            
            // private static uint id;
            // interpolating settings.
            private static float interpolationMultiplier = 0.35f;
            private static float rotationMultiplier = 0.28f;
            private static float snapThreshold = 0.2f;

            private static float toleranceMagnitude = snapThreshold * snapThreshold;

            public GameObject ServerRep;
            void Start(){
                inputController = InputController.Instance;
                movementController = GetComponent<MovementController>();
                networkedEntity = GetComponent<NetworkedEntity>();
                owner = networkedEntity.GetOwner();
                //id = networkedEntity.id;
                if( owner?.isLocalPlayer == true )
                    ServerRep = (GameObject)Instantiate(Resources.Load("ServerRepresentation"), transform.position, transform.rotation);
            }


            void Update(){
                
                if( owner?.isLocalPlayer == false )
                    return;

                moveAxis = inputController.GetMoveAxis();
                lookAxis = inputController.GetLookAxis();

                movementController.SetLookAngle(lookAxis);

                activeInputState = new InputState {
                    input = moveAxis,
                    rotation = transform.rotation,
                    jumping = (byte)0
                };

            }


            [MessageHandler((ushort)MessageID.ReceiveSimulationState)]
            private static void ReceiveSimulationState(Message message){

                uint entId = message.GetUInt();
                if( NetworkedEntity.Entities.ContainsKey(entId) ){

                    NetworkedEntity simulatedEntity = NetworkedEntity.Entities[entId];
                    if( simulatedEntity != null ){
                        

                        SimulationState stateReceived = new SimulationState {
                            position = message.GetVector3(),
                            velocity = message.GetVector3(),
                            rotation = message.GetQuaternion(),
                            tick = message.GetUInt()
                        };

                        UpdateSimulatedPlayer(simulatedEntity, stateReceived);


                    }

                }

            }

            [MessageHandler((ushort)MessageID.ReceiveOwnSimulationState)]
            private static void ReceiveOwnSimulationState(Message message){

                uint entId = message.GetUInt();
                NetworkedEntity simulatedEntity = NetworkedEntity.Entities[entId];

                if( simulatedEntity != null ){

                    SimulationState stateReceived = new SimulationState {
                        position = message.GetVector3(),
                        velocity = message.GetVector3(),
                        rotation = message.GetQuaternion(),
                        tick = message.GetUInt()
                    };

                    if( activeServerState == null || activeServerState.tick < stateReceived.tick ){
                        activeServerState = stateReceived;
                        // temp
                        SimulatedObjectController soc = simulatedEntity.GetComponent<SimulatedObjectController>();
                        soc.ServerRep.transform.position = stateReceived.position;
                        soc.ServerRep.transform.rotation = stateReceived.rotation;
                    }
                }
            }

            private static void UpdateSimulatedPlayer(NetworkedEntity entity, SimulationState state){

                if( entity.lastSimulatedTick < state.tick ){

                    if( (state.position - entity.transform.position).magnitude > snapThreshold ){
                        entity.transform.position = Vector3.Lerp(entity.transform.position, state.position, interpolationMultiplier);
                    }
                    entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, state.rotation, rotationMultiplier);
                    entity.movementController.velocity = state.velocity;
                    entity.lastSimulatedTick = state.tick;

                    Physics.SyncTransforms();

                }

            }

            public void SendInput(){

                Message message = Message.Create(MessageSendMode.unreliable, MessageID.ClientSendInput);
                message.AddUInt(networkedEntity.id);
                message.AddVector2(activeInputState.input);
                message.AddQuaternion(activeInputState.rotation);
                message.AddByte(activeInputState.jumping);
                message.AddUInt(activeInputState.tick);

                NetworkManager.GameClient.Client.Send(message);
            }


            void FixedUpdate(){

                if( activeInputState == null || owner?.isLocalPlayer == false )
                    return;
;
                activeInputState.tick = NetworkManager.tick;
                isJumping = inputController.GetJumpingStatus();   

                if( isJumping ){
                    movementController.Jump();
                    activeInputState.jumping = (byte)1;
                }

                movementController.AddForce(new Vector3(moveAxis.x, 0f, moveAxis.y).normalized);

                if( NetworkManager.GameClient.Client != null && NetworkManager.GameClient.IsConnected() ){
                    SendInput();
                    Reconcile();
                }

                uint index = activeInputState.tick % CACHE_SIZE;

                simulationStates[index] = GetSimulationState(activeInputState);
                inputStates[index] = activeInputState;


            }

            public SimulationState GetSimulationState(InputState inputState){
                return new SimulationState {
                    position = transform.position,
                    velocity = movementController.velocity,
                    rotation = transform.rotation,
                    tick = inputState.tick
                };
            }

            void Reconcile(){

                if( activeServerState == null || activeServerState.tick <= lastReconciledFrame )
                    return;

                uint cacheIndex = activeServerState.tick % CACHE_SIZE;
                InputState cachedInputState = inputStates[cacheIndex];
                SimulationState cachedSimulationState = simulationStates[cacheIndex];


                if( cachedInputState == null || cachedSimulationState == null ){

                    transform.position = activeServerState.position;
                    transform.rotation = activeServerState.rotation;
                    movementController.velocity = activeServerState.velocity;

                    lastReconciledFrame = activeServerState.tick;

                    Physics.SyncTransforms();

                    return;

                }

                float positionDifference = (activeServerState.position - cachedSimulationState.position).magnitude;
                if(  positionDifference > snapThreshold ){

                    Debug.Log($"Out of sync, interpolating...[{positionDifference}]");

                    transform.position = Vector3.Lerp(cachedSimulationState.position, activeServerState.position, interpolationMultiplier);
                    movementController.velocity = activeServerState.velocity;
                    transform.rotation = Quaternion.Lerp(cachedSimulationState.rotation, activeServerState.rotation, rotationMultiplier);

                    Physics.SyncTransforms();

                    uint rewindTick = activeServerState.tick;

                    while( rewindTick < NetworkManager.tick ){

                        uint rewindCache = rewindTick % CACHE_SIZE;

                        InputState rewoundInputState = inputStates[rewindCache];
                        SimulationState rewoundSimulationState = simulationStates[rewindCache];

                        if( rewoundInputState == null || rewoundSimulationState == null ){
                            ++rewindTick;
                            continue;
                        }

                        movementController.AddForce(new Vector3( rewoundInputState.input.x, 0f, rewoundInputState.input.y).normalized);
                        if( rewoundInputState.jumping == (byte)1 )
                            movementController.Jump();

                        transform.rotation = rewoundInputState.rotation;
                        movementController.Move();

                        Physics.SyncTransforms();

                        SimulationState resyncedSimulationState = GetSimulationState(rewoundInputState);
                        resyncedSimulationState.tick = rewindTick;
                        simulationStates[rewindCache] = resyncedSimulationState;

                        ++rewindTick;
                    }
                }

                lastReconciledFrame = activeServerState.tick;
            }

        #endif

    }

}
