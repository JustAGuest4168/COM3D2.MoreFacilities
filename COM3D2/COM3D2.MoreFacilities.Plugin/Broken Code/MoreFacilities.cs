//using BepInEx;
//using BepInEx.Harmony;
//using HarmonyLib;
//using System;
//using System.Reflection;
//using UnityEngine;

//namespace COM3D2.MoreFacilities.Plugin
//{
//    [BepInPlugin("org.guest4168.plugins.morefacilitiesplugin", "More Facilities Plug-In", "0.0.0.1")]
//    public class MoreFacilities : BaseUnityPlugin
//    {
//        #region Main Code

//        //Constants that are adjustable in the future
//        public const int MaxFacilities = 60;    //Maximum facilities to make available
//        private const int scrollJump = 1;       //How many rows you move when scrolling

//        //Replaced by Harmony Patch, but keeping for history
//        public void OnLevelWasLoaded(int level)
//        {
//            //Main Day Level
//            if (level == 3)
//            {
//                //UnityEngine.Debug.Log("MoreFacilities: OnLevelWasLoaded 3");
//                //if (GameMain.Instance.FacilityMgr != null)
//                //{
//                //    typeof(FacilityManager).GetField("FacilityCountMax", BindingFlags.Instance | BindingFlags.Public).SetValue(GameMain.Instance.FacilityMgr, MaxFacilities);
//                //}
//            }
//            //Facility Manager Level - might apply to Life Mode too, have not even looked at guest mode yet
//            else if (level == 40)
//            {
//                //Maybe something in the future
//            }
//        }

//        //Variables needed by Update -- scrollPosition tracking, and original position vector
//        private int wfd_scrollPosition = 0;
//        private UnityEngine.Vector3 wfd_originalPosition = new UnityEngine.Vector3(-9001f, -9001f, -9001f);

//        private int wfl_scrollPosition = 0;
//        private UnityEngine.Vector3 wfl_originalPosition = new UnityEngine.Vector3(-9001f, -9001f, -9001f);

//        private void Update()
//        {
//            //Check that the Main Window is loaded, this can be inactive while grid can be active, creates problems when using other tabs on facility page
//            UnityEngine.GameObject windowFacilityDetails = UnityEngine.GameObject.Find("Window Facility Details");
//            if (windowFacilityDetails == null || windowFacilityDetails.activeInHierarchy == false)
//            {
//                //Reset
//                wfd_scrollPosition = -1;
//            }
//            else
//            {
//                //Check that the Facility Grid is loaded
//                UnityEngine.GameObject listToFakeScroll = UnityEngine.GameObject.Find("Parent Facility Button");
//                this.fakeScroll(listToFakeScroll, 0, ref wfd_scrollPosition, 4, 3, scrollJump, ref wfd_originalPosition, true);
//            }

//            //This is for Facility Refinement Maid Job
//            UnityEngine.GameObject windowFacilityList = UnityEngine.GameObject.Find("Window Facility List");
//            if (windowFacilityList != null)
//            {
//                //Update the actual container -- name of this was too generic so had to grab parent first
//                UnityEngine.GameObject parentButton = windowFacilityList.transform.GetChild(1).gameObject;
//                this.fakeScroll(parentButton, 1, ref wfl_scrollPosition, 12, 1, scrollJump, ref wfl_originalPosition, false);
//            }

//        }

//        private void fakeScroll(UnityEngine.GameObject listToFakeScroll, int mode, ref int scrollPosition, int visibleRows, int numOfColums, int scrollRowJump, ref UnityEngine.Vector3 originalPosition, bool allowArrows)
//        {
//            //Check that the item you want to scroll exists
//            if (listToFakeScroll != null)
//            {
//                int listCount = listToFakeScroll.transform.childCount - 1; //Theres usually a dummy item in these lists at the beginning
//                int maxScrollPosition = (int)Math.Ceiling((decimal)((listCount - 1) / (numOfColums * scrollRowJump)));

//                int scroll = 0;
//                int oldScrollPosition = scrollPosition;

//                //Had to cheat
//                if (scrollPosition == -1)
//                {
//                    scrollPosition = 0;
//                }

//                //Capture Arrow Keys and Mouse Wheel Scroll
//                if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") != 0f || (allowArrows && (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.UpArrow))))
//                {

//                    //Scroll direction
//                    scroll = (UnityEngine.Input.GetAxis("Mouse ScrollWheel") < 0f || UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.DownArrow)) ? 1 : -1;

//                    //Store the original position of the grid for when we leave and have to come back here
//                    if (originalPosition.x == (-9001f))
//                    {
//                        originalPosition = new UnityEngine.Vector3(listToFakeScroll.transform.position.x,
//                                                                   listToFakeScroll.transform.position.y,
//                                                                   listToFakeScroll.transform.position.z);

