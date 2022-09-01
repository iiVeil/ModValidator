using HarmonyLib;
using BepInEx;
using UnityEngine;
using PacketHelper;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ModValidator.Patches
{
    [HarmonyPatch]
    class ModValidator
    {
        public static List<int> recieved = new List<int> { };
        public static Session session = new Session(Main.Id);
        public static bool sentMods = false;
        private static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
        static bool kickIfMissingValidator = false;

        [HarmonyPatch(typeof(Server), "InitializeServerPackets")]
        [HarmonyPostfix]
        static void CreateNewServerPackets()
        {
            session.CreateNewServerPacket("ServerHandleModCompat", ServerHandleModCompat);
        }


        [HarmonyPatch(typeof(ClientHandle), "SpawnPlayer")]
        [HarmonyPrefix]
        static void OnPlayerSpawn(ClientHandle __instance, Packet packet)
        {
            if (!sentMods && !LocalClient.serverOwner)
            {
                PluginInfo[] plugins = BepInEx.Bootstrap.Chainloader.PluginInfos.Values.ToArray();
                List<string> mods = new List<string>() { };
                Data data = new Data();
                foreach (PluginInfo plugin in plugins)
                {
                    mods.Add(plugin.Metadata.Name);
                }
                data.strings = mods.ToArray();
                Debug.Log("Sending mods to server.");
                session.SendPacketToServer("ServerHandleModCompat", data);
                sentMods = !sentMods;

            }
        }

        [HarmonyPatch(typeof(GameManager), "HostLeftGame")]
        [HarmonyPostfix]
        static void OnHostLeave(GameManager __instance)
        {
            recieved = new List<int> { };
        }

        [HarmonyPatch(typeof(Client), "Disconnect")]
        [HarmonyPostfix]
        static void OnClientDisconnect(Client __instance)
        {
            sentMods = false;
        }

        [HarmonyPatch(typeof(ServerHandle), "PlayerPosition")]
        [HarmonyPostfix]
        static void CheckPlayerHasModValidator(int fromClient, Packet packet)
        {
            if (fromClient != 0 && !recieved.Contains(fromClient))
            {
                if (kickIfMissingValidator)
                {
                    Debug.Log($"{Server.clients[fromClient].player.username} has been kicked. Missing ModValidator.");
                    Server.clients[fromClient].Disconnect();
                }
            }
        }

        static void ServerHandleModCompat(int fromClient, Packet packet)
        {
            recieved.Add(fromClient);
            List<string> mods = new List<string> { };
            while (true)
            {
                try
                {
                    mods.Add(packet.ReadString());
                }
                catch (Exception e)
                {
                    break;
                }
            }


            string[] blacklisted = new string[] { };
            string[] required = new string[] { };
            string[] whitelisted = new string[] { };
            XmlDocument doc = new XmlDocument();
            doc.Load(Paths.ConfigPath + Main.configPath + "config1.1.xml");
            for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
            {
                switch (i)
                {
                    case 1:
                        whitelisted = doc.DocumentElement.ChildNodes[i].InnerText.Split(',');
                        break;
                    case 2:
                        blacklisted = doc.DocumentElement.ChildNodes[i].InnerText.Split(',');
                        break;
                    case 3:
                        required = doc.DocumentElement.ChildNodes[i].InnerText.Split(',');
                        break;
                    case 4:
                        kickIfMissingValidator = Boolean.Parse(doc.DocumentElement.ChildNodes[i].InnerText);
                        break;
                }
            }
            int foundRequired = 0;
            bool hasRequiredMods = false;
            bool playerExists = true;

            bool hasBlacklisted = false;
            foreach (string mod in required)
            {
                if (mod != "")
                {
                    if (mods.Contains(mod))
                    { 
                        foundRequired++;
                    }
                }
            }

            foreach (string mod in blacklisted)
            {
                if (mod != "")
                {
                    if (mods.Contains(mod))
                    {
                        hasBlacklisted = true;
                    }
                }
            }

            if (foundRequired == required.Length)
            {

                hasRequiredMods = true;
            }

            List<int> except = new List<int> { };

            foreach (KeyValuePair<int, Client> client in Server.clients)
            {
                if (fromClient != client.Key)
                {
                    except.Add(client.Key);
                }
            }


            if (!Boolean.Parse(doc.DocumentElement.ChildNodes[0].InnerText))
            {
                if (!hasRequiredMods || hasBlacklisted)
                {
                    Debug.Log($"{Server.clients[fromClient].player.username} has been kicked. Missing a mod or has a blacklisted mod.");
                    Server.clients[fromClient].Disconnect();
                    playerExists = !playerExists;
                }
            } else
            {
                if (playerExists)
                {
                    foreach (string mod in whitelisted)
                    {
                        if (!mods.Contains(mod) && mod != "ModValidator")
                        {
                            Debug.Log($"{Server.clients[fromClient].player.username} has been kicked. Missing a required whitelisted mod. {mod}");
                            Server.clients[fromClient].Disconnect();
                            playerExists = !playerExists;
                        }
                    }
                }
                if (playerExists)
                {
                    foreach (string mod in mods)
                    {
                        if (!whitelisted.ToList<string>().Contains(mod) && mod!= "ModValidator")
                        {
                            Debug.Log($"{Server.clients[fromClient].player.username} has been kicked. Has a mod not in the whitelist. {mod}");
                            Server.clients[fromClient].Disconnect();
                            playerExists = !playerExists;
                        }
                    }
                }
            }
        }
    }
}
