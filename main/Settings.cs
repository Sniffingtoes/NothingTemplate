using Nothing.Classes;
using UnityEngine;
using Nothing.Notifications;
using System.Collections.Generic;

namespace Nothing
{
    public class Settings
    {

        public static void ToggleNotifications()
        {
            // didnt want to make it <3
        }

        public static bool fpsCounter = true;
        public static bool disconnectButton = true;
        public static bool rightHanded;
        public static bool disableNotifications;
        public static KeyCode keyboardButton = KeyCode.Q;
        public static Vector3 menuSize = new Vector3(0.06f, 0.63f, 0.8f);
        public static int buttonsPerPage = 5;
        public static float gradientSpeed = 0.5f;
    }
}