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

using System;
using System.Collections.Generic;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game;

namespace DaggerfallWorkshop.Game.Localization
{
    /// <summary>
    /// Manages a localization text database.
    /// Supports live merging of other TextDatabase stores and provides importer helpers for classic game text.
    /// Used primarily by TextManager singleton in game scene to lookup game text at runtime.
    /// TextManager Inspector provides a default TextDatabase editor.
    /// </summary>
    public class TextDatabase
    {
        // Text source prefixes
        const string textRSCPrefix = "text.{0}";

        // Markup for token conversion
        // Markup format is designed with the following requirements:
        //  1. Don't overcomplicate - must be simple to understand and edit using plain text
        //  2. Support all classic Daggerfall text data (excluding books which already have custom format)
        //  3. Convert easily between RSC tokens and markup as required
        //  4. Formatting must not conflict with regular text entry in any language
        const string markupJustifyLeft = "[/left]";
        const string markupJustifyCenter = "[/center]";
        const string markupNewLine = "\\n";
        const string markupTextPosition = "[/pos:x={0},y={1}]";
        const string markupInputCursor = "[/input]";

        Dictionary<string, TextGroup> textDict = new Dictionary<string, TextGroup>();

        #region Public Methods

        /// <summary>
        /// Get all results from database.
        /// Same as SearchDatabase(string.Empty).
        /// </summary>
        /// <returns></returns>
        public TextGroup[] SearchDatabase()
        {
            return SearchDatabase(string.Empty);
        }

        /// <summary>
        /// Search database for a specific substring pattern.
        /// </summary>
        /// <param name="searchString">Substring to search for. Invariant culture, ignores case. Use string.Empty for all results.</param>
        /// <returns>Zero or more text groups with any element matching search pattern.</returns>
        public TextGroup[] SearchDatabase(string searchString)
        {
            List<TextGroup> results = new List<TextGroup>();
            foreach(var kvp in textDict)
            {
                if (kvp.Value.Elements != null && kvp.Value.Elements.Count > 0)
                {
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        for (int i = 0; i < kvp.Value.Elements.Count; i++)
                        {
                            int index = kvp.Value.Elements[i].Text.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase);
                            if (index != -1)
                            {
                                results.Add(kvp.Value);
                                break;
                            }
                        }
                    }
                    else
                    {
                        results.Add(kvp.Value);
                    }
                }
            }

            return results.ToArray();
        }

        #endregion

        #region Importer Helpers

        /// <summary>
        /// Create TEXT.RSC key in format "text.nnnn" where "nnnn" is numeric ID from TEXT.RSC entry.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Key for TEXT.RSC ID.</returns>
        public static string MakeTextRSCKey(int id)
        {
            return string.Format(textRSCPrefix, id.ToString());
        }

        /// <summary>
        /// Helper to import TEXT.RSC from classic game data into specified text database.
        /// Duplicates will be overwritten.
        /// </summary>
        /// <param name="db">TextDatabase to receive TEXT.RSC data.</param>
        public static void ImportTextRSC(TextDatabase db)
        {
            // Load TEXT.RSC file
            TextFile rsc = new TextFile(DaggerfallUnity.Instance.Arena2Path, TextFile.Filename);
            if (rsc == null || rsc.IsLoaded == false)
                throw new Exception("Could not load TEXT.RSC");

            // Iterate records
            int overwriteCount = 0;
            for (int i = 0; i < rsc.RecordCount; i++)
            {
                // Extract this record to tokens
                byte[] buffer = rsc.GetBytesByIndex(i);
                TextFile.Token[] tokens = TextFile.ReadTokens(ref buffer, 0, TextFile.Formatting.EndOfRecord);

                // Get key and remove duplicate if one exists
                string key = MakeTextRSCKey(rsc.IndexToId(i));
                if (db.textDict.ContainsKey(key))
                {
                    db.textDict.Remove(key);
                    overwriteCount++;
                }

                // Convert tokens to localization TextGroup
                TextGroup group = new TextGroup()
                {
                    LegacySource = LegacySources.TextRSC,
                    PrimaryKey = key,
                    Elements = ConvertRSCTokensToTextElements(tokens),
                };

                // Add to database
                db.textDict.Add(key, group);
            }

            UnityEngine.Debug.LogFormat("Added {0} TEXT.RSC entries to database with {1} overwrites.", rsc.RecordCount, overwriteCount);
        }

        /// <summary>
        /// UNDER ACTIVE DEVELOPMENT
        /// Converts RSC tokens into TextElement markup.
        /// All tokens per subrecord will be converted into a single string with markup.
        /// This TextElement list can be converted back into the original RSC token stream.
        /// </summary>
        /// <param name="tokens">RSC token input.</param>
        /// <returns>List of TextElement markup converted from RSC tokens.</returns>
        public static List<TextElement> ConvertRSCTokensToTextElements(TextFile.Token[] tokens)
        {
            List<TextElement> elements = new List<TextElement>();

            // Convert RSC formatting tokens into markup text for easier human editing
            // Expect significant evolution of this before editor is completed
            TextElement newElement;
            string text = string.Empty;
            for (int i = 0; i < tokens.Length; i++)
            {
                switch (tokens[i].formatting)
                {
                    case TextFile.Formatting.Text:
                        text += tokens[i].text;
                        break;
                    case TextFile.Formatting.JustifyLeft:
                        text += markupJustifyLeft;
                        break;
                    case TextFile.Formatting.JustifyCenter:
                        text += markupJustifyCenter;
                        break;
                    case TextFile.Formatting.NewLine:
                        text += markupNewLine;
                        break;
                    case TextFile.Formatting.PositionPrefix:
                        text += string.Format(markupTextPosition, tokens[i].x, tokens[i].y);
                        break;
                    case TextFile.Formatting.SubrecordSeparator:
                        // Add this record and start new text
                        newElement = new TextElement() { Text = text };
                        elements.Add(newElement);
                        text = string.Empty;
                        break;
                    case TextFile.Formatting.InputCursorPositioner:
                        text += markupInputCursor;
                        break;
                    default:
                        throw new Exception(string.Format("Unexpected RSC formatting token encountered {0}", tokens[i].formatting.ToString()));
                }
            }

            // Add pending text
            newElement = new TextElement() { Text = text };
            elements.Add(newElement);

            return elements;
        }

        /// <summary>
        /// UNDER ACTIVE DEVELOPMENT
        /// Converts TextElement list back into RSC tokens.
        /// </summary>
        /// <param name="textElements">TextElements input.</param>
        /// <returns>Array of RSC tokens converted from TextElements.</returns>
        public static TextFile.Token[] ConvertTextElementsToRSCTokens(List<TextElement> textElements)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}