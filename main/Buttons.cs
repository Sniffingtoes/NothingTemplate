using Nothing.Classes;
using Nothing.Mods;
using NothingMenu.Mods;
using System.Linq;
using UnityEngine;
using static Nothing.Template.GunTemplate;
using static Nothing.Template.Main;
using static Nothing.Settings;

namespace Nothing.Template
{
    public class Buttons
    {
        public static ButtonInfo[][] buttons;

        static Buttons()
        {
            Init();
        }

        public static void Init()
        {
            if (buttons != null) return;

            buttons = new ButtonInfo[][]
            {
                new ButtonInfo[] { // 0: Main Menu
                    new ButtonInfo { buttonText = "Settings", method = () => currentCategory = 1, isTogglable = false },
                    new ButtonInfo { buttonText = "Movement", method = () => currentCategory = 2, isTogglable = false },
                },

                new ButtonInfo[] { // 1: Settings
                    new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                    new ButtonInfo { buttonText = "Check Master", method = () => Settingss.CheckMaster(), isTogglable = false },
                    new ButtonInfo { buttonText = "Disconnect [RT]", method = () => Settingss.RTDisconnect(), isTogglable = false },
                    new ButtonInfo { buttonText = "Equip Gun", method = () => GunTemplate.GunTest(), isTogglable = true },
                },

                new ButtonInfo[] { // 2: Movement
                    new ButtonInfo { buttonText = "Return to Main", method = () => currentCategory = 0, isTogglable = false },
                    new ButtonInfo { buttonText = "Platforms", method = () => Movement.Platforms(), isTogglable = true },
                    new ButtonInfo { buttonText = "Fly [B]", method = () => Movement.Fly(), isTogglable = true },
                    new ButtonInfo { buttonText = "TP Gun", method = () => Movement.TeleportGun(), isTogglable = true },
                    new ButtonInfo { buttonText = "Speed Boost", method = () => Movement.SpeedBoost(), isTogglable = true },
                },
            };
        }
    }
}