//                        //This is buggy bc of string length, especially if anyone tries to rename their facility, this kinda overwrites it as they type cause its in update and not gui
//                        //for (int i = 1; i < listToFakeScroll.transform.childCount; i++)
//                        //{
//                        //    //Update the Text to show that there are now more facilities, hopefully people will try to scroll or use arrow keys when they see, otherwise need to add some kinda indicator
//                        //    listToFakeScroll.transform.GetChild(i).gameObject.GetComponent<FacilityInfoUI>().textFacilityName.text += "   " + i + "/" + MaxFacilities;
//                        //}
//                    }
//                }

//                //Do not want to scroll too far
//                scrollPosition += scroll;
//                scrollPosition = Math.Max(scrollPosition, 0);
//                scrollPosition = Math.Min(scrollPosition, maxScrollPosition);

//                //Loop the children to set visibility, regardless of if scrollPosition changed bc have to hide extras when first initializing
//                int minVisible = (scrollPosition * (numOfColums * scrollRowJump)) + 1;
//                int maxVisible = minVisible + ((numOfColums * visibleRows) - 1);

//                for (int i = 1; i < listToFakeScroll.transform.childCount; i++)
//                {
//                    if (minVisible <= i && i <= maxVisible)
//                    {
//                        //Positioning mode or activate mode
//                        switch (mode)
//                        {
//                            case 0:
//                                listToFakeScroll.transform.GetChild(i).gameObject.transform.localScale = new UnityEngine.Vector3(1f, 1f, 1f);
//                                break;
//                            case 1:
//                                listToFakeScroll.transform.GetChild(i).gameObject.SetActive(true);
//                                break;
//                        }
//                    }
//                    else
//                    {
//                        //Positioning mode or activate mode
//                        switch (mode)
//                        {
//                            case 0:
//                                listToFakeScroll.transform.GetChild(i).gameObject.transform.localScale = new UnityEngine.Vector3(0f, 0f, 0f);
//                                break;
//                            case 1:
//                                listToFakeScroll.transform.GetChild(i).gameObject.SetActive(false);
//                                break;
//                        }
//                    }
//                }

//                if (mode == 0)
//                {
//                    //Now see if we actually changed, and move the container grid object
//                    if (originalPosition.x != (-9001f) && scrollPosition != oldScrollPosition)
//                    {
//                        float diff = listToFakeScroll.transform.GetChild(1).transform.position.y - listToFakeScroll.transform.GetChild(1 + (numOfColums * scrollRowJump)).transform.position.y;
//                        listToFakeScroll.transform.position = new UnityEngine.Vector3(originalPosition.x,
//                                                                                      originalPosition.y + (scrollPosition * scrollRowJump * diff),
//                                                                                      originalPosition.z);
//                    }
//                }
//            }
//            else
//            {
//                //Reset
//                scrollPosition = -1;
//            }
//        }
//        #endregion

//        #region Harmony Patching
//        private UnityEngine.GameObject managerObject;
//        public void Awake()
//        {
//            UnityEngine.Debug.Log("MoreFacilities: Awake");
//            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);
//            this.managerObject = new UnityEngine.GameObject("moreFacilitiesManager");
//            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this.managerObject);
//            this.managerObject.AddComponent<MoreFacilitiesCore>().Initialize();
//        }
//        #endregion
//    }

//    public class MoreFacilitiesCore : MonoBehaviour
//    {
//        public bool Initialized { get; private set; }
//        public void Initialize()
//        {
//            if (this.Initialized)
//                return;
//            MoreFacilitiesCoreHooks.Initialize();
//            this.Initialized = true;
//            UnityEngine.Debug.Log("MoreFacilities: Core Initialize");
//        }

//        public void Awake()
//        {
//            UnityEngine.Debug.Log("MoreFacilities: Core Awake");
//            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);
//        }
//    }

//    internal static class MoreFacilitiesCoreHooks
//    {
//        private static bool initialized;
//        private static HarmonyLib.Harmony instance;

//        public static void Initialize()
//        {
//            if (MoreFacilitiesCoreHooks.initialized)
//                return;

//            MoreFacilitiesCoreHooks.instance = HarmonyWrapper.PatchAll(typeof(MoreFacilitiesCoreHooks), "org.guest4168.morefacilitiesplugin.hooks.base");
//            MoreFacilitiesCoreHooks.initialized = true;
//            MoreFacilitiesCoreHooks.patcher_facility_f_nSaveNo = -1;
//            MoreFacilitiesCoreHooks.patcher_casino_f_nSaveNo = -1;

