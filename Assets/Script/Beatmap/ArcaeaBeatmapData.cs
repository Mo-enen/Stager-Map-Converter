namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public static class ArcaeaBeatmapData {



		public class ArcIntComparer : IComparer<(Arc, int)> {
			public int Compare ((Arc, int) x, (Arc, int) y) => x.Item1.TimeStart.CompareTo(y.Item1.TimeStart);
		}



		public struct Note {
			public int Time;
			public int Duration;
			public int Lane;
		}


		public struct Timing {
			public int Time;
			public float BPM;
			public float Beat;
		}


		public struct Arc {
			public int TimeStart;
			public int TimeEnd;
			public float XStart;
			public float XEnd;
			public string Ease;
			public float YStart;
			public float YEnd;
			public int Color;
			public string Fx;
			public bool Skyline;
			public List<int> Arctap;
		}


		private readonly static List<Note> Notes = new List<Note>();
		private readonly static List<Timing> Timings = new List<Timing>();
		private readonly static List<(Arc arc, bool done)> Arcs = new List<(Arc, bool)>();





		public static Beatmap Arcaea_To_Stager (string aMap) {

			// String >> Data
			if (!ArcaeaString_To_Data(aMap)) { return null; }

			// Data >> map
			var data = new Beatmap() {
				BPM = Timings.Count > 0 ? (int)Timings[0].BPM : 120,
				DropSpeed = 1f,
				Level = 1,
				Ratio = 1.5f,
				Shift = 0f,
				Tag = "Normal",
				Stages = {
					new Beatmap.Stage(){
						X = 0.5f,
						Y = 0f,
						Duration = float.MaxValue,
						Height = 1f,
						Rotation = 0f,
						Speed = 1f,
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
						StageIndex = 0,
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
				SpeedNotes = { },
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
					SwipeX = 1,
					SwipeY = 1,
					Tap = true,
					Width = 1f,
				});
			}

			// Arc Map
			var arcMap = new Dictionary<int, List<(Arc arc, int index)>>();
			for (int i = 0; i < Arcs.Count; i++) {
				var arc = Arcs[i];
				if (!arcMap.ContainsKey(arc.arc.TimeStart)) {
					arcMap.Add(arc.arc.TimeStart, new List<(Arc arc, int index)>());
				}
				arcMap[arc.arc.TimeStart].Add((arc.arc, i));
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
				byte swipeY = (byte)(arc.Ease == "so" ? 2 : arc.Ease == "si" ? 0 : 1);

				// Arc Head
				data.Notes.Add(new Beatmap.Note() {
					X = arc.XStart,
					Time = arc.TimeStart / 1000f,
					Width = 0.05f,
					Z = arc.YStart * 0.2f,
					ClickSoundIndex = 0,
					Comment = skyline ? " " : "",
					Duration = 0f,
					LinkedNoteIndex = data.Notes.Count + 1,
					SwipeX = 1,
					SwipeY = swipeY,
					Tap = true,
					TrackIndex = 4,
				});
				Arcs[i] = (arc, true);

				// Arc List
				for (int safe = 0; safe < Arcs.Count && arcMap.ContainsKey(time); safe++) {

					var list = arcMap[time];

					// Find Closest in List
					int NextIndex = -1;
					Arc? nextArc = null;
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

					// End Current Arc
					data.Notes.Add(new Beatmap.Note() {
						X = x,
						Time = time / 1000f,
						Width = 0.05f,
						Z = y * 0.2f,
						ClickSoundIndex = 0,
						Comment = skyline ? " " : "",
						Duration = 0f,
						LinkedNoteIndex = data.Notes.Count + 1,
						SwipeX = 1,
						SwipeY = swipeY,
						Tap = false,
						TrackIndex = 4,
					});

					Arcs[NextIndex] = (Arcs[NextIndex].arc, true);
					time = nextArc.Value.TimeEnd;
					x = nextArc.Value.XEnd;
					y = nextArc.Value.YEnd;
					swipeY = (byte)(nextArc.Value.Ease == "so" ? 2 : nextArc.Value.Ease == "si" ? 0 : 1);

				}

				// Arc Tail
				data.Notes.Add(new Beatmap.Note() {
					X = x,
					Time = time / 1000f,
					Width = 0.05f,
					Z = y * 0.2f,
					ClickSoundIndex = 0,
					Comment = skyline ? " " : "",
					Duration = 0f,
					LinkedNoteIndex = -1,
					SwipeX = 1,
					SwipeY = 1,
					Tap = false,
					TrackIndex = 4,
				});

			}

			// Speeds
			for (int i = 0; i < Timings.Count; i++) {
				var timing = Timings[i];
				data.SpeedNotes.Add(new Beatmap.SpeedNote(timing.Time / 1000f, timing.BPM / data.BPM));
			}

			// Final
			Notes.Clear();
			Timings.Clear();
			Arcs.Clear();
			return data;
		}



		public static string Stager_To_Arcaea (Beatmap sMap) {
			if (sMap == null) { return ""; }







			return "";
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
			var strs = aMap.Substring(audioOffsetLength, aMap.Length - audioOffsetLength).Replace("\r", "").Replace("-", "").Replace("\n", "").Split(';');
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
								arc.TimeStart = _timeStart + timeOffset;
								arc.TimeEnd = _timeEnd + timeOffset;
								arc.XStart = _xStart;
								arc.XEnd = _xEnd;
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




	}
}