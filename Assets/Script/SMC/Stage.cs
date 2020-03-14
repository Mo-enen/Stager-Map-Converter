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
		[SerializeField] private Image m_Mark = null;
		[SerializeField] private Sprite m_SuccessMark = null;
		[SerializeField] private Sprite m_FailMark = null;
		[Header("Assets")]
		[SerializeField] private TextAsset m_DynamixGene = null;
		[SerializeField] private TextAsset m_VoezGene = null;
		[SerializeField] private TextAsset m_DeemoGene = null;
		[SerializeField] private TextAsset m_DynamixTween = null;
		[SerializeField] private TextAsset m_VoezTween = null;
		[SerializeField] private TextAsset m_DeemoTween = null;
		[SerializeField] private Texture2D m_DynamixPalette = null;
		[SerializeField] private Texture2D m_VoezPalette = null;
		[SerializeField] private Texture2D m_DeemoPalette = null;


		#endregion




		#region --- MSG ---


		private void Awake () {
			Application.targetFrameRate = 60;
			QualitySettings.vSyncCount = 2;
			TooltipUI.TipLabel = m_Tip;
		}



		#endregion




		#region --- API ---


		// Dynamix
		public void UI_Dynamix_To_Stager () {
			var paths = DialogUtil.PickFilesDialog("Pick Dynamix XML files", "Dynamix XML", "xml", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
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
					// Assets
					Util.TextToFile(m_DynamixGene.text, Util.CombinePaths(rootPath, "Assets", m_DynamixGene.name + ".json"));
					Util.TextToFile(m_DynamixTween.text, Util.CombinePaths(rootPath, "Assets", m_DynamixTween.name + ".json"));
					Util.ByteToFile(m_DynamixPalette.EncodeToPNG(), Util.CombinePaths(rootPath, "Assets", m_DynamixPalette.name + ".png"));
					// Final
					successCount++;
				} catch { }
			}
			if (successCount > 0) {
				ShowHint("Success! Stager beatmaps created next to the original file.", true);
			} else {
				ShowHint("Fail! Can\'t load xml file.", false);
			}
		}


		public void UI_Stager_To_Dynamix () {
			var paths = DialogUtil.PickFilesDialog("Pick Stager Beatmap files", "Stager Beatmap", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			foreach (var path in paths) {
				try {
					var sMap = JsonUtility.FromJson<Beatmap>(Util.FileToText(path));
					var dMap = DynamixBeatmapData.SMap_to_DMap(sMap);
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Stager_to_Dynamix");
					Util.CreateFolder(rootPath);
					// Map
					Util.WriteXML(dMap, Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".xml"));
					successCount++;
				} catch { }
			}
			if (successCount > 0) {
				ShowHint("Success! Dynamix XML files created next to the original file.", true);
			} else {
				ShowHint("Fail! Can\'t load stager file.", false);
			}
		}


		// Voez
		public void UI_Voez_To_Stager () {
			var trackPath = DialogUtil.PickFileDialog("Pick Voez [Track]", "Voez Track", "json", "txt");
			if (string.IsNullOrEmpty(trackPath)) { return; }
			var notePath = DialogUtil.PickFileDialog("Pick Voez [Note]", "Voez Note", "json", "txt");
			if (string.IsNullOrEmpty(notePath)) { return; }

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
			// Assets
			Util.TextToFile(m_VoezGene.text, Util.CombinePaths(rootPath, "Assets", m_VoezGene.name + ".json"));
			Util.TextToFile(m_VoezTween.text, Util.CombinePaths(rootPath, "Assets", m_VoezTween.name + ".json"));
			Util.ByteToFile(m_VoezPalette.EncodeToPNG(), Util.CombinePaths(rootPath, "Assets", m_VoezPalette.name + ".png"));
			// Hint
			ShowHint("Success! Stager beatmaps created next to the original file.", true);
		}


		public void UI_Stager_To_Voez () {
			var paths = DialogUtil.PickFilesDialog("Pick Stager Beatmap files", "Stager Beatmap", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
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
				} catch { }
			}
			if (successCount > 0) {
				ShowHint("Success! Voez Json files created inside a folder next to the original file.", true);
			} else {
				ShowHint("Fail! Can\'t load stager file.", false);
			}
		}


		// Deemo
		public void UI_Deemo_To_Stager () {
			var paths = DialogUtil.PickFilesDialog("Pick Deemo JSON files", "Deemo Json", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
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
					// Assets
					Util.TextToFile(m_DeemoGene.text, Util.CombinePaths(rootPath, "Assets", m_DeemoGene.name + ".json"));
					Util.TextToFile(m_DeemoTween.text, Util.CombinePaths(rootPath, "Assets", m_DeemoTween.name + ".json"));
					Util.ByteToFile(m_DeemoPalette.EncodeToPNG(), Util.CombinePaths(rootPath, "Assets", m_DeemoPalette.name + ".png"));
					// Final
					successCount++;
				} catch { }
			}
			if (successCount > 0) {
				ShowHint("Success! Stager beatmaps created next to the original file.", true);
			} else {
				ShowHint("Fail! Can\'t load json file.", false);
			}
		}


		public void UI_Stager_To_Deemo () {
			var paths = DialogUtil.PickFilesDialog("Pick Stager Beatmap files", "Stager Beatmap", "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			foreach (var path in paths) {
				try {
					var sMap = JsonUtility.FromJson<Beatmap>(Util.FileToText(path));
					var dMap = DeemoBeatmapData.SMap_to_DMap(sMap);
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), "Stager_to_Deemo");
					Util.CreateFolder(rootPath);
					// Map
					Util.TextToFile(JsonUtility.ToJson(dMap, false).Replace("__", "$"), Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".json"));
					successCount++;
				} catch { }
			}
			if (successCount > 0) {
				ShowHint("Success! Deemo JSON files created next to the original file.", true);
			} else {
				ShowHint("Fail! Can\'t load stager file.", false);
			}
		}


		// Set Assets from Sync
		public void SetGenes (TextAsset dynamixGene, TextAsset voezGene, TextAsset deemoGene) {
			m_DynamixGene = dynamixGene;
			m_VoezGene = voezGene;
			m_DeemoGene = deemoGene;
		}


		public void SetPals (Texture2D dynamixPal, Texture2D voezPal, Texture2D deemoPal) {
			m_DynamixPalette = dynamixPal;
			m_VoezPalette = voezPal;
			m_DeemoPalette = deemoPal;
		}


		public void SetTweens (TextAsset dynamixTween, TextAsset voezTween, TextAsset deemoTween) {
			m_DynamixTween = dynamixTween;
			m_VoezTween = voezTween;
			m_DeemoTween = deemoTween;
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