//            UnityEngine.Debug.Log("MoreFacilities: Core Hooks Initialize");
//        }

//        [HarmonyPatch(typeof(FacilityManager), nameof(FacilityManager.Init))]
//        [HarmonyPrefix]
//        static void Prefix(FacilityManager __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: FacilityManager Init Prefix");

//            //Update private instance variable
//            typeof(FacilityManager).GetField("FacilityCountMax", BindingFlags.Instance | BindingFlags.Public).SetValue(__instance, MoreFacilities.MaxFacilities);
//        }


//        #region Save/Load
//        private const int KISS_save_version = 1450;
//        private static int patcher_facility_f_nSaveNo;
//        private static int patcher_casino_f_nSaveNo;
//        private static bool patcher_casino_vanilla_save;

//        private static System.Collections.Generic.List<Facility> savedFacilities;

//        #region GameMain

//        #region Serialize
//        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Serialize), new Type[] { typeof(int), typeof(string) })]
//        [HarmonyPrefix]
//        static void GameMain_Serialize_Prefix(int f_nSaveNo, string f_strComment, GameMain __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: GameMain Serialize Prefix");

//            //Before we save, we want to get the save file number for the facility data pre-serialize
//            patcher_facility_f_nSaveNo = f_nSaveNo;
//            patcher_casino_f_nSaveNo = f_nSaveNo;
//            patcher_casino_vanilla_save = false;
//        }

//        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Serialize), new Type[] { typeof(int), typeof(string) })]
//        [HarmonyPostfix]
//        static void GameMain_Serialize_Postfix(int f_nSaveNo, string f_strComment, GameMain __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: GameMain Serialize Postfix");

//            //Before we save, we want to get the save file number for the facility data pre-serialize
//            patcher_facility_f_nSaveNo = -1;
//            patcher_casino_f_nSaveNo = -1;
//        }

//        #endregion

//        #region Deserialize
//        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Deserialize))]//, new Type[] { typeof(int), typeof(bool) })]
//        [HarmonyPrefix]
//        static void GameMain_Deserialize_Prefix(int f_nSaveNo, GameMain __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: GameMain Deserialize Prefix");

//            //Before we save, we want to get the save file number for the facility data pre-serialize
//            patcher_facility_f_nSaveNo = f_nSaveNo;
//            patcher_casino_f_nSaveNo = f_nSaveNo;
//        }

//        [HarmonyPatch(typeof(GameMain), nameof(GameMain.Deserialize))]//, new Type[] { typeof(int), typeof(bool) })]
//        [HarmonyPostfix]
//        static void GameMain_Deserialize_Postfix(int f_nSaveNo, GameMain __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: GameMain Deserialize Postfix");

//            //Before we save, we want to get the save file number for the facility data pre-serialize
//            patcher_facility_f_nSaveNo = -1;
//            patcher_casino_f_nSaveNo = -1;
//        }
//        #endregion

//        #region Delete
//        [HarmonyPatch(typeof(GameMain), nameof(GameMain.DeleteSerializeData), new Type[] { typeof(int) })]
//        [HarmonyPostfix]
//        static void GameMain_DeleteSerializeData_Postfix(int f_nSaveNo, GameMain __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: GameMain DeleteSerializeData Postfix");

//            string[] paths = new string[] { "Facility", "Casino" };

//            //Delete the extra save file data
//            for (int i = 0; i < paths.Length; i++)
//            {
//                string path = UTY.gameProjectPath + "\\" + "SaveData";
//                if (!System.IO.Directory.Exists(path))
//                    System.IO.Directory.CreateDirectory(path);

//                path = path + "/" + string.Format("SaveData{0:D3}_" + paths[i], (f_nSaveNo)) + ".save";

//                if (System.IO.File.Exists(path))
//                {
//                    System.IO.File.Delete(path);
//                }
//            }
//        }
//        #endregion

//        #endregion

//        #region FacilityManager
//        [HarmonyPatch(typeof(FacilityManager), nameof(FacilityManager.Serialize), new Type[] { typeof(System.IO.BinaryWriter) })]
//        [HarmonyPrefix]
//        static void FacilityManager_Serialize_Prefix(System.IO.BinaryWriter brWrite, FacilityManager __instance, ref System.Collections.Generic.List<Facility> ___m_FacilityArray)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: FacilityManager Serialize Prefix");

//            if (patcher_facility_f_nSaveNo != -1)
//            {
//                //Create a modded save file that will hold just the facility data
//                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
//                System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter((System.IO.Stream)memoryStream);
//                string path = UTY.gameProjectPath + "\\" + "SaveData";
//                if (!System.IO.Directory.Exists(path))
//                    System.IO.Directory.CreateDirectory(path);
//                path = path + "/" + string.Format("SaveData{0}_Facility", (patcher_facility_f_nSaveNo)) + ".save";

