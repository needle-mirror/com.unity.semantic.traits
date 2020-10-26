using UnityEngine.UIElements;

namespace UnityEditor.Semantic.Traits.UI
{
	static class UIUtility
	{
		const string k_EditorUIAssetPath = "Packages/com.unity.semantic.traits/Editor/Unity.Semantic.Traits.Editor/UI/";
		const string k_StylesAssetPath = k_EditorUIAssetPath + "StyleSheets/";
		const string k_UxmlAssetPath = k_EditorUIAssetPath + "UXML/";

		const string k_DarkThemeSuffix = "#Dark";
		const string k_LightThemeSuffix = "#Light";

		public static VisualTreeAsset LoadTemplate(string filename)
		{
			return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UxmlAssetPath + filename);
		}

		static StyleSheet LoadStyleSheet(string path)
		{
			return EditorGUIUtility.Load(k_StylesAssetPath + path) as StyleSheet;
		}

		static StyleSheet LoadThemeStyleSheet(string path)
		{
			var themedPath = path.Replace(".uss", $"{(EditorGUIUtility.isProSkin?k_DarkThemeSuffix:k_LightThemeSuffix)}.uss");
			return EditorGUIUtility.Load(k_StylesAssetPath + themedPath) as StyleSheet;
		}

		public static void ApplyStyleSheet(VisualElement ve, string styleSheetPath)
		{
			var mainStyle = LoadStyleSheet(styleSheetPath);
			ve.styleSheets.Add(mainStyle);

			var themeStyle = LoadThemeStyleSheet(styleSheetPath);
			if (themeStyle != null)
			{
				ve.styleSheets.Add(themeStyle);
			}
		}
	}
}
