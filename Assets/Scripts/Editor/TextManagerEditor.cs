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

        private const string showDatabaseEditorFoldout = "TextManagerEditor_ShowDatabaseEditorFoldout";
        private static bool ShowDatabaseEditorFoldout
        {
            get { return EditorPrefs.GetBool(showDatabaseEditorFoldout, true); }
            set { EditorPrefs.SetBool(showDatabaseEditorFoldout, value); }
        }

        void DisplayGUI()
        {
            EditorGUILayout.Space();
            ShowDatabaseEditorFoldout = GUILayoutHelper.Foldout(ShowDatabaseEditorFoldout, new GUIContent("Database Editor"), () =>
            {
                // TEMP: Editor buttons to import text from classic game data
                // Currently just used for testing - might only be used to generate initial text database
                if (GUILayout.Button("Import TEXT.RSC"))
                {
                    TextDatabase.ImportTextRSC(TextManager.Instance.testTextDB);
                }
            });
        }
    }
}