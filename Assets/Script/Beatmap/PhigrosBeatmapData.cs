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
			for (int index = 0; index < pMap.judgeLineList.Length; index++) {
				var jLine = pMap.judgeLineList[index];
				// Stages
				sMap.Stages.Add(new Beatmap.Stage() {
					m_Time = 0,
					m_Duration = int.MaxValue,
					Height = 520f / 880f,
					ItemType = 0,
					Rotation = 0f,
					Speed = 1f,
					Width = 1f,
					X = 0f,
					Y = 0f,
					Color = 0,
					Positions = GetStagePositions(jLine.judgeLineMoveEvents),
					Rotations = GetStageRotations(jLine.judgeLineRotateEvents),
					Colors = GetStageColors(jLine.judgeLineDisappearEvents),
					Widths = { },
					Heights = { },
				});
				// Tracks
				sMap.Tracks.Add(new Beatmap.Track() {
					StageIndex = index,
					Angle = 0f,
					X = 0.5f,
					Time = 0f,
					m_Duration = int.MaxValue,
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
					Time = 0f,
					m_Duration = int.MaxValue,
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
						Time = note.time / 100f,
						X = Util.Remap(-10f, 10f, 0f, 1f, note.positionX),
						Duration = note.type == (int)NoteType.Hold ? note.holdTime / 100f : 0f,
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
						Time = note.time / 100f,
						X = Util.Remap(-10f, 10f, 0f, 1f, note.positionX),
						Duration = note.type == (int)NoteType.Hold ? note.holdTime / 100f : 0f,
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
					judgeLineDisappearEvents = GetDisappears(stage.Colors, maxNoteTime),
					judgeLineMoveEvents = GetMoves(stage.Positions, maxNoteTime),
					judgeLineRotateEvents = GetRotations(stage.Rotations, maxNoteTime),
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
		private static List<Beatmap.TimeFloatFloatTween> GetStagePositions (JudgeLineEvent[] moves) {
			var result = new List<Beatmap.TimeFloatFloatTween>();
			if (moves == null || moves.Length == 0) { return result; }
			for (int i = 0; i < moves.Length; i++) {
				var m = moves[i];
				result.Add(new Beatmap.TimeFloatFloatTween() {
					Time = Mathf.Max(m.startTime / 100f, 0f),
					A = Util.Remap(0f, 880f, 0f, 1f, m.start / 1000),
					B = Util.Remap(0f, 520f, 0f, 520f / 880f, m.start % 1000),
					Tween = 0,
				});
				if (i < moves.Length - 1) {
					var next = moves[i + 1];
					if (m.end != next.start && m.startTime < m.endTime - 1) {
						result.Add(new Beatmap.TimeFloatFloatTween() {
							Time = Mathf.Max((m.endTime - 1) / 100f, 0f),
							A = Util.Remap(0f, 880f, 0f, 1f, m.end / 1000),
							B = Util.Remap(0f, 520f, 0f, 1f, m.end % 1000),
							Tween = 0,
						});
					}
				} else if (i == moves.Length - 1) {
					result.Add(new Beatmap.TimeFloatFloatTween() {
						Time = Mathf.Max((m.endTime - 1) / 100f, 0f),
						A = Util.Remap(0f, 880f, 0f, 1f, m.end / 1000),
						B = Util.Remap(0f, 520f, 0f, 1f, m.end % 1000),
						Tween = 0,
					});
				}
			}
			return result;
		}
		private static List<Beatmap.TimeFloatTween> GetStageRotations (JudgeLineEvent[] rots) {
			var result = new List<Beatmap.TimeFloatTween>();
			if (rots == null || rots.Length == 0) { return result; }
			for (int i = 0; i < rots.Length; i++) {
				var r = rots[i];
				result.Add(new Beatmap.TimeFloatTween() {
					Time = Mathf.Max(r.startTime / 100f, 0f),
					Value = r.start,
					Tween = 0,
				});
				if (i < rots.Length - 1) {
					var next = rots[i + 1];
					if (r.end != next.start && r.startTime < r.endTime - 1) {
						result.Add(new Beatmap.TimeFloatTween() {
							Time = Mathf.Max((r.endTime - 1) / 100f, 0f),
							Value = r.end,
							Tween = 0,
						});
					}
				} else if (i == rots.Length - 1) {
					result.Add(new Beatmap.TimeFloatTween() {
						Time = Mathf.Max((r.endTime - 1) / 100f, 0f),
						Value = r.end,
						Tween = 0,
					});
				}
			}
			return result;
		}
		private static List<Beatmap.TimeIntTween> GetStageColors (JudgeLineEvent[] dis) {
			var result = new List<Beatmap.TimeIntTween>();
			if (dis == null || dis.Length == 0) { return result; }
			for (int i = 0; i < dis.Length; i++) {
				var d = dis[i];
				result.Add(new Beatmap.TimeIntTween() {
					Time = Mathf.Max(d.startTime / 100f, 0f),
					Value = 1 - d.start,
					Tween = 0,
				});
				result.Add(new Beatmap.TimeIntTween() {
					Time = Mathf.Max((d.endTime - 1) / 100f, 0f),
					Value = 1 - d.end,
					Tween = 0,
				});
			}
			return result;
		}

		private static JudgeLineEvent[] GetMoves (List<Beatmap.TimeFloatFloatTween> positions, float maxNoteTime) {
			var result = new List<JudgeLineEvent>();
			if (positions != null && positions.Count > 1) {
				for (int i = 0; i < positions.Count - 1; i++) {
					var pos = positions[i];
					var next = positions[i + 1];
					result.Add(new JudgeLineEvent() {
						startTime = (int)(pos.Time * 100),
						endTime = (int)(next.Time * 100),
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
		private static JudgeLineEvent[] GetRotations (List<Beatmap.TimeFloatTween> rots, float maxNoteTime) {
			var result = new List<JudgeLineEvent>();
			if (rots != null && rots.Count > 1) {
				for (int i = 0; i < rots.Count - 1; i++) {
					var rot = rots[i];
					var next = rots[i + 1];
					result.Add(new JudgeLineEvent() {
						startTime = (int)(rot.Time * 100),
						endTime = (int)(next.Time * 100),
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
		private static JudgeLineEvent[] GetDisappears (List<Beatmap.TimeIntTween> colors, float maxNoteTime) {
			var result = new List<JudgeLineEvent>();
			if (colors != null && colors.Count > 1) {
				for (int i = 0; i < colors.Count - 1; i++) {
					var color = colors[i];
					var next = colors[i + 1];
					result.Add(new JudgeLineEvent() {
						startTime = (int)(color.Time * 100),
						endTime = (int)(next.Time * 100),
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


	}
}