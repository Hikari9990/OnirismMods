using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace ColorSwap
{
    [BepInPlugin(modGuid, modName, modVersion)]
    [BepInProcess("Onirism.exe")]
    public class ColorSwap : BaseUnityPlugin
    {
        public const string modGuid = nameof(ColorSwap);
        public const string modName = "Color Swap";
        public const string modVersion = "1.0";

        private static ConfigEntry<KeyboardShortcut> cfgKeyNext { get; set; }
        private static ConfigEntry<KeyboardShortcut> cfgKeyPrevious { get; set; }
        private static ConfigEntry<int> cfgCarolColor { get; set; }

        private void Awake()
        {
            cfgKeyNext = Config.Bind("Key Bindings", "Next", new KeyboardShortcut(KeyCode.F12));
            cfgKeyPrevious = Config.Bind("Key Bindings", "Previous", new KeyboardShortcut(KeyCode.F11));
            cfgCarolColor = Config.Bind("Other", "Default Color", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 3)));

            Harmony harmony = new Harmony(modGuid);
            harmony.PatchAll(typeof(ColorSwap));

            Logger.LogInfo("Plugin loaded.");
        }

        private void Update()
        {
            if (cfgKeyNext.Value.IsDown())
            {
                if (cfgCarolColor.Value == 3)
                {
                    cfgCarolColor.Value = 0;
                }
                else
                {
                    cfgCarolColor.Value++;
                }
                SwapColor();
            }
            else if (cfgKeyPrevious.Value.IsDown())
            {
                if (cfgCarolColor.Value == 0)
                {
                    cfgCarolColor.Value = 3;
                }
                else
                {
                    cfgCarolColor.Value--;
                }
                SwapColor();
            }


        }

        private static void SwapColor()
        {
            foreach (Entity entity in Entity.players)
            {
                GameObject currentWeapon = entity.GetComponent<Inventory>().weaponsObjects[0];
                if (currentWeapon != null)
                {
                    CoopModelToggle[] models = currentWeapon.transform.GetComponentsInChildren<CoopModelToggle>(true);
                    foreach (CoopModelToggle md in models)
                    {
                        md.gameObject.SetActive(true);
                        md.enabled = true;
                    }
                }

                GameObject currentMelee = entity.GetComponent<Inventory>().pickedMelee;
                if (currentMelee != null)
                {
                    CoopModelToggle[] models = currentMelee.transform.GetComponentsInChildren<CoopModelToggle>(true);
                    foreach (CoopModelToggle md in models)
                    {
                        md.gameObject.SetActive(true);
                        md.enabled = true;
                    }
                }

                GameObject currentModel = entity.currentModel.model;
                if (currentModel != null)
                {
                    CoopModelToggle[] models = currentModel.transform.GetComponentsInChildren<CoopModelToggle>(true);
                    foreach (CoopModelToggle md in models)
                    {
                        md.gameObject.SetActive(true);
                        md.enabled = true;
                    }
                }
            }
        }


        [HarmonyPrefix, HarmonyPatch(typeof(CoopModelToggle), "Init")]
        private static bool Init_Prefix(CoopModelToggle __instance)
        {
            Entity entity = __instance.transform.root.GetComponent<Entity>();
            if (entity != null)
            {
                if (Entity.players.IndexOf(entity) != 0)
                {
                    return true;
                }
                else
                {
                    __instance.gameObject.SetActive(cfgCarolColor.Value == __instance.playerNumberToggle);
                    return false;
                }
            }
            return true;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(CoopModelToggle), "Update")]
        private static bool Update_Prefix(CoopModelToggle __instance)
        {
            Entity entity = __instance.transform.root.GetComponent<Entity>();
            if (entity != null)
            {
                if (Entity.players.IndexOf(entity) != 0)
                {
                    return true;
                }
                else
                {
                    __instance.gameObject.SetActive(cfgCarolColor.Value == __instance.playerNumberToggle);
                    __instance.enabled = false;
                    return false;
                }
            }
            return true;
        }

    }
}
