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
			var stage = Object.FindObjectOfType<Stage>();
			EditorUtility.SetDirty(stage);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// Set Assets
			var dyGene = AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Gene/Dynamix Gene.json");
			var vGene = AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Gene/Voez Gene.json");
			var dGene = AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Gene/Deemo Gene.json");

			var dyPal = AssetDatabase.LoadAssetAtPath<Texture2D>(@"Assets/Async/Palette/Dynamix Palette.png");
			var vPal = AssetDatabase.LoadAssetAtPath<Texture2D>(@"Assets/Async/Palette/Voez Palette.png");
			var dPal = AssetDatabase.LoadAssetAtPath<Texture2D>(@"Assets/Async/Palette/Deemo Palette.png");

			var dyTween = AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Tween/Dynamix Tween.json");
			var vTween = AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Tween/Voez Tween.json");
			var dTween = AssetDatabase.LoadAssetAtPath<TextAsset>(@"Assets/Async/Tween/Deemo Tween.json");

			EditorUtility.SetDirty(dyGene);
			EditorUtility.SetDirty(vGene);
			EditorUtility.SetDirty(dGene);
			EditorUtility.SetDirty(dyPal);
			EditorUtility.SetDirty(vPal);
			EditorUtility.SetDirty(dPal);
			EditorUtility.SetDirty(dyTween);
			EditorUtility.SetDirty(vTween);
			EditorUtility.SetDirty(dTween);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// Read Write 
			var dyImporter = AssetImporter.GetAtPath(@"Assets/Async/Palette/Dynamix Palette.png") as TextureImporter;
			dyImporter.isReadable = true;
			var vImporter = AssetImporter.GetAtPath(@"Assets/Async/Palette/Voez Palette.png") as TextureImporter;
			vImporter.isReadable = true;
			var dImporter = AssetImporter.GetAtPath(@"Assets/Async/Palette/Deemo Palette.png") as TextureImporter;
			dImporter.isReadable = true;

			EditorUtility.SetDirty(dyGene);
			EditorUtility.SetDirty(vGene);
			EditorUtility.SetDirty(dGene);
			EditorUtility.SetDirty(dyPal);
			EditorUtility.SetDirty(vPal);
			EditorUtility.SetDirty(dPal);
			EditorUtility.SetDirty(dyTween);
			EditorUtility.SetDirty(vTween);
			EditorUtility.SetDirty(dTween);
			EditorUtility.SetDirty(dyImporter);
			EditorUtility.SetDirty(vImporter);
			EditorUtility.SetDirty(dImporter);

			dyImporter.SaveAndReimport();
			vImporter.SaveAndReimport();
			dImporter.SaveAndReimport();

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			stage.SetGenes(dyGene, vGene, dGene);
			stage.SetPals(dyPal, vPal, dPal);
			stage.SetTweens(dyTween, vTween, dTween);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}



	}
}