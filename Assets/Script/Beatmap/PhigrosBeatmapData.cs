namespace StagerStudio.Data {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;


	[System.Serializable]
	public class PhigrosBeatmapData {




		private enum NoteType {
			Tap = 1,
			Drag = 2,
			Hold = 3,
			Flick = 4,
		}




		[System.Serializable]
		public class Note {
			public int type = 1; // 1:tap  2:drag  3:hold  4:flick
			public int time = 0;
			public float positionX = 0f;
			public int holdTime = 0;
			public float speed = 1f;
			public float floorPosition = 0f;
		}



		[System.Serializable]
		public class SpeedEvent {
			public int startTime = 0;
			public int endTime = 0;
			public float value = 1f;
		}


		[System.Serializable]
		public class JudgeLineEvent {
			public int startTime = 0;
			public int endTime = 0;
			public int start = 0;
			public int end = 0;
		}



		[System.Serializable]
		public class JudgelineData {
			public int numOfNotes = 0;
			public int numOfNotesAbove = 0;
			public int numOfNotesBelow = 0;
			public float bpm = 120f;
			public SpeedEvent[] speedEvents = { };
			public Note[] notesAbove = { };
			public Note[] notesBelow = { };
			public JudgeLineEvent[] judgeLineDisappearEvents = { };
			public JudgeLineEvent[] judgeLineMoveEvents = { };
			public JudgeLineEvent[] judgeLineRotateEvents = { };

		}


		public int formatVersion = 0;
		public float offset = 0f;
		public int numOfNotes = 0;
		public JudgelineData[] judgeLineList = { };


		// API
		public static Beatmap Phigros_to_Stager (PhigrosBeatmapData pMap) {
			if (pMap == null || pMap.judgeLineList == null || pMap.judgeLineList.Length == 0) { return null; }
			var sMap = new Beatmap() {
				Tag = "Phigros",
				Level = 1,
				BPM = (int)pMap.judgeLineList[0].bpm,
				Ratio = 880f / 520f,
				CreatedTime = System.DateTime.Now.Ticks,
				Shift = 0f,
				Stages = { },
				Tracks = { },
				Notes = { },
				Timings = { },
			};
			float spb = 60f / sMap.bpm;
			for (int index = 0; index < pMap.judgeLineList.Length; index++) {
				var jLine = pMap.judgeLineList[index];
				// Stages
				float time = -1f;
				float duration = -1f;
				foreach (var d in jLine.judgeLineDisappearEvents) {
					if (Mathf.Max(d.startTime, 0) >= d.endTime) { continue; }
					if (d.start == 1) {
						// Time
						if (time < -0.5f) {
							time = GetRealTime(d.startTime, spb);
						}
						// Duration
						duration = Mathf.Max(GetRealTime(d.endTime, spb) - time, duration);
					}
				}
				sMap.Stages.Add(new Beatmap.Stage() {
					Time = time,
					Duration = duration > 0f ? duration : float.MaxValue,
					Height = 520f / 880f,
					ItemType = 0,
					Rotation = 0f,
					Speed = 1f,
					Width = 1f,
					X = 0f,
					Y = 0f,
					Color = 0,
					Positions = GetStagePositions(jLine.judgeLineMoveEvents, -time, spb),
					Rotations = GetStageRotations(jLine.judgeLineRotateEvents, -time, spb),
					Colors = GetStageColors(jLine.judgeLineDisappearEvents, -time, spb),
					Widths = { },
					Heights = { },
				});
				// Tracks
				sMap.Tracks.Add(new Beatmap.Track() {
					StageIndex = index,
					Angle = 0f,
					X = 0.5f,
					Time = time,
					Duration = duration > 0f ? duration : float.MaxValue,
					Color = 0,
					HasTray = false,
					ItemType = 0,
					Width = 1f,
					Widths = { },
					Xs = { },
					Angles = { },
					Colors = { },
				});
				sMap.Tracks.Add(new Beatmap.Track() {
					StageIndex = index,
					Angle = 180f,
					X = 0.5f,
					Time = time,
					Duration = duration > 0f ? duration : float.MaxValue,
					Color = 0,
					HasTray = false,
					ItemType = 0,
					Width = 1f,
					Widths = { },
					Xs = { },
					Angles = { },
					Colors = { },
				});
				// Notes
				foreach (var note in jLine.notesAbove) {
					sMap.Notes.Add(new Beatmap.Note() {
						TrackIndex = index * 2,
						Time = GetRealTime(note.time, spb),
						X = Util.Remap(-10f, 10f, 0f, 1f, note.positionX),
						Duration = note.type == (int)NoteType.Hold ? GetRealTime(note.holdTime, spb) : 0f,
						ClickSoundIndex = 0,
						ItemType = note.type - 1,
						LinkedNoteIndex = -1,
						Width = 1f / 7f,
						Z = 0f,
						SoundFxIndex = 0,
						SoundFxParamA = 0,
						SoundFxParamB = 0,
					});
				}
				foreach (var note in jLine.notesBelow) {
					sMap.Notes.Add(new Beatmap.Note() {
						TrackIndex = index * 2 + 1,
						Time = GetRealTime(note.time, spb),
						X = Util.Remap(-10f, 10f, 0f, 1f, note.positionX),
						Duration = note.type == (int)NoteType.Hold ? GetRealTime(note.holdTime, spb) : 0f,
						ClickSoundIndex = 0,
						ItemType = note.type - 1,
						LinkedNoteIndex = -1,
						Width = 1f / 7f,
						Z = 0f,
						SoundFxIndex = 0,
						SoundFxParamA = 0,
						SoundFxParamB = 0,
					});
				}
				// Timings
				foreach (var sEvent in jLine.speedEvents) {
					sMap.Timings.Add(new Beatmap.Timing(Mathf.Max(sEvent.startTime / 100f, 0f), sEvent.value));
				}
			}
			return sMap;

		}


		public static PhigrosBeatmapData Stager_to_Phigros (Beatmap sMap) {

			if (sMap == null) { return null; }
			sMap.SortNotesByTime();

			var pMap = new PhigrosBeatmapData() {
				formatVersion = 1,
				offset = 0f,
				numOfNotes = sMap.Notes.Count,
				judgeLineList = new JudgelineData[sMap.Stages.Count],
			};
			float spb = 60f / sMap.BPM;
			for (int stageIndex = 0; stageIndex < sMap.Stages.Count; stageIndex++) {
				var noteAbove = new List<Note>();
				var noteBelow = new List<Note>();
				float maxNoteTime = 0f;
				foreach (var note in sMap.Notes) {
					if (note.TrackIndex == stageIndex * 2) {
						// Above
						noteAbove.Add(new Note() {
							type = note.ItemType + 1,
							time = (int)(note.Time * 100),
							holdTime = (note.ItemType + 1) == (int)NoteType.Hold ? (int)(note.Duration * 100) : 0,
							positionX = Util.Remap(0f, 1f, -10f, 10f, note.X),
							floorPosition = 25f / 8f * note.Time * spb,
							speed = 1f,
						});
					} else if (note.TrackIndex == stageIndex * 2 + 1) {
						// Below
						noteAbove.Add(new Note() {
							type = note.ItemType + 1,
							time = (int)(note.Time * 100),
							holdTime = (note.ItemType + 1) == (int)NoteType.Hold ? (int)(note.Duration * 100) : 0,
							positionX = Util.Remap(0f, 1f, -10f, 10f, note.X),
							floorPosition = 25f / 8f * note.Time * spb,
							speed = 1f,
						});
					}
					maxNoteTime = Mathf.Max(maxNoteTime, note.Time + note.Duration);
				}
				var stage = sMap.Stages[stageIndex];
				pMap.judgeLineList[stageIndex] = new JudgelineData() {
					bpm = sMap.BPM,
					numOfNotes = noteAbove.Count + noteBelow.Count,
					numOfNotesAbove = noteAbove.Count,
					numOfNotesBelow = noteBelow.Count,
					notesAbove = noteAbove.ToArray(),
					notesBelow = noteBelow.ToArray(),
					judgeLineDisappearEvents = GetDisappears(stage.Colors, maxNoteTime, stage.Time),
					judgeLineMoveEvents = GetMoves(stage.Positions, maxNoteTime, stage.Time),
					judgeLineRotateEvents = GetRotations(stage.Rotations, maxNoteTime, stage.Time),
					speedEvents = new SpeedEvent[1] {
						new SpeedEvent(){
							startTime = -1,
							endTime = (int)(maxNoteTime * 100),
							value = 1f,
						}
					},
				};
			}
			return pMap;
		}


		// LGC
		private static List<Beatmap.TimeFloatFloatTween> GetStagePositions (JudgeLineEvent[] moves, float timeOffset, float spb) {
			var result = new List<Beatmap.TimeFloatFloatTween>();
			if (moves == null || moves.Length == 0) { return result; }
			for (int i = 0; i < moves.Length; i++) {
				var m = moves[i];
				result.Add(new Beatmap.TimeFloatFloatTween() {
					Time = Mathf.Max(Mathf.Max(GetRealTime(m.startTime, spb), 0f) + timeOffset, 0f),
					A = Util.Remap(0f, 880f, 0f, 1f, m.start / 1000),
					B = Util.Remap(0f, 520f, 0f, 520f / 880f, m.start % 1000),
					Tween = 0,
				});
				result.Add(new Beatmap.TimeFloatFloatTween() {
					Time = Mathf.Max(Mathf.Max(GetRealTime(m.endTime, spb), 0f) + timeOffset - 0.00001f, 0f),
					A = Util.Remap(0f, 880f, 0f, 1f, m.end / 1000),
					B = Util.Remap(0f, 520f, 0f, 520f / 880f, m.end % 1000),
					Tween = 0,
				});
			}
			return result;
		}
		private static List<Beatmap.TimeFloatTween> GetStageRotations (JudgeLineEvent[] rots, float timeOffset, float spb) {
			var result = new List<Beatmap.TimeFloatTween>();
			if (rots == null || rots.Length == 0) { return result; }
			for (int i = 0; i < rots.Length; i++) {
				var r = rots[i];
				result.Add(new Beatmap.TimeFloatTween() {
					Time = Mathf.Max(Mathf.Max(GetRealTime(r.startTime, spb), 0f) + timeOffset, 0f),
					Value = -r.start,
					Tween = 0,
				});
				result.Add(new Beatmap.TimeFloatTween() {
					Time = Mathf.Max(Mathf.Max(GetRealTime(r.endTime, spb), 0f) + timeOffset - 0.00001f, 0f),
					Value = -r.end,
					Tween = 0,
				});
			}
			return result;
		}
		private static List<Beatmap.TimeIntTween> GetStageColors (JudgeLineEvent[] dis, float timeOffset, float spb) {
			var result = new List<Beatmap.TimeIntTween>();
			if (dis == null || dis.Length == 0) { return result; }
			for (int i = 0; i < dis.Length; i++) {
				var d = dis[i];
				result.Add(new Beatmap.TimeIntTween() {
					Time = Mathf.Max(Mathf.Max(GetRealTime(d.startTime, spb), 0f) + timeOffset, 0f),
					Value = 1 - d.start,
					Tween = 0,
				});
				result.Add(new Beatmap.TimeIntTween() {
					Time = Mathf.Max(Mathf.Max(GetRealTime(d.endTime, spb), 0f) + timeOffset - 0.00001f, 0f),
					Value = 1 - d.end,
					Tween = 0,
				});
			}
			return result;
		}

		private static JudgeLineEvent[] GetMoves (List<Beatmap.TimeFloatFloatTween> positions, float maxNoteTime, float timeOffset) {
			var result = new List<JudgeLineEvent>();
			if (positions != null && positions.Count > 1) {
				for (int i = 0; i < positions.Count - 1; i++) {
					var pos = positions[i];
					var next = positions[i + 1];
					result.Add(new JudgeLineEvent() {
						startTime = (int)((pos.Time + timeOffset) * 100),
						endTime = (int)((next.Time + timeOffset) * 100),
						start = (int)Util.Remap(0f, 1f, 0f, 880f, Mathf.Clamp01(pos.A)) * 1000 + (int)Util.Remap(0f, 1f, 0f, 520f, Mathf.Clamp01(pos.B)),
						end = (int)Util.Remap(0f, 1f, 0f, 880f, Mathf.Clamp01(next.A)) * 1000 + (int)Util.Remap(0f, 1f, 0f, 520f, Mathf.Clamp01(next.B)),
					});
				}
			} else {
				result.Add(new JudgeLineEvent() {
					startTime = 0,
					endTime = (int)(maxNoteTime * 100),
					start = 0,
					end = 0,
				});
			}
			return result.ToArray();
		}
		private static JudgeLineEvent[] GetRotations (List<Beatmap.TimeFloatTween> rots, float maxNoteTime, float timeOffset) {
			var result = new List<JudgeLineEvent>();
			if (rots != null && rots.Count > 1) {
				for (int i = 0; i < rots.Count - 1; i++) {
					var rot = rots[i];
					var next = rots[i + 1];
					result.Add(new JudgeLineEvent() {
						startTime = (int)((rot.Time + timeOffset) * 100),
						endTime = (int)((next.Time + timeOffset) * 100),
						start = (int)rot.Value,
						end = (int)next.Value,
					});
				}
			} else {
				result.Add(new JudgeLineEvent() {
					startTime = 0,
					endTime = (int)(maxNoteTime * 100),
					start = 0,
					end = 0,
				});
			}
			return result.ToArray();
		}
		private static JudgeLineEvent[] GetDisappears (List<Beatmap.TimeIntTween> colors, float maxNoteTime, float timeOffset) {
			var result = new List<JudgeLineEvent>();
			if (colors != null && colors.Count > 1) {
				for (int i = 0; i < colors.Count - 1; i++) {
					var color = colors[i];
					var next = colors[i + 1];
					result.Add(new JudgeLineEvent() {
						startTime = (int)((color.Time + timeOffset) * 100),
						endTime = (int)((next.Time + timeOffset) * 100),
						start = 1 - color.Value,
						end = 1 - next.Value,
					});
				}
			} else {
				result.Add(new JudgeLineEvent() {
					startTime = 0,
					endTime = (int)(maxNoteTime * 100),
					start = 1,
					end = 1,
				});
			}
			return result.ToArray();
		}

		private static float GetRealTime (int fuckedTime, float spb) {
			// 0.32 ?
			// 179 >> 0.33519 >> 1.0475019    
			// 120 >> 0.50000 >> 1.5625 ?
			// 140 >> 0.42857 >> 1.3392 ?
			return Mathf.Max(fuckedTime / 100f * (spb / 0.32f), 0f);
		}


	}
}