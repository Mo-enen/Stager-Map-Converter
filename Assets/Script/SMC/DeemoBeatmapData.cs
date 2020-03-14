namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	[System.Serializable]
	public class DeemoBeatmapData {




		#region --- SUB ---


		[System.Serializable]
		public class NoteData {
			[System.Serializable]
			public class SoundData {
				public float d = 0f; // Duration
				public int p = 0;   // Pitch
				public int v = 0;   // Volume
			}
			public int __id = 0;
			public SoundData[] sounds = null;
			public float pos = 0f;
			public float size = 0f;
			public float _time = 0f;
		}


		[System.Serializable]
		public class LinkData {
			[System.Serializable]
			public class LinkedNote {
				public int __ref = -1;
			}
			public LinkedNote[] notes = null;
		}


		#endregion




		#region --- VAR ---


		// Ser
		public float speed = 10f;
		public NoteData[] notes = null;
		public LinkData[] links = null;


		#endregion




		#region --- API ---


		public static Beatmap DMap_to_SMap (DeemoBeatmapData dMap) {
			if (dMap is null || dMap.notes is null) { return null; }
			int noteCount = dMap.notes.Length;
			var data = new Beatmap {
				BPM = 120f,
				Shift = 0f,
				DropSpeed = dMap.speed / 10f,
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
						Angle = 45f,
						X = 0.5f,
						Y = 0f,
						Heights ={ },
						Widths = { },
						Positions = { },
						Rotations = { },
						Angles = { },
					}
				},
				Tracks = new List<Beatmap.Track>() {
					new Beatmap.Track() { // Bottom
						Duration = float.MaxValue,
						Time = 0f,
						StageIndex = 0,
						Width = 1f,
						X = 0.5f,
						HasTray = false,
						Color = 9,
						Xs = { },
						Widths = { },
						Colors = { },
					},
				},
				Notes = new List<Beatmap.Note>(new Beatmap.Note[noteCount]),
			};
			// Notes
			for (int i = 0; i < noteCount; i++) {
				var dNote = dMap.notes[i];
				int id = dNote.__id - 1;
				if (id >= 0 && id < noteCount) {
					data.Notes[id] = new Beatmap.Note() {
						Time = dNote._time,
						X = Util.Remap(-2f, 2f, 0.1f, 0.9f, dNote.pos),
						Width = dNote.size / 5f,
						Tap = true,
						LinkedNoteIndex = -1,
						Duration = 0f,
						ClickSoundIndex = (byte)(dNote.sounds == null || dNote.sounds.Length == 0 ? -1 : 0),
						SwipeX = 1,
						SwipeY = 1,
						TrackIndex = 0,
					};
				}
			}
			// Links
			for (int i = 0; i < dMap.links.Length; i++) {
				var dLink = dMap.links[i];
				if (dLink.notes != null && dLink.notes.Length > 0) {
					for (int j = 0; j < dLink.notes.Length; j++) {
						int id = dLink.notes[j].__ref;
						if (id >= 0 && id < noteCount) {
							data.Notes[id].Tap = false;
						}
					}
				}
			}
			// Remove Needless Notes
			for (int i = 0; i < data.Notes.Count; i++) {
				var note = data.Notes[i];
				if (note.X < 0.05f || note.X > 0.95f) {
					data.Notes.RemoveAt(i);
					i--;
				}
			}
			// Final
			data.SortNotesByTime();
			return data;
		}


		public static DeemoBeatmapData SMap_to_DMap (Beatmap sMap) {
			if (sMap is null || sMap.Stages == null || sMap.Stages.Count == 0 || sMap.Notes == null) { return null; }
			sMap.SortNotesByTime();
			int noteCount = sMap.Notes.Count;
			var dMap = new DeemoBeatmapData() {
				speed = sMap.Stages[0].Speed * 10f,
				notes = new NoteData[noteCount],
				links = new LinkData[1] { new LinkData() }
			};
			// Notes
			List<int> taplessID = new List<int>();
			for (int i = 0; i < noteCount; i++) {
				var sNote = sMap.Notes[i];
				if (!sNote.Tap) {
					taplessID.Add(i);
				}
				dMap.notes[i] = new NoteData() {
					__id = i + 1,
					_time = sNote.Time,
					pos = Util.Remap(0.1f, 0.9f, -2f, 2f, sNote.X),
					size = sNote.Width * 5f,
					sounds = sNote.ClickSoundIndex >= 0 ? new NoteData.SoundData[1] { new NoteData.SoundData() { d = 0f, p = 0, v = 0, } } : null,
				};
			}
			// Links
			dMap.links[0].notes = new LinkData.LinkedNote[taplessID.Count];
			for (int i = 0; i < taplessID.Count; i++) {
				dMap.links[0].notes[i] = new LinkData.LinkedNote() {
					__ref = taplessID[i] + 1,
				};
			}
			// Final
			return dMap;
		}


		#endregion




	}
}