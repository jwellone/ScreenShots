using UnityEngine;
using UnityEditor;

#nullable enable

namespace jwelloneEditor
{
	public class ScreenShotsSettingWindow : EditorWindow
	{
		Texture2D? _folderIcon;

		[MenuItem("jwellone/Screenshot/Settings", false, 900)]
		static void Open()
		{
			var window = EditorWindow.CreateInstance<ScreenShotsSettingWindow>();
			window.titleContent.text = "Screen Shot Settings";
			window.maximized = false;
			window.minSize = window.maxSize = new Vector2(512, 70);
			window.ShowAuxWindow();
		}

		void OnEnable()
		{
			_folderIcon = EditorGUIUtility.Load("d_FolderOpened Icon") as Texture2D;
		}

		void OnGUI()
		{
			var openFinder = EditorGUILayout.Toggle("Open in finder after output", ScreenShots.isOpenFinder);
			if (openFinder != ScreenShots.isOpenFinder)
			{
				ScreenShots.isOpenFinder = openFinder;
			}

			var logEnabled = EditorGUILayout.Toggle("Log enabled", ScreenShots.logEnabled);
			if (logEnabled != ScreenShots.logEnabled)
			{
				ScreenShots.logEnabled = logEnabled;
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Output path:", GUILayout.Width(72));
			EditorGUILayout.TextField(ScreenShots.outPutPath);
			if (GUILayout.Button(_folderIcon, GUILayout.Width(36)))
			{
				var path = EditorUtility.OpenFolderPanel("Select output path", ScreenShots.outPutPath, "");
				if (string.IsNullOrEmpty(path))
				{
					return;
				}

				ScreenShots.outPutPath = path;
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}