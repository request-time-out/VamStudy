using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using MVR.FileManagementSecure;
    

namespace JayJayWon
{
    public static class PluginUtils
    {
        private static int GetVarTypeFromPluginStorable(string varName, JSONStorable pluginStorable)
        {
            int varType = PluginVariableType.vNone;

            if (pluginStorable.GetBoolParamNames().Contains(varName)) varType = PluginVariableType.vBool;
            else if (pluginStorable.GetFloatParamNames().Contains(varName)) varType = PluginVariableType.vFloat;
            else if (pluginStorable.GetStringParamNames().Contains(varName)) varType = PluginVariableType.vString;
            else if (pluginStorable.GetStringChooserParamNames().Contains(varName)) varType = PluginVariableType.vStringChooser;
            else if (pluginStorable.GetUrlParamNames().Contains(varName)) varType = PluginVariableType.vURL;
            else if (pluginStorable.GetColorParamNames().Contains(varName)) varType = PluginVariableType.vColor;

            return varType;
        }
        public static int GetVarTypeFromPlugin(string varName, string pluginTypeName)
        {
            int varType = PluginVariableType.vNone;
            foreach (MVRScript script in GetSceneAndSessionPlugins(pluginTypeName))
            {
                varType = GetVarTypeFromPluginStorable(varName, script);
                if (varType != PluginVariableType.vNone) return varType;
            }
            return (varType);
        }

        // This fuction assumes that the plugin is a Session plugin
        public static List<MVRScript> GetSessionPlugins(string pluginTypeFilter = null)
        {
            List<MVRScript> pluginList = new List<MVRScript>();
            foreach (Transform transform in UIAGlobals.mvrScript.manager.gameObject.transform.Find("Plugins"))
            {
                MVRScript script = transform.gameObject.GetComponent<MVRScript>();
                if (script != null && (pluginTypeFilter==null || script.name.EndsWith(pluginTypeFilter))) pluginList.Add(script);
            }

            return (pluginList);
        }

        public static List<MVRScript> GetScenePlugins(string pluginTypeFilter = null)
        {
            List<MVRScript> pluginList = new List<MVRScript>();

            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                MVRPluginManager manager = atom.GetStorableByID("PluginManager") as MVRPluginManager;
                if (manager != null)
                {
                    foreach (Transform transform in manager.gameObject.transform.Find("Plugins"))
                    {
                        MVRScript pluginStorable = transform.gameObject.GetComponent<MVRScript>();
                        if (pluginStorable != null && (pluginTypeFilter == null || pluginStorable.name.EndsWith(pluginTypeFilter))) pluginList.Add(pluginStorable);
                    }
                }
            }

            return (pluginList);
        }
        public static List<MVRScript> GetSceneAndSessionPlugins(string pluginTypeFilter = null)
        {
            return (GetSessionPlugins(pluginTypeFilter).Concat(GetScenePlugins(pluginTypeFilter)).ToList());
        }

        public static List<MVRScript> GetPluginsFromAtom(Atom atom, string pluginTypeFilter = null)
        {
            List<MVRScript> plugins = new List<MVRScript>();

            MVRPluginManager manager = atom.GetStorableByID("PluginManager") as MVRPluginManager;
            if (manager != null)
            {
                foreach (Transform transform in manager.gameObject.transform.Find("Plugins"))
                {
                    MVRScript pluginStorable = transform.gameObject.GetComponent<MVRScript>();
                    if (pluginStorable != null && (pluginTypeFilter == null || pluginStorable.name.EndsWith(pluginTypeFilter))) plugins.Add(pluginStorable);
                }
            }

            return plugins;
        }

        public static JSONArray GetPluginDataByName(string atomName, string pluginID)
        {
            JSONArray pluginData = new JSONArray();

            if (atomName == "SESSION")
            {
                foreach (MVRScript script in PluginUtils.GetSessionPlugins())
                {
                    if (script.name.StartsWith(pluginID))
                    {
                        JSONClass storableJSON = script.GetJSON();
                        pluginData[-1] = storableJSON;
                    }
                }
            }
            else
            {
                if (atomName == "SCENE") atomName = "CoreControl";
                Atom atom = SuperController.singleton.GetAtomByUid(atomName);
                if (atom != null)
                {
                    foreach (MVRScript pluginStorable in GetPluginsFromAtom(atom))
                    {
                        if (pluginStorable.name.StartsWith(pluginID))
                        {
                            JSONClass storableJSON = pluginStorable.GetJSON();
                            pluginData[-1] = storableJSON;
                        }
                    }
                }
            }
            return (pluginData);
        }

