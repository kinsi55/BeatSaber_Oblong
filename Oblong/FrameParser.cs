using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace BeatSaber_Oblong.Oblong {
	class ControllerInfo {
		// Using static for less generated garbage
		public static readonly Dictionary<string, ControllerInfo> instances = new Dictionary<string, ControllerInfo>();

		public readonly string name;

		public bool isConnected = false;
		public float[] position = new float[3];
		public float[] rotation = new float[3];
		public HashSet<string> pressedButtons = new HashSet<string>();

		public ControllerInfo(string name) {
			this.name = name;
		}

		public static ControllerInfo GetInstance(string name) {
			if(!instances.TryGetValue(name, out var frame))
				frame = instances[name] = new ControllerInfo(name);

			return frame;
		}
	}

	static class FrameParser {
		static readonly int headChars = "a[\"".Length;
		static readonly int tailChars = "\"]".Length;

		public static Dictionary<string, ControllerInfo> Parse(string frame) {
			// Filter for the messages we care about
			if(frame.Length == 0 || !frame.StartsWith("a[\"[\\\"wands\\\"", StringComparison.OrdinalIgnoreCase))
				return null;

			/*
			 * We can skip one unnecessary json deserialize with string manipulation
			 * 
			 * We are always sent an array that contains one value (string) which is... Json
			 */
			frame = frame.Substring(headChars, frame.Length - headChars - tailChars).Replace("\\", "");

			return JsonConvert.DeserializeObject<Dictionary<string, ControllerInfo>>(frame, 
				new JsonSerializerSettings {
					Converters = new[] { new FrameConverter() 
				}
			});
		}
	}

	class FrameConverter : JsonConverter<Dictionary<string, ControllerInfo>> {
		public override void WriteJson(JsonWriter writer, Dictionary<string, ControllerInfo> rect, JsonSerializer serializer) {}
		public override Dictionary<string, ControllerInfo> ReadJson(JsonReader reader, Type objectType, Dictionary<string, ControllerInfo> existingValue, bool hasExistingValue, JsonSerializer serializer) {
			// "wands"
			if(!reader.Read() || reader.TokenType != JsonToken.String || reader.Value.ToString() != "wands")
				return null;

			// Conveniently, before the object that we are interested about, theres no others
			// [...] {
			while(reader.Read() && reader.TokenType != JsonToken.StartObject);

			// "wand-report":
			while(reader.Read()) {
				if(reader.TokenType == JsonToken.PropertyName && reader.Value.ToString() == "wand-report") {
					// [
					reader.Read();
					break;
				}
			}

			foreach(var x in ControllerInfo.instances)
				x.Value.isConnected = false;

			while(ParseController(reader));
			// Gotta parse until the very end else json.net complains
			while(reader.Read());

			return ControllerInfo.instances;
		}

		bool ParseController(JsonReader reader) {
			// {
			if(!reader.Read() || reader.TokenType != JsonToken.StartObject)
				return false;

			void ReadVector(float[] target, JsonReader reader) {
				// [
				reader.Read();
				for(var i = 0; i < 3; i++)
					target[i] = (float)reader.ReadAsDouble();
				// ]
				reader.Read();
			}

			void SkipValue(JsonReader reader) {
				reader.Read();

				if(reader.TokenType == JsonToken.PropertyName)
					reader.Read();

				if(reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.StartObject) {
					while(reader.TokenType != JsonToken.EndArray && reader.TokenType != JsonToken.EndObject)
						SkipValue(reader);
				}
			}

			/*
			 * So this is kind of a big hack
			 * I'm assuming here that the name of the controller is always serialized
			 * before any of the values we care about (which seems to be the case)
			 */
			ControllerInfo c = null;
			float[] norm = new float[3];
			float[] over = new float[3];

			while(reader.Read()) {
				if(reader.TokenType == JsonToken.PropertyName) {
					var prop = reader.Value.ToString();

					if(c == null) {
						if(prop == "name") {
							c = ControllerInfo.GetInstance(reader.ReadAsString());
							c.isConnected = true;
						}

						continue;
					}

					if(prop == "loc") {
						ReadVector(c.position, reader);
					} else if(prop == "norm") {
						ReadVector(norm, reader);
					} else if(prop == "over") {
						ReadVector(over, reader);
					} else if(prop == "buttons") {
						// [[
						reader.Read(); reader.Read();

						c.pressedButtons.Clear();
						// "A", "B", .... ]
						while(reader.Read() && reader.TokenType != JsonToken.EndArray)
							c.pressedButtons.Add(reader.Value.ToString());
						// ]
						reader.Read();
					} else {
						SkipValue(reader);
					}
				} else if(reader.TokenType == JsonToken.EndObject) {
					break;
				}
			}

			// Stolen from Webpanel, get normal euler angles
			var v_norm = new Vector3(norm[0], norm[1], norm[2]);
			var v_over = new Vector3(over[0], over[1], over[2]);

			var u = Vector3.Cross(v_over, v_norm);

			var cos_pitch = Math.Sqrt((u[0] * u[0]) + (u[2] * u[2]));

			if(Math.Abs(cos_pitch) < 0.0000007) {
				c.rotation[0] = 0;
				c.rotation[1] = 0;
				c.rotation[2] = 0;
			} else {
				var sin_pitch = -u[1];
				var sin_yaw = -u[0] / cos_pitch;
				var cos_yaw = u[2] / cos_pitch;

				var yaw = Math.Atan2(-u[0], u[2]);

				var pitch = Math.Atan2(sin_pitch, cos_pitch);

				var roll0 = new Vector3((float)cos_yaw, 0, (float)sin_yaw);

				var roll = Math.Atan2(
					Vector3.Dot(roll0, v_norm),
					Vector3.Dot(roll0, v_over)
				);

				float MakePositive(float rot) {
					return rot >= 0 ? rot : 360 + rot;
				}

				c.rotation[0] = MakePositive((float)(pitch * (180 / Math.PI)));
				c.rotation[1] = MakePositive((float)(yaw * (180 / Math.PI)));

				c.rotation[2] = MakePositive((float)(roll * (180 / Math.PI)));
			}


			return c != null;
		}
	}
}
