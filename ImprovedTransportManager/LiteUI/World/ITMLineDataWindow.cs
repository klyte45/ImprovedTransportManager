using ColossalFramework.UI;
using ImprovedTransportManager.Localization;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;
using VehicleSkins.Localization;

namespace ImprovedTransportManager.UI
{
    internal enum LineActivityOptions
    {
        None,
        Day,
        Night,
        DayNight
    }

    public class ITMLineDataWindow : GUIOpacityChanging
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override bool ShowCloseButton => false;
        protected override bool ShowMinimizeButton => true;
        protected override float FontSizeMultiplier => .9f;
        private const float minHeight = 325;
        private GUIColorPicker picker;
        private Texture2D m_clearButton;
        private Texture2D m_helpButton;
        private readonly LineActivityOptions[] m_lineActivityOptions = Enum.GetValues(typeof(LineActivityOptions)).Cast<LineActivityOptions>().ToArray();
        private string[] m_lineActivityOptionsNames;

        public static ITMLineDataWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObjectUtils.CreateElement<ITMLineDataWindow>(UIView.GetAView().transform);
                    instance.Init(ModInstance.Instance.GeneralName, new Rect(256, 256, 400, minHeight), resizable: false, minSize: new Vector2(100, minHeight), hasTitlebar: true);
                    instance.Visible = false;
                }
                return instance;
            }
        }

        private static ITMLineDataWindow instance;
        private Tuple<UIComponent, PublicTransportWorldInfoPanel>[] currentBWIP;

        public override void Awake()
        {
            base.Awake();
            m_clearButton = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Delete);
            m_helpButton = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_QuestionMark);
            picker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            picker.Visible = false;
        }
        GUIStyle m_noBreakLabel;

        private GUIStyle m_redButton;
        public GUIStyle RedButton
        {
            get
            {
                if (m_redButton is null)
                {
                    m_redButton = new GUIStyle(Skin.button)
                    {
                        normal = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.darkRedTexture,
                            textColor = Color.white
                        },
                        hover = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.redTexture,
                            textColor = Color.white
                        },
                    };
                }
                return m_redButton;
            }
        }
        protected override void DrawWindow(Vector2 size)
        {
            if (m_noBreakLabel is null)
            {
                m_noBreakLabel = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = false,
                    alignment = TextAnchor.MiddleLeft,
                };
            }

            if (m_inlineBtnStyle is null)
            {
                m_inlineBtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fixedHeight = 20 * ResolutionMultiplier,
                    fixedWidth = 20 * ResolutionMultiplier,
                    padding = new RectOffset(0, 0, 0, 0),
                };
            }
            if (m_currentLine != 0)
            {
                ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[m_currentLine];
                m_currentLineData.GetUpdated();
                GUIKwyttoCommons.AddColorPicker(Str.itm_lineView_lineColor, picker, m_currentLineData.LineColor, (x) => m_currentLineData.LineColor = x ?? default);
                GUIKwyttoCommons.AddIntField(size.x, Str.itm_lineView_lineInternalNumber, m_currentLineData.LineInternalSequentialNumber(), (x) => TransportManager.instance.m_lines.m_buffer[m_currentLine].m_lineNumber = (ushort)(x ?? 0), min: 0, max: 65535);
                GUIKwyttoCommons.AddComboBox(size.x, Str.itm_lineView_lineActivity, m_currentLineData.LineActivity, m_lineActivityOptionsNames, m_lineActivityOptions, this, (x) => m_currentLineData.LineActivity = x);
                GUILayout.Space(4);
                GUIKwyttoCommons.AddSliderInt(size.x, Str.itm_lineView_lineBudget, m_currentLineData.BudgetSelf, (x) => m_currentLineData.BudgetSelf = x, 0, 500);
                GUILayout.Label($"\t- {Str.itm_lineView_dayBudgetTitle} " +
                    ((m_currentLineData.LineActivity & LineActivityOptions.Day) == 0
                        ? $"<color=red>{LineActivityOptions.None.ValueToI18n()}</color>"
                        : $"{m_currentLineData.BudgetSelf}% x {m_currentLineData.BudgetCategoryDay}% =<color=yellow> {m_currentLineData.BudgetEffectiveDay}%</color> ({m_currentLineData.VehiclesTargetDay})"));
                GUILayout.Label($"\t- {Str.itm_lineView_nightBudgetTitle} " +
                    ((m_currentLineData.LineActivity & LineActivityOptions.Night) == 0
                    ? $"<color=red>{LineActivityOptions.None.ValueToI18n()}</color>"
                    : $"{m_currentLineData.BudgetSelf}% x {m_currentLineData.BudgetCategoryNight}% =<color=yellow> {m_currentLineData.BudgetEffectiveNight}%</color> ({m_currentLineData.VehiclesTargetNight})"
                    ));
                GUILayout.Space(4);


                GUILayout.FlexibleSpace();
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(Str.itm_lineView_deleteLine, RedButton))
                    {
                        m_currentLineData.Delete();
                    }
                }
            }
        }


        private GUIStyle m_inlineBtnStyle;
        private ushort m_currentLine;
        private LineData m_currentLineData;


        private void FixedUpdate()
        {
            if (currentBWIP is null)
            {
                var BWIPs = UIView.GetAView().GetComponentsInChildren<PublicTransportWorldInfoPanel>();
                if (BWIPs is null || BWIPs.Length == 0)
                {
                    return;
                }
                currentBWIP = BWIPs.Select(x => Tuple.New(x.GetComponent<UIComponent>(), x)).ToArray();
            }
            if (currentBWIP.FirstOrDefault(x => x.First.isVisible) is Tuple<UIComponent, PublicTransportWorldInfoPanel> window)
            {
                if (m_currentLine != WorldInfoPanel.GetCurrentInstanceID().TransportLine)
                {
                    m_currentLine = WorldInfoPanel.GetCurrentInstanceID().TransportLine;
                    if (m_currentLine > 0)
                    {
                        m_lineActivityOptionsNames = m_lineActivityOptions.Select(x => x.ValueToI18n()).ToArray();
                        m_currentLineData = LineData.FromLine(m_currentLine);
                        Visible = true;
                        Title = TransportManager.instance.GetLineName(m_currentLine);
                    }
                }
            }
            else
            {
                Visible = false;
                m_currentLine = 0;
            }
        }
        protected override void OnWindowDestroyed()
        {
            instance = null;
        }

    }

}
