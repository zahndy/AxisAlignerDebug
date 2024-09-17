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
using System.Security.Cryptography.X509Certificates;
using static AxisAlignerDebug.Patch;

namespace AxisAlignerDebug
{
    public class Patch : ResoniteMod
    {
        public override String Name => "AxisAlignerDebug";
        public override String Author => "zahndy";
        public override String Link => "https://github.com/zahndy/AxisAlignerDebug";
        public override String Version => "1.0.0";

        public static ModConfiguration Config;

        static public SlotRefList _SlotRefList;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("enabled", "Enabled", () => true);

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);
            Harmony harmony = new Harmony("com.zahndy.AxisAlignerDebug");
            harmony.PatchAll();
            _SlotRefList = new SlotRefList();
        }

        public class SlotRef
        {
            public User user = null;
            public string UserName;
            public Slot slot;
            public AxisAligner instance;
            public int Hash;
            public SlotRef()
            {
                UserName = "World";
            }
        }

        public class SlotRefList
        {
            public List <SlotRef> RefList;

            public SlotRefList()
            {
                RefList = new List <SlotRef>();
            }
            public void AddRef(AxisAligner component)
            {
                Msg("AddRef");
                SlotRef sr = new SlotRef();
                User usr = component.Slot.ActiveUser;
                if (usr != null)
                {
                    sr.user = usr;
                    sr.UserName = usr.UserName;
                }
                sr.instance = component;
                sr.slot = component.Slot;
                sr.Hash = component.GetHashCode();
                RefList.AddItem(sr);
                Msg("AddItem");
               //Array.Sort(RefList);
               Msg(string.Join(",", RefList));
                RefList = RefList.OrderBy(var => var != null ? var.UserName : "Null").ToList();
            }
            public void RemoveRef(int hash)
            {
                RefList = RefList.Where(refe => refe.Hash != hash).ToList();
            }
            public void PrintDebug(StringBuilder str)
            {
                str.AppendLine(" --------------- axisaligners --------------- ");
                if (RefList != null)
                {
                    if (RefList.Count > 0)
                    {
                        foreach (var slotref in RefList)
                        {
                            str.AppendLine(slotref.UserName + " : " + slotref.slot.Name + " - " + slotref.slot.GetObjectRoot());
                        }
                        str.AppendLine(" RefList.Count > 0 ");
                    }
                    else
                    {
                        str.AppendLine(" RefList.Count < 0 ");
                    }
                }
                else 
                {
                    str.AppendLine(" RefList = null ");
                }
                str.AppendLine(" -------------------------------------------- ");
            }
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
                    _SlotRefList.AddRef(__instance);
                }
            }
            
        }
        [HarmonyPatch(typeof(AutoAddChildrenBase))]
        class AxisAligner_AutoAddChildrenBase_OnDispose_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnDispose")]
            static void Postfix(AutoAddChildrenBase __instance)
            {

                if (Config.GetValue(ENABLED))
                {
                    Msg("AutoAddChildrenBase OnDispose");
                    //_SlotRefList.RemoveRef(__instance.GetHashCode());
                }
            }
        }

            [HarmonyPatch(typeof(EngineDebugDialog))]
        class EngineDebugDialog_GenerateBackgroundWorkerDiagnostic_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("GenerateBackgroundWorkerDiagnostic")]
            static void Prefix(Engine engine, StringBuilder str)
            {
                if (Config.GetValue(ENABLED))
                {
                    _SlotRefList.PrintDebug(str);
                }
            }
        }
    }
}
