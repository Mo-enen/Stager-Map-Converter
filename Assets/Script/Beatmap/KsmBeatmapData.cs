namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using UnityEngine;


	public class KsmBeatmapData {




		private class ChartData {

			private const string LAZER_STR = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmno";
			private readonly static FxType[] STAGER_TO_FX = new FxType[14] {
				FxType.None,
				FxType.Retrigger,
				FxType.Gate,
				FxType.Echo,
				FxType.SideChain,
				FxType.BitCrusher,
				FxType.Phaser,
				FxType.TapeStop,
				FxType.Flanger,
				FxType.PitchShift,
				FxType.None,
				FxType.None,
				FxType.None,
				FxType.Wobble,
			};

			public int BT0 = 0; // 0:None  1:Chip  2:Long
			public int BT1 = 0;
			public int BT2 = 0;
			public int BT3 = 0;
			public int FX0 = 0; // 0:None  1:Chip  2:Long
			public int FX1 = 0;
			public int Lazer0 = -1; // 0-62:l-r   -1:none   -2:liner
			public int Lazer1 = -1;
			public FxType FxType0 = FxType.None;
			public FxType FxType1 = FxType.None;
			public int FxParam0 = -1;
			public int FxParam1 = -1;

			public int this[int type, int index] {
				get {
					switch (type) {
						default:
							return -1;
						case 0: // BT
							return index == 0 ? BT0 : index == 1 ? BT1 : index == 2 ? BT2 : BT3;
						case 1: // FX
							return index == 0 ? FX0 : FX1;
						case 2: // LZ
							return index == 0 ? Lazer0 : Lazer1;
						case 3: // FxType
							return index == 0 ? (int)FxType0 : (int)FxType1;
						case 4: // FxParam
							return index == 0 ? FxParam0 : FxParam1;
					}
				}
				set {
					switch (type) {
						case 0: // BT
							if (index == 0) {
								BT0 = value;
							} else if (index == 1) {
								BT1 = value;
							} else if (index == 2) {
								BT2 = value;
							} else if (index == 3) {
								BT3 = value;
							}
							break;
						case 1: // FX
							if (index == 0) {
								FX0 = value;
							} else if (index == 1) {
								FX1 = value;
							}
							break;
						case 2: // LZ
							if (index == 0) {
								Lazer0 = value;
							} else if (index == 1) {
								Lazer1 = value;
							}
							break;
						case 3: // FxType
							if (index == 0) {
								FxType0 = (FxType)value;
							} else if (index == 1) {
								FxType1 = (FxType)value;
							}
							break;
						case 4: // FxParam
							if (index == 0) {
								FxParam0 = value;
							} else if (index == 1) {
								FxParam1 = value;
							}
							break;
					}
				}
			}
			public float Lazer0_01 => Util.Remap(0f, 51f, 0f, 1f, Lazer0);
			public float Lazer1_01 => Util.Remap(0f, 51f, 0f, 1f, Lazer1);

			public static bool Same (ChartData a, ChartData b) =>
				a.BT0 == b.BT0 && a.BT1 == b.BT1 && a.BT2 == b.BT2 && a.BT3 == b.BT3 &&
				a.FX0 == b.FX0 && a.FX1 == b.FX1 &&
				a.Lazer0 == b.Lazer0 && a.Lazer1 == b.Lazer1;

			public static int Char_to_Int_BT (char bt) => bt == '0' ? 0 : bt == '1' ? 1 : 2;
			public static int Char_to_Int_FX (char fx) => fx == '0' ? 0 : fx == '2' ? 1 : 2;
			public static int Char_to_Int_LAZER (char lz) {
				switch (lz) {
					case '-':
						return -1;
					case ':':
						return -2;
					case var _ when lz >= '0' && lz <= '9':
						return lz - '0';
					case var _ when lz >= 'A' && lz <= 'Z':
						return lz - 'A' + 10;
					case var _ when lz >= 'a' && lz <= 'o':
						return lz - 'a' + 10 + 26;
				}
				return 0;
			}
			public static char Int_to_Char_LAZER (int lz) => lz >= 0 && lz < LAZER_STR.Length ? LAZER_STR[lz] : lz == -2 ? ':' : '-';

			public static int Fx_to_StagerFxType (int fx) {
				switch ((FxType)fx) {
					default:
					case FxType.None:
						return 0;
					case FxType.Retrigger:
						return 1;
					case FxType.Gate:
						return 2;
					case FxType.Flanger:
						return 8;
					case FxType.PitchShift:
						return 9;
					case FxType.BitCrusher:
						return 5;
					case FxType.Phaser:
						return 6;
					case FxType.Wobble:
						return 13;
					case FxType.TapeStop:
						return 7;
					case FxType.Echo:
						return 3;
					case FxType.SideChain:
						return 4;
				}
			}
			public static int StagerFxType_to_Fx (int stagerFx) => (int)(stagerFx >= 0 && stagerFx < STAGER_TO_FX.Length ? STAGER_TO_FX[stagerFx] : FxType.None);

			public static int Param_to_StagerParam (int type, int param) {
				switch ((FxType)type) {
					default:
					case FxType.None:
					case FxType.Flanger:
					case FxType.Phaser:
					case FxType.TapeStop:
						param = 0;
						break;
					case FxType.Retrigger:
					case FxType.Gate:
					case FxType.Echo:
					case FxType.BitCrusher:
						break;
					case FxType.PitchShift:
						param = (int)(param / 0.12f);
						break;
					case FxType.Wobble:
						param *= 10;
						break;
					case FxType.SideChain:
						param = 25;
						break;
				}
				return param;
			}
			public static int StagerParam_to_Param (int stype, int sParam) {
				int type = StagerFxType_to_Fx(stype);
				int param = 0;
				switch ((FxType)type) {
					default:
					case FxType.None:
					case FxType.Flanger:
					case FxType.Phaser:
					case FxType.TapeStop:
						param = 0;
						break;
					case FxType.Retrigger:
					case FxType.Gate:
					case FxType.Echo:
					case FxType.BitCrusher:
						break;
					case FxType.PitchShift:
						param = (int)(sParam * 0.12f);
						break;
					case FxType.Wobble:
						param /= 10;
						break;
					case FxType.SideChain:
						param = 25;
						break;
				}
				return param;
			}
		}



		private enum NoteType {
			BtChip = 0,
			BtLong = 1,
			FxChip = 2,
			FxLong = 3,
			LazerBlue = 4,
			LazerPink = 5,
		}


		private enum FxType {
			None,
			Retrigger,
			Gate,
			Flanger,
			PitchShift,
			BitCrusher,
			Phaser,
			Wobble,
			TapeStop,
			Echo,
			SideChain,
		}


		public static Beatmap KSM_to_Stager (string kMap) {
			if (string.IsNullOrEmpty(kMap)) { return null; }
			var sMap = new Beatmap() {
				CreatedTime = System.DateTime.Now.Ticks,
				BPM = -1,
				Ratio = 1.5f,
				Shift = 0f,
				Level = 1,
				Tag = "KShootMania",
				Stages = {
					new Beatmap.Stage() {
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
					}, new Beatmap.Stage() {
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
					}, new Beatmap.Stage() {
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
					// 0
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
					// 1
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
						Widths = { },
					},
					// 2
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
						Widths = { },
					},
					// 3
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
						Widths = { },
					},
					// 4
					new Beatmap.Track(){
						X = 1f / 4f,
						Width = 0.5f,
						Angle = 45f,
						StageIndex = 1,
						Time = 0f,
						Duration = float.MaxValue,
						Color = 0,
						HasTray = false,
						Colors = { },
						Xs = { },
						Angles = { },
						Widths = { },
					},
					// 5
					new Beatmap.Track(){
						X = 3f / 4f,
						Width = 0.5f,
						Angle = 45f,
						StageIndex = 1,
						Time = 0f,
						Duration = float.MaxValue,
						Color = 0,
						HasTray = false,
						Colors = { },
						Xs = { },
						Angles = { },
						Widths = { },
					},
					// 6
					new Beatmap.Track(){
						X = 0.5f,
						Width = 1f,
						Angle = 45f,
						StageIndex = 2,
						Time = 0f,
						Duration = float.MaxValue,
						Color = 1,
						HasTray = false,
						Colors = { },
						Xs = { },
						Angles = { },
						Widths = { },
					},
				},
				Notes = { },
				Timings = { new Beatmap.Timing(0f, 1f) },
			};
			// String >> Cache
			float timeOffset = 0f;
			float currentBPM = 120f;
			var strs = kMap.Replace("\r\n", "\n").Split('\n');
			int index = 0;
			// Header
			for (; index < strs.Length; index++) {
				var str = strs[index];
				if (str.StartsWith("--")) { break; }
				switch (str) {
					case var _ when str.StartsWith("//"):
						break;
					case var _ when str.StartsWith("t="):
						if (str.IndexOf('-') < 0) {
							if (float.TryParse(str.Substring(2), out float _bpm)) {
								currentBPM = _bpm;
								if (sMap.BPM < 0) {
									sMap.BPM = (int)_bpm;
								}
							}
						}
						break;
					case var _ when str.StartsWith("o="):
						if (int.TryParse(str.Substring(2), out int _offset)) {
							timeOffset = _offset / -1000f;
						}
						break;
					case var _ when str.StartsWith("level="):
						if (int.TryParse(str.Substring(6), out int _level)) {
							sMap.Level = _level;
						}
						break;
				}
			}
			// Chart
			float nextBeatAmount = 1f;
			var chartCaches = new List<ChartData>();
			float cacheTimeStart = 0f;
			var longBtStarts = new float[4] { -1f, -1f, -1f, -1f };
			var longFxStarts = new float[2] { -1f, -1f };
			var longSfxIndexs = new byte[2] { 0, 0 };
			var longSfxParams = new int[2] { 0, 0 };
			var prevLzIndex = new int[2] { -1, -1 };
			var fxTypes = new FxType[2] { FxType.None, FxType.None };
			var fxParams = new int[2] { -1, -1 };
			for (; index < strs.Length; index++) {
				var str = strs[index];
				switch (str) {
					case var _ when str.StartsWith("//"):
						break;
					case var _ when str.StartsWith("t="):
						if (str.IndexOf('-') < 0) {
							if (float.TryParse(str.Substring(2), out float _bpm)) {
								currentBPM = _bpm;
								if (sMap.BPM < 0) {
									sMap.BPM = (int)_bpm;
								}
							}
						}
						break;
					case var _ when str.StartsWith("beat="):
						var str2 = str.Substring(5).Split('/');
						if (str2 != null && str2.Length >= 2 && int.TryParse(str2[0], out int _b0) && int.TryParse(str2[1], out int _b1)) {
							nextBeatAmount = (float)_b0 / _b1;
						}
						break;
					case var _ when str.Length >= 10 && str[4] == '|' && str[7] == '|':
						chartCaches.Add(new ChartData() {
							BT0 = ChartData.Char_to_Int_BT(str[0]),
							BT1 = ChartData.Char_to_Int_BT(str[1]),
							BT2 = ChartData.Char_to_Int_BT(str[2]),
							BT3 = ChartData.Char_to_Int_BT(str[3]),
							FX0 = ChartData.Char_to_Int_FX(str[5]),
							FX1 = ChartData.Char_to_Int_FX(str[6]),
							Lazer0 = ChartData.Char_to_Int_LAZER(str[8]),
							Lazer1 = ChartData.Char_to_Int_LAZER(str[9]),
							FxType0 = fxTypes[0],
							FxType1 = fxTypes[1],
							FxParam0 = fxParams[0],
							FxParam1 = fxParams[1],
						});
						fxTypes[0] = fxTypes[1] = FxType.None;
						fxParams[0] = fxParams[1] = -1;
						break;
					case var _ when str.StartsWith("fx-l="):
					case var _ when str.StartsWith("fx-r="):
						var _str = str.Substring(5);
						int lrIndex = str[3] == 'l' ? 0 : 1;
						switch (_str) {
							default:
								fxTypes[lrIndex] = FxType.None;
								fxParams[lrIndex] = -1;
								break;
							case var _ when _str.StartsWith("Retrigger;"):
								fxTypes[lrIndex] = FxType.Retrigger;
								if (!int.TryParse(_str.Substring(10), out fxParams[lrIndex])) {
									fxParams[lrIndex] = 8;
								}
								break;
							case var _ when _str.StartsWith("Gate;"):
								fxTypes[lrIndex] = FxType.Gate;
								if (!int.TryParse(_str.Substring(5), out fxParams[lrIndex])) {
									fxParams[lrIndex] = 4;
								}
								break;
							case var _ when _str.StartsWith("Flanger"):
								fxTypes[lrIndex] = FxType.Flanger;
								fxParams[lrIndex] = -1;
								break;
							case var _ when _str.StartsWith("PitchShift;"):
								fxTypes[lrIndex] = FxType.PitchShift;
								if (!int.TryParse(_str.Substring(11), out fxParams[lrIndex])) {
									fxParams[lrIndex] = 12;
								}
								break;
							case var _ when _str.StartsWith("BitCrusher;"):
								fxTypes[lrIndex] = FxType.BitCrusher;
								if (!int.TryParse(_str.Substring(11), out fxParams[lrIndex])) {
									fxParams[lrIndex] = 5;
								}
								break;
							case var _ when _str.StartsWith("Phaser"):
								fxTypes[lrIndex] = FxType.Phaser;
								fxParams[lrIndex] = -1;
								break;
							case var _ when _str.StartsWith("Wobble;"):
								fxTypes[lrIndex] = FxType.Wobble;
								if (!int.TryParse(_str.Substring(7), out fxParams[lrIndex])) {
									fxParams[lrIndex] = 12;
								}
								break;
							case var _ when _str.StartsWith("TapeStop;"):
								fxTypes[lrIndex] = FxType.TapeStop;
								if (!int.TryParse(_str.Substring(9), out fxParams[lrIndex])) {
									fxParams[lrIndex] = 50;
								}
								break;
							case var _ when _str.StartsWith("Echo;"):
								fxTypes[lrIndex] = FxType.Echo;
								if (!int.TryParse(_str.Substring(5), out fxParams[lrIndex])) {
									fxParams[lrIndex] = 4;
								}
								break;
							case var _ when _str.StartsWith("SideChain"):
								fxTypes[lrIndex] = FxType.SideChain;
								fxParams[lrIndex] = -1;
								break;
						}
						break;

					case var _ when str.StartsWith("--"):
						if (chartCaches.Count == 0) { break; }
						float cacheDuration = 60f / currentBPM * 4f * nextBeatAmount;
						for (int i = 0; i < chartCaches.Count; i++) {
							var cache = chartCaches[i];
							float time = cacheTimeStart + i / (chartCaches.Count - 1f) * cacheDuration + timeOffset;
							// BT
							for (int trackIndex = 0; trackIndex < 4; trackIndex++) {
								int bt = cache[0, trackIndex];
								bool tryEndLong = bt != 2 || i == chartCaches.Count - 1;
								if (bt == 1) {
									// BT Chip
									sMap.Notes.Add(new Beatmap.Note() {
										Time = time,
										X = 0.5f,
										Z = 0f,
										Width = 1f,
										ItemType = (int)NoteType.BtChip,
										ClickSoundIndex = 0,
										Duration = 0,
										LinkedNoteIndex = -1,
										TrackIndex = trackIndex,
									});
								}
								if (bt == 2) {
									// Start Long
									if (longBtStarts[trackIndex] < 0f) {
										longBtStarts[trackIndex] = time;
									}
								}
								// End Long
								if (tryEndLong && longBtStarts[trackIndex] >= 0f) {
									sMap.Notes.Add(new Beatmap.Note() {
										Time = longBtStarts[trackIndex],
										X = 0.5f,
										Z = 0f,
										Width = 1f,
										ItemType = (int)NoteType.BtLong,
										ClickSoundIndex = 0,
										Duration = time - longBtStarts[trackIndex],
										LinkedNoteIndex = -1,
										TrackIndex = trackIndex,
									});
									longBtStarts[trackIndex] = -1f;
								}
							}
							// FX
							for (int trackIndex = 0; trackIndex < 2; trackIndex++) {
								int fx = cache[1, trackIndex];
								bool tryEndLong = fx != 2 || i == chartCaches.Count - 1;
								if (fx == 1) {
									// FX Chip
									sMap.Notes.Add(new Beatmap.Note() {
										Time = time,
										X = 0.5f,
										Z = 0f,
										Width = 1f,
										ItemType = (int)NoteType.FxChip,
										ClickSoundIndex = 0,
										Duration = 0,
										LinkedNoteIndex = -1,
										TrackIndex = trackIndex,
									});
								}
								if (fx == 2) {
									// Start FX Long
									if (longFxStarts[trackIndex] < 0f) {
										longFxStarts[trackIndex] = time;
										longSfxIndexs[trackIndex] = (byte)ChartData.Fx_to_StagerFxType(cache[3, trackIndex]);
										longSfxParams[trackIndex] = ChartData.Param_to_StagerParam(
											cache[3, trackIndex], cache[4, trackIndex]
										);
									}
								}
								// End Long
								if (tryEndLong && longFxStarts[trackIndex] >= 0f) {
									sMap.Notes.Add(new Beatmap.Note() {
										Time = longFxStarts[trackIndex],
										X = 0.5f,
										Z = 0f,
										Width = 1f,
										ItemType = (int)NoteType.FxLong,
										ClickSoundIndex = 0,
										Duration = time - longFxStarts[trackIndex],
										LinkedNoteIndex = -1,
										TrackIndex = trackIndex + 4,
										SoundFxIndex = longSfxIndexs[trackIndex],
										SoundFxParamA = longSfxParams[trackIndex],
									});
									longFxStarts[trackIndex] = -1f;
								}
							}
							// LZ
							for (int trackIndex = 0; trackIndex < 2; trackIndex++) {
								var lz = cache[2, trackIndex];
								if (lz >= 0) {
									// New
									int noteIndex = sMap.Notes.Count;
									sMap.Notes.Add(new Beatmap.Note() {
										Time = time,
										X = trackIndex == 0 ? cache.Lazer0_01 : cache.Lazer1_01,
										Z = 0f,
										Width = 0.2f,
										ItemType = (int)NoteType.LazerBlue + trackIndex,
										ClickSoundIndex = 0,
										Duration = 0,
										LinkedNoteIndex = -1,
										TrackIndex = 6,
									});
									if (prevLzIndex[trackIndex] >= 0) {
										sMap.Notes[prevLzIndex[trackIndex]].LinkedNoteIndex = noteIndex;
									}
									prevLzIndex[trackIndex] = noteIndex;
								}
								if (lz == -1) {
									// End
									prevLzIndex[trackIndex] = -1;
								}
							}
						}
						cacheTimeStart += cacheDuration;
						chartCaches.Clear();
						break;
				}
			}
			// Final
			return sMap;
		}



		public static string Stager_to_KSM (Beatmap sMap, string title) {

			sMap.SortNotesByTime();

			// Stager >> Charts
			float lastNoteTime = 0f;
			foreach (var note in sMap.Notes) {
				lastNoteTime = Mathf.Max(note.Time + note.Duration, lastNoteTime);
			}
			float barDuration = 60f / sMap.BPM / 4f;
			int barCount = Mathf.CeilToInt(lastNoteTime / barDuration);
			var charts = new ChartData[barCount * 32];
			for (int i = 0; i < charts.Length; i++) {
				charts[i] = new ChartData();
			}
			foreach (var note in sMap.Notes) {
				int barIndex0 = Mathf.FloorToInt(note.Time / barDuration * 32f);
				int barIndex1 = Mathf.FloorToInt((note.Time + note.Duration) / barDuration * 32f);
				for (int _b = barIndex0; _b <= barIndex1; _b++) {
					if (_b < 0 || _b >= charts.Length) { continue; }
					var chart = charts[_b];
					if (note.TrackIndex < 4) {
						// BT
						chart[0, note.TrackIndex] = barIndex0 == barIndex1 ? 1 : 2;
					} else if (note.TrackIndex < 6) {
						// FX
						chart[1, note.TrackIndex - 4] = barIndex0 == barIndex1 ? 1 : 2;
						if (_b == barIndex0) {
							chart[3, note.TrackIndex - 4] = ChartData.StagerFxType_to_Fx(note.SoundFxIndex);
							chart[4, note.TrackIndex - 4] = ChartData.StagerParam_to_Param(note.SoundFxIndex, note.SoundFxParamA);
						}
					} else if (note.TrackIndex == 6 && _b == barIndex0) {
						if (note.ItemType == 4 || note.ItemType == 5) {
							// LZ
							chart[2, note.ItemType - 4] = (int)Util.Remap(0f, 1f, 0, 51, Mathf.Clamp01(note.X));
							if (note.LinkedNoteIndex >= 0 && note.LinkedNoteIndex < sMap.Notes.Count) {
								var nextNote = sMap.Notes[note.LinkedNoteIndex];
								int barIndex_Next = Mathf.FloorToInt(nextNote.Time / barDuration * 32f);
								if (barIndex_Next >= 0 && barIndex_Next < charts.Length) {
									for (int i = _b + 1; i < barIndex_Next; i++) {
										charts[i][2, note.ItemType - 4] = -2;
									}
								}
							}
						}
					}



				}

			}

			// Charts >> Fixed Charts
			for (int i = 0; i < barCount; i++) {
				bool allSame = true;
				for (int j = 0; j < 31; j++) {
					if (!ChartData.Same(charts[i * 32 + j], charts[i * 32 + j + 1])) {
						allSame = false;
						break;
					}
				}
				if (allSame) {
					for (int j = 1; j < 32; j++) {
						charts[i * 32 + j] = null;
					}
				}
			}

			// Fixed Charts >> String
			var builder = new StringBuilder();
			builder.AppendLine($"title={title}");
			builder.AppendLine($"jacket=bg0.png");
			builder.AppendLine($"difficulty=infinite");
			builder.AppendLine($"level={sMap.Level}");
			builder.AppendLine($"t={sMap.BPM}");
			builder.AppendLine($"m=bgm.ogg");
			builder.AppendLine($"o=0");
			builder.AppendLine($"bg=bg1.png");
			builder.AppendLine($"layer=layer.gif");
			builder.AppendLine($"po=0");
			builder.AppendLine($"filtertype=peak");
			builder.AppendLine($"chokkakuautovol=0");
			builder.AppendLine($"chokkakuvol=50");
			for (int index = 0; index < charts.Length; index++) {
				// Bar Line
				if (index % 32 == 0) {
					builder.AppendLine($"--");
					if (index == 0) {
						builder.AppendLine($"beat=4/4");
					}
				}
				var chart = charts[index];
				if (chart == null) { continue; }
				// FX
				if (chart.FxType0 != FxType.None) {
					builder.Append($"fx-l={chart.FxType0.ToString()};");
					if (chart.FxParam0 >= 0) {
						builder.Append(chart.FxParam0.ToString());
					}
					builder.AppendLine();
				}
				if (chart.FxType1 != FxType.None) {
					builder.Append($"fx-r={chart.FxType1.ToString()};");
					if (chart.FxParam1 >= 0) {
						builder.Append(chart.FxParam1.ToString());
					}
					builder.AppendLine();
				}
				// Chart
				for (int i = 0; i < 4; i++) {
					int bt = chart[0, i];
					builder.Append(bt == 0 ? '0' : bt == 1 ? '1' : '2');
				}
				builder.Append('|');
				for (int i = 0; i < 2; i++) {
					int fx = chart[1, i];
					builder.Append(fx == 0 ? '0' : fx == 1 ? '2' : '1');
				}
				builder.Append('|');
				for (int i = 0; i < 2; i++) {
					int lz = chart[2, i];
					builder.Append(ChartData.Int_to_Char_LAZER(lz));
				}
				builder.AppendLine();
			}
			builder.AppendLine($"--");
			return builder.ToString();
		}





	}
}