namespace Moenen {
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Reflection;


	public class MoenenTools {



		// & alt   % ctrl   # Shift
		[MenuItem("Tools/Clear Console And ApplyPrefabs _F5")]
		public static void ClearAndReStage () {
			// Clear Console
			var assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
			var type = assembly.GetType("UnityEditor.LogEntries");
			if (type == null) {
				type = assembly.GetType("UnityEditorInternal.LogEntries");
			}
			var method = type.GetMethod("Clear");
			method.Invoke(new object(), null);
		}




	}



}