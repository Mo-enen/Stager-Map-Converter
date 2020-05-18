namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using UnityEngine;


	public static class ArcaeaBeatmapData {



		public class ArcIntComparer : IComparer<(Arc, int)> {
			public int Compare ((Arc, int) x, (Arc, int) y) => x.Item1.Time.CompareTo(y.Item1.Time);
		}


		public class ArcaeaItemComparer : IComparer<ArcaeaItem> {
			public int Compare (ArcaeaItem a, ArcaeaItem b) => a.Time.CompareTo(b.Time);
		}



		public class ArcaeaItem {
			public int Time;
		}



		public class Note : ArcaeaItem {
			public int Duration;
			public int Lane;
		}


		public class Timing : ArcaeaItem {
			public float BPM;
			public float Beat;
		}


		public class Arc : ArcaeaItem {
			public int TimeEnd;
			public float XStart;
			public float XEnd;
			public string Ease;
			public float YStart;
			public float YEnd;
			public int Color; // 0:Blue   1:Pink
			public string Fx;
			public bool Skyline;
			public List<int> Arctap;
		}


		public enum NoteType {
			Tap = 0,
			Hold = 1,
			ArcBlue = 2,
			ArcPink = 3,
			ArcTap = 4,
			Skyline = 5,
		}


		private readonly static List<Note> Notes = new List<Note>();
		private readonly static List<Timing> Timings = new List<Timing>();
		private readonly static List<(Arc arc, bool done)> Arcs = new List<(Arc, bool)>();
		private const float SCALE_Z = 0.2f;



		public static Beatmap Arcaea_To_Stager (string aMap) {

			// String >> Data
			if (!ArcaeaString_To_Data(aMap)) { return null; }

			// Data >> map
			var data = new Beatmap() {
				BPM = Timings.Count > 0 ? (int)Timings[0].BPM : 120,
				Level = 1,
				Ratio = 1.5f,
				Shift = 0f,
				Tag = "Normal",
				CreatedTime = System.DateTime.Now.Ticks,
				Stages = {
					new Beatmap.Stage(){
						X = 0.5f,
						Y = 0f,
						Duration = float.MaxValue,
						Height = 1.8f,
						Rotation = 0f,
						Speed = 0.6f,
						Time = 0f,
						Width = 0.618f,
						Widths = { },
						Heights = { },
						Positions = { },
						Rotations = { },
					},
					new Beatmap.Stage(){
						X = 0.5f,
						Y = 0f,
						Duration = float.MaxValue,
						Height = 1.8f,
						Rotation = 0f,
						Speed = 0.6f,
						Time = 0f,
						Width = 0.618f,
						Widths = { },
						Heights = { },
						Positions = { },
						Rotations = { },
					},
				},
				Tracks = {
					new Beatmap.Track(){
						X = 1f / 8f,
						Width = 0.25f,
						Angle = 45f,
						StageIndex = 0,
						Time = 0f,
						Duration = float.MaxValue,
						Color = 0,
						HasTray = false,
						Colors = { },
						Xs = { },
						Angles = { },
						Widths = { },
					},
					new Beatmap.Track(){
						X = 3f / 8f,
						Width = 0.25f,
						Angle = 45f,
						StageIndex = 0,
						Time = 0f,
						Duration = float.MaxValue,
						Color = 0,
						HasTray = false,
						Colors = { },
						Xs = { },
						Angles = { },
						Widths = { }, },
					new Beatmap.Track(){
						X = 5f / 8f,
						Width = 0.25f,
						Angle = 45f,
						StageIndex = 0,
						Time = 0f,
						Duration = float.MaxValue,
						Color = 0,
						HasTray = false,
						Colors = { },
						Xs = { },
						Angles = { },
						Widths = { },},
					new Beatmap.Track(){
						X = 7f / 8f,
						Width = 0.25f,
						Angle = 45f,
						StageIndex = 0,
						Time = 0f,
						Duration = float.MaxValue,
						Color = 0,
						HasTray = false,
						Colors = { },
						Xs = { },
						Angles = { },
						Widths = { }, },
					new Beatmap.Track(){
						X = 0.5f,
						Width = 1f,
						Angle = 45f,
						StageIndex = 1,
						Time = 0f,
						Duration = float.MaxValue,
						Color = 1,
						HasTray = false,
						Colors = { },
						Xs = { },
						Angles = { },
						Widths = { },},
				},
				Notes = { },
				Timings = { },
			};

			// Notes
			for (int i = 0; i < Notes.Count; i++) {
				var note = Notes[i];
				data.Notes.Add(new Beatmap.Note() {
					Time = note.Time / 1000f,
					Duration = note.Duration / 1000f,
					TrackIndex = note.Lane - 1,
					X = 0.5f,
					Z = 0f,
					ClickSoundIndex = 0,
					LinkedNoteIndex = -1,
					ItemType = note.Duration == 0 ? (int)NoteType.Tap : (int)NoteType.Hold,
					Width = 1f,
				});
			}

			// Arc Taps
			for (int i = 0; i < Arcs.Count; i++) {
				var arc = Arcs[i].arc;
				if (arc.Arctap == null || arc.Arctap.Count == 0) { continue; }
				for (int j = 0; j < arc.Arctap.Count; j++) {
					float time = arc.Arctap[j] / 1000f;
					data.Notes.Add(new Beatmap.Note() {
						Time = time,
						X = Util.Remap(arc.Time / 1000f, arc.TimeEnd / 1000f, arc.XStart, arc.XEnd, time),
						Z = Util.Remap(arc.Time / 1000f, arc.TimeEnd / 1000f, arc.YStart, arc.YEnd, time) * SCALE_Z,
						Width = 0.25f,
						ItemType = (int)NoteType.ArcTap,
						ClickSoundIndex = 0,
						Duration = 0,
						LinkedNoteIndex = -1,
						TrackIndex = 4,
					});
				}
			}


			// Arc Map
			var arcMap = new Dictionary<int, List<(Arc arc, int index)>>();
			for (int i = 0; i < Arcs.Count; i++) {
				var arc = Arcs[i];
				if (!arcMap.ContainsKey(arc.arc.Time)) {
					arcMap.Add(arc.arc.Time, new List<(Arc arc, int index)>());
				}
				arcMap[arc.arc.Time].Add((arc.arc, i));
			}
			foreach (var pair in arcMap) {
				pair.Value.Sort(new ArcIntComparer());
			}

			// Arcs
			for (int i = 0; i < Arcs.Count; i++) {

				var (arc, done) = Arcs[i];
				if (done) { continue; }

				// Arc Head
				int time = arc.TimeEnd;
				float x = arc.XEnd;
				float y = arc.YEnd;
				bool skyline = arc.Skyline;
				int color = arc.Color;

				// Arc Head
				data.Notes.Add(new Beatmap.Note() {
					X = arc.XStart,
					Time = arc.Time / 1000f,
					Width = 0.05f,
					Z = arc.YStart * SCALE_Z,
					ClickSoundIndex = 0,
					ItemType = skyline ? (int)NoteType.Skyline : color == 0 ? (int)NoteType.ArcBlue : (int)NoteType.ArcPink,
					Duration = 0f,
					LinkedNoteIndex = data.Notes.Count + 1,
					TrackIndex = 4,
				});
				Arcs[i] = (arc, true);

				// Arc List
				for (int safe = 0; safe < Arcs.Count && arcMap.ContainsKey(time); safe++) {

					var list = arcMap[time];

					// Find Closest in List
					int NextIndex = -1;
					Arc nextArc = null;
					foreach (var (_arc, _i) in list) {
						if (
							!Arcs[_i].done &&
							Mathf.Abs(_arc.XStart - x) < 0.01f &&
							Mathf.Abs(_arc.YStart - y) < 0.01f
						) {
							nextArc = _arc;
							NextIndex = _i;
							break;
						}
					}

					// Pass to next loop
					if (NextIndex < 0) { break; }

					skyline = nextArc.Skyline;
					color = nextArc.Color;

					// End Current Arc
					data.Notes.Add(new Beatmap.Note() {
						X = x,
						Time = time / 1000f,
						Width = 0.05f,
						Z = y * SCALE_Z,
						ClickSoundIndex = 0,
						ItemType = skyline ? (int)NoteType.Skyline : color == 0 ? (int)NoteType.ArcBlue : (int)NoteType.ArcPink,
						Duration = 0f,
						LinkedNoteIndex = data.Notes.Count + 1,
						TrackIndex = 4,
					});

					Arcs[NextIndex] = (Arcs[NextIndex].arc, true);
					time = nextArc.TimeEnd;
					x = nextArc.XEnd;
					y = nextArc.YEnd;

				}

				// Arc Tail
				data.Notes.Add(new Beatmap.Note() {
					X = x,
					Time = time / 1000f,
					Width = 0.05f,
					Z = y * SCALE_Z,
					ClickSoundIndex = 0,
					ItemType = skyline ? (int)NoteType.Skyline : color == 0 ? (int)NoteType.ArcBlue : (int)NoteType.ArcPink,
					Duration = 0f,
					LinkedNoteIndex = -1,
					TrackIndex = 4,
				});

			}

			// Speeds
			for (int i = 0; i < Timings.Count; i++) {
				var timing = Timings[i];
				data.Timings.Add(new Beatmap.Timing(timing.Time / 1000f, timing.BPM / data.BPM));
			}

			// Final
			Notes.Clear();
			Timings.Clear();
			Arcs.Clear();
			data.SortNotesByTime();
			return data;
		}



		public static string Stager_To_Arcaea (Beatmap sMap) {

			if (sMap == null) { return ""; }
			sMap.SortNotesByTime();

			// Map >> Data
			Notes.Clear();
			Timings.Clear();
			Arcs.Clear();

			var arcDoneHash = new HashSet<int>();
			for (int i = 0; i < sMap.Notes.Count; i++) {
				var note = sMap.Notes[i];
				if (note.TrackIndex < 4) {
					// Ground
					Notes.Add(new Note() {
						Time = note.time,
						Duration = note.duration,
						Lane = Mathf.Clamp(note.TrackIndex + 1, 1, 4),
					});
				} else if (note.LinkedNoteIndex >= 0 && note.LinkedNoteIndex < sMap.Notes.Count && !arcDoneHash.Contains(i)) {
					// Arc Tap
					arcDoneHash.Add(i);
					var linkedNote = sMap.Notes[note.LinkedNoteIndex];
					Arcs.Add((new Arc() {
						Time = note.time,
						TimeEnd = linkedNote.time,
						XStart = note.X,
						XEnd = linkedNote.X,
						YStart = note.Z / SCALE_Z,
						YEnd = linkedNote.Z / SCALE_Z,
						Skyline = note.ItemType == (int)NoteType.Skyline,
						Fx = "none",
						Ease = "s",
						Color = 0,
						Arctap = null,
					}, false));
					if (linkedNote.LinkedNoteIndex < 0) {
						arcDoneHash.Add(note.LinkedNoteIndex);
					}
				}
			}

			// Arc Tap
			for (int i = 0; i < sMap.Notes.Count; i++) {
				var note = sMap.Notes[i];
				if (note.ItemType != (int)NoteType.ArcTap || note.LinkedNoteIndex >= 0 || arcDoneHash.Contains(i)) { continue; }
				Arc closestArc = null;
				float distance = float.MaxValue;
				var notePos = new Vector2(note.X, note.Z / SCALE_Z);
				foreach (var (arc, _) in Arcs) {
					if (!arc.Skyline || note.time < arc.Time || note.time > arc.TimeEnd) { continue; }
					float dis = Vector2.Distance(notePos, new Vector2(
						Util.Remap(arc.Time / 1000f, arc.TimeEnd / 1000f, arc.XStart, arc.XEnd, note.Time),
						Util.Remap(arc.Time / 1000f, arc.TimeEnd / 1000f, arc.YStart, arc.YEnd, note.Time)
					));
					if (dis < 0.1f && dis < distance) {
						distance = dis;
						closestArc = arc;
					}
				}
				if (closestArc != null) {
					if (closestArc.Arctap == null) {
						closestArc.Arctap = new List<int>();
					}
					closestArc.Arctap.Add(note.time);
				}
			}

			// Timing
			foreach (var speed in sMap.Timings) {
				if (speed.time > 0) {
					Timings.Add(new Timing() {
						Time = speed.time,
						Beat = 4f,
						BPM = speed.Speed * sMap.BPM,
					});
				}
			}

			// Data >> ItemList >> String
			var items = new List<ArcaeaItem>();
			foreach (var note in Notes) {
				items.Add(note);
			}
			foreach (var timing in Timings) {
				if (timing.Time > 0) {
					items.Add(timing);
				}
			}
			foreach (var arc in Arcs) {
				arc.arc.XStart = (arc.arc.XStart - 1f / 4f) * 4f / 2f;
				arc.arc.XEnd = (arc.arc.XEnd - 1f / 4f) * 4f / 2f;
				items.Add(arc.arc);
			}
			Notes.Clear();
			Timings.Clear();
			Arcs.Clear();
			items.Sort(new ArcaeaItemComparer());
			return Data_To_ArcaeaString(items, sMap.BPM);

		}



		// LGC
		private static bool ArcaeaString_To_Data (string aMap) {

			if (string.IsNullOrEmpty(aMap)) { return false; }

			Notes.Clear();
			Timings.Clear();
			Arcs.Clear();

			// Time Offset
			int timeOffset = 0;
			int audioOffsetLength = aMap.IndexOf('-');
			int OFFSET_KEY_LENGTH = "audiooffset:".Length;

			if (int.TryParse(aMap.Substring(OFFSET_KEY_LENGTH, audioOffsetLength - OFFSET_KEY_LENGTH), out int _audioOffset)) {
				timeOffset = _audioOffset;
			}

			// String >> Data
			var strs = aMap.Substring(audioOffsetLength + 1, aMap.Length - audioOffsetLength - 1).Replace("\r", "").Replace("\n", "").Split(';');
			if (strs == null || strs.Length == 0) { return false; }
			for (int i = 0; i < strs.Length; i++) {

				string str = strs[i];

				// Key Value
				int keyLength = str.IndexOf('(');
				if (keyLength == -1) { continue; }
				string key = str.Substring(0, keyLength);
				string valueStr = str.Remove(0, key.Length);
				if (key == null) { key = ""; }

				switch (key.ToLower()) {
					case "": {
						var values = valueStr.Replace("(", "").Replace(")", "").Replace(";", "").Split(',');
						if (values == null || values.Length < 2) { break; }
						if (int.TryParse(values[0], out int _time) && int.TryParse(values[1], out int _lane)) {
							Notes.Add(new Note() {
								Time = _time + timeOffset,
								Duration = 0,
								Lane = _lane,
							});
						}
					}
					break;
					case "hold": {
						var values = valueStr.Replace("(", "").Replace(")", "").Replace(";", "").Split(',');
						if (values == null || values.Length < 2) { break; }
						if (int.TryParse(values[0], out int _time) && int.TryParse(values[1], out int _timeEnd) && int.TryParse(values[2], out int _lane)) {
							Notes.Add(new Note() {
								Time = _time + timeOffset,
								Duration = Mathf.Max(_timeEnd - _time, 0),
								Lane = _lane,
							});
						}
					}
					break;
					case "arc": {
						int arcLen = valueStr.IndexOf('[');
						if (arcLen < 0) {
							arcLen = valueStr.Length;
						}
						var values = valueStr.Substring(0, arcLen).Replace("(", "").Replace(")", "").Replace(";", "").Split(',');
						if (values == null || values.Length < 10) { break; }
						var arc = new Arc() { Arctap = null, };
						// Arc
						if (
							int.TryParse(values[0], out int _timeStart) &&
							int.TryParse(values[1], out int _timeEnd) &&
							float.TryParse(values[2], out float _xStart) &&
							float.TryParse(values[3], out float _xEnd) &&
							float.TryParse(values[5], out float _yStart) &&
							float.TryParse(values[6], out float _yEnd) &&
							int.TryParse(values[7], out int _color) &&
							bool.TryParse(values[9].ToLower(), out bool _skyLine)
						) {
							arc.Time = _timeStart + timeOffset;
							arc.TimeEnd = _timeEnd + timeOffset;
							arc.XStart = Mathf.LerpUnclamped(1f / 4f, 3f / 4f, _xStart);
							arc.XEnd = Mathf.LerpUnclamped(1f / 4f, 3f / 4f, _xEnd);
							arc.YStart = _yStart;
							arc.YEnd = _yEnd;
							arc.Color = _color;
							arc.Ease = values[4];
							arc.Fx = values[8];
							arc.Skyline = _skyLine;
						} else {
							break;
						}
						// Arc Tap
						if (arcLen < valueStr.Length) {
							var tapValues = valueStr.Substring(arcLen, valueStr.Length - arcLen).Replace("arctap", "").Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Replace(";", "").Split(',');
							if (tapValues != null && tapValues.Length > 0) {
								arc.Arctap = new List<int>();
								foreach (var tapStr in tapValues) {
									if (int.TryParse(tapStr, out int _tapTime)) {
										arc.Arctap.Add(_tapTime + timeOffset);
									}
								}
							}
						}
						// Add
						Arcs.Add((arc, false));
					}
					break;
					case "timing": {
						var values = valueStr.Replace("(", "").Replace(")", "").Replace(";", "").Split(',');
						if (values == null || values.Length < 3) { break; }
						if (int.TryParse(values[0], out int _offset) && float.TryParse(values[1], out float _bpm) && float.TryParse(values[2], out float _beats)) {
							Timings.Add(new Timing() {
								Time = _offset,
								BPM = _bpm,
								Beat = _beats,
							});
						}
					}
					break;
					case "camera": {



					}
					break;
				}
			}
			return true;
		}



		private static string Data_To_ArcaeaString (List<ArcaeaItem> items, float bpm) {
			var builder = new StringBuilder();
			builder.AppendLine("AudioOffset:0");
			builder.AppendLine("-");
			builder.AppendLine($"timing(0,{bpm.ToString("0.00")},4.00);");
			foreach (var item in items) {
				switch (item) {
					case Note nItem:
						if (nItem.Duration == 0) {
							builder.AppendLine($"({nItem.Time},{nItem.Lane});");
						} else {
							builder.AppendLine($"hold({nItem.Time},{nItem.Time + nItem.Duration},{nItem.Lane});");
						}
						break;
					case Timing tItem:
						builder.AppendLine($"timing({tItem.Time},{tItem.BPM.ToString("0.00")},{tItem.Beat.ToString("0.00")});");
						break;
					case Arc aItem:
						var strArc = $"arc({aItem.Time},{aItem.TimeEnd},{aItem.XStart.ToString("0.00")},{aItem.XEnd.ToString("0.00")},{aItem.Ease},{aItem.YStart.ToString("0.00")},{aItem.YEnd.ToString("0.00")},{aItem.Color},{aItem.Fx},{aItem.Skyline})";
						if (aItem.Arctap == null || aItem.Arctap.Count == 0) {
							builder.AppendLine($"{strArc};");
						} else {
							var strArctap = new StringBuilder();
							for (int _i = 0; _i < aItem.Arctap.Count; _i++) {
								strArctap.Append($"arctap({aItem.Arctap[_i]})");
								if (_i < aItem.Arctap.Count - 1) {
									strArctap.Append(',');
								}
							}
							builder.AppendLine($"{strArc}[{strArctap.ToString()}];");
						}
						break;
				}
			}
			return builder.ToString();
		}



	}
}