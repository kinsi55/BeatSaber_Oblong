using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Zenject;

namespace BeatSaber_Oblong.HarmonyPatches {
	[HarmonyPatch(typeof(MainSystemInit), nameof(MainSystemInit.InstallBindings))]
	static class PlatformHelperSetter {
		static void Postfix(DiContainer container) {
			if(container.HasBinding<IVRPlatformHelper>())
				container.Rebind<IVRPlatformHelper>().To<OblongPlatformHelper>().AsSingle();
		}
	}
}