//                //Save number
//                int temp = patcher_facility_f_nSaveNo;

//                //Perform regular Serialize against our fake binarywriter
//                patcher_facility_f_nSaveNo = -1;
//                __instance.Serialize(binaryWriter);

//                //Save to file
//                System.IO.File.WriteAllBytes(path, memoryStream.ToArray());
//                memoryStream.Close();
//                memoryStream.Dispose();

//                //Restore number
//                patcher_facility_f_nSaveNo = temp;

//                ////Fix the data for basic save
//                //for (int i = 0; i < __instance.GetFacilityArray().Length; i++)
//                //{
//                //    if (i > 11 && __instance.GetFacilityExist(i))
//                //    {
//                //        __instance.DestroyFacility(i);
//                //    }
//                //}

//                savedFacilities = new System.Collections.Generic.List<Facility>();
//                savedFacilities.AddRange(savedFacilities.GetRange(12, __instance.GetFacilityArray().Length - 12));
//                ___m_FacilityArray.RemoveRange(12, __instance.GetFacilityArray().Length - 12);
//            }
//        }

//        [HarmonyPatch(typeof(FacilityManager), nameof(FacilityManager.Serialize), new Type[] { typeof(System.IO.BinaryWriter) })]
//        [HarmonyPostfix]
//        static void FacilityManager_Serialize_Postfix(System.IO.BinaryWriter brWrite, FacilityManager __instance, ref System.Collections.Generic.List<Facility> ___m_FacilityArray)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: FacilityManager Serialize Postfix");

//            if (patcher_facility_f_nSaveNo != -1)
//            {
//                //After we save the basic data, restore the modified data
//                //fakeDeserialize_Facility(__instance);
//                ___m_FacilityArray.AddRange(savedFacilities);
//                savedFacilities = new System.Collections.Generic.List<Facility>();

//                //Cleanup
//                patcher_facility_f_nSaveNo = -1;
//            }
//        }

//        [HarmonyPatch(typeof(FacilityManager), nameof(FacilityManager.Deserialize), new Type[] { typeof(System.IO.BinaryReader) })]
//        [HarmonyPostfix]
//        static void FacilityManager_Deserialize_Postfix(System.IO.BinaryReader brRead, FacilityManager __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: FacilityManager Deserialize Postfix");

//            //Check that this isnt the Serialize Postfix
//            if (patcher_facility_f_nSaveNo != -1)
//            {
//                //Load the fake save data if it exists
//                fakeDeserialize_Facility(__instance);

//                //Cleanup
//                patcher_facility_f_nSaveNo = -1;
//            }
//        }

//        #endregion

//        #region CasinoDataMgr
//        //[HarmonyPatch(typeof(CasinoDataMgr), nameof(CasinoDataMgr.Serialize))]//, new Type[] { typeof(System.IO.BinaryWriter) })]
//        //[HarmonyPrefix]
//        //static void CasinoDataMgr_Serialize_Prefix(System.IO.BinaryWriter bw, CasinoDataMgr __instance, ref System.Collections.Generic.Dictionary<Facility, global::DealerMaid> ___m_FacilityDealerPair)
//        //{
//        //    UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix");

//        //    if (patcher_casino_f_nSaveNo != -1)
//        //    {
//        //        //Create a modded save file that will hold just the casino data
//        //        System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
//        //        System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter((System.IO.Stream)memoryStream);
//        //        string path = UTY.gameProjectPath + "\\" + "SaveData";
//        //        if (!System.IO.Directory.Exists(path))
//        //            System.IO.Directory.CreateDirectory(path);
//        //        path = path + "/" + string.Format("SaveData{0}_Casino", (patcher_casino_f_nSaveNo)) + ".save";

//        //        //Save number
//        //        int temp = patcher_casino_f_nSaveNo;

//        //        //Keep collection of keys because serializing adds to the Dictionary for some reason
//        //        System.Collections.Generic.List<Facility> m_FacilityDealerPairKeys = new System.Collections.Generic.List<Facility>();
//        //        foreach (System.Collections.Generic.KeyValuePair<Facility, global::DealerMaid> kvp in ___m_FacilityDealerPair)
//        //        {
//        //            m_FacilityDealerPairKeys.Add(kvp.Key);
//        //        }

//        //        //Perform regular Serialize against our fake binarywriter
//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Vanilla Dealer Z " + ___m_FacilityDealerPair.Keys.Count);

//        //        patcher_casino_f_nSaveNo = -1;
//        //        __instance.Serialize(binaryWriter);

