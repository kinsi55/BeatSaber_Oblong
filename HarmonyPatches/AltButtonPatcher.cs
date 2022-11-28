using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaber_Oblong.Oblong;
using HarmonyLib;

namespace BeatSaber_Oblong.HarmonyPatches {
	[HarmonyPatch(typeof(VRControllersInputManager), nameof(VRControllersInputManager.MenuButtonDown))]
	static class AltButtonPatcher {
		static bool Prefix(ref bool __result) {
			foreach(var x in ControllerInfo.instances) {
				if(x.Value.isConnected && x.Value.pressedButtons.Contains(Config.Instance.AltButtonMapping)) {
					__result = true;
					break;
				}
			}

			return false;
		}
	}
}
