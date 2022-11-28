using System;
using BeatSaber_Oblong.HarmonyPatches;
using BeatSaber_Oblong.Oblong;
using UnityEngine;
using UnityEngine.XR;

namespace BeatSaber_Oblong {
	internal class OblongPlatformHelper : IVRPlatformHelper {
		public bool hasInputFocus => true;
		public bool hasVrFocus => true;
		public bool isAlwaysWireless => true;

		public VRPlatformSDK vrPlatformSDK => VRPlatformSDK.Unknown;
		public XRDeviceModel currentXRDeviceModel => XRDeviceModel.Unknown;

#pragma warning disable 67
		public event Action inputFocusWasCapturedEvent;
		public event Action inputFocusWasReleasedEvent;
		public event Action vrFocusWasCapturedEvent;
		public event Action vrFocusWasReleasedEvent;
		public event Action hmdUnmountedEvent;
		public event Action hmdMountedEvent;
		public event Action joystickWasCenteredThisFrameEvent;
		public event Action<Vector2> joystickWasNotCenteredThisFrameEvent;
#pragma warning restore

		public void AdjustControllerTransform(XRNode node, Transform transform, Vector3 position, Vector3 rotation) {
			if(node != XRNode.LeftHand && node != XRNode.RightHand)
				return;
			
			transform.Rotate(rotation);
			transform.Translate(position);
		}
		float ClampAngle(float angle, float from, float to) {
			if(angle > 180)
				angle = 360 - angle;
			angle = Mathf.Clamp(angle, from, to);
			if(angle < 0)
				angle = 360 + angle;

			return angle;
		}


		public bool GetNodePose(XRNode nodeType, int idx, out Vector3 pos, out Quaternion rot) {
			if(nodeType == XRNode.Head) {
				pos = HeadTransformPatcher.GetHeadPos();
				rot = Quaternion.identity;
				return true;
			}

			ControllerInfo c = null;

			if(nodeType == XRNode.LeftHand) {
				ControllerInfo.instances.TryGetValue(Config.Instance.LeftController, out c);
			} else if(nodeType == XRNode.RightHand) {
				ControllerInfo.instances.TryGetValue(Config.Instance.RightController, out c);
			}
			
			
			if(c == null) {
				pos = Vector3.zero;
				rot = Quaternion.identity;
				return false;
			}

			/*
			 * We would need to rotate the coordinate space according to the correct forward facing direction...
			 * 
			 * But I'm not smart enough to accomplish that
			 */
			//var backward = new Vector3(
			//	0,
			//	-Config.Instance.Center_Rot_Y,
			//	0
			//);

			rot = Quaternion.Euler(
				c.rotation[0] - Config.Instance.Center_Rot_X,
				c.rotation[1] - Config.Instance.Center_Rot_Y,
				c.rotation[2] - Config.Instance.Center_Rot_Z 
			);

			pos = new Vector3(
				(c.position[0] - Config.Instance.Center_Pos_X) / Config.Instance.PositionDivider,
				(c.position[1] - Config.Instance.Center_Pos_Y) / Config.Instance.PositionDivider,
				(Config.Instance.Center_Pos_Z - c.position[2]) / Config.Instance.PositionDivider
			);

			//rot *= backward;
			//pos = Quaternion.Euler(backward) * pos;

			return true;
		}
		public void StopHaptics(XRNode node) { }
		public void TriggerHapticPulse(XRNode node, float duration, float strength, float frequency) { }
	}
}