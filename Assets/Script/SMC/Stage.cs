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
	using UnityEngine.EventSystems;

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
		public void UI_Dynamix_To_Stager (BaseEventData be) => Map_to_Stager(
			"Dynamix",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path) => DynamixBeatmapData.DMap_to_SMap(Util.ReadXML<DynamixBeatmapData>(path)),
			"xml", "txt"
		);


		public void UI_Stager_To_Dynamix (BaseEventData be) => Stager_to_Map(
			"Dynamix",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path, name, sMap) => Util.WriteXML(DynamixBeatmapData.SMap_to_DMap(sMap),
			Util.CombinePaths(path, name + ".xml"))
		);


		// Voez
		public void UI_Voez_To_Stager () => Map_to_Stager(
			"Voez", "Track", "Note", (track, note) => {
				var voezMap = JsonUtility.FromJson<VoezBeatmapData>(@"{""m_Tracks"":" + Util.FileToText(track) + @", ""m_Notes"":" + Util.FileToText(note) + @"}");
				return VoezBeatmapData.VMap_to_SMap(voezMap);
			}, "json", "txt");


		public void UI_Stager_To_Voez (BaseEventData be) => Stager_to_Map(
			"Voez",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path, name, sMap) => {
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
				string voezNoteJson = vJson.Substring(noteArrStartIndex, noteArrLength);
				string voezTrackJson = vJson.Substring(trackArrStartIndex, trackArrLength);
				Util.TextToFile(voezNoteJson, Util.CombinePaths(path, name + "_Note.json"));
				Util.TextToFile(voezTrackJson, Util.CombinePaths(path, name + "_Track.json"));
			}
		);



		// Deemo
		public void UI_Deemo_To_Stager (BaseEventData be) => Map_to_Stager(
			"Deemo",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path) => DeemoBeatmapData.DMap_to_SMap(JsonUtility.FromJson<DeemoBeatmapData>(Util.FileToText(path).Replace("$", "__"))),
			"json", "txt"
		);


		public void UI_Stager_To_Deemo (BaseEventData be) => Stager_to_Map(
			"Deemo",
			(be as PointerEventData).button == PointerEventData.InputButton.Right, (path, name, sMap) => Util.TextToFile(
			JsonUtility.ToJson(DeemoBeatmapData.SMap_to_DMap(sMap), false).Replace("__", "$"),
			Util.CombinePaths(path, name + ".json")
		));


		// Osu
		public void UI_Osu_To_Stager (BaseEventData be) => Map_to_Stager(
			"OSU",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path) => OsuBeatmapData.Osu_to_Stager(Util.FileToText(path)),
			"osu", "txt"
		);


		public void UI_Stager_To_Osu (BaseEventData be) => Stager_to_Map(
			"OSU",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path, name, sMap) => Util.TextToFile(
			OsuBeatmapData.Stager_to_Osu(sMap, name),
			Util.CombinePaths(path, name + ".osu")
		));


		// Arcaea
		public void UI_Arcaea_To_Stager (BaseEventData be) => Map_to_Stager(
			"Arcaea",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path) => ArcaeaBeatmapData.Arcaea_To_Stager(Util.FileToText(path)),
			"aff", "txt"
		);


		public void UI_Stager_To_Arcaea (BaseEventData be) => Stager_to_Map(
			"Arcaea",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path, name, sMap) => Util.TextToFile(
			ArcaeaBeatmapData.Stager_To_Arcaea(sMap),
			Util.CombinePaths(path, name + ".aff")
		));


		// BMS
		public void UI_BMS_To_Stager (BaseEventData be) => Map_to_Stager(
			"BMS",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path) => BmsBeatmapData.Bms_To_Stager(Util.FileToText(path)),
			"bms", "bme", "bml", "txt"
		);


		public void UI_Stager_To_BMS (BaseEventData be) => Stager_to_Map(
			"BMS",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path, name, sMap) => Util.TextToFile(
			BmsBeatmapData.Stager_To_Bms(sMap, name),
			Util.CombinePaths(path, name + ".bms")
		));


		// KSH
		public void UI_KSM_To_Stager (BaseEventData be) => Map_to_Stager(
			"KShootMania",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path) => KsmBeatmapData.KSM_to_Stager(Util.FileToText(path)),
			"ksh", "txt"
		);


		public void UI_Stager_To_KSM (BaseEventData be) => Stager_to_Map(
			"KShootMania",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path, name, sMap) => Util.TextToFile(
			KsmBeatmapData.Stager_to_KSM(sMap, name),
			Util.CombinePaths(path, name + ".ksh")
		));


		// Phigros
		public void UI_Phigros_To_Stager (BaseEventData be) => Map_to_Stager(
			"Phigros",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path) => PhigrosBeatmapData.Phigros_to_Stager(JsonUtility.FromJson<PhigrosBeatmapData>(Util.FileToText(path))),
			"json", "txt"
		);


		public void UI_Stager_To_Phigros (BaseEventData be) => Stager_to_Map(
			"Phigros",
			(be as PointerEventData).button == PointerEventData.InputButton.Right,
			(path, name, sMap) => Util.TextToFile(
			JsonUtility.ToJson(PhigrosBeatmapData.Stager_to_Phigros(sMap), false),
			Util.CombinePaths(path, name + ".txt")
		));



		#endregion




		#region --- LGC ---


		private void ShowHint (string msg, bool? success = null) {
			m_HintContainer.gameObject.SetActive(true);
			m_Hint.text = msg;
			m_Mark.sprite = success.HasValue ? success.Value ? m_SuccessMark : m_FailMark : null;
		}


		private void Map_to_Stager (
			string mapName, bool forAll,
			System.Func<string, Beatmap> convert,
			params string[] exts
		) {
			var paths = GetPathsFromDialog(mapName, forAll, exts);
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var sMap = convert(path);
					if (sMap is null) { continue; }
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), $"{mapName}_to_Stager");
					Util.CreateFolder(rootPath);
					Util.TextToFile(JsonUtility.ToJson(sMap, false), Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(path) + ".json"));
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


		private void Map_to_Stager (
			string mapName, string partA, string partB,
			System.Func<string, string, Beatmap> convert,
			params string[] exts
		) {
			var aPath = DialogUtil.PickFileDialog($"Pick {mapName} [{partA}]", $"{mapName} {partA}", exts);
			if (string.IsNullOrEmpty(aPath)) { return; }
			var bPath = DialogUtil.PickFileDialog($"Pick {mapName} [{partB}]", $"{mapName} {partB}", exts);
			if (string.IsNullOrEmpty(bPath)) { return; }
			string errorMsg = "";
			try {
				var sMap = convert(aPath, bPath);
				var rootPath = Util.CombinePaths(Util.GetParentPath(bPath), $"{mapName}_to_Stager");
				Util.CreateFolder(rootPath);
				// Map
				Util.TextToFile(JsonUtility.ToJson(sMap, false), Util.CombinePaths(rootPath, Util.GetNameWithoutExtension(bPath) + ".json"));
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


		private void Stager_to_Map (
			string mapName, bool forAll,
			System.Action<string, string, Beatmap> convert
		) {
			var paths = GetPathsFromDialog("Stager", forAll, "json", "txt");
			if (paths is null || paths.Length == 0) { return; }
			int successCount = 0;
			string errorMsg = "";
			foreach (var path in paths) {
				try {
					var sMap = JsonUtility.FromJson<Beatmap>(Util.FileToText(path));
					if (sMap == null) { continue; }
					var rootPath = Util.CombinePaths(Util.GetParentPath(path), $"Stager_to_{mapName}");
					Util.CreateFolder(rootPath);
					convert(rootPath, Util.GetNameWithoutExtension(path), sMap);
					successCount++;
				} catch (System.Exception ex) {
					errorMsg = ex.Message;
				}
			}
			if (successCount > 0) {
				ShowHint($"Success! {mapName} JSON files created next to the original file.", true);
			} else {
				ShowHint("Fail!\n" + errorMsg, false);
			}
		}


		private string[] GetPathsFromDialog (string mapName, bool forAll, params string[] exts) {
			string[] paths = null;
			if (forAll) {
				string folderPath = DialogUtil.PickFolderDialog($"Pick folder for {mapName} Map");
				if (!string.IsNullOrEmpty(folderPath)) {
					string[] fixedExts = new string[exts.Length];
					for (int i = 0; i < fixedExts.Length; i++) {
						fixedExts[i] = "*." + exts[i];
					}
					var files = Util.GetFilesIn(folderPath, false, fixedExts);
					paths = new string[files.Length];
					for (int i = 0; i < paths.Length; i++) {
						paths[i] = files[i].FullName;
					}
				}
			} else {
				paths = DialogUtil.PickFilesDialog($"Pick {mapName} Map files", $"{mapName} Map", exts);
			}
			return paths;
		}


		#endregion




	}
}
