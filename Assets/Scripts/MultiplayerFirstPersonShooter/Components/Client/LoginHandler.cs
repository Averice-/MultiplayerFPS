using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShardStudios {



    public class LoginHandler : MonoBehaviour
    {
        #if !SERVER
            [SerializeField] InputField usernameInput;
            [SerializeField] InputField passwordInput;
            [SerializeField] Button connectButton;

            public void Connect(){
                // Temporary
                NetworkManager.User.RequestQuickServer();
            }
        #endif
    }



}
