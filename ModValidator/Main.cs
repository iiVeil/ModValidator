using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Xml;

namespace ModValidator
{
    [BepInPlugin(Id, "ModValidator", Version)]
    public class Main : BaseUnityPlugin
    {
        #region[Declarations]

        public const string
            MODNAME = "$safeprojectname$",
            AUTHOR = "",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0.0";

        internal readonly ManualLogSource log;
        internal readonly Harmony harmony;
        internal readonly Assembly assembly;
        public readonly string modFolder;

        #endregion


        public const string Id = "utility.iiveil.ModValidator";
        public const string Version = "1.1.0";
        public const string Name = "ModValidator";
        public const string configPath = "/ModValidator/";
        public Main()
        {
            
            log = Logger;
            harmony = new Harmony(Id);
            assembly = Assembly.GetExecutingAssembly();
            modFolder = Path.GetDirectoryName(assembly.Location);
        }

        public void Start()
        {
            harmony.PatchAll(assembly);
            Directory.CreateDirectory(Paths.ConfigPath + configPath);
            if (!File.Exists(Paths.ConfigPath + configPath + "config1.1.xml"))
            {
                using (FileStream fileStream = new FileStream(Paths.ConfigPath + configPath + "config1.1.xml", FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fileStream))
                using (XmlTextWriter config = new XmlTextWriter(sw))
                {
                    config.Formatting = Formatting.Indented;
                    config.Indentation = 4;
                    config.WriteStartElement("configuration");
                    config.WriteElementString("whitelist", "true");
                    config.WriteElementString("whitelistedMods", " ");
                    config.WriteElementString("blacklistedMods", " ");
                    config.WriteElementString("requiredMods", " ");
                    config.WriteElementString("kickIfMissingValidator", "true");
                    config.WriteEndElement();
                    config.Flush();
                }
            }
            Debug.Log("Below is a names of the mods to be used in the ModValidator config.");
            foreach (PluginInfo mod in BepInEx.Bootstrap.Chainloader.PluginInfos.Values.ToArray())
            {
                Debug.Log(mod.Metadata.Name);
            }
        }
    }
}