//        //        //Save to file
//        //        System.IO.File.WriteAllBytes(path, memoryStream.ToArray());
//        //        memoryStream.Close();
//        //        memoryStream.Dispose();

//        //        //Deserialize to fix serialize breaking DealerPair
//        //        int save_version = getFilesSaveVersion(temp);
//        //        fakeDeserialize_CasinoData(__instance, save_version);

//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Vanilla Dealer Y " + ___m_FacilityDealerPair.Keys.Count);

//        //        //Restore number
//        //        patcher_casino_f_nSaveNo = temp;

//        //        //Remove the duplicate keys
//        //        System.Collections.Generic.List<Facility> removableKeys = new System.Collections.Generic.List<Facility>();
//        //        foreach (Facility key in ___m_FacilityDealerPair.Keys)
//        //        {
//        //            if (!m_FacilityDealerPairKeys.Contains(key))
//        //            {
//        //                removableKeys.Add(key);
//        //            }
//        //        }
//        //        foreach (Facility key in removableKeys)
//        //        {
//        //            ___m_FacilityDealerPair.Remove(key);
//        //        }

//        //        //Fix the data for basic save
//        //        //System.Collections.Generic.Dictionary<Facility, global::DealerMaid> my_m_FacilityDealerPair = (System.Collections.Generic.Dictionary<Facility, global::DealerMaid>)typeof(CasinoDataMgr).GetField("m_FacilityDealerPair", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(CasinoDataMgr.Instance);

//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Vanilla Dealer A " + ___m_FacilityDealerPair.Keys.Count);
//        //        //Fix dealer data
//        //        for (int i = 0; i < GameMain.Instance.FacilityMgr.GetFacilityArray().Length; i++)
//        //        {
//        //            if (i > 11 && GameMain.Instance.FacilityMgr.GetFacilityExist(i))
//        //            {
//        //                Facility facility = GameMain.Instance.FacilityMgr.GetFacility(i);

//        //                System.Collections.Generic.List<Facility> removeFac = new System.Collections.Generic.List<Facility>();
//        //                foreach(Facility key in ___m_FacilityDealerPair.Keys)
//        //                {
//        //                    if (key.guid.Equals(facility.guid))
//        //                    {
//        //                        removeFac.Add(key);
//        //                    }
//        //                }
//        //                foreach(Facility key in removeFac)
//        //                {
//        //                    UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Found Facility");
//        //                    ___m_FacilityDealerPair.Remove(key);
//        //                }
//        //            }
//        //        }
//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Vanilla Dealer B " + ___m_FacilityDealerPair.Keys.Count);

//        //        //typeof(CasinoDataMgr).GetField("m_FacilityDealerPair", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(CasinoDataMgr.Instance, my_m_FacilityDealerPair);

//        //        //Fix the Facility Data because CasinoDataMgr will try to re-add to the DealerPair
//        //        for (int i = 0; i < GameMain.Instance.FacilityMgr.GetFacilityArray().Length; i++)
//        //        {
//        //            if (i > 11 && GameMain.Instance.FacilityMgr.GetFacilityExist(i))
//        //            {
//        //                GameMain.Instance.FacilityMgr.DestroyFacility(i);
//        //            }
//        //        }
//        //        System.Collections.Generic.List<Facility> m_FacilityArray = ((System.Collections.Generic.List<Facility>)typeof(FacilityManager).GetField("m_FacilityArray", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GameMain.Instance.FacilityMgr));
//        //        m_FacilityArray.RemoveRange(12, m_FacilityArray.Count - 12);

//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Vanilla Fac " + GameMain.Instance.FacilityMgr.GetFacilityArray().Length);
//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Vanilla Dealer C " + ___m_FacilityDealerPair.Keys.Count);

//        //        patcher_casino_vanilla_save = true;
//        //    }
//        //    else
//        //    {
//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Mod Fac " + GameMain.Instance.FacilityMgr.GetFacilityArray().Length);
//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Mod Dealer " + ___m_FacilityDealerPair.Keys.Count);
//        //    }
//        //}

//        [HarmonyPatch(typeof(CasinoDataMgr), nameof(CasinoDataMgr.Serialize))]//, new Type[] { typeof(System.IO.BinaryWriter) })]
//        [HarmonyPrefix]
//        static void CasinoDataMgr_Serialize_Prefix(System.IO.BinaryWriter bw, CasinoDataMgr __instance, ref System.Collections.Generic.Dictionary<Facility, global::DealerMaid> ___m_FacilityDealerPair)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix");

//            if (patcher_casino_f_nSaveNo != -1)
//            {
//                //Perform regular Serialize against our fake binarywriter
//                ___m_FacilityDealerPair.Clear();

