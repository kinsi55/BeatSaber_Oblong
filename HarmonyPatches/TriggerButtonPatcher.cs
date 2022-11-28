using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaber_Oblong.Oblong;
using HarmonyLib;
using UnityEngine.XR;

namespace BeatSaber_Oblong.HarmonyPatches {
	[HarmonyPatch(typeof(VRControllersInputManager), nameof(VRControllersInputManager.TriggerValue))]
	static class TriggerButtonPatcher {
		static bool Prefix(XRNode node, ref float __result) {
			ControllerInfo c = null;

			if(node == XRNode.LeftHand) {
				ControllerInfo.instances.TryGetValue(Config.Instance.LeftController, out c);
			} else if(node == XRNode.RightHand) {
				ControllerInfo.instances.TryGetValue(Config.Instance.RightController, out c);
			}

			if(c?.pressedButtons.Contains(Config.Instance.TriggerMapping) != true)
				return true;

			__result = 1f;
			return false;
		}
	}
}
