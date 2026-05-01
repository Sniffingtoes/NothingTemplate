using BepInEx;
using Nothing.Classes;
using Nothing.Notifications;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;
using static Nothing.Template.Buttons;
using static Nothing.Settings;
using BoingKit;

namespace Nothing.Template
{
    [HarmonyPatch(typeof(GTPlayer), "LateUpdate")]
    public class Main : MonoBehaviour
    {
        public static bool hasLoadedSave = false;

        public static void Prefix()
        {
            try
            {
                bool toOpen = (!rightHanded && ControllerInputPoller.instance.leftControllerSecondaryButton) || (rightHanded && ControllerInputPoller.instance.rightControllerSecondaryButton);
                bool keyboardOpen = UnityInput.Current.GetKey(keyboardButton);

                if (toOpen || keyboardOpen)
                {
                    if (menu == null)
                    {
                        CreateMenu();
                        RecenterMenu(rightHanded, keyboardOpen);
                        if (reference == null) CreateReference(rightHanded);
                    }
                    else
                    {
                        RecenterMenu(rightHanded, keyboardOpen);
                    }
                }
                else
                {
                    if (menu != null)
                    {
                        GameObject.Find("Shoulder Camera")?.transform.Find("CM vcam1")?.gameObject.SetActive(true);
                        Rigidbody comp = menu.AddComponent<Rigidbody>();
                        comp.linearVelocity = (rightHanded ? GTPlayer.Instance.LeftHand.velocityTracker : GTPlayer.Instance.RightHand.velocityTracker).GetAverageVelocity(true, 0);
                        Destroy(menu);
                        menu = null;
                        Destroy(reference);
                        reference = null;
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogError("Menu Error: " + exc.Message);
            }

            try
            {
                if (fpsObject != null)
                    fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();

                foreach (ButtonInfo button in buttons.SelectMany(list => list).Where(button => button.enabled && button.method != null))
                {
                    try
                    {
                        button.method.Invoke();
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError($"{PluginInfo.Name} // Error with mod {button.buttonText} at {exc.StackTrace}: {exc.Message}");
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.LogError($"{PluginInfo.Name} // Error executing mods: {exc.Message}");
            }
        }


        public static GameObject BG;
        public static List<GameObject> allButtons = new List<GameObject>();
        public static List<TextMeshPro> allTexts = new List<TextMeshPro>();

		public static Color menuBackgroundColor = Color.black;
		public static Color buttonColor = Color.grey;
		public static Color textColor = Color.powderBlue;
		public static Color enabledColor = Color.green;

		public static void CreateMenu()
		{
			allButtons.Clear();
			allTexts.Clear();

			menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
			UnityEngine.Object.Destroy(menu.GetComponent<Rigidbody>());
			UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
			UnityEngine.Object.Destroy(menu.GetComponent<Renderer>());
			menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);

			GameObject roundedBG = new GameObject("RoundedBackground");
			roundedBG.transform.parent = menu.transform;
			roundedBG.transform.localRotation = Quaternion.identity;
			roundedBG.transform.localPosition = new Vector3(0.5f, 0f, 0f);

			float bgWidth = 0.19f;
			float bgHeight = 0.3f;
			float bgThick = 0.01f;
			float bgRad = 0.015f;
			Shader shader = Shader.Find("GorillaTag/UberShader");

			void AddPart(PrimitiveType type, Vector3 pos, Vector3 scale, Transform parent, Color color, bool isCyl = false)
			{
				GameObject part = GameObject.CreatePrimitive(type);
				UnityEngine.Object.Destroy(part.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy(part.GetComponent<Collider>());
				part.transform.parent = parent;
				part.transform.localPosition = pos;
				part.transform.localScale = scale;
				if (isCyl) part.transform.localRotation = Quaternion.Euler(0, 0, 90);
				var ren = part.GetComponent<Renderer>();
				if (ren != null)
				{
					ren.material.shader = shader;
					ren.material.color = color;
				}
			}

			float bgX = (bgWidth / 2) - bgRad;
			float bgZ = (bgHeight / 2) - bgRad;
			Vector3 bgCylScale = new Vector3(bgRad * 2, bgThick / 2.01f, bgRad * 2);

			AddPart(PrimitiveType.Cylinder, new Vector3(0, bgX, bgZ), bgCylScale, roundedBG.transform, menuBackgroundColor, true);
			AddPart(PrimitiveType.Cylinder, new Vector3(0, -bgX, bgZ), bgCylScale, roundedBG.transform, menuBackgroundColor, true);
			AddPart(PrimitiveType.Cylinder, new Vector3(0, bgX, -bgZ), bgCylScale, roundedBG.transform, menuBackgroundColor, true);
			AddPart(PrimitiveType.Cylinder, new Vector3(0, -bgX, -bgZ), bgCylScale, roundedBG.transform, menuBackgroundColor, true);
			AddPart(PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(bgThick, bgWidth - (bgRad * 2), bgHeight), roundedBG.transform, menuBackgroundColor);
			AddPart(PrimitiveType.Cube, new Vector3(0, bgX, 0), new Vector3(bgThick, bgRad * 2, bgHeight - (bgRad * 2)), roundedBG.transform, menuBackgroundColor);
			AddPart(PrimitiveType.Cube, new Vector3(0, -bgX, 0), new Vector3(bgThick, bgRad * 2, bgHeight - (bgRad * 2)), roundedBG.transform, menuBackgroundColor);

			if (menuBackground != null) UnityEngine.Object.Destroy(menuBackground);

			canvasObject = new GameObject("MenuCanvas");
			canvasObject.transform.parent = menu.transform;
			Canvas canvas = canvasObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvasObject.layer = 0;

			GameObject motd = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText");
			TMP_FontAsset motdFont = (motd != null) ? motd.GetComponent<TextMeshPro>().font : null;

			void CreateMenuText(string textContent, Vector3 localPos, Vector2 sizeDelta, float fontSize = 2.5f)
			{
				GameObject txtObj = new GameObject("Text");
				txtObj.transform.SetParent(canvasObject.transform, false);
				txtObj.layer = 0;
				TextMeshPro tmp = txtObj.AddComponent<TextMeshPro>();
				tmp.text = textContent;
				tmp.fontSize = fontSize;
				tmp.color = textColor;
				tmp.alignment = TextAlignmentOptions.Center;
				if (motdFont != null) tmp.font = motdFont;
				RectTransform rect = tmp.GetComponent<RectTransform>();
				rect.sizeDelta = sizeDelta;
				rect.localPosition = new Vector3((localPos.x == 0f) ? 0.06f : localPos.x, localPos.y, localPos.z);
				rect.localRotation = Quaternion.Euler(0f, -90f, -90f);
				rect.localScale = new Vector3(0.08f, 0.08f, 0.08f);
				allTexts.Add(tmp);
			}

			CreateMenuText(PluginInfo.Name + PluginInfo.Version, new Vector3(0.056f, 0f, 0.13f), new Vector2(10f, 4f), 2f);
			if (fpsCounter) CreateMenuText("FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString(), new Vector3(0.056f, 0f, 0.11f), new Vector2(8f, 2f), 1.8f);

			void CreateNavBtn(string name, Vector3 pos, float w, float h, string label, string text, Vector3 textPos, float fontSize)
			{
				GameObject btn = new GameObject(name);
				btn.transform.parent = menu.transform;
				btn.transform.localPosition = pos;
				float r = 0.01f;
				float t = 0.005f;
				float x = (w / 2) - r;
				float z = (h / 2) - r;
				Vector3 cS = new Vector3(r * 2, t / 2.01f, r * 2);
				AddPart(PrimitiveType.Cylinder, new Vector3(0, x, z), cS, btn.transform, buttonColor, true);
				AddPart(PrimitiveType.Cylinder, new Vector3(0, -x, z), cS, btn.transform, buttonColor, true);
				AddPart(PrimitiveType.Cylinder, new Vector3(0, x, -z), cS, btn.transform, buttonColor, true);
				AddPart(PrimitiveType.Cylinder, new Vector3(0, -x, -z), cS, btn.transform, buttonColor, true);
				AddPart(PrimitiveType.Cube, Vector3.zero, new Vector3(t, w - (r * 2), h), btn.transform, buttonColor);
				AddPart(PrimitiveType.Cube, new Vector3(0, x, 0), new Vector3(t, r * 2, h - (r * 2)), btn.transform, buttonColor);
				AddPart(PrimitiveType.Cube, new Vector3(0, -x, 0), new Vector3(t, r * 2, h - (r * 2)), btn.transform, buttonColor);
				if (!UnityInput.Current.GetKey(keyboardButton)) btn.layer = 2;
				BoxCollider trig = btn.AddComponent<BoxCollider>();
				trig.isTrigger = true;
				trig.size = new Vector3(t * 2f, w, h);
				btn.AddComponent<Classes.Button>().relatedText = label;
				allButtons.Add(btn);
				CreateMenuText(text, textPos, new Vector2(10f, 3f), fontSize);
			}

			if (disconnectButton) CreateNavBtn("DisconnectButton", new Vector3(0.5f, 0f, 0.48f), 0.19f, 0.035f, "Disconnect", "Disconnect", new Vector3(0f, 0f, 0.18f), 2.5f);
			CreateNavBtn("PrevButton", new Vector3(0.56f, 0.21f, -0.32f), 0.05f, 0.04f, "PreviousPage", "<", new Vector3(0f, 0.065f, -0.12f), 3f);
			CreateNavBtn("NextButton", new Vector3(0.56f, -0.21f, -0.32f), 0.05f, 0.04f, "NextPage", ">", new Vector3(0f, -0.065f, -0.12f), 3f);
			CreateNavBtn("HomeButton", new Vector3(0.56f, 0f, -0.32f), 0.06f, 0.04f, "HomeButton", "Home", new Vector3(0f, 0f, -0.12f), 2f);

			ButtonInfo[] activeButtons = buttons[currentCategory].Skip(pageNumber * buttonsPerPage).Take(buttonsPerPage).ToArray();
			for (int i = 0; i < activeButtons.Length; i++) CreateButton(i * 0.1f, activeButtons[i], motdFont);
		}

		public static void CreateButton(float offset, ButtonInfo method, TMP_FontAsset motdFont)
		{
			GameObject buttonContainer = new GameObject("Buttons");
			buttonContainer.transform.parent = menu.transform;
			buttonContainer.transform.localPosition = new Vector3(0.56f, 0f, 0.2f - offset);
			float w = 0.17f, h = 0.035f, t = 0.005f, r = 0.01f;
			float x = (w / 2) - r, z = (h / 2) - r;
			Shader s = Shader.Find("GorillaTag/UberShader");

			void AddBtnPart(PrimitiveType type, Vector3 pos, Vector3 scale, Quaternion rot)
			{
				GameObject part = GameObject.CreatePrimitive(type);
				UnityEngine.Object.Destroy(part.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy(part.GetComponent<Collider>());
				part.transform.parent = buttonContainer.transform;
				part.transform.localPosition = pos;
				part.transform.localScale = scale;
				part.transform.localRotation = rot;
				var ren = part.GetComponent<Renderer>();
				if (ren != null)
				{
					ren.material.shader = s;
					ren.material.color = buttonColor;
				}
			}

			Vector3 cS = new Vector3(r * 2, t / 2.01f, r * 2);
			Quaternion cR = Quaternion.Euler(0, 0, 90);
			AddBtnPart(PrimitiveType.Cylinder, new Vector3(0, x, z), cS, cR);
			AddBtnPart(PrimitiveType.Cylinder, new Vector3(0, -x, z), cS, cR);
			AddBtnPart(PrimitiveType.Cylinder, new Vector3(0, x, -z), cS, cR);
			AddBtnPart(PrimitiveType.Cylinder, new Vector3(0, -x, -z), cS, cR);
			AddBtnPart(PrimitiveType.Cube, Vector3.zero, new Vector3(t, w - (r * 2), h), Quaternion.identity);
			AddBtnPart(PrimitiveType.Cube, new Vector3(0, x, 0), new Vector3(t, r * 2, h - (r * 2)), Quaternion.identity);
			AddBtnPart(PrimitiveType.Cube, new Vector3(0, -x, 0), new Vector3(t, r * 2, h - (r * 2)), Quaternion.identity);

			if (!UnityInput.Current.GetKey(keyboardButton)) buttonContainer.layer = 2;
			BoxCollider trig = buttonContainer.AddComponent<BoxCollider>();
			trig.isTrigger = true;
			trig.size = new Vector3(t * 4f, w, h);
			buttonContainer.AddComponent<Classes.Button>().relatedText = method.buttonText;
			allButtons.Add(buttonContainer);

			GameObject txtObj = new GameObject("ButtonText");
			txtObj.transform.SetParent(buttonContainer.transform, false);
			TextMeshPro tmp = txtObj.AddComponent<TextMeshPro>();
			if (motdFont != null) tmp.font = motdFont;
			tmp.text = method.overlapText ?? method.buttonText;
			tmp.fontSize = 3f;
			tmp.alignment = TextAlignmentOptions.Center;
			tmp.color = method.enabled ? enabledColor : textColor;

			RectTransform rect = tmp.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(w * 20f, h * 20f);
			rect.localPosition = new Vector3(0.006f, 0f, 0f);
			rect.localRotation = Quaternion.Euler(180f, 90f, 90f);
			rect.localScale = new Vector3(0.05f, 0.05f, 0.05f);
		}

		public static void RecreateMenu()
        {
            if (menu != null)
            {
                Destroy(menu);
                menu = null;
                CreateMenu();
                RecenterMenu(rightHanded, UnityInput.Current.GetKey(keyboardButton));
            }
        }

        public static void RecenterMenu(bool isRightHanded, bool isKeyboardCondition)
        {
            if (!isKeyboardCondition)
            {
                if (!isRightHanded)
                {
                    menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                }
                else
                {
                    menu.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 rotation = GorillaTagger.Instance.rightHandTransform.rotation.eulerAngles;
                    rotation += new Vector3(0f, 0f, 180f);
                    menu.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            else
            {
                try { TPC = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>(); } catch { }
                GameObject.Find("Shoulder Camera")?.transform.Find("CM vcam1")?.gameObject.SetActive(false);

                if (TPC != null)
                {
                    TPC.transform.position = new Vector3(-999f, -999f, -999f);
                    TPC.transform.rotation = Quaternion.identity;

                    GameObject bgCamObj = new GameObject("MenuBackgroundCamera");
                    Camera bgCam = bgCamObj.AddComponent<Camera>();
                    bgCamObj.transform.position = new Vector3(-68.5885f, 12.0878f, -83.9583f);
                    bgCamObj.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
                    bgCam.fieldOfView = 75f;
                    bgCam.depth = TPC.depth - 1;
                    bgCam.farClipPlane = 2000f;
                    Destroy(bgCamObj, 0.05f);

                    menu.transform.parent = TPC.transform;
                    menu.transform.position = TPC.transform.position + (TPC.transform.forward * 0.5f) + (TPC.transform.up * -0.02f);
                    menu.transform.rotation = TPC.transform.rotation * Quaternion.Euler(-90f, 90f, 0f);

                    if (reference != null && Mouse.current.leftButton.isPressed)
                    {
                        Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                        if (Physics.Raycast(ray, out RaycastHit hit, 100))
                        {
                            hit.transform.gameObject.GetComponent<Classes.Button>()?.OnTriggerEnter(buttonCollider);
                        }
                    }
                }
            }
        }

        public static void CreateReference(bool isRightHanded)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reference.transform.parent = isRightHanded ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;

            reference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            buttonCollider = reference.GetComponent<SphereCollider>();

            if (reference.GetComponent<ColorChanger>() == null)
            {
                reference.AddComponent<ColorChanger>();
            }
        }

        public static void Toggle(string buttonText)
        {
            if (buttonText == "HomeButton")
            {
                currentCategory = 0;
                pageNumber = 0;
                RecreateMenu();
                return;
            }

            if (buttonText == "Disconnect")
            {
                PhotonNetwork.Disconnect();
                return;
            }

            int lastPage = ((buttons[currentCategory].Length + buttonsPerPage - 1) / buttonsPerPage) - 1;
            if (buttonText == "PreviousPage")
            {
                pageNumber = (pageNumber <= 0) ? lastPage : pageNumber - 1;
                RecreateMenu();
                return;
            }
            else if (buttonText == "NextPage")
            {
                pageNumber = (pageNumber >= lastPage) ? 0 : pageNumber + 1;
                RecreateMenu();
                return;
            }

            ButtonInfo target = GetIndex(buttonText);
            if (target != null)
            {
                if (ControllerInputPoller.instance.leftGrab)
                {

                }
                else
                {
                    if (target.isTogglable)
                    {
                        target.enabled = !target.enabled;

                        string status = target.enabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>";
                        NotifiLib.SendNotification($"<color=grey>[</color>{status}<color=grey>]</color> {target.buttonText}");

                        if (target.enabled) target.enableMethod?.Invoke();
                        else target.disableMethod?.Invoke();
                    }
                    else
                    {
                        NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLED</color><color=grey>]</color> " + target.buttonText);
                        target.method?.Invoke();
                    }
                }
            }
            else
            {
                Debug.LogError(buttonText + " does not exist in the button list.");
            }

            RecreateMenu();
        }

        private static readonly Dictionary<string, (int Category, int Index)> cacheGetIndex = new Dictionary<string, (int Category, int Index)>(); // Looping through 800 elements is not a light task :/
        public static ButtonInfo GetIndex(string buttonText)
        {
            if (buttonText == null)
                return null;

            if (cacheGetIndex.ContainsKey(buttonText))
            {
                var CacheData = cacheGetIndex[buttonText];
                try
                {
                    if (buttons[CacheData.Category][CacheData.Index].buttonText == buttonText)
                        return buttons[CacheData.Category][CacheData.Index];
                }
                catch { cacheGetIndex.Remove(buttonText); }
            }

            int categoryIndex = 0;
            foreach (ButtonInfo[] buttons in buttons)
            {
                int buttonIndex = 0;
                foreach (ButtonInfo button in buttons)
                {
                    if (button.buttonText == buttonText)
                    {
                        try
                        {
                            cacheGetIndex.Add(buttonText, (categoryIndex, buttonIndex));
                        }
                        catch
                        {
                            if (cacheGetIndex.ContainsKey(buttonText))
                                cacheGetIndex.Remove(buttonText);
                        }

                        return button;
                    }
                    buttonIndex++;
                }
                categoryIndex++;
            }

            return null;
        }

        public static Vector3 RandomVector3(float range = 1f) =>
            new Vector3(UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range),
                        UnityEngine.Random.Range(-range, range));

        public static Quaternion RandomQuaternion(float range = 360f) =>
            Quaternion.Euler(UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range),
                        UnityEngine.Random.Range(0f, range));

        public static Color RandomColor(byte range = 255, byte alpha = 255) =>
            new Color32((byte)UnityEngine.Random.Range(0, range),
                        (byte)UnityEngine.Random.Range(0, range),
                        (byte)UnityEngine.Random.Range(0, range),
                        alpha);

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueLeftHand()
        {
            Quaternion rot = GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handRotOffset;
            return (GorillaTagger.Instance.leftHandTransform.position + GorillaTagger.Instance.leftHandTransform.rotation * GTPlayer.Instance.LeftHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static (Vector3 position, Quaternion rotation, Vector3 up, Vector3 forward, Vector3 right) TrueRightHand()
        {
            Quaternion rot = GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handRotOffset;
            return (GorillaTagger.Instance.rightHandTransform.position + GorillaTagger.Instance.rightHandTransform.rotation * GTPlayer.Instance.RightHand.handOffset, rot, rot * Vector3.up, rot * Vector3.forward, rot * Vector3.right);
        }

        public static void WorldScale(GameObject obj, Vector3 targetWorldScale)
        {
            Vector3 parentScale = obj.transform.parent.lossyScale;
            obj.transform.localScale = new Vector3(
                targetWorldScale.x / parentScale.x,
                targetWorldScale.y / parentScale.y,
                targetWorldScale.z / parentScale.z
            );
        }

        public static void FixStickyColliders(GameObject platform)
        {
            Vector3[] localPositions = new Vector3[]
            {
                new Vector3(0, 1f, 0),
                new Vector3(0, -1f, 0),
                new Vector3(1f, 0, 0),
                new Vector3(-1f, 0, 0),
                new Vector3(0, 0, 1f),
                new Vector3(0, 0, -1f)
            };
            Quaternion[] localRotations = new Quaternion[]
            {
                Quaternion.Euler(90, 0, 0),
                Quaternion.Euler(-90, 0, 0),
                Quaternion.Euler(0, -90, 0),
                Quaternion.Euler(0, 90, 0),
                Quaternion.identity,
                Quaternion.Euler(0, 180, 0)
            };
            for (int i = 0; i < localPositions.Length; i++)
            {
                GameObject side = GameObject.CreatePrimitive(PrimitiveType.Cube);
                try
                {
                    if (platform.GetComponent<GorillaSurfaceOverride>() != null)
                    {
                        side.AddComponent<GorillaSurfaceOverride>().overrideIndex = platform.GetComponent<GorillaSurfaceOverride>().overrideIndex;
                    }
                }
                catch { }
                float size = 0.025f;
                side.transform.SetParent(platform.transform);
                side.transform.position = localPositions[i] * (size / 2);
                side.transform.rotation = localRotations[i];
                WorldScale(side, new Vector3(size, size, 0.01f));
                side.GetComponent<Renderer>().enabled = false;
            }
        }

        private static int? noInvisLayerMask;
        public static int NoInvisLayerMask()
        {
            noInvisLayerMask ??= ~(
                1 << LayerMask.NameToLayer("TransparentFX") |
                1 << LayerMask.NameToLayer("Ignore Raycast") |
                1 << LayerMask.NameToLayer("Zone") |
                1 << LayerMask.NameToLayer("Gorilla Trigger") |
                1 << LayerMask.NameToLayer("Gorilla Boundary") |
                1 << LayerMask.NameToLayer("GorillaCosmetics") |
                1 << LayerMask.NameToLayer("GorillaParticle"));

            return noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
        }

        
        public static GameObject menu;
        public static GameObject menuBackground;
        public static GameObject reference;
        public static GameObject canvasObject;

        public static SphereCollider buttonCollider;
        public static Camera TPC;
        public static Text fpsObject;

        private static GameObject GunPointer;
        private static LineRenderer GunLine;

        public static int pageNumber = 0;
        public static int _currentCategory;
        public static int currentCategory
        {
            get => _currentCategory;
            set
            {
                _currentCategory = value;
                pageNumber = 0;
            }
        }
    }
}