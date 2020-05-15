using BepInEx.Harmony;
using HarmonyLib;
using System;
using System.Reflection;

namespace COM3D2.MoreFacilities.Plugin.Core
{
    internal static class MoreFacilitiesHooks
    {
        private static bool initialized;
        private static HarmonyLib.Harmony instance;

        public static void Initialize()
        {
            //Copied from examples
            if (MoreFacilitiesHooks.initialized)
                return;

            MoreFacilitiesHooks.instance = HarmonyWrapper.PatchAll(typeof(MoreFacilitiesHooks), "org.guest4168.morefacilitiesplugin.hooks.base");
            MoreFacilitiesHooks.initialized = true;

            //Plugin variables
            patcher_casino_vanilla_save = false;
            patcher_mod_save = false;
            patcher_mod_load = false;

            UnityEngine.Debug.Log("MoreFacilities: Hooks Initialize");
        }

        #region Patching COM3D2 Class Objects
        [HarmonyPatch(typeof(FacilityManager), nameof(FacilityManager.Init))]
        [HarmonyPrefix]
        static void Prefix(FacilityManager __instance)
        {
            UnityEngine.Debug.Log("MoreFacilities: FacilityManager Init Prefix");

            //Update private instance variable for max number of facilities
            typeof(FacilityManager).GetField("FacilityCountMax", BindingFlags.Instance | BindingFlags.Public).SetValue(__instance, MoreFacilities.MaxFacilities);
        }
        #endregion


        #region Patching COM3D2 Save/Load
        //As recommended by users, in case a user wishes to remove this plugin in the future, we do not want them to lose their entire save data
        //So the purpose of this patcher code is to save a data file containing all the extra facilities, as well as a vanilla data file that only has the first 12
        //Originally, attempts were made to only save the Facility data in a separate file, but it also needed CasinoMgrData, which has errors when a casino is outside the first 12, so we save everything now
        
        private static bool patcher_casino_vanilla_save; //tells us if doing a vanilla save

        private static bool patcher_mod_save; //tells us if doing a modded save
        private static bool patcher_mod_load; //tells us if doing a modded load

        private static System.Collections.Generic.List<Facility> savedFacilities; //cache of extra facilities

        #region GameMain

