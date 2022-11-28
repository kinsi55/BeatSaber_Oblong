using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace BeatSaber_Oblong {
	internal class Config {
		public static Config Instance;
		public virtual string ServerIP { get; set; } = "129.21.24.168";
		public virtual string AuthToken { get; set; } = "";

		public virtual float PositionDivider { get; set; } = 1000f;

		public virtual string LeftController { get; set; } = "wand-1";
		public virtual string RightController { get; set; } = "wand-2";

		public virtual string TriggerMapping { get; set; } = "A";
		public virtual string AltButtonMapping { get; set; } = "B";


		public float Center_Pos_X = 0.0f;
		public float Center_Pos_Y = 0.0f;
		public float Center_Pos_Z = 0.0f;

		public float Center_Rot_X = 0.0f;
		public float Center_Rot_Y = 0.0f;
		public float Center_Rot_Z = 0.0f;



		/// <summary>
		/// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
		/// </summary>
		public virtual void OnReload() {
			// Do stuff after config is read from disk.
		}

		/// <summary>
		/// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
		/// </summary>
		public virtual void Changed() {
			// Do stuff when the config is changed.
		}

		/// <summary>
		/// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
		/// </summary>
		public virtual void CopyFrom(Config other) {
			// This instance's members populated from other
		}
	}
}
