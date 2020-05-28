namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using UnityEngine;


	public class BmsBeatmapData : MonoBehaviour {


		// SUB
		private class NoteListComparer : IComparer<(int, List<int>, List<int>)> {
			public int Compare ((int, List<int>, List<int>) x, (int, List<int>, List<int>) y) =>
				x.Item1.CompareTo(y.Item1);
		}


		// API
		public static Beatmap Bms_To_Stager (string bMap) {

			if (string.IsNullOrEmpty(bMap)) { return null; }

			// Stager
			var sMap = new Beatmap() {
				Ratio = 1.5f,
				Shift = 0f,
				Tag = "BMS",
				BPM = 130,
				Level = 1,
				CreatedTime = System.DateTime.Now.Ticks,
				Stages = new List<Beatmap.Stage>() { new Beatmap.Stage(){
					Duration = float.MaxValue,
					Height = 0.6666f,
					Rotation = 0f,
					Speed = 1f,
					Time = 0f,
					Width = 0.4375f,
					X = 0.5f,
					Y = 0f,
					Positions = { },
					Heights = { },
					Rotations = { },
					Widths = { },
				}},
				Tracks = { },
				Notes = { },
				Timings = { },
			};
			// Tracks
			for (int i = 0; i < 7; i++) {
				sMap.Tracks.Add(new Beatmap.Track() {
					Width = 1f / 7f,
					X = Util.Remap(0f, 6f, 0.5f / 7f, 1f - 0.5f / 7f, i),
					Time = 0f,
					Duration = float.MaxValue,
					StageIndex = 0,
					Color = 2 - i % 2,
					Angle = 0f,
					HasTray = false,
					Speed = 1f,
					Colors = { },
					Widths = { },
					Xs = { },
					Angles = { },
				});
			}
			sMap.Tracks.Add(new Beatmap.Track() {
				Width = 1f / 6f,
				X = -0.5f / 6f,
				Time = 0f,
				Duration = float.MaxValue,
				StageIndex = 0,
				Color = 3,
				Angle = 0f,
				HasTray = false,
				Speed = 1f,
				Colors = { },
				Widths = { },
				Xs = { },
				Angles = { },
			});

			// BMS
			int sectionCount = 0;
			var bpmMap = new Dictionary<string, float>();
			var beatPerSectionMap = new Dictionary<int, float>();
			var notes = new List<(int section, List<int> noteIndexs, List<int> holdIndexs)>();
			var bpms = new List<(int section, List<float> bpms)>();
			var prevAdded = new (int notesIndex, int nsIndex)[8];
			int lntType = 1;
			string lnObj = "";
			var strs = bMap.Replace("\r", "").Split('\n');
			foreach (var str in strs) {
				if (string.IsNullOrEmpty(str) || str[0] != '#') { continue; }
				if (str.StartsWith("#BPM")) {
					// BPM
					if (str.Length > 5) {
						if (str[4] == ' ') {
							if (float.TryParse(str.Substring(5, str.Length - 5), out float bpm)) {
								sMap.BPM = Mathf.RoundToInt(bpm);
							}
						} else if (str.Length > 7 && str[6] == ' ') {
							var key = str.Substring(4, 2);
							if (!bpmMap.ContainsKey(key) && float.TryParse(str.Substring(7, str.Length - 7), out float bpm)) {
								bpmMap.Add(key, bpm);
							}
						}
					}
				} else if (str.StartsWith("#PLAYLEVEL")) {
					// LEVEL
					if (float.TryParse(str.Substring(11, str.Length - 11), out float level)) {
						sMap.Level = Mathf.RoundToInt(level);
					}
				} else if (str.StartsWith("#LNTYPE")) {
					// LnType
					int.TryParse(str.Substring(8, str.Length - 8), out lntType);
				} else if (str.StartsWith("#LNOBJ")) {
					// LnObj
					lntType = 2;
					if (str.Length >= 9) {
						lnObj = str.Substring(7, 2);
					}
				} else if (
					str.Length > 7 &&
					int.TryParse(str.Substring(1, 3), out int section) &&
					int.TryParse(str.Substring(4, 2), out int channel)
				) {
					sectionCount = Mathf.Max(sectionCount, section + 1);
					// Notes
					string content = str.Substring(7, str.Length - 7);
					switch (channel) {
						case 2: { // Beat
								if (float.TryParse(content, out float beat) && !beatPerSectionMap.ContainsKey(section)) {
									beatPerSectionMap.Add(section, beat);
								}
							}
							break;
						case 3:
						case 8: // BPM
							var bs = new List<float>();
							for (int i = 0; i < content.Length - 1; i += 2) {
								string con = content.Substring(i, 2);
								if (con == "00") {
									bs.Add(-1f);
								} else if (bpmMap.ContainsKey(con)) {
									bs.Add(bpmMap[con]);
								} else {
									try {
										int bpm_Int = System.Convert.ToInt32(con, 16);
										bs.Add(bpm_Int);
									} catch { }
								}
							}
							bpms.Add((section, bs));
							break;
						case 11: // 1
						case 12: // 2
						case 13: // 3
						case 14: // 4
						case 15: // 5
						case 18: // 6
						case 19: // 7
						case 16: // Dish
						case 21: // 1
						case 22: // 2
						case 23: // 3
						case 24: // 4
						case 25: // 5
						case 28: // 6
						case 29: // 7
						case 26: { // Dish
								int tIndex = Channel_to_Track(channel);
								if (tIndex < 0) { break; }
								var ns = new List<int>();
								var hs = new List<int>();
								for (int i = 0; i < content.Length - 1; i += 2) {
									string con = content.Substring(i, 2);
									if (con == "00") {
										ns.Add(-1);
										hs.Add(-1);
									} else if (lntType == 2 && con == lnObj) {
										var (prevNoteIndex, prevNsIndex) = prevAdded[tIndex];
										if (prevNoteIndex < notes.Count) {
											notes[prevNoteIndex].noteIndexs[prevNsIndex] = -1;
											notes[prevNoteIndex].holdIndexs[prevNsIndex] = tIndex;
										} else {
											ns[prevNsIndex] = -1;
											hs[prevNsIndex] = tIndex;
										}
										ns.Add(-1);
										hs.Add(tIndex);
									} else {
										prevAdded[tIndex] = (notes.Count, ns.Count);
										ns.Add(tIndex);
										hs.Add(-1);
									}
								}
								notes.Add((section, ns, hs));
							}
							break;
						case 51: // 1
						case 52: // 2
						case 53: // 3
						case 54: // 4
						case 55: // 5
						case 58: // 6
						case 59: // 7
						case 56: // Dish
						case 61: // 1
						case 62: // 2
						case 63: // 3
						case 64: // 4
						case 65: // 5
						case 68: // 6
						case 69: // 7
						case 66: { // Dish
								if (lntType != 1) { break; }
								int tIndex = Channel_to_Track(channel);
								if (tIndex < 0) { break; }
								var ns = new List<int>();
								var hs = new List<int>();
								for (int i = 0; i < content.Length - 1; i += 2) {
									string con = content.Substring(i, 2);
									if (con == "00") {
										ns.Add(-1);
										hs.Add(-1);
									} else {
										ns.Add(-1);
										hs.Add(tIndex);
									}
								}
								notes.Add((section, ns, hs));
							}
							break;
					}
				}
			}
			notes.Sort(new NoteListComparer());

			// Section Time
			var sectionTimeDurations = new (float time, float duration)[sectionCount];
			if (sectionCount > 0) {
				sectionTimeDurations[0] = (0f, 0f);
			}
			float beatDuration = 60f / sMap.BPM;
			for (int section = 0; section < sectionCount; section++) {
				float beatPerSection = 1f;
				if (beatPerSectionMap.ContainsKey(section)) {
					beatPerSection = beatPerSectionMap[section];
				}
				float sectionDuration = section != 0 ? beatDuration * 4f * beatPerSection : 0f;
				sectionTimeDurations[section].duration = sectionDuration;
				if (section < sectionCount - 1) {
					sectionTimeDurations[section + 1].time = sectionTimeDurations[section].time + sectionDuration;
				}
			}

			// Hold Duration Map
			var holdDurationMap = new Dictionary<(int notesIndex, int index), float>();
			var holdEndHash = new HashSet<(int notesIndex, int index)>();
			for (int i = 0; i < notes.Count; i++) {
				var (section, _, holds) = notes[i];
				float sectionTime = sectionTimeDurations[section].time;
				float timeGap = sectionTimeDurations[section].duration / holds.Count;
				for (int j = 0; j < holds.Count; j++) {
					var hold = holds[j];
					if (hold < 0) { continue; }
					var key = (i, j);
					if (!holdDurationMap.ContainsKey(key) && !holdEndHash.Contains(key)) {
						var (nextSection, nextIndex, nextNotesIndex) = GetNextHoldNote(i, section, j);
						if (nextSection >= 0 && nextIndex >= 0) {
							float nextSectionTime = sectionTimeDurations[nextSection].time;
							float nextTimeGap = sectionTimeDurations[nextSection].duration / notes[nextNotesIndex].holdIndexs.Count;
							float time = sectionTime + j * timeGap;
							float nextTime = nextSectionTime + nextIndex * nextTimeGap;
							holdDurationMap.Add(key, nextTime - time);
							// End Hash
							if (!holdEndHash.Contains((nextNotesIndex, nextIndex))) {
								holdEndHash.Add((nextNotesIndex, nextIndex));
							}
						}
					}
				}
			}

			// Notes
			for (int i = 0; i < notes.Count; i++) {
				var (section, taps, holds) = notes[i];
				float sectionTime = sectionTimeDurations[section].time;
				float timeGap = sectionTimeDurations[section].duration / taps.Count;
				// Taps
				for (int j = 0; j < taps.Count; j++) {
					var tap = taps[j];
					if (tap < 0) { continue; }
					sMap.Notes.Add(new Beatmap.Note() {
						Time = sectionTime + j * timeGap,
						TrackIndex = tap,
						Duration = 0f,
						ClickSoundIndex = 0,
						LinkedNoteIndex = -1,
						SoundFxIndex = 0,
						SoundFxParamA = 0,
						SoundFxParamB = 0,
						Speed = 1f,
						ItemType = 0,
						X = 0.5f,
						Width = 1f,
						Z = 0f,
					});
				}
				// Holds
				for (int j = 0; j < holds.Count; j++) {
					var hold = holds[j];
					if (hold < 0) { continue; }
					var key = (i, j);
					if (holdEndHash.Contains(key)) { continue; }
					sMap.Notes.Add(new Beatmap.Note() {
						Time = sectionTime + j * timeGap,
						TrackIndex = hold,
						Duration = holdDurationMap.ContainsKey(key) ? holdDurationMap[key] : 0f,
						ClickSoundIndex = 0,
						LinkedNoteIndex = -1,
						SoundFxIndex = 0,
						SoundFxParamA = 0,
						SoundFxParamB = 0,
						ItemType = 0,
						Speed = 1f,
						X = 0.5f,
						Width = 1f,
						Z = 0f,
					});
				}
			}

			// Fianl
			return sMap;

			// === Func ===
			(int section, int index, int notesIndex) GetNextHoldNote (int notesIndex, int sectionIndex, int index) {
				int trackIndex = notes[notesIndex].holdIndexs[index];
				for (int i = 0; i < notes.Count; i++) {
					var (section, _, hs) = notes[i];
					if (section < sectionIndex) { continue; }
					for (int j = section > sectionIndex ? 0 : index + 1; j < hs.Count; j++) {
						if (hs[j] == trackIndex) {
							return (section, j, i);
						}
					}
				}
				return (-1, -1, -1);
			}
		}


		public static string Stager_To_Bms (Beatmap sMap, string title) {

			if (sMap == null || sMap.Notes == null || sMap.Notes.Count == 0) { return ""; }
			sMap.SortNotesByTime();

			float maxNoteTime = 0f;
			foreach (var note in sMap.Notes) {
				maxNoteTime = Mathf.Max(maxNoteTime, note.Time);
			}
			float sectionDuration = 240f / sMap.BPM;
			int sectionCount = Mathf.CeilToInt(maxNoteTime / sectionDuration);

			// Init Sections
			var sections = new (bool[,] noteTracks, bool[,] holdTracks)[sectionCount];
			for (int i = 0; i < sectionCount; i++) {
				var noteTracks = new bool[32, 8];
				var holdTracks = new bool[32, 8];
				for (int x = 0; x < 32; x++) {
					for (int y = 0; y < 8; y++) {
						noteTracks[x, y] = false;
						holdTracks[x, y] = false;
					}
				}
				sections[i] = (noteTracks, holdTracks);
			}

			// Notes >> Section
			for (int i = 0; i < sMap.Notes.Count; i++) {
				var note = sMap.Notes[i];
				int tIndex = Mathf.Clamp(note.TrackIndex, 0, 7);
				int sectionIndex = Mathf.FloorToInt(note.Time / sectionDuration);
				int noteSectionIndex = Mathf.FloorToInt(32f * ((note.Time % sectionDuration) / sectionDuration));
				if (note.duration <= 0) {
					sections[sectionIndex].noteTracks[noteSectionIndex, tIndex] = true;
				} else {
					sections[sectionIndex].holdTracks[noteSectionIndex, tIndex] = true;
					// End
					int endSectionIndex = Mathf.FloorToInt((note.Time + note.Duration) / sectionDuration);
					int endNoteSectionIndex = Mathf.FloorToInt(32f * (((note.Time + note.Duration) % sectionDuration) / sectionDuration));
					sections[endSectionIndex].holdTracks[endNoteSectionIndex, tIndex] = true;
				}
			}

			// Section >> String
			var builder = new StringBuilder();

			builder.AppendLine("*---------------------- HEADER FIELD");
			builder.AppendLine();
			builder.AppendLine("#PLAYER 1");
			builder.AppendLine("#TITLE " + title);
			builder.AppendLine("#BPM " + sMap.BPM.ToString());
			builder.AppendLine("#PLAYLEVEL " + sMap.Level.ToString());
			builder.AppendLine("#RANK 3");
			builder.AppendLine("#STAGEFILE _stagefile.png");
			builder.AppendLine("#BANNER _banner.png");
			builder.AppendLine("#TOTAL 700");
			builder.AppendLine("#WAV01 bgm.wav");
			builder.AppendLine("#WAVAA empty.wav");
			builder.AppendLine("#LNTYPE 1");
			builder.AppendLine();
			builder.AppendLine("*----------------------MAIN DATA FIELD");
			builder.AppendLine();
			builder.AppendLine("#00104:01");

			for (int i = 0; i < sectionCount; i++) {
				var (notes, holds) = sections[i];
				// Taps
				for (int tIndex = 0; tIndex < 8; tIndex++) {
					builder.Append($"#{(i + 1).ToString("000")}{Track_to_Channel(tIndex, false).ToString()}:");
					for (int j = 0; j < 32; j++) {
						builder.Append(notes[j, tIndex] ? "AA" : "00");
					}
					builder.AppendLine();
				}
				// Holds
				for (int tIndex = 0; tIndex < 8; tIndex++) {
					builder.Append($"#{(i + 1).ToString("000")}{Track_to_Channel(tIndex, true).ToString()}:");
					for (int j = 0; j < 32; j++) {
						builder.Append(holds[j, tIndex] ? "AA" : "00");
					}
					builder.AppendLine();
				}
				builder.AppendLine();
			}

			return builder.ToString();
		}


		// LGC
		private static int Channel_to_Track (int _channel) {
			switch (_channel % 10) {
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
					return (_channel % 10) - 1;
				case 8:
				case 9:
					return (_channel % 10) - 3;
				case 6:
					return 7;
			}
			return -1;
		}


		private static int Track_to_Channel (int tIndex, bool hold) {
			switch (tIndex) {
				case 0:
				case 1:
				case 2:
				case 3:
				case 4:
					return hold ? 50 + tIndex + 1 : 10 + tIndex + 1;
				case 5:
				case 6:
					return hold ? 50 + tIndex + 3 : 10 + tIndex + 3;
				case 7:
					return hold ? 56 : 16;
			}
			return -1;
		}


	}
}