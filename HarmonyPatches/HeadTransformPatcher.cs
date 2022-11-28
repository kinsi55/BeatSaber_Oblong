using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace BeatSaber_Oblong.HarmonyPatches {
	[HarmonyPatch(typeof(PlayerTransforms), nameof(PlayerTransforms.Update))]
	static class HeadTransformPatcher {
		public static Vector3 GetHeadPos() => new Vector3(0, 1.5f, -2f);

		static void Prefix(Transform ____headTransform) {
			____headTransform.rotation = Quaternion.identity;

			____headTransform.position = GetHeadPos();
		}
	}
}
