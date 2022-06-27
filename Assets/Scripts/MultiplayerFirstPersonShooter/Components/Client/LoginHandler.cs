using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShardStudios {

#if !SERVER

    public class LoginHandler : MonoBehaviour
    {
        [SerializeField] InputField usernameInput;
        [SerializeField] InputField passwordInput;
        [SerializeField] Button connectButton;

        public void Connect(){
            NetworkManager.User.RequestQuickServer();
        }
    }

#endif

}
