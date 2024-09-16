using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using FrooxEngine.UIX;

namespace AxisAlignerDebug
{
    public class Patch : ResoniteMod
    {
        public override String Name => "AxisAlignerDebug";
        public override String Author => "zahndy";
        public override String Link => "https://github.com/zahndy/AxisAlignerDebug";
        public override String Version => "1.0.0";

        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("enabled", "Enabled", () => true);

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);
            Harmony harmony = new Harmony("com.zahndy.AxisAlignerDebug");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(AxisAligner))]
        class AxisAligner_OnAwake_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnAwake")]
            static void Postfix(AxisAligner __instance)
            {

                if (Config.GetValue(ENABLED))
                {
                    Msg("AxisAligner OnAwake: ");
                    User use = __instance.Slot.ActiveUser;
                    Slot slotn = __instance.Slot;
                    Msg("AxisAligner User: "+use.UserName);
                    Msg("AxisAligner Slot: "+slotn.Name);
                    Msg("AxisAligner Hashcode: ");
                    Msg(__instance.GetHashCode());
                    if (use != null)
                    {
                         //user                                   
                    }
                    else 
                    {
                        //world 
                        //AxisAligner align;
                    }
                }
            }
            [HarmonyPrefix]
            [HarmonyPatch("Dispose")]
            static void Prefix(AxisAligner __instance)
            {

                if (Config.GetValue(ENABLED))
                {
                    Msg("AxisAligner Disposed");
                    Msg(__instance.GetHashCode());
                }
            }
        }

        [HarmonyPatch(typeof(EngineDebugDialog))]
        class EngineDebugDialog_GenerateBackgroundWorkerDiagnostic_Patch
        {
           /* [HarmonyPrefix]
            [HarmonyPatch("OnAttach")]
            static void Postfix(EngineDebugDialog __instance)
            {

                if (Config.GetValue(ENABLED))
                {
                    Msg("EngineDebugDialog OnAttach:");
                    Msg(__instance.Slot.ChildrenHierarchyToString());
                    Msg(__instance._contentRoot.Target.ChildrenHierarchyToString());
                    
                }
            }*/

            [HarmonyPrefix]
            [HarmonyPatch("GenerateBackgroundWorkerDiagnostic")]
            static void Prefix(Engine engine, StringBuilder str)
            {
                if (Config.GetValue(ENABLED))
                {
                    str.AppendLine("--------");
                    str.AppendLine("- Test -");
                    // return false;
                }
            }
        }
    }
}
