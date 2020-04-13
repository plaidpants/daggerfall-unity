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
using UnityEngine;

namespace DaggerfallWorkshop.Game.Localization
{
    //  Note: Will expand and change over time until text database is complete.

    /// <summary>
    /// Each TextGroup stores one or more TextElements in localization database.
    /// </summary>
    [Serializable]
    public struct TextGroup
    {
        public LegacySources LegacySource;      // Text source of text data if from classic game data (e.g. TEXT.RSC, FACTION.TXT)
        public string PrimaryKey;               // First key to identify this text element
        public List<TextElement> Elements;      // One or more text elements in this group - typically just a single primary element
    }

    /// <summary>
    /// Defines a single text element item in localization database.
    /// </summary>
    [Serializable]
    public struct TextElement
    {
        public string Text;                     // Text with markup for this entry
    }
}