//                //Create a modded save file that will hold just the casino data
//                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
//                System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter((System.IO.Stream)memoryStream);
//                string path = UTY.gameProjectPath + "\\" + "SaveData";
//                if (!System.IO.Directory.Exists(path))
//                    System.IO.Directory.CreateDirectory(path);
//                path = path + "/" + string.Format("SaveData{0}_Casino", (patcher_casino_f_nSaveNo)) + ".save";

//                //Save number
//                int temp = patcher_casino_f_nSaveNo;
//                patcher_casino_f_nSaveNo = -1;
//                __instance.Serialize(binaryWriter);
//                patcher_casino_f_nSaveNo = temp;

//                //Save to file
//                System.IO.File.WriteAllBytes(path, memoryStream.ToArray());
//                memoryStream.Close();
//                memoryStream.Dispose();

//                //Prep for vanilla
//                patcher_casino_vanilla_save = true;
//                ___m_FacilityDealerPair.Clear();
//            }
//            else
//            {
//                UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Mod Fac " + GameMain.Instance.FacilityMgr.GetFacilityArray().Length);
//                UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Prefix Mod Dealer " + ___m_FacilityDealerPair.Keys.Count);
//            }
//        }


//        //[HarmonyPatch(typeof(CasinoDataMgr), nameof(CasinoDataMgr.Serialize))]//, new Type[] { typeof(System.IO.BinaryWriter) })]
//        //[HarmonyPostfix]
//        //static void CasinoDataMgr_Serialize_Postfix(System.IO.BinaryWriter bw, CasinoDataMgr __instance)
//        //{
//        //    UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Postfix");

//        //    if (patcher_casino_f_nSaveNo != -1)
//        //    {
//        //        //After we save the basic data, restore the modified data

//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Postfix Vanilla Dealer A " + ((System.Collections.Generic.Dictionary<Facility, global::DealerMaid>)typeof(CasinoDataMgr).GetField("m_FacilityDealerPair", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).Keys.Count);
//        //        //Fix Facility Data
//        //        patcher_facility_f_nSaveNo  = patcher_casino_f_nSaveNo;
//        //        fakeDeserialize_Facility(GameMain.Instance.FacilityMgr);
//        //        patcher_facility_f_nSaveNo = -1;
//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Postfix Vanilla Dealer B " + ((System.Collections.Generic.Dictionary<Facility, global::DealerMaid>)typeof(CasinoDataMgr).GetField("m_FacilityDealerPair", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).Keys.Count);

//        //        //Need save version for deserialization, try to get from file
//        //        int save_version = getFilesSaveVersion(patcher_casino_f_nSaveNo);

//        //        //fake deserialize
//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Postfix Vanilla Dealer C " + ((System.Collections.Generic.Dictionary<Facility, global::DealerMaid>)typeof(CasinoDataMgr).GetField("m_FacilityDealerPair", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).Keys.Count);
//        //        fakeDeserialize_CasinoData(__instance, save_version);
//        //        UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Postfix Vanilla Dealer D " + ((System.Collections.Generic.Dictionary<Facility, global::DealerMaid>)typeof(CasinoDataMgr).GetField("m_FacilityDealerPair", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).Keys.Count);

//        //        //Cleanup
//        //        patcher_casino_f_nSaveNo = -1;
//        //    }

//        //    if(patcher_casino_vanilla_save)
//        //    {
//        //        patcher_casino_vanilla_save = false;
//        //    }
//        //}

//        [HarmonyPatch(typeof(CasinoDataMgr), nameof(CasinoDataMgr.Serialize))]//, new Type[] { typeof(System.IO.BinaryWriter) })]
//        [HarmonyPostfix]
//        static void CasinoDataMgr_Serialize_Postfix(System.IO.BinaryWriter bw, CasinoDataMgr __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Postfix");

//            if (patcher_casino_f_nSaveNo != -1)
//            {
//                //After we save the basic data, restore the modified data
//                patcher_casino_vanilla_save = false;

//                //Need save version for deserialization, try to get from file
//                //int save_version = getFilesSaveVersion(patcher_casino_f_nSaveNo);

//                //fake deserialize
//                //UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Postfix Vanilla Dealer C " + ((System.Collections.Generic.Dictionary<Facility, global::DealerMaid>)typeof(CasinoDataMgr).GetField("m_FacilityDealerPair", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).Keys.Count);
//                //fakeDeserialize_CasinoData(__instance, save_version);
//                //UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Serialize Postfix Vanilla Dealer D " + ((System.Collections.Generic.Dictionary<Facility, global::DealerMaid>)typeof(CasinoDataMgr).GetField("m_FacilityDealerPair", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance)).Keys.Count);

