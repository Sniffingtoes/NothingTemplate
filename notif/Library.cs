using BepInEx;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Nothing.Settings;

namespace Nothing.Notifications
{
    [BepInPlugin("org.gorillatag.megamind.notifications", "NotificationLibrary", "1.0.0")]
    public class NotifiLib : BaseUnityPlugin
    {
        public static bool IsInCategory = false;
        public static bool IsEnabled = true;
        private bool HasInit;
        private static float toggleDelay = 0f;

        public static List<NotifData> activeNotifs = new List<NotifData>();
        public static GameObject HUDObj;
        public static GameObject HUDObj2;
        public static GameObject MainCamera;

        private void Awake() => Logger.LogInfo("Notifications Loaded");

        private void Init()
        {
            MainCamera = GameObject.Find("Main Camera");
            HUDObj2 = new GameObject("NOTIF_PARENT");
            HUDObj = new GameObject("NOTIF_CANVAS");
            Canvas canvas = HUDObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = MainCamera.GetComponent<Camera>();
            HUDObj.AddComponent<CanvasScaler>();
            HUDObj.transform.SetParent(HUDObj2.transform, false);
            HUDObj.transform.localPosition = new Vector3(-0.25f, -0.50f, 1.1f);
            HUDObj.transform.localRotation = Quaternion.identity;
            HUDObj.transform.localScale = Vector3.one;
        }

        private void FixedUpdate()
        {
            if (!HasInit && GameObject.Find("Main Camera") != null)
            {
                Init();
                HasInit = true;
            }

            if (HUDObj2 != null && MainCamera != null)
            {
                HUDObj2.transform.position = MainCamera.transform.position;
                HUDObj2.transform.rotation = MainCamera.transform.rotation;
            }

            for (int i = 0; i < activeNotifs.Count; i++)
            {
                NotifData data = activeNotifs[i];
                data.timer -= Time.deltaTime;
                data.accentBar.anchorMax = new Vector2(Mathf.Clamp01(data.timer / 4f), 0);

                if (data.timer <= 0)
                {
                    Destroy(data.container);
                    activeNotifs.RemoveAt(i);
                }
                else
                {
                    float targetY = (activeNotifs.Count - 1 - i) * 0.13f;
                    data.container.transform.localPosition = Vector3.Lerp(data.container.transform.localPosition, new Vector3(0f, targetY, 0f), Time.deltaTime * 12f);
                }
            }
        }

        public static void SendNotification(string text)
        {
            if (Nothing.Settings.disableNotifications || IsInCategory || !IsEnabled || HUDObj == null) return;
            if (activeNotifs.Count > 4) { Destroy(activeNotifs[0].container); activeNotifs.RemoveAt(0); }

            GameObject container = new GameObject("Notif_Container");
            container.transform.SetParent(HUDObj.transform, false);
            container.transform.localScale = Vector3.zero;

            RectTransform contRect = container.AddComponent<RectTransform>();
            contRect.sizeDelta = new Vector2(0.42f, 0.12f);
            Image bgImage = container.AddComponent<Image>();
            bgImage.color = new Color(0.04f, 0.04f, 0.04f, 0.94f);

            GameObject bar = new GameObject("Bar");
            bar.transform.SetParent(container.transform, false);
            bar.transform.localPosition = new Vector3(0, 0, -0.001f);
            Image barImg = bar.AddComponent<Image>();
            barImg.color = new Color(0.2f, 0.55f, 1f, 1f);
            RectTransform barRect = barImg.rectTransform;
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = new Vector2(1, 0);
            barRect.sizeDelta = new Vector2(0, 0.006f);
            barRect.pivot = Vector2.zero;
            barRect.anchoredPosition = Vector2.zero;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(container.transform, false);
            titleObj.transform.localPosition = new Vector3(-0.05f, 0.035f, -0.002f);
            titleObj.transform.localScale = Vector3.one * 0.001f;
            Text titleT = titleObj.AddComponent<Text>();
            titleT.text = "Notification";
            titleT.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleT.fontStyle = FontStyle.Bold;
            titleT.fontSize = 36;
            titleT.color = Color.white;
            titleT.alignment = TextAnchor.MiddleLeft;
            titleT.rectTransform.sizeDelta = new Vector2(300, 50);

            GameObject bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(container.transform, false);
            bodyObj.transform.localPosition = new Vector3(-0.02f, -0.028f, -0.002f);
            bodyObj.transform.localScale = Vector3.one * 0.00085f;
            Text bodyT = bodyObj.AddComponent<Text>();

            bodyT.text = text;

            bodyT.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            bodyT.fontSize = 30;
            bodyT.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            bodyT.alignment = TextAnchor.UpperLeft;
            bodyT.supportRichText = true;
            bodyT.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyT.rectTransform.sizeDelta = new Vector2(400, 100);

            container.transform.localScale = Vector3.one;
            activeNotifs.Add(new NotifData { container = container, accentBar = barRect, timer = 4f });
        }

        public class NotifData { public GameObject container; public RectTransform accentBar; public float timer; }
    }
}