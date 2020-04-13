// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2020 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using UnityEditor;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Localization;

namespace DaggerfallWorkshop
{
    [CustomEditor(typeof(TextManager))]
    public class TextManagerEditor : Editor
    {
        Vector2 scrollPos;
        string searchString = string.Empty;
        string lastSearchString = string.Empty;
        bool updateSearch = true;
        TextGroup[] searchResults = null;

        SerializedProperty Prop(string name)
        {
            return serializedObject.FindProperty(name);
        }

        public override void OnInspectorGUI()
        {
            // Update
            serializedObject.Update();

            DisplayGUI();

            // Save modified properties
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }

        private const string showDatabaseImporterFoldout = "TextManagerEditor_ShowDatabaseImporterFoldout";
        private static bool ShowDatabaseImporterFoldout
        {
            get { return EditorPrefs.GetBool(showDatabaseImporterFoldout, true); }
            set { EditorPrefs.SetBool(showDatabaseImporterFoldout, value); }
        }

        private const string showDatabaseEditorFoldout = "TextManagerEditor_ShowDatabaseEditorFoldout";
        private static bool ShowDatabaseEditorFoldout
        {
            get { return EditorPrefs.GetBool(showDatabaseEditorFoldout, true); }
            set { EditorPrefs.SetBool(showDatabaseEditorFoldout, value); }
        }

        void DisplayGUI()
        {
            // Database importers used during development
            // Will be disabled once primary text database is fully populated by text data imported from classic
            // Localised versions of text data do not need to import from scratch, rather they will start from primary text database
            EditorGUILayout.Space();
            ShowDatabaseImporterFoldout = GUILayoutHelper.Foldout(ShowDatabaseImporterFoldout, new GUIContent("Database Importers (TEMP)"), () =>
            {
                GUILayoutHelper.Horizontal(() =>
                {
                    if (GUILayout.Button("TEXT.RSC"))
                    {
                        TextDatabase.ImportTextRSC(TextManager.Instance.testTextDB);
                        updateSearch = true;
                    }
                });
            });

            // Searchable text database view
            EditorGUILayout.Space();
            ShowDatabaseEditorFoldout = GUILayoutHelper.Foldout(ShowDatabaseEditorFoldout, new GUIContent("Text Database"), () =>
            {
                // Search bar
                GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
                searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    // Remove focus if cleared
                    searchString = "";
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();

                // Update search list
                if (searchResults == null || searchString != lastSearchString || updateSearch)
                {
                    searchResults = TextManager.Instance.testTextDB.SearchDatabase(searchString);
                    lastSearchString = searchString;
                    updateSearch = false;
                    scrollPos = Vector2.zero;
                }

                // Scrolling text list preview
                scrollPos = GUILayoutHelper.ScrollView(scrollPos, () =>
                {
                    for (int i = 0; i < searchResults.Length; i++)
                    {
                        ShowTextGroupPreviewItem(searchResults[i].Elements[0].Text);
                    }
                });
            });
        }

        void ShowTextGroupPreviewItem(string text)
        {
            const int lengthLimit = 90;

            // Cull text if over length limit
            if (text.Length > lengthLimit)
                text = text.Substring(0, lengthLimit);

            EditorGUILayout.SelectableLabel(text, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }
    }
}