//                //Cleanup
//                patcher_casino_f_nSaveNo = -1;
//            }
//        }



//        [HarmonyPatch(typeof(CasinoDataMgr), nameof(CasinoDataMgr.Deserialize))]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
//        [HarmonyPrefix]
//        static void CasinoDataMgr_Deserialize_Prefix(System.IO.BinaryReader br, int save_version, CasinoDataMgr __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Deserialize Prefix");

//            //Check that this isnt the Serialize Postfix
//            if (patcher_casino_f_nSaveNo != -1)
//            {
//                //int temp = patcher_casino_f_nSaveNo;
//                //patcher_casino_vanilla_save = false;

//                ////Load the fake save data if it exists
//                //fakeDeserialize_CasinoData(__instance, save_version);
//                //patcher_casino_f_nSaveNo = temp;

//                patcher_casino_vanilla_save = true;
//            }
//        }

//        [HarmonyPatch(typeof(CasinoDataMgr), nameof(CasinoDataMgr.Deserialize))]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
//        [HarmonyPostfix]
//        static void CasinoDataMgr_Deserialize_Postfix(System.IO.BinaryReader br, int save_version, CasinoDataMgr __instance)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr Deserialize Postfix");

//            //Check that this isnt the Serialize Postfix
//            if (patcher_casino_f_nSaveNo != -1)
//            {
//                //After we save the basic data, restore the modified data
//                patcher_casino_vanilla_save = false;

//                //Load the fake save data if it exists
//                fakeDeserialize_CasinoData(__instance, save_version);

//                //Cleanup
//                patcher_casino_f_nSaveNo = -1;
//            }
//        }




//        [HarmonyPatch(typeof(CasinoDataMgr), "ReadDealerData")]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
//        [HarmonyPrefix]
//        static void CasinoDataMgr_ReadDealerData_Prefix(System.IO.BinaryReader br, bool is_upward, CasinoDataMgr __instance, ref System.Collections.Generic.Dictionary<Facility, global::DealerMaid> ___m_FacilityDealerPair)
//        {
//            UnityEngine.Debug.Log(String.Format("MoreFacilities: CasinoDataMgr ReadDealerData Prefix "));

//            //Facility casinoFacility = __instance.GetCasinoFacility(is_upward);
//            //if (!((UnityEngine.Object)casinoFacility != (UnityEngine.Object)null))
//            //    return;
//            //___m_FacilityDealerPair.Add(casinoFacility, new global::DealerMaid());
//            //___m_FacilityDealerPair[casinoFacility].Deserialize(br);
//        }

//        [HarmonyPatch(typeof(CasinoDataMgr), "ReadDealerData")]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
//        [HarmonyPostfix]
//        static void CasinoDataMgr_ReadDealerData_Postfix(System.IO.BinaryReader br, bool is_upward, CasinoDataMgr __instance, ref System.Collections.Generic.Dictionary<Facility, global::DealerMaid> ___m_FacilityDealerPair)
//        {
//            UnityEngine.Debug.Log("MoreFacilities: CasinoDataMgr ReadDealerData Postfix");

//            //Facility casinoFacility = __instance.GetCasinoFacility(is_upward);
//            //if (!((UnityEngine.Object)casinoFacility != (UnityEngine.Object)null))
//            //    return;
//            //___m_FacilityDealerPair.Add(casinoFacility, new global::DealerMaid());
//            //___m_FacilityDealerPair[casinoFacility].Deserialize(br);
//        }




//        [HarmonyPatch(typeof(CasinoDataMgr), nameof(CasinoDataMgr.GetCasinoFacility))]//, new Type[] { typeof(System.IO.BinaryReader), typeof(int) })]
//        [HarmonyPostfix]
//        static void CasinoDataMgr_GetCasinoFacility_Postfix(ref Facility __result, bool is_upward = false)
//        {
//            if (patcher_casino_vanilla_save)
//            {
//                Facility facility = (Facility)null;
//                int facilityTypeID = (!is_upward) ? FacilityDataTable.GetFacilityTypeID("カジノ") : FacilityDataTable.GetFacilityTypeID("高級カジノ");

//                for (int i = 0; i < GameMain.Instance.FacilityMgr.GetFacilityArray().Length; i++)
//                {
//                    if (i < 12)
//                    {
//                        Facility facilitySearch = GameMain.Instance.FacilityMgr.GetFacilityArray()[i];
//                        if (!((UnityEngine.Object)facilitySearch == (UnityEngine.Object)null) && facilitySearch.param.typeID == facilityTypeID)
//                        {
//                            facility = facilitySearch;
//                            break;
//                        }
//                    }
//                }

