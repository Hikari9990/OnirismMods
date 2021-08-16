using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FirstPerson
{
    [BepInPlugin(modGuid, modName, modVersion)]
    [BepInProcess("Onirism.exe")]
    public class FirstPerson : BaseUnityPlugin
    {

        public const string modGuid = nameof(FirstPerson);
        public const string modName = "First Person";
        public const string modVersion = "1.0";


        private static bool FPEnabled = false;

        private static ConfigEntry<KeyboardShortcut> cfgEnableFPSMode { get; set; }
        
        private static ConfigEntry<float> cfgCameraFOV { get; set; }
        private static ConfigEntry<float> cfgCameraOffsetX { get; set; }
        private static ConfigEntry<float> cfgCameraOffsetY { get; set; }
        private static ConfigEntry<float> cfgCameraOffsetZ { get; set; }

        private static ConfigEntry<string> cfgComponentsToHide { get; set; }

        private static Dictionary<string, List<string>> outfitComponents;


        private void Awake()
        {
            
            cfgEnableFPSMode = Config.Bind("Key Bindings", "Toggle First Person mode", new KeyboardShortcut(KeyCode.X));
            cfgCameraFOV = Config.Bind("Camera", "Field of view", 80f);
            cfgCameraOffsetX = Config.Bind("Camera", "Camera position X", -0.1f);
            cfgCameraOffsetY = Config.Bind("Camera", "Camera position Y", 0.8f);
            cfgCameraOffsetZ = Config.Bind("Camera", "Camera position Z", 0.0f);

            cfgComponentsToHide = Config.Bind("Other", "Components to hide", "tete,Pyjm_hair017,Pyjm_hair017 (2),Pyjm_hair017 (3),Pyjm_hair018,Pyjm_R_Eye018,Cap,Russianhat,xmashat,Crown,Valkyrie_Helmet,ladybug_accessories,earrings,P,P (1),P (2),P (3),Hair_Flowing,Hair_Flowing (1),Hair_Flowing (2),Hair_Flowing (3),Hair_Flowing002,Hair_Flowing003,Hair_pigtails,eye (5),Frenchiehat,Helmet,Spacesuithair,hat,accessories,Sphere,Proton_backpack,gasmask001,magichat,Cap_lumberjack,Glasses,Object001,Hairflower,P1,P2,P3,P4,RobesHooded,halloweenoutfit,swimgoogles", 
                new ConfigDescription("List containing the objects within the player hierarchy that will be set to hidden while in first person mode. Only used for outfits that aren't handled by the mod itself.",null,new ConfigurationManagerAttributes { IsAdvanced = true }));

            outfitComponents = new Dictionary<string, List<string>>();
            outfitComponents["CAROL_PyjamaBasic(Clone)"] = new List<string> { "tete", "Pyjm_R_Eye018", "Pyjm_hair017", "Hair_Flowing", "Hair_Flowing (1)", "Hair_Flowing (2)", "Hair_Flowing (3)" };
            outfitComponents["CAROL_HoodieClassic(Clone)"] = new List<string> { "tete", "Pyjm_R_Eye018", "Pyjm_hair017", "Hair_Flowing", "Hair_Flowing (1)", "Hair_Flowing (2)", "Hair_Flowing (3)", "earrings" };
            outfitComponents["CAROL_xmas(Clone)"] = new List<string> { "tete", "Pyjm_R_Eye018", "Pyjm_hair017 (2)", "winter_accessories", "wintergoogles" };
            outfitComponents["CAROL_Wintercoat(Clone)"] = new List<string> { "tete", "Pyjm_R_Eye018", "Pyjm_R_Eye018 (1)", "Pyjm_R_Eye018 (2)", "Pyjm_R_Eye018 (3)", "Hair_Hoodoff", "Hair_Hoodie", "Skimask", "winterCoat", "Skigoggles","Earmuffs" };
            outfitComponents["CAROL_Wintercoa2t(Clone)"] = new List<string> { "tete", "Pyjm_R_Eye018", "Pyjm_R_Eye018 (1)", "Pyjm_R_Eye018 (2)", "Pyjm_R_Eye018 (3)", "Hair_Hoodoff", "Hair_Hoodie", "Skimask", "winterCoat", "Skigoggles", "Earmuffs" };
            outfitComponents["CAROL_Wintercoa3t(Clone)"] = new List<string> { "tete", "Pyjm_R_Eye018", "Pyjm_R_Eye018 (1)", "Pyjm_R_Eye018 (2)", "Pyjm_R_Eye018 (3)", "Hair_Hoodoff", "Hair_Hoodie", "Skimask", "winterCoat", "Skigoggles", "Earmuffs" };
            outfitComponents["CAROL_FrozenPrincess(Clone)"] = new List<string> { "tete", "Headband", "Hair_Flowing001", "Pyjm_R_Eye018" };
            outfitComponents["CAROL_NinjaIce(Clone)"] = new List<string> { "tete", "Ninjamask", "ninja", "Pyjm_R_Eye018", "Pyjm_hair018" };
            outfitComponents["CAROL_SummerBlast2(Clone)"] = new List<string> { "tete", "Pyjm_R_Eye018", "Pyjm_hair017", "earrings", "Cap" };
            outfitComponents["CAROL_SummerBlast3(Clone)"] = new List<string> { "tete", "Pyjm_R_Eye018", "Hair_twinbraid", "earrings", "swimgoogles" };


            Harmony harmony = new Harmony(modGuid);
            harmony.PatchAll(typeof(FirstPerson));

            Logger.LogInfo("Plugin loaded.");
        }

        private void Update()
        {

            if (cfgEnableFPSMode.Value.IsDown())
            {
                FPEnabled = !FPEnabled;

                if (FPEnabled && Entity.players != null && Entity.players[0] != null)
                {
                    Inventory inv = Entity.players[0].GetComponent<Inventory>();
                    
                    if (Entity.players[0].GetComponent<CarolController>().weaponDrawn)
                    {
                        inv.playerCam.GetComponent<Camera>().fieldOfView = cfgCameraFOV.Value;
                        inv.playerCam.GetComponent<Camera>().nearClipPlane = 0.02f;
                        HideObjects();    
                    }

                }
                else if (!FPEnabled && Entity.players != null && Entity.players[0] != null)
                {
                    
                    Inventory inv = Entity.players[0].GetComponent<Inventory>();
                    inv.playerCam.GetComponent<Camera>().fieldOfView = 60f;
                    inv.playerCam.GetComponent<Camera>().nearClipPlane = 0.15f;

                    UnhideObjects();
                }

            }

        }

        public static List<GameObject> FindInHierarchy(Transform parent, List<string> names, List<GameObject> objectsList)
        {
            if (objectsList == null) objectsList = new List<GameObject>();

            foreach (Transform child in parent)
            {
                if (names.Contains(child.name))
                {
                    objectsList.Add(child.gameObject);
                }
                FindInHierarchy(child, names, objectsList);
            }

            return objectsList;
        }

        public static void HideObjects()
        {
            List<GameObject> objects;
            if (outfitComponents.ContainsKey(Entity.players[0].modelData.name))
            {
                objects = FindInHierarchy(Entity.players[0].transform, outfitComponents[Entity.players[0].modelData.name], null);
            }
            else
            {
                objects = FindInHierarchy(Entity.players[0].transform, cfgComponentsToHide.Value.Split(',').ToList(), null);
            }

            if (objects != null)
            {
                foreach (GameObject ob in objects)
                {
                    if (ob.gameObject.activeSelf)
                    {
                        ob.SetActive(false);
                    }
                }
            }
        }

        public static void UnhideObjects()
        {
            List<GameObject> objects;
            if (outfitComponents.ContainsKey(Entity.players[0].modelData.name))
            {
                objects = FindInHierarchy(Entity.players[0].transform, outfitComponents[Entity.players[0].modelData.name], null);
            }
            else
            {
                objects = FindInHierarchy(Entity.players[0].transform, cfgComponentsToHide.Value.Split(',').ToList(), null);
            }
            

            if (objects != null)
            {
                foreach (GameObject ob in objects)
                {
                    ob.SetActive(true);
                    CoopModelToggle[] models = ob.transform.GetComponentsInChildren<CoopModelToggle>(true);
                    foreach (CoopModelToggle md in models)
                    {
                        md.gameObject.SetActive(true);
                        md.enabled = true;
                    }

                }


            }

            SaveManager.SaveData saveData = SaveManager.manager.data[SaveManager.manager.saveSlotCurrent];
            Entity.players[0].ToggleAccessory(saveData.players[0].inventory.accessory);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Inventory), "DrawWeapon")]
        private static void DrawWeapon_Postfix(GameObject weapon)
        {

            if (!FPEnabled) return;

            if (Entity.players != null && Entity.players[0] != null)
            {

                if (weapon != null)
                {
                    Inventory inv = Entity.players[0].GetComponent<Inventory>();
                    inv.playerCam.GetComponent<Camera>().fieldOfView = cfgCameraFOV.Value;
                    inv.playerCam.GetComponent<Camera>().nearClipPlane = 0.02f;
                    HideObjects();
                }
                else
                {
                    Inventory inv = Entity.players[0].GetComponent<Inventory>();
                    inv.playerCam.GetComponent<Camera>().fieldOfView = 60f;
                    inv.playerCam.GetComponent<Camera>().nearClipPlane = 0.15f;
                    UnhideObjects();
                }

            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Inventory), "HideHeldWeapon")]
        private static void HideHeldWeapon_Postfix()
        {
            if (Entity.players != null && Entity.players[0] != null)
            {
                UnhideObjects();
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Inventory), "LateUpdate")]
        private static void LateUpdate_Postfix(Inventory __instance)
        {

            if (!FPEnabled) return;

            if (Entity.players != null && Entity.players[0] != null && __instance.activeWeapon != null)
            {
                Vector3 cameraOffset = new Vector3(cfgCameraOffsetX.Value, cfgCameraOffsetY.Value, cfgCameraOffsetZ.Value);
                __instance.playerCam.transform.position = __instance.gameObject.transform.position + Vector3.Lerp(Quaternion.Euler(-__instance.manualAimAngle.eulerAngles.z, __instance.gameObject.transform.rotation.eulerAngles.y, 0f) * cameraOffset, Quaternion.Euler(0f, __instance.gameObject.transform.rotation.eulerAngles.y, 0f) * cameraOffset, 0.5f);
            }
        }


    }
}
