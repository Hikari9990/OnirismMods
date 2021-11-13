using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.UI;

namespace ForceEnglishText
{
    [BepInPlugin(modGuid, modName, modVersion)]
    [BepInProcess("Onirism.exe")]
    public class ForceEnglishText : BaseUnityPlugin
    {
        const string modGuid = "ForceEnglishText";
        const string modName = "Force English Text";
        const string modVersion = "1.0";

        public static ManualLogSource logSource = new ManualLogSource(modGuid);

        private void Awake()
        {
            Harmony harmony = new Harmony(modGuid);
            harmony.PatchAll(typeof(ForceEnglishText));

            BepInEx.Logging.Logger.Sources.Add(logSource);
            logSource.LogInfo("Plugin loaded.");
        }


        [HarmonyPostfix, HarmonyPatch(typeof(LocalizationReplace), "SetLanguage")]
        private static void SetLanguage_postfix(LocalizationReplace __instance)
        {
            if (__instance.type == LocalizationReplace.Type.Image)
            {
                __instance.gameObject.GetComponent<Image>().sprite = __instance.image.en;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LocalizationIndex), "AddLine")]
        private static bool AddLine_prefix(LocalizationIndex.LanguageType temp, string reference, string data)
        {
            if (temp == LocalizationIndex.LanguageType.French)
            {
                return false;
            }
            else
            {
                LocalizatioLanguage.indexEn.Add(reference, data);
                LocalizatioLanguage.indexEnglish.Add(new LocalizatioLanguage.IndexElement
                {
                    reference = reference,
                    text = data
                });

                LocalizatioLanguage.indexFr.Add(reference, data);
                LocalizatioLanguage.indexFrench.Add(new LocalizatioLanguage.IndexElement
                {
                    reference = reference,
                    text = data
                });

                return false;
            }
        }


    }
}
