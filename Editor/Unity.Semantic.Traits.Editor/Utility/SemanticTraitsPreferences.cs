using System;
using System.IO;
using UnityEditorInternal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Semantic.Traits
{
    class SemanticTraitsPreferences : ScriptableObject
    {
        public bool BuildOnEnteredPlayMode => m_BuildOnEnteredPlayMode;

        public bool BuildOnAssetChanged => m_BuildOnAssetChanged;

        public bool AutoSaveAssets
        {
            get => m_AutoSaveAssets;
            set
            {
                m_AutoSaveAssets = value;
                SaveSettings();
            }
        }

        const string k_PreferencesPath = "Library/SemanticTraitPreferences.asset";

        static SemanticTraitsPreferences s_Preferences;

        [SerializeField]
        bool m_BuildOnEnteredPlayMode = true;

        [SerializeField]
        bool m_BuildOnAssetChanged = false;

        [SerializeField]
        bool m_AutoSaveAssets = true;

        internal static SemanticTraitsPreferences GetOrCreatePreferences()
        {
            if (!s_Preferences)
            {
                if (File.Exists(k_PreferencesPath))
                    s_Preferences = (SemanticTraitsPreferences)InternalEditorUtility.LoadSerializedFileAndForget(k_PreferencesPath)[0];

                if (!s_Preferences)
                    s_Preferences = CreateInstance<SemanticTraitsPreferences>();
            }

            return s_Preferences;
        }

        static void SaveSettings()
        {
            if (s_Preferences)
                InternalEditorUtility.SaveToSerializedFileAndForget( new UnityObject[] { s_Preferences }, k_PreferencesPath, true);
        }

        static SerializedObject GetSerializedPreferences()
        {
            return new SerializedObject(GetOrCreatePreferences());
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Semantic Traits", SettingsScope.User)
            {
                deactivateHandler = SaveSettings,
                guiHandler = searchContext =>
                {
                    var settings = GetSerializedPreferences();

                    EditorGUILayout.LabelField("Auto-build");

                    // Auto-compile
                    using (new EditorGUI.IndentLevelScope())
                    {
                        var labelPlayMode = new GUIContent("On entered Play Mode",
                            "If any trait assets have changed, then the generated assemblies will be re-generated before entering into play mode.");

                        EditorGUILayout.PropertyField(settings.FindProperty(nameof(m_BuildOnEnteredPlayMode)), labelPlayMode);

                        var labelAssetChanged = new GUIContent("On asset changed",
                            "If any trait assets is changed, assemblies is re-generated.");

                        EditorGUILayout.PropertyField(settings.FindProperty(nameof(m_BuildOnAssetChanged)), labelAssetChanged);
                    }

                    // Auto-save assets
                    {
                        var label = new GUIContent("Auto-save assets",
                            "Any planner assets you change will be saved immediately.");

                        EditorGUILayout.PropertyField(settings.FindProperty(nameof(m_AutoSaveAssets)), label);
                    }

                    settings.ApplyModifiedProperties();
                }
            };

            return provider;
        }
    }
}
