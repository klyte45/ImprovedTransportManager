﻿extern alias UUI;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Xml;
using Kwytto.LiteUI;
using Kwytto.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleSkins.Localization;
using static Kwytto.LiteUI.KwyttoDialog;

namespace ImprovedTransportManager.UI
{
    public class ITMLineCustomBudgetWindow : GUIOpacityChanging
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override bool ShowCloseButton => false;
        protected override bool ShowMinimizeButton => true;
        protected override float FontSizeMultiplier => .9f;
        protected bool Resizable => true;
        protected string InitTitle => Str.itm_budgetCustomWindow_title;
        protected Vector2 StartSize => new Vector2(560, 300) * ResolutionMultiplier;
        protected Vector2 MinSize { get; } = new Vector2(560, 300);
        protected Vector2 StartPosition => new Vector2(UIScaler.MaxWidth - 960 * ResolutionMultiplier, UIScaler.MaxHeight - 300 * ResolutionMultiplier);

        public static ITMLineCustomBudgetWindow Instance { get; private set; }

        private string[] m_availableGroups = new[] { Str.itm_budgetCustomWindow_defaultGroupText }.Concat(new int[32].Select((_, i) => string.Format(Str.itm_budgetCustomWindow_groupNameTemplate, i + 1))).ToArray();
        private static readonly string[] m_availableCustomTypesOptions = EnumI18nExtensions.GetAllValuesI18n<BudgetEntryXml.BudgetType>();
        private static readonly BudgetEntryXml.BudgetType[] m_availableCustomType = Enum.GetValues(typeof(BudgetEntryXml.BudgetType)).Cast<BudgetEntryXml.BudgetType>().ToArray();
        private ITMTransportLineXml m_currentLineSettings;
        private ushort[] m_linesUsingSameGroup;
        private string[] m_linesUsingSameGroupNames;
        private ushort m_currentLine;
        private Vector2 m_scrollGroups, m_scrollTimetable;

        private byte[] m_clipboard;

        private GUIStyle m_selectionBtnUns;
        private GUIStyle m_previewTitle;
        private GUIStyle m_nobrLabel;
        private GUIStyle m_rightLabel, m_centerLabel;
        private BudgetEntryXml m_currentGroupData;
        private readonly List<string> m_currentEditingGroupTimeTableLines = new List<string>();

