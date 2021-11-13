using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace CameraMod
{
    [BepInPlugin(modGuid, modName, modVersion)]
    [BepInProcess("Onirism.exe")]
    public class CameraMod : BaseUnityPlugin
    {

        public const string modGuid = nameof(CameraMod);
        public const string modName = "Camera Mod";
        public const string modVersion = "1.0.0";

        private static ConfigEntry<KeyCode> cfgScreenshot { get; set; }
        private static ConfigEntry<KeyboardShortcut> cfgToggleHud { get; set; }

        private static ConfigEntry<KeyCode> cfgToggleFreeCam { get; set; }
        private static ConfigEntry<KeyCode> cfgFreeCamSpeedUp { get; set; }
        private static ConfigEntry<KeyCode> cfgFreeCamSlowDown { get; set; }
        private static ConfigEntry<KeyboardShortcut> cfgIncreaseFov { get; set; }
        private static ConfigEntry<KeyboardShortcut> cfgDecreaseFov { get; set; }
        private static ConfigEntry<KeyboardShortcut> cfgResetFov { get; set; }

        private static ConfigEntry<KeyboardShortcut> cfgToggleAnimMode { get; set; }
        private static ConfigEntry<KeyboardShortcut> cfgNextAnim { get; set; }
        private static ConfigEntry<KeyboardShortcut> cfgPrevAnim { get; set; }

        private static ConfigEntry<int> cfgUpsamplingRate;



        private static bool hudHidden = false;
        private static bool isPaused = false;
        private const float defaultFov = 60f;
        private const float mouseSensitivity = 3f;
        private const float moveSensitivityBase = 0.1f;
        private static float moveSensitivity = moveSensitivityBase;
        private static Vector3 velocity;
        private static bool animMode = false;
        private static int animIndex = 1;

        //private static float fov

        // Awake is called once when both the game and the plug-in are loaded
        private void Awake()
        {
            cfgToggleFreeCam = Config.Bind("Key Bindings", "Toggle free camera", KeyCode.F9, new ConfigDescription("Press the button to toggle free camera mode. Use the standard ingame controls to move the camera.", null, new ConfigurationManagerAttributes { Order = 10 }));
            cfgFreeCamSpeedUp = Config.Bind("Key Bindings", "Speed up camera movement", KeyCode.LeftShift, new ConfigDescription("Hold the button to speed up camera movement.", null, new ConfigurationManagerAttributes { Order = 9 }));
            cfgFreeCamSlowDown = Config.Bind("Key Bindings", "Slow down camera movement", KeyCode.LeftAlt, new ConfigDescription("Hold the button to slow down camera movement.", null, new ConfigurationManagerAttributes { Order = 8 }));
            cfgIncreaseFov = Config.Bind("Key Bindings", "Zoom out / Increase FOV", new KeyboardShortcut(KeyCode.KeypadMinus), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 7 }));
            cfgDecreaseFov = Config.Bind("Key Bindings", "Zoom in / Decrease FOV", new KeyboardShortcut(KeyCode.KeypadPlus), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 6 }));
            cfgResetFov = Config.Bind("Key Bindings", "Zoom / FOV reset", new KeyboardShortcut(KeyCode.KeypadMultiply), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 5 }));

            cfgScreenshot = Config.Bind("Key Bindings", "Take screenshot", KeyCode.M, new ConfigDescription("Press the button to take a screenshot. Refer to the [Screenshots] section for additional configurations.", null, new ConfigurationManagerAttributes { Order = 4 }));
            cfgToggleHud = Config.Bind("Key Bindings", "Toggle HUD", new KeyboardShortcut(KeyCode.F8), new ConfigDescription("Press the button to toggle the HUD.", null, new ConfigurationManagerAttributes { Order = 3 }));

            cfgToggleAnimMode = Config.Bind("Key Bindings", "Toggle animation mode", new KeyboardShortcut(KeyCode.End), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
            cfgNextAnim = Config.Bind("Key Bindings", "Next animation", new KeyboardShortcut(KeyCode.PageUp), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
            cfgPrevAnim = Config.Bind("Key Bindings", "Previous animation", new KeyboardShortcut(KeyCode.PageDown), new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 0 }));


            cfgUpsamplingRate = Config.Bind("Screenshots", "Upsampling Rate", 1, new ConfigDescription("Takes a screenshot at a higher resolution.", new AcceptableValueRange<int>(1, 5)));


            Harmony harmony = new Harmony(modGuid);
            harmony.PatchAll(typeof(CameraMod));

            Logger.LogInfo("Plugin Loaded.");

        }

        private void Update()
        {

            //SCREENSHOT
            if (Input.GetKeyDown(cfgScreenshot.Value))
            {
                GUI.enabled = false;
                ScreenCapture.CaptureScreenshot("Screenshot_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".png", cfgUpsamplingRate.Value);
                GUI.enabled = true;
            }

            //HUD
            if (cfgToggleHud.Value.IsDown())
            {
                hudHidden = !hudHidden;

                foreach (KeyValuePair<int, GameObject> cam in CameraController.cameras)
                {
                    cam.Value.GetComponent<CameraController>().target.GetComponent<CarolController>().hud.canvas.gameObject.SetActive(!hudHidden);
                    cam.Value.GetComponent<CameraController>().target.GetComponent<CarolController>().hud.enabled = !hudHidden;
                }
            }

            //CAMERA
            if (Input.GetKeyDown(cfgToggleFreeCam.Value) && !PauseDiary.manager.isPaused)
            {

                if (!isPaused)
                {
                    Time.timeScale = 1E-05f;
                    isPaused = true;
                    foreach (KeyValuePair<int, GameObject> keyValuePair in CameraController.cameras)
                    {
                        keyValuePair.Value.GetComponent<CameraController>().enabled = false;
                        keyValuePair.Value.GetComponent<CameraController>().target.GetComponent<Inventory>().enabled = false;
                    }
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Time.timeScale = 1f;
                    isPaused = false;
                    foreach (KeyValuePair<int, GameObject> keyValuePair2 in CameraController.cameras)
                    {
                        keyValuePair2.Value.GetComponent<CameraController>().target.GetComponent<Inventory>().enabled = true;
                        keyValuePair2.Value.GetComponent<CameraController>().enabled = true;
                    }
                    Cursor.lockState = CursorLockMode.Locked;
                }
                moveSensitivity = moveSensitivityBase;

            }
            if (isPaused)
            {

                if (Input.GetKeyDown(cfgFreeCamSpeedUp.Value))
                {
                    moveSensitivity = moveSensitivityBase * 2.5f;
                }
                else if (Input.GetKeyDown(cfgFreeCamSlowDown.Value))
                {
                    moveSensitivity = moveSensitivityBase * 0.25f;
                }
                else if (Input.GetKeyUp(cfgFreeCamSpeedUp.Value) || Input.GetKeyUp(cfgFreeCamSlowDown.Value))
                {
                    moveSensitivity = moveSensitivityBase;
                }

                foreach (KeyValuePair<int, GameObject> keyValuePair9 in CameraController.cameras)
                {
                    Vector3 b = new Vector3(InputManager.manager.GetAxis(0, InputManager.InputType.AxisX), 0f, InputManager.manager.GetAxis(0, InputManager.InputType.AxisY)) * moveSensitivity;
                    velocity = Vector3.Lerp(velocity, b, 0.8f);
                    keyValuePair9.Value.GetComponent<CameraController>().transform.position += keyValuePair9.Value.GetComponent<CameraController>().transform.rotation * velocity;
                    velocity *= 0.5f;
                    Vector3 euler = new Vector3(-InputManager.manager.GetAxis(0, InputManager.InputType.CameraAxisY), InputManager.manager.GetAxis(0, InputManager.InputType.CameraAxisX), 0f) * mouseSensitivity;
                    keyValuePair9.Value.GetComponent<CameraController>().transform.rotation *= Quaternion.Euler(euler);
                    keyValuePair9.Value.GetComponent<CameraController>().transform.rotation = Quaternion.Euler(keyValuePair9.Value.GetComponent<CameraController>().transform.rotation.eulerAngles.x, keyValuePair9.Value.GetComponent<CameraController>().transform.rotation.eulerAngles.y, 0f);
                    euler = new Vector3(-InputManager.manager.GetAxis(0, InputManager.InputType.CameraAxisYMouse), InputManager.manager.GetAxis(0, InputManager.InputType.CameraAxisXMouse), 0f) * mouseSensitivity;
                    keyValuePair9.Value.GetComponent<CameraController>().transform.rotation *= Quaternion.Euler(euler);
                    keyValuePair9.Value.GetComponent<CameraController>().transform.rotation = Quaternion.Euler(keyValuePair9.Value.GetComponent<CameraController>().transform.rotation.eulerAngles.x, keyValuePair9.Value.GetComponent<CameraController>().transform.rotation.eulerAngles.y, 0f);
                }
            }

            //FOV
            if (cfgIncreaseFov.Value.IsDown())
            {
                foreach (KeyValuePair<int, GameObject> keyValuePair8 in CameraController.cameras)
                {
                    keyValuePair8.Value.GetComponent<Camera>().fieldOfView = keyValuePair8.Value.GetComponent<Camera>().fieldOfView + 5f;
                }
            }
            if (cfgDecreaseFov.Value.IsDown())
            {
                foreach (KeyValuePair<int, GameObject> keyValuePair9 in CameraController.cameras)
                {
                    if (keyValuePair9.Value.GetComponent<Camera>().fieldOfView > 5f)
                    {
                        keyValuePair9.Value.GetComponent<Camera>().fieldOfView = keyValuePair9.Value.GetComponent<Camera>().fieldOfView - 5f;
                    }
                }
            }

            if (cfgResetFov.Value.IsDown())
            {
                foreach (KeyValuePair<int, GameObject> keyValuePair10 in CameraController.cameras)
                {
                    keyValuePair10.Value.GetComponent<Camera>().fieldOfView = defaultFov;
                }
            }

            //ANIMATIONS
            if (cfgToggleAnimMode.Value.IsDown())
            {
                foreach (Entity pl in Entity.players)
                {
                    if (animMode)
                    {
                        Animator anim = pl.anim;
                        anim.SetLayerWeight(anim.GetLayerIndex("Expressions"), 0f);
                    }
                    else
                    {
                        Animator anim = pl.anim;
                        anim.SetLayerWeight(anim.GetLayerIndex("Expressions"), 1f);
                        base.StartCoroutine(Dialogue.controller.SetExpressionValue(anim, "Expression", animIndex));
                    }
                }
                animMode = !animMode;
            } 
            else if (cfgNextAnim.Value.IsDown()) 
            {
                if (animIndex >= 11)
                {
                    animIndex = 1;
                }
                else
                {
                    animIndex++;
                }

                if (animMode)
                {
                    foreach (Entity pl in Entity.players)
                    {
                        Animator anim = pl.anim;
                        base.StartCoroutine(Dialogue.controller.SetExpressionValue(anim, "Expression", animIndex));
                    }
                }
            }
            else if (cfgPrevAnim.Value.IsDown()) 
            {
                if (animIndex <= 1)
                {
                    animIndex = 11;
                }
                else
                {
                    animIndex--;
                }

                if (animMode)
                {
                    foreach (Entity pl in Entity.players)
                    {
                        Animator anim = pl.anim;
                        base.StartCoroutine(Dialogue.controller.SetExpressionValue(anim, "Expression", animIndex));
                    }
                }
            }

        }


        //Patching pause methods so they won't run if free camera is currently enabled
        [HarmonyPrefix, HarmonyPatch(typeof(Pause_Menu), "Update")]
        private static bool Pause_Menu_UpdatePrefix()
        {
            if (isPaused)
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PauseDiary), "Pause")]
        private static bool PausePrefix()
        {
            if (isPaused)
            {
                return false;
            }
            return true;
        }



    }

}
