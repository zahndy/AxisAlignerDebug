using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using Component = FrooxEngine.Component;
using User = FrooxEngine.User;
using Elements.Core;

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

        private static SlotRefList _SlotRefList;       
        private static HashSet<int> ComparisonSet;

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);

            _SlotRefList = new SlotRefList();
            ComparisonSet = new HashSet<int>();

            Harmony harmony = new Harmony("com.zahndy.AxisAlignerDebug");
            harmony.PatchAll();
            
        }

        private class SlotRef
        {
            public AxisAligner instance;
            public User user = null;
            public string UserName;
            public Slot slot;
            public int hash;
            public Slot objRoot;
            public string worldName;
            public SlotRef(AxisAligner component)
            {
                instance = component;
                worldName = "<color=#ed8a09>" + instance.World.Name+"</color>";
                slot = component.Slot;
                hash = slot.GetHashCode();
                user = slot.ActiveUser;
                objRoot = slot.GetObjectRoot();
                if (user == null)
                {
                    UserName = "<color=#06E>World</color>";
                    Msg("Component Without User");
                }
                else 
                { 
                    UserName = "<color=#8cc90b> " + user.UserName+ "</color> ";
                    Msg("Component from User: " + UserName);
                }   
            }        
        }

        private class SlotRefList
        {
            private static List<SlotRef> RefList;
            StringBuilder outputStr;
            public SlotRefList()
            {
                RefList = new List<SlotRef>();
                outputStr = new StringBuilder();
            }
            public void AddRef(AxisAligner component)
            {
                SlotRef slotref = new SlotRef(component);

                RefList.Add(slotref);
                ComparisonSet.Add(slotref.hash);

                RefList = RefList.OrderBy(var => var != null ? var.UserName : "Null").ToList();
                RebuildDebugOutput();
            }
            public void RemoveRef(int hash)
            {
                RefList = RefList.Where(refe => refe.hash != hash).ToList();
                ComparisonSet.Remove(hash);
                RebuildDebugOutput();
            }
            void RebuildDebugOutput()
            {
                outputStr = new StringBuilder();
                string newline = System.Environment.NewLine;
                outputStr.AppendLine(newline);
                outputStr.AppendLine(" AxisAligners: " + _SlotRefList.Cnt().ToString());
                outputStr.AppendLine(newline);
                if (_SlotRefList.Cnt() > 0)
                {
                    foreach (var slotref in RefList)
                    {
                        outputStr.AppendLine(
                            slotref.worldName
                            + "  :  " + slotref.UserName
                            + "  -  " + slotref.objRoot.Name
                            + "  ...  " + slotref.slot.Parent.Name
                            + "  ->  " + slotref.slot.Name
                            );
                        //outputStr.AppendLine(newline);
                    }
                }
                else
                {
                    outputStr.AppendLine(" No AxisAligners Present... ");
                }
                outputStr.AppendLine(newline);
                outputStr.AppendLine(" -------------------------------------------- ");

            }
            public void PrintDebug(StringBuilder str)
            {
                str.Append(outputStr);
            }
            public int Cnt()
            {
                return RefList.Count;
            }
            public void Clear() 
            { 
                RefList.Clear();
                ComparisonSet.Clear();
                outputStr = new StringBuilder();
                outputStr.AppendLine("");
            }
        }

        [HarmonyPatch(typeof(AxisAligner))]
        class AxisAligner_OnCommonUpdate_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnCommonUpdate")] 
            static void Postfix(AxisAligner __instance)
            {
                if (Config.GetValue(ENABLED))
                {
                    int slothash = __instance.Slot.GetHashCode();                 
                    if (!ComparisonSet.Contains(slothash)) // bool contains = RefList.Any(p => p.Hash == slothash);
                    {
                        ComparisonSet.Add(slothash);
                        _SlotRefList.AddRef(__instance);
                    }
                }
            }
            
        }

       /* [HarmonyPatch(typeof(WorldManager))]
        class WorldManager_FocusWorld_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("FocusWorld")]
            static void Prefix(WorldManager __instance)
            {
                _SlotRefList.Clear();
                Msg("FocusWorld Prefix");
            }

        }*/

        [HarmonyPatch(typeof(AutoAddChildrenBase))]
        class AxisAligner_AutoAddChildrenBase_OnDispose_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnDispose")]
            static void Postfix(AutoAddChildrenBase __instance)
            {
                if (Config.GetValue(ENABLED))
                {
                    int hash = __instance.Slot.GetHashCode();                  
                    if (ComparisonSet.Contains(hash)) //bool contains = RefList.Any(p => p.Hash == hash);
                    {
                        _SlotRefList.RemoveRef(hash);
                        ComparisonSet.Remove(hash);
                       // Msg("Disposing: " + hash.ToString());
                    }

                }
            }
        }

        [HarmonyPatch(typeof(EngineDebugDialog))]
        class EngineDebugDialog_GenerateBackgroundWorkerDiagnostic_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("GenerateBackgroundWorkerDiagnostic")]
            static void Prefix(EngineDebugDialog __instance, Engine engine, StringBuilder str)
            {
               if (Config.GetValue(ENABLED))
               {
                  _SlotRefList.PrintDebug(str);
               }
            }
        }

    }
}
