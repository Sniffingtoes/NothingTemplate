using BepInEx;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Nothing.Template
{
    public class GunTemplate : MonoBehaviour
    {
        public static GameObject spherepointer;
        public static VRRig lockedPlayer;
        public static RaycastHit nray;
        public static float PointerScale = 0.1f;
        public static Color Black = Color.black;

        public static void StartVrGun(Action action, bool LockOn)
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out nray, float.MaxValue);

                if (spherepointer == null)
                {
                    spherepointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    spherepointer.transform.localScale = new Vector3(PointerScale, PointerScale, PointerScale);
                    spherepointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    Destroy(spherepointer.GetComponent<SphereCollider>());
                }

                Color currentColor = Color.red;

                if (lockedPlayer != null)
                {
                    spherepointer.transform.position = lockedPlayer.transform.position;
                    currentColor = Color.blue;
                }
                else
                {
                    spherepointer.transform.position = nray.point;
                }

                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f)
                {
                    currentColor = Color.green;
                    if (LockOn && lockedPlayer == null && nray.collider != null)
                    {
                        lockedPlayer = nray.collider.GetComponentInParent<VRRig>();
                    }
                    action();
                }
                else if (lockedPlayer != null)
                {
                    lockedPlayer = null;
                }

                if (spherepointer != null)
                {
                    spherepointer.GetComponent<Renderer>().material.color = currentColor;
                    DrawLine(GorillaTagger.Instance.rightHandTransform.position, spherepointer.transform.position, currentColor);
                }
            }
            else if (spherepointer != null)
            {
                Destroy(spherepointer);
                spherepointer = null;
                lockedPlayer = null;
            }
        }

        public static void StartPcGun(Action action, bool LockOn)
        {
            Ray ray = GorillaTagger.Instance.mainCamera.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.rightButton.isPressed)
            {
                if (Physics.Raycast(ray, out nray, float.PositiveInfinity) && spherepointer == null)
                {
                    spherepointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    spherepointer.transform.localScale = new Vector3(PointerScale, PointerScale, PointerScale);
                    spherepointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    Destroy(spherepointer.GetComponent<SphereCollider>());
                }

                Color currentColor = Color.red;

                if (lockedPlayer != null)
                {
                    spherepointer.transform.position = lockedPlayer.transform.position;
                    currentColor = Color.blue;
                }
                else
                {
                    spherepointer.transform.position = nray.point;
                }

                if (Mouse.current.leftButton.isPressed)
                {
                    currentColor = Color.green;
                    if (LockOn && lockedPlayer == null && nray.collider != null)
                    {
                        lockedPlayer = nray.collider.GetComponentInParent<VRRig>();
                    }
                    action();
                }
                else if (lockedPlayer != null)
                {
                    lockedPlayer = null;
                }

                if (spherepointer != null)
                {
                    spherepointer.GetComponent<Renderer>().material.color = currentColor;
                    DrawLine(GorillaTagger.Instance.rightHandTransform.position, spherepointer.transform.position, currentColor);
                }
            }
            else if (spherepointer != null)
            {
                Destroy(spherepointer);
                spherepointer = null;
                lockedPlayer = null;
            }
        }

        private static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            GameObject lineObj = new GameObject("GunLine");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("GUI/Text Shader"));
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = 0.015f;
            lr.endWidth = 0.015f;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            Destroy(lineObj, Time.deltaTime);
        }

        public static void StartBothGuns(Action action, bool locko)
        {
            if (XRSettings.isDeviceActive) StartVrGun(action, locko);
            else StartPcGun(action, locko);
        }

        public static void GunTest()
        {
            StartBothGuns(() => { }, true);
        }
    }
}