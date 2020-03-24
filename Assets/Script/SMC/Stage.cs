namespace StagerStudio {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using Saving;
	using Data;
	using System.Xml.Serialization;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;

	public partial class Stage : MonoBehaviour {




		#region --- VAR ---


		// Ser
		[Header("Hint")]
		[SerializeField] private RectTransform m_HintContainer = null;
		[SerializeField] private Text m_Hint = null;
		[SerializeField] private Text m_Tip = null;
		[SerializeField] private Text m_Version = null;
		[SerializeField] private Image m_Mark = null;
		[SerializeField] private Sprite m_SuccessMark = null;
		[SerializeField] private Sprite m_FailMark = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			Application.targetFrameRate = 60;
			QualitySettings.vSyncCount = 2;
			TooltipUI.TipLabel = m_Tip;
			m_Version.text = Application.version;
		}



		#endregion




		#region --- API ---


		// Dynamix
		public void UI_Dynamix_To_Stager () {
			var paths = DialogUtil.PickFilesDialog("Pick Dynamix XML files", "Dynamix XML", "xml", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var dyMap = Util.ReadXML<DynamixBeatmapData>(path);
					if (dyMap is null) { continue; }
					var sMap = DynamixBeatmapData.DMap_to_SMap(dyMap);
					if (sMap is null) { continue; }
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Dynamix_to_Stager");
					Util.CreateFolder(rootPath);
					// Map
					Util.TextToFile(JsonUtility.ToJson(sMap, false), Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".json"));
					// Final
					successCount++;
				} catch (System.Exception ex) {
					errorMsg = ex.Message;
				}
			}
			if (successCount > 0) {
				ShowHint("Success! Stager beatmaps created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		public void UI_Stager_To_Dynamix () {
			var paths = DialogUtil.PickFilesDialog("Pick Stager Beatmap files", "Stager Beatmap", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var sMap = JsonUtility.FromJson<Beatmap>(Util.FileToText(path));
					var dMap = DynamixBeatmapData.SMap_to_DMap(sMap);
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Stager_to_Dynamix");
					Util.CreateFolder(rootPath);
					// Map
					Util.WriteXML(dMap, Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".xml"));
					successCount++;
				} catch (System.Exception ex) {
					errorMsg = ex.Message;
				}
			}
			if (successCount > 0) {
				ShowHint("Success! Dynamix XML files created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		// Voez
		public void UI_Voez_To_Stager () {
			var trackPath = DialogUtil.PickFileDialog("Pick Voez [Track]", "Voez Track", "json", "txt");
			if (string.IsNullOrEmpty(trackPath)) { return; }
			var notePath = DialogUtil.PickFileDialog("Pick Voez [Note]", "Voez Note", "json", "txt");
			if (string.IsNullOrEmpty(notePath)) { return; }
			string errorMsg = "";
			try {

				string trackJson = Util.FileToText(trackPath);
				string noteJson = Util.FileToText(notePath);

				if (string.IsNullOrEmpty(trackJson) || string.IsNullOrEmpty(noteJson)) {
					ShowHint("Fail! Json file is empty.", false);
					return;
				}

				var voezMap = JsonUtility.FromJson<VoezBeatmapData>(@"{""m_Tracks"":" + trackJson + @", ""m_Notes"":" + noteJson + @"}");
				if (voezMap is null) {
					ShowHint("Fail! Json file is wrong.", false);
					return;
				}

				var sMap = VoezBeatmapData.VMap_to_SMap(voezMap);
				if (sMap is null) {
					ShowHint("Fail! Can\'t get Stager map.", false);
					return;
				}

				var rootPath = Util.CombinePaths(Util.GetParentPath(notePath), "Voez_to_Stager");
				Util.CreateFolder(rootPath);
				// Map
				Util.TextToFile(JsonUtility.ToJson(sMap, false), Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(notePath) + ".json"));
			} catch (System.Exception ex) {
				errorMsg = ex.Message;
			}
			// Hint
			if (string.IsNullOrEmpty(errorMsg)) {
				ShowHint("Success! Stager beatmaps created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		public void UI_Stager_To_Voez () {
			var paths = DialogUtil.PickFilesDialog("Pick Stager Beatmap files", "Stager Beatmap", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var sMap = JsonUtility.FromJson<Beatmap>(Util.FileToText(path));
					var vMap = VoezBeatmapData.SMap_to_VMap(sMap);
					string vJson = JsonUtility.ToJson(vMap);
					int noteKeyWordIndex = vJson.IndexOf("m_Notes");
					int trackKeyWordIndex = vJson.IndexOf("m_Tracks");
					int lastRightBracesIndex = vJson.LastIndexOf('}');
					int noteArrStartIndex = noteKeyWordIndex + 9;
					int trackArrStartIndex = trackKeyWordIndex + 10;
					int noteArrLength = 0;
					int trackArrLength = 0;
					if (noteArrStartIndex > trackArrStartIndex) {
						int trackEndCommaIndex = vJson.LastIndexOf(',', noteKeyWordIndex, noteKeyWordIndex - trackArrStartIndex);
						trackArrLength = trackEndCommaIndex - trackArrStartIndex;
						noteArrLength = lastRightBracesIndex - noteArrStartIndex;
					} else {
						int noteEndCommaIndex = vJson.LastIndexOf(',', trackKeyWordIndex, trackKeyWordIndex - noteArrStartIndex);
						noteArrLength = noteEndCommaIndex - noteArrStartIndex;
						trackArrLength = lastRightBracesIndex - trackArrStartIndex;
					}
					string mapName = Util.GetNameWithoutExtension(path);
					string voezNoteJson = vJson.Substring(noteArrStartIndex, noteArrLength);
					string voezTrackJson = vJson.Substring(trackArrStartIndex, trackArrLength);
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Stager_to_Voez");
					Util.CreateFolder(rootPath);
					Util.TextToFile(voezNoteJson, Util.CombinePaths(rootPath, mapName + "_Note.json"));
					Util.TextToFile(voezTrackJson, Util.CombinePaths(rootPath, mapName + "_Track.json"));
					successCount++;
				} catch (System.Exception ex) {
					errorMsg = ex.Message;
				}
			}
			if (successCount > 0) {
				ShowHint("Success! Voez Json files created inside a folder next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		// Deemo
		public void UI_Deemo_To_Stager () {
			var paths = DialogUtil.PickFilesDialog("Pick Deemo JSON files", "Deemo Json", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var json = Util.FileToText(path).Replace("$", "__");
					var dMap = JsonUtility.FromJson<DeemoBeatmapData>(json);
					if (dMap is null) { continue; }
					var sMap = DeemoBeatmapData.DMap_to_SMap(dMap);
					if (sMap is null) { continue; }
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Deemo_to_Stager");
					Util.CreateFolder(rootPath);
					// Map
					Util.TextToFile(JsonUtility.ToJson(sMap, false), Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".json"));
					// Final
					successCount++;
				} catch (System.Exception ex) {
					errorMsg = ex.Message;
				}
			}
			if (successCount > 0) {
				ShowHint("Success! Stager beatmaps created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		public void UI_Stager_To_Deemo () {
			var paths = DialogUtil.PickFilesDialog("Pick Stager Beatmap files", "Stager Beatmap", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var sMap = JsonUtility.FromJson<Beatmap>(Util.FileToText(path));
					var dMap = DeemoBeatmapData.SMap_to_DMap(sMap);
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Stager_to_Deemo");
					Util.CreateFolder(rootPath);
					// Map
					Util.TextToFile(JsonUtility.ToJson(dMap, false).Replace("__", "$"), Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".json"));
					successCount++;
				} catch (System.Exception ex) {
					errorMsg = ex.Message;
				}
			}
			if (successCount > 0) {
				ShowHint("Success! Deemo JSON files created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		// Osu
		public void UI_Osu_To_Stager () {
			var paths = DialogUtil.PickFilesDialog("Pick OSU Map files", "Osu", "osu", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var osu = Util.FileToText(path);
					var sMap = OsuBeatmapData.Osu_to_Stager(osu);
					if (sMap is null) { continue; }
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Osu_to_Stager");
					Util.CreateFolder(rootPath);
					// Map
					Util.TextToFile(JsonUtility.ToJson(sMap, false), Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".json"));
					// Final
					successCount++;
				} catch (System.Exception ex) {
					errorMsg = ex.Message;
				}
			}
			if (successCount > 0) {
				ShowHint("Success! Stager beatmaps created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		public void UI_Stager_To_Osu () {
			var paths = DialogUtil.PickFilesDialog("Pick Stager Beatmap files", "Stager Beatmap", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var sMap = JsonUtility.FromJson<Beatmap>(Util.FileToText(path));
					var osu = OsuBeatmapData.Stager_to_Osu(sMap, Util.GetNameWithoutExtension(path));
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Stager_to_Osu");
					Util.CreateFolder(rootPath);
					// Map
					Util.TextToFile(osu, Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".osu"));
					successCount++;
				} catch (System.Exception ex) {
					errorMsg = ex.Message;
				}
			}
			if (successCount > 0) {
				ShowHint("Success! OSU files created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		// Arcaea
		public void UI_Arcaea_To_Stager () {
			var paths = DialogUtil.PickFilesDialog("Pick Arcaea Map files", "AFF", "aff", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				//try {
				var aMap = Util.FileToText(path);
				var sMap = ArcaeaBeatmapData.Arcaea_To_Stager(aMap);
				if (sMap is null) { continue; }
				var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Arcaea_to_Stager");
				Util.CreateFolder(rootPath);
				// Map
				Util.TextToFile(JsonUtility.ToJson(sMap, false), Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".json"));
				// Final
				successCount++;
				//} catch (System.Exception ex) {
				//	errorMsg = ex.Message;
				//}
			}
			// Hint
			if (successCount > 0) {
				ShowHint("Success! Stager beatmaps created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}

		}


		public void UI_Stager_To_Arcaea () {
			var paths = DialogUtil.PickFilesDialog("Pick Stager Beatmap files", "Stager Beatmap", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var sMap = JsonUtility.FromJson<Beatmap>(Util.FileToText(path));
					var aMap = ArcaeaBeatmapData.Stager_To_Arcaea(sMap);
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Stager_to_Arcaea");
					Util.CreateFolder(rootPath);
					// Map
					Util.TextToFile(aMap, Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".aff"));
					successCount++;
				} catch (System.Exception ex) {
					errorMsg = ex.Message;
				}
			}
			if (successCount > 0) {
				ShowHint("Success! Arcaea files created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		#endregion




		#region --- LGC ---


		private void ShowHint (string msg, bool? success = null) {
			m_HintContainer.gameObject.SetActive(true);
			m_Hint.text = msg;
			m_Mark.sprite = success.HasValue ? success.Value ? m_SuccessMark : m_FailMark : null;
		}


		#endregion




	}
}
