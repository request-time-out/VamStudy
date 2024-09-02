using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

//using System.Threading.Tasks; // VaM throws an error when this is left included. Not needed?

namespace MvKlein
{
    class Sxs4SceneHelper: MVRScript
    {
        private UIDynamicButton randBtn;
        private UIDynamicButton nextBtn;
        private UIDynamicButton preBtn;
        private UIDynamicButton cumBtn;
        private List<string> animList = new List<string>();
        private List<Atom> personList = new List<Atom>();
        private int currentIndex= -1;
        private JSONStorableBool showLogStore = new JSONStorableBool("Show Log",false);
        private UIDynamicPopup chooserPopup;
        private JSONStorableStringChooser chooserStringStore;
        private UIDynamicButton reInitBtn;
        private List<JSONStorable> timelineList;
        const float SPEED_STEP = 0.25f;

        public override void Init()
        {
            Log(pluginLabelJSON.val + " Loaded");
            pluginLabelJSON.val = "sxs4SceneHelper";
            myWasLoading = true;
            myNeedInit = true;
            // SuperController.singleton.onSceneLoadedHandlers += OnSceneLoaded;
            SuperController.singleton.BroadcastMessage("OnActionsProviderAvailable", this, SendMessageOptions.DontRequireReceiver);
        }

        private void Log(string msg)
        {
            if (showLogStore.val)
            {            
                SuperController.LogMessage(pluginLabelJSON.val +" "+ msg);
            }
        }

        private void DoNext()
        {
            DoSpecific(currentIndex+1);
        }
        private void DoPrevious()
        {
            DoSpecific(currentIndex-1);
        }

        private void DoSpecific(int targetIndex)
        {
            if (targetIndex < 0)
            {
                currentIndex = 0;
            }
            else if(targetIndex > animList.Count-1)
            {
                currentIndex = animList.Count - 1;
            }
            else
            {
                currentIndex = targetIndex;
            }

            chooserStringStore.val = animList[currentIndex];
        }

        private void DoRandom() 
        {
            total = animList.Count - 1;
            var index_random = new Random().Next(1,total)
            DoSpecific(index_random)
        }

        private void ReInit()
        {
            timelineList = new List<JSONStorable>();
            personList = new List<Atom>();
            var sceneAtoms = GetSceneAtoms();
            foreach (var sceneAtom in sceneAtoms)
            {
                if (sceneAtom.type == "Person")
                {
                    Log($"person id {sceneAtom.uid}");
                    personList.Add(sceneAtom);
                    var timelineId = sceneAtom.GetStorableIDs().First((storableId => storableId.Contains("VamTimeline")));
                    timelineList.Add(sceneAtom.GetStorableByID(timelineId));
                }
            }
            animList = timelineList[0].GetStringChooserJSONParam("Animation").choices;
            chooserStringStore.choices = animList;
            chooserStringStore.val = "";
            speedStorable.val = 1f;
            currentIndex = -1;
        }

        // Runs once when plugin loads - after Init()
        protected void Start()
        {
            // show a message
            var descStore = new JSONStorableString("", "Helper for sxs4's scenes,set shortcut key in KeyBindings \n\n mvklein");
            var descText =  CreateTextField(descStore, true);
            descText.height = 300f;
            descText.UItext.fontSize = 30;
            var showLogToggle = CreateToggle(showLogStore);
            showLogStore.setCallbackFunction = val =>
            {
                showLogStore.val = val;
            };
            chooserStringStore = new JSONStorableStringChooser("Animation", animList, "", "Animation",
                (string targetAnim) =>
                {
                    currentIndex = animList.IndexOf(targetAnim);
                    // var actionNames = maleTimeline.GetStringChooserParamNames();
                    // foreach (var actionName in actionNames)
                    // {
                    //     Log(actionName);
                    // }
                    Log($"play {targetAnim}");
                    foreach (var timeline in timelineList)
                    {
                        timeline.CallAction("Stop And Reset");
                    }
                    if(targetAnim != "")
                    timelineList[0].SetStringChooserParamValue("Animation",targetAnim);
                    timelineList[0].CallAction($"Play {targetAnim}"); 
                });
            chooserPopup = CreateScrollablePopup(chooserStringStore);
            // chooserPopup.popup.topButton.image.color = new Color(0.35f, 0.60f, 0.65f);
            // chooserPopup.popup.selectColor = new Color(0.35f, 0.60f, 0.65f);

            reInitBtn = CreateButton("ReInit");
            reInitBtn.height = 100;
            reInitBtn.button.onClick.AddListener(ReInit);
            
            preBtn = CreateButton("Previous");
            preBtn.height = 100;
            preBtn.button.onClick.AddListener(DoPrevious);
            
            nextBtn = CreateButton("Next");
            nextBtn.height = 100;
            nextBtn.button.onClick.AddListener(DoNext);

            randBtn = CreateButton("Random");
            randBtn.height = 100;
            randBtn.button.onClick.AddListener(DoRandom);

            speedStorable = new JSONStorableFloat("", 1f, 0.1f, 10f);
            speedStorable.setCallbackFunction = val =>
            {
                DoSpecificSpeed(val);
            };
            var speedSlider = CreateSlider(speedStorable);
            speedSlider.label = "Speed";

            cumBtn = CreateButton("Cum");
            cumBtn.height = 100;
            cumBtn.button.onClick.AddListener(DoCum); 
            
            // // 测试代码
            // var testBtn = CreateButton("test");
            // testBtn.height = 100;
            // testBtn.button.onClick.AddListener(DoTest);
            // showLogStore.val = true;
        }

