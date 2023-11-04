using HarmonyLib;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ThreadsafeBlock
{
    public class Main : RocketPlugin<Config>
    {
        public static Main Instance { get; private set; }

        private static Harmony harmony;
        private static string harmonyId = "Jdance.ThreadsafeBlock";

        public static Config Config { get => Instance.Configuration.Instance; }

        public delegate void OnLoadHandler();
        public OnLoadHandler OnLoad;

        public delegate void OnUnloadHandler();
        public OnUnloadHandler OnUnload;

        protected override void Load()
        {
            Instance = this;
            Logger.Log($"ThreadsafeBlock {Assembly.GetName().Version} is now loaded.");
            try
            {
                harmony = new Harmony(harmonyId);
                harmony.PatchAll();
            }
            catch
            {

            }

            OnLoad?.Invoke();
        }

        protected override void Unload()
        {
            OnUnload?.Invoke();
            Logger.Log($"ThreadsafeBlock is now unloaded.");

            try
            {
                harmony.UnpatchAll(harmonyId);

            }
            catch
            {

            }
        }
    }

    public abstract class Eventual
    {
        public Eventual()
        {
            Main.Instance.OnUnload += CleanUp;
        }

        public abstract void CleanUp();
    }
}
