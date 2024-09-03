using Battlehub.RTHandles;
using MeshVR;
using MVR.FileManagementSecure;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace JayJayWon
{
    public class UIAButtonGrid : JSONStorableObject
    {
        private JSONStorableInt gridRowsJSInt;
        private JSONStorableInt gridColumnsJSInt;
        public JSONStorableEnumStringChooser buttonSizeJSEnum;
        public JSONStorableString gridLabelJSS;
        public JSONStorableBool quickLaunchBarJSBool;

        public string gridRef
        {
            get {
                if (UIAStorables.quickLaunchButtonGrid == this) return "QL";
                return (UIAStorables.buttonGridsList.IndexOf(this) + 1).ToString();
            }
        }
        public int gridIndex
        {
            get { return (UIAStorables.buttonGridsList.IndexOf(this)); }
        }

        public int GetButtonIndexInGrid (UIAButton button)
        {
            return buttonList.IndexOf(button);
        }

        public int GetButtonRowRefInGrid (UIAButton button)
        {
            int buttonRef = GetButtonIndexInGrid(button);
            return  (buttonRef / gridColumns)+1;
        }

        public int GetButtonColRefInGrid(UIAButton button)
        {
            int buttonRef = GetButtonIndexInGrid(button);
            return (buttonRef % gridColumns) +1;
        }

        public string GetButtonRCRef(UIAButton button)
        {
            return "R" + GetButtonRowRefInGrid(button) + "C" + GetButtonColRefInGrid(button);
        }

        public string GetButtonRCRef(int buttonIndex)
        {
            return GetButtonRCRef(buttonList[buttonIndex]);
        }

        public int GetButtonIndexFromRCRef(string buttonRCRef)
        {
            int indexOfC = buttonRCRef.IndexOf('C');
            string rowRef = buttonRCRef.Substring(1, indexOfC - 1);
            string colRef = buttonRCRef.Substring(indexOfC + 1);
            return ((int.Parse(rowRef)-1)*gridColumns)+int.Parse(colRef)-1;
        }

        public int gridRows {
            get { return gridRowsJSInt.val; }
            set {
                gridRowsJSInt.val = value;
                UpdateGridButtons();
                RecalcGazeSelections();

            }
        }
        public int gridColumns
        {
            get { return gridColumnsJSInt.val; }
            set {
                gridColumnsJSInt.val = value;
                UpdateGridButtons();
                RecalcGazeSelections();
            }
        }

        public string GetGridLayout()
        {
            return gridColumns.ToString() + "x" + gridRows.ToString();
        }

        public string _buttonSize
        {
            get { return buttonSizeJSEnum.displayVal; }
            set { buttonSizeJSEnum.val = ButtonSize.GetEnumVal(value); }
        }
        public string gridLabel
        {
            get
            {
                if (gridLabelJSS.val == "" || !GameControlSettings.displayGridLabelsJSB.val) return gridRef;
                return gridLabelJSS.val;
            }
            set { gridLabelJSS.val = value; }
        }
        public bool _quickLaunchBar
        {
            get { return quickLaunchBarJSBool.val; }
            set { quickLaunchBarJSBool.val = value; }
        }

        public bool GetTreeBrowserRowCollapsedRow(int row)
        {
            if (!treeBrowserRowCollapsedDict.ContainsKey(row)) return true;
            return treeBrowserRowCollapsedDict[row];
        }

        public void SetTreeBrowserRowCollapsedRow(int row, bool collapsed)
        {
            if (!treeBrowserRowCollapsedDict.ContainsKey(row)) treeBrowserRowCollapsedDict.Add(row, collapsed);
            else treeBrowserRowCollapsedDict[row] = collapsed;
        }

        public bool treeBrowserCollapsed = true;
        private Dictionary<int, bool> treeBrowserRowCollapsedDict;

        public int _activeButtonCount {
            get
            {
                int activeButtonCount = 0;

                int _buttonCount = 1;
                foreach (UIAButton button in buttonList)
                {
                    if (!button.ContainsAllBlankOperations() && _buttonCount <= (buttonCount))
                    {
                        activeButtonCount++;

                    }
                    _buttonCount++;
                    }
                return activeButtonCount;
            }
        }
        public int _maxButtonRow
        {
            get
            {
                int maxButtonRow = 0;

                int _buttonCount = 1;
                foreach (UIAButton button in buttonList)
                {
                    if (!button.ContainsAllBlankOperations() && _buttonCount <= (buttonCount))
                    {
                        maxButtonRow = (buttonCount / gridColumns);
                        if (buttonCount % gridColumns > 0) maxButtonRow++;
                    }
                    _buttonCount++;
                }
                return maxButtonRow;
            }
        }

        public bool _gazeSelectionAtoms = false;
        public bool _gazeSelectionFemales = false;
        public bool _gazeSelectionMales = false;
        public bool _gazeSelectionPerson = false;
        public bool _gazeSelectionNonPerson = false;
        public Dictionary<string, bool> _gazeSelectionCTGDic;
        public bool _vamSelectionAtoms = false;
        public bool _vamSelectionFemales = false;
        public bool _vamSelectionMales = false;
        public bool _vamSelectionPerson = false;
        public bool _vamSelectionNonPerson = false;

        public int buttonCount
        {
            get { return (gridRows * gridColumns); }
        }

        public List<UIAButton> buttonList = new List<UIAButton>();

        public UIAButtonGrid(int columns = 0, int rows = 0, bool quickLaunchBar = false)
        {
            try
            {
                gridRowsJSInt = new JSONStorableInt("gridRows", rows, 1, 9);
                gridColumnsJSInt = new JSONStorableInt("gridColumns", columns, 1, 9);
                buttonSizeJSEnum = new JSONStorableEnumStringChooser("buttonSize", ButtonSize.enumManifestName, ButtonSize.medium, "", SwitchButtonSize);
                gridLabelJSS = new JSONStorableString("gridLabel", "");
                quickLaunchBarJSBool = new JSONStorableBool("quickLaunchBar", quickLaunchBar);

                treeBrowserRowCollapsedDict = new Dictionary<int, bool>();
                RegisterParam(gridRowsJSInt);
                RegisterParam(gridColumnsJSInt);
                RegisterParam(buttonSizeJSEnum);
                RegisterParam(gridLabelJSS);
                RegisterParam(quickLaunchBarJSBool);

                _gazeSelectionCTGDic = new Dictionary<string, bool>();
                UpdateGridButtons();
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        private void SwitchButtonSize (string val)
        {
            GameControlUI.RefreshWristUIButtonGrid();
        }

        public void AtomNameUpdate(string oldName, string newName)
        {
            foreach (UIAButton button in buttonList)
            {
                button.AtomNameUpdate(oldName, newName);
            }
        }
        public void AtomRemovedUpdate(string oldName)
        {
            foreach (UIAButton button in buttonList)
            {
                button.AtomRemovedUpdate(oldName);
            }
        }

        public bool ContainsBlankButtons()
        {
            foreach (UIAButton button in buttonList.GetRange(0, buttonCount))
            {
                if (button.ContainsAllBlankOperations()) return (true);
            }
            return (false);
        }
        public int GetFirstBlankButtonIndex()
        {
            for (int i = 0; i < buttonList.GetRange(0, buttonCount).Count; i++)
            {
                if (buttonList[i].ContainsAllBlankOperations()) return (i);
            }
            return (-1);
        }
        public Vector2 GetButtonDimensions()
        {
            Vector2 dimensions = new Vector2();
            switch (buttonSizeJSEnum.val)
            {
                case ButtonSize.micro:
                    dimensions = ButtonSize.Micro.dimensions;
                    break;
                case ButtonSize.mini:
                    dimensions = ButtonSize.Mini.dimensions;
                    break;
                case ButtonSize.small:
                    dimensions = ButtonSize.Small.dimensions;
                    break;
                case ButtonSize.medium:
                    dimensions = ButtonSize.Medium.dimensions;
                    break;
                case ButtonSize.large:
                    dimensions = ButtonSize.Large.dimensions;
                    break;
            }

            return (dimensions);
        }
        public void UpdateGridButtons()
        {
            if (buttonList.Count < (buttonCount))
            {
                for (int i = buttonList.Count; i < (buttonCount); i++)
                {
                    UIAButton newButton = new UIAButton(this);
                    buttonList.Add(newButton);
                }
            }
        }


        public bool IsSelectionTargetButton()
        {
            bool cgtGazeSelection = false;
            foreach (KeyValuePair<string, bool> kvp in _gazeSelectionCTGDic)
            {
                if (kvp.Value)
                {
                    cgtGazeSelection = true;
                    break;
                }
            }
            return (_gazeSelectionAtoms || _gazeSelectionMales || _gazeSelectionFemales || _gazeSelectionPerson || _vamSelectionAtoms || _vamSelectionMales || _vamSelectionFemales || _vamSelectionPerson || cgtGazeSelection);
        }
        public void RecalcGazeSelections()
        {
            _gazeSelectionAtoms = false;
            _gazeSelectionMales = false;
            _gazeSelectionFemales = false;
            _gazeSelectionPerson = false;
            _gazeSelectionNonPerson = false;
            _vamSelectionAtoms = false;
            _vamSelectionMales = false;
            _vamSelectionFemales = false;
            _vamSelectionPerson = false;
            _vamSelectionNonPerson = false;
            _gazeSelectionCTGDic.Clear();
            foreach (CustomTargetGroupType cgt in CustomTargetGroupSettings.customTargetGroupTypeList)
            {
                _gazeSelectionCTGDic.Add(cgt.cgtNameJSS.val, false);
            }
            int gridCount = Math.Min(gridColumns * gridRows, buttonList.Count);
            foreach (UIAButton button in buttonList.GetRange(0, gridCount))
            {
                foreach (UIAButtonOperation buttonOp in button.buttonOperations)
                {
                    if (buttonOp.targetComponent.targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom && UIAButtonOpType.IsAtomTargetable(buttonOp.buttonOpTypeJSEnum.val))
                    {
                        if (buttonOp.targetComponent.targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.topEnumValFlag)
                        {
                            int lastViewedTarget = buttonOp.targetComponent.targetNameJSMultiEnum.valTopEnum;
                            if (lastViewedTarget == LastViewedTargetType.lastViewedAtom) _gazeSelectionAtoms = true;
                            if (lastViewedTarget == LastViewedTargetType.lastViewedPerson) _gazeSelectionPerson = true;
                            if (lastViewedTarget == LastViewedTargetType.lastViewedFemale) _gazeSelectionFemales = true;
                            if (lastViewedTarget == LastViewedTargetType.lastViewedMale) _gazeSelectionMales = true;
                            if (lastViewedTarget == LastViewedTargetType.lastViewedNonPerson) _gazeSelectionNonPerson = true;
                        }
                        else if (buttonOp.targetComponent.targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.mainValFlag)
                        {
                            string cgtName = buttonOp.targetComponent.targetNameJSMultiEnum.mainVal;
                            if (_gazeSelectionCTGDic.ContainsKey(cgtName)) _gazeSelectionCTGDic[cgtName] = true;
                        }
                    }
                    else if (buttonOp.targetComponent.targetCategoryJSEnum.val == TargetCategory.vamSelectedAtom && UIAButtonOpType.IsAtomTargetable(buttonOp.buttonOpTypeJSEnum.val))
                    {
                        if (buttonOp.targetComponent.targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.topEnumValFlag)
                        {
                            int lastSelectedAtom = buttonOp.targetComponent.targetNameJSMultiEnum.valTopEnum;
                            if (lastSelectedAtom == LastSelectedTargetType.lastSelectedAtom) _vamSelectionAtoms = true;
                            if (lastSelectedAtom == LastSelectedTargetType.lastSelectedPerson) _vamSelectionPerson = true;
                            if (lastSelectedAtom == LastSelectedTargetType.lastSelectedFemale) _vamSelectionFemales = true;
                            if (lastSelectedAtom == LastSelectedTargetType.lastSelectedMale) _vamSelectionMales = true;
                            if (lastSelectedAtom == LastSelectedTargetType.lastSelectedNonPerson) _vamSelectionNonPerson = true;
                        }
                    }
                }

            }

        }

        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);

            JSONArray buttonArray = new JSONArray();

            foreach (UIAButton button in buttonList.GetRange(0, buttonCount))
            {
                buttonArray.Add(button.GetJSON());
            }

            jc["Buttons"] = buttonArray;

            return jc;
        }
        public void LoadJSON(JSONClass gridJSON, string uiapPackageName, int uiapFormatVersion)
        {
            base.RestoreFromJSON(gridJSON);

            buttonList.Clear();
            if (gridJSON["Buttons"] != null)
            {
                JSONArray buttonArrayJSON = gridJSON["Buttons"].AsArray;
                for (int i = 0; i < buttonArrayJSON.Count; i++)
                {
                    JSONClass buttonJSON = buttonArrayJSON[i].AsObject;
                    UIAButton button = new UIAButton(this);
                    buttonList.Add(button);

                    button.LoadJSON(buttonJSON, uiapPackageName, uiapFormatVersion);
                }
            }
            RecalcGazeSelections();
        }
    }

    public abstract class ButtonComponentBase : JSONStorableObject
    {
        public UIAButton parentButton;

        public ButtonComponentBase(UIAButton _parent)
        {
            parentButton = _parent;
        }
    }
    public abstract class ButtonOperationComponentBase : ButtonComponentBase
    {
        public UIAButtonOperation parentButtonOperation;

        public ButtonOperationComponentBase(UIAButtonOperation parent):base(parent.parentButton)
        {
            parentButtonOperation = parent;
        }
    }

    public class FileReference : ButtonOperationComponentBase
    {
        public JSONStorableEnumStringChooser fileSelectionModeJSEnum;
        public JSONStorableString filePathJSString;
        public JSONStorableBool useLatestVARJSBool;
        public JSONStorableBool mergeLoadPresetJSBool;
        public JSONStorableBool forceUniqueSaveNameJSB;
        public JSONStorableBool includeSubFoldersJSB;

        public JSONStorableBool morphPresetIncludePhysicalJSB;
        public JSONStorableBool morphPresetIncludeAppJSB;

        public JSONStorableBool generalPresetIncludeAppJSB;
        public JSONStorableBool generalPresetIncludePhysicalJSB;
        public JSONStorableBool generalPresetIncludePoseJSB;

        public JSONStorableBool posePresetSnapBoneToPoseJSB;

        public JSONStorableBool closeGridOnLoadUIAPJSB;

        public BAOrderedResourceFilter baFilter;

        private Dictionary<string, string> lastSelectedFilePerAtom;

        public Texture2D buttonTexture { get; set; }

        public string currentActionSelectedFile { get; protected set; }

        public int fileReferenceType { get; protected set; }
        public int fileReferenceCategory { get
            {
                return FileReferenceTypes.GetFileRefCategory(fileReferenceType);
            }
        }

        public bool IsThumbnailFileRefType()
        {
            if (fileReferenceCategory != FileReferenceCategory.uiapCat && fileReferenceType != FileReferenceTypes.addonPackagesFolder && fileReferenceType != FileReferenceTypes.plugin) return true;
            return false;
        }

        public FileReference(int _fileRefType, UIAButtonOperation parent) : base(parent)
        {

            fileReferenceType = _fileRefType;

            baFilter = new BAOrderedResourceFilter(_fileRefType,"");

            fileSelectionModeJSEnum = new JSONStorableEnumStringChooser("fileSelectionMode", FileSelectionMode.enumManifestName, FileSelectionMode.singleFile, "",FileSelectModeChanged, fileSelectionModeExclusions);
            RegisterParam(fileSelectionModeJSEnum);

            filePathJSString = new JSONStorableString("filePath", "", FilePathUpdated);
            RegisterParam(filePathJSString);

            useLatestVARJSBool = new JSONStorableBool("useLatestVAR", true);
            RegisterParam(useLatestVARJSBool);

            mergeLoadPresetJSBool = new JSONStorableBool("mergeLoadPreset", false);
            RegisterParam(mergeLoadPresetJSBool);

            closeGridOnLoadUIAPJSB = new JSONStorableBool("closeGridOnLoadUIAP", true);
            RegisterParam(closeGridOnLoadUIAPJSB);

            forceUniqueSaveNameJSB = new JSONStorableBool("forceUniqueSaveName", false);
            RegisterParam(forceUniqueSaveNameJSB);

            includeSubFoldersJSB = new JSONStorableBool("includeSubFolders", false);
            RegisterParam(includeSubFoldersJSB);

            morphPresetIncludePhysicalJSB = new JSONStorableBool("morphPresetIncludePhysical", false);
            morphPresetIncludeAppJSB = new JSONStorableBool("morphPresetIncludeApp", true);
            if (parent.buttonOpTypeJSEnum.val == UIAButtonOpType.loadMorphPreset)
            {
                RegisterParam(morphPresetIncludePhysicalJSB);
                RegisterParam(morphPresetIncludeAppJSB);
            }

            generalPresetIncludeAppJSB = new JSONStorableBool("generalPresetIncludeApp", true);
            generalPresetIncludePhysicalJSB = new JSONStorableBool("generalPresetIncludePhysical", true);
            generalPresetIncludePoseJSB = new JSONStorableBool("generalPresetIncludePose", true);
            if (parent.buttonOpTypeJSEnum.val == UIAButtonOpType.loadGeneralPreset)
            {
                RegisterParam(generalPresetIncludeAppJSB);
                RegisterParam(generalPresetIncludePhysicalJSB);
                RegisterParam(generalPresetIncludePoseJSB);
            }

            posePresetSnapBoneToPoseJSB = new JSONStorableBool("posePresetSnapBoneToPose", true);
            if (parent.buttonOpTypeJSEnum.val == UIAButtonOpType.loadPosePreset)
            {
                RegisterParam(posePresetSnapBoneToPoseJSB);
            }

            lastSelectedFilePerAtom = new Dictionary<string, string>();
        }

        private List<int> fileSelectionModeExclusions { get
            {
                List<int> fileSelectionModeExclusions = new List<int>();

                if (parentButtonOperation.buttonOpCategoryJSEnum.val == UIAButtonCategory.presets && parentButtonOperation.savePresetJSB.val)
                {
                    fileSelectionModeExclusions.Add(FileSelectionMode.alphabeticalFromFolder);
                    fileSelectionModeExclusions.Add(FileSelectionMode.randomFromFolder);
                    fileSelectionModeExclusions.Add(FileSelectionMode.randomFromBA);
                    fileSelectionModeExclusions.Add(FileSelectionMode.sequentialFromBA);
                    fileSelectionModeExclusions.Add(FileSelectionMode.chooseFromBA);
                }
                if ((fileReferenceCategory != FileReferenceCategory.presetCat && fileReferenceCategory != FileReferenceCategory.sceneCat) || parentButtonOperation.buttonOpTypeJSEnum.val == UIAButtonOpType.loadSubScene)
                {
                    fileSelectionModeExclusions.Add(FileSelectionMode.randomFromBA);
                    fileSelectionModeExclusions.Add(FileSelectionMode.sequentialFromBA);
                    fileSelectionModeExclusions.Add(FileSelectionMode.chooseFromBA);
                }
                return fileSelectionModeExclusions;
            } }
        public void UpdateFileSelectionModeExclusions()
        {
            
            fileSelectionModeJSEnum.SetEnumChoices(FileSelectionMode.enumManifestName, fileSelectionModeExclusions);
        }

        public void CopyFrom(FileReference fileReference)
        {
            if (fileReference != null)
            {
                base.CopyFrom(fileReference);
                buttonTexture = fileReference.buttonTexture;
                baFilter.RestoreJSON(fileReference.baFilter.GetJSON());
            }           
        }

        public void QueueLoadTexture(string url)
        {
            
            if (string.IsNullOrEmpty(url))
                return;

            var normalizedURL = SuperController.singleton.NormalizeLoadPath(url);
            if (!FileManagerSecure.FileExists(normalizedURL))
            {
                buttonTexture = null;
                return;
            }

            ImageUtils.QueueLoadTexture(normalizedURL, QueuedImageLoadCallback);
        }

        private void QueuedImageLoadCallback(ImageLoaderThreaded.QueuedImage qi)
        {
            buttonTexture = qi.tex;
            parentButton.UpdateThumbnailImage(buttonTexture);
        }

        private void FileSelectModeChanged(string mode)
        {
            if (fileSelectionModeJSEnum.val!=FileSelectionMode.singleFile && fileSelectionModeJSEnum.val!=FileSelectionMode.none && filePathJSString.val != "" && !FileManagerSecure.DirectoryExists(filePathJSString.val))
            {
                filePathJSString.val = FileManagerSecure.GetDirectoryName(filePathJSString.val);
            }

            if (fileSelectionModeJSEnum.val == FileSelectionMode.singleFile && IsThumbnailFileRefType()) QueueLoadTexture(filePathJSString.val.Substring(0, filePathJSString.val.LastIndexOf('.')) + ".jpg");
            else buttonTexture = null;
        }

        public string GetFileRefTypeDescription()
        {
            return FileReferenceTypes.GetFileRefDescription(fileReferenceType);
        }

        public string GetFileTypeFilter()
        {
            return FileReferenceCategory.GetFileTypeFilter(fileReferenceCategory);
        }

        public string GetDefaultFolder()
        {
            int targetValTopEnum = parentButtonOperation.targetComponent.targetNameJSMultiEnum.valTopEnum;
            if (parentButtonOperation.buttonOpTypeJSEnum.val == UIAButtonOpType.spawnAtom && fileReferenceType == FileReferenceTypes.generalPreset) return "Custom\\Atom\\" + parentButtonOperation.spawnAtomComponent.atomTypeJSEnum.displayVal;
            else if (fileReferenceType == FileReferenceTypes.generalPreset)
            {
                if (parentButtonOperation.targetComponent.targetCategoryJSEnum.val == TargetCategory.specificAtom)
                {
                    if (parentButtonOperation.targetComponent.specificAtomTypeJSS.val != "") return ("Custom\\Atom\\" + parentButtonOperation.targetComponent.specificAtomTypeJSS.val);
                }
                else if (parentButtonOperation.targetComponent.targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.mainValFlag)
                {
                    string cgtName = parentButtonOperation.targetComponent.targetNameJSMultiEnum.mainVal;
                    CustomTargetGroupType cgt = CustomTargetGroupSettings.GetCGTFromName(cgtName);
                    return "Custom\\Atom\\" + cgt.cgtAtomTypeJSEnum.displayVal;

                }
                else if (parentButtonOperation.targetComponent.targetCategoryJSEnum.val == TargetCategory.userChosenAtom)
                {
                    if (targetValTopEnum != UserChosenTargetType.anyAtoms && targetValTopEnum != UserChosenTargetType.nonPersonAtoms) return "Custom\\Atom\\Person\\General";                    
                }
                else if (parentButtonOperation.targetComponent.targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom)
                {
                    if (targetValTopEnum != LastViewedTargetType.lastViewedAtom && targetValTopEnum != LastViewedTargetType.lastViewedNonPerson) return "Custom\\Atom\\Person\\General";
                }
                else if (parentButtonOperation.targetComponent.targetCategoryJSEnum.val == TargetCategory.vamSelectedAtom)
                {
                    if (targetValTopEnum != LastSelectedTargetType.lastSelectedAtom && targetValTopEnum != LastSelectedTargetType.lastSelectedNonPerson) return "Custom\\Atom\\Person\\General";
                }
                else if (parentButtonOperation.targetComponent.targetCategoryJSEnum.val == TargetCategory.atomGroup)
                {
                    if (targetValTopEnum != AllAtomsTargetType.allAtoms && targetValTopEnum != AllAtomsTargetType.allNonPersonAtoms) return "Custom\\Atom\\Person\\General";
                }
                return "Custom\\Atom";
            }
            return FileReferenceTypes.GetDefaultFolder(fileReferenceType);
        }

        public void GetFileFolderDialog()
        {            
            string folderName;
            if (filePathJSString.val != "" && filePathJSString.val.Contains(GetDefaultFolder()))
            {
                if (FileManagerSecure.DirectoryExists(filePathJSString.val)) folderName = filePathJSString.val;
                else folderName = FileManagerSecure.GetDirectoryName(filePathJSString.val);
            }
            else folderName = GetDefaultFolder();

            List<ShortCut> shortcuts;
            if (GetDefaultFolder() == "Custom\\Atom")
            {
                shortcuts = new List<ShortCut>();
                foreach (ShortCut sc in FileManagerSecure.GetShortCutsForDirectory(GetDefaultFolder(), false, false, true))
                {
                    if (FileManagerSecure.GetDirectories(sc.path).Count() == 1)
                    {
                        string fname = FileManagerSecure.GetDirectories(sc.path).First();
                        string dName = fname.Substring(fname.LastIndexOf('\\') + 1);
                        if (dName == "Person")
                        {
                            bool containsGeneral = false;
                            foreach (string personPresetFolder in FileManagerSecure.GetDirectories(fname))
                            {
                                string personPresetFolderName = personPresetFolder.Substring(personPresetFolder.LastIndexOf('\\') + 1);
                                if (personPresetFolderName == "General") containsGeneral = true;
                            }
                            if (!containsGeneral) continue;
                        }                        
                    }
                    shortcuts.Add(sc);                    
                }
            }
            else shortcuts = FileManagerSecure.GetShortCutsForDirectory(GetDefaultFolder(), false, false, true);

            string prefixRemoval = FileReferenceTypes.GetPrefixRemoval(fileReferenceType);

            if ((fileSelectionModeJSEnum.val == FileSelectionMode.singleFile || parentButtonOperation.buttonOpTypeJSEnum.val == UIAButtonOpType.loadMotionCapture) && fileReferenceType != FileReferenceTypes.addonPackagesFolder)
            {
                SuperController.singleton.GetMediaPathDialog(FileRefSelected, GetFileTypeFilter(), folderName, false, true, false, prefixRemoval, false, shortcuts);
                if (parentButtonOperation.buttonOpCategoryJSEnum.val== UIAButtonCategory.presets && parentButtonOperation.savePresetJSB.val)
                {
                    uFileBrowser.FileBrowser browser = SuperController.singleton.mediaFileBrowserUI;
                    browser.SetTextEntry(true);
                    browser.fileEntryField.text = String.Format("{0}.{1}", ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString(), "vap");
                    browser.ActivateFileNameField();

                }
            }
            else if (fileSelectionModeJSEnum.val != FileSelectionMode.none || fileReferenceType == FileReferenceTypes.addonPackagesFolder) SuperController.singleton.GetDirectoryPathDialog(FileRefSelected, folderName, shortcuts, false);
        }

        private void FileRefSelected(string newFileRef)
        {
            if (newFileRef != "")
            {
                newFileRef = FileUtils.NormalizeAndRemoveVARPath(newFileRef);
                if (parentButtonOperation.buttonOpCategoryJSEnum.val == UIAButtonCategory.presets && parentButtonOperation.savePresetJSB.val && fileSelectionModeJSEnum.val==FileSelectionMode.singleFile)
                {
                    newFileRef = FileManagerSecure.GetDirectoryName(newFileRef) + "\\Preset_" + FileManagerSecure.GetFileName(newFileRef);
                }
                filePathJSString.val = newFileRef;
                if (parentButtonOperation.buttonOpTypeJSEnum.val == UIAButtonOpType.loadMotionCapture) parentButtonOperation.motionCaptureComponent.ReloadMotionCaptureSceneData(newFileRef);          
            }
        }

        private void FilePathUpdated(string fileName)
        {
            if (fileReferenceType == FileReferenceTypes.clothingPreset) UIAButton.presetMergeClothingGeometryIDs.Remove(fileName);

            if (fileSelectionModeJSEnum.val == FileSelectionMode.singleFile && IsThumbnailFileRefType())
            {
                int periodIndex = filePathJSString.val.LastIndexOf('.');
                if (periodIndex >-1) QueueLoadTexture(filePathJSString.val.Substring(0, filePathJSString.val.LastIndexOf('.')) + ".jpg");
            }
            else buttonTexture = null;
        }
        public override void RestoreFromJSON(JSONClass jc, string uiapPackageName)
        {
            base.RestoreFromJSON(jc, null);
            if (jc["baFilter"]!=null) baFilter.RestoreJSON(jc["baFilter"] as JSONClass);
            filePathJSString.val = FileUtils.NormalizeAndRemoveVARPath(filePathJSString.val);
            if (uiapPackageName != "" && !filePathJSString.val.Contains(":") && FileManagerSecure.FileExists(uiapPackageName + ":/" + filePathJSString.val)) filePathJSString.val = uiapPackageName + ":/" + filePathJSString.val;

            if (fileSelectionModeJSEnum.val == FileSelectionMode.singleFile && IsThumbnailFileRefType() && filePathJSString.val != "")
            {
                int periodIndex = filePathJSString.val.LastIndexOf('.');
                if (periodIndex > -1)  QueueLoadTexture(filePathJSString.val.Substring(0, periodIndex) + ".jpg");
            }
            else buttonTexture = null;
        }
        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            var jc  = base.GetJSON(jspLoadExclusions);
            jc["baFilter"] = baFilter.GetJSON();
            return jc;
        }
        public string GetCurrentFile(string atomName)
        {
            if (fileSelectionModeJSEnum.val == FileSelectionMode.none) return "";
            if (fileSelectionModeJSEnum.val == FileSelectionMode.singleFile) return filePathJSString.val;
            if (lastSelectedFilePerAtom.ContainsKey(atomName)) return lastSelectedFilePerAtom[atomName];
            return "";
        }
        public void ClearCurrentFile(string atomName)
        {
            lastSelectedFilePerAtom.Remove(atomName);
        }
        private void SetLastSelectedFile(string atomName, string lastSelectedFile)
        {
            if (!lastSelectedFilePerAtom.ContainsKey(atomName)) lastSelectedFilePerAtom.Add(atomName, lastSelectedFile);
            else lastSelectedFilePerAtom[atomName] = lastSelectedFile;
        }
        public void AtomNameUpdate(string oldName, string newName)
        {
            foreach (string key in lastSelectedFilePerAtom.Keys.ToList())
            {
                if (key == oldName)
                {
                    lastSelectedFilePerAtom[newName] = lastSelectedFilePerAtom[key];
                    lastSelectedFilePerAtom.Remove(key);
                }
            }
        }

        public void AtomRemovedUpdate(string oldName)
        {
            foreach (string key in lastSelectedFilePerAtom.Keys.ToList())
            {
                if (key == oldName) lastSelectedFilePerAtom.Remove(key);
            }
        }
        public void Reset()
        {
            currentActionSelectedFile = "";
        }

        public bool GetNextFileSelection()
        {
            string filePath;
            string firstTargetAtomName = "";
            string lastSelectedFile = "";
            bool fileSelectionComplete = true;

            List<string> targetAtomNamesForCurrentAction = new List<string>();

            if (parentButtonOperation.IsComponentInButtonOpType(ButtonComponentTypes.targetComponent)) targetAtomNamesForCurrentAction = parentButtonOperation.targetComponent.currentActionTargetAtomNames;

            if (targetAtomNamesForCurrentAction.Count > 0) firstTargetAtomName = targetAtomNamesForCurrentAction.First();

            if (firstTargetAtomName != "" && lastSelectedFilePerAtom.ContainsKey(firstTargetAtomName)) lastSelectedFile = lastSelectedFilePerAtom[firstTargetAtomName];

            if (fileReferenceType == FileReferenceTypes.addonPackagesFolder)  filePath = filePathJSString.val;
            else
            {
                fileSelectionComplete = PatreonFeatures.GetNextFileSelection(this, out filePath, lastSelectedFile, UserSelectedFileReferenceCallback);

                if (filePath != "" && filePath.Contains(":") && filePath.IndexOf(':') > 1 && useLatestVARJSBool.val) filePath = FileUtils.GetLatestVARPath(filePath);

                if (fileSelectionComplete) SetLastSelectedFileReferenceToTargetAtoms(filePath);
            }

            return fileSelectionComplete;
        }

        private void SetLastSelectedFileReferenceToTargetAtoms(string filePath)
        {
            currentActionSelectedFile = filePath;
            UIAButton.presetMergeClothingGeometryIDs.Remove(filePath);

            if (parentButtonOperation.IsComponentInButtonOpType(ButtonComponentTypes.targetComponent))
            {
                foreach (string targetAtomName in parentButtonOperation.targetComponent.currentActionTargetAtomNames)
                {
                    SetLastSelectedFile(targetAtomName, filePath);
                }
            }

        }

        private void UserSelectedFileReferenceCallback(string filePath)
        {
            if (filePath == "") return;
            else
            {
                if (parentButtonOperation.buttonOpCategoryJSEnum.val == UIAButtonCategory.presets && parentButtonOperation.savePresetJSB.val)
                {
                    filePath = FileManagerSecure.GetDirectoryName(filePath) + "\\Preset_" + FileManagerSecure.GetFileName(filePath);
                }
                SetLastSelectedFileReferenceToTargetAtoms(filePath);
                parentButton.CheckActionFileReferences();
            }
        }
    }

    public class PresetLockStore
    {
        public bool _generalPresetLock;
        public bool _appPresetLock;
        public bool _posePresetLock;
        public bool _animationPresetLock;
        public bool _glutePhysPresetLock;
        public bool _breastPhysPresetLock;
        public bool _pluginPresetLock;
        public bool _skinPresetLock;
        public bool _morphPresetLock;
        public bool _hairPresetLock;
        public bool _clothingPresetLock;

        public void StorePresetLocks(Atom atom, bool clearAllLocks = false, bool lockClothingPreset = false)
        {
            List<PresetManagerControl> pmControlList = atom.presetManagerControls;
            foreach (PresetManagerControl pmc in pmControlList)
            {
                if (pmc.name == "geometry") _generalPresetLock = pmc.lockParams;
                if (pmc.name == "AppearancePresets") _appPresetLock = pmc.lockParams;
                if (pmc.name == "PosePresets") _posePresetLock = pmc.lockParams;
                if (pmc.name == "AnimationPresets") _animationPresetLock = pmc.lockParams;
                if (pmc.name == "FemaleGlutePhysicsPresets") _glutePhysPresetLock = pmc.lockParams;
                if (pmc.name == "FemaleBreastPhysicsPresets") _breastPhysPresetLock = pmc.lockParams;
                if (pmc.name == "PluginPresets") _pluginPresetLock = pmc.lockParams;
                if (pmc.name == "SkinPresets") _skinPresetLock = pmc.lockParams;
                if (pmc.name == "MorphPresets") _morphPresetLock = pmc.lockParams;
                if (pmc.name == "HairPresets") _hairPresetLock = pmc.lockParams;
                if (pmc.name == "ClothingPresets") _clothingPresetLock = pmc.lockParams;

                if (pmc.name == "ClothingPresets" && lockClothingPreset) pmc.lockParams = true;
                else if (clearAllLocks)
                {
                    pmc.lockParams = false;
                }
            }

        }
        public void RestorePresetLocks(Atom atom)
        {
            List<PresetManagerControl> pmControlList = atom.presetManagerControls;
            foreach (PresetManagerControl pmc in pmControlList)
            {
                if (pmc.name == "geometry") pmc.lockParams = _generalPresetLock;
                if (pmc.name == "AppearancePresets") pmc.lockParams = _appPresetLock;
                if (pmc.name == "PosePresets") pmc.lockParams = _posePresetLock;
                if (pmc.name == "AnimationPresets") pmc.lockParams = _animationPresetLock;
                if (pmc.name == "FemaleGlutePhysicsPresets") pmc.lockParams = _glutePhysPresetLock;
                if (pmc.name == "FemaleBreastPhysicsPresets") pmc.lockParams = _breastPhysPresetLock;
                if (pmc.name == "PluginPresets") pmc.lockParams = _pluginPresetLock;
                if (pmc.name == "SkinPresets") pmc.lockParams = _skinPresetLock;
                if (pmc.name == "MorphPresets") pmc.lockParams = _morphPresetLock;
                if (pmc.name == "HairPresets") pmc.lockParams = _hairPresetLock;
                if (pmc.name == "ClothingPresets") pmc.lockParams = _clothingPresetLock;
            }
        }
    }

    public class NodePhysicsXYZAngleElement : JSONStorableObject
    {
        public JSONStorableEnumStringChooser modifyXJSEnum;
        public JSONStorableEnumStringChooser modifyYJSEnum;
        public JSONStorableEnumStringChooser modifyZJSEnum;

        public JSONStorableFloat xAngleJSF;
        public JSONStorableFloat yAngleJSF;
        public JSONStorableFloat zAngleJSF;

        public NodePhysicsXYZAngleElement(string contextLabel)
        {
            modifyXJSEnum = new JSONStorableEnumStringChooser("ModifyXAngle", "NodePhysicsModificationMode", NodePhysicsModificationMode.noChange, "Modify " + contextLabel + "XTarget");
            modifyYJSEnum = new JSONStorableEnumStringChooser("ModifyYAngle", "NodePhysicsModificationMode", NodePhysicsModificationMode.noChange, "Modify " + contextLabel + "YTarget");
            modifyZJSEnum = new JSONStorableEnumStringChooser("ModifyZAngle", "NodePhysicsModificationMode", NodePhysicsModificationMode.noChange, "Modify " + contextLabel + "ZTarget");

            xAngleJSF = new JSONStorableFloat(contextLabel + "XTarget", 0f, -180f, +180f);          
            yAngleJSF = new JSONStorableFloat(contextLabel + "YTarget", 0f, -180f, +180f);
            zAngleJSF = new JSONStorableFloat(contextLabel + "ZTarget", 0f, -180f, +180f);
            RegisterParam(modifyXJSEnum);
            RegisterParam(modifyYJSEnum);
            RegisterParam(modifyZJSEnum);
            RegisterParam(xAngleJSF);
            RegisterParam(yAngleJSF);
            RegisterParam(zAngleJSF);
        }

        public bool IsMatchingState(JSONStorable nodeStorable)
        {
            if (modifyXJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeXAngle = nodeStorable.GetFloatJSONParam(xAngleJSF.name);
                if (modifyXJSEnum.val == NodePhysicsModificationMode.defaultValue && nodeXAngle.val != nodeXAngle.defaultVal) return false;
                if (modifyXJSEnum.val == NodePhysicsModificationMode.customValue && nodeXAngle.val != xAngleJSF.val) return false;
            }
            if (modifyYJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeYAngle = nodeStorable.GetFloatJSONParam(yAngleJSF.name);
                if (modifyYJSEnum.val == NodePhysicsModificationMode.defaultValue && nodeYAngle.val != nodeYAngle.defaultVal) return false;
                if (modifyYJSEnum.val == NodePhysicsModificationMode.customValue && nodeYAngle.val != yAngleJSF.val) return false;
            }
            if (modifyZJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeZAngle = nodeStorable.GetFloatJSONParam(zAngleJSF.name);
                if (modifyZJSEnum.val == NodePhysicsModificationMode.defaultValue && nodeZAngle.val != nodeZAngle.defaultVal) return false;
                if (modifyZJSEnum.val == NodePhysicsModificationMode.customValue && nodeZAngle.val != zAngleJSF.val) return false;
            }

            return true;
        }

        public void SetNodePhysicsAction(JSONStorable nodeStorable)
        {
            if (modifyXJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeXAngle = nodeStorable.GetFloatJSONParam(xAngleJSF.name);
                if (modifyXJSEnum.val == NodePhysicsModificationMode.defaultValue) nodeXAngle.SetValToDefault();
                if (modifyXJSEnum.val == NodePhysicsModificationMode.customValue) nodeXAngle.val = xAngleJSF.val;
            }
            if (modifyYJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeYAngle = nodeStorable.GetFloatJSONParam(yAngleJSF.name);
                if (modifyYJSEnum.val == NodePhysicsModificationMode.defaultValue) nodeYAngle.SetValToDefault();
                if (modifyYJSEnum.val == NodePhysicsModificationMode.customValue) nodeYAngle.val = yAngleJSF.val;
            }
            if (modifyZJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeZAngle = nodeStorable.GetFloatJSONParam(zAngleJSF.name);
                if (modifyZJSEnum.val == NodePhysicsModificationMode.defaultValue) nodeZAngle.SetValToDefault();
                if (modifyZJSEnum.val == NodePhysicsModificationMode.customValue) nodeZAngle.val = zAngleJSF.val;
            }

        }
    }
    public class NodePhysicsThesholdSprintDamperElement : JSONStorableObject
    {
        public JSONStorableEnumStringChooser modifyJSEnum;

        public JSONStorableFloat springJSF;
        public JSONStorableFloat damperJSF;
        public JSONStorableFloat thresholdJSF;

        public NodePhysicsThesholdSprintDamperElement(string contextLabel)
        {
            modifyJSEnum = new JSONStorableEnumStringChooser("Modify", "NodePhysicsModificationMode", NodePhysicsModificationMode.noChange,  "Modify "+contextLabel);

            if (contextLabel == "complyPosition")
            {
                springJSF = new JSONStorableFloat(contextLabel + "Spring", 1500f, 0f, 10000f, false);
                damperJSF = new JSONStorableFloat(contextLabel + "Damper", 100f, 0f, 1000f, false);
                thresholdJSF = new JSONStorableFloat(contextLabel + "Threshold", 0.001f, 0.0001f, 0.1f, true);
            }
            else
            {
                springJSF = new JSONStorableFloat(contextLabel + "Spring", 150f, 0f, 1000f, false);
                damperJSF = new JSONStorableFloat(contextLabel + "Damper", 10f, 0f, 100f, false);
                thresholdJSF = new JSONStorableFloat(contextLabel + "Threshold", 5f, 0.1f, 30f, true);
            }
            RegisterParam(modifyJSEnum);
            RegisterParam(springJSF);
            RegisterParam(damperJSF);
            RegisterParam(thresholdJSF);
        }

        public bool IsMatchingState(JSONStorable nodeStorable)
        {
            if (modifyJSEnum.val == NodePhysicsModificationMode.noChange) return true;

            JSONStorableFloat nodeSpringJSF = nodeStorable.GetFloatJSONParam(springJSF.name);
            JSONStorableFloat nodeDamperJSF = nodeStorable.GetFloatJSONParam(damperJSF.name);
            JSONStorableFloat nodeThresholdJSF = nodeStorable.GetFloatJSONParam(thresholdJSF.name);

            if (modifyJSEnum.val == NodePhysicsModificationMode.defaultValue)
            {
                if (nodeSpringJSF.val != nodeSpringJSF.defaultVal) return false;
                if (nodeDamperJSF.val != nodeDamperJSF.defaultVal) return false;
                if (nodeThresholdJSF.val != nodeThresholdJSF.defaultVal) return false;
            }
            else if (modifyJSEnum.val == NodePhysicsModificationMode.customValue)
            {
                if (nodeSpringJSF.val != springJSF.val) return false;
                if (nodeDamperJSF.val != damperJSF.val) return false;
                if (nodeThresholdJSF.val != thresholdJSF.val) return false;
            }

            return true;
        }

        public void SetNodePhysicsAction(JSONStorable nodeStorable)
        {
            if (modifyJSEnum.val == NodePhysicsModificationMode.noChange) return;

            JSONStorableFloat nodeSpringJSF = nodeStorable.GetFloatJSONParam(springJSF.name);
            JSONStorableFloat nodeDamperJSF = nodeStorable.GetFloatJSONParam(damperJSF.name);
            JSONStorableFloat nodeThresholdJSF = nodeStorable.GetFloatJSONParam(thresholdJSF.name);

            if (modifyJSEnum.val == NodePhysicsModificationMode.defaultValue)
            {
                nodeSpringJSF.SetValToDefault();
                nodeDamperJSF.SetValToDefault();
                nodeDamperJSF.SetValToDefault();

            }
            else if (modifyJSEnum.val == NodePhysicsModificationMode.customValue)
            {
                nodeSpringJSF.val = springJSF.val;
                nodeDamperJSF.val = damperJSF.val;
                nodeDamperJSF.val = thresholdJSF.val;
            }
        }
    }

    public class NodePhysicsSpringDamperForceElement : JSONStorableObject
    {
        public JSONStorableEnumStringChooser modifyJSEnum;

        public JSONStorableFloat springJSF;
        public JSONStorableFloat damperJSF;
        public JSONStorableFloat maxForceJSF;

        public NodePhysicsSpringDamperForceElement(string contextLabel)
        {
            modifyJSEnum = new JSONStorableEnumStringChooser("Modify", "NodePhysicsModificationMode", NodePhysicsModificationMode.noChange, "Modify " + contextLabel);
            if (contextLabel=="holdRotation")
            {
                springJSF = new JSONStorableFloat(contextLabel + "Spring", 100f, 0f, 1000f, false);
                damperJSF = new JSONStorableFloat(contextLabel + "Damper", 5f, 0f, 10f, false);
                maxForceJSF = new JSONStorableFloat(contextLabel + "MaxForce", 1000f, 0f, 1000f, false);
            }
            else if (contextLabel == "linkRotation" || contextLabel == "linkPosition")
            {
                springJSF = new JSONStorableFloat(contextLabel + "Spring", 100000f, 0f, 100000f, false);
                damperJSF = new JSONStorableFloat(contextLabel + "Damper", 250f, 0f, 1000f, false);
                maxForceJSF = new JSONStorableFloat(contextLabel + "MaxForce", 100000f, 0f, 100000f, false);
            }
            else if (contextLabel == "jointDrive")
            {
                springJSF = new JSONStorableFloat(contextLabel + "Spring", 25f, 0f, 200f, false);
                damperJSF = new JSONStorableFloat(contextLabel + "Damper", 0.5f, 0f, 10f, false);
                maxForceJSF = new JSONStorableFloat(contextLabel + "MaxForce", 5f, 0f, 100f, false);
            }
            else
            {
                springJSF = new JSONStorableFloat(contextLabel + "Spring", 2000f, 0f, 10000f, false);
                damperJSF = new JSONStorableFloat(contextLabel + "Damper", 35f, 0f, 100f, false);
                maxForceJSF = new JSONStorableFloat(contextLabel + "MaxForce", 1000f, 0f, 10000f, false);
            }
            
            RegisterParam(modifyJSEnum);
            RegisterParam(springJSF);
            RegisterParam(damperJSF);
            RegisterParam(maxForceJSF);
        }
        public bool IsMatchingState(JSONStorable nodeStorable)
        {
            if (modifyJSEnum.val == NodePhysicsModificationMode.noChange) return true;

            JSONStorableFloat nodeSpringJSF = nodeStorable.GetFloatJSONParam(springJSF.name);
            JSONStorableFloat nodeDamperJSF = nodeStorable.GetFloatJSONParam(damperJSF.name);
            JSONStorableFloat nodeMaxForceJSF = nodeStorable.GetFloatJSONParam(maxForceJSF.name);

            if (modifyJSEnum.val== NodePhysicsModificationMode.defaultValue)
            {
                if (nodeSpringJSF.val != nodeSpringJSF.defaultVal) return false;
                if (nodeDamperJSF.val != nodeDamperJSF.defaultVal) return false;
                if (nodeMaxForceJSF.val != nodeMaxForceJSF.defaultVal) return false;
            }
            else if (modifyJSEnum.val == NodePhysicsModificationMode.customValue)
            {
                if (nodeSpringJSF.val != springJSF.val) return false;
                if (nodeDamperJSF.val != damperJSF.val) return false;
                if (nodeMaxForceJSF.val != maxForceJSF.val) return false;
            }
            return true;
        }
        public void SetNodePhysicsAction(JSONStorable nodeStorable)
        {
            if (modifyJSEnum.val == NodePhysicsModificationMode.noChange) return;

            JSONStorableFloat nodeSpringJSF = nodeStorable.GetFloatJSONParam(springJSF.name);
            JSONStorableFloat nodeDamperJSF = nodeStorable.GetFloatJSONParam(damperJSF.name);
            JSONStorableFloat nodeMaxForceJSF = nodeStorable.GetFloatJSONParam(maxForceJSF.name);

            if (modifyJSEnum.val == NodePhysicsModificationMode.defaultValue)
            {
                nodeSpringJSF.SetValToDefault();
                nodeDamperJSF.SetValToDefault();
                nodeMaxForceJSF.SetValToDefault();

            }
            else if (modifyJSEnum.val == NodePhysicsModificationMode.customValue)
            {
                nodeSpringJSF.val = springJSF.val;
                nodeDamperJSF.val = damperJSF.val;
                nodeMaxForceJSF.val = maxForceJSF.val;
            }
        }

    }

    public class NodePhysicsPositionRotationSDF : JSONStorableObject
    {
        public NodePhysicsSpringDamperForceElement positionSpringDamperForce;
        public NodePhysicsSpringDamperForceElement rotationSpringDamperForce;

        public NodePhysicsPositionRotationSDF(string contextLabel)
        {
            positionSpringDamperForce = new NodePhysicsSpringDamperForceElement(contextLabel + "Position");
            rotationSpringDamperForce = new NodePhysicsSpringDamperForceElement(contextLabel + "Rotation");
        }
        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);
            jc["positionSpringDamperForce"] = positionSpringDamperForce.GetJSON();
            jc["rotationSpringDamperForce"] = rotationSpringDamperForce.GetJSON();
            return jc;
        }

        public void LoadJSON(JSONClass jc)
        {
            base.RestoreFromJSON(jc);
            if (jc["positionSpringDamperForce"] != null) positionSpringDamperForce.RestoreFromJSON((JSONClass)jc["positionSpringDamperForce"]);
            if (jc["rotationSpringDamperForce"] != null) rotationSpringDamperForce.RestoreFromJSON((JSONClass)jc["rotationSpringDamperForce"]);
        }

        public void CopyFrom(NodePhysicsPositionRotationSDF sourceComponent, string contextLabel)
        {
            base.CopyFrom(sourceComponent);
            positionSpringDamperForce = new NodePhysicsSpringDamperForceElement(contextLabel + "Position");
            rotationSpringDamperForce = new NodePhysicsSpringDamperForceElement(contextLabel + "Rotation");
            positionSpringDamperForce.CopyFrom(sourceComponent.positionSpringDamperForce);
            rotationSpringDamperForce.CopyFrom(sourceComponent.rotationSpringDamperForce);
        }

        public bool IsMatchingState(JSONStorable nodeStorable)
        {
            if (!positionSpringDamperForce.IsMatchingState(nodeStorable)) return false;
            if (!rotationSpringDamperForce.IsMatchingState(nodeStorable)) return false;
            return true;
        }

        public void SetNodePhysicsAction(JSONStorable nodeStorable)
        {
            positionSpringDamperForce.SetNodePhysicsAction(nodeStorable);
            rotationSpringDamperForce.SetNodePhysicsAction(nodeStorable);

        }

    }
    public class NodePhysicsCompliance : JSONStorableObject
    {
        public NodePhysicsThesholdSprintDamperElement positionComplianceNodePhysics;
        public NodePhysicsThesholdSprintDamperElement rotationComplianceNodePhysics;

        public JSONStorableEnumStringChooser modifyComplianceSpeedJSEnum;
        public JSONStorableFloat complianceSpeedJSF;
        public JSONStorableEnumStringChooser modifyJointDriveSpringJSEnum;
        public JSONStorableFloat jointDriveSpringJSF;

        public NodePhysicsCompliance(string contextLabel)
        {
            positionComplianceNodePhysics = new NodePhysicsThesholdSprintDamperElement(contextLabel + "Position");
            rotationComplianceNodePhysics = new NodePhysicsThesholdSprintDamperElement(contextLabel + "Rotation");
            modifyComplianceSpeedJSEnum = new JSONStorableEnumStringChooser("ModifyComplianceSpeed", "NodePhysicsModificationMode", NodePhysicsModificationMode.noChange, "Modify " + contextLabel +"Speed");
            complianceSpeedJSF = new JSONStorableFloat(contextLabel + "Speed", 10f, 0f, 100f, true);
            modifyJointDriveSpringJSEnum = new JSONStorableEnumStringChooser("ModifyJointDriveSpring", "NodePhysicsModificationMode", NodePhysicsModificationMode.noChange, "Modify " + contextLabel + " JointDriveSpring");
            jointDriveSpringJSF = new JSONStorableFloat(contextLabel + "JointDriveSpring", 20f, 0f, 100f, false);
            RegisterParam(modifyComplianceSpeedJSEnum);
            RegisterParam(complianceSpeedJSF);
            RegisterParam(modifyJointDriveSpringJSEnum);
            RegisterParam(jointDriveSpringJSF);
        }
        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);
            jc["positionComplianceNodePhysics"] = positionComplianceNodePhysics.GetJSON();
            jc["rotationComplianceNodePhysics"] = rotationComplianceNodePhysics.GetJSON();
            return jc;
        }

        public void LoadJSON(JSONClass jc)
        {
            base.RestoreFromJSON(jc);
            if (jc["positionComplianceNodePhysics"] != null) positionComplianceNodePhysics.RestoreFromJSON((JSONClass)jc["positionComplianceNodePhysics"]);
            if (jc["rotationComplianceNodePhysics"] != null) rotationComplianceNodePhysics.RestoreFromJSON((JSONClass)jc["rotationComplianceNodePhysics"]);
        }

        public void CopyFrom(NodePhysicsCompliance sourceComponent, string contextLabel)
        {
            base.CopyFrom(sourceComponent);
            positionComplianceNodePhysics = new NodePhysicsThesholdSprintDamperElement(contextLabel + "Position");
            rotationComplianceNodePhysics = new NodePhysicsThesholdSprintDamperElement(contextLabel + "Rotation");
            positionComplianceNodePhysics.CopyFrom(sourceComponent.positionComplianceNodePhysics);
            rotationComplianceNodePhysics.CopyFrom(sourceComponent.rotationComplianceNodePhysics);
        }
        public bool IsMatchingState(JSONStorable nodeStorable)
        {
            if (modifyComplianceSpeedJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeComplianceSpeed = nodeStorable.GetFloatJSONParam(complianceSpeedJSF.name);
                if (modifyComplianceSpeedJSEnum.val == NodePhysicsModificationMode.defaultValue && nodeComplianceSpeed.val != nodeComplianceSpeed.defaultVal) return false;
                if (modifyComplianceSpeedJSEnum.val == NodePhysicsModificationMode.customValue && nodeComplianceSpeed.val != complianceSpeedJSF.val) return false;
            }
            if (modifyJointDriveSpringJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeJointDriveSpring = nodeStorable.GetFloatJSONParam(jointDriveSpringJSF.name);
                if (modifyJointDriveSpringJSEnum.val == NodePhysicsModificationMode.defaultValue && nodeJointDriveSpring.val != nodeJointDriveSpring.defaultVal) return false;
                if (modifyJointDriveSpringJSEnum.val == NodePhysicsModificationMode.customValue && nodeJointDriveSpring.val != jointDriveSpringJSF.val) return false;
            }

            if (!positionComplianceNodePhysics.IsMatchingState(nodeStorable)) return false;
            if (!rotationComplianceNodePhysics.IsMatchingState(nodeStorable)) return false;

            return true;
        }
        public void SetNodePhysicsAction(JSONStorable nodeStorable)
        {
            if (modifyComplianceSpeedJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeComplianceSpeed = nodeStorable.GetFloatJSONParam(complianceSpeedJSF.name);
                if (modifyComplianceSpeedJSEnum.val == NodePhysicsModificationMode.defaultValue)  nodeComplianceSpeed.SetValToDefault();
                if (modifyComplianceSpeedJSEnum.val == NodePhysicsModificationMode.customValue) nodeComplianceSpeed.val = complianceSpeedJSF.val;
            }
            if (modifyJointDriveSpringJSEnum.val != NodePhysicsModificationMode.noChange)
            {
                JSONStorableFloat nodeJointDriveSpring = nodeStorable.GetFloatJSONParam(jointDriveSpringJSF.name);
                if (modifyJointDriveSpringJSEnum.val == NodePhysicsModificationMode.defaultValue)nodeJointDriveSpring.SetValToDefault() ;
                if (modifyJointDriveSpringJSEnum.val == NodePhysicsModificationMode.customValue) nodeJointDriveSpring.val = jointDriveSpringJSF.val;
            }
            positionComplianceNodePhysics.SetNodePhysicsAction(nodeStorable);
            rotationComplianceNodePhysics.SetNodePhysicsAction(nodeStorable);

        }


    }
    public class NodePhysicsComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public NodePhysicsPositionRotationSDF holdNodePhysics;
        public NodePhysicsPositionRotationSDF linkNodePhysics;
        public NodePhysicsSpringDamperForceElement jointDriveSDFPhysics;
        public NodePhysicsXYZAngleElement jointDriveXYZPhysics;
        public NodePhysicsCompliance nodePhysicsCompliance;

        public JSONStorableEnumStringChooser modifyMaxVelocityJSEnum;
        public JSONStorableBool maxVelocityEnabledJSB;
        public JSONStorableFloat maxVelocityJSF;

        public NodePhysicsComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;
            holdNodePhysics = new NodePhysicsPositionRotationSDF("hold");
            linkNodePhysics = new NodePhysicsPositionRotationSDF("link");
            jointDriveSDFPhysics = new NodePhysicsSpringDamperForceElement("jointDrive");
            jointDriveXYZPhysics = new NodePhysicsXYZAngleElement("jointDrive");
            nodePhysicsCompliance = new NodePhysicsCompliance("comply");

            modifyMaxVelocityJSEnum = new JSONStorableEnumStringChooser("ModifyMaxVelocity", "NodePhysicsModificationMode", NodePhysicsModificationMode.noChange, "Modify Max Velocity Physics");
            maxVelocityEnabledJSB = new JSONStorableBool("maxVelocityEnable", true);
            maxVelocityJSF = new JSONStorableFloat("maxVelocity", 10f, 0f, 100f, false);
            RegisterParam(modifyMaxVelocityJSEnum);
            RegisterParam(maxVelocityEnabledJSB);
            RegisterParam(maxVelocityJSF);
        }

        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);
            jc["holdNodePhysics"] = holdNodePhysics.GetJSON();
            jc["linkNodePhysics"] = linkNodePhysics.GetJSON();
            jc["jointDriveSDFPhysics"] = jointDriveSDFPhysics.GetJSON();
            jc["jointDriveXYZPhysics"] = jointDriveXYZPhysics.GetJSON();
            jc["nodePhysicsCompliance"] = nodePhysicsCompliance.GetJSON();
            return jc;
        }

        public void LoadJSON(JSONClass jc)
        {
            base.RestoreFromJSON(jc);
            if (jc["holdNodePhysics"] != null) holdNodePhysics.LoadJSON((JSONClass)jc["holdNodePhysics"]);
            if (jc["linkNodePhysics"] != null) linkNodePhysics.LoadJSON((JSONClass)jc["linkNodePhysics"]);
            if (jc["jointDriveSDFPhysics"] != null) jointDriveSDFPhysics.RestoreFromJSON((JSONClass)jc["jointDriveSDFPhysics"]);
            if (jc["jointDriveXYZPhysics"] != null) jointDriveXYZPhysics.RestoreFromJSON((JSONClass)jc["jointDriveXYZPhysics"]);
            if (jc["nodePhysicsCompliance"] != null) nodePhysicsCompliance.LoadJSON((JSONClass)jc["nodePhysicsCompliance"]);
        }

        public void CopyFrom(NodePhysicsComponent sourceComponent)
        {
            base.CopyFrom(sourceComponent);
            holdNodePhysics = new NodePhysicsPositionRotationSDF("hold");
            linkNodePhysics = new NodePhysicsPositionRotationSDF("link");
            jointDriveSDFPhysics = new NodePhysicsSpringDamperForceElement("jointDrive");
            jointDriveXYZPhysics = new NodePhysicsXYZAngleElement("jointDrive");
            nodePhysicsCompliance = new NodePhysicsCompliance("comply");

            holdNodePhysics.CopyFrom(sourceComponent.holdNodePhysics, "hold");
            linkNodePhysics.CopyFrom(sourceComponent.linkNodePhysics, "link");
            jointDriveSDFPhysics.CopyFrom(sourceComponent.jointDriveSDFPhysics);
            jointDriveXYZPhysics.CopyFrom(sourceComponent.jointDriveXYZPhysics);
            nodePhysicsCompliance.CopyFrom(sourceComponent.nodePhysicsCompliance, "comply");
        }

        public bool IsMatchingState(Atom atom, List<string> activeNodeSelections)
        {
            
            if (activeNodeSelections.Count == 0) return false;
            foreach(var nodeName in activeNodeSelections)
            {
                JSONStorable nodeStorable = atom.GetStorableByID(nodeName);
                if (modifyMaxVelocityJSEnum.val !=NodePhysicsModificationMode.noChange)
                {
                    JSONStorableBool nodeMaxVelocityEnabled = nodeStorable.GetBoolJSONParam("maxVelocityEnabled");
                    JSONStorableFloat nodeMaxVelocity = nodeStorable.GetFloatJSONParam("maxVelocity");
                    if (modifyMaxVelocityJSEnum.val == NodePhysicsModificationMode.defaultValue)
                    {                        
                        if (nodeMaxVelocityEnabled.val != nodeMaxVelocityEnabled.defaultVal) return false;                        
                        if (nodeMaxVelocity.val != nodeMaxVelocity.defaultVal) return false;

                    }
                    if (modifyMaxVelocityJSEnum.val == NodePhysicsModificationMode.customValue)
                    {
                        if (nodeMaxVelocityEnabled.val != maxVelocityEnabledJSB.val) return false;
                        if (nodeMaxVelocity.val != nodeMaxVelocity.val) return false;
                    }

                }
                if (!holdNodePhysics.IsMatchingState(nodeStorable)) return false;
                if (!linkNodePhysics.IsMatchingState(nodeStorable)) return false;
                if (nodeName != "hipControl" && !jointDriveSDFPhysics.IsMatchingState(nodeStorable)) return false;
                if (nodeName != "hipControl" && !jointDriveXYZPhysics.IsMatchingState(nodeStorable)) return false;
                if (!nodePhysicsCompliance.IsMatchingState(nodeStorable)) return false;
            }
            return true;
        }
        public void SetNodePhysicsAction(Atom targetAtom, List<string> activeNodeSelections)
        {
            if (activeNodeSelections.Count == 0) return ;
            foreach (var nodeName in activeNodeSelections)
            {
                JSONStorable nodeStorable = targetAtom.GetStorableByID(nodeName);
                if (modifyMaxVelocityJSEnum.val != NodePhysicsModificationMode.noChange)
                {
                    JSONStorableBool nodeMaxVelocityEnabled = nodeStorable.GetBoolJSONParam("maxVelocityEnabled");
                    JSONStorableFloat nodeMaxVelocity = nodeStorable.GetFloatJSONParam("maxVelocity");
                    if (modifyMaxVelocityJSEnum.val == NodePhysicsModificationMode.defaultValue)
                    {
                        nodeMaxVelocityEnabled.SetValToDefault();
                        nodeMaxVelocity.SetValToDefault();
                    }
                    if (modifyMaxVelocityJSEnum.val == NodePhysicsModificationMode.customValue)
                    {
                        nodeMaxVelocityEnabled.val = maxVelocityEnabledJSB.val;
                        nodeMaxVelocity.val = nodeMaxVelocity.val;
                    }

                }
                holdNodePhysics.SetNodePhysicsAction(nodeStorable);
                linkNodePhysics.SetNodePhysicsAction(nodeStorable);
                if (nodeName != "hipControl" && nodeName != "control")
                {
                    jointDriveSDFPhysics.SetNodePhysicsAction(nodeStorable);
                    jointDriveXYZPhysics.SetNodePhysicsAction(nodeStorable);
                }
                if (nodeName != "control") nodePhysicsCompliance.SetNodePhysicsAction(nodeStorable);
            }


        }

    }
    public class NodeSelectionComponent : ButtonOperationComponentBase
    {
        private Dictionary<string, JSONStorableBool> nodeSelectionDict = new Dictionary<string, JSONStorableBool>();

        public NodeSelectionComponent(UIAButtonOperation parent) : base(parent)
        {
            foreach (string nodeName in UIAConsts.headFCs) InitNodeName(nodeName);
            foreach (string nodeName in UIAConsts.bodyFCs) InitNodeName(nodeName);
            foreach (string nodeName in UIAConsts.genFCs) InitNodeName(nodeName);
            foreach (string nodeName in UIAConsts.limbFCs) InitNodeName(nodeName);
        }

        private void InitNodeName(string nodeName)
        {
            nodeSelectionDict.Add(nodeName, new JSONStorableBool(nodeName,false));
            RegisterParam(nodeSelectionDict[nodeName]);
        }

        public List<string> GetActiveNodeSelections()
        {
            var nodeControlSelections = new List<string>();
            foreach (string nodeName in UIAConsts.bodyFCs)
            {
                if (nodeSelectionDict[nodeName].val) nodeControlSelections.Add(nodeName);
            }
            foreach (string nodeName in UIAConsts.headFCs)
            {
                if (nodeSelectionDict[nodeName].val) nodeControlSelections.Add(nodeName);
            }
            foreach (string nodeName in UIAConsts.limbFCs)
            {
                if (nodeSelectionDict[nodeName].val) nodeControlSelections.Add(nodeName);
            }
            foreach (string nodeName in UIAConsts.genFCs)
            {
                if (nodeSelectionDict[nodeName].val) nodeControlSelections.Add(nodeName);
            }
            return nodeControlSelections;

        }
        public List<JSONStorableBool> GetNodeControlSelections(int nodeBodyRegion)
        {
            var nodeControlSelections = new List<JSONStorableBool>();

            var nodeNames = new List<string>();
            if (nodeBodyRegion == NodeBodyRegion.torso) nodeNames = UIAConsts.bodyFCs;
            if (nodeBodyRegion == NodeBodyRegion.head) nodeNames = UIAConsts.headFCs;
            if (nodeBodyRegion == NodeBodyRegion.gens) nodeNames = UIAConsts.genFCs;
            if (nodeBodyRegion == NodeBodyRegion.limbs) nodeNames = UIAConsts.limbFCs;

            foreach (var nodeName in nodeNames)
            {
                nodeControlSelections.Add(nodeSelectionDict[nodeName]);
            }

            return nodeControlSelections;
        }
    }
    public class NodeControlComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableEnumStringChooser multiNodeControlStateJSEnum;

        private Dictionary<string, JSONStorableEnumStringChooser> customNodeControlStateDict = new Dictionary<string, JSONStorableEnumStringChooser>();

        private JSONStorableEnumStringChooser rootNodeControlState;

        public NodeControlComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;
            multiNodeControlStateJSEnum = new JSONStorableEnumStringChooser("multiNodeControlStateOn", MultiNodeControlState.enumManifestName, MultiNodeControlState.custom, "Control Preset");
            RegisterParam(multiNodeControlStateJSEnum);

            foreach (string nodeName in UIAConsts.headFCs) InitNodeName(nodeName);
            foreach (string nodeName in UIAConsts.bodyFCs) InitNodeName(nodeName);
            foreach (string nodeName in UIAConsts.genFCs) InitNodeName(nodeName);
            foreach (string nodeName in UIAConsts.limbFCs) InitNodeName(nodeName);

            rootNodeControlState = new JSONStorableEnumStringChooser("rootNode", NodeControlState.enumManifestName, NodeControlState.on, "Control State");
            RegisterParam(rootNodeControlState);
        }

        private void SetControlState(FreeControllerV3 fc, int targetState,bool isRotation)
        {
            if (isRotation) fc.currentRotationState = GetEquivalentFC3Rotation(targetState);
            else fc.currentPositionState = GetEquivalentFC3Position(targetState);
        }
        public void SetControlStateAction(Atom targetAtom, bool isRotation)
        {
            if (multiNodeControlStateJSEnum.val != MultiNodeControlState.custom)
            {
                List<string> joints = targetMultiNodeControlJointNames;
                int targetState = targetMultiNodeControlState;

                foreach (var fc in targetAtom.freeControllers)
                {
                    if (joints.Contains(fc.name)) SetControlState(fc, targetState, isRotation);
                    else SetControlState(fc, NodeControlState.off, isRotation);
                }
            }
            else
            {
                Dictionary<string, FreeControllerV3> atomFCDict = new Dictionary<string, FreeControllerV3>();
                foreach (var fc in targetAtom.freeControllers) atomFCDict.Add(fc.name, fc);

                foreach (var kvp in customNodeControlStateDict)
                {
                    if (kvp.Value.val != NodeControlState.noChange) SetControlState(atomFCDict[kvp.Key], kvp.Value.val,isRotation);
                }
            }
        }

        private int targetMultiNodeControlState
        {
            get
            {
                if (multiNodeControlStateJSEnum.val == MultiNodeControlState.complyAllJoints || multiNodeControlStateJSEnum.val == MultiNodeControlState.complyKeyElbowKneeJoints || multiNodeControlStateJSEnum.val == MultiNodeControlState.complyKeyJoints) return NodeControlState.comply;
                if (multiNodeControlStateJSEnum.val == MultiNodeControlState.offAllJoints) return NodeControlState.off;
                return NodeControlState.on;
            }
        }

        private List<string> targetMultiNodeControlJointNames
        {
            get
            {
                if (multiNodeControlStateJSEnum.val == MultiNodeControlState.complyKeyJoints || multiNodeControlStateJSEnum.val == MultiNodeControlState.onKeyJoints) return UIAConsts.keyJointFCs;
                if (multiNodeControlStateJSEnum.val == MultiNodeControlState.complyKeyElbowKneeJoints || multiNodeControlStateJSEnum.val == MultiNodeControlState.onKeyElbowKneeJoints) return UIAConsts.keyElbowKneeJointFCs;
                return UIAConsts.allJointFCs;
            }
        }

        private FreeControllerV3.RotationState GetEquivalentFC3Rotation(int nodeState)
        {
            if (nodeState == NodeControlState.on) return FreeControllerV3.RotationState.On ;
            if (nodeState == NodeControlState.off) return FreeControllerV3.RotationState.Off;
            if (nodeState == NodeControlState.comply) return FreeControllerV3.RotationState.Comply;
            if (nodeState == NodeControlState.parentLink) return FreeControllerV3.RotationState.ParentLink;
            if (nodeState == NodeControlState.physicsLink ) return FreeControllerV3.RotationState.PhysicsLink;
            if (nodeState == NodeControlState.lockNode ) return FreeControllerV3.RotationState.Lock;
            if (nodeState == NodeControlState.hold ) return FreeControllerV3.RotationState.Hold;
            return FreeControllerV3.RotationState.Off;
        }
        private FreeControllerV3.PositionState GetEquivalentFC3Position(int nodeState)
        {
            if (nodeState == NodeControlState.on) return FreeControllerV3.PositionState.On;
            if (nodeState == NodeControlState.off) return FreeControllerV3.PositionState.Off;
            if (nodeState == NodeControlState.comply) return FreeControllerV3.PositionState.Comply;
            if (nodeState == NodeControlState.parentLink) return FreeControllerV3.PositionState.ParentLink;
            if (nodeState == NodeControlState.physicsLink) return FreeControllerV3.PositionState.PhysicsLink;
            if (nodeState == NodeControlState.lockNode) return FreeControllerV3.PositionState.Lock;
            if (nodeState == NodeControlState.hold) return FreeControllerV3.PositionState.Hold;
            return FreeControllerV3.PositionState.Off;
        }
        private int GetEquivalentNodeControlState(FreeControllerV3.RotationState fc3RotationState)
        {
            if (fc3RotationState == FreeControllerV3.RotationState.On) return NodeControlState.on;
            if (fc3RotationState == FreeControllerV3.RotationState.Off) return NodeControlState.off;
            if (fc3RotationState == FreeControllerV3.RotationState.Comply) return NodeControlState.comply;
            if (fc3RotationState == FreeControllerV3.RotationState.ParentLink) return NodeControlState.parentLink;
            if (fc3RotationState == FreeControllerV3.RotationState.PhysicsLink) return NodeControlState.physicsLink;
            if (fc3RotationState == FreeControllerV3.RotationState.Lock) return NodeControlState.lockNode;
            if (fc3RotationState == FreeControllerV3.RotationState.Hold) return NodeControlState.hold;
            return -1;
        }
        private int GetEquivalentNodeControlState(FreeControllerV3.PositionState fc3RotationState)
        {
            if (fc3RotationState == FreeControllerV3.PositionState.On) return NodeControlState.on;
            if (fc3RotationState == FreeControllerV3.PositionState.Off) return NodeControlState.off;
            if (fc3RotationState == FreeControllerV3.PositionState.Comply) return NodeControlState.comply;
            if (fc3RotationState == FreeControllerV3.PositionState.ParentLink) return NodeControlState.parentLink;
            if (fc3RotationState == FreeControllerV3.PositionState.PhysicsLink) return NodeControlState.physicsLink;
            if (fc3RotationState == FreeControllerV3.PositionState.Lock) return NodeControlState.lockNode;
            if (fc3RotationState == FreeControllerV3.PositionState.Hold) return NodeControlState.hold;
            return -1;
        }
        private bool MatchFCState(FreeControllerV3 fc3,int nodeTargetState, bool isRotation)
        {
            if (isRotation)
            {
                if (nodeTargetState!= NodeControlState.noChange && nodeTargetState != GetEquivalentNodeControlState(fc3.currentRotationState)) return false;
            }
            else
            {
                if (nodeTargetState != NodeControlState.noChange && nodeTargetState != GetEquivalentNodeControlState(fc3.currentPositionState)) return false;
            }
            return true;
        }
        public bool IsMatchingState(Atom atom, bool isRotations)
        {
            if (multiNodeControlStateJSEnum.val != MultiNodeControlState.custom)
            {
                List<string> joints = targetMultiNodeControlJointNames;              
                int targetState = targetMultiNodeControlState;

                foreach (var fc in atom.freeControllers)
                {
                    if (joints.Contains(fc.name) && !MatchFCState(fc, targetState, isRotations)) return false;
                    if (!joints.Contains(fc.name) && !MatchFCState(fc, NodeControlState.off, isRotations)) return false;
                }
            }
            else
            {
                Dictionary<string, FreeControllerV3> atomFCDict = new Dictionary<string, FreeControllerV3>();
                foreach (var fc in atom.freeControllers) atomFCDict.Add(fc.name, fc);
                foreach (var kvp in customNodeControlStateDict)
                {
                    if (!MatchFCState(atomFCDict[kvp.Key], kvp.Value.val, isRotations)) return false;
                }
            }

            return true;
        }
        private void InitNodeName(string nodeName)
        {
            customNodeControlStateDict.Add(nodeName, new JSONStorableEnumStringChooser(nodeName, NodeControlState.enumManifestName, NodeControlState.noChange, nodeName));
            RegisterParam(customNodeControlStateDict[nodeName]);
        }

        public List<JSONStorableEnumStringChooser> GetNodeControlStates(int nodeBodyRegion)
        {
            var nodeControlStates = new List<JSONStorableEnumStringChooser>();

            var nodeNames = new List<string>();
            if (nodeBodyRegion == NodeBodyRegion.torso) nodeNames = UIAConsts.bodyFCs;
            if (nodeBodyRegion == NodeBodyRegion.head) nodeNames = UIAConsts.headFCs;
            if (nodeBodyRegion == NodeBodyRegion.gens) nodeNames = UIAConsts.genFCs;
            if (nodeBodyRegion == NodeBodyRegion.limbs) nodeNames = UIAConsts.limbFCs;

            foreach (var nodeName in nodeNames) {
                nodeControlStates.Add(customNodeControlStateDict[nodeName]);
            }

            return nodeControlStates;
        }
        public JSONStorableEnumStringChooser GetRootNodeControlStates()
        {
            return rootNodeControlState;
        }
    }
    public class AppearancePresetComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableBool suppressClothingLoadJSBool;
        public JSONStorableBool onlySuppressRealClothingJSB;
        public JSONStorableBool onlyReplaceRealClothingJSB;
        public JSONStorableBool suppressPersonScaleLoadJSBool;

        public static string lastLoadSceneFromVar = "";

        public AppearancePresetComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            suppressClothingLoadJSBool = new JSONStorableBool("suppressClothingLoad", false);
            onlySuppressRealClothingJSB = new JSONStorableBool("onlySuppressRealClothing", true);
            onlyReplaceRealClothingJSB = new JSONStorableBool("onlyRemoveRealClothing", true);
            RegisterParam(suppressClothingLoadJSBool);
            RegisterParam(onlySuppressRealClothingJSB);
            RegisterParam(onlyReplaceRealClothingJSB);

            suppressPersonScaleLoadJSBool = new JSONStorableBool("suppressPersonScaleLoad", false);
            RegisterParam(suppressPersonScaleLoadJSBool);
        }

        public static void OnSceneLoaded()
        {
            string currentLoadDir = SuperController.singleton.currentLoadDir;
            if (currentLoadDir.Contains(":/Saves/scene")) lastLoadSceneFromVar = currentLoadDir;
        }

        public void PlayLegacyPresetAction(Atom atom)
        {
            FileReference fileRef = null;

            switch (buttonTypeJSEnum.val)
            {
                case UIAButtonOpType.loadLegacyLook:
                case UIAButtonOpType.saveLegacyLook:
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.legacyLookPreset];
                    break;
                case UIAButtonOpType.loadLegacyPose:
                case UIAButtonOpType.saveLegacyPose:
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.legacyPosePreset];
                    break;
                case UIAButtonOpType.loadLegacyPreset:
                case UIAButtonOpType.saveLegacyPreset:
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.legacyPreset];
                    break;
            }

            if (fileRef.currentActionSelectedFile != "" && (FileManagerSecure.FileExists(fileRef.currentActionSelectedFile) || UIAButtonOpType.IsLegacyPresetSaveType(buttonTypeJSEnum.val)))
            {
                switch (buttonTypeJSEnum.val)
                {
                    case UIAButtonOpType.saveLegacyPreset:
                    case UIAButtonOpType.saveLegacyPose:
                    case UIAButtonOpType.saveLegacyLook:
                        SuperController.singleton.Save(fileRef.currentActionSelectedFile, atom, buttonTypeJSEnum.val == UIAButtonOpType.saveLegacyPose || buttonTypeJSEnum.val == UIAButtonOpType.saveLegacyPose, buttonTypeJSEnum.val == UIAButtonOpType.saveLegacyLook || buttonTypeJSEnum.val == UIAButtonOpType.saveLegacyPose);
                        break;
                    case UIAButtonOpType.loadLegacyLook:
                        atom.LoadAppearancePreset(fileRef.currentActionSelectedFile);
                        break;
                    case UIAButtonOpType.loadLegacyPose:
                        atom.LoadPhysicalPreset(fileRef.currentActionSelectedFile);
                        break;
                    case UIAButtonOpType.loadLegacyPreset:
                        atom.LoadPreset(fileRef.currentActionSelectedFile);
                        break;
                }
            }
        }

        public void PlayPresetAction(Atom atom)
        {
            JSONStorable js = null;
            FileReference fileRef = null;

            switch (buttonTypeJSEnum.val)
            {
                case UIAButtonOpType.loadAppPreset:
                    js = atom.GetStorableByID("AppearancePresets");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.appearancePreset];
                    break;
                case UIAButtonOpType.loadAnimationPreset:
                    js = atom.GetStorableByID("AnimationPresets");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.animationPreset];
                    break;
                case UIAButtonOpType.loadPosePreset:
                    js = atom.GetStorableByID("PosePresets");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.posePreset];
                    break;
                case UIAButtonOpType.loadHairPreset:
                    js = atom.GetStorableByID("HairPresets");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.hairPreset];
                    break;
                case UIAButtonOpType.loadGlutePreset:
                    js = atom.GetStorableByID("FemaleGlutePhysicsPresets");
                    if (js == null) js = atom.GetStorableByID("MaleGlutePhysicsPresets");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.glutePreset];
                    break;
                case UIAButtonOpType.loadBreastPreset:
                    js = atom.GetStorableByID("FemaleBreastPhysicsPresets");
                    if (js == null) js = atom.GetStorableByID("MaleBreastPhysicsPresets");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.breastPreset];
                    break;
                case UIAButtonOpType.loadMorphPreset:
                    js = atom.GetStorableByID("MorphPresets");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.morphPreset];
                    break;
                case UIAButtonOpType.loadSkinPreset:
                    js = atom.GetStorableByID("SkinPresets");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.skinPreset];
                    break;
                case UIAButtonOpType.loadGeneralPreset:
                    js = atom.GetStorableByID("Preset");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.generalPreset];
                    break;
                case UIAButtonOpType.loadClothPreset:
                    js = atom.GetStorableByID("ClothingPresets");
                    fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.clothingPreset];
                    break;
                case UIAButtonOpType.loadPluginsPreset:
                    if (atom.name == "CoreControl" && atom.type == "SessionPluginManager")
                    {
                        js = SuperController.singleton.sessionPresetManagerControl;
                        fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.sessionPluginPreset];
                    }
                    else if (atom.name == "CoreControl")
                    {
                        js = atom.GetStorableByID("PluginManagerPresets");
                        fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.scenePluginPreset];
                    }
                    else
                    {
                        js = atom.GetStorableByID("PluginPresets");
                        fileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.pluginsPreset];
                    }
                    
                    break;
            }

            if (js != null && fileRef.currentActionSelectedFile != "")
            {
                if (parentButtonOperation.savePresetJSB.val) SavePreset(atom,js,fileRef) ;
                else if (FileManagerSecure.FileExists(fileRef.currentActionSelectedFile))
                {
                    if (js.name == "SkinPresets" && parentButtonOperation.skinPresetDecalComponent.AnyDecalLoadsActive() && PatreonFeatures.patreonContentEnabled) PatreonFeatures.SkinPresetDecalLoad(atom, fileRef, parentButtonOperation.skinPresetDecalComponent);
                    else LoadPreset(atom, js, fileRef, (suppressClothingLoadJSBool.val || PresetLoadSettings.suppressClothingLoadJSB.val)  && buttonTypeJSEnum.val == UIAButtonOpType.loadAppPreset, (suppressPersonScaleLoadJSBool.val || PresetLoadSettings.suppressScaleLoadJSB.val) && buttonTypeJSEnum.val == UIAButtonOpType.loadAppPreset,onlySuppressRealClothing:onlySuppressRealClothingJSB.val,onlyRemoveRealClothing:onlyReplaceRealClothingJSB.val);
                }
                
            }
        }

        public static void SavePreset(Atom atom, JSONStorable js, FileReference fileRef)
        {
            JSONStorableBool loadOnSelectJSON = js.GetBoolJSONParam("loadPresetOnSelect");
            bool preState = loadOnSelectJSON.val;
            loadOnSelectJSON.val = false;

            JSONStorableUrl presetPathJSON = js.GetUrlJSONParam("presetBrowsePath");

            string fileName = SuperController.singleton.NormalizePath(fileRef.currentActionSelectedFile);

            if (fileRef.fileSelectionModeJSEnum.val==FileSelectionMode.singleFile && fileRef.forceUniqueSaveNameJSB.val && FileManagerSecure.FileExists(fileName))
            {
                string newFileName = "";
                string fileNameWithoutExt = fileName.Substring(0, fileName.Length - 4);
                for (int i=1; i<10000; i++)
                {
                    newFileName = fileNameWithoutExt + i.ToString() + ".vap";
                    if (!FileManagerSecure.FileExists(newFileName))
                    {
                        fileName = newFileName;
                        break;
                    }
                }
                if (fileName != newFileName)
                {
                    loadOnSelectJSON.val = preState;
                    return;
                }
            }

            presetPathJSON.val = fileName;

            js.CallAction("StorePresetWithScreenshot");

            loadOnSelectJSON.val = preState;
        }
        private static HashSet<DAZClothingItem> LockNonRealClothing(Atom atom)
        {
            var lockedDCIs = new HashSet<DAZClothingItem>();
#if VAM_GT_1_21
            if (TargetControl.atomActiveClothingDictionary.ContainsKey(atom.name))
            {

                ActiveClothingLists acl = TargetControl.atomActiveClothingDictionary[atom.name];
                foreach (var dci in acl._activeClothingDCIs)
                {
                    if (!dci.isRealItem && !dci.locked)
                    {
                        dci.SetLocked(true);
                        lockedDCIs.Add(dci);
                    }
                }
            }
            else
            {
                SuperController.LogError("UIA.AppearancePresetComponent.LockNonRealClothing: Unable to find active clothing list for Atom '" + atom.name + "'");
            }
#endif
            return lockedDCIs;
        }
        private static void UnlockNonRealClothing(HashSet<DAZClothingItem> lockedDCIs)
        {
#if VAM_GT_1_21
            foreach (var dci in lockedDCIs) dci.SetLocked(false);
#endif
        }

        private static void RemoveNonRealClothing(Atom atom)
        {
#if VAM_GT_1_21
            if (TargetControl.atomActiveClothingDictionary.ContainsKey(atom.name))
            {
                JSONStorable receiver = atom.GetStorableByID("geometry");

                ActiveClothingLists acl = TargetControl.atomActiveClothingDictionary[atom.name];
                foreach (var dci in acl._activeClothingDCIs)
                {
                    if (!dci.isRealItem)
                    {                        
                        if (receiver != null)
                        {
                            JSONStorableBool active = receiver.GetBoolJSONParam("clothing:" + dci.uid);
                            if (active != null) active.val = false;
                        }
                    }
                }
            }
            else
            {
                SuperController.LogError("UIA.AppearancePresetComponent.RemoveNonRealClothing: Unable to find active clothing list for Atom '" + atom.name+"'");
            }
#endif
        }
        public static void RemoveRealClothing(Atom atom)
        {
#if VAM_GT_1_21
            if (TargetControl.atomActiveClothingDictionary.ContainsKey(atom.name))
            {
                JSONStorable receiver = atom.GetStorableByID("geometry");

                ActiveClothingLists acl = TargetControl.atomActiveClothingDictionary[atom.name];
                foreach (var dci in acl._activeClothingDCIs)
                {
                    if (dci.isRealItem)
                    {
                        if (receiver != null)
                        {
                            JSONStorableBool active = receiver.GetBoolJSONParam("clothing:" + dci.uid);
                            if (active != null) active.val = false;
                        }
                    }
                }
            }
            else
            {
                SuperController.LogError("UIA.AppearancePresetComponent.RemoveRealClothing: Unable to find active clothing list for Atom '" + atom.name + "'");
            }
#endif
        }


        public static JSONClass GetNonRealClothingPreset(FileReference fileRef)
        {
            JSONClass nonRealClothingPresetJC = new JSONClass();
            nonRealClothingPresetJC["setUnlistedParamsToDefault"].AsBool = true;

            JSONArray nonRealClothingPresetStorablesJA = new JSONArray();
            nonRealClothingPresetJC["storables"] = nonRealClothingPresetStorablesJA;

            JSONClass nonRealClothingPresetGeometryJC = new JSONClass();
            nonRealClothingPresetGeometryJC["id"] = "geometry";
            JSONArray nonRealClothingPresetClothingGeometryJA = new JSONArray();
            nonRealClothingPresetGeometryJC["clothing"] = nonRealClothingPresetClothingGeometryJA;
            nonRealClothingPresetStorablesJA.Add(nonRealClothingPresetGeometryJC);

            JSONClass appPresetJC = (JSONClass)SuperController.singleton.LoadJSON(SuperController.singleton.NormalizePath(fileRef.currentActionSelectedFile));

            if (appPresetJC["storables"] == null) return nonRealClothingPresetJC;

            var appPresetStorableJCs = new Dictionary<string, JSONClass>();

            foreach (JSONClass jc in appPresetJC["storables"].AsArray)
            {
                if (jc["id"] != null)
                {
                    string storableID = jc["id"].Value;
                    appPresetStorableJCs.Add(storableID, jc);
                }
            }
            HashSet<string> enabledNonRealClothingGeometryIDs= new HashSet<string>();

            if (appPresetStorableJCs.ContainsKey("geometry"))
            {
                var geometryJC = appPresetStorableJCs["geometry"];
                if (geometryJC["clothing"] != null)
                {
                    JSONArray clothingGeometryJA = geometryJC["clothing"].AsArray;
                    foreach (JSONClass clothingGeometryItemJC in clothingGeometryJA)
                    {
                        string internalID ;
                        if (clothingGeometryItemJC["internalId"] != null) internalID = clothingGeometryItemJC["internalId"];
                        else internalID =  FileManagerSecure.GetFileName( clothingGeometryItemJC["id"]);

                        if (clothingGeometryItemJC["enabled"]==null || clothingGeometryItemJC["enabled"].AsBool)
                        {
                            string itemControlStorableID = internalID + "ItemControl";
                            if (appPresetStorableJCs.ContainsKey(itemControlStorableID) && appPresetStorableJCs[itemControlStorableID]["isRealClothingItem"]!=null)
                            {
                                bool isRealClothingItem = appPresetStorableJCs[itemControlStorableID]["isRealClothingItem"].AsBool;
                                if (!isRealClothingItem)
                                {
                                    enabledNonRealClothingGeometryIDs.Add(internalID);
                                    nonRealClothingPresetClothingGeometryJA.Add(clothingGeometryItemJC);
                                }
                            }
                            
                        }
                    }
                }
            }

            foreach (var kvp in appPresetStorableJCs)
            {
                foreach(string enabledNonRealClothingGeometryID in enabledNonRealClothingGeometryIDs)
                {
                    if (kvp.Key.StartsWith(enabledNonRealClothingGeometryID+ "WrapControl") || kvp.Key.StartsWith(enabledNonRealClothingGeometryID + "Sim") || kvp.Key.StartsWith(enabledNonRealClothingGeometryID + "ItemControl") || kvp.Key.StartsWith(enabledNonRealClothingGeometryID + "Material"))
                    {
                        nonRealClothingPresetStorablesJA.Add(kvp.Value);
                    }
                }
            }
            if (nonRealClothingPresetClothingGeometryJA.Count == 0) return null;

            return nonRealClothingPresetJC;
        }

        public static void LoadPreset(Atom atom, JSONStorable js, FileReference fileRef, bool suppressClothingLoad = false, bool suppressScaleLoad = false, bool suppressMegeLoad = false, bool onlySuppressRealClothing = true, bool onlyRemoveRealClothing = true)
        {
            if (fileRef.currentActionSelectedFile == "") return;
            PresetLockStore tempPresetLockStore = new PresetLockStore();
            if (atom.type == "Person") tempPresetLockStore.StorePresetLocks(atom, PresetLoadSettings.suppressPresetLocksJSB.val, suppressClothingLoad);

            JSONClass nonRealClothingPresetJC = null;

            if (js.name=="AppearancePresets" && suppressClothingLoad && onlySuppressRealClothing &&(!fileRef.mergeLoadPresetJSBool.val || suppressMegeLoad) && UIAGlobals.isRealClothingAvailable)
            {
                RemoveNonRealClothing(atom);
                nonRealClothingPresetJC = GetNonRealClothingPreset(fileRef);

                if (nonRealClothingPresetJC != null)
                {
                    var clothingPMC = atom.presetManagerControls.First(x => x.name == "ClothingPresets");
                    clothingPMC.lockParams = false;
                    JSONStorable presetJS = atom.GetStorableByID("ClothingPresets");
                    PresetManager pm = presetJS.GetComponentInChildren<PresetManager>();

                    atom.SetLastRestoredData(nonRealClothingPresetJC, true, true);
                    pm.LoadPresetFromJSON(nonRealClothingPresetJC, true);

                    clothingPMC.lockParams = suppressClothingLoad;
                }
            }

            HashSet<DAZClothingItem> lockedDCIs =null;
            if (js.name=="ClothingPresets" && fileRef.parentButtonOperation.buttonOpTypeJSEnum.val ==UIAButtonOpType.loadClothPreset && !fileRef.mergeLoadPresetJSBool.val && onlyRemoveRealClothing && UIAGlobals.isRealClothingAvailable)
            {
                lockedDCIs = LockNonRealClothing(atom);
            }

            if (js.name == "AppearancePresets" && suppressScaleLoad && PatreonFeatures.patreonContentEnabled) PatreonFeatures.AppPresetSuppressScaleLoad(atom, fileRef, fileRef.mergeLoadPresetJSBool.val && !suppressMegeLoad);
            else
            {
                JSONStorableBool loadOnSelectJSON = js.GetBoolJSONParam("loadPresetOnSelect");
                bool preState = loadOnSelectJSON.val;
                loadOnSelectJSON.val = false;
                bool preState1=false;
                bool preState2 = false;
                bool preState3 = false;
                if (js.name == "PosePresets")
                {
                    JSONStorable snapStorable = atom.GetStorableByID("CharacterPoseSnapRestore");
                    JSONStorableBool snapEnabled = snapStorable.GetBoolJSONParam("enabled");
                    preState1 = snapEnabled.val;
                    snapEnabled.val = fileRef.posePresetSnapBoneToPoseJSB.val;
                }
                if (js.name == "MorphPresets")
                {
                    JSONStorableBool includePhysical = js.GetBoolJSONParam("includePhysical");
                    preState1 = includePhysical.val;
                    includePhysical.val = fileRef.morphPresetIncludePhysicalJSB.val;
                    JSONStorableBool includeAppearance = js.GetBoolJSONParam("includeAppearance");
                    preState2 = includeAppearance.val;
                    includeAppearance.val = fileRef.morphPresetIncludeAppJSB.val;
                }
                if (js.name == "geometry")
                {
                    JSONStorableBool includePhysical = js.GetBoolJSONParam("includePhysical");
                    preState1 = includePhysical.val;
                    includePhysical.val = fileRef.generalPresetIncludePhysicalJSB.val;

                    JSONStorableBool includeAppearance = js.GetBoolJSONParam("includeAppearance");
                    preState2 = includeAppearance.val;
                    includeAppearance.val = fileRef.generalPresetIncludeAppJSB.val;

                    JSONStorableBool includePose = js.GetBoolJSONParam("includeOptional");
                    preState3 = includePose.val;
                    includePose.val = fileRef.generalPresetIncludePoseJSB.val;
                }


                JSONStorableUrl presetPathJSON = js.GetUrlJSONParam("presetBrowsePath");
                string pathPreState = presetPathJSON.val;
                presetPathJSON.val = SuperController.singleton.NormalizePath(fileRef.currentActionSelectedFile);
                
                if (fileRef.mergeLoadPresetJSBool.val && !suppressMegeLoad) js.CallAction("MergeLoadPreset");
                else js.CallAction("LoadPreset");

                fileRef.Reset();

                presetPathJSON.val = pathPreState;
                loadOnSelectJSON.val = preState;

                if (js.name == "PosePresets")
                {
                    JSONStorable snapStorable = atom.GetStorableByID("CharacterPoseSnapRestore");
                    JSONStorableBool snapEnabled = snapStorable.GetBoolJSONParam("enabled");
                    snapEnabled.val = preState1 ;
                }
                if (js.name == "MorphPresets")
                {
                    JSONStorableBool includePhysical = js.GetBoolJSONParam("includePhysical");
                    includePhysical.val = preState1;
                    JSONStorableBool includeAppearance = js.GetBoolJSONParam("includeAppearance");
                    includeAppearance.val = preState2;
                }
                if (js.name == "GeneralPresets")
                {
                    JSONStorableBool includePhysical = js.GetBoolJSONParam("includePhysical");
                    includePhysical.val = preState1;

                    JSONStorableBool includeAppearance = js.GetBoolJSONParam("includeAppearance");
                    includeAppearance.val = preState2;

                    JSONStorableBool includePose = js.GetBoolJSONParam("includeOptional");
                    includePose.val = preState3;
                }
            }

            if (js.name=="PosePresets")
            {
                JSONStorable geometryReceiver = atom.GetStorableByID("geometry");
                JSONStorableBool harliHeelsEnabled = geometryReceiver.GetBoolJSONParam("clothing:Harli Heels");
                JSONStorableBool casualDenimHeelsEnabled = geometryReceiver.GetBoolJSONParam("clothing:Casual Denim Shoes");
                if (harliHeelsEnabled != null && harliHeelsEnabled.val)
                {
                    harliHeelsEnabled.val = false;
                    harliHeelsEnabled.val = true;
                }
                if (casualDenimHeelsEnabled != null && casualDenimHeelsEnabled.val)
                {
                    casualDenimHeelsEnabled.val = false;
                    casualDenimHeelsEnabled.val = true;
                }
            }

            if (lockedDCIs != null) UnlockNonRealClothing(lockedDCIs);

            if (atom.type == "Person") tempPresetLockStore.RestorePresetLocks(atom);
            if (js.name == "ClothingPresets" || js.name == "Preset" || (js.name == "AppearancePresets" && !suppressClothingLoad)) GridsDisplay._uiActiveClothingEditor.RefreshClothing(atom.name);
        }

        public void ResetAppearance(Atom atom, bool appearanceReset)
        {
            JSONArray atomsArray = null;
            if (SuperController.singleton.loadJson != null) atomsArray = SuperController.singleton.loadJson["atoms"].AsArray;
            bool atomExistedAtLoad = false;
            bool varReferenceAvailable = true;
            JSONClass atomAtLoadJSON = null;
            if (atomsArray != null)
            {
                foreach (JSONClass atomJSON in atomsArray)
                {
                    if ((string)atomJSON["id"] == atom.name)
                    {
                        if (atomJSON["storables"] != null)
                        {
                            atomExistedAtLoad = true;
                            atomAtLoadJSON = atomJSON;

                            JSONArray storablesJSON = atomJSON["storables"].AsArray;
                            JSONArray newStorablesJSON = new JSONArray();

                            foreach (JSONClass storableJSON in storablesJSON)
                            {
                                if (!((string)storableJSON["id"]).EndsWith("Animation"))
                                {
                                    string storableString = storableJSON.ToString();
                                    if (storableString.Contains("SELF:"))
                                    {
                                        if (lastLoadSceneFromVar.Contains(":/Saves/scene"))
                                        {
                                            string varPackageName = lastLoadSceneFromVar.Substring(0, lastLoadSceneFromVar.IndexOf(':'));
                                            newStorablesJSON.Add((JSONClass)JSON.Parse(storableString.Replace("SELF", varPackageName)));
                                        }
                                        else varReferenceAvailable = false;
                                    }
                                }
                                else newStorablesJSON.Add(storableJSON);
                            }
                        }
                        break;
                    }
                }
            }

            if (!appearanceReset)
            {

                JSONStorable scaleStorableJSON = atom.GetStorableByID("rescaleObject");
                JSONStorableFloat scaleJSON = scaleStorableJSON.GetFloatJSONParam("scale");
                if (atomExistedAtLoad)
                {
                    float resetScale = 1f;
                    foreach (JSONClass storable in atomAtLoadJSON["storables"].AsArray)
                    {
                        if (storable["id"].Value == "rescaleObject")
                        {
                            resetScale = storable["scale"].AsFloat;
                            break;
                        }
                    }
                    scaleJSON.val = resetScale;
                }
                else { scaleJSON.val = 1f; }
            }
            else if (appearanceReset && atomExistedAtLoad && varReferenceAvailable)
            {

                JSONArray newAtomsArray = new JSONArray();
                JSONClass newSave = new JSONClass();
                newAtomsArray.Add(atomAtLoadJSON);
                newSave["atoms"] = newAtomsArray;

                PresetLockStore tempPresetLockStore1 = new PresetLockStore();
                if (atom.type == "Person") tempPresetLockStore1.StorePresetLocks(atom, true, false);

                if (FileUtils.CreatePluginDataFolder())
                {
                    SuperController.singleton.SaveJSON(newSave, UIAConsts._PluginDataSubfolderName+ "\\UIAtemp.json");
                    atom.LoadAppearancePreset(UIAConsts._PluginDataSubfolderName+"\\UIAtemp.json");
                    FileManagerSecure.DeleteFile(UIAConsts._PluginDataSubfolderName+"\\UIAtemp.json");
                }

                if (atom.type == "Person") tempPresetLockStore1.RestorePresetLocks(atom);
            }
            else if (appearanceReset && !atomExistedAtLoad)
            {
                PresetLockStore tempPresetLockStore1 = new PresetLockStore();
                if (atom.type == "Person") tempPresetLockStore1.StorePresetLocks(atom, true, false);

                atom.ResetAppearance();

                if (atom.type == "Person") tempPresetLockStore1.RestorePresetLocks(atom);
            }
        }


    }

    public class ClothingComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableBool accessoryTagRemoveJSBool;
        public JSONStorableBool bodysuitTagRemoveJSBool;
        public JSONStorableBool bottomTagRemoveJSBool;
        public JSONStorableBool braTagRemoveJSBool;
        public JSONStorableBool dressTagRemoveJSBool;
        public JSONStorableBool glassesTagRemoveJSBool;
        public JSONStorableBool glovesTagRemoveJSBool;
        public JSONStorableBool hatTagRemoveJSBool;
        public JSONStorableBool jewelryTagRemoveJSBool;
        public JSONStorableBool maskTagRemoveJSBool;
        public JSONStorableBool pantiesTagRemoveJSBool;
        public JSONStorableBool pantsTagRemoveJSBool;
        public JSONStorableBool shirtTagRemoveJSBool;
        public JSONStorableBool shoesTagRemoveJSBool;
        public JSONStorableBool shortsTagRemoveJSBool;
        public JSONStorableBool skirtTagRemoveJSBool;
        public JSONStorableBool socksTagRemoveJSBool;
        public JSONStorableBool stockingsTagRemoveJSBool;
        public JSONStorableBool sweaterTagRemoveJSBool;
        public JSONStorableBool topTagRemoveJSBool;
        public JSONStorableBool underwearTagRemoveJSBool;

        public JSONStorableBool armsTagRemoveJSBool;
        public JSONStorableBool feetTagRemoveJSBool;
        public JSONStorableBool fullBodyTagRemoveJSBool;
        public JSONStorableBool handsTagRemoveJSBool;
        public JSONStorableBool headTagRemoveJSBool;
        public JSONStorableBool hipTagRemoveJSBool;
        public JSONStorableBool legsTagRemoveJSBool;
        public JSONStorableBool neckTagRemoveJSBool;
        public JSONStorableBool torsoTagRemoveJSBool;

        public JSONStorableBool onlyRemoveRealClothingJSB;

        public List<string> tagsToRemoveList
        {
            get
            {
                List<string> tags = new List<string>();
                foreach (JSONStorableParam jsp in GetParamList())
                {
                    if (jsp.name.EndsWith("Tag"))
                    {
                        JSONStorableBool tagRemoveJSB = jsp as JSONStorableBool;
                        if (tagRemoveJSB.val) tags.Add(jsp.name.Substring(0, jsp.name.Length - 3));
                    }
                }
                return tags;
            }
        }

        public List<JSONStorableBool> GetRegionTagBools()
        {
            List<JSONStorableBool> tags = new List<JSONStorableBool>();
            foreach (JSONStorableParam jsp in GetParamList())
            {
                if (jsp.name.EndsWith("Tag"))
                {
                    JSONStorableBool tagRemoveJSB = jsp as JSONStorableBool;
                    if (tagRemoveJSB.name=="armsTag" || tagRemoveJSB.name == "handsTag" ||tagRemoveJSB.name == "legsTag" || tagRemoveJSB.name == "feetTag" || tagRemoveJSB.name == "headTag" || tagRemoveJSB.name == "neckTag" || tagRemoveJSB.name == "fullbodyTag" || tagRemoveJSB.name == "hipTag" || tagRemoveJSB.name == "torsoTag") tags.Add(tagRemoveJSB);
                }
            }
            return tags;
        }

        public List<JSONStorableBool> GetTypeTagBools()
        {
            List<JSONStorableBool> tags = new List<JSONStorableBool>();
            foreach (JSONStorableParam jsp in GetParamList())
            {
                if (jsp.name.EndsWith("Tag"))
                {
                    JSONStorableBool tagRemoveJSB = jsp as JSONStorableBool;
                    if (tagRemoveJSB.name == "armsTag" || tagRemoveJSB.name == "handsTag" || tagRemoveJSB.name == "legsTag" || tagRemoveJSB.name == "feetTag" || tagRemoveJSB.name == "headTag" || tagRemoveJSB.name == "neckTag" || tagRemoveJSB.name == "fullbodyTag" || tagRemoveJSB.name == "hipTag" || tagRemoveJSB.name == "torsoTag") break;
                    else tags.Add(tagRemoveJSB);
                }
            }
            return tags;
        }

        public ClothingComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            accessoryTagRemoveJSBool = new JSONStorableBool("accessoryTag", false);
            RegisterParam(accessoryTagRemoveJSBool);
            bodysuitTagRemoveJSBool = new JSONStorableBool("bodysuitTag", false);
            RegisterParam(bodysuitTagRemoveJSBool);
            bottomTagRemoveJSBool = new JSONStorableBool("bottomTag", false);
            RegisterParam(bottomTagRemoveJSBool);
            braTagRemoveJSBool = new JSONStorableBool("braTag", false);
            RegisterParam(braTagRemoveJSBool);
            dressTagRemoveJSBool = new JSONStorableBool("dressTag", false);
            RegisterParam(dressTagRemoveJSBool);
            glassesTagRemoveJSBool = new JSONStorableBool("glassesTag", false);
            RegisterParam(glassesTagRemoveJSBool);
            glovesTagRemoveJSBool = new JSONStorableBool("glovesTag", false);
            RegisterParam(glovesTagRemoveJSBool);
            hatTagRemoveJSBool = new JSONStorableBool("hatTag", false);
            RegisterParam(hatTagRemoveJSBool);
            jewelryTagRemoveJSBool = new JSONStorableBool("jewelryTag", false);
            RegisterParam(jewelryTagRemoveJSBool);
            maskTagRemoveJSBool = new JSONStorableBool("maskTag", false);
            RegisterParam(maskTagRemoveJSBool);
            pantiesTagRemoveJSBool = new JSONStorableBool("pantiesTag", false);
            RegisterParam(pantiesTagRemoveJSBool);
            pantsTagRemoveJSBool = new JSONStorableBool("pantsTag", false);
            RegisterParam(pantsTagRemoveJSBool);
            shirtTagRemoveJSBool = new JSONStorableBool("shirtTag", false);
            RegisterParam(shirtTagRemoveJSBool);
            shoesTagRemoveJSBool = new JSONStorableBool("shoesTag", false);
            RegisterParam(shoesTagRemoveJSBool);
            shortsTagRemoveJSBool = new JSONStorableBool("shortsTag", false);
            RegisterParam(shortsTagRemoveJSBool);
            skirtTagRemoveJSBool = new JSONStorableBool("skirtTag", false);
            RegisterParam(skirtTagRemoveJSBool);
            socksTagRemoveJSBool = new JSONStorableBool("socksTag", false);
            RegisterParam(socksTagRemoveJSBool);
            stockingsTagRemoveJSBool = new JSONStorableBool("stockingsTag", false);
            RegisterParam(stockingsTagRemoveJSBool);
            sweaterTagRemoveJSBool = new JSONStorableBool("sweaterTag", false);
            RegisterParam(sweaterTagRemoveJSBool);
            topTagRemoveJSBool = new JSONStorableBool("topTag", false);
            RegisterParam(topTagRemoveJSBool);
            underwearTagRemoveJSBool = new JSONStorableBool("underwearTag", false);
            RegisterParam(underwearTagRemoveJSBool);

            armsTagRemoveJSBool = new JSONStorableBool("armsTag", false);
            RegisterParam(armsTagRemoveJSBool);
            feetTagRemoveJSBool = new JSONStorableBool("feetTag", false);
            RegisterParam(feetTagRemoveJSBool);
            fullBodyTagRemoveJSBool = new JSONStorableBool("fullbodyTag", false);
            RegisterParam(fullBodyTagRemoveJSBool);
            handsTagRemoveJSBool = new JSONStorableBool("handsTag", false);
            RegisterParam(handsTagRemoveJSBool);
            headTagRemoveJSBool = new JSONStorableBool("headTag", false);
            RegisterParam(headTagRemoveJSBool);
            hipTagRemoveJSBool = new JSONStorableBool("hipTag", false);
            RegisterParam(hipTagRemoveJSBool);
            legsTagRemoveJSBool = new JSONStorableBool("legsTag", false);
            RegisterParam(legsTagRemoveJSBool);
            neckTagRemoveJSBool = new JSONStorableBool("neckTag", false);
            RegisterParam(neckTagRemoveJSBool);
            torsoTagRemoveJSBool = new JSONStorableBool("torsoTag", false);
            RegisterParam(torsoTagRemoveJSBool);

            onlyRemoveRealClothingJSB = new JSONStorableBool("onlyRemoveRealClothing", true);
            RegisterParam(onlyRemoveRealClothingJSB);
        }


        public void LoadJSON(JSONClass clothingPresetComponentJSON, int uiapVersion)
        {
            base.RestoreFromJSON(clothingPresetComponentJSON);
            if (uiapVersion == 1 && clothingPresetComponentJSON["tagsToRemove"] != null)
            {
                JSONArray tagsToRemoveArray = clothingPresetComponentJSON["tagsToRemove"].AsArray;
                for (int i = 0; i < tagsToRemoveArray.Count; i++)
                {
                    string tagToRemove = tagsToRemoveArray[i].Value;
                    JSONStorableBool tagJSBool = GetJSONParam(tagToRemove.ToLower() + "Tag") as JSONStorableBool;
                    if (tagJSBool != null) tagJSBool.val = true;
                }
            }

        }

        public static void ClothingActions(Atom atom, int mode, bool reverse, bool onlyRemoveRealClothing=true)
        {
            foreach (string receiverName in atom.GetStorableIDs())
            {
                JSONStorable receiver = atom.GetStorableByID(receiverName);

                if (receiver != null)
                {
                    if (receiver.storeId.Length >= 3 && receiver.storeId.Substring(receiver.storeId.Length - 3) == "Sim")
                    {
                        JSONStorableBool unDressBool = receiver.GetBoolJSONParam("allowDetach");
                        JSONStorableBool simEnabledBool = receiver.GetBoolJSONParam("simEnabled");

                        if (mode == ClothingActionMode.undress && unDressBool != null) { unDressBool.val = !reverse; }
                        if (mode == ClothingActionMode.resetSim && simEnabledBool != null) { if (simEnabledBool.val) { receiver.CallAction("Reset"); } }
                    }

                    
                    if (receiver.storeId == "geometry" && mode == ClothingActionMode.remove)
                    {
                        if (onlyRemoveRealClothing && UIAGlobals.isRealClothingAvailable) AppearancePresetComponent.RemoveRealClothing(atom);
                        else {
                            foreach (string target in receiver.GetBoolParamNames())
                            {
                                if (target.Length > 8 && target.Substring(0, 9) == "clothing:")
                                {
                                    JSONStorableBool boolTarget = receiver.GetBoolJSONParam(target);
                                    if (boolTarget != null) { boolTarget.val = false; }
                                }
                            }
                        }

                        
                    }
                }
            }
        }

        public void ClothingPresetActions(Atom atom, int actionMode, bool reverse, FileReference fileRef, TargetButtonComponent targetComponent)
        {
            if (PatreonFeatures.patreonContentEnabled)
            {
                if (actionMode == ClothingActionMode.merge && tagsToRemoveList.Count > 0 && TargetControl.atomActiveClothingDictionary.ContainsKey(atom.name))
                {
                    PatreonFeatures.RemoveTagClothingItems(atom, TargetControl.atomActiveClothingDictionary[atom.name]._activeClothingTags, tagsToRemoveList);
                }
                else if (actionMode == ClothingActionMode.merge)
                {
                    if (!UIAButton.presetMergeClothingGeometryIDs.ContainsKey(fileRef.currentActionSelectedFile)) UIAButton.presetMergeClothingGeometryIDs.Add(fileRef.currentActionSelectedFile, null);
                    else UIAButton.presetMergeClothingGeometryIDs[fileRef.currentActionSelectedFile] = null;
                }

                if (actionMode == ClothingActionMode.merge) UIAButton.cachedClothingPresetFiles.Remove(fileRef.currentActionSelectedFile);

                if (FileManagerSecure.FileExists(fileRef.currentActionSelectedFile))
                {
                    PatreonFeatures.ClothingPresetActions(atom, actionMode, fileRef.currentActionSelectedFile, reverse);

                    if (reverse) targetComponent.ResetLastUserChosenAtom();

                }

            }
        }

        public int GetUndressAllClothingButtonState(Atom atom)
        {           
            int clothingItemCount = 0;
            foreach (string receiverName in atom.GetStorableIDs())
            {
                JSONStorable receiver = atom.GetStorableByID(receiverName);
                if (receiver != null)
                {
                    if (receiver.storeId.Length >= 3 && receiver.storeId.Substring(receiver.storeId.Length - 3) == "Sim")
                    {
                        JSONStorableBool unDressBool = receiver.GetBoolJSONParam("allowDetach");
                        if (unDressBool != null)
                        {
                            if (!unDressBool.val) return ButtonState.inactive;
                            clothingItemCount++;
                        }
                        
                    }
                }
            }
            if (clothingItemCount > 0) return ButtonState.active;
            return ButtonState.inactive;
        }
    }

    public class VAMPlayEditModeComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableBool closeGameUIonPlayModeJSBool;
        public JSONStorableBool openGameUIonEditModeJSBool;

        public VAMPlayEditModeComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            closeGameUIonPlayModeJSBool = new JSONStorableBool("closeGameUIonPlayMode", false);
            RegisterParam(closeGameUIonPlayModeJSBool);
            openGameUIonEditModeJSBool = new JSONStorableBool("openGameUIonEditMode", false);
            RegisterParam(openGameUIonEditModeJSBool);
        }
    }

    public class SpawnAtomComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableEnumStringChooser atomCategoryJSEnum;
        public JSONStorableEnumStringChooser atomTypeJSEnum;
        public JSONStorableBool parentLinkToTargetJSBool;
        public JSONStorableBool atomSpawnSpecifyNameJSBool;
        public JSONStorableString atomSpawnNameJSString;

        public static bool atomSpawningActive = false;

        private RelativePositionComponent relativePositionComponent { get { return parentButtonOperation.relativePositionComponent; } }

        public SpawnAtomComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            atomCategoryJSEnum = new JSONStorableEnumStringChooser("atomCategory", AtomCategories.enumManifestName, AtomCategories.people, "", AtomCategoryUpdated);
            RegisterParam(atomCategoryJSEnum);
            atomTypeJSEnum = new JSONStorableEnumStringChooser("atomType", AtomTypes.enumManifestName, AtomTypes.person, "", AtomTypeUpdated, AtomTypes.GetAtomTypesExcluding(atomCategoryJSEnum.val));
            RegisterParam(atomTypeJSEnum);
            parentLinkToTargetJSBool = new JSONStorableBool("parentLinkToTarget", false);
            RegisterParam(parentLinkToTargetJSBool);
            atomSpawnSpecifyNameJSBool = new JSONStorableBool("atomSpawnSpecifyName", false);
            RegisterParam(atomSpawnSpecifyNameJSBool);
            atomSpawnNameJSString = new JSONStorableString("atomSpawnName", "");
            RegisterParam(atomSpawnNameJSString);
        }

        protected void AtomCategoryUpdated(int atomCat)
        {
            atomTypeJSEnum.SetEnumChoices(AtomTypes.enumManifestName, AtomTypes.GetAtomTypesExcluding(atomCat));
        }
        protected void AtomTypeUpdated(int atomType)
        {
            parentButtonOperation.RefreshFileRefTypes();
        }

        public void SpawnAtomAction()
        {
            atomSpawningActive = true;

            string atomSpecifiedName = "";
            if (atomSpawnSpecifyNameJSBool.val) atomSpecifiedName = atomSpawnNameJSString.val;
            string atomType = atomTypeJSEnum.displayVal;
            if (buttonTypeJSEnum.val == UIAButtonOpType.loadGeneralPreset)
            {
                atomType = parentButtonOperation.targetComponent.specificAtomTypeJSS.val;
                atomSpecifiedName = parentButtonOperation.targetComponent.targetNameJSMultiEnum.displayVal;
            }

            if (atomType != "")
            {
                UIAGlobals.mvrScript.StartCoroutine(CreateAtom(atomType, atomSpecifiedName, (atomName) =>
                {
                    try
                    {

                        relativePositionComponent.TeleportAtomToSpawnPoint(atomName);

                        UIAGlobals.mvrScript.StartCoroutine(RestoreAtomPreset(atomName));

                    }
                    catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
                }));
            }
            else SuperController.LogMessage("UIAssist.SpawnAtomComponent.SpawnAtomAction: The atom type to be spawned cant be identified. If this is General Preset Button Operation being performed on a missing Target Atom, then try resetting the button target to an exising atom of the correct type, then delete the atom again.");
        }

        private IEnumerator CreateAtom(string atomType, string atomName, Action<string> onAtomCreated)
        {
            string newAtomName = atomName;
            if (atomName == "") newAtomName = atomType;

            newAtomName = AtomUtils.GetNextAvailableAtomName(newAtomName);

            yield return SuperController.singleton.AddAtomByType(atomType, newAtomName);

            onAtomCreated(newAtomName);
        }


        private IEnumerator RestoreAtomPreset(string atomName)
        {
            yield return new WaitForEndOfFrame();

            try
            {
                Atom atom = SuperController.singleton.GetAtomByUid(atomName);

                if (parentLinkToTargetJSBool.val && (relativePositionComponent.atomPositionRelativeToJSEnum.val == RelativePositionMode.lastViewedFemale || relativePositionComponent.atomPositionRelativeToJSEnum.val == RelativePositionMode.lastViewedMale || relativePositionComponent.atomPositionRelativeToJSEnum.val == RelativePositionMode.lastViewedPerson))
                {
                    string targetAtomName = relativePositionComponent.GetRelativePositionAtomName();
                    if (targetAtomName != "")
                    {
                        string targetNode = relativePositionComponent.targetNodeJSSC.val;
                        if (targetNode == "Lips" || targetNode == "Mouth") targetNode = "head";
                        else if (targetNode == "Labia" || targetNode == "Vagina") targetNode = "pelvis";
                        Atom targetAtom = SuperController.singleton.GetAtomByUid(targetAtomName);
                        atom.mainController.SetLinkToAtom(targetAtomName);
                        Rigidbody linkRB = targetAtom.rigidbodies.First(rb => rb.name == targetNode);
                        atom.mainController.linkToRB = linkRB;
                        atom.mainController.currentPositionState = FreeControllerV3.PositionState.ParentLink;
                        atom.mainController.currentRotationState = FreeControllerV3.RotationState.ParentLink;
                    }
                }

                if (atomTypeJSEnum.val == AtomTypes.person && buttonTypeJSEnum.val != UIAButtonOpType.loadGeneralPreset)
                {
                    FileReference posePresetFileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.posePreset];

                    if (posePresetFileRef.fileSelectionModeJSEnum.val != FileSelectionMode.none)
                    {
                        JSONStorable js = atom.GetStorableByID("PosePresets");
                        AppearancePresetComponent.LoadPreset(atom, js, posePresetFileRef, false, false,true,false);
                    }
                }
                else if (atomTypeJSEnum.val != AtomTypes.subScene)
                {
                    FileReference generalPresetFileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.generalPreset];

                    if (generalPresetFileRef.fileSelectionModeJSEnum.val != FileSelectionMode.none)
                    {
                        JSONStorable js = atom.GetStorableByID("Preset");
                        AppearancePresetComponent.LoadPreset(atom, js, generalPresetFileRef, false, false,true);
                    }

                }
                if (parentButtonOperation.pluginsLoadComponent != null)  parentButtonOperation.pluginsLoadComponent.LoadPluginsAsPreset(atom);

                if (buttonTypeJSEnum.val != UIAButtonOpType.loadGeneralPreset && atomSpawnSpecifyNameJSBool.val)
                {
                    atom.SetUID(atomSpawnNameJSString.val);
                    atomName = atom.uid;
                }
                else if (buttonTypeJSEnum.val == UIAButtonOpType.loadGeneralPreset)
                {
                    
                    atom.SetUID(parentButtonOperation.targetComponent.targetNameJSMultiEnum.mainVal);
                    atomName = atom.uid;
                }
                atomSpawningActive = false;
                SuperController.singleton.helpHUDText.text = "";

            }
            catch (Exception e) { SuperController.LogError("Exception1 caught: " + e); }
            yield return new WaitForFixedUpdate();

            try
            {
                Atom atom1 = SuperController.singleton.GetAtomByUid(atomName);

                if (buttonTypeJSEnum.val != UIAButtonOpType.loadGeneralPreset && atomTypeJSEnum.val == AtomTypes.person)
                {
                    FileReference appearancePresetFileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.appearancePreset];

                    if (appearancePresetFileRef.fileSelectionModeJSEnum.val != FileSelectionMode.none)
                    {
                        JSONStorable js1 = atom1.GetStorableByID("AppearancePresets");
                        AppearancePresetComponent.LoadPreset(atom1, js1, appearancePresetFileRef, false, false, true,true);
  //                      AppearancePresetComponent.LoadPreset(atom1, js1, appearancePresetFileRef, false, false, true);
                    }

                    FileReference pluginPresetFileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.pluginsPreset];

                    if (pluginPresetFileRef.fileSelectionModeJSEnum.val != FileSelectionMode.none)
                    {
                        JSONStorable js2 = atom1.GetStorableByID("PluginPresets");
                        AppearancePresetComponent.LoadPreset(atom1, js2, pluginPresetFileRef, false, false);
                    }

                }
                else if (buttonTypeJSEnum.val != UIAButtonOpType.loadGeneralPreset && atomTypeJSEnum.val == AtomTypes.subScene)
                {
                    JSONStorable js1 = atom1.GetStorableByID("SubScene");
                    JSONStorableUrl subScenePathJSON = js1.GetUrlJSONParam("browsePath");

                    string subScenePreset = parentButtonOperation.fileReferenceDict[FileReferenceTypes.subScene].currentActionSelectedFile;

                    if (subScenePreset != "" && FileManagerSecure.FileExists(SuperController.singleton.NormalizePath(subScenePreset)))
                    {
                        if (subScenePathJSON.val != SuperController.singleton.NormalizePath(subScenePreset)) subScenePathJSON.val = SuperController.singleton.NormalizePath(subScenePreset);
                        else js1.CallAction("LoadSubScene");
                    }
                    atom1.SetUID(atomName);
                }
            }
            catch (Exception e) { SuperController.LogError("Exception2 caught: " + e); }
        }

    }

    public class RelativePositionComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableEnumStringChooser atomPositionRelativeToJSEnum;
        public JSONStorableStringChooser targetNodeJSSC;

        public JSONStorableFloat atomPositionXJSFloat;
        public JSONStorableFloat atomPositionYJSFloat;
        public JSONStorableFloat atomPositionZJSFloat;
        public JSONStorableFloat atomRotationXJSFloat;
        public JSONStorableFloat atomRotationYJSFloat;
        public JSONStorableFloat atomRotationZJSFloat;

        public JSONStorableBool atomAbsoluteHeightJSBool;
        public JSONStorableBool atomAbsolutePitchJSBool;
        public JSONStorableBool atomAbsoluteRollJSBool;


        public RelativePositionComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            atomPositionRelativeToJSEnum = new JSONStorableEnumStringChooser("atomPositionRelativeTo", RelativePositionMode.enumManifestName, RelativePositionMode.sceneOrigin, "", GetPositionRelativeToExclusions());
            targetNodeJSSC = new JSONStorableStringChooser("targetNode", GetTargetNodeChoices(), "control", "");
            atomPositionXJSFloat = new JSONStorableFloat("atomPositionX", 0f, -3f, 3f, false);
            atomPositionYJSFloat = new JSONStorableFloat("atomPositionY", 0f, -3f, 3f, false);
            atomPositionZJSFloat = new JSONStorableFloat("atomPositionZ", 0f, -3f, 3f, false);
            atomRotationXJSFloat = new JSONStorableFloat("atomRotationX", 0f, -180f, 180f);
            atomRotationYJSFloat = new JSONStorableFloat("atomRotationY", 0f, -180f, 180f);
            atomRotationZJSFloat = new JSONStorableFloat("atomRotationZ", 0f, -180f, 180f);
            atomAbsoluteHeightJSBool = new JSONStorableBool("atomAbsoluteHeight", false);
            atomAbsolutePitchJSBool = new JSONStorableBool("atomAbsolutePitch", false);
            atomAbsoluteRollJSBool = new JSONStorableBool("atomAbsoluteRoll", false);

            RegisterParam(atomPositionRelativeToJSEnum);
            RegisterParam(targetNodeJSSC);

            RegisterParam(atomPositionXJSFloat);
            RegisterParam(atomPositionYJSFloat);
            RegisterParam(atomPositionZJSFloat);
            RegisterParam(atomRotationXJSFloat);
            RegisterParam(atomRotationYJSFloat);
            RegisterParam(atomRotationZJSFloat);

            RegisterParam(atomAbsoluteHeightJSBool);
            RegisterParam(atomAbsolutePitchJSBool);
            RegisterParam(atomAbsoluteRollJSBool);
        }

        public static List<string> GetTargetNodeChoices()
        {
            List<string> nodeChoices = new List<string>();
            nodeChoices.Add("control");
            nodeChoices.Add("headControl");
            nodeChoices.Add("head");
            nodeChoices.Add("Lips");
            nodeChoices.Add("Mouth");
            nodeChoices.Add("neckControl");
            nodeChoices.Add("neck");
            nodeChoices.Add("lShoulderControl");
            nodeChoices.Add("lCollar");
            nodeChoices.Add("lPectoral");
            nodeChoices.Add("lArmControl");
            nodeChoices.Add("lShldr");
            nodeChoices.Add("lElbowControl");
            nodeChoices.Add("lForeArm");
            nodeChoices.Add("lHandControl");
            nodeChoices.Add("lHand");
            nodeChoices.Add("rShoulderControl");
            nodeChoices.Add("rCollar");
            nodeChoices.Add("rPectoral");
            nodeChoices.Add("rArmControl");
            nodeChoices.Add("rShldr");
            nodeChoices.Add("rElbowControl");
            nodeChoices.Add("rForeArm");
            nodeChoices.Add("rHandControl");
            nodeChoices.Add("rHand");

            nodeChoices.Add("chestControl");
            nodeChoices.Add("chest");
            nodeChoices.Add("lNippleControl");
            nodeChoices.Add("lNipple");
            nodeChoices.Add("rNippleControl");
            nodeChoices.Add("rNipple");

            nodeChoices.Add("abdomen2Control");
            nodeChoices.Add("abdomen2");
            nodeChoices.Add("pelvisControl");
            nodeChoices.Add("pelvis");
            nodeChoices.Add("hipControl");
            nodeChoices.Add("hip");
            nodeChoices.Add("abdomenControl");
            nodeChoices.Add("abdomen");
            nodeChoices.Add("LGlute");
            nodeChoices.Add("RGlute");

            nodeChoices.Add("testesControl");
            nodeChoices.Add("penisBaseControl");
            nodeChoices.Add("penisMidControl");
            nodeChoices.Add("penisTipControl");

            nodeChoices.Add("Labia");
            nodeChoices.Add("Vagina");

            nodeChoices.Add("lThighControl");
            nodeChoices.Add("lThigh");
            nodeChoices.Add("lKneeControl");
            nodeChoices.Add("lShin");
            nodeChoices.Add("lFootControl");
            nodeChoices.Add("lFoot");
            nodeChoices.Add("lToeControl");

            nodeChoices.Add("rThighControl");
            nodeChoices.Add("rThigh");
            nodeChoices.Add("rKneeControl");
            nodeChoices.Add("rShin");
            nodeChoices.Add("rFootControl");
            nodeChoices.Add("rFoot");
            nodeChoices.Add("rToeControl");

            return nodeChoices;
        }

        public string GetRelativePositionAtomName()
        {
            if (atomPositionRelativeToJSEnum.val == RelativePositionMode.lastViewedFemale) return TargetControl.lastViewedFemale;
            if (atomPositionRelativeToJSEnum.val == RelativePositionMode.lastViewedMale) return TargetControl.lastViewedMale;
            if (atomPositionRelativeToJSEnum.val == RelativePositionMode.lastViewedPerson) return TargetControl.lastViewedPerson;
            return "";
        }

        public void UpdateSpawnPosition(ref Vector3 position, ref Quaternion rotation, string testAtomName = null)
        {            
            string gazeTargetAtomName = GetRelativePositionAtomName();
            
            if (testAtomName != null) gazeTargetAtomName = testAtomName;

            Vector3 offsetPosition = new Vector3(atomPositionXJSFloat.val, atomPositionYJSFloat.val, atomPositionZJSFloat.val);
            Vector3 basePosition = new Vector3(0f, 0f, 0f);
            Quaternion baseRotation = new Quaternion();
            if (atomAbsoluteHeightJSBool.val) offsetPosition.y = 0f;
            Transform relativeToTransform = GetSpawnRelativeToTransform(gazeTargetAtomName);
            if (relativeToTransform != null)
            {
                basePosition = relativeToTransform.position;
                baseRotation = relativeToTransform.rotation;
                if (buttonTypeJSEnum.val != UIAButtonOpType.teleportPlayer) offsetPosition = relativeToTransform.rotation * offsetPosition;
            }

            position.x = basePosition.x + offsetPosition.x;
            if (!atomAbsoluteHeightJSBool.val) position.y = basePosition.y + offsetPosition.y;
            else position.y = atomPositionYJSFloat.val;
            position.z = basePosition.z + offsetPosition.z;

            if (buttonTypeJSEnum.val != UIAButtonOpType.teleportPlayer)
            {
                /*               float xRotation = baseRotation.eulerAngles.x + atomRotationXJSFloat.val;
                               if (atomAbsolutePitchJSBool.val) xRotation = atomRotationXJSFloat.val;
                               float zRotation = baseRotation.eulerAngles.z + atomRotationXJSFloat.val;
                               if (atomAbsoluteRollJSBool.val) zRotation = atomRotationZJSFloat.val;
                               rotation.eulerAngles = new Vector3(xRotation, baseRotation.eulerAngles.y + atomRotationYJSFloat.val, zRotation);
                */
                Quaternion offsetRotation = new Quaternion();
                offsetRotation.eulerAngles = new Vector3(atomRotationXJSFloat.val, atomRotationYJSFloat.val, atomRotationZJSFloat.val);

                rotation = baseRotation * offsetRotation;
                if (atomAbsolutePitchJSBool.val) rotation = Quaternion.Euler(atomRotationXJSFloat.val, rotation.y, rotation.z);
                if (atomAbsoluteRollJSBool.val) rotation = Quaternion.Euler(rotation.x, rotation.y, atomRotationZJSFloat.val);
            }
            else
            {

                if (atomPositionRelativeToJSEnum.val == RelativePositionMode.sceneOrigin) rotation.eulerAngles = new Vector3(0f, atomRotationYJSFloat.val, 0f);
                else rotation.SetLookRotation(basePosition - position, SuperController.singleton.navigationRig.up);
                
            }

        }

        public void TeleportAtomToSpawnPoint(string teleportAtomName, bool applyPresets = false)
        {
            Vector3 position = new Vector3(0f, 0f, 0f);
            Quaternion rotation = new Quaternion();
            UpdateSpawnPosition(ref position, ref rotation);
            TeleportAtomToSpawnPoint(teleportAtomName, position, rotation, applyPresets);
        }
        public void TeleportAtomToSpawnPoint(string teleportAtomName, Vector3 position, Quaternion rotation, bool applyPresets = false)
        {
            Atom atom = SuperController.singleton.GetAtomByUid(teleportAtomName);
            if (atom != null)
            {
                JSONClass atomsJSON = SuperController.singleton.GetSaveJSON(atom);
                JSONArray atomsArrayJSON = atomsJSON["atoms"].AsArray;
                JSONClass atomJSON = (JSONClass)atomsArrayJSON[0];

                JSONArray storablesArrayJSON = atomJSON["storables"].AsArray;

                atomJSON["position"]["x"].AsFloat = position.x;
                atomJSON["position"]["y"].AsFloat = position.y;
                atomJSON["position"]["z"].AsFloat = position.z;
                atomJSON["containerPosition"]["x"].AsFloat = position.x;
                atomJSON["containerPosition"]["y"].AsFloat = position.y;
                atomJSON["containerPosition"]["z"].AsFloat = position.z;
                atomJSON["rotation"]["x"].AsFloat = rotation.eulerAngles.x;
                atomJSON["rotation"]["y"].AsFloat = rotation.eulerAngles.y;
                atomJSON["rotation"]["z"].AsFloat = rotation.eulerAngles.z;
                atomJSON["containerRotation"]["x"].AsFloat = rotation.eulerAngles.x;
                atomJSON["containerRotation"]["y"].AsFloat = rotation.eulerAngles.y;
                atomJSON["containerRotation"]["z"].AsFloat = rotation.eulerAngles.z;

                Dictionary<string, int> storablesIndexDict = new Dictionary<string, int>();

                for (int i = 0; i < storablesArrayJSON.Count; i++)
                {
                    JSONClass storableJSON = (JSONClass)storablesArrayJSON[i];
                    storablesIndexDict.Add(storableJSON["id"].Value, i);
                    if (storableJSON["id"].Value == "control")
                    {
                        atomJSON["storables"][i]["position"]["x"].AsFloat = position.x;
                        atomJSON["storables"][i]["position"]["y"].AsFloat = position.y;
                        atomJSON["storables"][i]["position"]["z"].AsFloat = position.z;
                    }
                    if (storableJSON["id"].Value == "hip")
                    {
                        atomJSON["storables"][i]["rootPosition"]["x"].AsFloat = position.x;
                        atomJSON["storables"][i]["rootPosition"]["y"].AsFloat = atomJSON["storables"][i]["rootPosition"]["y"].AsFloat + position.y;
                        atomJSON["storables"][i]["rootPosition"]["z"].AsFloat = position.z;
                    }
                    if (storableJSON["id"].Value.StartsWith("hair"))
                    {
                        atomJSON["storables"][i]["position"]["x"].AsFloat = atomJSON["storables"][i]["position"]["x"].AsFloat + position.x;
                        atomJSON["storables"][i]["position"]["y"].AsFloat = atomJSON["storables"][i]["position"]["y"].AsFloat + position.y;
                        atomJSON["storables"][i]["position"]["z"].AsFloat = atomJSON["storables"][i]["position"]["z"].AsFloat + position.z;
                    }

                    if (storableJSON["id"].Value == "control" || storableJSON["id"].Value == "hip" || storableJSON["id"].Value.StartsWith("hair"))
                    {
                        string rotationKeyName = "";
                        if (storableJSON["id"].Value == "hip") rotationKeyName = "rootRotation";
                        else rotationKeyName = "rotation";
                        atomJSON["storables"][i][rotationKeyName]["x"].AsFloat = rotation.eulerAngles.x;
                        atomJSON["storables"][i][rotationKeyName]["y"].AsFloat = rotation.eulerAngles.y;
                        atomJSON["storables"][i][rotationKeyName]["z"].AsFloat = rotation.eulerAngles.z;
                    }
                }

                atom.PreRestore();
                atom.RestoreTransform(atomJSON);
                atom.Restore(atomJSON);
                atom.LateRestore(atomJSON);
                atom.PostRestore();

                ClothingComponent.ClothingActions(atom, ClothingActionMode.resetSim, false);
            }

        }

        public void TeleportPlayer()
        {
            Vector3 targetPosition = new Vector3(0f, 0f, 0f);
            Quaternion rotation = new Quaternion();
            UpdateSpawnPosition(ref targetPosition, ref rotation);
            var navigationRig = SuperController.singleton.navigationRig;
            var camera = SuperController.singleton.lookCamera;
            var cameraMoveDelta = targetPosition - camera.transform.position;

            var nrPosition = navigationRig.position;
            nrPosition += cameraMoveDelta;
            var up = navigationRig.up;

            var upDelta = Vector3.Dot(cameraMoveDelta, up);
            nrPosition += up * (0f - upDelta);
            navigationRig.position = nrPosition;
            SuperController.singleton.playerHeightAdjust += upDelta;

            navigationRig.rotation = rotation;

            SuperController.singleton.SyncMonitorRigPosition();
        }

        public Transform GetSpawnRelativeToTransform(string gazeTargetAtomName)
        {
            GameObject emptyGO = new GameObject();
            Transform relativeToTransform = emptyGO.transform;
            relativeToTransform.position = new Vector3(0f, 0f, 0f);
            relativeToTransform.rotation = new Quaternion(0f, 0f, 0f, 1f);

            if (atomPositionRelativeToJSEnum.val == RelativePositionMode.lastViewedFemale || atomPositionRelativeToJSEnum.val == RelativePositionMode.lastViewedMale || atomPositionRelativeToJSEnum.val == RelativePositionMode.lastViewedPerson)
            {
                Atom targetRelativePositionAtom = SuperController.singleton.GetAtomByUid(gazeTargetAtomName);
                if (targetRelativePositionAtom != null)
                {
                    if (targetNodeJSSC.val == "Lips" || targetNodeJSSC.val == "Mouth" || targetNodeJSSC.val == "Labia" || targetNodeJSSC.val == "Vagina")
                    {
                        string targetNode = targetNodeJSSC.val;
                        if (targetNodeJSSC.val == "Lips") targetNode = "LipTrigger";
                        else if (targetNodeJSSC.val == "Mouth") targetNode = "MouthTrigger";
                        else if (targetNodeJSSC.val == "Labia") targetNode = "LabiaTrigger";
                        else if (targetNodeJSSC.val == "Vagina") targetNode = "VaginaTrigger";
                        Rigidbody nodeRB = targetRelativePositionAtom.rigidbodies.First(rb => rb.name == targetNode);
                        if (nodeRB != null) relativeToTransform = nodeRB.transform;
                    }
                    else if (targetNodeJSSC.val.Contains("control") || targetNodeJSSC.val.Contains("Control"))
                    {
                        FreeControllerV3 nodeFCV3 = targetRelativePositionAtom.freeControllers.First(fc => fc.name == targetNodeJSSC.val);
                        if (nodeFCV3 != null) relativeToTransform = nodeFCV3.transform;
                    }
                    else
                    {
                        Rigidbody nodeRB = targetRelativePositionAtom.rigidbodies.First(rb => rb.name == targetNodeJSSC.val);
                        if (nodeRB != null) relativeToTransform = nodeRB.transform;
                    }
                }
            }
            else if (atomPositionRelativeToJSEnum.val == RelativePositionMode.vrHead || (!UIAGlobals.vrActive && (atomPositionRelativeToJSEnum.val == RelativePositionMode.vrLeftHand || atomPositionRelativeToJSEnum.val == RelativePositionMode.vrRightHand))) relativeToTransform = SuperController.singleton.lookCamera.transform;
            else if (atomPositionRelativeToJSEnum.val == RelativePositionMode.vrLeftHand) relativeToTransform = SuperController.singleton.leftHand.transform;
            else if (atomPositionRelativeToJSEnum.val == RelativePositionMode.vrRightHand) relativeToTransform = SuperController.singleton.rightHand.transform;

            return relativeToTransform;
        }


        public List<int> GetPositionRelativeToExclusions()
        {
            List<int> exclusions = new List<int>();
            if (buttonTypeJSEnum.val == UIAButtonOpType.teleportAtom)
            {
                exclusions.Add(RelativePositionMode.lastViewedFemale);
                exclusions.Add(RelativePositionMode.lastViewedMale);
                exclusions.Add(RelativePositionMode.lastViewedPerson);
            }
            if (buttonTypeJSEnum.val == UIAButtonOpType.teleportPlayer)
            {
                exclusions.Add(RelativePositionMode.vrHead);
                exclusions.Add(RelativePositionMode.vrLeftHand);
                exclusions.Add(RelativePositionMode.vrRightHand);
            }
            return exclusions;
        }
        public void ButtonTypeUpdated()
        {
            atomPositionRelativeToJSEnum.SetEnumChoices(RelativePositionMode.enumManifestName, GetPositionRelativeToExclusions(), true, true);

            if (buttonTypeJSEnum.val == UIAButtonOpType.teleportPlayer)
            {
                atomPositionRelativeToJSEnum.val = RelativePositionMode.lastViewedFemale;
                targetNodeJSSC.val = "head";
                atomPositionXJSFloat.val = 0f;
                atomPositionYJSFloat.val = 0f;
                atomPositionZJSFloat.val = 1.5f;
                atomAbsoluteHeightJSBool.val = false;
            }
        }

        public List<string> GetMoveRelativeToChoices()
        {
            return (atomPositionRelativeToJSEnum.displayChoices);
        }

    }
    public class MoveAtomComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableBool lockedPositionXJSB;
        public JSONStorableBool lockedPositionYJSB;
        public JSONStorableBool lockedPositionZJSB;
        public JSONStorableBool lockedRotationXJSB;
        public JSONStorableBool lockedRotationYJSB;
        public JSONStorableBool lockedRotationZJSB;
        public JSONStorableBool vrMoveDisablePhysicsJSB;

        public static bool atomVRMoveActive = false;
        public static bool atomVRMovePaused = true;
        public static bool atomVRMoveLeapActivated = false;
        public static float atomVRMoveStartTime = 0f;
        public static string atomVRMoveAtomName = "";
        public static int atomVRMoveControllerLorR;
        public static UIAButtonOperation atomVRMoveUIAButton = null;

        public static Vector3 atomVRMoveControllerLastPosition = new Vector3();
        public static Quaternion atomVRMoveControllerLastRotation = new Quaternion();
        public static bool currentVRMovelockedPositionX = false;
        public static bool currentVRMovelockedPositionY = false;
        public static bool currentVRMovelockedPositionZ = false;
        public static bool currentVRMovelockedRotationX = false;
        public static bool currentVRMovelockedRotationY = false;
        public static bool currentVRMovelockedRotationZ = false;

        public MoveAtomComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            lockedPositionXJSB = new JSONStorableBool("lockedPositionX", false);
            lockedPositionYJSB = new JSONStorableBool("lockedPositionY", true);
            lockedPositionZJSB = new JSONStorableBool("lockedPositionZ", false);
            lockedRotationXJSB = new JSONStorableBool("lockedRotationX", true);
            lockedRotationYJSB = new JSONStorableBool("lockedRotationY", false);
            lockedRotationZJSB = new JSONStorableBool("lockedRotationZ", true);
            vrMoveDisablePhysicsJSB = new JSONStorableBool("vrMoveDisablePhysics", false);

            RegisterParam(lockedPositionXJSB);
            RegisterParam(lockedPositionYJSB);
            RegisterParam(lockedPositionZJSB);
            RegisterParam(lockedRotationXJSB);
            RegisterParam(lockedRotationYJSB);
            RegisterParam(lockedRotationZJSB);
            RegisterParam(vrMoveDisablePhysicsJSB);
        }

        public void PlayMoveAtomAction(Atom atom, int vrHand, bool leapActivated)
        {
            if (UIAGlobals.vrActive && vrHand != LeftRight.neither)
            {
                if (!atomVRMoveActive || atomVRMoveUIAButton != parentButtonOperation)
                {
                    if (atomVRMoveActive)
                    {
                        Atom previousMoveAtom = SuperController.singleton.GetAtomByUid(atomVRMoveAtomName);
                        if (previousMoveAtom != null)
                        {
                            previousMoveAtom.tempFreezePhysics = false;
                        }
                    }

                    if (vrMoveDisablePhysicsJSB.val) atom.tempFreezePhysics = true;
                    atomVRMoveActive = true;
                    atomVRMoveAtomName = atom.name;
                    atomVRMoveUIAButton = parentButtonOperation;
                    atomVRMoveLeapActivated = leapActivated;
                    FreeControllerV3 atomFC = atom.mainController;
                    Transform vrHandTransform = null;
                    if (vrHand == LeftRight.neither && VRSettings.vrHandControlJSEnum.val == VRHandControl.leftHand) vrHand = LeftRight.left;
                    if (vrHand == LeftRight.neither && VRSettings.vrHandControlJSEnum.val == VRHandControl.rightHand) vrHand = LeftRight.right;
                    if (vrHand == LeftRight.neither && VRSettings.vrHandControlJSEnum.val == VRHandControl.disabled) vrHand = LeftRight.right;

                    if (vrHand == LeftRight.left) vrHandTransform = SuperController.singleton.leftHand;
                    else if (vrHand == LeftRight.right) vrHandTransform = SuperController.singleton.rightHand;
                    atomVRMoveControllerLorR = vrHand;
                    atomVRMoveControllerLastPosition = vrHandTransform.position;
                    atomVRMoveControllerLastRotation = vrHandTransform.rotation;

                    currentVRMovelockedPositionX = lockedPositionXJSB.val;
                    currentVRMovelockedPositionY = lockedPositionYJSB.val;
                    currentVRMovelockedPositionZ = lockedPositionZJSB.val;
                    currentVRMovelockedRotationX = lockedRotationXJSB.val;
                    currentVRMovelockedRotationY = lockedRotationYJSB.val;
                    currentVRMovelockedRotationZ = lockedRotationZJSB.val;
                }
                else StopAtomVRMove(GameControlUI._leftHLeapControl, GameControlUI._rightHLeapControl);
            }

        }

        public static void UpdateAtomMove()
        {
            Transform vrMoveHandTransform = null;
            Atom atom = SuperController.singleton.GetAtomByUid(atomVRMoveAtomName);
            if (atomVRMoveControllerLorR == LeftRight.left) vrMoveHandTransform = SuperController.singleton.leftHand;
            else if (atomVRMoveControllerLorR == LeftRight.right) vrMoveHandTransform = SuperController.singleton.rightHand;

            if (atom != null && vrMoveHandTransform != null)
            {
                bool buttonGrabActived = false;

                if (atomVRMoveControllerLorR == LeftRight.left && (SuperController.singleton.GetLeftGrab() || GameControlUI._leftHLeapControl._pinchVRMoveActived)) buttonGrabActived = true;
                if (atomVRMoveControllerLorR == LeftRight.right && (SuperController.singleton.GetRightGrab() || GameControlUI._rightHLeapControl._pinchVRMoveActived)) buttonGrabActived = true;
                GameControlUI._leftHLeapControl._pinchVRMoveActived = false;
                GameControlUI._rightHLeapControl._pinchVRMoveActived = false;

                if (buttonGrabActived)
                {
                    atomVRMovePaused = false;
                    atomVRMoveStartTime = Time.time;

                    atomVRMoveControllerLastPosition = vrMoveHandTransform.position;
                    atomVRMoveControllerLastRotation = vrMoveHandTransform.rotation;
                }
                bool buttonGrabDeactived = false;
                if (atomVRMoveControllerLorR == LeftRight.left && (SuperController.singleton.GetLeftGrabRelease() || GameControlUI._leftHLeapControl._pinchVRMoveDeActived)) buttonGrabDeactived = true;
                if (atomVRMoveControllerLorR == LeftRight.right && (SuperController.singleton.GetRightGrabRelease() || GameControlUI._rightHLeapControl._pinchVRMoveDeActived)) buttonGrabDeactived = true;
                GameControlUI._leftHLeapControl._pinchVRMoveDeActived = false;
                GameControlUI._rightHLeapControl._pinchVRMoveDeActived = false;
                if (buttonGrabDeactived)
                {
                    atomVRMovePaused = true;
                    float timeSinceMoveStarted = Time.time - atomVRMoveStartTime;
                    if (timeSinceMoveStarted < 0.5f) StopAtomVRMove(GameControlUI._leftHLeapControl, GameControlUI._rightHLeapControl);
                    atomVRMoveStartTime = 0f;
                }

                if (!atomVRMovePaused && atomVRMoveActive)
                {
                    Vector3 direction = atomVRMoveControllerLastPosition - vrMoveHandTransform.position;

                    float angleChange = Quaternion.Angle(atomVRMoveControllerLastRotation, vrMoveHandTransform.rotation);

                    if (direction.magnitude > 0.06f)
                    {
                        Vector3 newPosition = new Vector3();
                        Vector3 currentPosition = atom.mainController.transform.position;
                        direction = direction.normalized * Mathf.Clamp(direction.magnitude - 0.06f, 0f, 0.5f);
                        newPosition = currentPosition + (direction * ((0f - Time.deltaTime) * 10f));
                        if (currentVRMovelockedPositionX) newPosition.x = currentPosition.x;
                        if (currentVRMovelockedPositionY) newPosition.y = currentPosition.y;
                        if (currentVRMovelockedPositionZ) newPosition.z = currentPosition.z;
                        atom.mainController.transform.position = newPosition;
                    }
                    else if (Mathf.Abs(angleChange) > 10f)
                    {
                        float rotationMultiplier = Time.deltaTime * 4.5f;

                        Quaternion angleDifference = vrMoveHandTransform.rotation * Quaternion.Inverse(atomVRMoveControllerLastRotation);
                        Vector3 angleDifferenceVector = new Vector3((angleDifference.eulerAngles.x), (angleDifference.eulerAngles.y), (angleDifference.eulerAngles.z));

                        if (angleDifferenceVector.y > 180f) angleDifferenceVector.y = angleDifferenceVector.y - 360f;
                        if (angleDifferenceVector.x > 180f) angleDifferenceVector.x = angleDifferenceVector.x - 360f;
                        if (angleDifferenceVector.z > 180f) angleDifferenceVector.z = angleDifferenceVector.z - 360f;

                        angleDifferenceVector = new Vector3((angleDifferenceVector.x * rotationMultiplier), (angleDifferenceVector.y * rotationMultiplier), (angleDifferenceVector.z * rotationMultiplier));

                        if (currentVRMovelockedRotationX) angleDifferenceVector.x = 0f;
                        if (currentVRMovelockedRotationY) angleDifferenceVector.y = 0f;
                        if (currentVRMovelockedRotationZ) angleDifferenceVector.z = 0f;

                        Quaternion lockedRotationDifference = Quaternion.Euler(angleDifferenceVector.x, angleDifferenceVector.y, angleDifferenceVector.z);

                        atom.mainController.transform.rotation *= lockedRotationDifference;
                    }
                }
            }

        }
        public static void StopAtomVRMove(LeapTouchControl _leftHLeapControl, LeapTouchControl _rightHLeapControl)
        {
            Atom atom = SuperController.singleton.GetAtomByUid(atomVRMoveAtomName);
            atomVRMoveActive = false;
            _leftHLeapControl._pinchVRMoveActived = false;
            _rightHLeapControl._pinchVRMoveActived = false;
            _leftHLeapControl._pinchVRMoveDeActived = false;
            _rightHLeapControl._pinchVRMoveDeActived = false;
            SuperController.singleton.helpHUDText.text = "";
            atomVRMoveLeapActivated = false;
            if (atom != null) atom.tempFreezePhysics = false;
        }

    }

    public class MotionCaptureComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableBool mocapLoadHeelAdjustJSB;
        public JSONStorableStringChooser motionCaptureAtomNameJSSC;

        public JSONClass motionCaptureSceneFileJSON = null;
        public JSONClass moCapFileJSON = null;

        public MotionCaptureComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            mocapLoadHeelAdjustJSB = new JSONStorableBool("mocapLoadHeelAdjust", false);
            motionCaptureAtomNameJSSC = new JSONStorableStringChooser("motionCaptureAtomName", null,"","");

            RegisterParam(mocapLoadHeelAdjustJSB);
            RegisterParam(motionCaptureAtomNameJSSC);
        }

        public void ReloadMotionCaptureSceneData(string fileName)
        {
            if (FileManagerSecure.FileExists(fileName))
            {
                if (parentButtonOperation.buttonOpTypeJSEnum.val == UIAButtonOpType.loadMotionCapture)
                {
                    motionCaptureSceneFileJSON = SuperController.singleton.LoadJSON(fileName).AsObject;
                    List<string> atomChoices = GetMotionCaptureAtomChoices();
                    if (!atomChoices.Contains(motionCaptureAtomNameJSSC.val)) motionCaptureAtomNameJSSC.val = atomChoices[0];
                }
                else if (parentButtonOperation.buttonOpTypeJSEnum.val == UIAButtonOpType.loadMotionCapture)
                {
                    moCapFileJSON = SuperController.singleton.LoadJSON(fileName).AsObject;
                }


            }
            else motionCaptureSceneFileJSON = null;
        }

        public void RefreshMocapAtomChoices()
        {
            motionCaptureAtomNameJSSC.choices = GetMotionCaptureAtomChoices();
        }

        public List<string> GetMotionCaptureAtomChoices()
        {
            List<string> popupChoices = new List<string>();

            FileReference sceneFileRef = parentButtonOperation.fileReferenceDict[FileReferenceTypes.scene];

            if (FileManagerSecure.FileExists(sceneFileRef.filePathJSString.val))
            {
                if (motionCaptureSceneFileJSON == null) motionCaptureSceneFileJSON = (JSONClass)SuperController.singleton.LoadJSON(sceneFileRef.filePathJSString.val);

                if (motionCaptureSceneFileJSON != null)
                {
                    JSONArray sceneFileAtomsJSON = motionCaptureSceneFileJSON["atoms"].AsArray;
                    if (sceneFileAtomsJSON != null)
                    {
                        List<string> personAtomNames = new List<string>();
                        foreach (JSONClass sceneFileAtomJSON in sceneFileAtomsJSON)
                        {
                            if (sceneFileAtomJSON["type"] != null && sceneFileAtomJSON["type"].Value == "Person")
                            {
                                foreach (JSONClass atomStorable in sceneFileAtomJSON["storables"].AsArray)
                                {
                                    if (atomStorable["id"].Value.EndsWith("Animation"))
                                    {
                                        popupChoices.Add(sceneFileAtomJSON["id"].Value);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else popupChoices.Add("Scene File is not valid format");
                }
                else popupChoices.Add("Scene File is not valid format");
            }
            else popupChoices.Add("Scene File does not exist");

            if (popupChoices.Count == 0) popupChoices.Add("Scene does not contain Person atoms with Motion Capture");

            return (popupChoices);
        }

        private Dictionary<string, JSONNode> GetStorableDictionaryFromArray(JSONNode personData, float footXRotationOffset = 0f)
        {
            Dictionary<string, JSONNode> storablesDict = new Dictionary<string, JSONNode>();

            foreach (JSONNode storable in personData["storables"].AsArray)
            {
                if (storable["id"].Value == "rFootAnimation" || storable["id"].Value == "lFootAnimation")
                {
                    foreach (JSONNode step in storable["steps"].AsArray)
                    {
                        if (step["originalRotation"] == null)
                        {
                            step["originalRotation"]["x"].AsFloat = step["rotation"]["x"].AsFloat;
                            step["originalRotation"]["y"].AsFloat = step["rotation"]["y"].AsFloat;
                            step["originalRotation"]["z"].AsFloat = step["rotation"]["z"].AsFloat;
                            step["originalRotation"]["w"].AsFloat = step["rotation"]["w"].AsFloat;
                        }
                        Quaternion rotation = new Quaternion(step["originalRotation"]["x"].AsFloat, step["originalRotation"]["y"].AsFloat, step["originalRotation"]["z"].AsFloat, step["originalRotation"]["w"].AsFloat);

                        rotation *= Quaternion.Euler(Vector3.right * footXRotationOffset);
                        step["rotation"]["x"].AsFloat = rotation.x;
                        step["rotation"]["y"].AsFloat = rotation.y;
                        step["rotation"]["z"].AsFloat = rotation.z;
                        step["rotation"]["w"].AsFloat = rotation.w;
                    }
                }
                storablesDict.Add(storable["id"].Value, storable);
            }
            return (storablesDict);
        }

        public void LoadMocapFromMocapFile(Atom targetAtom, string mocapFileName)
        {
            if (moCapFileJSON == null && mocapFileName != "" && FileManagerSecure.FileExists(mocapFileName)) moCapFileJSON = SuperController.singleton.LoadJSON(mocapFileName).AsObject;
            if (moCapFileJSON != null)
            {
                RestoreAtomAnim(targetAtom, moCapFileJSON["Person"].AsObject);
                RestoreMotionAnimMaster(moCapFileJSON["CoreControl"].AsObject);
            }
            
        }

        public void LoadMocapFromSceneToAtom(Atom targetAtom, string mocapFileName)
        {
            if (motionCaptureAtomNameJSSC.val != "Scene File is not valid format" && motionCaptureAtomNameJSSC.val != "Scene File does not exist" && motionCaptureAtomNameJSSC.val != "Scene does not contain Person atoms with Motion Capture")
            {
                if (motionCaptureSceneFileJSON == null && mocapFileName != "" && FileManagerSecure.FileExists(mocapFileName)) motionCaptureSceneFileJSON = (JSONClass)SuperController.singleton.LoadJSON(mocapFileName);
                if (motionCaptureSceneFileJSON != null)
                {
                    // Get the source atom from
                    JSONArray sceneFileAtomsJSON = motionCaptureSceneFileJSON["atoms"].AsArray;
                    JSONClass sceneFileSourceAtomJSON = null;
                    JSONClass sceneFileCoreControlAtomJSON = null;
                    if (sceneFileAtomsJSON != null)
                    {
                        List<string> personAtomNames = new List<string>();
                        foreach (JSONClass sceneFileAtomJSON in sceneFileAtomsJSON)
                        {
                            if (sceneFileAtomJSON["id"].Value == "CoreControl")
                            {
                                sceneFileCoreControlAtomJSON = sceneFileAtomJSON;
                                if (sceneFileSourceAtomJSON != null) break;
                            }
                            if (sceneFileAtomJSON["type"] != null && sceneFileAtomJSON["type"].Value == "Person")
                            {
                                if (sceneFileAtomJSON["id"].Value == motionCaptureAtomNameJSSC.val)
                                {
                                    sceneFileSourceAtomJSON = sceneFileAtomJSON;
                                    if (sceneFileCoreControlAtomJSON != null) break;
                                }
                            }
                        }
                    }
                    RestoreAtomAnim(targetAtom, sceneFileSourceAtomJSON);
                    RestoreMotionAnimMaster(sceneFileCoreControlAtomJSON);
                }
            }
        }

        private void RestoreAtomAnim (Atom targetAtom, JSONClass sceneFileSourceAtomJSON)
        {

            // Check if targetAtom has Heel Adjust active
            HASetting haSetting = HeelAdjustTool.GetHASetting(targetAtom);
            float footXRotationOffset = 0f;
            if (haSetting != null && mocapLoadHeelAdjustJSB.val) footXRotationOffset = haSetting._footRotation.x;

            // Create a lookup Dictionary for storables in the source atom
            Dictionary<string, JSONNode> sceneFileAtomStorablesDict = GetStorableDictionaryFromArray(sceneFileSourceAtomJSON, footXRotationOffset);

            // Process Target Atom to obtatin target Storables
            List<string> rbStorableIds = new List<string>();
            List<string> storableIDs = targetAtom.GetStorableIDs();
            List<string> nodeNames = RelativePositionComponent.GetTargetNodeChoices();

            foreach (string id in storableIDs)
            {
                if (id.EndsWith("Animation"))
                {
                    JSONStorable storable = targetAtom.GetStorableByID(id);
                    if (sceneFileAtomStorablesDict.ContainsKey(id)) storable.RestoreFromJSON((JSONClass)sceneFileAtomStorablesDict[id]);
                    else storable.RestoreAllFromDefaults();
                }
                else if (id.EndsWith("Control") && nodeNames.Contains(id))
                {
                    JSONStorable storable = targetAtom.GetStorableByID(id);
                    if (sceneFileAtomStorablesDict.ContainsKey(id)) storable.RestoreFromJSON((JSONClass)sceneFileAtomStorablesDict[id]);
                    else storable.RestoreAllFromDefaults();
                }
                else if (nodeNames.Contains(id) && id != "control")
                {
                    JSONStorable storable = targetAtom.GetStorableByID(id);
                    if (sceneFileAtomStorablesDict.ContainsKey(id)) rbStorableIds.Add(id);
                    else storable.RestoreAllFromDefaults();
                }
            }
            foreach (string storableId in rbStorableIds)
            {
                JSONStorable rbStorable = targetAtom.GetStorableByID(storableId);
                rbStorable.RestoreFromJSON((JSONClass)sceneFileAtomStorablesDict[storableId]);
            }
            ClothingComponent.ClothingActions(targetAtom, ClothingActionMode.resetSim, false);
        }
        private void RestoreMotionAnimMaster(JSONClass sceneFileCoreControlAtomJSON)
        {
            foreach (JSONClass storable in sceneFileCoreControlAtomJSON["storables"].AsArray)
            {
                if (string.Equals(storable["id"], "MotionAnimationMaster"))
                {
                    storable["playbackCounter"] = "0";
                    storable["startTimestep"] = "0";
                    /*                           if (storable["stopTimestep"].AsFloat < SuperController.singleton.motionAnimationMaster.totalTime)
                                               {
                                                   storable["stopTimestep"].AsFloat = SuperController.singleton.motionAnimationMaster.totalTime;
                                               }
                                               if (storable["recordedLength"].AsFloat < SuperController.singleton.motionAnimationMaster.totalTime - SuperController.singleton.motionAnimationMaster.loopbackTime)
                                               {
                                                   storable["recordedLength"].AsFloat = SuperController.singleton.motionAnimationMaster.totalTime - SuperController.singleton.motionAnimationMaster.loopbackTime;
                                               }*/
                    SuperController.singleton.motionAnimationMaster.RestoreFromJSON(storable);
                    break;
                }
            }
            SuperController.singleton.motionAnimationMaster.StartPlayback();
        }

    }

    public class WorldScaleComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableFloat worldScaleAbsJSF;
        public JSONStorableFloat worldScaleIncrementJSF;

        public WorldScaleComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            worldScaleAbsJSF = new JSONStorableFloat("worldScaleAbs", 1f, 0.1f, 20f);
            worldScaleIncrementJSF = new JSONStorableFloat("worldScaleIncrement", 0.1f, -1f, 1f);

            RegisterParam(worldScaleAbsJSF);
            RegisterParam(worldScaleIncrementJSF);
        }
    }

    public class UserPreferencesComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableEnumStringChooser shaderQualityJSEnum;
        public JSONStorableEnumStringChooser msaaLevelJSEnum;

        public JSONStorableEnumStringChooser pixelLightCountJSEnum;
        public JSONStorableEnumStringChooser smoothPassesJSEnum;
        public JSONStorableEnumStringChooser glowEffectsJSEnum;
        public JSONStorableEnumStringChooser physicsRateJSEnum;
        public JSONStorableEnumStringChooser physicsUpdateCapJSEnum;

        public JSONStorableBool softbodyPhysicsJSB;
        public JSONStorableBool desktopVSyncJSB;
        public JSONStorableBool realtimeReflectionProbesJSB;
        public JSONStorableBool mirrorReflectionsJSB;
        public JSONStorableBool hqPhysicsJSB;

        public JSONStorableFloat renderScaleJSF;

        public UserPreferencesComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            shaderQualityJSEnum = new JSONStorableEnumStringChooser("shaderQuality", UserPrefShaderQuality.enumManifestName, UserPrefShaderQuality.high, "Shader Quality");
            msaaLevelJSEnum = new JSONStorableEnumStringChooser("msaaLevel", UserPrefMSAALevel.enumManifestName, UserPrefMSAALevel.x8, "MSAA Level");
            pixelLightCountJSEnum = new JSONStorableEnumStringChooser("pixelLightCount", UserPrefPixelLightCount.enumManifestName, UserPrefPixelLightCount.two, "Pixel Light Count");
            smoothPassesJSEnum = new JSONStorableEnumStringChooser("smoothPasses", UserPrefSmoothPasses.enumManifestName, UserPrefSmoothPasses.two, "Smooth Passes");

            glowEffectsJSEnum = new JSONStorableEnumStringChooser("glowEffects", UserPrefGlowEffects.enumManifestName, UserPrefGlowEffects.off, "Glow Effects");
            physicsRateJSEnum = new JSONStorableEnumStringChooser("physicsRate", UserPrefPhysicsRate.enumManifestName, UserPrefPhysicsRate.auto, "Physics Rate");
            physicsUpdateCapJSEnum = new JSONStorableEnumStringChooser("physicsUpdateCap", UserPrefPhysicsUpdateCap.enumManifestName, UserPrefPhysicsUpdateCap.two, "Physics Update Cap");

            softbodyPhysicsJSB = new JSONStorableBool("softbodyPhysics", true);
            desktopVSyncJSB = new JSONStorableBool("desktopVSync", true);
            realtimeReflectionProbesJSB = new JSONStorableBool("realtimeReflectionProbes", true);
            mirrorReflectionsJSB = new JSONStorableBool("mirrorReflections", true);
            hqPhysicsJSB = new JSONStorableBool("hqPhysics", true);

            renderScaleJSF = new JSONStorableFloat("renderScale", 1f, 0.5f, 2f);

            RegisterParam(shaderQualityJSEnum);
            RegisterParam(msaaLevelJSEnum);

            RegisterParam(pixelLightCountJSEnum);
            RegisterParam(smoothPassesJSEnum);
            RegisterParam(glowEffectsJSEnum);
            RegisterParam(physicsRateJSEnum);
            RegisterParam(physicsUpdateCapJSEnum);

            RegisterParam(softbodyPhysicsJSB);
            RegisterParam(desktopVSyncJSB);
            RegisterParam(realtimeReflectionProbesJSB);
            RegisterParam(mirrorReflectionsJSB);
            RegisterParam(hqPhysicsJSB);
            RegisterParam(renderScaleJSF);
        }
    }


    public class ShowVRHandsComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableBool toggleShowLeftVRHandJSB;
        public JSONStorableBool toggleShowRightVRHandJSB;
        public JSONStorableEnumStringChooser leapHandsControlJSE;
        public JSONStorableBool enableLeapHandsJSB;

        public ShowVRHandsComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            toggleShowLeftVRHandJSB = new JSONStorableBool("toggleShowLeftVRHand", true);
            toggleShowRightVRHandJSB = new JSONStorableBool("toggleShowRightVRHand", true);
            var exclusion = new List<int>();
            exclusion.Add(LeftRight.neither);
            leapHandsControlJSE = new JSONStorableEnumStringChooser("leapHandsControl", LeftRight.enumManifestName, LeftRight.both, "Toggle VR Hand", exclusion);

            enableLeapHandsJSB = new JSONStorableBool("enableLeapHands", true);


            RegisterParam(toggleShowLeftVRHandJSB);
            RegisterParam(toggleShowRightVRHandJSB);
            RegisterParam(enableLeapHandsJSB);
            RegisterParam(leapHandsControlJSE);
        }
        public void SetLeapMotion()
        {
            if (leapHandsControlJSE.val == LeftRight.left || leapHandsControlJSE.val == LeftRight.both) UserPreferences.singleton.leapHandModelControl.leftHandEnabled = enableLeapHandsJSB.val;

            if (leapHandsControlJSE.val == LeftRight.right || leapHandsControlJSE.val == LeftRight.both) UserPreferences.singleton.leapHandModelControl.rightHandEnabled = enableLeapHandsJSB.val;
        }
        public void ToggleLeapMotion()
        {
            if (leapHandsControlJSE.val == LeftRight.left || leapHandsControlJSE.val == LeftRight.both) UserPreferences.singleton.leapHandModelControl.leftHandEnabled = !UserPreferences.singleton.leapHandModelControl.leftHandEnabled;

            if (leapHandsControlJSE.val == LeftRight.right || leapHandsControlJSE.val == LeftRight.both) UserPreferences.singleton.leapHandModelControl.rightHandEnabled = !UserPreferences.singleton.leapHandModelControl.rightHandEnabled;
        }
        public void ToggleVRHands()
        {
            if (toggleShowLeftVRHandJSB.val) SuperController.singleton.commonHandModelControl.leftHandEnabled = !SuperController.singleton.commonHandModelControl.leftHandEnabled;
            if (toggleShowRightVRHandJSB.val) SuperController.singleton.commonHandModelControl.rightHandEnabled = !SuperController.singleton.commonHandModelControl.rightHandEnabled;
        }

        public int GetShowVRHandButtonState()
        {
            if (toggleShowLeftVRHandJSB.val && !SuperController.singleton.commonHandModelControl.leftHandEnabled) return ButtonState.inactive;
            if (toggleShowRightVRHandJSB.val && !SuperController.singleton.commonHandModelControl.rightHandEnabled) return ButtonState.inactive;
            if (!toggleShowRightVRHandJSB.val && !toggleShowRightVRHandJSB.val) return ButtonState.inactive;

            return ButtonState.active;
        }
        public int GetToggleLeapMotionButtonState()
        {
            if (leapHandsControlJSE.val == LeftRight.left || leapHandsControlJSE.val == LeftRight.both)
            {
                if (!UserPreferences.singleton.leapHandModelControl.leftHandEnabled) return ButtonState.inactive;
            }
            if (leapHandsControlJSE.val == LeftRight.right || leapHandsControlJSE.val == LeftRight.both)
            {
                if (!UserPreferences.singleton.leapHandModelControl.rightHandEnabled) return ButtonState.inactive;
            }
            return ButtonState.active;
        }
    }

    public class SwitchUIAGridComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableInt switchUIAGridTargetJSI;

        public SwitchUIAGridComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            switchUIAGridTargetJSI = new JSONStorableInt("swtichUIAScreenTarget", 1, 1, 1000);

            RegisterParam(switchUIAGridTargetJSI);
        }
    }

    public class HairColorComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONClass hairScalpControlJC;
        public JSONClass hairSimControlJC;

        public HairColorComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

        }

        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);
            if (hairScalpControlJC != null) jc["hairScalpControl"] = hairScalpControlJC;
            if (hairSimControlJC != null) jc["hairSimControl"] = hairSimControlJC;

            return jc;
        }

        public override void RestoreFromJSON(JSONClass jc, string _paramName = null)
        {
            base.RestoreFromJSON(jc, _paramName);

            if (jc["hairSimControl"] != null) hairSimControlJC = jc["hairSimControl"].AsObject;
            else hairSimControlJC = null;

            if (jc["hairScalpControl"] != null) hairScalpControlJC = jc["hairScalpControl"].AsObject;
            else hairScalpControlJC = null;

        }

        public void CopyFrom(HairColorComponent source)
        {
            base.CopyFrom(source);

            if (source.hairScalpControlJC != null) hairScalpControlJC = (JSONClass)JSONClass.Parse(source.hairScalpControlJC.ToString());
            else hairScalpControlJC = null;

            if (source.hairSimControlJC != null) hairSimControlJC = (JSONClass)JSONClass.Parse(source.hairSimControlJC.ToString());
            else hairSimControlJC = null;

        }

        public void SetHairColor(Atom atom)
        {
            if (hairScalpControlJC != null) UpdateScalpMaterials(atom, hairScalpControlJC);
            if (hairSimControlJC != null) UpdateHairMaterials(atom, hairSimControlJC);
        }

        private void UpdateHairMaterials(Atom atom, JSONNode hairProperties)
        {
            foreach (DAZHairGroup hairGroup in atom.GetComponentsInChildren<DAZHairGroup>())
            {
                HairSimControl hairControl = hairGroup.GetComponentInChildren<HairSimControl>();

                if (hairControl != null)
                {
                    hairControl.SetFloatParamValue("primarySpecularSharpness", hairProperties["primarySpecularSharpness"].AsFloat);
                    hairControl.SetFloatParamValue("secondarySpecularSharpness", hairProperties["secondarySpecularSharpness"].AsFloat);
                    hairControl.SetFloatParamValue("specularShift", hairProperties["specularShift"].AsFloat);
                    hairControl.SetFloatParamValue("randomColorPower", hairProperties["randomColorPower"].AsFloat);
                    hairControl.SetFloatParamValue("randomColorOffset", hairProperties["randomColorOffset"].AsFloat);
                    hairControl.SetFloatParamValue("diffuseSoftness", hairProperties["diffuseSoftness"].AsFloat);
                    hairControl.SetFloatParamValue("fresnelPower", hairProperties["fresnelPower"].AsFloat);
                    hairControl.SetFloatParamValue("fresnelAttenuation", hairProperties["fresnelAttenuation"].AsFloat);
                    hairControl.SetFloatParamValue("IBLFactor", hairProperties["IBLFactor"].AsFloat);
                    hairControl.SetFloatParamValue("normalRandomize", hairProperties["normalRandomize"].AsFloat);

                    JSONNode rootColorNode = hairProperties["rootColor"];
                    JSONNode tipColorNode = hairProperties["tipColor"];
                    JSONNode specColorNode = hairProperties["specularColor"];

                    HSVColor rootColor; rootColor.H = rootColorNode["h"].AsFloat; rootColor.S = rootColorNode["s"].AsFloat; rootColor.V = rootColorNode["v"].AsFloat;
                    HSVColor tipColor; tipColor.H = tipColorNode["h"].AsFloat; tipColor.S = tipColorNode["s"].AsFloat; tipColor.V = tipColorNode["v"].AsFloat;
                    HSVColor specColor; specColor.H = specColorNode["h"].AsFloat; specColor.S = specColorNode["s"].AsFloat; specColor.V = specColorNode["v"].AsFloat;

                    hairControl.SetColorParamValue("rootColor", rootColor);
                    hairControl.SetColorParamValue("tipColor", tipColor);
                    hairControl.SetFloatParamValue("colorRolloff", hairProperties["colorRolloff"].AsFloat);
                    hairControl.SetColorParamValue("specularColor", specColor);
                }
            }
        }
        private void UpdateScalpMaterials(Atom atom, JSONNode scalpProperties)
        {
            foreach (DAZHairGroup hairGroup in atom.GetComponentsInChildren<DAZHairGroup>())
            {
                DAZHairGroupControl hairControl = hairGroup.GetComponentInChildren<DAZHairGroupControl>();
                HairSimControl simControl = hairGroup.GetComponentInChildren<HairSimControl>();

                if (hairControl != null && simControl != null)
                {
                    DAZSkinWrapMaterialOptions scalpControl = hairGroup.GetComponentInChildren<DAZSkinWrapMaterialOptions>();

                    float currentScalpAlpha = scalpControl.GetFloatParamValue("Alpha Adjust");

                    // Don't update anything if the current alpha is almost transparent.
                    if (currentScalpAlpha > -0.97f)
                    {

                        float specularTextureOffset = scalpProperties["Specular Texture Offset"].AsFloat;
                        float specularIntensity = scalpProperties["Specular Intensity"].AsFloat;
                        float gloss = scalpProperties["Gloss"].AsFloat;
                        float specularFresnel = scalpProperties["Specular Fresnel"].AsFloat;
                        float glossTextureOffset = scalpProperties["Gloss Texture Offset"].AsFloat;
                        float giFilter = scalpProperties["Global Illumination Filter"].AsFloat;
                        float alphaAdjust = scalpProperties["Alpha Adjust"].AsFloat;
                        float diffuseTextureOffset = scalpProperties["Diffuse Texture Offset"].AsFloat;

                        JSONNode diffuseColorNode = scalpProperties["Diffuse Color"];
                        JSONNode specularColorNode = scalpProperties["Specular Color"];
                        JSONNode subsurfaceColorNode = scalpProperties["Subsurface Color"];

                        HSVColor diffuseColor; diffuseColor.H = diffuseColorNode["h"].AsFloat; diffuseColor.S = diffuseColorNode["s"].AsFloat; diffuseColor.V = diffuseColorNode["v"].AsFloat;
                        HSVColor specularColor; specularColor.H = specularColorNode["h"].AsFloat; specularColor.S = specularColorNode["s"].AsFloat; specularColor.V = specularColorNode["v"].AsFloat;
                        HSVColor subsurfaceColor; subsurfaceColor.H = subsurfaceColorNode["h"].AsFloat; subsurfaceColor.S = subsurfaceColorNode["s"].AsFloat; subsurfaceColor.V = subsurfaceColorNode["v"].AsFloat;

                        scalpControl.SetFloatParamValue("Specular Texture Offset", specularTextureOffset);
                        scalpControl.SetFloatParamValue("Specular Intensity", specularIntensity);
                        scalpControl.SetFloatParamValue("Gloss", gloss);
                        scalpControl.SetFloatParamValue("Specular Fresnel", specularFresnel);
                        scalpControl.SetFloatParamValue("Gloss Texture Offset", glossTextureOffset);
                        scalpControl.SetFloatParamValue("Global Illumination Filter", giFilter);
                        scalpControl.SetFloatParamValue("Alpha Adjust", alphaAdjust);
                        scalpControl.SetFloatParamValue("Diffuse Texture Offset", diffuseTextureOffset);

                        scalpControl.SetColorParamValue("Diffuse Color", diffuseColor);
                        scalpControl.SetColorParamValue("Specular Color", specularColor);
                        scalpControl.SetColorParamValue("Subsurface Color", subsurfaceColor);
                    }
                }
            }
        }
    }

    public class PresetLockComponent : ButtonOperationComponentBase
    {
        public JSONStorableBool generalPresetLockJSON;
        public JSONStorableBool appPresetLockJSON;
        public JSONStorableBool posePresetLockJSON;
        public JSONStorableBool animationPresetLockJSON;
        public JSONStorableBool glutePhysPresetLockJSON;
        public JSONStorableBool breastPhysPresetLockJSON;
        public JSONStorableBool pluginPresetLockJSON;
        public JSONStorableBool skinPresetLockJSON;
        public JSONStorableBool morphPresetLockJSON;
        public JSONStorableBool hairPresetLockJSON;
        public JSONStorableBool clothingPresetLockJSON;

        public JSONStorableBool toggleOffKeepPresetLockJSON;
        public JSONStorableBool toggleOnKeepPresetLockJSON;

        public JSONStorableEnumStringChooser buttonTypeJSEnum;

        public PresetLockComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;
            generalPresetLockJSON = new JSONStorableBool("generalPresetLock", false);
            appPresetLockJSON = new JSONStorableBool("appPresetLock", false);
            posePresetLockJSON = new JSONStorableBool("posePresetLock", false);
            animationPresetLockJSON = new JSONStorableBool("animationPresetLock", false);
            glutePhysPresetLockJSON = new JSONStorableBool("glutePhysPresetLock", true);
            breastPhysPresetLockJSON = new JSONStorableBool("breastPhysPresetLock", true);
            pluginPresetLockJSON = new JSONStorableBool("pluginPresetLock", false);
            skinPresetLockJSON = new JSONStorableBool("skinPresetLock", true);
            morphPresetLockJSON = new JSONStorableBool("morphPresetLock", true);
            hairPresetLockJSON = new JSONStorableBool("hairPresetLock", true);
            clothingPresetLockJSON = new JSONStorableBool("clothingPresetLock", false);

            toggleOffKeepPresetLockJSON = new JSONStorableBool("toggleOffKeepPresetLock", true);
            toggleOnKeepPresetLockJSON = new JSONStorableBool("toggleOnKeepPresetLock", true);

            RegisterParam(generalPresetLockJSON);
            RegisterParam(appPresetLockJSON);
            RegisterParam(posePresetLockJSON);
            RegisterParam(animationPresetLockJSON);
            RegisterParam(glutePhysPresetLockJSON);
            RegisterParam(breastPhysPresetLockJSON);
            RegisterParam(pluginPresetLockJSON);
            RegisterParam(skinPresetLockJSON);
            RegisterParam(morphPresetLockJSON);
            RegisterParam(hairPresetLockJSON);
            RegisterParam(clothingPresetLockJSON);

            RegisterParam(toggleOffKeepPresetLockJSON);
            RegisterParam(toggleOnKeepPresetLockJSON);
        }

        public int GetPresetLockAtomState(Atom atom)
        {
            int state = ButtonState.active;

            List<PresetManagerControl> pmControlList = atom.presetManagerControls;
            foreach (PresetManagerControl pmc in pmControlList)
            {
                if ((pmc.name == "geometry" && generalPresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "AppearancePresets" && appPresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "PosePresets" && posePresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "AnimationPresets" && animationPresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "FemaleGlutePhysicsPresets" && glutePhysPresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "FemaleBreastPhysicsPresets" && breastPhysPresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "PluginPresets" && pluginPresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "SkinPresets" && skinPresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "MorphPresets" && morphPresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "HairPresets" && hairPresetLockJSON.val && !pmc.lockParams) ||
                    (pmc.name == "ClothingPresets" && clothingPresetLockJSON.val && !pmc.lockParams)
                    )
                {
                    state = ButtonState.inactive;
                    break;
                }
            }
            if (state == ButtonState.active && toggleOnKeepPresetLockJSON.val && !atom.keepParamLocksWhenPuttingBackInPoolJSON.val) state = ButtonState.inactive;

            return (state);
        }
        public void ActionTogglePresetLock(Atom atom, int currentState)
        {
            bool switchOn = false;
            if (currentState == 1) switchOn = true;
            List<PresetManagerControl> pmControlList = atom.presetManagerControls;
            foreach (PresetManagerControl pmc in pmControlList)
            {
                if ((pmc.name == "geometry" && generalPresetLockJSON.val) ||
                    (pmc.name == "AppearancePresets" && appPresetLockJSON.val) ||
                    (pmc.name == "PosePresets" && posePresetLockJSON.val) ||
                    (pmc.name == "AnimationPresets" && animationPresetLockJSON.val) ||
                    (pmc.name == "FemaleGlutePhysicsPresets" && glutePhysPresetLockJSON.val) ||
                    (pmc.name == "FemaleBreastPhysicsPresets" && breastPhysPresetLockJSON.val) ||
                    (pmc.name == "PluginPresets" && pluginPresetLockJSON.val) ||
                    (pmc.name == "SkinPresets" && skinPresetLockJSON.val) ||
                    (pmc.name == "MorphPresets" && morphPresetLockJSON.val) ||
                    (pmc.name == "HairPresets" && hairPresetLockJSON.val) ||
                    (pmc.name == "ClothingPresets" && clothingPresetLockJSON.val)
                    )
                {
                    pmc.lockParams = switchOn;
                }
            }
            if (currentState == 1 && toggleOnKeepPresetLockJSON.val) atom.keepParamLocksWhenPuttingBackInPoolJSON.val = true;
            else if (currentState == 2 && toggleOffKeepPresetLockJSON.val) atom.keepParamLocksWhenPuttingBackInPoolJSON.val = false;
        }
    }

    public class SkinPresetDecalComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public JSONStorableBool decalFaceLoadJSBool;
        public JSONStorableBool decalBodyLoadJSBool;
        public JSONStorableBool decalLimbsLoadJSBool;
        public JSONStorableBool decalGenLoadJSBool;

        public SkinPresetDecalComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            decalFaceLoadJSBool = new JSONStorableBool("decalFaceLoad", false);
            RegisterParam(decalFaceLoadJSBool);
            decalBodyLoadJSBool = new JSONStorableBool("decalBodyLoad", false);
            RegisterParam(decalBodyLoadJSBool);
            decalLimbsLoadJSBool = new JSONStorableBool("decalLimbsLoad", false);
            RegisterParam(decalLimbsLoadJSBool);
            decalGenLoadJSBool = new JSONStorableBool("decalGenLoad", false);
            RegisterParam(decalGenLoadJSBool);
        }

        public bool AnyDecalLoadsActive()
        {
            return decalFaceLoadJSBool.val || decalBodyLoadJSBool.val || decalLimbsLoadJSBool.val || decalGenLoadJSBool.val;
        }
    }

    public class PluginsLoadComponent : ButtonOperationComponentBase
    {
        public readonly List<ButtonPluginLoad> pluginLoadList;

        public JSONStorableBool useLatestVARJSBool;
        public JSONStorableBool openPluginUIOnLoadJSB;


        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public PluginsLoadComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;
            pluginLoadList = new List<ButtonPluginLoad>();

            useLatestVARJSBool = new JSONStorableBool("useLatestVAR", true);
            RegisterParam(useLatestVARJSBool);

            openPluginUIOnLoadJSB = new JSONStorableBool("openPluginUIOnLoad", false);
            RegisterParam(openPluginUIOnLoadJSB);

            ButtonPluginLoad tempPIL = new ButtonPluginLoad();
            pluginLoadList.Add(tempPIL);
        }

        public void CopyFrom(PluginsLoadComponent sourcePluginsLoadComponent)
        {
            base.CopyFrom(sourcePluginsLoadComponent);
            pluginLoadList.Clear();
            sourcePluginsLoadComponent.pluginLoadList.ForEach((sourcePluginLoad) =>
            {
                ButtonPluginLoad pluginLoad = new ButtonPluginLoad();
                pluginLoad.CopyFrom(sourcePluginLoad);
                pluginLoadList.Add(pluginLoad);
            });

        }

        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);

            JSONArray pluginLoadArray = new JSONArray();
            pluginLoadList.ForEach((pluginLoad) =>
            {
                if (pluginLoad.filePathJSString.val != "") pluginLoadArray.Add(pluginLoad.GetJSON());
            });

            jc["pluginLoads"] = pluginLoadArray;

            return (jc);
        }

        public void LoadJSON(JSONClass pluginLoadsComponentJSON, string uiapPackageName)
        {
            base.RestoreFromJSON(pluginLoadsComponentJSON);

            JSONArray pluginLoadsArrayJSON = pluginLoadsComponentJSON["pluginLoads"].AsArray;

            for (int i = 0; i < pluginLoadsArrayJSON.Count; i++)
            {
                JSONClass pluginLoadJSON = pluginLoadsArrayJSON[i].AsObject;
                ButtonPluginLoad pluginLoad;
                if (i >= pluginLoadList.Count)
                {
                    pluginLoad = new ButtonPluginLoad();
                    pluginLoadList.Add(pluginLoad);
                }
                else pluginLoad = pluginLoadList[i];

                pluginLoad.LoadJSON(pluginLoadJSON, uiapPackageName);
            }
        }


        public void LoadPluginsAsPreset(Atom atom)
        {
            int pluginCount = 1;
            MVRPluginManager manager;

            if (atom.type == "SessionPluginManager") manager = UIAGlobals.mvrScript.manager;
            else manager = atom.GetStorableByID("PluginManager") as MVRPluginManager;

            JSONClass currentPlugins = manager.GetJSON(true, true, true);

            JSONClass pluginPresetJSON = new JSONClass();
            pluginPresetJSON["setUnlistedParamsToDefault"].AsBool = true;

            JSONArray pluginPresetStorablesJSON = new JSONArray();
            pluginPresetJSON["storables"] = pluginPresetStorablesJSON;

            JSONClass pluginMgrStorableJSON = new JSONClass();
            pluginPresetStorablesJSON.Add(pluginMgrStorableJSON);
            pluginMgrStorableJSON["id"] = "PluginManager";

            JSONClass pluginMgrPluginsListJSON = new JSONClass();
            pluginMgrStorableJSON["plugins"] = pluginMgrPluginsListJSON;

            List<string> pluginRefsOfFirstLoadedPlugin = new List<string>();
            string firstPluginFilePath = "";

            foreach (ButtonPluginLoad buttonPluginToLoad in pluginLoadList)
            {
                if (buttonPluginToLoad.filePathJSString.val != "" && buttonPluginToLoad.filePathJSString.val != null)
                {
                    string normalFilePath = SuperController.singleton.NormalizePath(buttonPluginToLoad.filePathJSString.val);

                    bool isPluginLoaded = PluginUtils.IsPluginLoaded(currentPlugins, normalFilePath);

                    if (useLatestVARJSBool.val) normalFilePath = FileUtils.GetLatestVARPath(normalFilePath);

                    if (pluginLoadList.First() == buttonPluginToLoad)
                    {
                        pluginRefsOfFirstLoadedPlugin = PluginUtils.GetPluginRefsOfType(manager, normalFilePath);
                        firstPluginFilePath = normalFilePath;
                    }
                    

                    if (!buttonPluginToLoad.singleInstancePerAtomJSBool.val || buttonTypeJSEnum.val == UIAButtonOpType.spawnAtom || !isPluginLoaded)
                    {
                        pluginMgrPluginsListJSON["plugin#" + pluginCount.ToString()] = normalFilePath;

                        if (buttonPluginToLoad.pluginSaveDataJSString.val != "" && buttonPluginToLoad.pluginSaveDataJSString.val != null)
                        {
                            foreach (JSONClass newPluginStorable in buttonPluginToLoad.pluginSaveJSON)
                            {
                                string pluginStorableID = newPluginStorable["id"].Value;
                                int pluginIDEndIndex = pluginStorableID.IndexOf("_");
                                newPluginStorable["id"].Value = "plugin#" + pluginCount.ToString() + pluginStorableID.Substring(pluginIDEndIndex, pluginStorableID.Length - pluginIDEndIndex);
                                pluginPresetStorablesJSON.Add(newPluginStorable);
                            }
                        }
                        pluginCount++;
                    }
                    else if (isPluginLoaded && buttonPluginToLoad.singleInstancePerAtomJSBool.val && buttonPluginToLoad._restorePluginDataIfLoaded)
                    {
                        List<string> pluginRefsInAtom = PluginUtils.GetPluginRefsOfType(manager, normalFilePath);
                        if (pluginRefsInAtom.Count > 0)
                        {
                            if (buttonPluginToLoad.pluginSaveDataJSString.val != "" && buttonPluginToLoad.pluginSaveDataJSString.val != null)
                            {
                                foreach (JSONClass newPluginStorable in buttonPluginToLoad.pluginSaveJSON)
                                {
                                    string pluginStorableID = newPluginStorable["id"].Value;
                                    int pluginIDEndIndex = pluginStorableID.IndexOf("_");
                                    newPluginStorable["id"].Value = pluginRefsInAtom[0].ToString() + pluginStorableID.Substring(pluginIDEndIndex, pluginStorableID.Length - pluginIDEndIndex);
                                    JSONStorable pluginStorable = atom.GetStorableByID(newPluginStorable["id"].Value);
                                    pluginStorable.RestoreFromJSON(newPluginStorable, true, true);
                                }
                            }
                        }
                    }
                }

            }          

            if (pluginCount > 1) MergePluginPreset(atom, pluginPresetJSON);

            if (firstPluginFilePath != "" && openPluginUIOnLoadJSB.val)
            {
                var finalPluginRefsOfFirstLoadedPlugin = PluginUtils.GetPluginRefsOfType(manager, firstPluginFilePath);

                int pluginSlot = int.Parse(finalPluginRefsOfFirstLoadedPlugin.Except(pluginRefsOfFirstLoadedPlugin).First().Substring(7));

                SelectAndOpenUI(atom, pluginSlot);
            }
        }

        static public void SelectAndOpenUI(Atom atom, string pluginRef)
        {
            if (atom.name == "CoreControl" || atom.type == "SessionPluginManager")
            {
                SuperController.singleton.ShowMainHUDMonitor();
                SuperController.singleton.activeUI = SuperController.ActiveUI.MainMenu;
            }
            else
            {
                SuperController.singleton.SelectController(atom.mainController, false, false, true);
                SuperController.singleton.ShowMainHUDMonitor();
            }

            SuperController.singleton.StartCoroutine(WaitForUI(atom, pluginRef));
        }

        static public void SelectAndOpenUI(Atom atom, int pluginSlot)
        {
            string pluginRef = "plugin#" + pluginSlot.ToString() + "_";
            SelectAndOpenUI(atom, pluginRef);
        }

        static private IEnumerator WaitForUI(Atom atom, string pluginRef)
        {
            var expiration = Time.unscaledTime + 1f;
            UITabSelector selector;
            while (Time.unscaledTime < expiration)
            {
                yield return 0;
                if (atom.name == "CoreControl" || atom.type == "SessionPluginManager") selector = SuperController.singleton.mainMenuTabSelector;
                else selector = atom.gameObject.GetComponentInChildren<UITabSelector>();
                if (selector == null) continue;

                if (atom.type == "SessionPluginManager") SuperController.singleton.mainMenuTabSelector.SetActiveTab("TabSessionPlugins");
                else if (atom.name == "CoreControl") SuperController.singleton.mainMenuTabSelector.SetActiveTab("TabScenePlugins");
                else selector.SetActiveTab("Plugins");

                IEnumerator enumerator1 = selector.transform.GetEnumerator();
                while (enumerator1.MoveNext())
                {
                    UITab component = ((Component)enumerator1.Current).GetComponent<UITab>();

                    if ((UnityEngine.Object)component != (UnityEngine.Object)null)
                    {
                        foreach (var scriptUI in component.GetComponentsInChildren<MVRScriptUI>())
                        {
                            
                            scriptUI.closeButton?.onClick.Invoke();
                        }

                        foreach (var scriptController in component.GetComponentsInChildren<MVRScriptControllerUI>())
                        {
                            if (scriptController.label.text.StartsWith(pluginRef))
                            {
                                scriptController.openUIButton?.onClick?.Invoke();
                                break;
                            }
                        }

                    }
                }

                yield break;
            }
        }



        public static void MergePluginPreset(Atom atom, JSONClass pluginPresetJSON)
        {
            PresetLockStore tempPresetLockStore = new PresetLockStore();
            if (atom.type == "Person") tempPresetLockStore.StorePresetLocks(atom, PresetLoadSettings.suppressPresetLocksJSB.val);

            PresetManager pm;
            if (atom.name == "CoreControl")
            {
                if (atom.type == "SessionPluginManager") pm = atom.GetComponentInChildren<PresetManager>();
                else
                {
                    JSONStorable js = atom.GetStorableByID("PluginManagerPresets");
                    pm = js.GetComponentInChildren<PresetManager>();
                }
            }
            else
            {
                if (atom.type != "Person") pm = atom.GetComponentInChildren<PresetManager>();
                else
                {
                    JSONStorable js = atom.GetStorableByID("PluginPresets");
                    pm = js.GetComponentInChildren<PresetManager>();
                }
            }
            atom.SetLastRestoredData(pluginPresetJSON, true, true);
            pm.LoadPresetFromJSON(pluginPresetJSON, true);

            if (atom.type == "Person") tempPresetLockStore.RestorePresetLocks(atom);
        }

        public void UnloadPlugins(Atom atom)
        {
            if (atom.type != "SessionPluginManager")
            {
                MVRPluginManager manager = atom.GetStorableByID("PluginManager") as MVRPluginManager;

                JSONClass emptyManager = new JSONClass();
                emptyManager["id"] = "PluginManager";
                manager.LateRestoreFromJSON(emptyManager);
            }

        }
    }

    public class ButtonPluginLoad : JSONStorableObject
    {
        public bool _restorePluginDataIfLoaded
        {
            get { return restorePluginDataIfLoadedJSBool.val; }
            set { restorePluginDataIfLoadedJSBool.val = value; }
        }
        public string _pluginSaveData
        {
            get { return pluginSaveDataJSString.val; }
            set { pluginSaveDataJSString.val = value; }
        }
        public JSONArray pluginSaveJSON;

        public JSONStorableString filePathJSString;
        public JSONStorableBool singleInstancePerAtomJSBool;
        public JSONStorableBool restorePluginDataIfLoadedJSBool;
        public JSONStorableString pluginSaveDataJSString;
        public ButtonPluginLoad()
        {
            filePathJSString = new JSONStorableString("filePath", "");
            RegisterParam(filePathJSString);
            singleInstancePerAtomJSBool = new JSONStorableBool("singleInstancePerAtom", true);
            RegisterParam(singleInstancePerAtomJSBool);
            restorePluginDataIfLoadedJSBool = new JSONStorableBool("restorePluginDataIfLoaded", false);
            RegisterParam(restorePluginDataIfLoadedJSBool);


            pluginSaveDataJSString = new JSONStorableString("saveDataSummary", "");
            RegisterParam(pluginSaveDataJSString, false);
        }

        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass node = base.GetJSON(jspLoadExclusions);

            if (pluginSaveJSON != null) node["pluginSaveJSON"] = pluginSaveJSON.AsArray;
            return node;
        }
        public void CopyFrom(ButtonPluginLoad bpl)
        {
            base.CopyFrom(bpl);
            pluginSaveJSON = bpl.pluginSaveJSON;
        }

        public void LoadJSON(JSONClass pluginLoadJSON, string uiapPackageName)
        {
            base.RestoreFromJSON(pluginLoadJSON);
            pluginSaveJSON = new JSONArray();
            pluginSaveDataJSString.val = "";

            if (uiapPackageName != "" && !filePathJSString.val.Contains(":") && FileManagerSecure.FileExists(uiapPackageName + ":/" + filePathJSString.val)) filePathJSString.val = uiapPackageName + ":/" + filePathJSString.val;

            if (pluginLoadJSON["pluginSaveJSON"] != null)
            {
                pluginSaveJSON = pluginLoadJSON["pluginSaveJSON"].AsArray;
                string tempString = pluginSaveJSON.ToString();
                pluginSaveDataJSString.val = tempString.Substring(0, Math.Min(tempString.Length, 500));
            }
        }
    }
    public class PluginSettingComponent : ButtonOperationComponentBase
    {
        public JSONStorableStringChooser targetParamNameOnJSSC;
        public JSONStorableStringChooser targetParamNameOffJSSC;
        public JSONStorableStringChooser pluginTypeJSSC;

        public readonly PluginVariables pluginVariablesButtonOn;
        public readonly PluginVariables pluginVariablesButtonOff;

        public JSONStorableEnumStringChooser buttonTypeJSEnum;

        public Dictionary<string, int> atomButtonToggleStates;

        public PluginSettingComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;

            pluginTypeJSSC = new JSONStorableStringChooser("pluginType", null, "", "", PluginTypeUpdated);
            RegisterParam(pluginTypeJSSC);

            targetParamNameOnJSSC = new JSONStorableStringChooser("targetParamNameOn", null, "","");
            RegisterParam(targetParamNameOnJSSC);

            targetParamNameOffJSSC = new JSONStorableStringChooser("targetParamNameOff", null, "", "");
            RegisterParam(targetParamNameOffJSSC);

            pluginVariablesButtonOn = new PluginVariables(this);
            pluginVariablesButtonOff = new PluginVariables(this);

            atomButtonToggleStates = new Dictionary<string, int>();
        }

        public void ButtonTypeUpdated()
        {
            pluginTypeJSSC.choices = GetPluginTypeChoices();
            if (pluginTypeJSSC.val =="") pluginTypeJSSC.val = pluginTypeJSSC.choices[0]; 
        }
        private void PluginTypeUpdated(string pluginType)
        {
            pluginVariablesButtonOn._pluginVarDict.Clear();
            pluginVariablesButtonOff._pluginVarDict.Clear();
            RefreshPluginParamNameChoices();
        }

        public void RefreshPluginParamNameChoices()
        {
            List<string> targetParamNameChoices;
            if (buttonTypeJSEnum.val == UIAButtonOpType.pluginBoolFalse || buttonTypeJSEnum.val == UIAButtonOpType.pluginBoolTrue || buttonTypeJSEnum.val == UIAButtonOpType.pluginBoolToggle) targetParamNameChoices = GetTargetPluginParamNameChoices(PluginVariableType.vBool);
            else if (buttonTypeJSEnum.val == UIAButtonOpType.pluginAction || buttonTypeJSEnum.val == UIAButtonOpType.pluginActionToggle) targetParamNameChoices = GetTargetPluginParamNameChoices(PluginVariableType.vAction);
            else return;

            targetParamNameOnJSSC.choices = targetParamNameChoices;
            targetParamNameOffJSSC.choices = targetParamNameChoices;

            if (targetParamNameOnJSSC.val=="") targetParamNameOnJSSC.val = targetParamNameChoices[0];
            if (buttonTypeJSEnum.val == UIAButtonOpType.pluginActionToggle || buttonTypeJSEnum.val == UIAButtonOpType.pluginBoolToggle)
            {
                if (targetParamNameOffJSSC.val == "" || targetParamNameOffJSSC.val =="No Actions available") targetParamNameOffJSSC.val = targetParamNameChoices[0];
            }
            else targetParamNameOffJSSC.val = "";
        }


        public void AtomNameUpdate(string oldName, string newName)
        {
            foreach (string key in atomButtonToggleStates.Keys.ToList())
            {
                if (key == oldName)
                {
                    atomButtonToggleStates[newName] = atomButtonToggleStates[key];
                    atomButtonToggleStates.Remove(key);
                }
            }
        }

        public void AtomRemovedUpdate(string oldName)
        {
            foreach (string key in atomButtonToggleStates.Keys.ToList())
            {
                if (key == oldName) atomButtonToggleStates.Remove(key);
            }
        }

        public void CopyFrom(PluginSettingComponent pluginSettingComponent)
        {
            base.CopyFrom(pluginSettingComponent);

            pluginVariablesButtonOn.CopyFrom(pluginSettingComponent.pluginVariablesButtonOn);
            pluginVariablesButtonOff.CopyFrom(pluginSettingComponent.pluginVariablesButtonOff);

            atomButtonToggleStates.Clear();
        }

        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);
            if (buttonTypeJSEnum.val != UIAButtonOpType.pluginOpenUI && buttonTypeJSEnum.val != UIAButtonOpType.pluginToggleOpenUI)
            {
                jc["pluginVariablesOn"] = pluginVariablesButtonOn.GetJSON();
                jc["pluginVariablesOff"] = pluginVariablesButtonOff.GetJSON();
            }
            
            return (jc);
        }

        public void RestoreFromJSON(JSONClass pluginSettingComponentJSON, int uiapFormatVersion)
        {
            base.RestoreFromJSON(pluginSettingComponentJSON, null);

            if (buttonTypeJSEnum.val != UIAButtonOpType.pluginOpenUI && buttonTypeJSEnum.val != UIAButtonOpType.pluginToggleOpenUI)
            {
                if (pluginSettingComponentJSON["pluginVariablesOn"] != null) pluginVariablesButtonOn.RestoreFromJSON(pluginSettingComponentJSON["pluginVariablesOn"].AsObject, uiapFormatVersion);
                if (pluginSettingComponentJSON["pluginVariablesOff"] != null) pluginVariablesButtonOff.RestoreFromJSON(pluginSettingComponentJSON["pluginVariablesOff"].AsObject, uiapFormatVersion);
            }
                

            atomButtonToggleStates.Clear();
        }
        public List<string> GetTargetPluginParamNameChoices(int paramType)
        {
            List<string> choices = new List<string>();
            bool pluginOfTypeFound = false;

            foreach (MVRScript script in PluginUtils.GetSceneAndSessionPlugins())
            {
                if (script.name.EndsWith(pluginTypeJSSC.val))
                {
                    UpdateParamsFromPluginStorable(script, paramType, choices);
                    pluginOfTypeFound = true;
                }
            }

            if (choices.Count < 1)
            {
                if (!pluginOfTypeFound) choices.Add("No Plugin of type loaded");
                else
                {
                    if (paramType == PluginVariableType.vAction) { choices.Add("No Actions available"); }
                    else { choices.Add("No Bools available"); }
                }
            }
            choices.Sort();
            return (choices);
        }
        private void UpdateParamsFromPluginStorable(JSONStorable storable, int paramType, List<string> choices)
        {
            if (paramType == PluginVariableType.vAction)
            {
                foreach (string actionName in storable.GetActionNames())
                {
                    if (!UIAConsts.VAMSaveRestoreActions.Contains(actionName) && !choices.Contains(actionName)) { choices.Add(actionName); }
                }
            }
            else
            {
                foreach (string boolName in storable.GetBoolParamNames())
                {
                    if (!choices.Contains(boolName)) { choices.Add(boolName); }
                }
                if (paramType == PluginVariableType.vAll)
                {
                    foreach (string floatName in storable.GetFloatParamNames())
                    {
                        if (!choices.Contains(floatName)) { choices.Add(floatName); }
                    }
                    foreach (string stringName in storable.GetStringParamNames())
                    {
                        if (!choices.Contains(stringName)) { choices.Add(stringName); }
                    }
                    foreach (string stringChooserName in storable.GetStringChooserParamNames())
                    {
                        if (!choices.Contains(stringChooserName)) { choices.Add(stringChooserName); }
                    }
                    foreach (string urlName in storable.GetUrlParamNames())
                    {
                        //  if (!choices.Contains(urlName)) { choices.Add(urlName); }
                    }
                    foreach (string colorName in storable.GetColorParamNames())
                    {
                        if (!choices.Contains(colorName)) { choices.Add(colorName); }
                    }
                }
            }
        }


        public List<string> GetPluginTypeChoices()
        {
            List<string> popupChoices = new List<string>();

            foreach (MVRScript script in PluginUtils.GetSceneAndSessionPlugins())
            {
                string pluginType = script.name.Substring(script.name.IndexOf('_') + 1);
                if (!popupChoices.Contains(pluginType)) popupChoices.Add(pluginType);
            }

            if (popupChoices.Count == 0) { popupChoices.Add("No plugins in Scene"); }
            popupChoices.Sort();
            return (popupChoices);
        }

        public float GetMinFloatValue(string varName)
        {
            float minFloat = 100000000000f;

            foreach (MVRScript script in PluginUtils.GetSceneAndSessionPlugins())
            {
                if (script.GetFloatParamNames().Contains(varName))
                {
                    JSONStorableFloat floatJSON = script.GetFloatJSONParam(varName);
                    if (floatJSON.min < minFloat) minFloat = floatJSON.min;
                }
            }
            if (minFloat == 100000000000f) minFloat = 0f;
            return (minFloat);
        }
        public float GetMaxFloatValue(string varName)
        {
            float maxFloat = -100000000000f;

            foreach (MVRScript script in PluginUtils.GetSceneAndSessionPlugins())
            {
                if (script.GetFloatParamNames().Contains(varName))
                {
                    JSONStorableFloat floatJSON = script.GetFloatJSONParam(varName);
                    if (floatJSON.max > maxFloat) maxFloat = floatJSON.max;
                }
            }
            if (maxFloat == -100000000000f) maxFloat = 1f;
            return (maxFloat);
        }

        public List<string> GetStringChooserChoices(string varName)
        {
            List<string> choices = new List<string>();

            foreach (MVRScript script in PluginUtils.GetSceneAndSessionPlugins())
            {
                if (script.GetStringChooserParamNames().Contains(varName))
                {
                    List<string> pluginSCChoices = script.GetStringChooserJSONParamChoices(varName);
                    if (pluginSCChoices != null)
                    {
                        foreach (string choice in pluginSCChoices)
                        {
                            if (!choices.Contains(choice)) choices.Add(choice);
                        }
                    }
                }
            }
            return (choices);
        }

        private void PreSettings(JSONStorable pluginStorable, bool toggleParamMode)
        {
            PluginVariables preActionVariables;

            if (!toggleParamMode || parentButton.GetButtonState() == 1) preActionVariables = pluginVariablesButtonOn;
            else preActionVariables = pluginVariablesButtonOff;

            // Set any pre-action plugin parameters
            foreach (KeyValuePair<string, PluginVariable> kvp in preActionVariables._pluginVarDict)
            {
                if (pluginStorable.GetBoolParamNames().Contains(kvp.Key))
                {
                    if (kvp.Key != targetParamNameOnJSSC.val)
                    {
                        JSONStorableBool boolJSON = pluginStorable.GetBoolJSONParam(kvp.Key);
                        boolJSON.val = kvp.Value.pluginBoolValue.val;
                    }

                }
                else if (pluginStorable.GetFloatParamNames().Contains(kvp.Key))
                {
                    JSONStorableFloat floatJSON = pluginStorable.GetFloatJSONParam(kvp.Key);
                    floatJSON.val = kvp.Value.pluginFloatValue.val;
                }
                else if (pluginStorable.GetStringParamNames().Contains(kvp.Key))
                {
                    JSONStorableString stringJSON = pluginStorable.GetStringJSONParam(kvp.Key);
                    stringJSON.val = kvp.Value.pluginStringValue.val;
                }
                else if (pluginStorable.GetStringChooserParamNames().Contains(kvp.Key))
                {
                    JSONStorableStringChooser stringChooserJSON = pluginStorable.GetStringChooserJSONParam(kvp.Key);
                    stringChooserJSON.val = kvp.Value.pluginStringValue.val;

                }
                else if (pluginStorable.GetUrlParamNames().Contains(kvp.Key))
                {
                    JSONStorableUrl urlJSON = pluginStorable.GetUrlJSONParam(kvp.Key);
                    urlJSON.val = kvp.Value.pluginStringValue.val;
                }
                else if (pluginStorable.GetColorParamNames().Contains(kvp.Key))
                {
                    JSONStorableColor colorJSON = pluginStorable.GetColorJSONParam(kvp.Key);
                    colorJSON.val = kvp.Value.pluginColorValue.val;
                }

            }
        }

        private List<MVRScript> GetPluginStorables(Atom atom)
        {
            List<MVRScript> pluginStorables = new List<MVRScript>();
            if (atom.type == "SessionPluginManager")
            {
                foreach (MVRScript script in PluginUtils.GetSessionPlugins())
                {
                    if (script.name.EndsWith(pluginTypeJSSC.val)) pluginStorables.Add(script);
                }
            }
            else
            {
                foreach (MVRScript pluginStorable in PluginUtils.GetPluginsFromAtom(atom))
                {
                    if (pluginStorable != null)
                    {
                        string storableName = pluginStorable.name;
                        if (storableName.EndsWith(pluginTypeJSSC.val)) pluginStorables.Add(pluginStorable);
                    }
                }
            }

            return (pluginStorables);
        }

        public void PluginBool(Atom atom, bool boolState, bool toggleParamMode)
        {
            List<MVRScript> pluginStorables = GetPluginStorables(atom);

            foreach (MVRScript storable in pluginStorables)
            {
                if (storable.GetBoolParamNames().Contains(targetParamNameOnJSSC.val))
                {
                    PreSettings(storable, toggleParamMode);
                    storable.SetBoolParamValue(targetParamNameOnJSSC.val, boolState);
                }
            }
        }
        public void PluginAction(Atom atom, bool singleActionMode, bool toggleParamMode)
        {
            string actionName = "";

            if (!toggleParamMode || parentButton.GetButtonState() == 1) actionName = targetParamNameOnJSSC.val;
            else actionName = targetParamNameOffJSSC.val;

            List<MVRScript> pluginStorables = GetPluginStorables(atom);

            foreach (MVRScript storable in pluginStorables)
            {
                if (storable != null && (!singleActionMode || storable.GetActionNames().Contains(actionName)))
                {
                    PreSettings(storable, toggleParamMode);
                    if (singleActionMode) storable.CallAction(actionName);
                }
            }

            if (toggleParamMode)
            {
                if (atomButtonToggleStates[atom.name] == ButtonState.active) atomButtonToggleStates[atom.name] = ButtonState.inactive;
                else atomButtonToggleStates[atom.name] = ButtonState.active;
            }
        }

        public void PluginOpenUI(Atom atom)
        {
            List<MVRScript> pluginStorables = GetPluginStorables(atom);
            if (pluginStorables.Count > 0)
            {

                PluginsLoadComponent.SelectAndOpenUI(atom, pluginStorables.First().name);
            }
        }

    }
    public class PluginVariables : JSONStorableObject
    {
        public Dictionary<string, PluginVariable> _pluginVarDict;
        private PluginSettingComponent parentPluginSettingComponent;

        public PluginVariables(PluginSettingComponent _parentPluginSettingComponent)
        {
            try
            {
                parentPluginSettingComponent = _parentPluginSettingComponent;
                _pluginVarDict = new Dictionary<string, PluginVariable>();
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }
        public void CopyFrom(PluginVariables pv)
        {
            if (pv != null)
            {
                base.CopyFrom(pv);

                _pluginVarDict = new Dictionary<string, PluginVariable>();
                foreach (KeyValuePair<string, PluginVariable> kvp in pv._pluginVarDict)
                {
                    PluginVariable newPV = new PluginVariable(kvp.Value.pluginVarName.val,kvp.Value.pluginVarType.val);
                    newPV.CopyFrom(kvp.Value);
                    _pluginVarDict.Add(kvp.Key, newPV);
                }
            }
        }

        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);
            JSONArray pluginVariablesArray = new JSONArray();

            foreach (KeyValuePair<string, PluginVariable> kvp in _pluginVarDict)
            {
                JSONClass variableNode = kvp.Value.GetJSON();
                pluginVariablesArray.Add(variableNode);
            }

            jc["pluginVariables"] = pluginVariablesArray;

            return jc;
        }

        public void RestoreFromJSON(JSONClass pluginVarJSON, int uiapFormatVersion)
        {
            base.RestoreFromJSON(pluginVarJSON);

            _pluginVarDict = new Dictionary<string, PluginVariable>();

            if (pluginVarJSON["pluginVariables"] != null)
            {
                JSONArray pluginVariablesArray = pluginVarJSON["pluginVariables"].AsArray;
                for (int i = 0; i < pluginVariablesArray.Count; i++)
                {
                    JSONClass variableNodeJSON = pluginVariablesArray[i].AsObject;
                    PluginVariable pluginVariable = new PluginVariable();
                    pluginVariable.RestoreFromJSON(variableNodeJSON, uiapFormatVersion);

                    _pluginVarDict.Add(variableNodeJSON["pluginVarName"], pluginVariable);
                }
            }
        }
        public List<string> GetPluginParamChoices()
        {
            List<string> displayParamChoices = new List<string>();
            List<string> paramChoices = parentPluginSettingComponent.GetTargetPluginParamNameChoices(PluginVariableType.vAll);

            foreach (string choice in paramChoices)
            {
                if (_pluginVarDict.ContainsKey(choice))
                {
                    displayParamChoices.Add("+ " + choice + " +");
                }
                else displayParamChoices.Add("  " + choice + "  ");

            }
            return (displayParamChoices);
        }

        public void SetPluginParamChoices(JSONStorableStringChooser jssc)
        {
            List<string> displayParamChoices = new List<string>();
            List<string> fullParamChoices = parentPluginSettingComponent.GetTargetPluginParamNameChoices(PluginVariableType.vAll);
            if (parentPluginSettingComponent.buttonTypeJSEnum.val == UIAButtonOpType.pluginBoolToggle || parentPluginSettingComponent.buttonTypeJSEnum.val == UIAButtonOpType.pluginBoolTrue || parentPluginSettingComponent.buttonTypeJSEnum.val == UIAButtonOpType.pluginBoolFalse)
            {
                if (fullParamChoices.Contains(parentPluginSettingComponent.targetParamNameOnJSSC.val)) fullParamChoices.Remove(parentPluginSettingComponent.targetParamNameOnJSSC.val);
            }

            List<string> notSetDisplayParamChoices = new List<string>();
            List<string> paramChoices = new List<string>();
            List<string> notSetParamChoices = new List<string>();

            foreach (string choice in fullParamChoices)
            {
                if (_pluginVarDict.ContainsKey(choice))
                {
                    paramChoices.Add(choice);
                    displayParamChoices.Add("+ " + choice + " +");
                }
                else
                {
                    notSetDisplayParamChoices.Add("  " + choice + "  ");
                    notSetParamChoices.Add(choice);
                }
            }
            foreach (KeyValuePair<string, PluginVariable> kvp in _pluginVarDict)
            {
                if (!paramChoices.Contains(kvp.Key) && !notSetParamChoices.Contains(kvp.Key))
                {
                    paramChoices.Add(kvp.Key);
                    displayParamChoices.Add("+ " + kvp.Key + " +");
                }
            }


            jssc.choices = paramChoices.Concat(notSetParamChoices).ToList();
            jssc.displayChoices = displayParamChoices.Concat(notSetDisplayParamChoices).ToList();
        }


        public int GetVarType(string varName)
        {
            int varType = PluginVariableType.vNone;

            if (_pluginVarDict.ContainsKey(varName))
            {
                varType = _pluginVarDict[varName].pluginVarType.val;
            }

            return (varType);
        }
        public bool GetVarSet(string varName)
        {
            return (_pluginVarDict.ContainsKey(varName));
        }

        public int GetVarTypeFromPlugin(string varName)
        {
            int varType = PluginUtils.GetVarTypeFromPlugin(varName, parentPluginSettingComponent.pluginTypeJSSC.val);
            return (varType);
        }
        public float GetMinFloatValue(string varName)
        {
            float minFloat = parentPluginSettingComponent.GetMinFloatValue(varName);
            if (_pluginVarDict.ContainsKey(varName))
            {
                float varValue = _pluginVarDict[varName].pluginFloatValue.val;
                if (varValue < minFloat) minFloat = varValue;
            }
            return (minFloat);
        }
        public float GetMaxFloatValue(string varName)
        {
            float maxFloat = parentPluginSettingComponent.GetMaxFloatValue(varName);
            if (_pluginVarDict.ContainsKey(varName))
            {
                float varValue = _pluginVarDict[varName].pluginFloatValue.val;
                if (varValue > maxFloat) maxFloat = varValue;
            }
            return (maxFloat);
        }
        public List<string> GetStringChooserChoices(string varName)
        {
            List<string> choices = parentPluginSettingComponent.GetStringChooserChoices(varName);
            if (choices.Count < 1) choices.Add("NO VALUES AVAILABLE");
            return (choices);
        }
    }
    public class PluginVariable : JSONStorableObject
    {
        public JSONStorableString pluginVarName;
        public JSONStorableEnumStringChooser pluginVarType;
        public JSONStorableFloat pluginFloatValue;
        public JSONStorableBool pluginBoolValue;
        public JSONStorableString pluginStringValue;
        public JSONStorableColor pluginColorValue;

        

        public PluginVariable()
        {
            pluginVarName = new JSONStorableString("pluginVarName", "");
            RegisterParam(pluginVarName);

            pluginVarType = new JSONStorableEnumStringChooser("pluginVarType", PluginVariableType.enumManifestName, PluginVariableType.vBool, "", null);
            RegisterParam(pluginVarType);
        }

        public PluginVariable(string varName, int varType)
        {
            pluginVarName = new JSONStorableString("pluginVarName", "");
            pluginVarName.val = varName;
            RegisterParam(pluginVarName, true,false);

            pluginVarType = new JSONStorableEnumStringChooser("pluginVarType", PluginVariableType.enumManifestName, PluginVariableType.vBool, "", null);
            pluginVarType.val = varType;
            RegisterParam(pluginVarType, true, false);

            RegisterValueJSONParam();
        }

        private void RegisterValueJSONParam()
        {
            if (pluginVarType.val == PluginVariableType.vFloat)
            {
                pluginFloatValue = new JSONStorableFloat("pluginVarFloatValue", 0f, -1f, 1f,false);
                RegisterParam(pluginFloatValue);
            }
            else if (pluginVarType.val == PluginVariableType.vBool)
            {
                pluginBoolValue = new JSONStorableBool("pluginVarBoolValue", false);
                RegisterParam(pluginBoolValue);
            }
            else if (pluginVarType.val == PluginVariableType.vColor)
            {
                pluginColorValue = new JSONStorableColor("pluginVarColorValue", new HSVColor());
                RegisterParam(pluginColorValue);
            }
            else
            {
                pluginStringValue = new JSONStorableString("pluginVarStringValue", "");
                RegisterParam(pluginStringValue);
            }
        }

        public void RestoreFromJSON(JSONClass jc, int uiapFormatVersion)
        {
            ClearAllRegisteredParams();
            RegisterParam(pluginVarName);
            RegisterParam(pluginVarType);
            base.RestoreFromJSON(jc, "pluginVarType");
            RegisterValueJSONParam();
            base.RestoreFromJSON(jc);
            if (uiapFormatVersion == 1 && pluginVarType.val == PluginVariableType.vColor)
            {
                HSVColor hsvColor = new HSVColor();
                if (jc["pluginVarColorHValue"] != null) hsvColor.H = jc["pluginVarColorHValue"].AsFloat;
                if (jc["pluginVarColorSValue"] != null) hsvColor.S = jc["pluginVarColorSValue"].AsFloat;
                if (jc["pluginVarColorVValue"] != null) hsvColor.V = jc["pluginVarColorVValue"].AsFloat;
                pluginColorValue.val = hsvColor;
            }
        }
    }

    public class DecalMakerComponent : ButtonOperationComponentBase
    {
        protected JSONStorableEnumStringChooser buttonTypeJSEnum;

        public DecalMakerTextureCollection decalMakerTextureCollectionFace;
        public DecalMakerTextureCollection decalMakerTextureCollectionTorso;
        public DecalMakerTextureCollection decalMakerTextureCollectionLimbs;
        public DecalMakerTextureCollection decalMakerTextureCollectionGen;

        private int decalMakerSaveVersion = 1;

        public DecalMakerComponent(JSONStorableEnumStringChooser _buttonTypeJSEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeJSEnum;
            decalMakerTextureCollectionFace = new DecalMakerTextureCollection();
            decalMakerTextureCollectionTorso = new DecalMakerTextureCollection();
            decalMakerTextureCollectionLimbs = new DecalMakerTextureCollection();
            decalMakerTextureCollectionGen = new DecalMakerTextureCollection();
        }

        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);

            jc["decalMakerTextureCollectionFace"] = decalMakerTextureCollectionFace.GetJSON();
            jc["decalMakerTextureCollectionTorso"] = decalMakerTextureCollectionTorso.GetJSON();
            jc["decalMakerTextureCollectionLimbs"] = decalMakerTextureCollectionLimbs.GetJSON();
            jc["decalMakerTextureCollectionGen"] = decalMakerTextureCollectionGen.GetJSON();

            return jc;
        }

        public void LoadJSON(JSONClass jc, int uiapFormatVersion)
        {
            base.RestoreFromJSON(jc);
            if (jc["decalMakerTextureCollectionFace"] != null) decalMakerTextureCollectionFace.LoadJSON((JSONClass)jc["decalMakerTextureCollectionFace"], uiapFormatVersion);
            if (jc["decalMakerTextureCollectionTorso"] != null) decalMakerTextureCollectionTorso.LoadJSON((JSONClass)jc["decalMakerTextureCollectionTorso"], uiapFormatVersion);
            if (jc["decalMakerTextureCollectionLimbs"] != null) decalMakerTextureCollectionLimbs.LoadJSON((JSONClass)jc["decalMakerTextureCollectionLimbs"], uiapFormatVersion);
            if (jc["decalMakerTextureCollectionGen"] != null) decalMakerTextureCollectionGen.LoadJSON((JSONClass)jc["decalMakerTextureCollectionGen"], uiapFormatVersion);
        }

        public void CopyFrom(DecalMakerComponent sourceDMComponent)
        {
            base.CopyFrom(sourceDMComponent);

            decalMakerTextureCollectionFace = new DecalMakerTextureCollection();
            decalMakerTextureCollectionFace.CopyFrom(sourceDMComponent.decalMakerTextureCollectionFace);
            decalMakerTextureCollectionTorso = new DecalMakerTextureCollection();
            decalMakerTextureCollectionTorso.CopyFrom(sourceDMComponent.decalMakerTextureCollectionTorso);
            decalMakerTextureCollectionLimbs = new DecalMakerTextureCollection();
            decalMakerTextureCollectionLimbs.CopyFrom(sourceDMComponent.decalMakerTextureCollectionLimbs);
            decalMakerTextureCollectionGen = new DecalMakerTextureCollection();
            decalMakerTextureCollectionGen.CopyFrom(sourceDMComponent.decalMakerTextureCollectionGen);
        }


        public string GetDecalMakerPluginKey(Atom atom)
        {
            MVRPluginManager manager = atom.GetStorableByID("PluginManager") as MVRPluginManager;

            JSONClass currentPlugins = manager.GetJSON(true, true, false);
            string pluginKey = "";

            if (currentPlugins["plugins"] != null)
            {
                foreach (KeyValuePair<string, JSONNode> kvp in (JSONClass)currentPlugins["plugins"])
                {
                    string pluginPath = kvp.Value.ToString().TrimStart('"').TrimEnd('"');
                    if (pluginPath.StartsWith("Chokaphi.DecalMaker."))
                    {
                        JSONStorable pluginStorable = atom.GetStorableByID(kvp.Key + "_VAM_Decal_Maker.Decal_Maker");
                        if (pluginStorable != null && pluginStorable.GetActionNames().Contains("ClearAll"))
                        {
                            pluginKey = kvp.Key;
                            break;
                        }
                    }
                }
            }


            return (pluginKey);
        }

        public MVRScript GetDecalMakerScript(Atom atom)
        {
            return atom.GetStorableByID(GetDecalMakerPluginKey(atom) + "_VAM_Decal_Maker.Decal_Maker") as MVRScript;
        }
        private List<string> GetLoadedDecalTextures(JSONArray decalsJSONArray)
        {
            List<string> decalsList = new List<string>();

            foreach (JSONClass decalJSON in decalsJSONArray)
            {
                decalsList.Add(decalJSON["Path"].Value);
            }

            return (decalsList);
        }
        public int GetDecalMakerButtonState(Atom atom)
        {
            MVRScript dmStorable = GetDecalMakerScript(atom);
            if (dmStorable != null)
            {
                JSONClass decalMakerJSON;
                try
                {
                    // Causes an exception if DecalMaker is loading - so we just catch and ignore.
                    decalMakerJSON = dmStorable.GetJSON(true, true);
                }
                catch (Exception e) { decalMakerJSON = null; }
                if (decalMakerJSON != null)
                {
                    string decalNodeName = "Decal";
                    if (decalMakerJSON["SaveVersion"].AsInt > 1)
                    {
                        decalNodeName = "_DecalTex";
                        decalMakerSaveVersion = decalMakerJSON["SaveVersion"].AsInt;
                    }
                    List<string> loadedFaceDecalTextures = GetLoadedDecalTextures((JSONArray)decalMakerJSON[decalNodeName]["face"]);
                    List<string> loadedTorsoDecalTextures = GetLoadedDecalTextures((JSONArray)decalMakerJSON[decalNodeName]["torso"]);
                    List<string> loadedGenDecalTextures = GetLoadedDecalTextures((JSONArray)decalMakerJSON[decalNodeName]["genitals"]);
                    List<string> loadedLimbsDecalTextures = GetLoadedDecalTextures((JSONArray)decalMakerJSON[decalNodeName]["limbs"]);

                    foreach (DecalMakerTexture dmt in decalMakerTextureCollectionFace.decalMakerTexturesList)
                    {
                        if (dmt._decalTexturePath != "" && !loadedFaceDecalTextures.Contains(dmt._decalTexturePath)) return ButtonState.inactive;
                    }

                    foreach (DecalMakerTexture dmt in decalMakerTextureCollectionTorso.decalMakerTexturesList)
                    {
                        if (dmt._decalTexturePath != "" && !loadedTorsoDecalTextures.Contains(dmt._decalTexturePath)) return ButtonState.inactive;
                    }

                    foreach (DecalMakerTexture dmt in decalMakerTextureCollectionGen.decalMakerTexturesList)
                    {
                        if (dmt._decalTexturePath != "" && !loadedGenDecalTextures.Contains(dmt._decalTexturePath)) return ButtonState.inactive;
                    }

                    foreach (DecalMakerTexture dmt in decalMakerTextureCollectionLimbs.decalMakerTexturesList)
                    {
                        if (dmt._decalTexturePath != "" && !loadedLimbsDecalTextures.Contains(dmt._decalTexturePath)) return ButtonState.inactive;
                    }
                }
                else return ButtonState.inactive;
            }
            else return ButtonState.inactive;

            return ButtonState.active;
        }

        private static void DecalMakerToggleBodyRegion(Atom atom, string bodyRegion, DecalMakerTextureCollection dmtc, int state, string decalNodeName, JSONClass decalStorableJSON)
        {
            JSONStorableUrl bodyRegionDecalUrl = atom.GetStorableByID("textures").GetUrlJSONParam(bodyRegion+"DecalUrl");
            Dictionary<string, int> loadedBodyRegionDecalIndicies;

            foreach (DecalMakerTexture dmt in dmtc.decalMakerTexturesList)
            {
                loadedBodyRegionDecalIndicies = GetLoadedDecalArrayIndicies(decalStorableJSON[decalNodeName][bodyRegion].AsArray);

                if (dmt._decalTexturePath != "" && dmt._decalTexturePath != bodyRegionDecalUrl.val)
                {
                    if (state == 1) AddDMDecal((JSONArray) decalStorableJSON[decalNodeName][bodyRegion], dmt, loadedBodyRegionDecalIndicies);
                    else RemoveDMDecal((JSONArray)decalStorableJSON[decalNodeName][bodyRegion], dmt, loadedBodyRegionDecalIndicies);
                }

            }

            loadedBodyRegionDecalIndicies = GetLoadedDecalArrayIndicies(decalStorableJSON[decalNodeName][bodyRegion].AsArray);

            if (bodyRegionDecalUrl.val != "")
            {
                if (!loadedBodyRegionDecalIndicies.ContainsKey(bodyRegionDecalUrl.val))
                {
                    JSONClass newDecalSaveJSON = new JSONClass();
                    newDecalSaveJSON["H"].AsFloat = 0f;
                    newDecalSaveJSON["S"].AsFloat = 0f;
                    newDecalSaveJSON["V"].AsFloat = 1f;
                    newDecalSaveJSON["Alpha"].AsFloat = 1f;
                    newDecalSaveJSON["Path"] = bodyRegionDecalUrl.val;
                    JSONArray decalArray = (JSONArray)decalStorableJSON[decalNodeName][bodyRegion];
                    decalArray.Add(newDecalSaveJSON);
                }
            }
        }
        public void DecalMakerToggle(Atom atom)
        {
            string pluginKey = GetDecalMakerPluginKey(atom);
            JSONStorable decalPluginStorable = null;
            JSONClass decalStorableJSON = null;
            if (pluginKey == "") decalStorableJSON = GetDecalMakerBlankStorable();
            else
            {
                decalPluginStorable = atom.GetStorableByID(pluginKey + "_VAM_Decal_Maker.Decal_Maker");
                decalStorableJSON = decalPluginStorable.GetJSON(true, true);
            }

            string decalNodeName = "Decal";
            if (decalMakerSaveVersion > 1)   decalNodeName = "_DecalTex";

            if (decalStorableJSON != null)
            {
                int state = parentButton.GetButtonState();

                DecalMakerToggleBodyRegion(atom, "face", decalMakerTextureCollectionFace, state, decalNodeName, decalStorableJSON);

                DecalMakerToggleBodyRegion(atom, "torso", decalMakerTextureCollectionTorso, state, decalNodeName, decalStorableJSON);

                DecalMakerToggleBodyRegion(atom, "limbs", decalMakerTextureCollectionLimbs, state, decalNodeName, decalStorableJSON);

                DecalMakerToggleBodyRegion(atom, "genitals", decalMakerTextureCollectionGen, state, decalNodeName, decalStorableJSON);

                if (pluginKey == "") MergeLoadDecalMakerPlugin(atom, decalStorableJSON);
                else
                {
                    if (decalPluginStorable.GetActionNames().Contains("PerformLoad"))
                    {

                        JSONStorableAction clearAllAction = decalPluginStorable.GetAction("ClearAll");
                        JSONStorableAction performLoadAction = decalPluginStorable.GetAction("PerformLoad");
                        if (clearAllAction != null)
                        {
                            decalPluginStorable.CallAction("ClearAll");
                            decalPluginStorable.RestoreFromJSON(decalStorableJSON);
                            decalPluginStorable.CallAction("PerformLoad"); 
                        }
                    }
                    else UIAGlobals.mvrScript.StartCoroutine(DMToggleByCharSkinChange(atom, decalPluginStorable, decalStorableJSON, false));
                }
            }
        }

        private IEnumerator DMLateLoad(JSONStorable decalPluginStorable, JSONClass decalJSON)
        {
            yield return new WaitForSeconds(1f);
            if (decalPluginStorable.GetActionNames().Contains("PerformLoad"))
            {
                JSONStorableAction clearAllAction = decalPluginStorable.GetAction("ClearAll");
                JSONStorableAction performLoadAction = decalPluginStorable.GetAction("PerformLoad");
                if (clearAllAction != null)
                {
                    decalPluginStorable.CallAction("ClearAll");
                    decalPluginStorable.RestoreFromJSON(decalJSON);
                    decalPluginStorable.CallAction("PerformLoad");
                }
            }
        }
        private IEnumerator DMToggleByCharSkinChange(Atom atom, JSONStorable decalPluginStorable, JSONClass decalJSON, bool waitForDMLoad)
        {
            DAZCharacterSelector dazCharacterSelector = atom.GetComponentInChildren<DAZCharacterSelector>();
            DAZCharacter dazCharacter = dazCharacterSelector.selectedCharacter;
            JSONStorable receiver = atom.GetStorableByID("geometry");
            JSONStorableStringChooser characterJSON = receiver.GetStringChooserJSONParam("characterSelection");

            string currentCharacterSkinName = characterJSON.val;
            string tempCharacterSkinName = "";

            switch (dazCharacter.UVname)
            {
                case "UV: Base Female":
                    if (currentCharacterSkinName == "Female 1") tempCharacterSkinName = "Female 7";
                    else tempCharacterSkinName = "Female 1";
                    break;
                case "UV: Victoria 6":
                    if (currentCharacterSkinName == "Female 2") tempCharacterSkinName = "Female 4";
                    else tempCharacterSkinName = "Female 2";
                    break;
                case "UV: Base Male":
                    if (currentCharacterSkinName == "Male 4") tempCharacterSkinName = "Male 5";
                    else tempCharacterSkinName = "Male 4";
                    break;
                case "UV: Michael 6":
                    tempCharacterSkinName = "Male 4";
                    break;
                case "UV: Gianni 6":
                    tempCharacterSkinName = "Male 4";
                    break;
                case "UV: Darius 6":
                    tempCharacterSkinName = "Male 4";
                    break;
                case "UV: Lee 6":
                    tempCharacterSkinName = "Male 4";
                    break;
                case "UV: Scott 6":
                    tempCharacterSkinName = "Male 4";
                    break;
                default:
                    tempCharacterSkinName = "Female 1";
                    break;
            }

            if (waitForDMLoad) yield return new WaitForSeconds(3f);
            JSONStorableAction clearAllAction = decalPluginStorable.GetAction("ClearAll");
            if (clearAllAction != null)
            {
                decalPluginStorable.CallAction("ClearAll");
                yield return new WaitForSeconds(0.5f);
                yield return new WaitUntil(() => dazCharacter.ready);
                decalPluginStorable.RestoreFromJSON(decalJSON);
                yield return new WaitForSeconds(0.5f);
                yield return new WaitUntil(() => dazCharacter.ready);
                characterJSON.val = tempCharacterSkinName;
                yield return new WaitForSeconds(0.5f);
                yield return new WaitUntil(() => dazCharacter.ready);
                characterJSON.val = currentCharacterSkinName;
            }
        }
        private static Dictionary<string, int> GetLoadedDecalArrayIndicies(JSONArray decalsJSONArray)
        {
            Dictionary<string, int> decalsDict = new Dictionary<string, int>();
            int index = 0;
            foreach (JSONClass decalJSON in decalsJSONArray)
            {
                string path = decalJSON["Path"].Value;
                if (!decalsDict.ContainsKey(path)) decalsDict.Add(path, index);
                index++;
            }

            return (decalsDict);
        }


        private static void AddDMDecal(JSONArray decalArray, DecalMakerTexture dmt, Dictionary<string, int> loadedDecalIndicies)
        {
            if (loadedDecalIndicies.ContainsKey(dmt._decalTexturePath))
            {
                JSONClass existingDecalSaveJSON = (JSONClass)decalArray[loadedDecalIndicies[dmt._decalTexturePath]];
                existingDecalSaveJSON["H"].AsFloat = dmt._decalTextureColor.H;
                existingDecalSaveJSON["S"].AsFloat = dmt._decalTextureColor.S;
                existingDecalSaveJSON["V"].AsFloat = dmt._decalTextureColor.V;
                existingDecalSaveJSON["Alpha"].AsFloat = dmt._decalTextureAlpha;
                existingDecalSaveJSON["Path"] = dmt._decalTexturePath;
            }
            else
            {

                JSONClass newDecalSaveJSON = new JSONClass();
                newDecalSaveJSON["H"].AsFloat = dmt._decalTextureColor.H;
                newDecalSaveJSON["S"].AsFloat = dmt._decalTextureColor.S;
                newDecalSaveJSON["V"].AsFloat = dmt._decalTextureColor.V;
                newDecalSaveJSON["Alpha"].AsFloat = dmt._decalTextureAlpha;
                newDecalSaveJSON["Path"] = dmt._decalTexturePath;
                decalArray.Add(newDecalSaveJSON);
            }
        }
        private static void RemoveDMDecal(JSONArray decalArray, DecalMakerTexture dmt, Dictionary<string, int> loadedDecalIndicies)
        {
            if (loadedDecalIndicies.ContainsKey(dmt._decalTexturePath))
            {
                decalArray.Remove(loadedDecalIndicies[dmt._decalTexturePath]);
            }
        }

        private JSONClass GetDecalMakerBlankStorable()
        {
            JSONClass decalJSON = new JSONClass();

            string decalNodeName = "Decal";
            if (decalMakerSaveVersion > 1)
            {
                decalNodeName = "_DecalTex";
                decalJSON["SaveVersion"].AsInt = 2;
            }
            else decalJSON["SaveVersion"].AsInt = 1;

            decalJSON["Nipple Cutouts ON"] = "true";
            decalJSON["Genital Cutouts ON"] = "true";
            decalJSON["enabled"] = "true";        

            decalJSON[decalNodeName] = new JSONClass();
            decalJSON[decalNodeName]["face"] = new JSONArray();
            decalJSON[decalNodeName]["torso"] = new JSONArray();
            decalJSON[decalNodeName]["genitals"] = new JSONArray();
            decalJSON[decalNodeName]["limbs"] = new JSONArray();
            decalJSON["_SpecTex"] = new JSONClass();
            decalJSON["_SpecTex"]["face"] = new JSONArray();
            decalJSON["_SpecTex"]["torso"] = new JSONArray();
            decalJSON["_SpecTex"]["genitals"] = new JSONArray();
            decalJSON["_SpecTex"]["limbs"] = new JSONArray();

            decalJSON["_GlossTex"] = new JSONClass();
            decalJSON["_GlossTex"]["face"] = new JSONArray();
            decalJSON["_GlossTex"]["torso"] = new JSONArray();
            decalJSON["_GlossTex"]["genitals"] = new JSONArray();
            decalJSON["_GlossTex"]["limbs"] = new JSONArray();

            decalJSON["_BumpMap"] = new JSONClass();
            decalJSON["_BumpMap"]["face"] = new JSONArray();
            decalJSON["_BumpMap"]["torso"] = new JSONArray();
            decalJSON["_BumpMap"]["genitals"] = new JSONArray();
            decalJSON["_BumpMap"]["limbs"] = new JSONArray();

            return (decalJSON);
        }

        private void MergeLoadDecalMakerPlugin(Atom atom, JSONClass decalMakerJSON)
        {
            JSONClass pluginPresetJSON = new JSONClass();
            pluginPresetJSON["setUnlistedParamsToDefault"].AsBool = true;

            JSONArray pluginPresetStorablesJSON = new JSONArray();
            pluginPresetJSON["storables"] = pluginPresetStorablesJSON;

            JSONClass pluginMgrStorableJSON = new JSONClass();
            pluginPresetStorablesJSON.Add(pluginMgrStorableJSON);
            pluginMgrStorableJSON["id"] = "PluginManager";

            JSONClass pluginMgrPluginsListJSON = new JSONClass();
            pluginMgrStorableJSON["plugins"] = pluginMgrPluginsListJSON;

            pluginMgrPluginsListJSON["plugin#1"] = "Chokaphi.DecalMaker.latest:/Custom/Scripts/Chokaphi/VAM_Decal_Maker/VAM_Decal_Maker.cs";

            if (decalMakerJSON == null) decalMakerJSON = GetDecalMakerBlankStorable();
            decalMakerJSON["id"] = "plugin#1_VAM_Decal_Maker.Decal_Maker";
            //            pluginPresetStorablesJSON.Add(decalMakerJSON);

            PluginsLoadComponent.MergePluginPreset(atom, pluginPresetJSON);
            string pluginKey = GetDecalMakerPluginKey(atom);
            if (pluginKey != "")
            {
                JSONStorable decalPluginStorable = atom.GetStorableByID(pluginKey + "_VAM_Decal_Maker.Decal_Maker");
                UIAGlobals.mvrScript.StartCoroutine(DMLateLoad(decalPluginStorable, decalMakerJSON));
            }
        }

    }

    public class DecalMakerTextureCollection : JSONStorableObject
    {
        public List<DecalMakerTexture> decalMakerTexturesList;

        public DecalMakerTextureCollection()
        {
            decalMakerTexturesList = new List<DecalMakerTexture>();
        }
        public void CopyFrom(DecalMakerTextureCollection dmtc)
        {
            if (dmtc != null)
            {
                decalMakerTexturesList = new List<DecalMakerTexture>();
                foreach (DecalMakerTexture dmt in dmtc.decalMakerTexturesList)
                {
                    DecalMakerTexture newDMT = new DecalMakerTexture();
                    newDMT.CopyFrom(dmt);
                    decalMakerTexturesList.Add(newDMT);
                }
            }
        }

        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);
            JSONArray decalMakerTexturesArray = new JSONArray();

            foreach (DecalMakerTexture dmt in decalMakerTexturesList)
            {
                decalMakerTexturesArray.Add(dmt.GetJSON());
            }

            jc["decalTexturesCollection"] = decalMakerTexturesArray;

            return jc;
        }
        public void LoadJSON(JSONClass dmtcJSON, int uiapVersion)
        {
            decalMakerTexturesList = new List<DecalMakerTexture>();

            if (dmtcJSON["decalTexturesCollection"] != null)
            {
                JSONArray decalTexturesArray = dmtcJSON["decalTexturesCollection"].AsArray;
                for (int i = 0; i < decalTexturesArray.Count; i++)
                {
                    JSONClass dmtNode = decalTexturesArray[i].AsObject;
                    if (uiapVersion == 1)
                    {
                        HSVColor hsvColor = new HSVColor();
                        hsvColor.H = dmtNode["decalTextureColorHValue"].AsFloat;
                        hsvColor.S = dmtNode["decalTextureColorSValue"].AsFloat;
                        hsvColor.V = dmtNode["decalTextureColorVValue"].AsFloat;
                        JSONStorableColor tempColJSC = new JSONStorableColor("decalTextureColor", hsvColor);
                        tempColJSC.StoreJSON(dmtNode, true, true, true);
                    }
                    DecalMakerTexture newDMT = new DecalMakerTexture();
                    newDMT.RestoreFromJSON(dmtNode);
                    decalMakerTexturesList.Add(newDMT);
                }
            }
        }
        public List<string> GetDecalSelectChoices(string currentSelection)
        {
            List<string> choices = new List<string>();
            int index = 1;
            foreach (DecalMakerTexture dm in decalMakerTexturesList)
            {
                choices.Add(index.ToString());
                index++;
            }
            choices.Add("Add Decal");
            if (currentSelection != "Select Texture...") choices.Add("Remove Decal " + currentSelection);
            return (choices);
        }
        public void AddDecal()
        {
            DecalMakerTexture newDMT = new DecalMakerTexture();
            decalMakerTexturesList.Add(newDMT);
        }
        public void RemoveDecal(int index)
        {
            decalMakerTexturesList.RemoveAt(index);
        }
    }
    public class DecalMakerTexture : JSONStorableObject
    {
        public string _decalTexturePath
        {
            get { return decalTexturePathJSS.val; }
            set { decalTexturePathJSS.val = value; }
        }
        public HSVColor _decalTextureColor
        {
            get { return decalTextureColorJSC.val; }
            set { decalTextureColorJSC.val = value; }
        }
        public float _decalTextureAlpha
        {
            get { return decalTextureAlphaJSF.val; }
            set { decalTextureAlphaJSF.val = value; }
        }

        public JSONStorableString decalTexturePathJSS;
        public JSONStorableColor decalTextureColorJSC;
        public JSONStorableFloat decalTextureAlphaJSF;
        public DecalMakerTexture()
        {
            HSVColor defaultColor = new HSVColor();
            defaultColor.H = 0f;
            defaultColor.S = 0f;
            defaultColor.V = 1f;

            decalTextureColorJSC = new JSONStorableColor("decalTextureColor", defaultColor);
            decalTexturePathJSS = new JSONStorableString("decalTexturePath", "");
            decalTextureAlphaJSF = new JSONStorableFloat("decalTextureAlpha", 0f, -1f, 1f);
            RegisterParam(decalTextureColorJSC);
            RegisterParam(decalTexturePathJSS);
            RegisterParam(decalTextureAlphaJSF);

        }

    }


    public class TargetButtonComponent : ButtonOperationComponentBase
    {
        public JSONStorableEnumStringChooser targetCategoryJSEnum;
        public JSONStorableMultiEnumStringChooser targetNameJSMultiEnum;
        public JSONStorableBool spawnAtomIfTargetMissingJSBool;
        public JSONStorableString specificAtomTypeJSS;
        public JSONStorableString changeAtomNameJSS;
        public JSONStorableBool keepOpenAtomSelectorJSB;
        public JSONStorableBool keepOpenOptionAtomSelectorJSB;

        public JSONStorableEnumStringChooser aceModeJSEnum;

        public string lastUserChosenAtomName { get; protected set; }

        public List<string> currentActionTargetAtomNames;

        protected JSONStorableEnumStringChooser buttonTypeJSEnum;
        protected int buttonCategory
        {
            get
            {
                return UIAButtonOpType.GetButtonCategory(buttonTypeJSEnum.val);
            }
        }

        public bool isPersonTargetType
        {
            get
            {
                if (targetCategoryJSEnum.val != TargetCategory.specificAtom && targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.mainValFlag && CustomTargetGroupSettings.GetCGTFromName(targetNameJSMultiEnum.mainVal).isPersonCGT ) return true;

                if (targetCategoryJSEnum.val == TargetCategory.atomGroup)
                {
                    if (targetNameJSMultiEnum.valType== JSONStorableMultiEnumStringChooser.topEnumValFlag)
                    {
                        if (targetNameJSMultiEnum.valTopEnum == AllAtomsTargetType.allFemaleAtoms || targetNameJSMultiEnum.valTopEnum == AllAtomsTargetType.allMaleAtoms || targetNameJSMultiEnum.valTopEnum == AllAtomsTargetType.allPersonAtoms) return true;
                    }
                }

                
                if (targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom)
                {
                    if (targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.topEnumValFlag)
                    {
                        if (targetNameJSMultiEnum.valTopEnum == LastViewedTargetType.lastViewedFemale || targetNameJSMultiEnum.valTopEnum == LastViewedTargetType.lastViewedMale || targetNameJSMultiEnum.valTopEnum == LastViewedTargetType.lastViewedPerson) return true;
                    }                    
                }


                if (targetCategoryJSEnum.val == TargetCategory.vamSelectedAtom)
                {
                    if (targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.topEnumValFlag)
                    {
                        if (targetNameJSMultiEnum.valTopEnum == LastSelectedTargetType.lastSelectedFemale || targetNameJSMultiEnum.valTopEnum == LastSelectedTargetType.lastSelectedMale || targetNameJSMultiEnum.valTopEnum == LastSelectedTargetType.lastSelectedPerson) return true;
                    }
                }
                if (targetCategoryJSEnum.val == TargetCategory.userChosenAtom)
                {
                    if (targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.topEnumValFlag)
                    {
                        if (targetNameJSMultiEnum.valTopEnum == UserChosenTargetType.femaleAtoms || targetNameJSMultiEnum.valTopEnum == UserChosenTargetType.maleAtoms || targetNameJSMultiEnum.valTopEnum == UserChosenTargetType.personAtoms) return true;
                    }
                }
                if (targetCategoryJSEnum.val == TargetCategory.specificAtom && specificAtomTypeJSS.val == "Person") return true;
                return false;
            }
        }

        public TargetButtonComponent(JSONStorableEnumStringChooser _buttonTypeEnum, UIAButtonOperation parent) : base(parent)
        {
            buttonTypeJSEnum = _buttonTypeEnum;

            targetCategoryJSEnum = new JSONStorableEnumStringChooser("targetCategory", TargetCategory.enumManifestName, TargetCategory.gazeSelectedAtom, "", TargetCategoryUpdated, GetTargetCatExclusions());
            RegisterParam(targetCategoryJSEnum);

            targetNameJSMultiEnum = new JSONStorableMultiEnumStringChooser("targetName", null, null, null, null, TargetNameMainEnumChanged, LastViewedTargetType.enumManifestName, TargetNameTopEnumChanged);
            RegisterParam(targetNameJSMultiEnum);

            specificAtomTypeJSS = new JSONStorableString("specificAtomType", "");
            RegisterParam(specificAtomTypeJSS);

            spawnAtomIfTargetMissingJSBool = new JSONStorableBool("spawnAtomIfTargetMissing", false,SpawnAtomCallback);
            RegisterParam(spawnAtomIfTargetMissingJSBool);

            changeAtomNameJSS = new JSONStorableString("changeAtomNewName","");
            RegisterParam(changeAtomNameJSS);

            keepOpenAtomSelectorJSB = new JSONStorableBool("keepOpen", false);
            keepOpenOptionAtomSelectorJSB = new JSONStorableBool("keepOpenOption", true);
            RegisterParam(keepOpenAtomSelectorJSB);
            RegisterParam(keepOpenOptionAtomSelectorJSB);

            aceModeJSEnum = new JSONStorableEnumStringChooser("aceMode", ACEMode.enumManifestName, ACEMode.enhanced, "Acitve Clothing Editor Mode");
            RegisterParam(aceModeJSEnum);

            currentActionTargetAtomNames = new List<string>();
        }

        private void SpawnAtomCallback(bool value)
        {
            parentButtonOperation.CreateButtonComponents();
        }
        public void ChangeTargetAtomNameAction(Atom atom)
        {
            if (changeAtomNameJSS.val != "" && atom.uid!= changeAtomNameJSS.val)
            {
                atom.SetUID(changeAtomNameJSS.val);
            }
        }

        private void TargetNameTopEnumChanged(int targetNameTopEnum)
        {
            if (targetCategoryJSEnum.val == TargetCategory.userChosenAtom)
            {
                if (!GetUserTargetAtomChoices().Contains(lastUserChosenAtomName)) lastUserChosenAtomName = "";
            }
            parentButton.parentGrid.RecalcGazeSelections();
        }

        private void TargetNameMainEnumChanged(string targetName)
        {
            if (targetCategoryJSEnum.val == TargetCategory.userChosenAtom)
            {
                if (!GetUserTargetAtomChoices().Contains(lastUserChosenAtomName)) lastUserChosenAtomName = "";
            }
            if (targetCategoryJSEnum.val == TargetCategory.specificAtom)
            {
                Atom atom = SuperController.singleton.GetAtomByUid(targetNameJSMultiEnum.displayVal);
                if (atom != null) specificAtomTypeJSS.val = atom.type;
                else specificAtomTypeJSS.val = "";
            }
            parentButton.parentGrid.RecalcGazeSelections();
        }

        public void RefreshTargetNameChoices(bool resetVal = true)
        {
            if (targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom)
            {
                targetNameJSMultiEnum.SetTopEnum(LastViewedTargetType.enumManifestName, GetTargetNameExclusions());
                
                targetNameJSMultiEnum.SetMainChoices(GetCGTNames());
                targetNameJSMultiEnum.SetDisplayChoicePreFix("Last viewed ");
                targetNameJSMultiEnum.SetDisplayChoicePostFix("");
                targetNameJSMultiEnum.SetBottomEnum(null);
                if (targetNameJSMultiEnum.choices.Count==0) targetNameJSMultiEnum.SetBottomEnum(NoneAvailable.enumManifestName);
            }
            if (targetCategoryJSEnum.val == TargetCategory.vamSelectedAtom)
            {
                targetNameJSMultiEnum.SetTopEnum(LastSelectedTargetType.enumManifestName, GetTargetNameExclusions());
                
                targetNameJSMultiEnum.SetMainChoices(null);
                targetNameJSMultiEnum.SetDisplayChoicePreFix("Last selected ");
                targetNameJSMultiEnum.SetDisplayChoicePostFix("");
                targetNameJSMultiEnum.SetBottomEnum(null);
                if (targetNameJSMultiEnum.choices.Count == 0) targetNameJSMultiEnum.SetBottomEnum(NoneAvailable.enumManifestName);
            }
            if (targetCategoryJSEnum.val == TargetCategory.atomGroup)
            {
                targetNameJSMultiEnum.SetTopEnum(AllAtomsTargetType.enumManifestName, GetTargetNameExclusions());
                targetNameJSMultiEnum.SetMainChoices(GetCGTNames());
                targetNameJSMultiEnum.SetDisplayChoicePreFix("All ");
                targetNameJSMultiEnum.SetDisplayChoicePostFix(" Atoms");
                targetNameJSMultiEnum.SetBottomEnum(null);
                if (targetNameJSMultiEnum.choices.Count == 0) targetNameJSMultiEnum.SetBottomEnum(NoneAvailable.enumManifestName);
            }
            if (targetCategoryJSEnum.val == TargetCategory.specificAtom)
            {              
                List<string> targetNames = GetSpecificAtomTargetChoices();
                
                if (!targetNames.Contains(targetNameJSMultiEnum.displayVal) && !resetVal && targetNameJSMultiEnum.valType!=JSONStorableMultiEnumStringChooser.topEnumValFlag) targetNames.Add(targetNameJSMultiEnum.displayVal);
                if (targetNames.Count == 0) {

                    targetNameJSMultiEnum.SetTopEnum(NoneAvailable.enumManifestName);
                    targetNameJSMultiEnum.SetMainChoices(null);
                    targetNameJSMultiEnum.valTopEnum = NoneAvailable.noneAvailable;
                }
                else
                {
                    targetNameJSMultiEnum.SetTopEnum(null);
                    targetNameJSMultiEnum.SetMainChoices(targetNames);
                }

                targetNameJSMultiEnum.SetDisplayChoicePreFix("");
                targetNameJSMultiEnum.SetDisplayChoicePostFix("");
                targetNameJSMultiEnum.SetBottomEnum(null);
                if (targetNameJSMultiEnum.choices.Count == 0) targetNameJSMultiEnum.SetBottomEnum(NoneAvailable.enumManifestName);
            }
            if (targetCategoryJSEnum.val == TargetCategory.userChosenAtom)
            {
                targetNameJSMultiEnum.SetTopEnum(UserChosenTargetType.enumManifestName, GetTargetNameExclusions());
                targetNameJSMultiEnum.SetMainChoices(GetCGTNames());
                targetNameJSMultiEnum.SetDisplayChoicePreFix("");
                targetNameJSMultiEnum.SetDisplayChoicePostFix(" Atoms");
                targetNameJSMultiEnum.SetBottomEnum(null);
                if (targetNameJSMultiEnum.choices.Count == 0) targetNameJSMultiEnum.SetBottomEnum(NoneAvailable.enumManifestName);
            }

            if (resetVal)
            {
                if (targetCategoryJSEnum.val== TargetCategory.gazeSelectedAtom && targetNameJSMultiEnum.displayChoices.Contains(GazeTargetSettings.defaultGazeTargetJSEnum.displayVal))
                {
                    
                    targetNameJSMultiEnum.valTopEnum = GazeTargetSettings.defaultGazeTargetJSEnum.val;
                }
                else targetNameJSMultiEnum.ResetValFromChoices();
            }

        }

        protected void TargetCategoryUpdated(int targetCat)
        {
            parentButtonOperation.RefreshFileRefTypes();
            RefreshTargetNameChoices();
        }
        public List<string> GetTargetCategoryChoices()
        {
            return targetCategoryJSEnum.displayChoices;
        }

        public void ResetLastUserChosenAtom()
        {
            lastUserChosenAtomName = "";
        }

        protected List<string> GetSpecificAtomTargetChoices()
        {
            List<string> targetAtoms = new List<string>();
            foreach (string atomUID in SuperController.singleton.GetAtomUIDs())
            {
                Atom atom = SuperController.singleton.GetAtomByUid(atomUID);
                if (atomUID != "[CameraRig]" && (atomUID != "CoreControl" || buttonTypeJSEnum.val == UIAButtonOpType.loadPlugins) && (atom.type == "Person" || UIAButtonOpType.IsNonPersonAtomTargetableType(buttonTypeJSEnum.val)))
                {
                    if (buttonTypeJSEnum.val != UIAButtonOpType.loadSubScene || atom.type == "SubScene") targetAtoms.Add(atomUID);
                }
            }
            return (targetAtoms);
        }

        protected List<int> GetTargetCatExclusions()
        {
            List<int> targetCatExclusions = new List<int>();
            

            if (buttonTypeJSEnum.val == UIAButtonOpType.moveAtom || buttonTypeJSEnum.val == UIAButtonOpType.teleportAtom || buttonTypeJSEnum.val == UIAButtonOpType.changeAtomName || buttonTypeJSEnum.val == UIAButtonOpType.selectAtom || buttonTypeJSEnum.val == UIAButtonOpType.activeClothingEditor) targetCatExclusions.Add(TargetCategory.atomGroup);
            if (buttonCategory != UIAButtonCategory.pluginsLoading && buttonCategory != UIAButtonCategory.pluginSettings && buttonTypeJSEnum.val != UIAButtonOpType.loadPluginsPreset)
            {
                targetCatExclusions.Add(TargetCategory.scenePlugins);
                targetCatExclusions.Add(TargetCategory.sessionPlugins);
            }

            return (targetCatExclusions);
        }

        protected List<int> GetTargetNameExclusions()
        {
            List<int> targetNameExclusions = new List<int>();

            if (buttonTypeJSEnum.val == UIAButtonOpType.loadSubScene)
            {
                if (targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom)
                {
                    targetNameExclusions.Add(LastViewedTargetType.lastViewedFemale);
                    targetNameExclusions.Add(LastViewedTargetType.lastViewedMale);
                    targetNameExclusions.Add(LastViewedTargetType.lastViewedPerson);
                }
                else if (targetCategoryJSEnum.val == TargetCategory.vamSelectedAtom)
                {
                    targetNameExclusions.Add(LastSelectedTargetType.lastSelectedFemale);
                    targetNameExclusions.Add(LastSelectedTargetType.lastSelectedMale);
                    targetNameExclusions.Add(LastSelectedTargetType.lastSelectedPerson);
                }
                else if (targetCategoryJSEnum.val == TargetCategory.atomGroup)
                {
                    targetNameExclusions.Add(AllAtomsTargetType.allPersonAtoms);
                    targetNameExclusions.Add(AllAtomsTargetType.allFemaleAtoms);
                    targetNameExclusions.Add(AllAtomsTargetType.allMaleAtoms);
                }
                else if (targetCategoryJSEnum.val == TargetCategory.userChosenAtom)
                {
                    targetNameExclusions.Add(UserChosenTargetType.femaleAtoms);
                    targetNameExclusions.Add(UserChosenTargetType.maleAtoms);
                    targetNameExclusions.Add(UserChosenTargetType.personAtoms);
                }

            }
            else if ((buttonCategory != UIAButtonCategory.pluginsLoading && buttonCategory != UIAButtonCategory.pluginSettings && buttonCategory != UIAButtonCategory.atomControl && buttonTypeJSEnum.val != UIAButtonOpType.loadGeneralPreset && UIAButtonOpType.GetButtonCategory(buttonTypeJSEnum.val)!=UIAButtonCategory.nodeControl) || buttonTypeJSEnum.val == UIAButtonOpType.togglePresetLocks)
            {
                targetNameExclusions.Add(LastViewedTargetType.lastViewedAtom);
                targetNameExclusions.Add(LastSelectedTargetType.lastSelectedAtom);
                targetNameExclusions.Add(AllAtomsTargetType.allAtoms);
                targetNameExclusions.Add(UserChosenTargetType.anyAtoms);
                targetNameExclusions.Add(LastViewedTargetType.lastViewedNonPerson);
                targetNameExclusions.Add(LastSelectedTargetType.lastSelectedNonPerson);
                targetNameExclusions.Add(AllAtomsTargetType.allNonPersonAtoms);
                targetNameExclusions.Add(UserChosenTargetType.nonPersonAtoms);
            }
            else if (UIAButtonOpType.GetButtonCategory(buttonTypeJSEnum.val) == UIAButtonCategory.nodeControl && buttonTypeJSEnum.val != UIAButtonOpType.detachAtomRoot)
            {
                targetNameExclusions.Add(LastViewedTargetType.lastViewedAtom);
                targetNameExclusions.Add(LastSelectedTargetType.lastSelectedAtom);
                targetNameExclusions.Add(AllAtomsTargetType.allAtoms);
                targetNameExclusions.Add(UserChosenTargetType.anyAtoms);
            }
            return (targetNameExclusions);
        }

        protected List<string> GetCGTNames()
        {
            List<string> cgtNames = new List<string>();
            foreach (CustomTargetGroupType cgt in CustomTargetGroupSettings.customTargetGroupTypeList)
            {
                if (buttonTypeJSEnum.val == UIAButtonOpType.loadSubScene && cgt.cgtAtomTypeJSEnum.val != AtomTypes.subScene) continue;
                if (UIAButtonOpType.IsOnlyPersonAtomTargetable(buttonTypeJSEnum.val) && cgt.cgtAtomCategoryJSEnum.val != AtomCategories.people) continue;


                cgtNames.Add(cgt.cgtNameJSS.val);
            }

            return cgtNames;
        }


        public void ButtonTypeUpdated()
        {
            targetCategoryJSEnum.SetEnumChoices(TargetCategory.enumManifestName, GetTargetCatExclusions(), true, true);
            RefreshTargetNameChoices();
        }
        public void AtomNameUpdate(string oldName, string newName)
        {
            if (lastUserChosenAtomName == oldName) lastUserChosenAtomName = newName;
        }

        public void AtomRemovedUpdate(string oldName)
        {
            if (lastUserChosenAtomName == oldName) lastUserChosenAtomName = "";
        }

        public void UpdateCGTName(string oldName, string newName)
        {
            if ((targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom || targetCategoryJSEnum.val == TargetCategory.userChosenAtom || targetCategoryJSEnum.val == TargetCategory.atomGroup) && targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.mainValFlag && targetNameJSMultiEnum.mainVal == oldName)
            {
                targetNameJSMultiEnum.mainVal = newName;
            }
        }

        public void RemoveCGTName(string cgtName)
        {
            if ((targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom || targetCategoryJSEnum.val == TargetCategory.userChosenAtom || targetCategoryJSEnum.val == TargetCategory.atomGroup) && targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.mainValFlag && targetNameJSMultiEnum.mainVal == cgtName)
            {
                RefreshTargetNameChoices();
            }
        }

        public List<string> GetUserTargetAtomChoices()
        {
            List<string> choices = new List<string>();
            if (targetCategoryJSEnum.val == TargetCategory.userChosenAtom)
            {
                foreach (Atom atom in AtomUtils.GetAtoms())
                {
                    bool isMale = AtomUtils.IsMale(atom);
                    bool isFemale = AtomUtils.IsFemale(atom);

                    bool atomAvailable = false;
                    if (UIAButtonOpType.GetButtonCategory(buttonTypeJSEnum.val) == UIAButtonCategory.pluginSettings)
                    {
                        foreach (MVRScript plugin in PluginUtils.GetPluginsFromAtom(atom))
                        {
                            if (plugin.name.EndsWith(parentButtonOperation.pluginSettingComponent.pluginTypeJSSC.val))
                            {
                                atomAvailable = true;
                                break;
                            }
                        }
                    }
                    else atomAvailable = true;
                    if (atomAvailable)
                    {
                        if (targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.topEnumValFlag)
                        {
                            switch (targetNameJSMultiEnum.valTopEnum)
                            {
                                case UserChosenTargetType.personAtoms:
                                    if (isMale || isFemale) choices.Add(atom.name);
                                    break;
                                case UserChosenTargetType.femaleAtoms:
                                    if (isFemale) choices.Add(atom.name);
                                    break;
                                case UserChosenTargetType.maleAtoms:
                                    if (isMale) choices.Add(atom.name);
                                    break;
                                case UserChosenTargetType.anyAtoms:
                                    choices.Add(atom.name);
                                    break;
                                case UserChosenTargetType.nonPersonAtoms:
                                    if (!isMale && !isFemale) choices.Add(atom.name);
                                    break;
                            }
                        }
                        else {
                            CustomTargetGroupType cgt = CustomTargetGroupSettings.GetCGTFromName(targetNameJSMultiEnum.mainVal);
                            if (cgt.IsInScope(atom, isMale, isFemale)) choices.Add(atom.name);
                        }
                    }
                }
            }
            return (choices);
        }

        public void SetCurrentActionTargetAtoms()
        {
            currentActionTargetAtomNames = GetTargetAtomNames();

            if (currentActionTargetAtomNames.Count == 1 && targetCategoryJSEnum.val == TargetCategory.userChosenAtom) lastUserChosenAtomName = currentActionTargetAtomNames[0];

            else if (currentActionTargetAtomNames.Count == 0 && targetCategoryJSEnum.val == TargetCategory.userChosenAtom)
            {
                lastUserChosenAtomName = "";
                if (buttonTypeJSEnum.val != UIAButtonOpType.activeClothingEditor)
                {
                    GameControlUI.gameControlDisplayMode = GameControlDisplayModes.atomSelect;
                    GridsDisplay._uiTargetAtomSelector.SetTargetChoices(GetUserTargetAtomChoices(), TASSelected, keepOpenOptionAtomSelectorJSB.val);
                    GridsDisplay._uiTargetAtomSelector.keepOpenJSB.val = keepOpenAtomSelectorJSB.val;
                    GameControlUI.RefreshWristUIButtonGrid();
                }
                else
                {
                    var personAtoms = AtomUtils.GetPersonAtoms();
                    if (personAtoms.Count > 0) currentActionTargetAtomNames.Add(personAtoms[0].name);
                }
                
            }
            else lastUserChosenAtomName = "";
        }

        public List<string> GetTargetAtomNames()
        {
            List<string> targetAtomNames = new List<string>();                   
            if (parentButtonOperation.IsComponentInButtonOpType(ButtonComponentTypes.targetComponent))
            {
                if (targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom || targetCategoryJSEnum.val == TargetCategory.vamSelectedAtom || targetCategoryJSEnum.val == TargetCategory.userChosenAtom || targetCategoryJSEnum.val == TargetCategory.specificAtom)
                {
                    string targetAtomName = GetTargetAtomName();
                    if (targetAtomName!="") targetAtomNames.Add(targetAtomName);
                }
                else if (targetCategoryJSEnum.val == TargetCategory.scenePlugins || targetCategoryJSEnum.val == TargetCategory.sessionPlugins) targetAtomNames.Add("CoreControl");
                else if (targetCategoryJSEnum.val == TargetCategory.atomGroup)
                {
                    foreach (Atom atomFromGroup in AtomUtils.GetAtoms())
                    {
                        bool isMale = AtomUtils.IsMale(atomFromGroup);
                        bool isFemale = AtomUtils.IsFemale(atomFromGroup);

                        if (targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.topEnumValFlag)
                        {
                            switch (targetNameJSMultiEnum.valTopEnum)
                            {
                                case AllAtomsTargetType.allAtoms:
                                    targetAtomNames.Add(atomFromGroup.name);
                                    break;
                                case AllAtomsTargetType.allPersonAtoms:
                                    if (isMale || isFemale) targetAtomNames.Add(atomFromGroup.name);
                                    break;
                                case AllAtomsTargetType.allFemaleAtoms:
                                    if (isFemale) targetAtomNames.Add(atomFromGroup.name);
                                    break;
                                case AllAtomsTargetType.allMaleAtoms:
                                    if (isMale) targetAtomNames.Add(atomFromGroup.name);
                                    break;
                                case AllAtomsTargetType.allNonPersonAtoms:
                                    if (!isMale && !isFemale) targetAtomNames.Add(atomFromGroup.name);
                                    break;
                            }
                        }
                        else if (targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.mainValFlag)
                        {
                            string cgtName = targetNameJSMultiEnum.mainVal;
                            CustomTargetGroupType cgt = CustomTargetGroupSettings.GetCGTFromName(cgtName);

                            if (cgt.IsInScope(atomFromGroup, isMale, isFemale)) targetAtomNames.Add(atomFromGroup.name);
                        }
                    }
                }
            }

            return targetAtomNames;
        }

        public Atom GetTargetAtom()
        {
            return (SuperController.singleton.GetAtomByUid(GetTargetAtomName()));
        }
        public string GetTargetAtomName()
        {
            switch (targetCategoryJSEnum.val)
            {
                case TargetCategory.gazeSelectedAtom:
                    if (targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.topEnumValFlag)
                    {
                        if (targetNameJSMultiEnum.valTopEnum == LastViewedTargetType.lastViewedPerson) return TargetControl.lastViewedPerson;
                        if (targetNameJSMultiEnum.valTopEnum == LastViewedTargetType.lastViewedFemale) return TargetControl.lastViewedFemale;
                        if (targetNameJSMultiEnum.valTopEnum == LastViewedTargetType.lastViewedMale) return TargetControl.lastViewedMale;
                        if (targetNameJSMultiEnum.valTopEnum == LastViewedTargetType.lastViewedAtom) return TargetControl.lastViewedAtom;
                        if (targetNameJSMultiEnum.valTopEnum == LastViewedTargetType.lastViewedNonPerson) return TargetControl.lastViewedNonPerson;
                    }
                    else if (TargetControl.lastViewedCTGDic.ContainsKey(targetNameJSMultiEnum.mainVal)) return TargetControl.lastViewedCTGDic[targetNameJSMultiEnum.mainVal];
                    break;
                case TargetCategory.vamSelectedAtom:
                    if (targetNameJSMultiEnum.valType == JSONStorableMultiEnumStringChooser.topEnumValFlag)
                    {
                        if (targetNameJSMultiEnum.valTopEnum == LastSelectedTargetType.lastSelectedPerson) return TargetControl.lastSelectedPerson;
                        if (targetNameJSMultiEnum.valTopEnum == LastSelectedTargetType.lastSelectedFemale) return TargetControl.lastSelectedFemale;
                        if (targetNameJSMultiEnum.valTopEnum == LastSelectedTargetType.lastSelectedMale) return TargetControl.lastSelectedMale;
                        if (targetNameJSMultiEnum.valTopEnum == LastSelectedTargetType.lastSelectedAtom) return TargetControl.lastSelectedAtom;
                        if (targetNameJSMultiEnum.valTopEnum == LastSelectedTargetType.lastSelectedNonPerson) return TargetControl.lastSelectedNonPerson;
                    }
                    break;
                case TargetCategory.userChosenAtom:
                    List<string> choices = GetUserTargetAtomChoices();
                    if (choices.Count == 1) return choices[0];
                    if (parentButton.maxButtonStates > 1 && choices.Contains(lastUserChosenAtomName)) return lastUserChosenAtomName;
                    break;
                case TargetCategory.specificAtom:
                    return targetNameJSMultiEnum.mainVal;
                case TargetCategory.atomGroup:
                    if (UIAConsts.debugLogging) SuperController.LogMessage("UIA.TargetButtonComponent.GetLastViewedOrSelectedAtomName: attempt to get atom name for button with Group target");
                    break;
                case TargetCategory.scenePlugins:
                case TargetCategory.sessionPlugins:
                    if (UIAConsts.debugLogging) SuperController.LogMessage("UIA.TargetButtonComponent.GetLastViewedOrSelectedAtomName: attempt to get atom name for button with Scene/Session plugin target");
                    break;
            }

            return "";
        }
        public void TASSelected(List<Atom> atoms)
        {
            if (atoms.Count == 1)
            {
                Atom atom = atoms[0];
                if (atom != null)
                {
                    currentActionTargetAtomNames.Clear();
                    currentActionTargetAtomNames.Add(atom.name);
                    int buttonState = parentButton.GetButtonState();
                    if (parentButton.maxButtonStates > 1 && buttonState==ButtonState.inactive) lastUserChosenAtomName = atom.name;
                    else lastUserChosenAtomName = "";
                    parentButton.CheckActionTargets();
                }
            }
            else if (UIAConsts.debugLogging) SuperController.LogMessage("UIA.TargetButtonComponent.TASSelected: Unexpected number of atoms supplied as param");
        }

        public bool SpawnAtomIfTargetMissingToggleAvailable()
        {
            if (buttonTypeJSEnum.val == UIAButtonOpType.loadGeneralPreset && targetCategoryJSEnum.val == TargetCategory.specificAtom && specificAtomTypeJSS.val != "Person" && specificAtomTypeJSS.val != "PlayerNavigationPanel" && specificAtomTypeJSS.val != "WindowCamera") return true;
            return (false);
        }
        public bool SpawnAtomIfTargetMissing()
        {
            if (spawnAtomIfTargetMissingJSBool.val && SpawnAtomIfTargetMissingToggleAvailable()) return true;

            return (false);
        }
    }


    public class ButtonSkinComponent : ButtonComponentBase
    {
        public ButtonFontSizeRange fontSize;
        public ButtonFontSizeRange defaultMicroFontSize;
        public ButtonFontSizeRange defaultMiniFontSize;
        public ButtonFontSizeRange defaultSmallFontSize;
        public ButtonFontSizeRange defaultMediumFontSize;
        public ButtonFontSizeRange defaultLargeFontSize;

        public ButtonSkinToggleParam buttonSkinOnParams ;
        public ButtonSkinToggleParam buttonSkinOffParams;


        public JSONStorableBool fontSizeDefault = new JSONStorableBool("fontSizeDefault", true);

        public JSONStorableStringChooser textAlignment = new JSONStorableStringChooser("textAlignment",null, "MiddleCenter","");

        public JSONStorableBool textAlignmentDefault = new JSONStorableBool("textAlignmentDefault", true);

        public JSONStorableStringChooser buttonFont = new JSONStorableStringChooser("buttonFont",null, "Arial","");

        public JSONStorableBool buttonFontDefault = new JSONStorableBool("buttonFontDefault", true);

        public int maxButtonStates
        {
            get
            {
                if (defaultSkin) return 2;
                return parentButton.maxButtonStates;
            }
        }

        public JSONStorableBool usePresetButtonTexture ;
        private bool defaultSkin ;

        public ButtonSkinComponent(UIAButton parent) : base(parent)
        {
            try
            {
                usePresetButtonTexture = new JSONStorableBool("usePresetButtonTexture", true, UsePresetThumbnailUpdated);
                defaultSkin = parent==null;

                if (defaultSkin)
                {
                    defaultMicroFontSize = new ButtonFontSizeRange(3f, 12f);
                    defaultMiniFontSize = new ButtonFontSizeRange(3f, 15f);
                    defaultSmallFontSize = new ButtonFontSizeRange(4f, 15f);
                    defaultMediumFontSize = new ButtonFontSizeRange(4f, 25f);
                    defaultLargeFontSize = new ButtonFontSizeRange(4f, 35f);
                }
                else
                {
                    fontSize = new ButtonFontSizeRange(4f, 25f);
                    RegisterParam(fontSizeDefault);
                    RegisterParam(textAlignmentDefault);
                    RegisterParam(buttonFontDefault);
                    RegisterParam(usePresetButtonTexture);
                }

                List<string> fontPopupChoices = new List<string>();
                fontPopupChoices.Add("Arial");
                fontPopupChoices.Add("chintzy");
                fontPopupChoices.Add("DroidSansMono");
                fontPopupChoices.Add("Laffayette_Comic_Pro");
                buttonFont.choices = fontPopupChoices;

                List<string> alignmentPopupChoices = new List<string>();
                alignmentPopupChoices.Add("MiddleCenter");
                alignmentPopupChoices.Add("LowerCenter");
                alignmentPopupChoices.Add("UpperCenter");
                alignmentPopupChoices.Add("MiddleLeft");
                alignmentPopupChoices.Add("LowerLeft");
                alignmentPopupChoices.Add("UpperLeft");
                alignmentPopupChoices.Add("MiddleRight");
                alignmentPopupChoices.Add("LowerRight");
                alignmentPopupChoices.Add("UpperRight");
                textAlignment.choices = alignmentPopupChoices;

                buttonSkinOnParams = new ButtonSkinToggleParam(this);
                buttonSkinOffParams = new ButtonSkinToggleParam(this);
                buttonSkinOnParams.buttonColor.val = new HSVColor() { V = 0.84f };
                buttonSkinOffParams.buttonColor.val = new HSVColor() { V = 0.4f };

                RegisterParam(textAlignment);                
                RegisterParam(buttonFont);                
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }
        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);
            if (defaultSkin)
            {
                jc["defaultMicroFontSize"] = defaultMicroFontSize.GetJSON();
                jc["defaultMiniFontSize"] = defaultMiniFontSize.GetJSON();
                jc["defaultSmallFontSize"] = defaultSmallFontSize.GetJSON();
                jc["defaultMediumFontSize"] = defaultMediumFontSize.GetJSON();
                jc["defaultLargeFontSize"] = defaultLargeFontSize.GetJSON();
            }
            else jc["fontSize"] = fontSize.GetJSON();

            jc["buttonSkinOnParams"] = buttonSkinOnParams.GetJSON();
            if (maxButtonStates > 1) jc["buttonSkinOffParams"] = buttonSkinOffParams.GetJSON();

            return jc;
        }

        public void LoadJSON(JSONClass jc, string uiapPackageName, int uiapFormatVersion)
        {
            base.RestoreFromJSON(jc);

            if (uiapFormatVersion == 1)
            {
                if (defaultSkin)
                {
                    if (jc["minMicroFontSize"] != null) jc["defaultMicroFontSize"]["minFontSize"] = jc["minMicroFontSize"];
                    if (jc["maxMicroFontSize"] != null) jc["defaultMicroFontSize"]["maxFontSize"] = jc["maxMicroFontSize"];

                    if (jc["minMiniFontSize"] != null) jc["defaultMiniFontSize"]["minFontSize"] = jc["minMiniFontSize"];
                    if (jc["maxMiniFontSize"] != null) jc["defaultMiniFontSize"]["maxFontSize"] = jc["maxMiniFontSize"];

                    if (jc["minSmallFontSize"] != null) jc["defaultSmallFontSize"]["minFontSize"] = jc["minSmallFontSize"];
                    if (jc["maxSmallFontSize"] != null) jc["defaultSmallFontSize"]["maxFontSize"] = jc["maxSmallFontSize"];

                    if (jc["minMediumFontSize"] != null) jc["defaultMediumFontSize"]["minFontSize"] = jc["minMediumFontSize"];
                    if (jc["maxMediumFontSize"] != null) jc["defaultMediumFontSize"]["maxFontSize"] = jc["maxMediumFontSize"];

                    if (jc["minLargeFontSize"] != null) jc["defaultLargeFontSize"]["minFontSize"] = jc["minLargeFontSize"];
                    if (jc["maxLargeFontSize"] != null) jc["defaultLargeFontSize"]["maxFontSize"] = jc["maxLargeFontSize"];
                }
                else
                {
                    if (jc["minFontSize"] != null) jc["fontSize"]["minFontSize"] = jc["minFontSize"];
                    if (jc["maxFontSize"] != null) jc["fontSize"]["maxFontSize"] = jc["maxFontSize"];
                }

                if (jc["textOnColor"] != null)
                {
                    jc["buttonSkinOnParams"]["textColor"]["h"] = jc["textOnColor"]["H"];
                    jc["buttonSkinOnParams"]["textColor"]["s"] = jc["textOnColor"]["S"];
                    jc["buttonSkinOnParams"]["textColor"]["v"] = jc["textOnColor"]["V"];
                }
                if (jc["textOnColorDefault"] != null) jc["buttonSkinOnParams"]["textColorUseDefault"] = jc["textOnColorDefault"];
                if (jc["textOffColor"] != null && (maxButtonStates > 1|| defaultSkin))
                {
                    jc["buttonSkinOffParams"]["textColor"]["h"] = jc["textOffColor"]["H"];
                    jc["buttonSkinOffParams"]["textColor"]["s"] = jc["textOffColor"]["S"];
                    jc["buttonSkinOffParams"]["textColor"]["v"] = jc["textOffColor"]["V"];
                }
                if (jc["textOffColorDefault"] != null && maxButtonStates > 1) jc["buttonSkinOffParams"]["textColorUseDefault"] = jc["textOffColorDefault"];

                if (jc["buttonOnColor"] != null)
                {
                    jc["buttonSkinOnParams"]["buttonColor"]["h"] = jc["buttonOnColor"]["H"];
                    jc["buttonSkinOnParams"]["buttonColor"]["s"] = jc["buttonOnColor"]["S"];
                    jc["buttonSkinOnParams"]["buttonColor"]["v"] = jc["buttonOnColor"]["V"];
                }
                if (jc["buttonOnColorDefault"] != null) jc["buttonSkinOnParams"]["buttonColorUseDefault"] = jc["buttonOnColorDefault"];
                if (jc["buttonOffColor"] != null && (maxButtonStates > 1 || defaultSkin))
                {
                    jc["buttonSkinOffParams"]["buttonColor"]["h"] = jc["buttonOffColor"]["H"];
                    jc["buttonSkinOffParams"]["buttonColor"]["s"] = jc["buttonOffColor"]["S"];
                    jc["buttonSkinOffParams"]["buttonColor"]["v"] = jc["buttonOffColor"]["V"];
                }
                if (jc["buttonOffColorDefault"] != null && maxButtonStates > 1) jc["buttonSkinOffParams"]["buttonColorUseDefault"] = jc["buttonOffColorDefault"];

                if (jc["customButtonOnTextureURL"] != null) jc["buttonSkinOnParams"]["customTextureURL"] = jc["customButtonOnTextureURL"];
                if (jc["customButtonOnTextureDefault"] != null) jc["buttonSkinOnParams"]["customTextureURLUseDefault"] = jc["customButtonOnTextureDefault"];
                if (jc["customButtonOffTextureURL"] != null) jc["buttonSkinOffParams"]["customTextureURL"] = jc["customButtonOffTextureURL"];
                if (jc["customButtonOffTextureDefault"] != null) jc["buttonSkinOffParams"]["customTextureURLUseDefault"] = jc["customButtonOffTextureDefault"];
            }

            if (defaultSkin)
            {
                defaultMicroFontSize.RestoreFromJSON(jc["defaultMicroFontSize"].AsObject);
                defaultMiniFontSize.RestoreFromJSON(jc["defaultMiniFontSize"].AsObject);
                defaultSmallFontSize.RestoreFromJSON(jc["defaultSmallFontSize"].AsObject);
                defaultMediumFontSize.RestoreFromJSON(jc["defaultMediumFontSize"].AsObject);
                defaultLargeFontSize.RestoreFromJSON(jc["defaultLargeFontSize"].AsObject);
            }
            else fontSize.RestoreFromJSON(jc["fontSize"].AsObject);

            buttonSkinOnParams.LoadJSON(jc["buttonSkinOnParams"].AsObject, uiapPackageName);
            if (jc["buttonSkinOffParams"] != null) buttonSkinOffParams.LoadJSON(jc["buttonSkinOffParams"].AsObject, uiapPackageName);
        }

        public void CopyFrom(ButtonSkinComponent sourceBS)
        {
            base.CopyFrom(sourceBS);

            if (defaultSkin)
            {
                defaultMicroFontSize.CopyFrom(sourceBS.defaultMicroFontSize);
                defaultMiniFontSize.CopyFrom(sourceBS.defaultMiniFontSize);
                defaultSmallFontSize.CopyFrom(sourceBS.defaultSmallFontSize);
                defaultMediumFontSize.CopyFrom(sourceBS.defaultMediumFontSize);
                defaultLargeFontSize.CopyFrom(sourceBS.defaultLargeFontSize);
            }
            else fontSize.CopyFrom(sourceBS.fontSize);

            buttonSkinOnParams.CopyFrom(sourceBS.buttonSkinOnParams);
            if (sourceBS.maxButtonStates > 1) buttonSkinOffParams.CopyFrom(sourceBS.buttonSkinOffParams);


        }

        private void UsePresetThumbnailUpdated(bool usePreset)
        {
            parentButton.UpdateThumbnailImage(parentButton.buttonTexture);
        }
    }
    public class ButtonFontSizeRange : JSONStorableObject
    {
        public JSONStorableFloat minFontSize;
        public JSONStorableFloat maxFontSize;

        public ButtonFontSizeRange(float defaultMinFontSize, float defaultMaxFontSize)
        {
            minFontSize = new JSONStorableFloat("minFontSize", defaultMinFontSize, 2f, 40f);
            maxFontSize = new JSONStorableFloat("maxFontSize", defaultMaxFontSize, 2f, 40f);

            RegisterParam(minFontSize);
            RegisterParam(maxFontSize);
        }
    }

    public class ButtonSkinToggleParam : JSONStorableObject
    {
        public JSONStorableColor textColor = new JSONStorableColor("textColor", new HSVColor());
        public JSONStorableColor buttonColor = new JSONStorableColor("buttonColor", new HSVColor());
        public JSONStorableString customThumbnailURL;

        public JSONStorableBool textColorUseDefault = new JSONStorableBool("textColorUseDefault", true);
        public JSONStorableBool buttonColorUseDefault = new JSONStorableBool("buttonColorUseDefault", true);
        public JSONStorableBool customTextureURLUseDefault;
        public Texture2D buttonTexture;

        private ButtonSkinComponent parentSkinComponent;

        public ButtonSkinToggleParam(ButtonSkinComponent parent)
        {
            customThumbnailURL = new JSONStorableString("customTextureURL", "", QueueLoadTexture);
            customTextureURLUseDefault = new JSONStorableBool("customTextureURLUseDefault", true, TextureUseDefault);


            parentSkinComponent = parent;

            RegisterParam(textColor);
            RegisterParam(buttonColor);
            RegisterParam(customThumbnailURL);

            RegisterParam(textColorUseDefault);
            RegisterParam(buttonColorUseDefault);
            RegisterParam(customTextureURLUseDefault);
        }

        public void CopyFrom(ButtonSkinToggleParam source)
        {
            base.CopyFrom(source);
            buttonTexture = source.buttonTexture;
            RefreshButtonTexture();
        }

        public void LoadJSON(JSONClass jc, string uiapPackageName)
        {
            base.RestoreFromJSON(jc);

            if (customThumbnailURL.val != "")
            {
                string normalizedPath = FileManagerSecure.NormalizePath(customThumbnailURL.val);
                if (uiapPackageName != "" && !normalizedPath.Contains(":") && FileManagerSecure.FileExists(uiapPackageName + ":/" + normalizedPath)) customThumbnailURL.val = uiapPackageName + ":/" + normalizedPath;
            }
        }

        private void QueuedImageLoadCallback(ImageLoaderThreaded.QueuedImage qi)
        {
            Texture2D tex = qi.tex;
            buttonTexture = tex;
            RefreshButtonTexture();
        }

        public void QueueLoadTexture(string url)
        {
            
            if (string.IsNullOrEmpty(url) || !FileManagerSecure.FileExists(SuperController.singleton.NormalizeLoadPath(url)))
            {
                buttonTexture = null;
                RefreshButtonTexture();
                return;
            }
            ImageUtils.QueueLoadTexture(SuperController.singleton.NormalizeLoadPath(url), QueuedImageLoadCallback);
        }

        private void TextureUseDefault(bool useDefault)
        {
            RefreshButtonTexture();
        }
        private void RefreshButtonTexture()
        {
            if (parentSkinComponent.parentButton == null) GameControlUI.RefreshWristUIButtonGrid();
            else parentSkinComponent.parentButton.UpdateThumbnailImage(buttonTexture);  
        }
    }

    public class UIAButton : JSONStorableObject
    {

        public static Dictionary<string, List<string>> presetMergeClothingGeometryIDs = new Dictionary<string, List<string>>();
        public static Dictionary<string, JSONClass> cachedClothingPresetFiles = new Dictionary<string, JSONClass>();

        public static Dictionary<string, Dictionary<string,string>> presetUndressClothingGeometryIDsToSimStorables = new Dictionary<string, Dictionary<string, string>>();

        public List<UIAButtonOperation> buttonOperations = new List<UIAButtonOperation>();

        public static bool hudOnPreButtonAction = false;
        public delegate void UserAtomSelectedCallback(List<Atom> atoms);

        public int vrHand { get; private set; } = LeftRight.neither;
        public bool leapActivated { get; private set; }

        public int maxButtonStates
        {
            get
            {
                foreach (UIAButtonOperation buttonOp in buttonOperations)
                {
                    if (buttonOp.maxButtonOpStates == 2) return 2;
                }
                return 1;
            }
        }

        public Texture2D buttonTexture {
            get
            {
                FileReference fileRef = GetFirstThumbnailFileReferences();
                if (fileRef != null && buttonSkinComponent.usePresetButtonTexture.val)
                {
                    return fileRef.buttonTexture;
                }
                else 
                {
                    ButtonSkinToggleParam toggleParam = buttonSkinComponent.buttonSkinOffParams;
                    if (GetButtonState() == ButtonState.inactive) toggleParam = buttonSkinComponent.buttonSkinOnParams;

                    if (!toggleParam.customTextureURLUseDefault.val) return toggleParam.buttonTexture;
                }
                return null;
            }
        }
        public bool ContainsOneBlankOperations()
        {
            if (buttonOperations.Count == 1 && ContainsAllBlankOperations()) return true;
            return false;
        }

        public bool ContainsAllBlankOperations()
        {
            foreach (UIAButtonOperation buttonOp in buttonOperations)
            {
                if (buttonOp.buttonOpTypeJSEnum.val != UIAButtonOpType.blank) return (false);
            }
            return true;
        }

        public bool treeBrowserCollapsed = false;

        public JSONStorableString buttonLabelOnJSString;
        public JSONStorableString buttonLabelOffJSString;
        public JSONStorableBool autoLabelJSBool;
        public ButtonSkinComponent buttonSkinComponent;

        public UIAButtonGrid parentGrid { get; set; }
        public string buttonRef { get
            {
                return parentGrid.GetButtonRCRef(this);
            }
        }

        public int buttonColumn
        {
            get
            {
                return parentGrid.GetButtonColRefInGrid(this);
            }
        }
        public int buttonRow
        {
            get
            {
                return parentGrid.GetButtonRowRefInGrid(this);
            }
        }

        public int buttonGridIndex
        {
            get
            {
                return parentGrid.GetButtonIndexInGrid(this);
            }
        }

        public int GetButtonOperationIndex(UIAButtonOperation buttonOp)
        {
            return buttonOperations.IndexOf(buttonOp);
        }

        public UIAButton(UIAButtonGrid _parent)
        {
            buttonOperations.Add(new UIAButtonOperation(this));
            parentGrid = _parent;

            autoLabelJSBool = new JSONStorableBool("autoLabel", true);
            buttonLabelOnJSString = new JSONStorableString("buttonLabelOn", "");
            buttonLabelOffJSString = new JSONStorableString("buttonLabelOff", "");

            RegisterParam(autoLabelJSBool);
            RegisterParam(buttonLabelOnJSString);
            RegisterParam(buttonLabelOffJSString);
                
            buttonSkinComponent = new ButtonSkinComponent(this);             
        }
        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);

            if (!ContainsAllBlankOperations())
            {
                jc["ButtonSkin"] = buttonSkinComponent.GetJSON();
            }

            JSONArray buttonOperationJA = new JSONArray();
            foreach (UIAButtonOperation buttonOperation in buttonOperations)
            {
                buttonOperationJA.Add(buttonOperation.GetJSON());
            }
            jc["ButtonOperations"] = buttonOperationJA;
            return jc;
        }
        public void LoadJSON(JSONClass buttonJSON, string uiapPackageName, int uiapFormatVersion)
        {
            RestoreFromJSON(buttonJSON);
            buttonOperations.Clear();
            
            if (uiapFormatVersion == 1)
            {
                buttonOperations.Add(new UIAButtonOperation(this));
                buttonOperations[0].LoadJSON(buttonJSON, uiapPackageName, uiapFormatVersion);

                if (buttonJSON["useButtonTexture"] != null && buttonJSON["ButtonSkin"] != null) buttonJSON["ButtonSkin"]["usePresetButtonTexture"] = buttonJSON["useButtonTexture"];
                if (buttonJSON["ButtonSkin"] != null) buttonSkinComponent.LoadJSON((JSONClass)buttonJSON["ButtonSkin"], uiapPackageName, 1);

            }
            else
            {
                JSONArray buttonOperationsJA = buttonJSON["ButtonOperations"].AsArray;
                for (int i = 0; i < buttonOperationsJA.Count; i++)
                {
                    JSONClass buttonOperationJC = buttonOperationsJA[i].AsObject;
                    UIAButtonOperation buttonOpertation = new UIAButtonOperation(this);
                    buttonOperations.Add(buttonOpertation);
                    buttonOpertation.LoadJSON(buttonOperationJC, uiapPackageName,uiapFormatVersion);
                    
                }

                if (buttonJSON["ButtonSkin"] != null) buttonSkinComponent.LoadJSON((JSONClass)buttonJSON["ButtonSkin"], uiapPackageName, uiapFormatVersion);
                else buttonSkinComponent = new ButtonSkinComponent(this);
            }
           
        }
        public void CopyFrom(UIAButton bn)
        {
            if (bn != null)
            {
                base.CopyFrom(bn);
                
                buttonOperations.Clear();
                foreach (UIAButtonOperation sourceBO in bn.buttonOperations)
                {
                    UIAButtonOperation newBO = new UIAButtonOperation(this);
                    newBO.CopyFrom(sourceBO);
                    buttonOperations.Add(newBO);
                }

                buttonSkinComponent.CopyFrom(bn.buttonSkinComponent);
            }
        }
        public void AtomNameUpdate(string oldName, string newName)
        {
            foreach (UIAButtonOperation bo in buttonOperations)
            {
                bo.AtomNameUpdate(oldName, newName);
            }            
        }

        public void AtomRemovedUpdate(string oldName)
        { 
            foreach (UIAButtonOperation bo in buttonOperations)
            {
                bo.AtomRemovedUpdate(oldName);                
            }          
        }

        public void UpdateCTGName(string oldName, string newName)
        {
            foreach (UIAButtonOperation bo in buttonOperations)
            {
                bo.targetComponent.UpdateCGTName(oldName, newName);
            }

        }

        public void RemoveCTGName(string cgtName)
        {
            foreach (UIAButtonOperation bo in buttonOperations)
            {
                bo.targetComponent.RemoveCGTName(cgtName);
            }
        }

        public string GetManualLabel()
        {
            string rawLabel ;
            if (GetButtonState() == ButtonState.inactive) rawLabel = buttonLabelOnJSString.val;
            else rawLabel = buttonLabelOffJSString.val;

            if (buttonOperations.Count==1) return buttonOperations[0].LabelKeyWordsReplace(rawLabel);

            return (rawLabel);
        }
        public string GetAutoLabel()
        {
            if (buttonOperations.Count == 1)
            {
                return buttonOperations[0].GetAutoLabel();
            }

            return "";
        }


        public int GetButtonState()
        {
            if (maxButtonStates == 1) return ButtonState.inactive;
            foreach (UIAButtonOperation buttonOp in buttonOperations)
            {
                if (buttonOp.GetButtonOpState() == ButtonState.active) return ButtonState.active;
            }
            return ButtonState.inactive;
        }

        public List<FileReference> GetAllThumbnailFileReferences()
        {
            List<FileReference> fileRefList = new List<FileReference>();
            foreach (UIAButtonOperation buttonOp in buttonOperations)
            {
                buttonOp.AppendThumbnailFileReferences(fileRefList);
            }

            return fileRefList;
        }

        public FileReference GetFirstThumbnailFileReferences()
        {
            foreach (FileReference fileRef in GetAllThumbnailFileReferences())
            {
                return fileRef;
            }

            return null;
        }


        public void UpdateThumbnailImage(Texture2D tex)
        {
            if (buttonTexture== tex)GridsDisplay.UpdateButtonTextures(this);
        }


        public void ActionInit(int _vrHand, bool _leapActivated)
        {
            vrHand = _vrHand;
            leapActivated = _leapActivated;

            TargetResest();
            FileRefReset();
            CheckActionTargets();
        }

        private void TargetResest()
        {
            foreach (UIAButtonOperation buttonOp in buttonOperations)
            {
                buttonOp.TargetReset();
            }
        }

        private void FileRefReset()
        {
            foreach (UIAButtonOperation buttonOp in buttonOperations)
            {
                buttonOp.FileRefReset();
            }
            
        }

        public void CheckActionTargets()
        {
            foreach (UIAButtonOperation buttonOp in buttonOperations)
            {
                if (!buttonOp.CheckActionTarget()) return;
            }
            CheckActionFileReferences();
        }

        public void CheckActionFileReferences()
        {
            foreach (UIAButtonOperation buttonOp in buttonOperations)
            {
                if (!buttonOp.CheckActionFileReferences()) return;
            }

            uFileBrowser.FileBrowser browser = SuperController.singleton.mediaFileBrowserUI;
            if (!hudOnPreButtonAction && browser.IsHidden()) SuperController.singleton.HideMainHUD();

            PlayActions();
        }

        private void PlayActions()
        {
            int buttonState = GetButtonState();
            foreach (UIAButtonOperation buttonOp in buttonOperations)
            {
                buttonOp.PlayAction(buttonState);
            }

            
        }
       
    }


    public class UIAButtonOperation : JSONStorableObject
    {
        public UIAButton parentButton { get; set; }

        private int vrHandActionInit { get { return parentButton.vrHand; } } 
        private bool leapActivatedAction { get { return parentButton.leapActivated; } }
        public TargetButtonComponent targetComponent { get; protected set; }
        public JSONStorableEnumStringChooser buttonOpCategoryJSEnum { get; protected set; }
        public JSONStorableEnumStringChooser buttonOpTypeJSEnum { get; protected set; }

        public JSONStorableBool savePresetJSB;

        public Dictionary<int, FileReference> fileReferenceDict { get; protected set; }
        public PluginSettingComponent pluginSettingComponent { get; protected set; }
        public PluginsLoadComponent pluginsLoadComponent { get; protected set; }
        public AppearancePresetComponent appearancePresetComponent { get; protected set; }
        public ClothingComponent clothingComponent { get; protected set; }

        public VAMPlayEditModeComponent vamPlayEditModeComponent { get; protected set; }
        public SkinPresetDecalComponent skinPresetDecalComponent { get; protected set; }
        public RelativePositionComponent relativePositionComponent { get; protected set; }
        public SpawnAtomComponent spawnAtomComponent { get; protected set; }
        public MoveAtomComponent moveAtomComponent { get; protected set; }
        public MotionCaptureComponent motionCaptureComponent { get; protected set; }
        public WorldScaleComponent worldScaleComponent { get; protected set; }
        public ShowVRHandsComponent showVRHandsComponent { get; protected set; }
        public SwitchUIAGridComponent switchUIAGridComponent { get; protected set; }
        public HairColorComponent hairColorComponent { get; protected set; }
        public DecalMakerComponent decalMakerComponent { get; protected set; }
        public PresetLockComponent presetLockComponent { get; protected set; }

        public UserPreferencesComponent userPreferencesComponent { get; protected set; }

        public NodeControlComponent nodeControlOnComponent { get; protected set; }
        public NodeControlComponent nodeControlOffComponent { get; protected set; }

        public NodePhysicsComponent nodePhysicsOnComponent { get; protected set; }
        public NodePhysicsComponent nodePhysicsOffComponent { get; protected set; }
        public NodeSelectionComponent nodeSelectionComponent { get; protected set; }

        public string buttonOpRef
        {
            get
            {
                return (parentButton.GetButtonOperationIndex(this) + 1).ToString();
            }
        }

        public int buttonOpIndex
        {
            get
            {
                return parentButton.GetButtonOperationIndex(this);
            }
        }

        public UIAButtonOperation(UIAButton parent) : base()
        {
            parentButton = parent;            

            buttonOpCategoryJSEnum = new JSONStorableEnumStringChooser("buttonCategory", UIAButtonCategory.enumManifestName, UIAButtonCategory.misc, "", ButtonCategoryChanged);
            buttonOpTypeJSEnum = new JSONStorableEnumStringChooser("buttonType", UIAButtonOpType.enumManifestName, UIAButtonOpType.blank, "", ButtonTypeChanged);

            savePresetJSB = new JSONStorableBool("savePreset", false, SavePresetChanged);

            RegisterParam(buttonOpTypeJSEnum);
            RegisterParam(savePresetJSB);

            targetComponent = new TargetButtonComponent(buttonOpTypeJSEnum, this);
            fileReferenceDict = new Dictionary<int, FileReference>();

            CreateButtonComponents();
        }
        public void CreateButtonComponents()
        {
            List<int> components = GetComponentTypes();

            if (components.Contains(ButtonComponentTypes.pluginSettingComponent) && pluginSettingComponent==null) pluginSettingComponent = new PluginSettingComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.pluginsLoadComponent) && pluginsLoadComponent == null) pluginsLoadComponent = new PluginsLoadComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.appearancePresetComponent) && appearancePresetComponent == null) appearancePresetComponent = new AppearancePresetComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.clothingComponent) && clothingComponent == null) clothingComponent = new ClothingComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.vamPlayEditModeComponent) && vamPlayEditModeComponent == null) vamPlayEditModeComponent = new VAMPlayEditModeComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.skinPresetDecalComponent) && skinPresetDecalComponent == null) skinPresetDecalComponent = new SkinPresetDecalComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.relativePositionComponent) && relativePositionComponent == null) relativePositionComponent = new RelativePositionComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.spawnAtomComponent) && spawnAtomComponent == null) spawnAtomComponent = new SpawnAtomComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.moveAtomComponent) && moveAtomComponent == null) moveAtomComponent = new MoveAtomComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.motionCaptureComponent) && motionCaptureComponent == null) motionCaptureComponent = new MotionCaptureComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.worldScaleComponent) && worldScaleComponent == null) worldScaleComponent = new WorldScaleComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.showVRHandsComponent) && showVRHandsComponent == null) showVRHandsComponent = new ShowVRHandsComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.switchUIAGridComponent) && switchUIAGridComponent == null) switchUIAGridComponent = new SwitchUIAGridComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.hairColorComponent) && hairColorComponent == null) hairColorComponent = new HairColorComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.decalMakerComponent) && decalMakerComponent == null) decalMakerComponent = new DecalMakerComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.presetLockComponent) && presetLockComponent == null) presetLockComponent = new PresetLockComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.nodeControlOnComponent) && nodeControlOnComponent == null) nodeControlOnComponent = new NodeControlComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.nodeControlOffComponent) && nodeControlOffComponent == null) nodeControlOffComponent = new NodeControlComponent(buttonOpTypeJSEnum, this);

            if (components.Contains(ButtonComponentTypes.nodePhysicsOnComponent) && nodePhysicsOnComponent == null) nodePhysicsOnComponent = new NodePhysicsComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.nodePhysicsOffComponent) && nodePhysicsOffComponent == null) nodePhysicsOffComponent = new NodePhysicsComponent(buttonOpTypeJSEnum, this);
            if (components.Contains(ButtonComponentTypes.nodeSelectionComponent) && nodeSelectionComponent == null) nodeSelectionComponent = new NodeSelectionComponent(this);

            if (components.Contains(ButtonComponentTypes.userPreferencesComponent) && userPreferencesComponent == null) userPreferencesComponent = new UserPreferencesComponent(buttonOpTypeJSEnum, this);
        }
        public string GetAutoLabel()
        {
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.offsetWorldScale)
            {
                if (worldScaleComponent.worldScaleIncrementJSF.val < 0) return "Offset World Scale by -" + worldScaleComponent.worldScaleIncrementJSF.val.ToString("0.##");
                return "Offset World Scale by +" + worldScaleComponent.worldScaleIncrementJSF.val.ToString("0.##");
            }
            else
            {
                string rawLabel;
                if (parentButton.GetButtonState() == ButtonState.inactive) rawLabel = UIAButtonOpType.GetInactiveLabel(buttonOpTypeJSEnum.val);
                else rawLabel = UIAButtonOpType.GetActiveLabel(buttonOpTypeJSEnum.val);

                return LabelKeyWordsReplace(rawLabel);
            }
        }
        public override JSONClass GetJSON(HashSet<JSONStorableParam> jspLoadExclusions = null)
        {
            JSONClass jc = base.GetJSON(jspLoadExclusions);

            foreach (int fileRefType in GetFileReferenceTypes())
            {
                if (fileReferenceDict.ContainsKey(fileRefType)) jc[EnumManifest.GetStoreVal(FileReferenceTypes.enumManifestName, fileRefType)] = fileReferenceDict[fileRefType].GetJSON();
            }

            if (IsComponentInButtonOpType(ButtonComponentTypes.targetComponent))
            {
                HashSet<JSONStorableParam> jspExclusions = new HashSet<JSONStorableParam>();
                jspExclusions.Add(targetComponent.targetNameJSMultiEnum);
                if (targetComponent.targetCategoryJSEnum.val == TargetCategory.scenePlugins || targetComponent.targetCategoryJSEnum.val == TargetCategory.sessionPlugins)
                    jc["TargetComponent"] = targetComponent.GetJSON(jspExclusions);
                else jc["TargetComponent"] = targetComponent.GetJSON();
            }
            if (IsComponentInButtonOpType(ButtonComponentTypes.appearancePresetComponent)&& buttonOpTypeJSEnum.val==UIAButtonOpType.loadAppPreset) jc["AppearancePresetComponent"] = appearancePresetComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.clothingComponent) && buttonOpTypeJSEnum.val==UIAButtonOpType.mergeClothingPreset) jc["ClothingComponent"] = clothingComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.vamPlayEditModeComponent)) jc["VAMPlayEditModeComponent"] = vamPlayEditModeComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.skinPresetDecalComponent)) jc["SkinPresetDecalComponent"] = skinPresetDecalComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.pluginsLoadComponent)) jc["PluginLoadComponent"] = pluginsLoadComponent.GetJSON();

            if (IsComponentInButtonOpType(ButtonComponentTypes.pluginSettingComponent)) jc["PluginSettingComponent"] = pluginSettingComponent.GetJSON();

            if (IsComponentInButtonOpType(ButtonComponentTypes.spawnAtomComponent)) jc["SpawnAtomComponent"] = spawnAtomComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.relativePositionComponent)) jc["RelativePositionComponent"] = relativePositionComponent.GetJSON();

            if (IsComponentInButtonOpType(ButtonComponentTypes.moveAtomComponent)) jc["MoveAtomComponent"] = moveAtomComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.motionCaptureComponent)) jc["MotionCaptureComponent"] = motionCaptureComponent.GetJSON();

            if (IsComponentInButtonOpType(ButtonComponentTypes.worldScaleComponent)) jc["WorldScaleComponent"] = worldScaleComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.showVRHandsComponent)) jc["ShowVRHandsComponent"] = showVRHandsComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.switchUIAGridComponent)) jc["SwitchUIAScreenComponent"] = switchUIAGridComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.hairColorComponent)) jc["HairColorComponent"] = hairColorComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.decalMakerComponent)) jc["DecalMakerComponent"] = decalMakerComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.presetLockComponent)) jc["PresetLockComponent"] = presetLockComponent.GetJSON();

            if (IsComponentInButtonOpType(ButtonComponentTypes.nodeControlOnComponent)) jc["nodeControlONComponent"] = nodeControlOnComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.nodeControlOffComponent)) jc["nodeControlOFFComponent"] = nodeControlOffComponent.GetJSON();

            if (IsComponentInButtonOpType(ButtonComponentTypes.nodePhysicsOnComponent)) jc["nodePhysicsOnComponent"] = nodePhysicsOnComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.nodePhysicsOffComponent)) jc["nodePhysicsOffComponent"] = nodePhysicsOffComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.nodeSelectionComponent)) jc["nodeSelectionComponent"] = nodeSelectionComponent.GetJSON();
            if (IsComponentInButtonOpType(ButtonComponentTypes.userPreferencesComponent)) jc["UserPreferencesComponent"] = userPreferencesComponent.GetJSON();


            return jc;
        }

        public void LoadJSON(JSONClass buttonOperationJSON, string uiapPackageName, int uiapFormatVersion)
        {           
            RestoreFromJSON(buttonOperationJSON);          
            buttonOpCategoryJSEnum.val = UIAButtonOpType.GetButtonCategory(buttonOpTypeJSEnum.val);     
            if (uiapFormatVersion == 1)
            {
                if (IsComponentInButtonOpType(ButtonComponentTypes.spawnAtomComponent)) spawnAtomComponent.RestoreFromJSON(buttonOperationJSON);

                foreach (int fileRefType in GetFileReferenceTypes())
                {
                    string legacyFileStoreNumber = UIAButtonOpType.GetLegacyUIAPFileStoreNumber(buttonOpTypeJSEnum.val, fileRefType);
                    string legacyFilePathStoreNumber = legacyFileStoreNumber;
                    if (legacyFilePathStoreNumber == "-1") break;
                    if (legacyFilePathStoreNumber == "1") legacyFilePathStoreNumber = "";

                    if (buttonOperationJSON["preset" + legacyFilePathStoreNumber + "FilePath"] != null)
                    {
                        JSONClass jc = new JSONClass();
                        jc["filePath"] = buttonOperationJSON["preset" + legacyFilePathStoreNumber + "FilePath"];
                        if (buttonOperationJSON["preset" + legacyFileStoreNumber + "Type"] != null) jc["fileSelectionMode"] = buttonOperationJSON["preset" + legacyFileStoreNumber + "Type"];
                        if (buttonOperationJSON["useLatestVARforPresets"] != null) jc["useLatestVAR"] = buttonOperationJSON["useLatestVARforPresets"];
                        if (buttonOperationJSON["mergeLoadPreset"] != null) jc["mergeLoadPreset"] = buttonOperationJSON["mergeLoadPreset"];
                        buttonOperationJSON[EnumManifest.GetStoreVal(FileReferenceTypes.enumManifestName, fileRefType)] = jc;
                    }
                }
            }
            else
            {
                if (buttonOperationJSON["SpawnAtomComponent"] != null) spawnAtomComponent.RestoreFromJSON(buttonOperationJSON["SpawnAtomComponent"].AsObject);
                else spawnAtomComponent = new SpawnAtomComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["TargetComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.targetComponent))
                {
                    targetComponent.RestoreFromJSON(buttonOperationJSON["TargetComponent"].AsObject);
                    targetComponent.RefreshTargetNameChoices(false);
                }
                else if (IsComponentInButtonOpType(ButtonComponentTypes.targetComponent)) targetComponent = new TargetButtonComponent(buttonOpTypeJSEnum, this);
            }

            foreach (int fileRefType in GetFileReferenceTypes())
            {
                string fileRefStoreID = EnumManifest.GetStoreVal(FileReferenceTypes.enumManifestName, fileRefType);
                if (!fileReferenceDict.ContainsKey(fileRefType)) fileReferenceDict.Add(fileRefType, new FileReference(fileRefType, this));
                if (buttonOperationJSON[fileRefStoreID] != null)
                {
                    fileReferenceDict[fileRefType].RestoreFromJSON(buttonOperationJSON[fileRefStoreID].AsObject, uiapPackageName);
                }
                else if ((buttonOperationJSON["pluginsPreset"] != null))
                {
                    // Correcting an error where Session & Scene plugin presets were being storted as an Atom Plugin Preset File Ref
                    if (targetComponent.targetCategoryJSEnum.val == TargetCategory.scenePlugins)
                    {
                        fileReferenceDict[FileReferenceTypes.scenePluginPreset].RestoreFromJSON(buttonOperationJSON["pluginsPreset"].AsObject, uiapPackageName);
                    }
                    if (targetComponent.targetCategoryJSEnum.val == TargetCategory.sessionPlugins)
                    {
                        fileReferenceDict[FileReferenceTypes.sessionPluginPreset].RestoreFromJSON(buttonOperationJSON["pluginsPreset"].AsObject, uiapPackageName);
                    }
                }
            }

            if (uiapFormatVersion == 1)
            {
                targetComponent.RestoreFromJSON(buttonOperationJSON, "targetCategory");
                string legacyVal = buttonOperationJSON["targetName"];
              
                if (legacyVal != null && legacyVal != "" && legacyVal != "<None available>")
                {
                    if (UserChosenTargetType.storeValList.Contains(legacyVal) || LastViewedTargetType.storeValList.Contains(legacyVal) || LastSelectedTargetType.storeValList.Contains(legacyVal) || AllAtomsTargetType.storeValList.Contains(legacyVal) || legacyVal == "") buttonOperationJSON["targetName"] = "T" + legacyVal;
                    else
                    {
                        int preFixLength = targetComponent.targetNameJSMultiEnum.displayChoicePreFix.Length;
                        int posFixLength = targetComponent.targetNameJSMultiEnum.displayChoicePostFix.Length;
                        buttonOperationJSON["targetName"] = "M" + legacyVal.Substring(preFixLength, legacyVal.Length - preFixLength - posFixLength);
                    }
                    targetComponent.RestoreFromJSON(buttonOperationJSON);
                }
                else if (legacyVal == "") targetComponent.RestoreFromJSON(buttonOperationJSON);

                if (buttonOpTypeJSEnum.val == UIAButtonOpType.loadGeneralPreset &&  targetComponent.targetCategoryJSEnum.val== TargetCategory.specificAtom && targetComponent.spawnAtomIfTargetMissingJSBool.val && buttonOperationJSON["atomType"]!=null)
                {
                    targetComponent.specificAtomTypeJSS.val = buttonOperationJSON["atomType"];
                }

                if (IsComponentInButtonOpType(ButtonComponentTypes.pluginSettingComponent)) pluginSettingComponent.RestoreFromJSON(buttonOperationJSON, uiapFormatVersion);
                if (IsComponentInButtonOpType(ButtonComponentTypes.pluginsLoadComponent))
                {
                    if (buttonOperationJSON["pluginLoads"] != null && buttonOpTypeJSEnum.val != UIAButtonOpType.blank) pluginsLoadComponent.LoadJSON(buttonOperationJSON, uiapPackageName);
                    else pluginsLoadComponent = new PluginsLoadComponent(buttonOpTypeJSEnum, this);
                }
                    
                if (IsComponentInButtonOpType(ButtonComponentTypes.appearancePresetComponent)) appearancePresetComponent.RestoreFromJSON(buttonOperationJSON);
                if (IsComponentInButtonOpType(ButtonComponentTypes.clothingComponent)) clothingComponent.LoadJSON(buttonOperationJSON, uiapFormatVersion);
                if (IsComponentInButtonOpType(ButtonComponentTypes.vamPlayEditModeComponent)) vamPlayEditModeComponent.RestoreFromJSON(buttonOperationJSON);
                if (IsComponentInButtonOpType(ButtonComponentTypes.skinPresetDecalComponent)) skinPresetDecalComponent.RestoreFromJSON(buttonOperationJSON);

                if (IsComponentInButtonOpType(ButtonComponentTypes.relativePositionComponent)) relativePositionComponent.RestoreFromJSON(buttonOperationJSON);
                if (IsComponentInButtonOpType(ButtonComponentTypes.moveAtomComponent)) moveAtomComponent.RestoreFromJSON(buttonOperationJSON);
                if (IsComponentInButtonOpType(ButtonComponentTypes.motionCaptureComponent)) motionCaptureComponent.RestoreFromJSON(buttonOperationJSON);
                if (IsComponentInButtonOpType(ButtonComponentTypes.decalMakerComponent)) decalMakerComponent.LoadJSON(buttonOperationJSON, uiapFormatVersion);
                if (IsComponentInButtonOpType(ButtonComponentTypes.presetLockComponent)) presetLockComponent.RestoreFromJSON(buttonOperationJSON);
                if (IsComponentInButtonOpType(ButtonComponentTypes.hairColorComponent)) hairColorComponent.RestoreFromJSON(buttonOperationJSON);

                if (IsComponentInButtonOpType(ButtonComponentTypes.worldScaleComponent)) worldScaleComponent.RestoreFromJSON(buttonOperationJSON);
                if (IsComponentInButtonOpType(ButtonComponentTypes.showVRHandsComponent)) showVRHandsComponent.RestoreFromJSON(buttonOperationJSON);
                if (IsComponentInButtonOpType(ButtonComponentTypes.switchUIAGridComponent)) switchUIAGridComponent.RestoreFromJSON(buttonOperationJSON,null);
            

            }
            else
            {
                if (buttonOperationJSON["TargetComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.targetComponent))
                {
                    targetComponent.RestoreFromJSON(buttonOperationJSON["TargetComponent"].AsObject);
                    targetComponent.RefreshTargetNameChoices(false);
                }               
                else if (IsComponentInButtonOpType(ButtonComponentTypes.targetComponent))targetComponent = new TargetButtonComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["PluginSettingComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.pluginSettingComponent)) pluginSettingComponent.RestoreFromJSON(buttonOperationJSON["PluginSettingComponent"].AsObject, uiapFormatVersion);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.pluginSettingComponent)) pluginSettingComponent = new PluginSettingComponent(buttonOpTypeJSEnum, this);
                if (buttonOperationJSON["PluginLoadComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.pluginsLoadComponent)) pluginsLoadComponent.LoadJSON(buttonOperationJSON["PluginLoadComponent"].AsObject, uiapPackageName);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.pluginsLoadComponent)) pluginsLoadComponent = new PluginsLoadComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["AppearancePresetComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.appearancePresetComponent)) appearancePresetComponent.RestoreFromJSON(buttonOperationJSON["AppearancePresetComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.appearancePresetComponent)) appearancePresetComponent = new AppearancePresetComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["ClothingComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.clothingComponent)) clothingComponent.LoadJSON(buttonOperationJSON["ClothingComponent"].AsObject, uiapFormatVersion);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.clothingComponent)) clothingComponent = new ClothingComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["VAMPlayEditModeComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.vamPlayEditModeComponent)) vamPlayEditModeComponent.RestoreFromJSON(buttonOperationJSON["VAMPlayEditModeComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.vamPlayEditModeComponent)) vamPlayEditModeComponent = new VAMPlayEditModeComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["SkinPresetDecalComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.skinPresetDecalComponent)) skinPresetDecalComponent.RestoreFromJSON(buttonOperationJSON["SkinPresetDecalComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.skinPresetDecalComponent)) skinPresetDecalComponent = new SkinPresetDecalComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["RelativePositionComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.relativePositionComponent)) relativePositionComponent.RestoreFromJSON(buttonOperationJSON["RelativePositionComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.relativePositionComponent)) relativePositionComponent = new RelativePositionComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["MoveAtomComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.moveAtomComponent)) moveAtomComponent.RestoreFromJSON(buttonOperationJSON["MoveAtomComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.moveAtomComponent)) moveAtomComponent = new MoveAtomComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["MotionCaptureComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.motionCaptureComponent)) motionCaptureComponent.RestoreFromJSON(buttonOperationJSON["MotionCaptureComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.motionCaptureComponent)) motionCaptureComponent = new MotionCaptureComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["DecalMakerComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.decalMakerComponent)) decalMakerComponent.LoadJSON(buttonOperationJSON["DecalMakerComponent"].AsObject, uiapFormatVersion);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.decalMakerComponent)) decalMakerComponent = new DecalMakerComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["PresetLockComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.presetLockComponent)) presetLockComponent.RestoreFromJSON(buttonOperationJSON["PresetLockComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.presetLockComponent)) presetLockComponent = new PresetLockComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["HairColorComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.hairColorComponent)) hairColorComponent.RestoreFromJSON(buttonOperationJSON["HairColorComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.hairColorComponent)) hairColorComponent = new HairColorComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["WorldScaleComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.worldScaleComponent)) worldScaleComponent.RestoreFromJSON(buttonOperationJSON["WorldScaleComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.worldScaleComponent)) worldScaleComponent = new WorldScaleComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["UserPreferencesComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.userPreferencesComponent)) userPreferencesComponent.RestoreFromJSON(buttonOperationJSON["UserPreferencesComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.userPreferencesComponent)) userPreferencesComponent = new UserPreferencesComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["ShowVRHandsComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.showVRHandsComponent)) showVRHandsComponent.RestoreFromJSON(buttonOperationJSON["ShowVRHandsComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.showVRHandsComponent)) showVRHandsComponent = new ShowVRHandsComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["SwitchUIAScreenComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.switchUIAGridComponent)) switchUIAGridComponent.RestoreFromJSON(buttonOperationJSON["SwitchUIAScreenComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.switchUIAGridComponent)) switchUIAGridComponent = new SwitchUIAGridComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["nodeControlONComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.nodeControlOnComponent)) nodeControlOnComponent.RestoreFromJSON(buttonOperationJSON["nodeControlONComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.nodeControlOnComponent)) nodeControlOnComponent = new NodeControlComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["nodeControlOFFComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.nodeControlOffComponent)) nodeControlOffComponent.RestoreFromJSON(buttonOperationJSON["nodeControlOFFComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.nodeControlOffComponent)) nodeControlOffComponent = new NodeControlComponent(buttonOpTypeJSEnum, this);

                if (buttonOperationJSON["nodePhysicsOnComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.nodePhysicsOnComponent)) nodePhysicsOnComponent.LoadJSON(buttonOperationJSON["nodePhysicsOnComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.nodePhysicsOnComponent)) nodePhysicsOnComponent = new NodePhysicsComponent(buttonOpTypeJSEnum, this);
                if (buttonOperationJSON["nodePhysicsOffComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.nodePhysicsOffComponent)) nodePhysicsOffComponent.LoadJSON(buttonOperationJSON["nodePhysicsOffComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.nodePhysicsOffComponent)) nodePhysicsOffComponent = new NodePhysicsComponent(buttonOpTypeJSEnum, this);
                if (buttonOperationJSON["nodeSelectionComponent"] != null && IsComponentInButtonOpType(ButtonComponentTypes.nodeSelectionComponent)) nodeSelectionComponent.RestoreFromJSON(buttonOperationJSON["nodeSelectionComponent"].AsObject);
                else if (IsComponentInButtonOpType(ButtonComponentTypes.nodeSelectionComponent)) nodeSelectionComponent = new NodeSelectionComponent(this);
            }
        }

        public void CopyFrom(UIAButtonOperation sourceBO)
        {
            if (sourceBO != null)
            {
                base.CopyFrom(sourceBO);

                buttonOpCategoryJSEnum.val = UIAButtonOpType.GetButtonCategory(buttonOpTypeJSEnum.val);

                fileReferenceDict.Clear();

                foreach (KeyValuePair<int, FileReference> kvp in sourceBO.fileReferenceDict)
                {
                    fileReferenceDict.Add(kvp.Key, new FileReference(kvp.Value.fileReferenceType, this));
                    fileReferenceDict[kvp.Key].CopyFrom(sourceBO.fileReferenceDict[kvp.Key]);
                }
                if (IsComponentInButtonOpType(ButtonComponentTypes.targetComponent))targetComponent.CopyFrom(sourceBO.targetComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.pluginSettingComponent)) pluginSettingComponent.CopyFrom(sourceBO.pluginSettingComponent);

                if (IsComponentInButtonOpType(ButtonComponentTypes.appearancePresetComponent)) appearancePresetComponent.CopyFrom(sourceBO.appearancePresetComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.clothingComponent)) clothingComponent.CopyFrom(sourceBO.clothingComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.vamPlayEditModeComponent)) vamPlayEditModeComponent.CopyFrom(sourceBO.vamPlayEditModeComponent);

                if (IsComponentInButtonOpType(ButtonComponentTypes.spawnAtomComponent)) spawnAtomComponent.CopyFrom(sourceBO.spawnAtomComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.relativePositionComponent)) relativePositionComponent.CopyFrom(sourceBO.relativePositionComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.skinPresetDecalComponent)) skinPresetDecalComponent.CopyFrom(sourceBO.skinPresetDecalComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.pluginsLoadComponent)) pluginsLoadComponent.CopyFrom(sourceBO.pluginsLoadComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.moveAtomComponent)) moveAtomComponent.CopyFrom(sourceBO.moveAtomComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.motionCaptureComponent)) motionCaptureComponent.CopyFrom(sourceBO.motionCaptureComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.worldScaleComponent)) worldScaleComponent.CopyFrom(sourceBO.worldScaleComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.showVRHandsComponent)) showVRHandsComponent.CopyFrom(sourceBO.showVRHandsComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.switchUIAGridComponent)) switchUIAGridComponent.CopyFrom(sourceBO.switchUIAGridComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.hairColorComponent)) hairColorComponent.CopyFrom(sourceBO.hairColorComponent);

                if (IsComponentInButtonOpType(ButtonComponentTypes.decalMakerComponent)) decalMakerComponent.CopyFrom(sourceBO.decalMakerComponent);

                if (IsComponentInButtonOpType(ButtonComponentTypes.nodeControlOnComponent)) nodeControlOnComponent.CopyFrom(sourceBO.nodeControlOnComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.nodeControlOffComponent)) nodeControlOffComponent.CopyFrom(sourceBO.nodeControlOffComponent);

                if (IsComponentInButtonOpType(ButtonComponentTypes.nodePhysicsOnComponent)) nodePhysicsOnComponent.CopyFrom(sourceBO.nodePhysicsOnComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.nodePhysicsOffComponent)) nodePhysicsOffComponent.CopyFrom(sourceBO.nodePhysicsOffComponent);
                if (IsComponentInButtonOpType(ButtonComponentTypes.nodeSelectionComponent)) nodeSelectionComponent.CopyFrom(sourceBO.nodeSelectionComponent);
            }
        }

        public int maxButtonOpStates
        {
            get
            {
                return UIAButtonOpType.GetMaxStates(buttonOpTypeJSEnum.val);
            }
        }

        private int GetBoolToggleButtonState(bool toggle)
        {
            if (toggle) return ButtonState.active;
            else return ButtonState.inactive;
        }

        public int GetButtonOpState()
        {
            if (maxButtonOpStates == 1) return ButtonState.inactive;

            if (buttonOpTypeJSEnum.val == UIAButtonOpType.suppressScaleLoad) return GetBoolToggleButtonState(PresetLoadSettings.suppressScaleLoadJSB.val);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.suppressClothingLoad) return GetBoolToggleButtonState(PresetLoadSettings.suppressClothingLoadJSB.val);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleLoopPlayback) return GetBoolToggleButtonState(SuperController.singleton.motionAnimationMaster.loop);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.freezeMotionSound) return GetBoolToggleButtonState(SuperController.singleton.freezeAnimation);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.togglePlayEdit) return GetBoolToggleButtonState(SuperController.singleton.gameMode == SuperController.GameMode.Edit);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.moveAtom) return GetBoolToggleButtonState(MoveAtomComponent.atomVRMoveActive && MoveAtomComponent.atomVRMoveUIAButton == this);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.softBodyPhysicsToggle) return GetBoolToggleButtonState(UserPreferences.singleton.softPhysics);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.desktopVSyncToggle) return GetBoolToggleButtonState(UserPreferences.singleton.desktopVsync);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.vrHeadCollider) return GetBoolToggleButtonState(UserPreferences.singleton.useHeadCollider);

            if (buttonOpTypeJSEnum.val == UIAButtonOpType.realtimeReflectionProbesToggle) return GetBoolToggleButtonState(UserPreferences.singleton.realtimeReflectionProbes);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.mirrorReflectionsToggle) return GetBoolToggleButtonState(UserPreferences.singleton.mirrorReflections);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.hqPhysicsToggle) return GetBoolToggleButtonState(UserPreferences.singleton.physicsHighQuality);

            if (buttonOpTypeJSEnum.val == UIAButtonOpType.showVRHands) return showVRHandsComponent.GetShowVRHandButtonState();
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.leapMotionToggle) return showVRHandsComponent.GetToggleLeapMotionButtonState();
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.showHiddenAtoms) return GetBoolToggleButtonState(SuperController.singleton.showHiddenAtoms);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.gazeAssistedSelect) return GetBoolToggleButtonState(GazeAssistedSelectTool.gazeAssistActiveJSB.val);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.hideInactiveTargets) return GetBoolToggleButtonState(UserPreferences.singleton.hideInactiveTargets);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.suppressPresetLocks) return GetBoolToggleButtonState(PresetLoadSettings.suppressPresetLocksJSB.val);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.activeClothingEditor) return GetBoolToggleButtonState(GameControlUI.gameControlDisplayMode == GameControlDisplayModes.ace || GameControlUI.gameControlDisplayMode == GameControlDisplayModes.clothItemPresetSelect);

            if (buttonOpTypeJSEnum.val == UIAButtonOpType.raiseByHAHeight) return GetBoolToggleButtonState(HeelAdjustTool.heelAdjustRaisePeopleJSB.val);
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.perfMon)
            {
                PerfMon pm = UserPreferences.singleton.transform.Find("PerfMon").GetComponent<PerfMon>();
                return GetBoolToggleButtonState(pm.onToggle.isOn);
            }
            
            // If the button target is a User Chosen Atom and no atom has been chosen yet and there are multiple choices available, then the button state must be inactive.
            if (targetComponent.targetCategoryJSEnum.val == TargetCategory.userChosenAtom && targetComponent.lastUserChosenAtomName == "" && targetComponent.GetUserTargetAtomChoices().Count > 1) return ButtonState.inactive;

            if (IsComponentInButtonOpType(ButtonComponentTypes.targetComponent) && maxButtonOpStates > 1)
            {
                List<string> atomNames = targetComponent.GetTargetAtomNames();
                bool sessionPluginsTarget = (targetComponent.targetCategoryJSEnum.val == TargetCategory.sessionPlugins);
                bool scenePluginsTarget = (targetComponent.targetCategoryJSEnum.val == TargetCategory.scenePlugins);

                int activeAtomsCount = 0;
                foreach (string atomName in atomNames)
                {
                    Atom atom = SuperController.singleton.GetAtomByUid(atomName);
                    if (atom != null)
                    {
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.detachAtomRoot)
                        {
                            JSONStorable storable = atom.GetStorableByID("control");
                            if (storable != null)
                            {
                                JSONStorableBool detach = storable.GetBoolJSONParam("detachControl");
                                if (detach != null && GetBoolToggleButtonState(detach.val) == ButtonState.inactive) return ButtonState.inactive;
                                else activeAtomsCount++;
                            }
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.hideAtom)
                        {
                            if (GetBoolToggleButtonState(atom.hidden) == ButtonState.inactive) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleAtomOn)
                        {
                            if (GetBoolToggleButtonState(atom.on) == ButtonState.inactive) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleAtomCollision)
                        {
                            if (GetBoolToggleButtonState(atom.collisionEnabledJSON.val) == ButtonState.inactive) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginActionToggle || buttonOpTypeJSEnum.val == UIAButtonOpType.pluginMultiSettingsToggle)
                        {
                            if (pluginSettingComponent.atomButtonToggleStates.ContainsKey(atomName)) return pluginSettingComponent.atomButtonToggleStates[atomName];
                            pluginSettingComponent.atomButtonToggleStates.Add(atomName, ButtonState.inactive);
                            return ButtonState.inactive;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.togglePresetLocks)
                        {
                            if (presetLockComponent.GetPresetLockAtomState(atom) == ButtonState.inactive) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginBoolToggle)
                        {
                            List<MVRScript> plugins;
                            if (sessionPluginsTarget) plugins = PluginUtils.GetSessionPlugins(pluginSettingComponent.pluginTypeJSSC.val);
                            else if (scenePluginsTarget) plugins = PluginUtils.GetScenePlugins(pluginSettingComponent.pluginTypeJSSC.val);
                            else plugins = PluginUtils.GetPluginsFromAtom(atom);

                            int activeBools = 0;
                            foreach (MVRScript script in plugins)
                            {
                                if (script.name.EndsWith(pluginSettingComponent.pluginTypeJSSC.val))
                                {
                                    if (script.GetBoolParamNames().Contains(pluginSettingComponent.targetParamNameOnJSSC.val))
                                    {
                                        if (!script.GetBoolParamValue(pluginSettingComponent.targetParamNameOnJSSC.val)) return ButtonState.inactive;
                                        else activeBools++;
                                    }
                                }
                            }
                            if (activeBools > 0) activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.decalMakerToggle)
                        {
                            if (decalMakerComponent.GetDecalMakerButtonState(atom) == ButtonState.inactive) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.mergeClothingPreset)
                        {
                            if (PatreonFeatures.GetMergeClothingButtonState(atom, fileReferenceDict[FileReferenceTypes.clothingPreset]) == ButtonState.inactive) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.undressClothingPreset)
                        {
                            if (PatreonFeatures.GetUndressClothingButtonState(atom, fileReferenceDict[FileReferenceTypes.clothingPreset]) == ButtonState.inactive) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.undressAllClothing)
                        {
                            if (clothingComponent.GetUndressAllClothingButtonState(atom) == ButtonState.inactive) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleNodePositionState || buttonOpTypeJSEnum.val == UIAButtonOpType.toggleNodeRotationState)
                        {
                            if (!nodeControlOnComponent.IsMatchingState(atom, buttonOpTypeJSEnum.val == UIAButtonOpType.toggleNodeRotationState)) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                        if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleNodePhysics)
                        {
                            if (!nodePhysicsOnComponent.IsMatchingState(atom, nodeSelectionComponent.GetActiveNodeSelections())) return ButtonState.inactive;
                            else activeAtomsCount++;
                        }
                    }
                }
                if (activeAtomsCount > 0) return ButtonState.active;
                else return ButtonState.inactive;
            }
            else
            {
                if (UIAConsts.debugLogging) SuperController.LogMessage("UIA.UIAButton.GetButtonState: Potentially unhandled Button type (" + buttonOpTypeJSEnum.displayVal + ") that is not multistate or does not have a target component");
                return ButtonState.inactive;
            }
        }

        public List<int> GetFileReferenceTypes()
        {
            List<int> fileRefTypes = new List<int>();

            foreach (int frt in UIAButtonOpType.GetFileReferenceTypes(buttonOpTypeJSEnum.val)) fileRefTypes.Add(frt);

            if (buttonOpTypeJSEnum.val == UIAButtonOpType.spawnAtom)
            {
                if (spawnAtomComponent.atomTypeJSEnum.val == AtomTypes.person)
                {
                    fileRefTypes.Remove(FileReferenceTypes.generalPreset);
                    fileRefTypes.Remove(FileReferenceTypes.subScene);
                }
                else if (spawnAtomComponent.atomTypeJSEnum.val == AtomTypes.subScene)
                {
                    fileRefTypes.Remove(FileReferenceTypes.generalPreset);
                    fileRefTypes.Remove(FileReferenceTypes.appearancePreset);
                    fileRefTypes.Remove(FileReferenceTypes.posePreset);
                    fileRefTypes.Remove(FileReferenceTypes.pluginsPreset);
                }
                else
                {
                    fileRefTypes.Remove(FileReferenceTypes.subScene);
                    fileRefTypes.Remove(FileReferenceTypes.appearancePreset);
                    fileRefTypes.Remove(FileReferenceTypes.posePreset);
                    fileRefTypes.Remove(FileReferenceTypes.pluginsPreset);
                }
            }

            if (buttonOpTypeJSEnum.val == UIAButtonOpType.loadPluginsPreset)
            {
                if (targetComponent.targetCategoryJSEnum.val == TargetCategory.scenePlugins || targetComponent.targetCategoryJSEnum.val == TargetCategory.sessionPlugins) fileRefTypes.Remove(FileReferenceTypes.pluginsPreset);
                if (targetComponent.targetCategoryJSEnum.val == TargetCategory.scenePlugins) fileRefTypes.Add(FileReferenceTypes.scenePluginPreset);
                if (targetComponent.targetCategoryJSEnum.val == TargetCategory.sessionPlugins) fileRefTypes.Add(FileReferenceTypes.sessionPluginPreset);
            }

            return fileRefTypes;
        }

        public void AppendThumbnailFileReferences(List<FileReference> fileRefList)
        {
            foreach (int fileRefType in GetFileReferenceTypes())
            {
                if (fileReferenceDict[fileRefType].IsThumbnailFileRefType()) fileRefList.Add(fileReferenceDict[fileRefType]);
            }
        }

        public bool IsComponentInButtonOpType(int componentType)
        {
            if (targetComponent.SpawnAtomIfTargetMissing())
            {
                if (componentType == ButtonComponentTypes.relativePositionComponent) return true;
            }
            return UIAButtonOpType.IsComponentInButtonType(buttonOpTypeJSEnum.val, componentType);
        }

        public List<int> GetComponentTypes()
        {
            List<int> componentTypes = new List<int> (UIAButtonOpType.GetComponentTypes(buttonOpTypeJSEnum.val));
          
            if (targetComponent.SpawnAtomIfTargetMissing()) componentTypes.Add(ButtonComponentTypes.relativePositionComponent);

            return componentTypes;
        }


        public void ButtonCategoryChanged(int categoryEnum)
        {
            buttonOpTypeJSEnum.SetEnumChoices(UIAButtonOpType.enumManifestName, UIAButtonOpType.GetButtonTypeExclusions(buttonOpCategoryJSEnum.val));
        }

        public void RefreshFileRefTypes()
        {
            foreach (int fileRefType in GetFileReferenceTypes())
            {
                if (!fileReferenceDict.ContainsKey(fileRefType)) fileReferenceDict.Add(fileRefType, new FileReference(fileRefType, this));
            }
        }

        protected void ButtonTypeChanged(int buttonType)
        {
            CreateButtonComponents();
            RefreshFileRefTypes();
            
            if (IsComponentInButtonOpType(ButtonComponentTypes.targetComponent)) targetComponent.ButtonTypeUpdated();
            if (IsComponentInButtonOpType(ButtonComponentTypes.relativePositionComponent)) relativePositionComponent.ButtonTypeUpdated();
            if (IsComponentInButtonOpType(ButtonComponentTypes.pluginSettingComponent)) pluginSettingComponent.ButtonTypeUpdated();

            if (buttonOpCategoryJSEnum.val != UIAButtonCategory.clothing && pluginSettingComponent!=null)
            {
                pluginSettingComponent.atomButtonToggleStates.Clear();
            }
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.activeClothingEditor) targetComponent.targetCategoryJSEnum.val = TargetCategory.gazeSelectedAtom;
            parentButton.UpdateThumbnailImage(parentButton.buttonTexture);
        }
        protected void SavePresetChanged(bool enabled)
        {
            var fileRefs = GetFileReferenceTypes();
            if (fileRefs.Count>0) fileReferenceDict[fileRefs[0]].UpdateFileSelectionModeExclusions();

        }

        public void AtomNameUpdate(string oldName, string newName)
        {

            foreach (FileReference fileRef in fileReferenceDict.Values.ToList())
            {
                fileRef.AtomNameUpdate(oldName, newName);
            }

            if (targetComponent!=null)targetComponent.AtomNameUpdate(oldName, newName);
            if (pluginSettingComponent != null) pluginSettingComponent.AtomNameUpdate(oldName, newName);

        }

        public void AtomRemovedUpdate(string oldName)
        {
            foreach (FileReference fileRef in fileReferenceDict.Values.ToList())
            {
                fileRef.AtomRemovedUpdate(oldName);
            }
            if (targetComponent != null) targetComponent.AtomRemovedUpdate(oldName);
            if (pluginSettingComponent != null) pluginSettingComponent.AtomRemovedUpdate(oldName);
        }

        public string LabelKeyWordsReplace(string rawLabel)
        {
            string label = rawLabel;

            string targetName;
            if (targetComponent.targetCategoryJSEnum.val == TargetCategory.atomGroup || targetComponent.targetCategoryJSEnum.val == TargetCategory.userChosenAtom) targetName = targetComponent.targetNameJSMultiEnum.displayVal;
            else if (targetComponent.targetCategoryJSEnum.val == TargetCategory.scenePlugins) targetName = "Scene Plugins";
            else if (targetComponent.targetCategoryJSEnum.val == TargetCategory.sessionPlugins) targetName = "Session Plugins";
            else
            {
                targetName = targetComponent.GetTargetAtomName();
                if (targetName == "") targetName = targetComponent.targetNameJSMultiEnum.displayVal;
            }
            if (IsComponentInButtonOpType(ButtonComponentTypes.targetComponent)) label = label.Replace("<TARGET>", targetName);

            if (IsComponentInButtonOpType(ButtonComponentTypes.spawnAtomComponent)) label = label.Replace("<ATOMTYPE>", spawnAtomComponent.atomTypeJSEnum.displayVal);

            if (IsComponentInButtonOpType(ButtonComponentTypes.relativePositionComponent))
            {
                string relPosAtomName = relativePositionComponent.GetRelativePositionAtomName();
                if (relPosAtomName == "") relPosAtomName = relativePositionComponent.atomPositionRelativeToJSEnum.displayVal;
                label = label.Replace("<RELATIVETARGET>", relPosAtomName);
            }

            List<int> fileRefTypes = GetFileReferenceTypes();
            if (fileRefTypes.Count > 0)
            {
                FileReference fileRef = fileReferenceDict[fileRefTypes.First()];
                string fileRefName = "<FILEREF1>";
                if (fileRef.fileSelectionModeJSEnum.val == FileSelectionMode.singleFile)
                {
                    string filePath = fileRef.filePathJSString.val;
                    if (filePath != "")
                    {
                        int filenameIndex = filePath.LastIndexOf('/');
                        if (filenameIndex == -1) fileRefName = filePath;
                        else
                        {
                            fileRefName = filePath.Substring(filenameIndex, filePath.Length - filenameIndex);
                            if (fileRefName.StartsWith("Preset_")) fileRefName = fileRefName.Substring(8);
                            if (fileRefName.Contains(".")) fileRefName = fileRefName.Substring(0, fileRefName.LastIndexOf('.'));
                            fileRefName = "'" + fileRefName + "'";
                        }
                    }
                }
                else fileRefName = "";

                label = label.Replace("<FILEREF1>", fileRefName);
            }
            if (IsComponentInButtonOpType(ButtonComponentTypes.pluginSettingComponent))
            {
                string pluginType = pluginSettingComponent.pluginTypeJSSC.val;
                if (pluginType == "<None available>") pluginType = "<PLUGINTYPE>";
                label = label.Replace("<PLUGINTYPE>", pluginType);

                string paramOn = pluginSettingComponent.targetParamNameOnJSSC.val;
                if (paramOn == "<None available>") paramOn = "<PARAMON>";
                label = label.Replace("<PARAMON>", paramOn);

                string paramOff = pluginSettingComponent.targetParamNameOffJSSC.val;
                if (paramOff == "<None available>") paramOff = "<PARAMOFF>";
                label = label.Replace("<PARAMOFF>", paramOff);
            }
            if (IsComponentInButtonOpType(ButtonComponentTypes.switchUIAGridComponent))
            {
                label = label.Replace("<UIASCREEN>", switchUIAGridComponent.switchUIAGridTargetJSI.val.ToString());
            }

            if (IsComponentInButtonOpType(ButtonComponentTypes.worldScaleComponent))
            {
                label = label.Replace("<WORLDSCALE>", worldScaleComponent.worldScaleAbsJSF.val.ToString());
            }

            return label;
        }

        public void TargetReset()
        {
            targetComponent.currentActionTargetAtomNames.Clear();
        }
        public void FileRefReset()
        {
            foreach (int fileRefType in GetFileReferenceTypes()) fileReferenceDict[fileRefType].Reset();
        }

        public bool CheckActionTarget()
        {
            if (!IsComponentInButtonOpType(ButtonComponentTypes.targetComponent)) return true;

            if (targetComponent.currentActionTargetAtomNames.Count > 0) return true;
            targetComponent.SetCurrentActionTargetAtoms();

            if (targetComponent.currentActionTargetAtomNames.Count > 0) return true;

            return false;
        }

        public bool CheckActionFileReferences()
        {
            foreach (int fileRefType in GetFileReferenceTypes())
            {
                
                FileReference fileRef = fileReferenceDict[fileRefType];

                if (fileRef.currentActionSelectedFile == "" && fileRef.fileSelectionModeJSEnum.val != FileSelectionMode.none)
                {
                    bool fileSelectionComplete = fileRef.GetNextFileSelection();
                    if (!fileSelectionComplete) return false;
                }
            }
            return true;
        }
        public void PlayAction(int buttonState)
        {
            if (!IsComponentInButtonOpType(ButtonComponentTypes.targetComponent))
            {
                if (GetFileReferenceTypes().Count == 0) PlayDiscreteAction();
                else PlayNoTargetFileRefAction();
            }
            else PlayTargetableAction(buttonState);
        }

        private void PlayNoTargetFileRefAction()
        {
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.loadScene) SuperController.singleton.Load(fileReferenceDict[FileReferenceTypes.scene].currentActionSelectedFile);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.mergeLoadScene) SuperController.singleton.LoadMerge(fileReferenceDict[FileReferenceTypes.scene].currentActionSelectedFile);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.loadUIAProfile) UIAPProfile.LoadUIAPSelected(fileReferenceDict[FileReferenceTypes.uiap].currentActionSelectedFile, fileReferenceDict[FileReferenceTypes.uiap].closeGridOnLoadUIAPJSB.val);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.spawnAtom) spawnAtomComponent.SpawnAtomAction();
            else if (UIAConsts.debugLogging) SuperController.LogMessage("UIA.UIAButton.PlayNoTargetFileRefAction: Unhandled button type '" + buttonOpTypeJSEnum.displayVal + "'");
        }

        private void PlayDiscreteAction()
        {
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.suppressScaleLoad) PresetLoadSettings.suppressScaleLoadJSB.val = !PresetLoadSettings.suppressScaleLoadJSB.val;
            if (buttonOpTypeJSEnum.val == UIAButtonOpType.suppressClothingLoad) PresetLoadSettings.suppressClothingLoadJSB.val = !PresetLoadSettings.suppressClothingLoadJSB.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.startPlayback) SuperController.singleton.motionAnimationMaster.StartPlayback();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.stopPlayback) SuperController.singleton.motionAnimationMaster.StopPlayback();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.resetAnimation) SuperController.singleton.motionAnimationMaster.ResetAnimation();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.beginRecordMode) SuperController.singleton.motionAnimationMaster.StartRecordMode();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.stopRecording) SuperController.singleton.motionAnimationMaster.StopRecordMode();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleLoopPlayback) SuperController.singleton.motionAnimationMaster.loop = !SuperController.singleton.motionAnimationMaster.loop;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.freezeMotionSound) SuperController.singleton.SetFreezeAnimation(!SuperController.singleton.freezeAnimation);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.togglePlayEdit)
            {
                if (SuperController.singleton.gameMode == SuperController.GameMode.Play)
                {
                    SuperController.singleton.gameMode = SuperController.GameMode.Edit;
                    if (vamPlayEditModeComponent.openGameUIonEditModeJSBool.val) SuperController.singleton.ShowMainHUDAuto();
                }
                else
                {
                    SuperController.singleton.gameMode = SuperController.GameMode.Play;
                    if (vamPlayEditModeComponent.closeGameUIonPlayModeJSBool.val) SuperController.singleton.HideMainHUD();
                }
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.showVRHands) showVRHandsComponent.ToggleVRHands();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.leapMotionToggle) showVRHandsComponent.ToggleLeapMotion();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.leapMotionEnable) showVRHandsComponent.SetLeapMotion();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.setWorldScale) SuperController.singleton.worldScale = worldScaleComponent.worldScaleAbsJSF.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.offsetWorldScale) SuperController.singleton.worldScale += worldScaleComponent.worldScaleIncrementJSF.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.softBodyPhysicsToggle) UserPreferences.singleton.softPhysics = !UserPreferences.singleton.softPhysics;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.desktopVSyncToggle) UserPreferences.singleton.desktopVsync = !UserPreferences.singleton.desktopVsync;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.realtimeReflectionProbesToggle) UserPreferences.singleton.realtimeReflectionProbes = !UserPreferences.singleton.realtimeReflectionProbes;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.mirrorReflectionsToggle) UserPreferences.singleton.mirrorReflections = !UserPreferences.singleton.mirrorReflections;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.hqPhysicsToggle) UserPreferences.singleton.physicsHighQuality = !UserPreferences.singleton.physicsHighQuality;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.vrHeadCollider) UserPreferences.singleton.useHeadCollider = !UserPreferences.singleton.useHeadCollider;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.vrHeadCollider) UserPreferences.singleton.useHeadCollider = !UserPreferences.singleton.useHeadCollider;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.shaderQaulity)
            {
                if (userPreferencesComponent.shaderQualityJSEnum.val == UserPrefShaderQuality.high) UserPreferences.singleton.shaderLOD = UserPreferences.ShaderLOD.High;
                else if (userPreferencesComponent.shaderQualityJSEnum.val == UserPrefShaderQuality.medium) UserPreferences.singleton.shaderLOD = UserPreferences.ShaderLOD.Medium;
                else if (userPreferencesComponent.shaderQualityJSEnum.val == UserPrefShaderQuality.low) UserPreferences.singleton.shaderLOD = UserPreferences.ShaderLOD.Low;
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.msaaLevel)
            {
                if (userPreferencesComponent.msaaLevelJSEnum.val == UserPrefMSAALevel.off) UserPreferences.singleton.msaaLevel = 0;
                else if (userPreferencesComponent.msaaLevelJSEnum.val == UserPrefMSAALevel.x2) UserPreferences.singleton.msaaLevel = 2;
                else if (userPreferencesComponent.msaaLevelJSEnum.val == UserPrefMSAALevel.x4) UserPreferences.singleton.msaaLevel = 4;
                else if (userPreferencesComponent.msaaLevelJSEnum.val == UserPrefMSAALevel.x8) UserPreferences.singleton.msaaLevel = 8;
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pixelLightCount)
            {
                if (userPreferencesComponent.pixelLightCountJSEnum.val == UserPrefPixelLightCount.zero) UserPreferences.singleton.pixelLightCount = 0;
                else if (userPreferencesComponent.pixelLightCountJSEnum.val == UserPrefPixelLightCount.one) UserPreferences.singleton.pixelLightCount = 1;
                else if (userPreferencesComponent.pixelLightCountJSEnum.val == UserPrefPixelLightCount.two) UserPreferences.singleton.pixelLightCount = 2;
                else if (userPreferencesComponent.pixelLightCountJSEnum.val == UserPrefPixelLightCount.three) UserPreferences.singleton.pixelLightCount = 3;
                else if (userPreferencesComponent.pixelLightCountJSEnum.val == UserPrefPixelLightCount.four) UserPreferences.singleton.pixelLightCount = 4;
                else if (userPreferencesComponent.pixelLightCountJSEnum.val == UserPrefPixelLightCount.five) UserPreferences.singleton.pixelLightCount = 5;
                else if (userPreferencesComponent.pixelLightCountJSEnum.val == UserPrefPixelLightCount.six) UserPreferences.singleton.pixelLightCount = 6;
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.smoothPasses)
            {
                if (userPreferencesComponent.smoothPassesJSEnum.val == UserPrefSmoothPasses.zero) UserPreferences.singleton.smoothPasses = 0;
                else if(userPreferencesComponent.smoothPassesJSEnum.val == UserPrefSmoothPasses.one) UserPreferences.singleton.smoothPasses = 1;
                else if (userPreferencesComponent.smoothPassesJSEnum.val == UserPrefSmoothPasses.two) UserPreferences.singleton.smoothPasses = 2;
                else if (userPreferencesComponent.smoothPassesJSEnum.val == UserPrefSmoothPasses.three) UserPreferences.singleton.smoothPasses = 3;
                else if (userPreferencesComponent.smoothPassesJSEnum.val == UserPrefSmoothPasses.four) UserPreferences.singleton.smoothPasses = 4;
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.glowEffects)
            {
                if (userPreferencesComponent.glowEffectsJSEnum.val == UserPrefGlowEffects.off) UserPreferences.singleton.glowEffects = UserPreferences.GlowEffectsLevel.Off;
                else if (userPreferencesComponent.glowEffectsJSEnum.val == UserPrefGlowEffects.low) UserPreferences.singleton.glowEffects = UserPreferences.GlowEffectsLevel.Low;
                else if (userPreferencesComponent.glowEffectsJSEnum.val == UserPrefGlowEffects.high) UserPreferences.singleton.glowEffects = UserPreferences.GlowEffectsLevel.High;

            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.physicsRate)
            {
                if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.auto) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate.Auto;
                else if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.hz45) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate._45;
                else if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.hz60) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate._60;

                else if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.hz72) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate._72;
                else if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.hz80) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate._80;
                else if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.hz90) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate._90;
                else if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.hz120) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate._120;
                else if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.hz144) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate._144;
                else if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.hz240) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate._240;
                else if (userPreferencesComponent.physicsRateJSEnum.val == UserPrefPhysicsRate.hz288) UserPreferences.singleton.physicsRate = UserPreferences.PhysicsRate._288;
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.physicsUpdateCap)
            {
                if (userPreferencesComponent.physicsUpdateCapJSEnum.val == UserPrefPhysicsUpdateCap.one) UserPreferences.singleton.physicsUpdateCap = 1;
                else if (userPreferencesComponent.physicsUpdateCapJSEnum.val == UserPrefPhysicsUpdateCap.two) UserPreferences.singleton.physicsUpdateCap = 2;
                else if (userPreferencesComponent.physicsUpdateCapJSEnum.val == UserPrefPhysicsUpdateCap.three) UserPreferences.singleton.physicsUpdateCap =3;

            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.softBodyPhysics)  UserPreferences.singleton.softPhysics = userPreferencesComponent.softbodyPhysicsJSB.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.desktopVSync) UserPreferences.singleton.desktopVsync = userPreferencesComponent.desktopVSyncJSB.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.realtimeReflectionProbes) UserPreferences.singleton.realtimeReflectionProbes = userPreferencesComponent.realtimeReflectionProbesJSB.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.mirrorReflections) UserPreferences.singleton.mirrorReflections = userPreferencesComponent.mirrorReflectionsJSB.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.hqPhysics) UserPreferences.singleton.physicsHighQuality = userPreferencesComponent.hqPhysicsJSB.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.renderScale) UserPreferences.singleton.renderScale = userPreferencesComponent.renderScaleJSF.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.perfMon)
            {
                PerfMon pm = UserPreferences.singleton.transform.Find("PerfMon").GetComponent<PerfMon>();
                pm.onToggle.isOn = !pm.onToggle.isOn;
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.showHiddenAtoms) SuperController.singleton.showHiddenAtoms = !SuperController.singleton.showHiddenAtoms;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.switchUIAGrid)
            {
                if (UIAStorables.buttonGridsList.Count >= switchUIAGridComponent.switchUIAGridTargetJSI.val && UIAStorables.buttonGridsList[switchUIAGridComponent.switchUIAGridTargetJSI.val - 1]._activeButtonCount > 0)
                {
                    GridsControlDisplay.SetCurrentGrid(switchUIAGridComponent.switchUIAGridTargetJSI.val - 1);
                }
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.gazeAssistedSelect)
            {
                if (GazeAssistedSelectTool.gazeAssistActiveJSB.val == false)
                {
                    GazeAssistedSelectTool.gazeAssistActiveJSB.val = true;
                    SuperController.singleton.gameMode = SuperController.GameMode.Edit;
                }
                else GazeAssistedSelectTool.gazeAssistActiveJSB.val = false;
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.suppressPresetLocks) PresetLoadSettings.suppressPresetLocksJSB.val = !PresetLoadSettings.suppressPresetLocksJSB.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.hideInactiveTargets) UserPreferences.singleton.hideInactiveTargets = !UserPreferences.singleton.hideInactiveTargets;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.raiseByHAHeight) HeelAdjustTool.heelAdjustRaisePeopleJSB.val = !HeelAdjustTool.heelAdjustRaisePeopleJSB.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.hideUGameControlUI) UIAGlobals.hideGameControlUI = true;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginAssistLoad && UIAPluginInterop._loadPluginsUIJSON != null) UIAPluginInterop._loadPluginsUIJSON.actionCallback();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginAssistFind && UIAPluginInterop._findPluginsUIJSON != null) UIAPluginInterop._findPluginsUIJSON.actionCallback();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.teleportPlayer) relativePositionComponent.TeleportPlayer();

            else if (UIAConsts.debugLogging) SuperController.LogMessage("UIA.UIAButton.PlayDiscreteAction: Unhandled button type '" + buttonOpTypeJSEnum.displayVal + "'");
        }

        private void PlayTargetableAction(int buttonState)
        {


            foreach (string atomName in targetComponent.currentActionTargetAtomNames)
            {
                Atom atom = SuperController.singleton.GetAtomByUid(atomName);
                if (buttonOpTypeJSEnum.val == UIAButtonOpType.loadGeneralPreset && targetComponent.SpawnAtomIfTargetMissing() && atom == null) spawnAtomComponent.SpawnAtomAction();
                else
                {
                    if (atomName == "CoreControl" && targetComponent.targetCategoryJSEnum.val == TargetCategory.sessionPlugins) atom = UIAGlobals.mvrScript.containingAtom;
                    if (atom != null) PlayActionTargetAtom(atom, buttonState);
                }
            }

            

        }

        private void PlayActionTargetAtom(Atom atom, int buttonState)
        {
            bool revertAtomOn = false;
            if (!atom.on && buttonOpTypeJSEnum.val != UIAButtonOpType.toggleAtomOn)
            {
                revertAtomOn = true;
                atom.ToggleOn();
            }

            if (buttonOpCategoryJSEnum.val == UIAButtonCategory.legacyPresets) appearancePresetComponent.PlayLegacyPresetAction(atom);
            else if (buttonOpCategoryJSEnum.val == UIAButtonCategory.presets) appearancePresetComponent.PlayPresetAction(atom);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.activeClothingEditor && PatreonFeatures.patreonContentEnabled)
            {
                if (GameControlUI.gameControlDisplayMode != GameControlDisplayModes.ace && GameControlUI.gameControlDisplayMode != GameControlDisplayModes.clothItemPresetSelect)
                {
                    GameControlUI.gameControlDisplayMode = GameControlDisplayModes.ace;
                    var ace = GridsDisplay._uiActiveClothingEditor;
                    ace.aceMode = targetComponent.aceModeJSEnum.val;
                    if (targetComponent.aceModeJSEnum.val == ACEMode.original)
                    {
                        ace.isGazeSelectedMode = true;
                        if (targetComponent.targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom)
                            ace.gazeSelectedTargetType = targetComponent.targetNameJSMultiEnum.valTopEnum;
                        else ace.gazeSelectedTargetType = LastViewedTargetType.lastViewedPerson;
                    }
                    else
                    {
                        ace.isGazeSelectedMode = false;
                        if (targetComponent.targetCategoryJSEnum.val == TargetCategory.gazeSelectedAtom)
                        {
                            ace.isGazeSelectedMode = true;
                            ace.gazeSelectedTargetType = targetComponent.targetNameJSMultiEnum.valTopEnum;
                        }
                        else
                        {
                            ace.RefreshPersonAtomNames();
                            if (ace.personAtomNamesJSSC.choices.Contains(atom.name)) ace.personAtomNamesJSSC.val = atom.name;
                        }
                    }
                }
                else GameControlUI.gameControlDisplayMode = GameControlDisplayModes.standard;
                GameControlUI.RefreshWristUIButtonGrid();
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleAtomOn) atom.ToggleOn();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.hideAtom) atom.hidden = !atom.hidden;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.deleteAtom) atom.Remove();
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleAtomCollision) atom.collisionEnabledJSON.val = !atom.collisionEnabledJSON.val;
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.detachAtomRoot)
            {
                JSONStorableBool detach = atom.GetStorableByID("control").GetBoolJSONParam("detachControl");
                if (detach != null) detach.val = !detach.val;
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.selectAtom)
            {
                FreeControllerV3 fcV3 = atom.freeControllers.First(fc => fc.name == "control");
                SuperController.singleton.SelectController(fcV3);
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.togglePresetLocks) presetLockComponent.ActionTogglePresetLock(atom, buttonState);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.changeAtomName) targetComponent.ChangeTargetAtomNameAction(atom);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.teleportAtom) relativePositionComponent.TeleportAtomToSpawnPoint(atom.name);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.moveAtom) moveAtomComponent.PlayMoveAtomAction(atom, vrHandActionInit, leapActivatedAction);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.loadSubScene && atom.type == "SubScene" && fileReferenceDict[FileReferenceTypes.subScene].currentActionSelectedFile != "")
            {
                JSONStorable js = atom.GetStorableByID("SubScene");
                JSONStorableUrl subScenePathJSON = js.GetUrlJSONParam("browsePath");
                string atomName = atom.uid;
                if (subScenePathJSON.val != SuperController.singleton.NormalizePath(fileReferenceDict[FileReferenceTypes.subScene].currentActionSelectedFile)) subScenePathJSON.val = SuperController.singleton.NormalizePath(fileReferenceDict[FileReferenceTypes.subScene].currentActionSelectedFile);
                else js.CallAction("LoadSubScene");
                atom.SetUID(atomName);
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.loadPlugins) pluginsLoadComponent.LoadPluginsAsPreset(atom);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.unLoadPlugins) pluginsLoadComponent.UnloadPlugins(atom);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.decalMakerToggle) decalMakerComponent.DecalMakerToggle(atom);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.setHairColor) hairColorComponent.SetHairColor(atom);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.resetAppearance) appearancePresetComponent.ResetAppearance(atom, true);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.resetScale) appearancePresetComponent.ResetAppearance(atom, false);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginAction) pluginSettingComponent.PluginAction(atom, true, false);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginActionToggle) pluginSettingComponent.PluginAction(atom, true, true);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginMultiSettings) pluginSettingComponent.PluginAction(atom, false, false);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginMultiSettingsToggle) pluginSettingComponent.PluginAction(atom, false, true);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginBoolToggle) pluginSettingComponent.PluginBool(atom, buttonState == ButtonState.inactive,true);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginBoolTrue) pluginSettingComponent.PluginBool(atom, true,false);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginBoolFalse) pluginSettingComponent.PluginBool(atom, false,false);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginOpenUI) pluginSettingComponent.PluginOpenUI(atom);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.pluginToggleOpenUI) pluginSettingComponent.PluginOpenUI(atom);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.loadMotionCapture)
            {
                motionCaptureComponent.LoadMocapFromSceneToAtom(atom, fileReferenceDict[FileReferenceTypes.scene].currentActionSelectedFile);
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.loadELMotionCapture)
            {
                motionCaptureComponent.LoadMocapFromMocapFile(atom, fileReferenceDict[FileReferenceTypes.mocap].currentActionSelectedFile);
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.removeAllClothing) ClothingComponent.ClothingActions(atom, ClothingActionMode.remove, false, clothingComponent.onlyRemoveRealClothingJSB.val);

            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.undressAllClothing) ClothingComponent.ClothingActions(atom, ClothingActionMode.undress, buttonState == ButtonState.active);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.resetSimAllClothing) ClothingComponent.ClothingActions(atom, ClothingActionMode.resetSim, false);

            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.removeClothingPreset) clothingComponent.ClothingPresetActions(atom, ClothingActionMode.remove, false, fileReferenceDict[FileReferenceTypes.clothingPreset], targetComponent);

            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.mergeClothingPreset) clothingComponent.ClothingPresetActions(atom, ClothingActionMode.merge, buttonState == ButtonState.active, fileReferenceDict[FileReferenceTypes.clothingPreset], targetComponent);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.undressClothingPreset) clothingComponent.ClothingPresetActions(atom, ClothingActionMode.undress, buttonState == ButtonState.active, fileReferenceDict[FileReferenceTypes.clothingPreset], targetComponent);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.resetSimClothingPreset) clothingComponent.ClothingPresetActions(atom, ClothingActionMode.resetSim, false, fileReferenceDict[FileReferenceTypes.clothingPreset], targetComponent);
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleNodePositionState || buttonOpTypeJSEnum.val == UIAButtonOpType.toggleNodeRotationState || buttonOpTypeJSEnum.val == UIAButtonOpType.nodeRotationState || buttonOpTypeJSEnum.val == UIAButtonOpType.nodePositionState)
            {
                if (buttonState == ButtonState.active) nodeControlOffComponent.SetControlStateAction(atom, buttonOpTypeJSEnum.val == UIAButtonOpType.toggleNodeRotationState );
                else nodeControlOnComponent.SetControlStateAction(atom, buttonOpTypeJSEnum.val == UIAButtonOpType.toggleNodeRotationState || buttonOpTypeJSEnum.val == UIAButtonOpType.nodeRotationState);
            }
            else if (buttonOpTypeJSEnum.val == UIAButtonOpType.toggleNodePhysics ||buttonOpTypeJSEnum.val == UIAButtonOpType.nodePhysics)
            {
                if (buttonState == ButtonState.active) nodePhysicsOffComponent.SetNodePhysicsAction(atom, nodeSelectionComponent.GetActiveNodeSelections());
                else nodePhysicsOnComponent.SetNodePhysicsAction(atom, nodeSelectionComponent.GetActiveNodeSelections());
            }
            if (revertAtomOn) atom.ToggleOn();
            if (buttonState==ButtonState.active) targetComponent.ResetLastUserChosenAtom();
        }

    }
}
