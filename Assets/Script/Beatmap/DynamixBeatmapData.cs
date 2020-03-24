namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using System.Xml.Serialization;
	using UnityEngine;



	[System.Serializable]
	[XmlRoot("CMap")]
	public class DynamixBeatmapData {



		#region --- SUB ---


		private enum NoteType {
			Tap = 0,
			Hold = 1,
			Slide = 2,
		}


		[System.Serializable]
		public class Notes {

			[System.Serializable]
			public class CMapNoteAsset {
				public int m_id;
				public string m_type; //NORMAL HOLD CHAIN SUB
				public float m_time;
				public float m_position;
				public float m_width;
				public int m_subId;
			}

			[XmlArray("m_notes")]
			[XmlArrayItem("CMapNoteAsset")]
			public List<CMapNoteAsset> m_notes;

		}



		#endregion




		#region --- VAR ---


		public string m_path;
		public float m_barPerMin;
		public float m_timeOffset;
		public string m_leftRegion;//MIXER PAD
		public string m_rightRegion;//MIXER PAD
		public string m_mapID;
		public Notes m_notes;
		public Notes m_notesLeft;
		public Notes m_notesRight;


		#endregion




		#region --- API ---


		public static Beatmap DMap_to_SMap (DynamixBeatmapData dMap) {
			if (dMap is null) { return null; }
			var data = new Beatmap {
				BPM = (int)Mathf.Max(dMap.m_barPerMin * 4, 1),
				Shift = dMap.m_timeOffset,
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
						X = 0.5f,
						Y = 0f,
						Widths = { },
						Heights = { },
						Positions = { },
						Rotations = { },
					}, new Beatmap.Stage() { // Right
						Duration = float.MaxValue,
						Rotation = -90f,
						Speed = 1f,
						Time = 0f,
						Width = 2f / 3f,
						Height = 0.5f,
						X = 1f,
						Y = 1f / 3f,
						Widths = { },
						Heights = { },
						Positions = { },
						Rotations = { },
					}, new Beatmap.Stage() { // Left
						Duration = float.MaxValue,
						Rotation = 90f,
						Speed = 1f,
						Time = 0f,
						Width = 2f / 3f,
						Height = 0.5f,
						X = 0f,
						Y = 1f / 3f,
						Widths = { },
						Heights = { },
						Positions = { },
						Rotations = { },
					},
				},
				Tracks = new List<Beatmap.Track> {
					new Beatmap.Track() { // Bottom
						Duration = float.MaxValue,
						Time = 0f,
						StageIndex = 0,
						Width = 1f,
						X = 0.5f,
						HasTray = false,
						Color = 9,
						Angle = 0f,
						Xs = { },
						Widths = { },
						Colors = { },
						Angles = { },
					},
					new Beatmap.Track() { // Right
						Duration = float.MaxValue,
						Time = 0f,
						StageIndex = 1,
						Width = 1f,
						X = 0.5f,
						HasTray = dMap.m_rightRegion == "MIXER",
						Color = 9,
						Angle = 0f,
						Xs = { },
						Widths ={ },
						Colors ={ },
						Angles = { },
					},
					new Beatmap.Track() { // Left
						Duration = float.MaxValue,
						Time = 0f,
						StageIndex = 2,
						Width = 1f,
						X = 0.5f,
						HasTray = dMap.m_leftRegion == "MIXER",
						Color = 9,
						Angle = 0f,
						Xs = { },
						Widths = { },
						Colors = { },
						Angles = { },
					},
				},
				Notes = null,
			};
			var notes = new List<Beatmap.Note>();
			notes.AddRange(GetNoteDataFromDynamix(dMap.m_notes, 0, -dMap.m_timeOffset, 60f / dMap.m_barPerMin));
			notes.AddRange(GetNoteDataFromDynamix(dMap.m_notesRight, 1, -dMap.m_timeOffset, 60f / dMap.m_barPerMin));
			notes.AddRange(GetNoteDataFromDynamix(dMap.m_notesLeft, 2, -dMap.m_timeOffset, 60f / dMap.m_barPerMin, true));
			data.Notes = notes;
			data.SortNotesByTime();
			return data;
		}


		public static DynamixBeatmapData SMap_to_DMap (Beatmap source) {
			var dMap = new DynamixBeatmapData();
			if (source.Tracks.Count < 3) { return dMap; }
			source.SortNotesByTime();
			dMap.m_path = "";
			dMap.m_barPerMin = Mathf.Max(1f, source.BPM / 4f);
			dMap.m_timeOffset = 0f;
			dMap.m_mapID = "Dynamix Beatmap";
			dMap.m_leftRegion = source.Tracks[2].HasTray ? "MIXER" : "PAD";
			dMap.m_rightRegion = source.Tracks[1].HasTray ? "MIXER" : "PAD";
			dMap.m_notes = new Notes() { m_notes = new List<Notes.CMapNoteAsset>() };
			dMap.m_notesLeft = new Notes() { m_notes = new List<Notes.CMapNoteAsset>() };
			dMap.m_notesRight = new Notes() { m_notes = new List<Notes.CMapNoteAsset>() };
			float timeMuti = dMap.m_barPerMin / 60f;
			for (int i = 0; i < source.Notes.Count; i++) {
				var note = source.Notes[i];
				Notes notes = note.TrackIndex == 0 ? dMap.m_notes : note.TrackIndex == 1 ? dMap.m_notesRight : dMap.m_notesLeft;
				float w = note.Width * (note.TrackIndex == 0 ? 5.6f : 6.5f);
				float noteX = note.TrackIndex == 2 ? 1f - note.X : note.X;
				float pos = (note.TrackIndex == 0 ? (noteX * 5.6f - 0.3f) : noteX * 6f) - w * 0.5f;
				NoteType noteType = note.Duration > 0.001f ? NoteType.Hold : note.Tap ? NoteType.Tap : NoteType.Slide;
				notes.m_notes.Add(new Notes.CMapNoteAsset() {
					m_id = notes.m_notes.Count,
					m_subId = noteType == NoteType.Hold ? notes.m_notes.Count + 1 : -1,
					m_type = GetDynamixType(noteType),
					m_time = note.Time * timeMuti,
					m_width = w,
					m_position = pos,
				});
				if (noteType == NoteType.Hold) {
					notes.m_notes.Add(new Notes.CMapNoteAsset() {
						m_id = notes.m_notes.Count,
						m_subId = -1,
						m_type = "SUB",
						m_time = (note.Time + note.Duration) * timeMuti,
						m_width = w,
						m_position = pos,
					});
				}
			}
			return dMap;
		}


		#endregion




		#region --- LGC ---


		private static List<Beatmap.Note> GetNoteDataFromDynamix (Notes source, int trackID, float timeOffset, float timeMuti, bool reverseX = false) {
			var target = new List<Beatmap.Note>();
			for (int i = 0; i < source.m_notes.Count; i++) {
				var note = source.m_notes[i];
				if (note.m_type != "SUB") {
					float w = note.m_width / (trackID == 0 ? 5.6f : 6.5f);
					float x = (trackID == 0 ? (note.m_position + 0.3f) / 5.6f : note.m_position / 6f) + w * 0.5f;
					target.Add(new Beatmap.Note() {
						TrackIndex = trackID,
						Time = (note.m_time + timeOffset) * timeMuti,
						Width = w,
						X = reverseX ? 1f - x : x,
						Duration = note.m_type == "HOLD" ? (note.m_subId >= 0 && note.m_subId < source.m_notes.Count ? source.m_notes[note.m_subId].m_time - note.m_time : 0) : 0f,
						Tap = GetNoteTypeFromDynamix(note.m_type) != NoteType.Slide,
						LinkedNoteIndex = -1,
						ClickSoundIndex = 0,
						SwipeX = 1,
						SwipeY = 1,
					});
				}
			}
			return target;
		}


		private static NoteType GetNoteTypeFromDynamix (string dyType) {
			switch (dyType) {
				default:
				case "NORMAL":
					return NoteType.Tap;
				case "HOLD":
					return NoteType.Hold;
				case "CHAIN":
					return NoteType.Slide;
			}
		}


		private static string GetDynamixType (NoteType type) {
			switch (type) {
				default:
				case NoteType.Tap:
					return "NORMAL";
				case NoteType.Hold:
					return "HOLD";
				case NoteType.Slide:
					return "CHAIN";
			}
		}


		#endregion



	}
}