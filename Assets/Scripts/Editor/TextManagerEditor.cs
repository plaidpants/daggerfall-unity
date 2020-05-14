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
using System.Collections;
using System.Collections.Generic;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Localization;
using UnityEditorInternal;

namespace DaggerfallWorkshop
{
    [CustomEditor(typeof(TextManager))]
    public class TextManagerEditor : Editor
    {
        const int previewLengthLimit = 90;
        const int fixedLineHeight = 18;
        const int scrollingListHeight = 350;

        Vector2 scrollPos;
        string searchString = string.Empty;
        string lastSearchString = string.Empty;
        bool updateSearch = true;
        TextGroup[] searchResults = null;
        GUIStyle buttonStyle;
        int selectedIndex = -1;

        Color selectedColor = Color.red;

        List<string> records = new List<string>();
        ReorderableList recordsListView;

        SerializedProperty Prop(string name)
        {
            return serializedObject.FindProperty(name);
        }

        private void OnEnable()
        {
            recordsListView = new ReorderableList(records, typeof(string), true, true, true, true);
            recordsListView.drawElementCallback = DrawListItems;
            recordsListView.drawHeaderCallback = DrawHeader;
        }

        void DrawHeader(Rect rect)
        {
            string name = "Stuff";
            EditorGUI.LabelField(rect, name);
        }

        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
        }

        public override void OnInspectorGUI()
        {
            // Update
            serializedObject.Update();

            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.alignment = TextAnchor.MiddleLeft;
                buttonStyle.fixedHeight = fixedLineHeight;
                buttonStyle.active.background = buttonStyle.normal.background;
                buttonStyle.margin = new RectOffset(0, 0, 0, 0);
            }

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
            ShowDatabaseEditorFoldout = GUILayoutHelper.Foldout(ShowDatabaseEditorFoldout, new GUIContent("Record Editor"), () =>
            {
                // Search bar
                EditorGUILayout.Space();
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
                    selectedIndex = -1;
                }

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true, GUILayout.Height(scrollingListHeight));
                for (int i = 0; i < searchResults.Length; i++)
                {
                    ShowTextGroupPreviewItem(searchResults[i].PrimaryKey, searchResults[i].Elements[0].Text, i, buttonStyle);
                }
                EditorGUILayout.EndScrollView();

                // Subrecord list
                EditorGUILayout.Space();
                recordsListView.DoLayoutList();
            });
        }

        void ShowTextGroupPreviewItem(string key, string text, int index, GUIStyle style)
        {
            float startPos = -scrollPos.y;
            float itemTopPos = startPos + index * fixedLineHeight;
            float itemBottomPos = itemTopPos + fixedLineHeight;

            Color oldBackgroundColor = GUI.backgroundColor;
            if (itemBottomPos < 0 || itemTopPos > scrollingListHeight)
            {
                // Draw empty line outside of visible scroller area
                GUILayout.Space(fixedLineHeight);
            }
            else
            {
                GUI.backgroundColor = (selectedIndex == index) ? selectedColor : Color.clear;

                string previewText = string.Format("{0} - {1}", key, text);
                if (previewText.Length > previewLengthLimit)
                    previewText = previewText.Substring(0, previewLengthLimit);

                style.fontStyle = (selectedIndex == index) ? FontStyle.Bold : style.fontStyle = FontStyle.Normal;

                if (GUILayout.Button(previewText, style))
                {
                    selectedIndex = index;
                    //Debug.LogFormat("Selected key={0}", searchResults[index].PrimaryKey);
                }
            }
            GUI.backgroundColor = oldBackgroundColor;
        }
    }
}