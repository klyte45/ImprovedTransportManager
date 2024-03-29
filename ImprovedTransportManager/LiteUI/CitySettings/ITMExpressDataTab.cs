﻿using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using Kwytto.LiteUI;
using Kwytto.UI;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    public class ITMExpressDataTab : IGUIVerticalITab
    {
        public string TabDisplayName => Str.itm_expressData_title;

        public void DrawArea(Vector2 tabAreaSize)
        {
            GUIKwyttoCommons.AddToggle(Str.itm_expressData_expressBuses, ref ITMCitySettings.Instance.expressBuses);
            GUIKwyttoCommons.AddToggle(Str.itm_expressData_expressTrams, ref ITMCitySettings.Instance.expressTrams);
            GUIKwyttoCommons.AddToggle(Str.itm_expressData_expressTrolleybus, ref ITMCitySettings.Instance.expressTrolleybus);
            GUIKwyttoCommons.Space(6);
            GUIKwyttoCommons.AddToggle(Str.itm_expressData_disableUnbunchingTerminals, ref ITMCitySettings.Instance.disableUnbunchingTerminals);
            GUIKwyttoCommons.Space(6);
            GUILayout.Label(Str.itm_expressData_disableUnbunchingDescription, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.ExpandHeight(true));
        }

        public void Reset()
        {
        }
    }
}
