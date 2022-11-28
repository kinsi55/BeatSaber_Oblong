using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace BeatSaber_Oblong.Oblong {
	static class Connection {
		private static bool AllowSelfSign(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			return true;
		}

		static WebSocket connection;

		public static void Connect() {
			if(connection != null) {
				connection.OnClose -= Connection_OnClose;
				connection.OnError -= Connection_OnError;

				if(connection.IsAlive)
					connection.Close();
			}

			connection = new WebSocket($"wss://{Config.Instance.ServerIP}/plasma-web-proxy/sockjs/42/42/websocket");

			connection.SslConfiguration.ServerCertificateValidationCallback = AllowSelfSign;
			connection.SslConfiguration.CheckCertificateRevocation = false;
			connection.SslConfiguration.EnabledSslProtocols =
				System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls11;

			connection.SetCookie(new Cookie("admin", Config.Instance.AuthToken));

			connection.OnMessage += Connection_OnMessage;
			connection.OnClose += Connection_OnClose;
			connection.OnError += Connection_OnError;
			connection.OnOpen += Connection_OnOpen;

			connection.ConnectAsync();
		}

		static void Connection_OnError(object sender, ErrorEventArgs e) {
			Plugin.Log.Warn("WS connection error");
			Reconnect();
		}

		static void Connection_OnClose(object sender, CloseEventArgs e) {
			Plugin.Log.Warn("WS connection dropped");
			Reconnect();
		}

		static int reconnectBackoff = 0;
		static async void Reconnect() {
			if(reconnectBackoff < 8)
				reconnectBackoff += reconnectBackoff == 0 ? 1 : reconnectBackoff;

			Plugin.Log.Info($"Reconnecting in {reconnectBackoff} seconds");

			await Task.Delay(reconnectBackoff * 1000);

			Connect();
		}

		/*
		 * I am not entirely sure what these messages do, or what the specific reqId's mean / how they are derived
		 * I guess its just <int++>.<rand>
		 * 
		 * Some of them are probably not needed, thats to be found out - The values partially originate from the
		 * dashboard request https://129.21.24.168/admin/wands
		 * 
		 *	var Settings = {
		 *	  wand_slots: 2,
		 *	  wands_pool: "wands",
		 *	  into_wandreader_pool: "wandreader",
		 *	  from_wandreader_pool: "wandreader-state",
		 *	  wand_ctrl_pool: "wand-control",
		 *	  perception_ctrl_pool: "perception-control",
		 *	  perception_machine: "172.28.0.77",
		 *	  firmware_status_pool: "wand-firmware-status"
		 *	};
		 * 
		 * And oh my god its nested serialized json there is so much of it
		 */
		static string[] initMessages = new string[] {
			"[\"{\\\"reqId\\\":\\\"1.19fhipi\\\",\\\"action\\\":\\\"addpool\\\",\\\"pool\\\":\\\"wands\\\"}\"]",
			"[\"{\\\"reqId\\\":\\\"2.82fs83\\\",\\\"action\\\":\\\"addpool\\\",\\\"pool\\\":\\\"wandreader\\\"}\"]",
			"[\"{\\\"reqId\\\":\\\"3.wccp04\\\",\\\"action\\\":\\\"addpool\\\",\\\"pool\\\":\\\"wandreader-state\\\"}\"]",

			//These are sent after receiving the responses of the previous messages - Not sure if we need to actually wait for those
			"[\"{\\\"reqId\\\":\\\"4.ivwixu\\\",\\\"action\\\":\\\"deposit\\\",\\\"descrips\\\":[\\\"pogo\\\",\\\"wandreader\\\"],\\\"ingests\\\":{\\\"get-system-topology\\\":\\\"\\\"},\\\"pool\\\":\\\"wandreader\\\"}\"]",
			"[\"{\\\"reqId\\\":\\\"5.1u8hqh\\\",\\\"action\\\":\\\"deposit\\\",\\\"descrips\\\":[\\\"pogo\\\",\\\"wandreader\\\"],\\\"ingests\\\":{\\\"get-settings\\\":\\\"\\\"},\\\"pool\\\":\\\"wandreader\\\"}\"]"
		};


		static void Connection_OnOpen(object sender, EventArgs e) {
			foreach(var m in initMessages)
				connection.Send(m);
		}

		static void Connection_OnMessage(object sender, MessageEventArgs e) {
			var m = e.Data;

			// Filter for the messages we care about
			if(m.Length == 0 || !m.StartsWith("a[\"[\\\"wands\\\"", StringComparison.OrdinalIgnoreCase))
				return;

			FrameParser.Parse(m);
		}
	}
}
