using BeatSaber_Oblong.Oblong;
using BeatSaber_Oblong.UI;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace BeatSaber_Oblong {
	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
		internal static Plugin Instance;
		internal static IPALogger Log;
		internal static Harmony harmony;

		[Init]
		public Plugin(IPALogger logger, IPA.Config.Config conf) {
#if !DEBUG
			if(!Array.Exists(Environment.GetCommandLineArgs(), x => x == "--oblong"))
				return;
#endif

			Instance = this;
			Log = logger;
			Config.Instance = conf.Generated<Config>();
			harmony = new Harmony("Kinsi55.BeatSaber.BeatSaber_Oblong");

			harmony.PatchAll(Assembly.GetExecutingAssembly());

			TheFlowCoordinator.Initialize();
			Connection.Connect();
		}
	}
}