        private void DoSpecificSpeed(float val)
        {
            if (val < 0.1f || val > 10f) return;
            foreach (var timelineItem in timelineList)
            {
                timelineItem.SetFloatParamValue("Speed",val);
            }
        }

        private void DoTest()
        {
            foreach (var allParamAndActionName in (GetAtomById("cum3").GetAllParamAndActionNames()))
            {
                Log(allParamAndActionName);
            }
            // var person = personList[1];
            // var storableIDs = person.GetStorableIDs();
            // Log($"{storableIDs.Count}");
            // foreach (var storableID in storableIDs)
            // {
            //     if (person.GetStorableByID(storableID).ToString().Contains("skin"))
            //     {
            //         Log($"{person.uid} {storableID} {person.GetStorableByID(storableID)}");
            //     }
            //     
            // }
        }

        private void DoCum()
        {
            foreach (var atom in personList)
            {
                if (IsMale(atom))
                {
                    var pluginId = atom.GetStorableIDs().First((storableId => storableId.Contains("PersonFluidEditor")));
                    var personFluidEditorPlugin = atom.GetStorableByID(pluginId);
                    // Spray X times, customizable
                    personFluidEditorPlugin.SetFloatParamValue("Spray X times, random duration",new Random().Next(3,10));
                    // foreach (var actionName in personFluidEditorPlugin.GetAllParamAndActionNames())
                    // {
                    //     Log(actionName);
                    // }

                }
            }
        }




        private void DoIncreaseSpeed()
        {
            speedStorable.val = Math.Min(speedStorable.val + SPEED_STEP, 10f);
        }

        private void DoDecreaseSpeed()
        {
            speedStorable.val = Math.Max(speedStorable.val - SPEED_STEP, 0.1f);
        }
        

        public void OnDestroy()
        {
            // Call this when this plugin should not receive shortcuts anymore
            SuperController.singleton.BroadcastMessage("OnActionsProviderDestroyed", this, SendMessageOptions.DontRequireReceiver);
            // SuperController.singleton.onSceneLoadedHandlers -= OnSceneLoaded;
            // timelineList.Clear();
            // animList.Clear();
            // RemoveButton(preBtn);
            // RemoveButton(nextBtn);
            // RemoveButton(reInitBtn);
            // RemovePopup(chooserPopup);
        }

        // This method will be called by Keybindings when it is ready.
        public void OnBindingsListRequested(List<object> bindings)
        {
            bindings.Add(new JSONStorableAction("Previous Anim", DoPrevious));
            bindings.Add(new JSONStorableAction("Next Anim", DoNext));
            bindings.Add(new JSONStorableAction("Cum", DoCum));
            bindings.Add(new JSONStorableAction("Increase speed", DoIncreaseSpeed));
            bindings.Add(new JSONStorableAction("Decrease speed", DoDecreaseSpeed));
        }

        // A Unity thing - runs every physics cycle
        public void FixedUpdate()
        {
            // put code here
            
        }

        // Unity thing - runs every rendered frame
        bool myWasLoading = true;
        bool myNeedInit = true;
        private JSONStorableFloat speedStorable;

        private void ActualInit()
        {
            // ...
            Log("ActualInit");
            try
            {
                ReInit();
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log("Init error, Maybe it's not sxs4's scene");
            }
            myNeedInit = false;
        }

        private void Update()
        {
            bool isLoading = SuperController.singleton.isLoading;
            bool isOrWasLoading = isLoading || myWasLoading;
            myWasLoading = isLoading;
            if (isOrWasLoading)
                myNeedInit = true;
            else if (myNeedInit)
                ActualInit();
        
            // ...
        }
        
        public static bool IsMale(Atom atom)
        {
            bool isMale = false;
            // If the peson atom is not "On", then we cant determine their gender it seems as GetComponentInChildren<DAZCharacter> just returns null
            if (atom.on && (atom.containingSubScene == null || atom.containingSubScene.containingAtom.on) &&
                atom.type == "Person")
                isMale = atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("male") ||
                         atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("jarlee") ||
                         atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("jarjulian") ||
                         atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("Futa");
            return (isMale);
        }
    }
}