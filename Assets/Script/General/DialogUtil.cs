namespace StagerStudio {
	using UnityEngine;
	using Crosstales.FB;


	public static class DialogUtil {


		public static string PickFolderDialog (string title) {
			var lastPickedFolder = PlayerPrefs.GetString(
				"DialogUtil.LastPickedFolder",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
			);
			string path = FileBrowser.OpenSingleFolder(title, lastPickedFolder);
			if (!string.IsNullOrEmpty(path)) {
				PlayerPrefs.SetString("DialogUtil.LastPickedFolder", GetParentPath(path));
				return path;
			}
			return "";
		}


		public static string CreateFolderDialog (string title, string defaultName) => CreateFileDialog(title, defaultName, "");


		public static string PickFileDialog (string title, string filterName, params string[] filters) {
			var lastPickedFolder = PlayerPrefs.GetString(
				"DialogUtil.LastPickedFolder",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
			);
			var path = FileBrowser.OpenSingleFile(title, lastPickedFolder, new ExtensionFilter[1] { new ExtensionFilter(filterName, filters) });
			if (!string.IsNullOrEmpty(path)) {
				PlayerPrefs.SetString("DialogUtil.LastPickedFolder", GetParentPath(path));
				return path;
			}
			return "";
		}


		public static string[] PickFilesDialog (string title, string filterName, params string[] filters) {
			var lastPickedFolder = PlayerPrefs.GetString(
				"DialogUtil.LastPickedFolder",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
			);
			var paths = FileBrowser.OpenFiles(title, lastPickedFolder, new ExtensionFilter[1] { new ExtensionFilter(filterName, filters) });
			if (!(paths is null) && paths.Length != 0) {
				PlayerPrefs.SetString("DialogUtil.LastPickedFolder", GetParentPath(paths[0]));
			}
			return paths;
		}


		public static string CreateFileDialog (string title, string defaultName, string ext) {
			var lastPickedFolder = PlayerPrefs.GetString(
				"DialogUtil.LastPickedFolder",
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop)
			);
			var path = FileBrowser.SaveFile(title, lastPickedFolder, defaultName, ext);
			if (!string.IsNullOrEmpty(path)) {
				PlayerPrefs.SetString("DialogUtil.LastPickedFolder", GetParentPath(path));
				return path;
			}
			return "";
		}


		private static string GetParentPath (string path) => System.IO.Directory.GetParent(path).FullName;


	}
}