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
using DaggerfallWorkshop.Game.UserInterface;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Renders a preview of in-game text inside editor.
    /// </summary>
    [CustomPreview(typeof(TextManager))]
    public class LocalizationTextPreview : ObjectPreview
    {
        TextPreviewWindow previewWindow;

        public override bool HasPreviewGUI()
        {
            return DaggerfallUnity.Instance != null && DaggerfallUnity.Instance.IsReady;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (previewWindow == null)
                previewWindow = new TextPreviewWindow();

            previewWindow.UpdateCustom(r);
            previewWindow.Draw();
        }
    }

    /// <summary>
    /// Using a custom UI stack to render DagUI into editor preview area.
    /// Currently fitting text to preview area.
    /// Possible to add other text layout simulations (e.g. 320x200, book, quest log) as required.
    /// </summary>
    public class TextPreviewWindow : UserInterfaceWindow
    {
        Panel previewAreaPanel;
        Panel nativePanel;
        MultiFormatTextLabel textLabel;
        Color32 parchmentColor = new Color32(151, 110, 69, 255);

        public TextPreviewWindow()
        {
            // Preview area panel fills the entire preview area offered by editor
            previewAreaPanel = new Panel();
            previewAreaPanel.HorizontalAlignment = HorizontalAlignment.None;
            previewAreaPanel.VerticalAlignment = VerticalAlignment.None;
            parentPanel.Components.Add(previewAreaPanel);

            // Native panel is a virtual 320x200 display scaled to fit inside total preview area
            nativePanel = new Panel();
            nativePanel.HorizontalAlignment = HorizontalAlignment.Center;
            nativePanel.VerticalAlignment = VerticalAlignment.Middle;
            nativePanel.AutoSize = AutoSizeModes.ScaleToFit;
            nativePanel.BackgroundColor = parchmentColor;
            previewAreaPanel.Components.Add(nativePanel);

            // Text label to display virtual in-game text
            textLabel = new MultiFormatTextLabel();
            textLabel.HorizontalAlignment = HorizontalAlignment.Center;
            textLabel.VerticalAlignment = VerticalAlignment.Middle;
            textLabel.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            textLabel.SetText(DaggerfallUnity.Instance.TextProvider.GetRSCTokens(0));
            nativePanel.Components.Add(textLabel);
        }

        public void UpdateCustom(Rect rect)
        {
            base.Update();

            previewAreaPanel.Position = new Vector2(rect.x, rect.y);
            previewAreaPanel.Size = new Vector2(rect.width, rect.height);

            nativePanel.Size = textLabel.Size;
        }
    }
}