        #region Serialize
        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Serialize), new Type[] { typeof(int), typeof(string) })]
        [HarmonyPrefix]
        static void GameMain_Serialize_Prefix(int f_nSaveNo, string f_strComment, GameMain __instance)
        {
            //Make sure the user clicked a save slot, and that this is not the fake save (avoid infinite loop)
            if (f_nSaveNo != -1 && !patcher_mod_save)
            {
                UnityEngine.Debug.Log("MoreFacilities: GameMain Serialize Prefix");

                //Make a fake save file
                patcher_mod_save = true;
                __instance.Serialize(f_nSaveNo, f_strComment);
                patcher_mod_save = false;

                //Cache the Facility data to restore after vanilla saving
                savedFacilities = new System.Collections.Generic.List<Facility>();

                //We only care if there are more than 12
                if (__instance.FacilityMgr.GetFacilityArray().Length > 12)
                {
                    savedFacilities.AddRange(((System.Collections.Generic.List<Facility>)typeof(FacilityManager).GetField("m_FacilityArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance.FacilityMgr)).GetRange(12, __instance.FacilityMgr.GetFacilityArray().Length - 12));

                    //Remove all the extra facilities for vanilla saving
                    ((System.Collections.Generic.List<Facility>)typeof(FacilityManager).GetField("m_FacilityArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance.FacilityMgr)).RemoveRange(12, __instance.FacilityMgr.GetFacilityArray().Length - 12);
                }
                //Prep Casino Data
                patcher_casino_vanilla_save = true;
            }
        }

        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Serialize), new Type[] { typeof(int), typeof(string) })]
        [HarmonyPostfix]
        static void GameMain_Serialize_Postfix(int f_nSaveNo, string f_strComment, GameMain __instance)
        {
            //Make sure the user clicked a save slot, and that this is not the fake save
            if (f_nSaveNo != -1 && !patcher_mod_save)
            {
                UnityEngine.Debug.Log("MoreFacilities: GameMain Serialize Postfix");

                //Return to modded functionality
                patcher_casino_vanilla_save = false;

                //Reload cached facilities after finished saving vanilla data
                ((System.Collections.Generic.List<Facility>)typeof(FacilityManager).GetField("m_FacilityArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance.FacilityMgr)).AddRange(savedFacilities);
                savedFacilities = new System.Collections.Generic.List<Facility>();
            }
        }

        #endregion

        #region Deserialize
        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Deserialize))]//, new Type[] { typeof(int), typeof(bool) })]
        [HarmonyPrefix]
        static void GameMain_Deserialize_Prefix(int f_nSaveNo, GameMain __instance)
        {
            //Make sure the user clicked a load slot
            if (f_nSaveNo != -1)
            {
                UnityEngine.Debug.Log("MoreFacilities: GameMain Deserialize Prefix");

                //Set variable that will re-route the save file name during fetch
                patcher_mod_load = true;
            }
        }

        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Deserialize))]//, new Type[] { typeof(int), typeof(bool) })]
        [HarmonyPostfix]
        static void GameMain_Deserialize_Postfix(int f_nSaveNo, GameMain __instance)
        {
            //Make sure the user clicked a save slot
            if (f_nSaveNo != -1)
            {
                UnityEngine.Debug.Log("MoreFacilities: GameMain Deserialize Postfix");

                //Reset variable for routing save file name
                patcher_mod_load = false;
            }
        }
        #endregion

        #region Delete
        [HarmonyPatch(typeof(GameMain), nameof(GameMain.DeleteSerializeData), new Type[] { typeof(int) })]
        [HarmonyPostfix]
        static void GameMain_DeleteSerializeData_Postfix(int f_nSaveNo, GameMain __instance)
        {
            UnityEngine.Debug.Log("MoreFacilities: GameMain DeleteSerializeData Postfix");

            //Get fake path
            string path = getFakeSavePath(f_nSaveNo);

            //Delete the extra save file data
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        #endregion

        [HarmonyPatch(typeof(GameMain), nameof(GameMain.MakeSavePathFileName), new Type[] { typeof(int) })]
        [HarmonyPostfix]
        static void GameMain_MakeSavePathFileName(int f_nSaveNo, ref String __result)
        {
            //If saving non-vanilla(fake) data or loading non-vanilla(fake) data
            if (patcher_mod_save || patcher_mod_load)
            {
                //Get fake path
                string path = getFakeSavePath(f_nSaveNo);

                //If performing a load, if this is first time a save was loaded with this plugin, they wont have the fake save file
                if (patcher_mod_load && System.IO.File.Exists(path) || patcher_mod_save)
                {
                    __result = path;
                }
            }
        }

        private static string getFakeSavePath(int f_nSaveNo)
        {
            //Normal path
            string path = UTY.gameProjectPath + "\\" + "SaveData";
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            //Add "_MoreFacilities"
            return path + "/" + string.Format("SaveData{0:D3}_MoreFacilities", (f_nSaveNo)) + ".save";
        }

        #endregion

        #region CasinoDataMgr

        [HarmonyPatch(typeof(CasinoDataMgr), "ReadDealerData")]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
        [HarmonyPrefix]
        static void CasinoDataMgr_ReadDealerData_Prefix(System.IO.BinaryReader br, bool is_upward, CasinoDataMgr __instance, ref System.Collections.Generic.Dictionary<Facility, global::DealerMaid> ___m_FacilityDealerPair)
        {
            //In early attempt, only wanted to save partial data in separate files, but CasinoDataMgr kept breaking cause it duplicates things, so no longer used
            UnityEngine.Debug.Log(String.Format("MoreFacilities: CasinoDataMgr ReadDealerData Prefix "));
        }

        [HarmonyPatch(typeof(CasinoDataMgr), "ReadDealerData")]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
        [HarmonyPostfix]
        static void CasinoDataMgr_ReadDealerData_Postfix(System.IO.BinaryReader br, bool is_upward, CasinoDataMgr __instance, ref System.Collections.Generic.Dictionary<Facility, global::DealerMaid> ___m_FacilityDealerPair)
        {
            //In early attempt, only wanted to save partial data in separate files, but CasinoDataMgr kept breaking cause it duplicates things, so no longer used
            UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr ReadDealerData Postfix");
        }

        [HarmonyPatch(typeof(CasinoDataMgr), nameof(CasinoDataMgr.GetCasinoFacility))]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
        [HarmonyPostfix]
        static void CasinoDataMgr_GetCasinoFacility_Postfix(ref Facility __result, bool is_upward = false)
        {
            //CasinoDataMgr throws an error when loading "blackjack dealer data" if a Casino is fouud outside of the original 12 facilities
            //So to fix this when it looks for casino facilities we return null for anything past the first 12 (it handles null)
            //There can be only one of the regular and high-end casino each so we should not have any issues

            //If saving vanilla data
            if (patcher_casino_vanilla_save)
            {
                //Prep null
                Facility facility = (Facility)null;
                int facilityTypeID = (!is_upward) ? FacilityDataTable.GetFacilityTypeID("カジノ") : FacilityDataTable.GetFacilityTypeID("高級カジノ");

                //Loop all facilities in case there are less than 12
                for (int i = 0; i < GameMain.Instance.FacilityMgr.GetFacilityArray().Length; i++)
                {
                    if (i < 12)
                    {
                        Facility facilitySearch = GameMain.Instance.FacilityMgr.GetFacilityArray()[i];
                        if (!((UnityEngine.Object)facilitySearch == (UnityEngine.Object)null) && facilitySearch.param.typeID == facilityTypeID)
                        {
                            facility = facilitySearch;
                            break;
                        }
                    }
                }

                __result = facility;
            }
        }

        #endregion

        #endregion
    }
}
