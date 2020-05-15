using BepInEx;
using BepInEx.Harmony;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace COM3D2.MoreFacilities.Plugin.Core
{
    [BepInPlugin("org.guest4168.plugins.morefacilitiesplugin", "More Facilities Plug-In", "1.0.0.0")]
    public class MoreFacilities : BaseUnityPlugin
    {
        #region Main Code

        //Constants that are may be adjustable in the future
        public const int MaxFacilities = 60;    //Maximum facilities to make available
        private const int scrollJump = 1;       //How many rows you move when scrolling

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
            //Copied from examples
            UnityEngine.Debug.Log("MoreFacilities: Awake");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this);
            this.managerObject = new UnityEngine.GameObject("moreFacilitiesManager");
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this.managerObject);
            this.managerObject.AddComponent<MoreFacilitiesManager>().Initialize();
        }
        #endregion
    }
}