        public static List<string> GetPluginRefsOfType (MVRPluginManager manager, string pluginPath)
        {
            List<string> pluginRefs = new List<string>();
            if (manager != null)
            {
                JSONClass currentPlugins = manager.GetJSON(true, true, true);
                if (currentPlugins != null)
                {
                    foreach (KeyValuePair<string, JSONNode> kvp in (JSONClass)currentPlugins["plugins"])
                    {
                        if (kvp.Value.ToString().Substring(kvp.Value.ToString().LastIndexOfAny(new char[] { '/', '\\' }) + 1) == pluginPath.Substring(pluginPath.LastIndexOfAny(new char[] { '/', '\\' }) + 1) + "\"") pluginRefs.Add(kvp.Key);
                    }
                }
            }
            return (pluginRefs);
        }
        public static List<string> GetPluginRefsOfType(Atom atom, string pluginPath)
        {
            MVRPluginManager manager = atom.GetStorableByID("PluginManager") as MVRPluginManager;

            return GetPluginRefsOfType(manager, pluginPath);
        }

        public static List<string> GetSessionPluginRefsOfType(string pluginPath)
        {

            return GetPluginRefsOfType(UIAGlobals.mvrScript.manager, pluginPath);
        }

        public static bool IsPluginLoaded(JSONClass pluginMgrJSON, string pluginFileName)
        {
            bool loaded = false;
            for (int index = 0; index < pluginMgrJSON["plugins"].Count; ++index)
            {
                if (pluginMgrJSON["plugins"][index] == null) { break; }
                else if (FileUtils.GetLatestVARPath(pluginMgrJSON["plugins"][index].Value) == FileUtils.GetLatestVARPath(pluginFileName))
                {
                    loaded = true;
                    break;
                }
            }
            return (loaded);
        }
    }

    public static class AtomUtils
    {
        public static List<Atom> GetAtoms(bool includeOffAtoms = false, bool includeCoreControl = false, bool includeCameraRig = false)
        {
            List<Atom> atomsList = new List<Atom>();
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                if ((atom.on && (atom.containingSubScene==null ||atom.containingSubScene.containingAtom.on)) || includeOffAtoms)
                {
                    if (atom.name == "CoreControl")
                    {
                        if (includeCoreControl) atomsList.Add(atom);
                    }
                    else if (atom.name == "[CameraRig]")
                    {
                        if (includeCameraRig) atomsList.Add(atom);
                    }
                    else atomsList.Add(atom);
                }
            }

            return (atomsList);
        }

        public static List<Atom> GetPersonAtoms (int gender = GenderTypes.any, bool includeOffAtoms=false)
        {
            List<Atom> atomsList = new List<Atom>();
            foreach (Atom atom in SuperController.singleton.GetAtoms())
            {
                bool isMale = IsMale(atom);
                bool isFemale = IsFemale(atom);
                if (atom.on || includeOffAtoms)
                {
                    if (isMale && gender != GenderTypes.femaleOnly) atomsList.Add(atom);
                    if (isFemale && gender != GenderTypes.maleOnly) atomsList.Add(atom);
                }          

            }
            return atomsList;
        }

        public static bool IsMale(Atom atom)
        {
            bool isMale = false;
            // If the peson atom is not "On", then we cant determine their gender it seems as GetComponentInChildren<DAZCharacter> just returns null
            if (atom.on && (atom.containingSubScene == null || atom.containingSubScene.containingAtom.on) && atom.type == "Person") isMale = atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("male") ||
                    atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("jarlee") ||
                    atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("jarjulian") ||
                    atom.GetComponentInChildren<DAZCharacter>().name.StartsWith("Futa");
            return (isMale);
        }

        public static bool IsFemale(Atom atom)
        {
            bool isFemale = false;
            // If the peson atom is not "On", then we cant determine their gender it seems as GetComponentInChildren<DAZCharacter> just returns null
            if (atom.on && (atom.containingSubScene == null || atom.containingSubScene.containingAtom.on) && atom.type == "Person" && !IsMale(atom)) isFemale = true;
            return (isFemale);
        }

        public static bool IsSelectableFreeController(string contollerName, bool isMale = false)
        {
            bool result = true;
            if (contollerName.StartsWith("hair")) result = false;
            else if (!isMale && contollerName.StartsWith("penis")) result = false;
            else if (!isMale && contollerName == "testesControl") result = false;

            return (result);
        }

        public static void DisableHidePersonNodes()
        {
            SuperController.singleton.SetOnlyShowControllers(null);
        }

        public static string GetNextAvailableAtomName(string preferredAtomName)
        {
            List<Atom> atoms = GetAtoms(true, true, true);
            List<string> atomNames = new List<string>();
            foreach (Atom tempAtom in atoms) atomNames.Add(tempAtom.uid);

            if (atomNames.Contains(preferredAtomName))
            {
                for (int i = 2; i < 10000; i++)
                {
                    string newAtomName = preferredAtomName + "#" + i.ToString();
                    if (!atomNames.Contains(newAtomName)) return newAtomName;
                }
            }
            else return preferredAtomName;
            return "";
        }
    }

    public static class ImageUtils
    {
        public static void QueueLoadTexture(string url, ImageLoaderThreaded.ImageLoaderCallback callback, bool immediate=false)
        {
            if (ImageLoaderThreaded.singleton == null)
                return;

            ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
            queuedImage.imgPath = url;
            queuedImage.forceReload = true;
            queuedImage.skipCache = true;
            queuedImage.compress = true;
            queuedImage.createMipMaps = false;
            queuedImage.isNormalMap = false;
            queuedImage.linear = false;
            queuedImage.createAlphaFromGrayscale = false;
            queuedImage.createNormalFromBump = false;
            queuedImage.bumpStrength = 0f;
            queuedImage.isThumbnail = false;
            queuedImage.fillBackground = false;
            queuedImage.invert = false;
            queuedImage.callback = callback;

            Texture2D tex = ImageLoaderThreaded.singleton.GetCachedThumbnail(url);
            if (tex != null)
            {
                queuedImage.tex = tex;
                callback.Invoke(queuedImage);
            }
            else
            {
                if (immediate) ImageLoaderThreaded.singleton.QueueThumbnailImmediate(queuedImage);
                else ImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
            }
        }
    }

    public static class FileUtils
    {
        static public HashSet<string> GetFilesAtPathRecursive(string path, string pattern)
        {

            string[] files = FileManagerSecure.GetFiles(path, pattern);
            string[] directories = FileManagerSecure.GetDirectories(path);

            HashSet<string> combined = new HashSet<string>(files);

            directories.ToList().ForEach(directory =>
            {
                combined.UnionWith(GetFilesAtPathRecursive(directory, pattern));
            });
            return combined;
        }

        public static string GetLatestVARPath(string path)
        {
            if (!path.Contains(".latest:") && path.Contains(":") && path.IndexOf(':') > 1)
            {
                string varPackageName = path.Substring(0, path.IndexOf(':'));
                string varFileName = path.Substring(varPackageName.Length + 1);
                path = varPackageName.Substring(0, varPackageName.LastIndexOf('.')) + ".latest:" + varFileName;
            }
            path = SuperController.singleton.NormalizeLoadPath(path);

            return (path);
        }

        public static bool CreatePluginDataFolder()
        {
            bool result = false;
            if (FileManagerSecure.DirectoryExists("Saves\\PluginData"))
            {
                if (!FileManagerSecure.DirectoryExists(UIAConsts._PluginDataSubfolderName)) FileManagerSecure.CreateDirectory(UIAConsts._PluginDataSubfolderName);
                if (FileManagerSecure.DirectoryExists(UIAConsts._PluginDataSubfolderName)) result = true;
                if (!FileManagerSecure.DirectoryExists(UIAConsts._PluginDataSubfolderName + "\\" + UIAConsts._HeelAdjustSubfolderName))
                {
                    FileManagerSecure.CreateDirectory(UIAConsts._PluginDataSubfolderName + "\\" + UIAConsts._HeelAdjustSubfolderName);
                    if (FileManagerSecure.DirectoryExists(SuperController.singleton.savesDir + UIAConsts._HeelAdjustSubfolderName))
                    {
                        foreach (string file in SuperController.singleton.GetFilesAtPath(SuperController.singleton.savesDir + UIAConsts._HeelAdjustSubfolderName))
                        {
                            string fileName = FileManagerSecure.GetFileName(file);
                            FileManagerSecure.CopyFile(file, UIAConsts._PluginDataSubfolderName + "\\" + UIAConsts._HeelAdjustSubfolderName + "\\" + fileName);
                        }
                    }
                }
            }
            else SuperController.LogMessage("UIAssist: this function requires the folder Saves\\PluginData to exist.");
            return (result);
        }

        public static string GetLocalUIAPPath()
        {
            CreatePluginDataFolder();
            return (UIAConsts._PluginDataSubfolderName);
        }

        public static List<ShortCut> GetUIAPShortcutFolders()
        {
            List<ShortCut> shortcuts = FileManagerSecure.GetShortCutsForDirectory(UIAConsts.LegacyPluginDataSubfolderName, false, false, true, true);
            shortcuts.AddRange(FileManagerSecure.GetShortCutsForDirectory(UIAConsts._PluginDataSubfolderName, false, false, true, true));
            return (shortcuts);
        }

        public static string NormalizeAndRemoveVARPath(string filePath)
        {
            if (filePath != "")
            {
                filePath = FileManagerSecure.NormalizePath(filePath);
                int index = filePath.IndexOf(".var:/");
                if (index > -1)
                {
                    string varFilePath = filePath.Substring(0, index);
                    int folderIndex = varFilePath.LastIndexOf('/')+1;

                    return(filePath.Substring(folderIndex,index-folderIndex)+filePath.Substring(index+4));
                }
            }
            return filePath;
        }

        
    }

    public static class GeometryUtils
    {
        public static float DistancePointToRectangle(Vector2 point, Rect rect)
        {
            //  Calculate a distance between a point and a rectangle.
            //  The area around/in the rectangle is defined in terms of
            //  several regions:
            //
            //  O--x
            //  |
            //  y
            //
            //
            //        I   |    II    |  III
            //      ======+==========+======   --yMin
            //       VIII |  IX (in) |  IV
            //      ======+==========+======   --yMax
            //       VII  |    VI    |   V
            //
            //
            //  Note that the +y direction is down because of Unity's GUI coordinates.

            if (point.x < rect.xMin)
            { // Region I, VIII, or VII
                if (point.y < rect.yMin)
                { // I
                    Vector2 diff = point - new Vector2(rect.xMin, rect.yMin);
                    return diff.magnitude;
                }
                else if (point.y > rect.yMax)
                { // VII
                    Vector2 diff = point - new Vector2(rect.xMin, rect.yMax);
                    return diff.magnitude;
                }
                else
                { // VIII
                    return rect.xMin - point.x;
                }
            }
            else if (point.x > rect.xMax)
            { // Region III, IV, or V
                if (point.y < rect.yMin)
                { // III
                    Vector2 diff = point - new Vector2(rect.xMax, rect.yMin);
                    return diff.magnitude;
                }
                else if (point.y > rect.yMax)
                { // V
                    Vector2 diff = point - new Vector2(rect.xMax, rect.yMax);
                    return diff.magnitude;
                }
                else
                { // IV
                    return point.x - rect.xMax;
                }
            }
            else
            { // Region II, IX, or VI
                if (point.y < rect.yMin)
                { // II
                    return rect.yMin - point.y;
                }
                else if (point.y > rect.yMax)
                { // VI
                    return point.y - rect.yMax;
                }
                else
                { // IX
                    return 0f;
                }
            }
        }

        public static float GetButtonDistance(UIDynamicButton button, Ray ray)
        {
            float distance = 1000f;
            Vector3[] rectCornersArray = new Vector3[4];

            RectTransform rt = button.transform.GetComponent<RectTransform>();
            rt.GetWorldCorners(rectCornersArray);
            Vector3 buttonCentre = rectCornersArray[0] + ((rectCornersArray[2] - rectCornersArray[0]) * 0.5f);
            distance = Vector3.Cross(ray.direction, buttonCentre - ray.origin).magnitude;
            return (distance);
        }
        public static float GetToggleDistance(UIDynamicToggle toggle, Ray ray)
        {
            float distance = 1000f;
            Vector3[] rectCornersArray = new Vector3[4];

            RectTransform checkBoxRT = toggle.transform.GetComponent<RectTransform>();

            checkBoxRT.GetWorldCorners(rectCornersArray);
            Vector3 buttonCentre = rectCornersArray[0] + ((rectCornersArray[2] - rectCornersArray[0]) * 0.5f);
            distance = Vector3.Cross(ray.direction, buttonCentre - ray.origin).magnitude;

            return (distance);
        }
    }
}
