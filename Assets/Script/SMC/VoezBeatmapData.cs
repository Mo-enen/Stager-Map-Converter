namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	[System.Serializable]
	public class VoezBeatmapData {


		private enum NoteType {
			Tap = 0,
			Hold = 1,
			Slide = 2,
			SwipeLeft = 3,
			SwipeRight = 4,
		}


		[System.Serializable]
		public class VoezTrackData {

			[System.Serializable]
			public class MovementItem {
				public float To = 0f;
				public string Ease = "";
				public float Start = 0f;
				public float End = 0f;
			}

			public int Id = -1;
			public bool EntranceOn = false;
			public float X = 0f;
			public bool PositionLock = false;
			public float Size = 1f;
			public float Start = 0f;
			public float End = 0f;
			public int Color = 0;
			public MovementItem[] Move = new MovementItem[0];
			public MovementItem[] Scale = new MovementItem[0];
			public MovementItem[] ColorChange = new MovementItem[0];
		}


		[System.Serializable]
		public class VoezNoteData {
			public int Id = -1;
			public string Type = "";// click swipe hold slide
			public int Track = -1;
			public float Time = -1f;
			public float Hold = 0f;
			public int Dir = 1;
		}


		private static string[] TWEEN_NAMES { get; } = new string[34] {
			"const",

			"easelinear",
			"easeswing",
			"reserved",

			"easeinquad",
			"easeoutquad",
			"easeinoutquad",

			"easeincubic",
			"easeoutcubic",
			"easeinoutcubic",

			"easeinquart",
			"easeoutquart",
			"easeinoutquart",

			"easeinquint",
			"easeoutquint",
			"easeinoutquint",

			"easeinexpo",
			"easeoutexpo",
			"easeinoutexpo",

			"easeinsine",
			"easeoutsine",
			"easeinoutsine",

			"easeincirc",
			"easeoutcirc",
			"easeinoutcirc",

			"easeinelastic",
			"easeoutelastic",
			"easeinoutelastic",

			"easeinback",
			"easeoutback",
			"easeinoutback",

			"easeinbounce",
			"easeoutbounce",
			"easeinoutbounce",
		};


		[SerializeField] private VoezTrackData[] m_Tracks;
		[SerializeField] private VoezNoteData[] m_Notes;


		#region --- API ---


		public static Beatmap VMap_to_SMap (VoezBeatmapData vMap) {
			if (vMap is null) { return null; }
			var data = new Beatmap {
				BPM = 120f,
				Shift = 0f,
				DropSpeed = 1f,
				Level = 1,
				Ratio = 1.5f,
				Tag = "Normal",
				CreatedTime = Util.GetLongTime(),
				SpeedNotes = new List<Beatmap.SpeedNote>(),
				Stages = new List<Beatmap.Stage> {
					new Beatmap.Stage() { // Bottom
						Duration = float.MaxValue,
						Rotation = 0f,
						Speed = 1f,
						Time = 0f,
						Width = 1f,
						Height = 2f / 3f,
						Angle = 0f,
						X = 0.5f,
						Y = 0f,
						Heights ={ },
						Widths = { },
						Positions = { },
						Rotations = { },
						Angles = { },
					}
				},
				Tracks = new List<Beatmap.Track>(),
				Notes = new List<Beatmap.Note>(),
			};

			// Notes
			for (int i = 0; i < vMap.m_Notes.Length; i++) {
				var vNote = vMap.m_Notes[i];
				var type = GetNoteType(vNote.Type, vNote.Dir);
				data.Notes.Add(new Beatmap.Note() {
					TrackIndex = vNote.Track,
					Time = vNote.Time,
					Duration = vNote.Hold,
					Tap = type != NoteType.Slide,
					SwipeX = (byte)(type == NoteType.SwipeLeft ? 0 : type == NoteType.SwipeRight ? 2 : 1),
					X = 0.5f,
					Width = 1f,
					LinkedNoteIndex = -1,
					ClickSoundIndex = 0,
					SwipeY = 1,
				});
			}

			// Tracks
			for (int i = 0; i < vMap.m_Tracks.Length; i++) {
				data.Tracks.Add(new Beatmap.Track());
			}
			for (int i = 0; i < vMap.m_Tracks.Length; i++) {
				var vTrack = vMap.m_Tracks[i];
				int id = vTrack.Id;
				if (id < 0 || id >= data.Tracks.Count) { continue; }
				data.Tracks[id] = new Beatmap.Track() {
					Time = vTrack.Start,
					X = vTrack.X,
					Duration = vTrack.End - vTrack.Start,
					Width = vTrack.Size * 0.1f,
					Color = (byte)vTrack.Color,
					Xs = GetMovementArrayFromVoezData(
						vTrack.Move, vTrack.End, -vTrack.X,
						-vTrack.Start, 0f,
						0.1f, 0.9f
					),
					Widths = GetMovementArrayFromVoezData(
						vTrack.Scale, vTrack.End, 0f,
						-vTrack.Start, vTrack.Size,
						0f, 1f
					),
					Colors = GetColorArrayFromVoezData(
						vTrack.ColorChange, vTrack.End, 0,
						-vTrack.Start, 0,
						0f, 1f
					),
					StageIndex = 0,
					HasTray = false,
				};
			}
			data.SortNotesByTime();
			return data;
		}


		public static VoezBeatmapData SMap_to_VMap (Beatmap sMap) {
			if (sMap is null) { return null; }
			sMap.SortNotesByTime();
			int noteLen = sMap.Notes.Count;
			int trackLen = sMap.Tracks.Count;
			var vMap = new VoezBeatmapData() {
				m_Notes = new VoezNoteData[noteLen],
				m_Tracks = new VoezTrackData[trackLen],
			};
			// Notes
			for (int i = 0; i < noteLen; i++) {
				var note = sMap.Notes[i];
				var type = GetNoteType(note);
				vMap.m_Notes[i] = new VoezNoteData() {
					Id = i,
					Track = note.TrackIndex,
					Type = GetVoezNoteType(type),
					Dir = type == NoteType.SwipeRight ? 1 : 0,
					Hold = type == NoteType.Hold ? note.Duration : 0f,
					Time = note.Time,
				};
			}
			// Tracks
			for (int i = 0; i < trackLen; i++) {
				var sTrack = sMap.Tracks[i];
				for (int j = 0; j < sTrack.Colors.Count; j++) {
					var value = sTrack.Colors[j];
					value.Value = (byte)Mathf.Clamp(sTrack.Colors[j].Value, 0, 10);
					sTrack.Colors[j] = value;
				}
				vMap.m_Tracks[i] = new VoezTrackData() {
					Id = i,
					Start = sTrack.Time,
					End = Mathf.Max(Mathf.Max(
						sTrack.Colors.Count > 0 ? sTrack.Colors[sTrack.Colors.Count - 1].Time : 0,
						sTrack.Xs.Count > 0 ? sTrack.Xs[sTrack.Xs.Count - 1].Time : 0),
						sTrack.Widths.Count > 0 ? sTrack.Widths[sTrack.Widths.Count - 1].Time : 0
					) + sTrack.Time,
					X = Mathf.Clamp01(Util.Remap(0.1f, 0.9f, 0f, 1f, sTrack.X)),
					Size = sTrack.Width,
					Color = sTrack.Color,
					Move = GetVoezMovementArray(sTrack.Xs, sTrack.X, sTrack.Time, 0.1f, 0.9f),
					Scale = GetVoezMovementArray(sTrack.Widths, 0f, sTrack.Time, 0f, 10f),
					ColorChange = GetVoezMovementArray(sTrack.Colors, 0, sTrack.Time, 0f, 1f),
					EntranceOn = true,
					PositionLock = false,
				};
			}
			return vMap;
		}


		#endregion



		#region --- LGC ---


		// Time Value ID
		private static List<Beatmap.TimeFloatTween> GetMovementArrayFromVoezData (VoezTrackData.MovementItem[] source, float endTime, float valueOffset, float timeOffset, float startValue, float valueL, float valueR) {
			var valueList = new List<Beatmap.TimeFloatTween> {
				new Beatmap.TimeFloatTween(0f, startValue, 0)
			};
			float lastValue = Lerp(valueList[0].Value);
			for (int j = 0; j < source.Length; j++) {
				var move = source[j];
				byte tweenID = GetTweenID(move.Ease);
				if (Mathf.Abs(valueList[valueList.Count - 1].Time - (move.Start + timeOffset)) < 0.001f) {
					valueList[valueList.Count - 1] = new Beatmap.TimeFloatTween(move.Start + timeOffset, lastValue, tweenID);
				} else {
					valueList.Add(new Beatmap.TimeFloatTween(move.Start + timeOffset, lastValue, tweenID));
				}
				lastValue = Lerp(move.To) + valueOffset;
				valueList.Add(new Beatmap.TimeFloatTween(move.End + timeOffset, lastValue, 0));
			}
			valueList.Add(new Beatmap.TimeFloatTween(endTime + timeOffset, lastValue, 0));
			return valueList;
			// Func
			float Lerp (float value) => Mathf.LerpUnclamped(valueL, valueR, value);
		}


		private static List<Beatmap.TimeByteTween> GetColorArrayFromVoezData (VoezTrackData.MovementItem[] source, float endTime, float valueOffset, float timeOffset, float startValue, float valueL, float valueR) {
			var tftList = GetMovementArrayFromVoezData(source, endTime, valueOffset, timeOffset, startValue, valueL, valueR);
			var result = new List<Beatmap.TimeByteTween>();
			foreach (var tft in tftList) {
				result.Add(new Beatmap.TimeByteTween(tft.Time, (byte)Mathf.Clamp(tft.Value, 0f, 255f), tft.Tween));
			}
			return result;
		}


		private static VoezTrackData.MovementItem[] GetVoezMovementArray (List<Beatmap.TimeByteTween> source, float valueOffset, float timeOffset, float valueL, float valueR) {
			var list = new List<Beatmap.TimeFloatTween>();
			foreach (var s in source) {
				list.Add(new Beatmap.TimeFloatTween(s.Time, s.Value, s.Tween));
			}
			return GetVoezMovementArray(list, valueOffset, timeOffset, valueL, valueR);
		}


		private static VoezTrackData.MovementItem[] GetVoezMovementArray (List<Beatmap.TimeFloatTween> source, float valueOffset, float timeOffset, float valueL, float valueR) {
			var movementList = new List<VoezTrackData.MovementItem>();
			for (int i = 0; i < source.Count - 1; i++) {
				byte tweenID = source[i].Tween;
				if (tweenID == 0) { continue; }
				movementList.Add(new VoezTrackData.MovementItem() {
					Start = source[i].Time + timeOffset,
					End = source[i + 1].Time + timeOffset,
					To = Lerp(source[i + 1].Value) + valueOffset,
					Ease = TWEEN_NAMES[tweenID % TWEEN_NAMES.Length],
				});
			}
			return movementList.ToArray();
			// Func
			float Lerp (float value) => Mathf.LerpUnclamped(valueL, valueR, value);
		}


		// Tween
		private static byte GetTweenID (string name) {
			switch (name) {
				default:
				case "const":
					return 0;
				case "easelinear":
					return 1;
				case "easeswing":
					return 2;
				case "reserved":
					return 3;

				case "easeinquad":
					return 4;
				case "easeoutquad":
					return 5;
				case "easeinoutquad":
					return 6;

				case "easeincubic":
					return 7;
				case "easeoutcubic":
					return 8;
				case "easeinoutcubic":
					return 9;

				case "easeinquart":
					return 10;
				case "easeoutquart":
					return 11;
				case "easeinoutquart":
					return 12;

				case "easeinquint":
					return 13;
				case "easeoutquint":
					return 14;
				case "easeinoutquint":
					return 15;

				case "easeinexpo":
					return 16;
				case "easeoutexpo":
					return 17;
				case "easeinoutexpo":
					return 18;

				case "easeinsine":
					return 19;
				case "easeoutsine":
					return 20;
				case "easeinoutsine":
					return 21;

				case "easeincirc":
					return 22;
				case "easeoutcirc":
					return 23;
				case "easeinoutcirc":
					return 24;

				case "easeinElastic":
					return 25;
				case "easeoutElastic":
					return 26;
				case "easeinoutElastic":
					return 27;

				case "easeinback":
					return 28;
				case "easeoutback":
					return 29;
				case "easeinoutback":
					return 30;

				case "easeinbounce":
					return 31;
				case "easeoutbounce":
					return 32;
				case "easeinoutbounce":
					return 33;

			}
		}


		// Note Type
		private static NoteType GetNoteType (string voezType, int dir) {
			switch (voezType) {
				default:
				case "click":
					return NoteType.Tap;
				case "hold":
					return NoteType.Hold;
				case "slide":
					return NoteType.Slide;
				case "swipe":
					return dir > 0 ? NoteType.SwipeRight : NoteType.SwipeLeft;
			}
		}


		private static NoteType GetNoteType (Beatmap.Note note) {
			if (note.Duration > 0.001f) {
				return NoteType.Hold;
			} else if (!note.Tap) {
				return NoteType.Slide;
			} else if (note.SwipeX == 1) {
				return NoteType.Tap;
			} else if (note.SwipeX == 0) {
				return NoteType.SwipeLeft;
			} else {
				return NoteType.SwipeRight;
			}
		}


		private static string GetVoezNoteType (NoteType type) {
			switch (type) {
				case NoteType.Tap:
					return "click";
				case NoteType.Hold:
					return "hold";
				case NoteType.Slide:
					return "slide";
				default:
					return "swipe";
			}
		}


		#endregion


	}
}