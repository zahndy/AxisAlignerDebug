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
using Newtonsoft.Json;
using System.ComponentModel;

namespace AxisAlignerDebug
{
    public class Patch : ResoniteMod
    {
        public override String Name => "AxisAlignerDebug";
        public override String Author => "zahndy";
        public override String Link => "https://github.com/zahndy/AxisAlignerDebug";
        public override String Version => "1.0.0";

        public static ModConfiguration Config;

        private static SlotRefList _SlotRefList;
        private static List<SlotRef> RefList;

        private static string _testvar = "";

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("enabled", "Enabled", () => true);

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);
            Harmony harmony = new Harmony("com.zahndy.AxisAlignerDebug");
            harmony.PatchAll();
            RefList = new List<SlotRef>();
            _SlotRefList = new SlotRefList();
        }

        private class SlotRef
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

        static class SlotList
        {
           // Slot user = RootSpace.DefaultSpace.LocalUserSpace
           // Slot FlagSlot = user.LocalUserRoot.Slot.AddSlot("CustomBadgesFlag");
           // FlagSlot.Tag = "CustomBadgesFlag";

                //user / id
                //world / id

        }

        private class SlotRefList
        {                 

            public void AddRef(AxisAligner component)
            {
                Msg("AxisAligner OnAwake: " + component.Slot.Parent.Name);
                Msg(component.Slot.ParentHierarchyToString());
                SlotRef sr = new SlotRef();
                User usr = component.Slot.ActiveUser;
                if (usr != null)
                {
                    sr.user = usr;
                    sr.UserName = usr.UserName;
                    Msg("from: " + usr.UserName);
                }
                sr.instance = component;
                sr.slot = component.Slot;
                sr.Hash = component.GetHashCode();
                Msg("AxisAligner __instance.Slot.Name: " + component.Slot.Parent.Name);
                Msg("AddRef sr.slot.Name: " + sr.slot.Parent.Name);
                RefList.AddItem(sr);
                Msg("AddItem: " + sr.Hash);
                //Array.Sort(RefList);
                //Msg(string.Join(",", RefList));
                Msg("RefList Length: " + RefList.Count.ToString());
                Msg("RefList: " + JsonConvert.SerializeObject(RefList));

                RefList = RefList.OrderBy(var => var != null ? var.UserName : "Null").ToList();
            }
            public void RemoveRef(int hash)
            {
                RefList = RefList.Where(refe => refe.Hash != hash).ToList();
            }
            public void PrintDebug(StringBuilder str)
            {
              
            }
            public int Cnt()
            {
                return RefList.Count;
            }
        }


         [HarmonyPatch(typeof(AxisAligner))]
        class AxisAligner_OnAwake_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnAwake")] // change to OnCommonUpdate
            static void Postfix(AxisAligner __instance)
            {
                if (Config.GetValue(ENABLED))
                {
                   Patch._SlotRefList.AddRef(__instance); 
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
                    // _SlotRefList.PrintDebug(str);

                    str.AppendLine(" --------------- axisaligners --------------- ");

                        if (Patch._SlotRefList.Cnt() > 0)
                        {
                           // foreach (var slotref in RefList)
                           // {
                           //     str.AppendLine(slotref.UserName + " : " + slotref.slot.Name + " - " + slotref.slot.GetObjectRoot());
                           // }
                            str.AppendLine(" RefList.Count > 0  :) ");
                        }
                        else
                        {
                            str.AppendLine(" RefList.Count < 0 ");
                        }
                    str.AppendLine(" -------------------------------------------- ");
                }
            }
        }
    }
}