        protected override void DrawWindow(Vector2 size)
        {
            if (!ITMCitySettings.Instance.expertMode) return;

            InitStyles();
            using (new GUILayout.HorizontalScope())
            {
                var selectionGroup = GUIComboBox.Box(m_currentLineSettings.BudgetGroup, m_availableGroups, "BudgetGroupPicker_line", this, size.x);
                if (selectionGroup != m_currentLineSettings.BudgetGroup)
                {
                    OnChangeBudgetGroup(selectionGroup);
                }
            }
            if (m_linesUsingSameGroup.Length > 0)
            {
                if (GUILayout.Button(string.Format(Str.itm_budgetCustomWindow_linesUsingSameBudgetGroupFormat, m_linesUsingSameGroup.Length)))
                {

                }
            }
            else
            {
                GUILayout.Label(string.Format(Str.itm_budgetCustomWindow_linesUsingSameBudgetGroupFormat, 0), m_nobrLabel);
            }
            if (m_currentLineSettings.BudgetGroup == 0)
            {
                GUILayout.Label(Str.itm_budgetCustomWindow_defaultGroupDescription);
                return;
            }
            GUIKwyttoCommons.AddComboBox(size.x, Str.itm_budgetCustomWindow_budgetType, m_currentGroupData.Type, m_availableCustomTypesOptions, m_availableCustomType, this, (x) => m_currentGroupData.Type = x);
            switch (m_currentGroupData.Type)
            {
                case BudgetEntryXml.BudgetType.Fixed:
                    GUIKwyttoCommons.AddSliderInt(size.x, Str.itm_lineView_lineBudget, m_currentGroupData.BaseBudget, (x) => m_currentGroupData.BaseBudget = (ushort)x, 0, 500);
                    break;
                case BudgetEntryXml.BudgetType.PerHour:
                    GUILayout.Label(Str.itm_budgetCustomWindow_groupsEachHour);
                    GUIKwyttoCommons.Space(60);
                    var refPos = GUILayoutUtility.GetLastRect().position;
                    var btnWidth = size.x / 24;
                    for (int i = 0; i < 24; i++)
                    {
                        var offsetX = btnWidth * i;
                        GUI.Label(new Rect(refPos + new Vector2(offsetX, 0), new Vector2(btnWidth, 20 * ResolutionMultiplier)), $"{i}", m_centerLabel);
                        if (GUI.Button(new Rect(refPos + new Vector2(offsetX, 25), new Vector2(btnWidth, 35 * ResolutionMultiplier)), $"{(char)('A' + m_currentGroupData.DefaultValue[i])}\n{m_currentGroupData.BudgetGroups[m_currentGroupData.DefaultValue[i]]}", m_selectionBtnUns))
                        {
                            m_currentGroupData.IncrementGroupAt(-1, i);
                            m_currentEditingGroupTimeTableLines.Clear();
                        }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope(GUILayout.Width(size.x * .3f)))
                        {

                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label(Str.itm_budgetCustomWindow_groupsBudgets);
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button("+"))
                                {
                                    m_currentGroupData.AddGroup(100);
                                }
                            }
                            using (var scroll = new GUILayout.ScrollViewScope(m_scrollGroups))
                            {
                                for (int idx = 0; idx < m_currentGroupData.BudgetGroups.Length; idx++)
                                {
                                    ushort groupVal = m_currentGroupData.BudgetGroups[idx];
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUIKwyttoCommons.AddIntField(size.x * .3f - 40, $"{(char)('A' + idx)}", groupVal, (x) => SetBudgetGroupForHourDefault(idx, x), min: 0, max: 500);
                                        if (m_currentGroupData.BudgetGroups.Length > 1 && GUILayout.Button("X"))
                                        {
                                            m_currentGroupData.RemoveGroup(idx);
                                        }
                                    }
                                }
                                m_scrollGroups = scroll.scrollPosition;
                            }
                        }
                        using (new GUILayout.VerticalScope(GUILayout.Width(size.x * .7f)))
                        {
                            GUILayout.Label(Str.itm_budgetCustomWindow_budgetTimetableResume);
                            using (var scroll = new GUILayout.ScrollViewScope(m_scrollTimetable))
                            {
                                if (m_currentEditingGroupTimeTableLines.Count == 0)
                                {
                                    UpdateTimeTableLines();
                                }
                                foreach (var str in m_currentEditingGroupTimeTableLines)
                                {
                                    GUILayout.Label(str);
                                }
                                m_scrollTimetable = scroll.scrollPosition;
                            }
                        }
                    }
                    break;
                case BudgetEntryXml.BudgetType.PerHourAndWeek:
                    break;
            }
        }

        private void SetBudgetGroupForHourDefault(int idx, int? x)
        {
            m_currentGroupData.BudgetGroups[idx] = (ushort)x.Value;
            m_currentEditingGroupTimeTableLines.Clear();
        }

        private void UpdateTimeTableLines()
        {
            m_currentEditingGroupTimeTableLines.Clear();
            int lastGroup = m_currentGroupData.DefaultValue[23];
            int startLastHourGroup = -1;
            for (int idx = 0; idx < m_currentGroupData.DefaultValue.Length; idx++)
            {
                var currentGroup = m_currentGroupData.DefaultValue[idx];
                if (lastGroup != currentGroup)
                {
                    var budgetLastGroup = m_currentGroupData.BudgetGroups[lastGroup];
                    var budgetCurrentGroup = m_currentGroupData.BudgetGroups[currentGroup];
                    if (budgetLastGroup == budgetCurrentGroup)
                    {
                        lastGroup = currentGroup;
                        continue;
                    }
                    if (idx - startLastHourGroup > 1)
                    {
                        m_currentEditingGroupTimeTableLines.Add($"{(startLastHourGroup < 0 ? "0h00" : $"{startLastHourGroup}h30")}-{idx - 1}h29: {budgetLastGroup}%");
                    }
                    m_currentEditingGroupTimeTableLines.Add($"{(idx + 23) % 24}h30{(idx == 0 ? "(-1D)" : "")}-{idx}h29: {string.Format(Str.itm_budgetCustomWindow_transitionBudgetLabel, budgetLastGroup, budgetCurrentGroup)}");

                    lastGroup = currentGroup;
                    startLastHourGroup = idx;
                }
            }

            var budgetLastGroup2 = m_currentGroupData.BudgetGroups[lastGroup];
            var budgetFirstGroup = m_currentGroupData.BudgetGroups[m_currentGroupData.DefaultValue[0]];
            var budgetGroup23 = m_currentGroupData.BudgetGroups[m_currentGroupData.DefaultValue[23]];
            var lastDiffFirst = budgetFirstGroup != budgetGroup23;
            if (budgetLastGroup2 == budgetFirstGroup)
            {
                m_currentEditingGroupTimeTableLines.Add($"{(startLastHourGroup < 0 ? "0h00" : $"{startLastHourGroup}h30")}-23h{(lastDiffFirst ? "30" : "59")}: {budgetLastGroup2}%");
            }
            else if (startLastHourGroup < 23)
            {
                m_currentEditingGroupTimeTableLines.Add($"{(startLastHourGroup < 0 ? "0h00" : $"{startLastHourGroup}h30")}-23h29: {budgetLastGroup2}%");

            }
            if (lastDiffFirst)
            {
                m_currentEditingGroupTimeTableLines.Add($"23h30-0h29 (+1D): {string.Format(Str.itm_budgetCustomWindow_transitionBudgetLabel, budgetGroup23, budgetFirstGroup)}");
            }
        }

        private void OnChangeBudgetGroup(int selectionGroup)
        {
            m_currentLineSettings.BudgetGroup = (byte)selectionGroup;
            UpdateGroupDataCache();
        }

        private void UpdateGroupDataCache()
        {
            m_currentGroupData = ITMTransportLineSettings.Instance.GetBudgetGroup(m_currentLineSettings.CachedTransportType, m_currentLineSettings.BudgetGroup);
            m_linesUsingSameGroup = ITMTransportLineSettings.Instance.Lines.Where(x => x.Value.BudgetGroup == m_currentLineSettings.BudgetGroup && x.Value.CachedTransportType == m_currentLineSettings.CachedTransportType && x.Key != m_currentLine).Select(x => (ushort)x.Key).ToArray();
            m_currentEditingGroupTimeTableLines.Clear();
        }

        private void ShowHelp() => KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
        {
            buttons = new[]
        {
            SpaceBtn,
            new ButtonDefinition
            {
                title = KStr.comm_releaseNotes_Ok,
                onClick = ()=>true
            }
        },
            scrollText = Str.itm_budgetCustomWindow_helpContent
        });

        private void InitStyles()
        {

            if (m_selectionBtnUns is null)
            {
                m_selectionBtnUns = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        background = GUIKwyttoCommons.blackTexture
                    },
                    hover =
                    {
                        background = GUIKwyttoCommons.whiteTexture
                    },
                    fixedHeight = 35 * GUIWindow.ResolutionMultiplier,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = false
                };
            }
            if (m_previewTitle is null)
            {
                m_previewTitle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = Mathf.RoundToInt(DefaultSize * 1.2f),
                    normal =
                    {
                        textColor = Color.black
                    }
                };
            }
            if (m_nobrLabel is null)
            {
                m_nobrLabel = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = false,
                };
            }
            if (m_rightLabel is null)
            {
                m_rightLabel = new GUIStyle(m_nobrLabel)
                {
                    alignment = TextAnchor.MiddleRight,
                };
            }
            if (m_centerLabel is null)
            {
                m_centerLabel = new GUIStyle(m_nobrLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                };
            }
        }

        internal void OnIdChanged(InstanceID currentId)
        {
            if (currentId.TransportLine > 0)
            {
                m_currentLine = currentId.TransportLine;
                var settings = ITMTransportLineSettings.Instance;
                m_currentLineSettings = settings.SafeGetLine(m_currentLine);
                if (!m_currentLineSettings.CachedTransportType.HasVehicles())
                {
                    Visible = false;
                    return;
                }
                UpdateGroupDataCache();
            }
            else
            {
            }
        }

        public override void Awake()
        {
            base.Awake();
            Init();
            Instance = this;
            Visible = false;
        }
        private void Init() => Init(InitTitle, new Rect(StartPosition, StartSize), Resizable, true, minSize: MinSize);


    }
}