//                __result = facility;
//            }
//        }

//        #endregion

//        //Helper
//        private static void fakeDeserialize_Facility(FacilityManager manager)
//        {
//            string path = UTY.gameProjectPath + "\\" + "SaveData";
//            if (!System.IO.Directory.Exists(path))
//                System.IO.Directory.CreateDirectory(path);
//            path = path + "/" + string.Format("SaveData{0}_Facility", (patcher_facility_f_nSaveNo)) + ".save";

//            if (System.IO.File.Exists(path))
//            {
//                System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open);
//                if (fileStream != null)
//                {
//                    byte[] buffer = new byte[fileStream.Length];
//                    fileStream.Read(buffer, 0, (int)fileStream.Length);
//                    fileStream.Close();
//                    fileStream.Dispose();
//                    System.IO.BinaryReader binaryReader = new System.IO.BinaryReader((System.IO.Stream)new System.IO.MemoryStream(buffer));

//                    //Deserialize should restore the data
//                    patcher_facility_f_nSaveNo = -1;
//                    manager.Deserialize(binaryReader);

//                    //Cleanup
//                    binaryReader.Close();
//                }
//            }
//        }

//        private static void fakeDeserialize_CasinoData(CasinoDataMgr manager, int save_version)
//        {
//            string path = UTY.gameProjectPath + "\\" + "SaveData";
//            if (!System.IO.Directory.Exists(path))
//                System.IO.Directory.CreateDirectory(path);
//            path = path + "/" + string.Format("SaveData{0}_Casino", (patcher_casino_f_nSaveNo)) + ".save";

//            if (System.IO.File.Exists(path))
//            {
//                System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open);
//                if (fileStream != null)
//                {
//                    byte[] buffer = new byte[fileStream.Length];
//                    fileStream.Read(buffer, 0, (int)fileStream.Length);
//                    fileStream.Close();
//                    fileStream.Dispose();
//                    System.IO.BinaryReader binaryReader = new System.IO.BinaryReader((System.IO.Stream)new System.IO.MemoryStream(buffer));

//                    //Deserialize should restore the data
//                    patcher_casino_f_nSaveNo = -1;
//                    manager.Deserialize(binaryReader, save_version);

//                    //Cleanup
//                    binaryReader.Close();
//                }
//            }
//        }

//        private static int getFilesSaveVersion(int f_nSaveNo)
//        {
//            //The save version is still in the real SaveData, have to grab it
//            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
//            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter((System.IO.Stream)memoryStream);
//            string path = UTY.gameProjectPath + "\\" + "SaveData";
//            if (!System.IO.Directory.Exists(path))
//                System.IO.Directory.CreateDirectory(path);
//            path = path + "/" + string.Format("SaveData{0:D3}", (f_nSaveNo)) + ".save";

//            //Default, but try to get from save file so we dont have to update this in the future
//            int save_version = KISS_save_version;
//            if (System.IO.File.Exists(path))
//            {
//                System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open);

//                byte[] buffer = new byte[fileStream.Length];
//                fileStream.Read(buffer, 0, (int)fileStream.Length);
//                fileStream.Close();
//                fileStream.Dispose();
//                System.IO.BinaryReader binaryReader = new System.IO.BinaryReader((System.IO.Stream)new System.IO.MemoryStream(buffer));
//                string str1 = binaryReader.ReadString();
//                save_version = binaryReader.ReadInt32();
//            }

//            return save_version;
//        }
//        #endregion
//    }

//    //[HarmonyPatch(typeof(FacilityManager))]
//    //[HarmonyPatch("Init")]
//    //[HarmonyPatch(new Type[] { typeof(GameMain) })]
//    //public class HarmonyPatcher
//    //{
//    //    static void Prefix(FacilityManager __instance)
//    //    {
//    //        UnityEngine.Debug.Log("MoreFacilities: Patcher Prefix");
//    //        typeof(FacilityManager).GetField("FacilityCountMax", BindingFlags.Instance | BindingFlags.Public).SetValue(__instance, MoreFacilities.MaxFacilities);
//    //    }
//    //}

//    //public static class Patcher
//    //{
//    //    public static void Start()
//    //    {
//    //        UnityEngine.Debug.Log("MoreFacilities: Patcher Start");
//    //        if (GameMain.Instance.FacilityMgr != null)
//    //        {
//    //            typeof(FacilityManager).GetField("FacilityCountMax", BindingFlags.Instance | BindingFlags.Public).SetValue(GameMain.Instance.FacilityMgr, MoreFacilities.MaxFacilities);
//    //        }
//    //    }
//    //}
//}
