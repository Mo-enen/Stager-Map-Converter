namespace StagerStudio.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;


	public static class FileAsync {



		[MenuItem("Tools/Sync from StagerStudio")]
		public static void SyncBeatmapCS () {
			// Sync
			(string source, string target)[] FILE_PATH = {
				(
					@"C:\Data\Mine\Unity3D\Project - Stager Studio\Assets\Stager Studio\Script\Data\Beatmap.cs",
					@"Script\Beatmap.cs"
				),
			};
			const string TARGET_ROOT = @"C:\Data\Mine\Unity3D\Project - Stager Studio Map Converter\Assets\Async";
			Util.DeleteAllFilesIn(TARGET_ROOT);
			foreach (var (source, target) in FILE_PATH) {
				var targetPath = Util.CombinePaths(TARGET_ROOT, target);
				if (Util.FileExists(source)) {
					Util.CopyFile(source, targetPath);
				} else if (Util.DirectoryExists(source)) {
					Util.CopyDirectory(source, targetPath, true, true);
				} else {
					Debug.LogWarning($"Source file/folder not exists ({source})");
				}
			}
			var stage = Object.FindObjectOfType<Stage>();
			EditorUtility.SetDirty(stage);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}



	}
}