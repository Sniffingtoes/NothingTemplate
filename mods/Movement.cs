using BepInEx;
using Custom.Inputs;
using g3;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using Nothing.Template;
using Oculus.Platform;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using UnityEngine.XR;
using Valve.VR;
using static Nothing.Template.GunTemplate;

namespace Nothing.Mods
{
    public class Movement
    {
        public static GameObject platl;
        public static GameObject platr;

        public static void Platforms()
        {
            Color platformColor = Color.black;

            if (Get.leftGrab)
            {
                if (platl == null)
                {
                    platl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platl.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platl.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    platl.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                    platl.GetComponent<Renderer>().material.color = platformColor;

                    var surface = platl.AddComponent<GorillaSurfaceOverride>();
                    surface.overrideIndex = 0;
                }
            }
            else if (platl != null)
            {
                UnityEngine.Object.Destroy(platl);
                platl = null;
            }

            if (Get.rightGrab)
            {
                if (platr == null)
                {
                    platr = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platr.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platr.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    platr.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                    platr.GetComponent<Renderer>().material.color = platformColor;

                    var surface = platr.AddComponent<GorillaSurfaceOverride>();
                    surface.overrideIndex = 0;
                }
            }
            else if (platr != null)
            {
                UnityEngine.Object.Destroy(platr);
                platr = null;
            }
        }

        public static void SpeedBoost()
        {
            GorillaLocomotion.GTPlayer.Instance.maxJumpSpeed = 7.5f;
            GorillaLocomotion.GTPlayer.Instance.jumpMultiplier = 7.5f;
        }

        public static float FlySpeed = 15f;

        public static void Fly()
        {
            if (Get.bButton)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * FlySpeed);

                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public static bool canTeleport = true;

        public static void TeleportGun()
        {
            if (!Get.rIndexFloat && !Mouse.current.leftButton.isPressed)
            {
                canTeleport = true;
            }

            StartBothGuns(() =>
            {
                if (canTeleport && nray.collider != null)
                {
                    Rigidbody rb = GorillaTagger.Instance.GetComponent<Rigidbody>();
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    Vector3 destination = nray.point + (nray.normal * 0.1f);
                    GorillaTagger.Instance.transform.position = destination;
                    rb.position = destination;
                    GorillaTagger.Instance.StartVibration(true, 0.1f, 0.05f);

                    canTeleport = false;
                }
            }, false);
        }

    }

}
