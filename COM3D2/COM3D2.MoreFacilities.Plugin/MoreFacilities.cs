using BepInEx;
using BepInEx.Harmony;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MoreFacilities.Plugin
{
    [BepInPlugin("org.guest4168.plugins.morefacilitiesplugin", "More Facilities Plug-In", "0.0.0.1")]
    public class MoreFacilities : BaseUnityPlugin
    {
        #region Main Code

        //Constants that are adjustable in the future
        public const int MaxFacilities = 60;    //Maximum facilities to make available
        private const int scrollJump = 1;       //How many rows you move when scrolling

        //Replaced by Harmony Patch, but keeping for history
        public void OnLevelWasLoaded(int level)
        {
            //Main Day Level
            if (level == 3)
            {
                //UnityEngine.Debug.Log("MoreFacilities: OnLevelWasLoaded 3");
                //if (GameMain.Instance.FacilityMgr != null)
                //{
                //    typeof(FacilityManager).GetField("FacilityCountMax", BindingFlags.Instance | BindingFlags.Public).SetValue(GameMain.Instance.FacilityMgr, MaxFacilities);
                //}
            }
            //Facility Manager Level - might apply to Life Mode too, have not even looked at guest mode yet
            else if (level == 40)
            {
                //Maybe something in the future
            }
        }

        //Variables needed by Update -- scrollPosition tracking, and original position vector
        private int wfd_scrollPosition = 0;
        private UnityEngine.Vector3 wfd_originalPosition = new UnityEngine.Vector3(-9001f, -9001f, -9001f);

        private int wfl_scrollPosition = 0;
        private UnityEngine.Vector3 wfl_originalPosition = new UnityEngine.Vector3(-9001f, -9001f, -9001f);

        private void Update()
        {
            //Check that the Main Window is loaded, this can be inactive while grid can be active, creates problems when using other tabs on facility page
            UnityEngine.GameObject windowFacilityDetails = UnityEngine.GameObject.Find("Window Facility Details");
            if (windowFacilityDetails == null || windowFacilityDetails.activeInHierarchy == false)
            {
                //Reset
                wfd_scrollPosition = -1;
            }
            else
            {
                //Check that the Facility Grid is loaded
                UnityEngine.GameObject listToFakeScroll = UnityEngine.GameObject.Find("Parent Facility Button");
                this.fakeScroll(listToFakeScroll, 0, ref wfd_scrollPosition, 4, 3, scrollJump, ref wfd_originalPosition, true);
            }

            //This is for Facility Refinement Maid Job
            UnityEngine.GameObject windowFacilityList = UnityEngine.GameObject.Find("Window Facility List");
            if (windowFacilityList != null)
            {
                //Update the actual container -- name of this was too generic so had to grab parent first
                UnityEngine.GameObject parentButton = windowFacilityList.transform.GetChild(1).gameObject;
                this.fakeScroll(parentButton, 1, ref wfl_scrollPosition, 12, 1, scrollJump, ref wfl_originalPosition, false);
            }

        }

        private void fakeScroll(UnityEngine.GameObject listToFakeScroll, int mode, ref int scrollPosition, int visibleRows, int numOfColums, int scrollRowJump, ref UnityEngine.Vector3 originalPosition, bool allowArrows)
        {
            //Check that the item you want to scroll exists
            if (listToFakeScroll != null)
            {
                int listCount = listToFakeScroll.transform.childCount - 1; //Theres usually a dummy item in these lists at the beginning
                int maxScrollPosition = (int)Math.Ceiling((decimal)((listCount - 1) / (numOfColums * scrollRowJump)));

                int scroll = 0;
                int oldScrollPosition = scrollPosition;

                //Had to cheat
                if (scrollPosition == -1)
                {
                    scrollPosition = 0;
                }

                //Capture Arrow Keys and Mouse Wheel Scroll
                if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") != 0f || (allowArrows && (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow))))
                {

                    //Scroll direction
                    scroll = (UnityEngine.Input.GetAxis("Mouse ScrollWheel") < 0f || UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) ? 1 : -1;

                    //Store the original position of the grid for when we leave and have to come back here
                    if (originalPosition.x == (-9001f))
                    {
                        originalPosition = new UnityEngine.Vector3(listToFakeScroll.transform.position.x,
                                                                   listToFakeScroll.transform.position.y,
                                                                   listToFakeScroll.transform.position.z);

                        //This is buggy bc of string length, especially if anyone tries to rename their facility, this kinda overwrites it as they type cause its in update and not gui
                        //for (int i = 1; i < listToFakeScroll.transform.childCount; i++)
                        //{
                        //    //Update the Text to show that there are now more facilities, hopefully people will try to scroll or use arrow keys when they see, otherwise need to add some kinda indicator
                        //    listToFakeScroll.transform.GetChild(i).gameObject.GetComponent<FacilityInfoUI>().textFacilityName.text += "   " + i + "/" + MaxFacilities;
                        //}
                    }
                }

                //Do not want to scroll too far
                scrollPosition += scroll;
                scrollPosition = Math.Max(scrollPosition, 0);
                scrollPosition = Math.Min(scrollPosition, maxScrollPosition);

                //Loop the children to set visibility, regardless of if scrollPosition changed bc have to hide extras when first initializing
                int minVisible = (scrollPosition * (numOfColums * scrollRowJump)) + 1;
                int maxVisible = minVisible + ((numOfColums * visibleRows) - 1);

                for (int i = 1; i < listToFakeScroll.transform.childCount; i++)
                {
                    if (minVisible <= i && i <= maxVisible)
                    {
                        //Positioning mode or activate mode
                        switch (mode)
                        {
                            case 0:
                                listToFakeScroll.transform.GetChild(i).gameObject.transform.localScale = new UnityEngine.Vector3(1f, 1f, 1f);
                                break;
                            case 1:
                                listToFakeScroll.transform.GetChild(i).gameObject.SetActive(true);
                                break;
                        }
                    }
                    else
                    {
                        //Positioning mode or activate mode
                        switch (mode)
                        {
                            case 0:
                                listToFakeScroll.transform.GetChild(i).gameObject.transform.localScale = new UnityEngine.Vector3(0f, 0f, 0f);
                                break;
                            case 1:
                                listToFakeScroll.transform.GetChild(i).gameObject.SetActive(false);
                                break;
                        }
                    }
                }

                if (mode == 0)
                {
                    //Now see if we actually changed, and move the container grid object
                    if (originalPosition.x != (-9001f) && scrollPosition != oldScrollPosition)
                    {
                        float diff = listToFakeScroll.transform.GetChild(1).transform.position.y - listToFakeScroll.transform.GetChild(1 + (numOfColums * scrollRowJump)).transform.position.y;
                        listToFakeScroll.transform.position = new UnityEngine.Vector3(originalPosition.x,
                                                                                      originalPosition.y + (scrollPosition * scrollRowJump * diff),
                                                                                      originalPosition.z);
                    }
                }
            }
            else
            {
                //Reset
                scrollPosition = -1;
            }
        }
        #endregion

        #region Harmony Patching
        private UnityEngine.GameObject managerObject;
        public void Awake()
        {
            UnityEngine.Debug.Log("MoreFacilities: Awake");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);
            this.managerObject = new UnityEngine.GameObject("moreFacilitiesManager");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this.managerObject);
            this.managerObject.AddComponent<MoreFacilitiesCore>().Initialize();
        }
        #endregion
    }

    public class MoreFacilitiesCore : MonoBehaviour
    {
        public bool Initialized { get; private set; }
        public void Initialize()
        {
            if (this.Initialized)
                return;
            MoreFacilitiesCoreHooks.Initialize();
            this.Initialized = true;
            UnityEngine.Debug.Log("MoreFacilities: Core Initialize");
        }

        public void Awake()
        {
            UnityEngine.Debug.Log("MoreFacilities: Core Awake");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);
        }
    }

    internal static class MoreFacilitiesCoreHooks
    {
        private static bool initialized;
        private static HarmonyLib.Harmony instance;

        public static void Initialize()
        {
            if (MoreFacilitiesCoreHooks.initialized)
                return;

            MoreFacilitiesCoreHooks.instance = HarmonyWrapper.PatchAll(typeof(MoreFacilitiesCoreHooks), "org.guest4168.morefacilitiesplugin.hooks.base");
            MoreFacilitiesCoreHooks.initialized = true;
            patcher_casino_vanilla_save = false;
            patcher_mod_save = false;
            patcher_mod_load = false;

            UnityEngine.Debug.Log("MoreFacilities: Core Hooks Initialize");
        }

        [HarmonyPatch(typeof(FacilityManager), nameof(FacilityManager.Init))]
        [HarmonyPrefix]
        static void Prefix(FacilityManager __instance)
        {
            UnityEngine.Debug.Log("MoreFacilities: FacilityManager Init Prefix");

            //Update private instance variable
            typeof(FacilityManager).GetField("FacilityCountMax", BindingFlags.Instance | BindingFlags.Public).SetValue(__instance, MoreFacilities.MaxFacilities);
        }


        #region Save/Load
        private static bool patcher_casino_vanilla_save;

        private static bool patcher_mod_save;
        private static bool patcher_mod_load;

        private static System.Collections.Generic.List<Facility> savedFacilities;

        #region GameMain

        #region Serialize
        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Serialize), new Type[] { typeof(int), typeof(string) })]
        [HarmonyPrefix]
        static void GameMain_Serialize_Prefix(int f_nSaveNo, string f_strComment, GameMain __instance)
        {
            if (f_nSaveNo != -1 && !patcher_mod_save)
            {
                UnityEngine.Debug.Log("MoreFacilities: GameMain Serialize Prefix");

                //Make a fake save file
                patcher_mod_save = true;
                __instance.Serialize(f_nSaveNo, f_strComment);
                patcher_mod_save = false;

                //Prep Facility Data
                //for (int i = 0; i < __instance.FacilityMgr.GetFacilityArray().Length; i++)
                //{
                //    if (i > 11 && __instance.FacilityMgr.GetFacilityExist(i))
                //    {
                //        __instance.FacilityMgr.DestroyFacility(i);
                //    }
                //}
                savedFacilities = new System.Collections.Generic.List<Facility>();
                savedFacilities.AddRange(((System.Collections.Generic.List<Facility>)typeof(FacilityManager).GetField("m_FacilityArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance.FacilityMgr)).GetRange(12, __instance.FacilityMgr.GetFacilityArray().Length - 12));
                ((System.Collections.Generic.List<Facility>)typeof(FacilityManager).GetField("m_FacilityArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance.FacilityMgr)).RemoveRange(12, __instance.FacilityMgr.GetFacilityArray().Length - 12);

                //Prep Casino Data
                patcher_casino_vanilla_save = true;
            }
        }

        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Serialize), new Type[] { typeof(int), typeof(string) })]
        [HarmonyPostfix]
        static void GameMain_Serialize_Postfix(int f_nSaveNo, string f_strComment, GameMain __instance)
        {
            if (f_nSaveNo != -1 && !patcher_mod_save)
            {
                UnityEngine.Debug.Log("MoreFacilities: GameMain Serialize Postfix");

                //Return to modded functionality
                patcher_casino_vanilla_save = false;

                //Deserialize the fake save data after finished saving vanilla data to restore
                //__instance.Deserialize(f_nSaveNo, true);
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
            if (f_nSaveNo != -1)
            {
                UnityEngine.Debug.Log("MoreFacilities: GameMain Deserialize Prefix");

                //We want to use the fake save
                patcher_mod_load = true;
            }
        }

        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Deserialize))]//, new Type[] { typeof(int), typeof(bool) })]
        [HarmonyPostfix]
        static void GameMain_Deserialize_Postfix(int f_nSaveNo, GameMain __instance)
        {
            if (f_nSaveNo != -1)
            {
                UnityEngine.Debug.Log("MoreFacilities: GameMain Deserialize Postfix");

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

            //string[] paths = new string[] { "Facility", "Casino" };
            string[] paths = new string[] { "MoreFacilities" };

            //Delete the extra save file data
            for (int i = 0; i < paths.Length; i++)
            {
                string path = UTY.gameProjectPath + "\\" + "SaveData";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                path = path + "/" + string.Format("SaveData{0:D3}_" + paths[i], (f_nSaveNo)) + ".save";

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }
        #endregion

        [HarmonyPatch(typeof(GameMain), nameof(GameMain.MakeSavePathFileName), new Type[] { typeof(int) })]
        [HarmonyPostfix]
        static void GameMain_MakeSavePathFileName(int f_nSaveNo, ref String __result)
        {
            if (patcher_mod_save || patcher_mod_load)
            {
                string path = UTY.gameProjectPath + "\\" + "SaveData";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);
                __result = path + "/" + string.Format("SaveData{0:D3}_MoreFacilities", (object)f_nSaveNo) + ".save";

                //If performing a load, if this is first time a save was loaded with this mod, they wont have the fake save file
                if (patcher_mod_load)
                {
                    if (!System.IO.File.Exists(__result))
                    {
                        path = UTY.gameProjectPath + "\\" + "SaveData";
                        __result = path + "/" + string.Format("SaveData{0:D3}", (object)f_nSaveNo) + ".save";
                    }
                }
            }
        }

        #endregion

        #region CasinoDataMgr

        [HarmonyPatch(typeof(CasinoDataMgr), "ReadDealerData")]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
        [HarmonyPrefix]
        static void CasinoDataMgr_ReadDealerData_Prefix(System.IO.BinaryReader br, bool is_upward, CasinoDataMgr __instance, ref System.Collections.Generic.Dictionary<Facility, global::DealerMaid> ___m_FacilityDealerPair)
        {
            UnityEngine.Debug.Log(String.Format("MoreFacilities: CasinoDataMgr ReadDealerData Prefix "));
        }

        [HarmonyPatch(typeof(CasinoDataMgr), "ReadDealerData")]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
        [HarmonyPostfix]
        static void CasinoDataMgr_ReadDealerData_Postfix(System.IO.BinaryReader br, bool is_upward, CasinoDataMgr __instance, ref System.Collections.Generic.Dictionary<Facility, global::DealerMaid> ___m_FacilityDealerPair)
        {
            UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr ReadDealerData Postfix");
        }




        [HarmonyPatch(typeof(CasinoDataMgr), nameof(CasinoDataMgr.GetCasinoFacility))]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
        [HarmonyPostfix]
        static void CasinoDataMgr_GetCasinoFacility_Postfix(ref Facility __result, bool is_upward = false)
        {
            if (patcher_casino_vanilla_save)
            {
                Facility facility = (Facility)null;
                int facilityTypeID = (!is_upward) ? FacilityDataTable.GetFacilityTypeID("カジノ") : FacilityDataTable.GetFacilityTypeID("高級カジノ");

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
