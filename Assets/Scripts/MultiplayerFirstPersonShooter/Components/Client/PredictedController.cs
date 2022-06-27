using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class PredictedController : MonoBehaviour
    {
        InputController inputController;
        MovementController movementController;
        CharacterController characterController;

        Vector2 inputsThisFrame = Vector2.zero;

        void Start(){
            inputController = GetComponent<InputController>();
            movementController = GetComponent<MovementController>();
            characterController = GetComponent<CharacterController>();
        }

        void UpdateInputs(){
            inputsThisFrame = inputController.GetMoveAxis();
            if( inputController.GetJumpingStatus() )
                movementController.Jump();

            movementController.SetLookAngle(inputController.GetLookAxis());
        }

        void UpdateMovement(){
            movementController.AddForce(new Vector3(inputsThisFrame.x, 0f, inputsThisFrame.y).normalized);
        }

        void Update(){
            UpdateInputs();
        }

        void FixedUpdate(){
            UpdateMovement();
        }
    }

}
