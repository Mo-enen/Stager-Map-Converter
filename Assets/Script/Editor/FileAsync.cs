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
				(
					@"C:\Data\Mine\Unity3D\Project - Stager Studio\Assets\Stager Studio\Script\Data\Gene.cs",
					@"Script\Gene.cs"
				),
				(
					@"C:\Data\Mine\Unity3D\Project - Stager Studio\Assets\Stager Studio\Data\Gene",
					@"Gene"
				),
				(
					@"C:\Data\Mine\Unity3D\Project - Stager Studio\Assets\Stager Studio\Data\Palette",
					@"Palette"
				),
				(
					@"C:\Data\Mine\Unity3D\Project - Stager Studio\Assets\Stager Studio\Data\Tween",
					@"Tween"
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
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			// Set
			var stage = Object.FindObjectOfType<Stage>();
			stage.SetGenes(
				AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Gene/Dynamix Gene.json"),
				AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Gene/Voez Gene.json"),
				AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Gene/Deemo Gene.json")
			);
			stage.SetPals(
				AssetDatabase.LoadAssetAtPath<Texture2D>(@"Assets/Async/Palette/Dynamix Palette.png"),
				AssetDatabase.LoadAssetAtPath<Texture2D>(@"Assets/Async/Palette/Voez Palette.png"),
				AssetDatabase.LoadAssetAtPath<Texture2D>(@"Assets/Async/Palette/Deemo Palette.png")
			);
			stage.SetTweens(
				AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Tween/Dynamix Tween.json"),
				AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Tween/Voez Tween.json"),
				AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Tween/Deemo Tween.json")
			);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}



	}
}