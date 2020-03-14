namespace Moenen {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using System.IO;
	using System.Linq;


	public struct Util {




		#region --- File ---



		public static string FileToText (string path) {
			StreamReader sr = File.OpenText(path);
			string data = sr.ReadToEnd();
			sr.Close();
			return data;
		}


		public static string FileFirstLineToText (string path) {
			StreamReader sr = File.OpenText(path);
			string data = sr.ReadLine();
			sr.Close();
			return data;
		}


		public static void TextToFile (string data, string path) {
			FileStream fs = new FileStream(path, FileMode.Create);
			StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
			sw.Write(data);
			sw.Close();
			fs.Close();
		}




		public static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !DirectoryExists(path)) {
				string pPath = GetParentPath(path);
				if (!DirectoryExists(pPath)) {
					CreateFolder(pPath);
				}
				Directory.CreateDirectory(path);
			}
		}





		public static byte[] FileToByte (string path) {
			byte[] bytes = null;
			if (FileExists(path)) {
				bytes = File.ReadAllBytes(path);
			}
			return bytes;
		}



		public static void ByteToFile (byte[] bytes, string path) {
			string parentPath = GetParentPath(path);
			CreateFolder(parentPath);
			FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();
			fs.Dispose();
		}




		public static bool HasFileIn (string path, params string[] searchPattern) {
			if (PathIsDirectory(path)) {
				for (int i = 0; i < searchPattern.Length; i++) {
					if (new DirectoryInfo(path).GetFiles(searchPattern[i], SearchOption.AllDirectories).Length > 0) {
						return true;
					}
				}
			}
			return false;
		}



		public static FileInfo[] GetFilesIn (string path, params string[] searchPattern) {
			List<FileInfo> allFiles = new List<FileInfo>();
			if (PathIsDirectory(path)) {
				if (searchPattern.Length == 0) {
					allFiles.AddRange(new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories));
				} else {
					for (int i = 0; i < searchPattern.Length; i++) {
						allFiles.AddRange(new DirectoryInfo(path).GetFiles(searchPattern[i], SearchOption.AllDirectories));
					}
				}
			}
			return allFiles.ToArray();
		}



		public static void DeleteFile (string path) {
			if (FileExists(path)) {
				File.Delete(path);
			}
		}



		public static void CopyFile (string from, string to) {
			if (FileExists(from)) {
				// Create Parent
				string pPath = GetParentPath(to);
				if (!DirectoryExists(pPath)) {
					CreateFolder(pPath);
				}
				// Copy
				File.Copy(from, to, true);
			}
		}



		public static bool CopyDirectory (string from, string to, bool copySubDirs, bool ignoreHidden) {

			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(from);

			if (!dir.Exists) {
				return false;
			}

			// Create Parent
			string pPath = GetParentPath(to);
			if (!DirectoryExists(pPath)) {
				CreateFolder(pPath);
			}

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(to)) {
				Directory.CreateDirectory(to);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				string temppath = Path.Combine(to, file.Name);
				if (!ignoreHidden || (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
					file.CopyTo(temppath, true);
				}
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs) {
				foreach (DirectoryInfo subdir in dirs) {
					string temppath = Path.Combine(to, subdir.Name);
					if (!ignoreHidden || (subdir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
						CopyDirectory(subdir.FullName, temppath, copySubDirs, ignoreHidden);
					}

				}
			}
			return true;
		}



		public static void DeleteDirectory (string path) {
			if (DirectoryExists(path)) {
				Directory.Delete(path, true);
			}
		}



		public static void DeleteAllFilesIn (string path) {
			if (DirectoryExists(path)) {
				var files = GetFilesIn(path, "*");
				foreach (var file in files) {
					DeleteFile(file.FullName);
				}
			}
		}



		public static float GetFileSize (string path) {
			float size = -1f;
			if (FileExists(path)) {
				var info = new FileInfo(path);
				size = (info.Length / 1024f) / 1024f;
			}
			return size;
		}




		public static int GetFileCount (string path, string search = "", SearchOption option = SearchOption.TopDirectoryOnly) {
			if (DirectoryExists(path)) {
				return Directory.EnumerateFiles(path, search, option).Count();
			}
			return 0;
		}




		#endregion




		#region --- Path ---




		public static string GetParentPath (string path) => Directory.GetParent(path).FullName;



		public static string GetFullPath (string path) => new FileInfo(path).FullName;



		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return path;
		}



		public static string GetExtension (string path) => Path.GetExtension(path);//.txt



		public static string GetNameWithoutExtension (string path) => Path.GetFileNameWithoutExtension(path);


		public static string GetNameWithExtension (string path) => Path.GetFileName(path);


		public static string ChangeExtension (string path, string newEx) => Path.ChangeExtension(path, newEx);



		public static bool DirectoryExists (string path) => Directory.Exists(path);



		public static bool FileExists (string path) => File.Exists(path);



		public static bool PathIsDirectory (string path) {
			if (!DirectoryExists(path)) { return false; }
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}



		public static bool IsChildPath (string pathA, string pathB) {
			if (pathA.Length == pathB.Length) {
				return pathA == pathB;
			} else if (pathA.Length > pathB.Length) {
				return IsChildPathCompair(pathA, pathB);
			} else {
				return IsChildPathCompair(pathB, pathA);
			}
		}



		public static bool IsChildPathCompair (string longPath, string path) {
			if (longPath.Length <= path.Length || !PathIsDirectory(path) || !longPath.StartsWith(path)) {
				return false;
			}
			char c = longPath[path.Length];
			if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar) {
				return false;
			}
			return true;
		}



		public static string GetUrl (string path) => string.IsNullOrEmpty(path) ? "" : new System.Uri(path).AbsoluteUri;



		public static AudioType GetAudioType (string path) {
			var ex = GetExtension(path);
			switch (ex) {
				default:
					return AudioType.UNKNOWN;
				case ".mp3":
					return AudioType.MPEG;
				case ".ogg":
					return AudioType.OGGVORBIS;
				case ".wav":
					return AudioType.WAV;
			}
		}



		public static string GetTimeString () {
			return System.DateTime.Now.ToString("yyyyMMddTHHmmss");
		}



		#endregion



	}

}