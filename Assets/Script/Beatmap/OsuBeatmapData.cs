namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class OsuBeatmapData {


		// SUB
		public class TrackComparer : IComparer<Beatmap.Track> {
			public int Compare (Beatmap.Track x, Beatmap.Track y) => x.X.CompareTo(y.X);
		}


		public class TimeSpeedComparer : IComparer<(int time, float speed)> {
			public int Compare ((int time, float speed) x, (int time, float speed) y) => x.time.CompareTo(y.time);
		}


		public class HitObject {
			public int X = 0;
			public int Y = 192;
			public int Time = 0;
			public bool Hold = false;
			public int HitSound = 0;
			public int EndTime = 0;
			public bool LoadFromString (string str) {
				if (string.IsNullOrEmpty(str)) { return false; }
				str = str.Replace('\r', '\0').Replace('\n', '\0').Replace(' ', '\0');
				if (string.IsNullOrEmpty(str)) { return false; }
				var strs = str.Split(',');
				if (strs == null || strs.Length == 0) { return false; }
				if (strs.Length > 0 && !string.IsNullOrEmpty(strs[0]) && int.TryParse(strs[0], out int x)) {
					X = x;
				} else {
					return false;
				}
				if (strs.Length > 1 && !string.IsNullOrEmpty(strs[1]) && int.TryParse(strs[1], out int y)) {
					Y = y;
				}
				if (strs.Length > 2 && !string.IsNullOrEmpty(strs[2]) && int.TryParse(strs[2], out int time)) {
					Time = time;
				} else {
					return false;
				}
				if (strs.Length > 3 && !string.IsNullOrEmpty(strs[3]) && int.TryParse(strs[3], out int type)) {
					Hold = Util.GetBit(type, 7);
				} else {
					return false;
				}
				if (strs.Length > 4 && !string.IsNullOrEmpty(strs[4]) && int.TryParse(strs[4], out int hitSound)) {
					HitSound = hitSound;
				}
				if (strs.Length > 5 && !string.IsNullOrEmpty(strs[5])) {
					var samples = strs[5].Split(':');
					if (samples != null && samples.Length > 0 && int.TryParse(samples[0], out int endTime)) {
						EndTime = endTime;
					}
				}
				return true;
			}
			public string GetStr () => $"{X},192,{Time},{(Hold ? 128 : 1)},{HitSound},{EndTime}:0:0:0:0:";
		}


		public class TimingObject {
			public int Time = 0;
			public float BeatLength = -100f;    // -100:100%speed   // -50:200%speed
			public bool LoadFromString (string str) {
				if (string.IsNullOrEmpty(str)) { return false; }
				str = str.Replace('\r', '\0').Replace('\n', '\0').Replace(' ', '\0');
				if (string.IsNullOrEmpty(str)) { return false; }
				var strs = str.Split(',');
				if (strs == null || strs.Length == 0) { return false; }

				if (strs.Length > 0 && !string.IsNullOrEmpty(strs[0]) && int.TryParse(strs[0], out int time)) {
					Time = time;
				} else {
					return false;
				}

				if (strs.Length > 1 && !string.IsNullOrEmpty(strs[1]) && float.TryParse(strs[1], out float beatLength)) {
					if (beatLength == 0f) { beatLength = -0.0001f; }
					BeatLength = beatLength;
				} else {
					return false;
				}

				return true;
			}
			public string GetStr () => $"{Time},{BeatLength},4,2,1,100,{(BeatLength > 0f ? 1 : 0)},0";
		}



		// VAR
		private readonly static List<HitObject> Hits = new List<HitObject>();
		private readonly static List<TimingObject> Timings = new List<TimingObject>();



		// API
		public static Beatmap Osu_to_Stager (string osu) {
			if (string.IsNullOrEmpty(osu)) { return null; }
			osu = osu.Replace("\r\n", "\n");
			Beatmap data;
			{
				// Str >> Hit
				Hits.Clear();
				int startIndex = osu.IndexOf("[HitObjects]\n") + 13;
				if (startIndex < 0 || startIndex >= osu.Length) { return null; }
				string osu_hit = osu.Substring(startIndex, osu.Length - startIndex);
				if (string.IsNullOrEmpty(osu_hit)) { return null; }
				var strs = osu_hit.Split('\n');
				if (strs == null || strs.Length == 0) { return null; }
				var columnHash = new HashSet<int>();
				foreach (var str in strs) {
					if (string.IsNullOrEmpty(str)) { break; }
					var hit = new HitObject();
					if (hit.LoadFromString(str)) {
						Hits.Add(hit);
						if (!columnHash.Contains(hit.X)) {
							columnHash.Add(hit.X);
						}
					}
				}
				int columnCount = columnHash.Count;
				if (Hits.Count == 0 || columnCount == 0) { return null; }
				// Hits >> Stager
				data = new Beatmap() {
					BPM = 120,
					DropSpeed = 1f,
					Level = 1,
					Ratio = 1.5f,//0.25f * columnCount / 3f,
					Shift = 0f,
					Tag = "Normal",
					Notes = new List<Beatmap.Note>(),
					SpeedNotes = new List<Beatmap.SpeedNote>(),
					Stages = new List<Beatmap.Stage>() { new Beatmap.Stage(){
					Angle = 0f,
					Duration = float.MaxValue,
					Height = 0.6666f,
					Rotation = 0f,
					Speed = 1f,
					Time = 0f,
					Width = 1f,
					X = 0.5f,
					Y = 0f,
					Positions = { },
					Angles = { },
					Heights = { },
					Rotations = { },
					Widths = { },
				}},
					Tracks = new List<Beatmap.Track>(),
				};
				// Tracks
				for (int i = 0; i < columnCount; i++) {
					data.Tracks.Add(new Beatmap.Track() {
						Width = 0.0625f,
						X = Mathf.Lerp(0.5f - (columnCount - 1f) / 32f, 0.5f + (columnCount - 1f) / 32f, i / (columnCount - 1f)),
						Time = 0f,
						Duration = float.MaxValue,
						StageIndex = 0,
						Color = 1,
						HasTray = false,
						Colors = { },
						Widths = { },
						Xs = { },
					});
				}
				// Notes
				foreach (var hit in Hits) {
					if (hit == null) { continue; }
					data.Notes.Add(new Beatmap.Note() {
						X = 0.5f,
						TrackIndex = Mathf.FloorToInt(hit.X * columnCount / 512f),
						ClickSoundIndex = 0,
						Duration = !hit.Hold ? 0f : (hit.EndTime - hit.Time) / 1000f,
						LinkedNoteIndex = -1,
						SwipeX = 1,
						SwipeY = 1,
						Tap = true,
						Time = hit.Time / 1000f,
						Width = 1f,
					});
				}
				Hits.Clear();
			}
			{
				// Str >> Timing
				Timings.Clear();
				int startIndex = osu.IndexOf("[TimingPoints]\n") + 15;
				if (startIndex < 0 || startIndex >= osu.Length) { return data; }
				var osu_Tim = osu.Substring(startIndex, osu.Length - startIndex);
				if (string.IsNullOrEmpty(osu_Tim)) { return data; }
				var strs = osu_Tim.Split('\n');
				if (strs == null || strs.Length == 0) { return data; }
				foreach (var str in strs) {
					if (string.IsNullOrEmpty(str)) { break; }
					var timing = new TimingObject();
					if (timing.LoadFromString(str)) {
						Timings.Add(timing);
					}
				}
				// Timing >> Speed
				bool bpmSetted = false;
				var speedList = new List<(int time, float speed)>();
				for (int i = 0; i < Timings.Count; i++) {
					var timing = Timings[i];
					if (!bpmSetted && timing.BeatLength > 0f) {
						data.BPM = BL_to_BPM(timing.BeatLength);
						bpmSetted = true;
					}
					speedList.Add((
						timing.Time,
						timing.BeatLength > 0f ?
						BL_to_BPM(timing.BeatLength) / data.BPM :
						-100f / timing.BeatLength
					));
				}
				speedList.Sort(new TimeSpeedComparer());
				// Speed >> Map
				float currentSpeed = 1f;
				for (int i = 0; i < speedList.Count; i++) {
					var (time, speed) = speedList[i];
					currentSpeed *= speed;
					if (i == 0 || time != speedList[i - 1].time) {
						data.SpeedNotes.Add(new Beatmap.SpeedNote(time / 1000f, 0f, currentSpeed));
						currentSpeed = 1f;
					}
				}
				// End Timing
				Timings.Clear();
			}
			// Final
			return data;
		}



		public static string Stager_to_Osu (Beatmap map, string name) {
			if (map == null || map.Tracks == null || map.Notes == null || map.Notes.Count == 0) { return ""; }
			// Get Hits
			{
				Hits.Clear();
				int noteCount = map.Notes.Count;
				int columnCount = map.Tracks.Count;
				map.Tracks.Sort(new TrackComparer());
				for (int i = 0; i < noteCount; i++) {
					var note = map.Notes[i];
					Hits.Add(new HitObject() {
						X = Mathf.RoundToInt(512f * ((note.TrackIndex * 2f + 1f) / (columnCount * 2f))),
						Time = Mathf.RoundToInt(note.Time * 1000f),
						Hold = note.Duration > 0.001f,
						EndTime = Mathf.RoundToInt((note.Time + note.Duration) * 1000f),
						Y = 192,
						HitSound = 0,
					});
				}
			}
			// Get Timing
			{
				Timings.Clear();
				Timings.Add(new TimingObject() {
					Time = 0,
					BeatLength = BPM_to_BL(map.BPM),
				});
				if (map.SpeedNotes != null) {
					foreach (var note in map.SpeedNotes) {
						Timings.Add(new TimingObject() {
							Time = Mathf.RoundToInt(note.Time * 1000f),
							BeatLength = note.Speed * -100f,
						});
					}
				}
			}
			// OSU String
			var builder = new System.Text.StringBuilder();
			builder.AppendLine("osu file format v12");
			builder.AppendLine();

			builder.AppendLine("[General]");
			builder.AppendLine("AudioFilename: ");
			builder.AppendLine("AudioLeadIn: 0");
			builder.AppendLine("PreviewTime: 0");
			builder.AppendLine("Countdown: 0");
			builder.AppendLine("SampleSet: Normal");
			builder.AppendLine("StackLeniency: 0.7");
			builder.AppendLine("Mode: 3");
			builder.AppendLine("LetterboxInBreaks: 0");
			builder.AppendLine("SpecialStyle: 0");
			builder.AppendLine("WidescreenStoryboard: 1");

			builder.AppendLine("[Metadata]");
			builder.AppendLine($"Title:{name}");
			builder.AppendLine($"TitleUnicode:{name}");
			builder.AppendLine("Artist:");
			builder.AppendLine("ArtistUnicode:");
			builder.AppendLine("Creator:");
			builder.AppendLine("Version:");
			builder.AppendLine("Source:");
			builder.AppendLine($"Tags:{map.Tag}");
			builder.AppendLine("BeatmapID:0");
			builder.AppendLine("BeatmapSetID:0");

			builder.AppendLine("[TimingPoints]");
			foreach (var timing in Timings) {
				builder.AppendLine(timing.GetStr());
			}

			builder.AppendLine("[HitObjects]");
			foreach (var hit in Hits) {
				builder.AppendLine(hit.GetStr());
			}

			// Final
			Hits.Clear();
			Timings.Clear();
			return builder.ToString();
		}



		private static float BL_to_BPM (float bl) => 1f / bl * 1000f * 60f;


		private static float BPM_to_BL (float bpm) => 1000f * 60f / bpm;





	}
}