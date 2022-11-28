using BeatSaber_Oblong.Oblong;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BeatSaber_Oblong.UI {
	class TheFlowCoordinator : FlowCoordinator {
		MAN view = null;

		protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
			if(firstActivation) {
				SetTitle("Oblong Settings");
				showBackButton = true;

				if(view == null)
					view = BeatSaberUI.CreateViewController<MAN>();

				ProvideInitialViewControllers(view);
			}
		}

		protected override void BackButtonWasPressed(ViewController topViewController) {
			BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Horizontal);
			view?.CancelModal();
		}

		public void ShowFlow() {
			var _parentFlow = BeatSaberUI.MainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();

			BeatSaberUI.PresentFlowCoordinator(_parentFlow, this);
		}

		static TheFlowCoordinator flow = null;

		public static void Initialize() {
			MenuButtons.instance.RegisterButton(new MenuButton("Oblong Settings", "", () => {
				if(flow == null)
					flow = BeatSaberUI.CreateFlowCoordinator<TheFlowCoordinator>();

				flow.ShowFlow();
			}, true));
		}
	}

	[HotReload(RelativePathToLayout = @"./settings.bsml")]
	[ViewDefinition("BeatSaber_Oblong.UI.settings.bsml")]
	class MAN : BSMLAutomaticViewController {
		readonly Config config = Config.Instance;

		[UIParams] readonly BSMLParserParams parserParams = null;
		[UIComponent("titleText")] readonly TextMeshProUGUI titleText = null;
		[UIComponent("mainText")] readonly TextMeshProUGUI mainText = null;


		[UIAction("Recenter")]
		void Recenter() {
			ShowModal(
				"Recenter",
				"Hold the Right controller pointing forward in the middle of the playspace and press the trigger button",
				WaitForInput(
					x => x.pressedButtons.Contains(Config.Instance.TriggerMapping),
					x => {
						Config.Instance.Center_Pos_X = x.position[0];
						Config.Instance.Center_Pos_Y = x.position[1];
						Config.Instance.Center_Pos_Z = x.position[2];

						Config.Instance.Center_Rot_X = x.rotation[0];
						Config.Instance.Center_Rot_Y = x.rotation[1];
						Config.Instance.Center_Rot_Z = x.rotation[2];

						Config.Instance.Changed();
					}
				)
			);
		}

		[UIAction("SetButtonTrigger")]
		void SetButtonTrigger() {
			ShowModal(
				"Map Trigger Button",
				"Press any button on any controller to set as the trigger button",
				WaitForInput(
					x => x.pressedButtons.Any(),
					x => {
						Config.Instance.TriggerMapping = x.pressedButtons.First();
					}
				)
			);
		}

		[UIAction("SetButtonAlt")]
		void SetButtonAlt() {
			ShowModal(
				"Map Trigger Button",
				"Press any button on any controller to set as the alt button",
				WaitForInput(
					x => x.pressedButtons.Any(),
					x => {
						Config.Instance.AltButtonMapping = x.pressedButtons.First();
					}
				)
			);
		}

		[UIAction("SetControllerLeft")]
		void SetControllerLeft() {
			ShowModal(
				"Map Trigger Button",
				"Press any button on the controller that you want to use as the left controller",
				WaitForInput(
					x => x.pressedButtons.Any(),
					x => {
						Config.Instance.LeftController = x.name;
					}
				)
			);
		}

		[UIAction("SetControllerRight")]
		void SetControllerRight() {
			ShowModal(
				"Map Trigger Button",
				"Press any button on the controller that you want to use as the right controller",
				WaitForInput(
					x => x.pressedButtons.Any(),
					x => {
						Config.Instance.RightController = x.name;
					}
				)
			);
		}

		bool doCancel = false;
		IEnumerator WaitForInput(Func<ControllerInfo, bool> checkForInput, Action<ControllerInfo> finishAction) {
			while(!doCancel) {
				foreach(var x in ControllerInfo.instances.Values) {
					if(!x.isConnected)
						continue;

					if(checkForInput(x)) {
						parserParams.EmitEvent("CloseResultModal");
						finishAction(x);
						yield break;
					}
				}
				yield return null;
			}
		}

		public void CancelModal() {
			doCancel = true;
			parserParams.EmitEvent("CloseResultModal");
		}

		void ShowModal(string title, string text, IEnumerator coro) {
			titleText.text = title;
			mainText.text = text;
			doCancel = false;

			parserParams.EmitEvent("ShowResultModal");
			StartCoroutine(coro);
		}
	}
}
