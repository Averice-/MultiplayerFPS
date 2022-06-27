using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShardStudios {

    public class InputController : MonoBehaviour
    {   

        private DefaultActions defaultActions;

        private bool isJumpingThisFrame = false;

        void Start(){
            defaultActions = new DefaultActions();
            defaultActions.PlayerMovement.Enable();

            defaultActions.PlayerMovement.Jump.performed += JumpInputPressed;
        }

        public void JumpInputPressed(InputAction.CallbackContext context){
            isJumpingThisFrame = true;
        }

        public bool GetJumpingStatus(){
            if( isJumpingThisFrame ){
                isJumpingThisFrame = false;
                return true;
            }
            return false;
        }

        public Vector2 GetMoveAxis(){
            return defaultActions.PlayerMovement.Move.ReadValue<Vector2>();
        }

        public Vector2 GetLookAxis(){
            return defaultActions.PlayerMovement.Look.ReadValue<Vector2>();
        }
    }

}
