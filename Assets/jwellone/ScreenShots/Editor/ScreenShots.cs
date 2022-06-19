using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace jwelloneEditor
{
	static class ScreenShots
	{
		const string EDITOR_USER_KEY_OPEN_FINDER = "EUS_KEY_SS_OPEN_FINDER";
		const string EDITOR_USER_KEY_OUTPUT_PATH = "EUS_KEY_SS_OUTPUT_PATH";
		const string EDITOR_USER_KEY_LOG_DISABLED = "EUS_KEY_SS_LOG_DISABLED";
		static readonly string DEFAULT_OUTPUT_PATH = Path.GetFullPath($"{Application.dataPath}/../ScreenShot/");
		const BindingFlags BIND_FLAGS = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

		static Type? _tSceneView;
		static Type? tSceneView => _tSceneView ??= typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneView");

		static Type? _tScreenShots;
		static Type? tScreenShots => _tScreenShots ??= Type.GetType("UnityEditor.ScreenShots, UnityEditor");

		static MethodInfo? _miScreenshot;
		static MethodInfo? miScreenshot => _miScreenshot ??= tScreenShots?.GetMethod("Screenshot", BIND_FLAGS);

		static MethodInfo? _miSetMainWindowSizeSmall;
		static MethodInfo? miSetMainWindowSizeSmall => _miSetMainWindowSizeSmall ??= tScreenShots?.GetMethod("SetMainWindowSizeSmall", BIND_FLAGS);

		static MethodInfo? _miSetMainWindowSize;
		static MethodInfo? miSetMainWindowSize => _miSetMainWindowSize ??= tScreenShots?.GetMethod("SetMainWindowSize", BIND_FLAGS);

		static MethodInfo? _miScreenshotToolbar;
		static MethodInfo? miScreenshotToolbar => _miScreenshotToolbar ??= tScreenShots?.GetMethod("ScreenshotToolbar", BIND_FLAGS);

		static MethodInfo? _miScreenshotExtendedRight;
		static MethodInfo? miScreenshotExtendedRight => _miScreenshotExtendedRight ??= tScreenShots?.GetMethod("ScreenshotExtendedRight", BIND_FLAGS);

		static MethodInfo? _miSaveScreenShot;
		static MethodInfo? miSaveScreenShot => _miSaveScreenShot ??= tScreenShots?.GetMethod("SaveScreenShot", BindingFlags.Public | BindingFlags.Static);

		static MethodInfo? _miGetUniquePathForName;
		static MethodInfo? miGetUniquePathForName => _miGetUniquePathForName ??= tScreenShots?.GetMethod("GetUniquePathForName", BIND_FLAGS);

		static MethodInfo? _miGetMouseOverView;
		static MethodInfo? miGetMouseOverView => _miGetMouseOverView ??= tScreenShots?.GetMethod("GetMouseOverView", BIND_FLAGS);

		static MethodInfo? _miGetGUIViewName;
		static MethodInfo? miGetGUIViewName => _miGetGUIViewName ??= tScreenShots?.GetMethod("GetGUIViewName", BIND_FLAGS);

		public static bool isOpenFinder
		{
			get
			{
				return EditorUserSettings.GetConfigValue(EDITOR_USER_KEY_OPEN_FINDER) == "true";
			}

			set
			{
				EditorUserSettings.SetConfigValue(EDITOR_USER_KEY_OPEN_FINDER, value ? "true" : "false");
			}
		}

		public static bool logEnabled
		{
			get
			{
				return !(EditorUserSettings.GetConfigValue(EDITOR_USER_KEY_LOG_DISABLED) == "true");
			}
			set
			{
				EditorUserSettings.SetConfigValue(EDITOR_USER_KEY_LOG_DISABLED, !value ? "true" : "false");
			}
		}

		public static string outPutPath
		{
			get
			{
				var path = EditorUserSettings.GetConfigValue(EDITOR_USER_KEY_OUTPUT_PATH);
				return string.IsNullOrEmpty(path) ? DEFAULT_OUTPUT_PATH : path;
			}

			set
			{
				EditorUserSettings.SetConfigValue(EDITOR_USER_KEY_OUTPUT_PATH, value);
			}
		}

		static string guiViewName => (string)miGetGUIViewName?.Invoke(null, new object[] { miGetMouseOverView?.Invoke(null, null)! })!;

		[MenuItem("jwellone/Screenshot/Set Window Size %&l", false, 1000)]
		public static void SetMainWindowSize()
		{
			miSetMainWindowSize?.Invoke(null, null);
		}

		[MenuItem("jwellone/Screenshot/Set Window Size Small", false, 1000)]
		public static void SetMainWindowSizeSmall()
		{
			miSetMainWindowSizeSmall?.Invoke(null, null);
		}

		[MenuItem("jwellone/Screenshot/Snap View %&j", false)]
		public static void Screenshot()
		{
			var tmp = Debug.unityLogger.logEnabled;
			Debug.unityLogger.logEnabled = false;
			miScreenshot?.Invoke(null, null);
			PostOutputProcessing(guiViewName);
			Debug.unityLogger.logEnabled = tmp;
		}

		[MenuItem("jwellone/Screenshot/Snap View Toolbar", false, 1000)]
		public static void ScreenshotToolbar()
		{
			var tmp = Debug.unityLogger.logEnabled;
			Debug.unityLogger.logEnabled = false;
			miScreenshotToolbar?.Invoke(null, null);
			PostOutputProcessing($"{guiViewName}Toolbar");
			Debug.unityLogger.logEnabled = tmp;
		}

		[MenuItem("jwellone/Screenshot/Snap View Extended Right %&k", false, 1000)]
		public static void ScreenshotExtendedRight()
		{
			var tmp = Debug.unityLogger.logEnabled;
			Debug.unityLogger.logEnabled = false;
			miScreenshotExtendedRight?.Invoke(null, null);
			PostOutputProcessing($"{guiViewName}Extended");
			Debug.unityLogger.logEnabled = tmp;
		}

		[MenuItem("jwellone/Screenshot/Snap Game View(x1) %&g", false, 1000)]
		public static void ScreenGameViewContent()
		{
			TakeGameView(1);
		}

		[MenuItem("jwellone/Screenshot/Snap Game View(x2)", false, 1000)]
		public static void ScreenGameViewX2()
		{
			TakeGameView(2);
		}

		[MenuItem("jwellone/Screenshot/Snap Game View(x3)", false, 1000)]
		public static void ScreenGameViewX3()
		{
			TakeGameView(3);
		}

		[MenuItem("jwellone/Screenshot/Snap Game View(x4)", false, 1000)]
		public static void ScreenGameViewX4()
		{
			TakeGameView(4);
		}

		[MenuItem("jwellone/Screenshot/Snap Scene View %&s", false, 1000)]
		public static void ScreenSceneView()
		{
			var path = (string)(miGetUniquePathForName?.Invoke(null, new object[] { "SceneView" }))!;
			TakeSceneView(path);
		}

		[MenuItem("jwellone/Screenshot/Snap Editor %&e", false, 1000)]
		public static void ScreenUnityEditor()
		{
			var tmp = Debug.unityLogger.logEnabled;
			Debug.unityLogger.logEnabled = false;
			SaveScreenShot(GetMainWindowPosition(), "Editor");
			PostOutputProcessing("Editor");
			Debug.unityLogger.logEnabled = tmp;
		}


		public static void SaveScreenShot(Rect r, string name)
		{
			miSaveScreenShot?.Invoke(null, new object[] { r, name });
		}

		static void TakeGameView(int superSize)
		{
			var path = (string)(miGetUniquePathForName?.Invoke(null, new object[] { "GameView" }))!;
			ScreenCapture.CaptureScreenshot(path, superSize);
			var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
			EditorWindow.GetWindow(type).Repaint();
			PostOutputProcessing("GameView");
		}

		static void TakeSceneView(string filePath)
		{
			void onWrite()
			{
				var texture = GenerateCapturedTexture(SceneView.lastActiveSceneView.camera);
				var bytes = texture.EncodeToPNG();
				Texture.DestroyImmediate(texture);
				File.WriteAllBytes(filePath, bytes);
				PostOutputProcessing("SceneView");
			}

			if (SceneView.lastActiveSceneView == null)
			{
				EditorWindow.GetWindow(tSceneView).Repaint();
				EditorApplication.delayCall += () =>
				{
					onWrite();
				};
				return;
			}

			onWrite();
		}

		static Texture2D GenerateCapturedTexture(Camera targetCamera, TextureFormat format = TextureFormat.RGB24)
		{
			var temporaryRT = targetCamera.targetTexture == null ? RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat) : null;
			if (temporaryRT)
			{
				targetCamera.targetTexture = temporaryRT;
			}

			var targetTexture = targetCamera.targetTexture!;
			var texture = new Texture2D(targetTexture.width, targetTexture.height, format, false);
			var tmpRT = RenderTexture.active;

			RenderTexture.active = targetTexture;
			targetCamera.Render();
			texture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);

			RenderTexture.active = tmpRT;

			if (temporaryRT != null)
			{
				targetCamera.targetTexture = null;
				RenderTexture.ReleaseTemporary(temporaryRT);
			}

			return texture;
		}

		static Rect GetMainWindowPosition()
		{
			var windowType = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => t.IsSubclassOf(typeof(ScriptableObject)))
				.FirstOrDefault(t => t.Name == "ContainerWindow");

			if (windowType == null)
			{
				return Rect.zero;
			}

			var field = windowType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
			var property = windowType.GetProperty("position", BindingFlags.Public | BindingFlags.Instance);

			if (field == null || property == null)
			{
				return Rect.zero;
			}

			foreach (var window in Resources.FindObjectsOfTypeAll(windowType))
			{
				if ((int)field.GetValue(window) == 4)
				{
					return (Rect)property.GetValue(window, null);
				}
			}

			return Rect.zero;
		}

		static void PostOutputProcessing(string fileName)
		{
			Debug.unityLogger.logEnabled = logEnabled;

			var srcPath = string.Format("{0}/../../{1}.png", Application.dataPath, fileName);
			int i = 0;
			while (!System.IO.File.Exists(srcPath))
			{
				srcPath = string.Format("{0}/../../{1}{2:000}.png", Application.dataPath, fileName, i);
				if (++i > 999)
				{
					break;
				}
			}
			srcPath = Path.GetFullPath(srcPath);

			var dstPath = Path.Combine(outPutPath, string.Format("{0}.png", fileName));
			Directory.CreateDirectory(Path.GetDirectoryName(dstPath));

			i = 0;
			while (System.IO.File.Exists(dstPath))
			{
				dstPath = Path.Combine(outPutPath, string.Format("{0}{1:000}.png", fileName, i));
				if (++i > 999)
				{
					break;
				}
			}

			if (File.Exists(srcPath))
			{
				File.Move(srcPath, dstPath);
				Debug.Log(string.Format("Saved screenshot at {0}", dstPath));
			}
			else
			{
				Debug.LogError(string.Format("Saved screenshot at {0}", outPutPath));
			}

			if (isOpenFinder)
			{
				EditorUtility.RevealInFinder(dstPath);
			}
		}
	}
}