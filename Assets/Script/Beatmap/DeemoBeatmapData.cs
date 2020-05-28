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


		public enum NoteType {
			Normal = 0,
			Mute = 1,
			Slide = 2,
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
			// Fix Deemo ID
			for (int i = 0; i < dMap.notes.Length; i++) {
				dMap.notes[i].__id--;
			}
			for (int i = 0; i < dMap.links.Length; i++) {
				var link = dMap.links[i];
				for (int j = 0; j < link.notes.Length; j++) {
					dMap.links[i].notes[j].__ref--;
				}
			}
			// Start
			int noteCount = dMap.notes.Length;
			var data = new Beatmap {
				BPM = 120,
				Shift = 0f,
				Level = 1,
				Ratio = 1.5f,
				Tag = "Normal",
				CreatedTime = System.DateTime.Now.Ticks,
				Timings = new List<Beatmap.Timing>(),
				Stages = new List<Beatmap.Stage> {
					new Beatmap.Stage() { // Bottom
						Duration = float.MaxValue,
						Rotation = 0f,
						Speed = 2f / 3f,
						Time = 0f,
						Width = 0.8f,
						Height = 1f,
						X = 0.5f,
						Y = 0f,
						Heights ={ },
						Widths = { },
						Positions = { },
						Rotations = { },
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
						Speed = 1f,
						Color = 1,
						Angle = 30,
						Xs = { },
						Widths = { },
						Colors = { },
						Angles= { },
					},
				},
			};
			// Note Array
			var realIDs = new int[noteCount];
			int realNoteCount = 0;
			for (int i = 0; i < noteCount; i++) {
				var dNote = dMap.notes[i];
				if (dNote.pos >= -2.01f && dNote.pos <= 2.01f) {
					realIDs[i] = realNoteCount;
					realNoteCount++;
				} else {
					realIDs[i] = -1;
					dMap.notes[i] = null;
				}
			}
			// Notes
			data.Notes = new List<Beatmap.Note>(new Beatmap.Note[realNoteCount]);
			for (int i = 0; i < noteCount; i++) {
				var dNote = dMap.notes[i];
				int realID = realIDs[i];
				if (dNote != null && realID >= 0) {
					data.Notes[realID] = new Beatmap.Note() {
						Time = dNote._time,
						X = Util.Remap(-2f, 2f, 0f, 1f, dNote.pos),
						Width = dNote.size / 5f,
						ItemType = dNote.sounds == null || dNote.sounds.Length == 0 ? (int)NoteType.Mute : (int)NoteType.Normal,
						LinkedNoteIndex = -1,
						Duration = 0f,
						ClickSoundIndex = (short)(dNote.sounds == null || dNote.sounds.Length == 0 ? -1 : 0),
						TrackIndex = 0,
						Speed = 1f,
					};
				}
			}
			// Links
			for (int i = 0; i < dMap.links.Length; i++) {
				var dLink = dMap.links[i];
				if (dLink.notes != null) {
					for (int j = 0; j < dLink.notes.Length; j++) {
						int refID = dLink.notes[j].__ref;
						if (refID < 0 || refID >= noteCount) { continue; }
						int realID = realIDs[refID];
						if (realID < 0 || realID >= noteCount) { continue; }
						// Slide
						data.Notes[realID].ItemType = (int)NoteType.Slide;
						// Link to Next
						if (j < dLink.notes.Length - 1) {
							data.Notes[realID].LinkedNoteIndex = realIDs[dLink.notes[j + 1].__ref];
						}
					}
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
				speed = 1f,
				notes = new NoteData[noteCount],
			};
			// Notes
			var linkMap = new Dictionary<int, List<int>>();
			for (int i = 0; i < noteCount; i++) {
				var sNote = sMap.Notes[i];
				int linkIndex = sNote.LinkedNoteIndex;
				if (linkIndex >= 0 && linkIndex < noteCount) {
					// Link Map
					if (!linkMap.ContainsKey(i)) {
						if (!linkMap.ContainsKey(linkIndex)) {
							// Get List
							int index = i;
							var list = new List<int>();
							int safe = 0;
							while (index >= 0 && index < noteCount) {
								list.Add(index);
								index = sMap.Notes[index].LinkedNoteIndex;
								safe++;
								if (safe > noteCount + 1) { break; }
							}
							// Add to Map
							if (list.Count > 0) {
								linkMap.Add(i, list);
								for (int i1 = 1; i1 < list.Count; i1++) {
									int lIndex = list[i1];
									if (linkMap.ContainsKey(lIndex)) {
										linkMap[lIndex].Clear();
									} else {
										linkMap.Add(lIndex, new List<int>());
									}
								}
							}
						}
					}
				}
				dMap.notes[i] = new NoteData() {
					__id = i + 1,
					_time = sNote.Time,
					pos = Util.Remap(0f, 1f, -2f, 2f, sNote.X),
					size = sNote.Width * 5f,
					sounds = sNote.ItemType == (int)NoteType.Normal ? new NoteData.SoundData[1] { new NoteData.SoundData() { d = 0f, p = 0, v = 0, } } : null,
				};
			}
			// Links
			var finalLinkedList = new List<int[]>();
			foreach (var pair in linkMap) {
				if (pair.Value != null && pair.Value.Count > 0) {
					finalLinkedList.Add(pair.Value.ToArray());
				}
			}
			dMap.links = new LinkData[finalLinkedList.Count];
			if (finalLinkedList.Count > 0) {
				for (int i1 = 0; i1 < finalLinkedList.Count; i1++) {
					int[] list = finalLinkedList[i1];
					dMap.links[i1] = new LinkData() {
						notes = new LinkData.LinkedNote[list.Length],
					};
					for (int i = 0; i < list.Length; i++) {
						dMap.links[i1].notes[i] = new LinkData.LinkedNote() { __ref = list[i] + 1 };
					}
				}
			}
			// Final
			return dMap;
		}


		#endregion




	}
}