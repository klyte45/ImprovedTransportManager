using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;
using VehicleSkins.Localization;

namespace ImprovedTransportManager.UI
{
    public class ITMLineDataWindow : GUIOpacityChanging
    {
        public static ITMLineDataWindow Instance { get; private set; }
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override bool ShowCloseButton => true;
        protected override bool ShowMinimizeButton => false;
        protected override float FontSizeMultiplier => .9f;
        protected bool Resizable => false;
        protected string InitTitle => ModInstance.Instance.GeneralName;
        protected Vector2 StartSize => new Vector2(400, 600);
        protected Vector2 StartPosition => new Vector2((UIScaler.MaxWidth / 2) - 200, 256);
        protected virtual Vector2 MinSize { get; } = default;
        protected virtual Vector2 MaxSize { get; } = default;
        public ushort CurrentLine { get; private set; }

        private GUIColorPicker picker;
        private Texture2D m_childTex;
        private Texture2D m_teenTex;
        private Texture2D m_youngTex;
        private Texture2D m_adultTex;
        private Texture2D m_seniorTex;
        private Texture2D m_autonameTex;
        private readonly LineActivityOptions[] m_lineActivityOptions = Enum.GetValues(typeof(LineActivityOptions)).Cast<LineActivityOptions>().ToArray();
        private string[] m_lineActivityOptionsNames;
        private const string COLOR_CHILDREN = "CC8844";
        private const string COLOR_TEEN = "CCCC44";
        private const string COLOR_YOUNG = "44CC44";
        private const string COLOR_ADULT = "44CCCC";
        private const string COLOR_SENIOR = "CC44CC";


        private GUIStyle m_inlineBtnStyle;
        private LineData m_currentLineData;
        private GUIStyle m_rightTextLabel;
        private GUIStyle m_centerTextLabel;

        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init();
            m_childTex = TextureUtils.NewSingleColorForUI(ColorExtensions.FromRGB(COLOR_CHILDREN));
            m_teenTex = TextureUtils.NewSingleColorForUI(ColorExtensions.FromRGB(COLOR_TEEN));
            m_youngTex = TextureUtils.NewSingleColorForUI(ColorExtensions.FromRGB(COLOR_YOUNG));
            m_adultTex = TextureUtils.NewSingleColorForUI(ColorExtensions.FromRGB(COLOR_ADULT));
            m_seniorTex = TextureUtils.NewSingleColorForUI(ColorExtensions.FromRGB(COLOR_SENIOR));
            m_autonameTex = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_AutoNameIcon);
            picker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            picker.Visible = false;
            Visible = false;
        }
        private void Init() => Init(InitTitle, new Rect(StartPosition, StartSize), Resizable, true, MinSize, MaxSize);
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
            InitStyles();
            if (CurrentLine != 0)
            {
                ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[CurrentLine];
                m_currentLineData.GetUpdated();
                GUILayout.Label(string.Format(Str.itm_lineView_distanceStops, m_currentLineData.m_lengthKm, m_currentLineData.m_stopsCount, m_currentLineData.TripsSaved), m_centerTextLabel);
                GUILayout.Space(4);
                using (new GUILayout.HorizontalScope())
                {
                    GUIKwyttoCommons.TextWithLabel(size.x - 25, Str.itm_lineView_lineName, m_currentLineData.LineName, (x) => m_currentLineData.LineName = x);
                    GUIKwyttoCommons.SquareTextureButton(m_autonameTex, "", () => ITMLineUtils.DoAutoname(CurrentLine), size: 20);
                }
                GUIKwyttoCommons.AddColorPicker(Str.itm_lineView_lineColor, picker, m_currentLineData.LineColor, (x) => m_currentLineData.LineColor = x ?? default);
                GUIKwyttoCommons.AddIntField(size.x, Str.itm_lineView_lineInternalNumber, m_currentLineData.LineInternalSequentialNumber(), (x) => TransportManager.instance.m_lines.m_buffer[CurrentLine].m_lineNumber = (ushort)(x ?? 0), min: 0, max: 65535);
                GUIKwyttoCommons.AddComboBox(size.x, Str.itm_lineView_lineActivity, m_currentLineData.LineActivity, m_lineActivityOptionsNames, m_lineActivityOptions, this, (x) => m_currentLineData.LineActivity = x);
                if (m_currentLineData.m_type.HasVehicles())
                {
                    GUIKwyttoCommons.AddSliderInt(size.x, Str.itm_lineView_ticketPrice, m_currentLineData.TicketPrice, (x) => m_currentLineData.TicketPrice = x, 0, tl.Info.m_ticketPrice * 5);
                    GUILayout.Space(10);
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

                    if (m_currentLineData.FreeStops > 0)
                    {
                        GUILayout.Label(string.Format(Str.itm_lineView_thereAreFreeStops, m_currentLineData.FreeStops, m_currentLineData.m_stopsCount), m_centerTextLabel);
                    }
                }

                if (m_currentLineData.Broken)
                {
                    GUILayout.Label(Str.itm_lineView_thisLineIsBroken, m_centerTextLabel);
                }
                GUILayout.FlexibleSpace();

                GUILayout.Label($"<b>{Str.itm_lineView_weeklyDataTitle}</b>", m_centerTextLabel);
                GUILayout.Label(string.Format(Str.itm_lineView_residentTourists, m_currentLineData.m_passengersResCount, m_currentLineData.m_passengersTouCount), m_centerTextLabel);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label($"{m_currentLineData.PassengersChild}", m_rightTextLabel);
                        GUILayout.Label($"{m_currentLineData.PassengersTeen}", m_rightTextLabel);
                        GUILayout.Label($"{m_currentLineData.PassengersYoung}", m_rightTextLabel);
                        GUILayout.Label($"{m_currentLineData.PassengersAdult}", m_rightTextLabel);
                        GUILayout.Label($"{m_currentLineData.PassengersSenior}", m_rightTextLabel);
                    }
                    GUILayout.Space(2);
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.Label($"<color=#{COLOR_CHILDREN}>{Str.itm_lineView_childrenLbl}</color>");
                        GUILayout.Label($"<color=#{COLOR_TEEN}>{Str.itm_lineView_teenLbl}</color>");
                        GUILayout.Label($"<color=#{COLOR_YOUNG}>{Str.itm_lineView_youngLbl}</color>");
                        GUILayout.Label($"<color=#{COLOR_ADULT}>{Str.itm_lineView_adultLbl}</color>");
                        GUILayout.Label($"<color=#{COLOR_SENIOR}>{Str.itm_lineView_seniorLbl}</color>");
                    }
                    var height = GUILayoutUtility.GetLastRect().height;
                    GUILayout.Space(10);
                    using (new GUILayout.VerticalScope())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(100);
                        }
                    }
                    var position = GUILayoutUtility.GetLastRect().position + new Vector2(0, 3);
                    var sumPassengers = m_currentLineData.PassengersChild + m_currentLineData.PassengersTeen + m_currentLineData.PassengersYoung + m_currentLineData.PassengersAdult + m_currentLineData.PassengersSenior;
                    if (sumPassengers > 0)
                    {
                        float pixelsPerPassenger = height / sumPassengers;
                        float currentHeightStep = 0;
                        float lastRectSize;
                        GUI.DrawTexture(new Rect(position, new Vector2(100, lastRectSize = pixelsPerPassenger * m_currentLineData.PassengersChild)), m_childTex);
                        GUI.DrawTexture(new Rect(position + new Vector2(0, currentHeightStep += lastRectSize), new Vector2(100, lastRectSize = pixelsPerPassenger * m_currentLineData.PassengersTeen)), m_teenTex);
                        GUI.DrawTexture(new Rect(position + new Vector2(0, currentHeightStep += lastRectSize), new Vector2(100, lastRectSize = pixelsPerPassenger * m_currentLineData.PassengersYoung)), m_youngTex);
                        GUI.DrawTexture(new Rect(position + new Vector2(0, currentHeightStep += lastRectSize), new Vector2(100, lastRectSize = pixelsPerPassenger * m_currentLineData.PassengersAdult)), m_adultTex);
                        GUI.DrawTexture(new Rect(position + new Vector2(0, currentHeightStep += lastRectSize), new Vector2(100, lastRectSize = pixelsPerPassenger * m_currentLineData.PassengersSenior)), m_seniorTex);
                    }
                    else
                    {
                        GUI.DrawTexture(new Rect(position, new Vector2(100, height)), m_youngTex);
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(Str.itm_lineView_budgetViewBtn))
                    {
                        if (ToolsModifierControl.IsUnlocked(UnlockManager.Feature.Economy))
                        {
                            ToolsModifierControl.mainToolbar.ShowEconomyPanel(1);
                            WorldInfoPanel.Hide<PublicTransportWorldInfoPanel>();
                        }
                    }
                    if (GUILayout.Button(Str.itm_lineView_linesListBtn))
                    {
                        LinesListingUI.Instance.Visible = true;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(Str.itm_lineView_deleteLine, RedButton))
                    {
                        m_currentLineData.Delete();
                    }
                }
                GUILayout.Space(2);
            }
        }

        private void InitStyles()
        {
            if (m_noBreakLabel is null)
            {
                m_noBreakLabel = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = false,
                    alignment = TextAnchor.MiddleLeft,
                };
            }
            if (m_centerTextLabel is null)
            {
                m_centerTextLabel = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                };
            }
            if (m_rightTextLabel is null)
            {
                m_rightTextLabel = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                };
            }

            if (m_inlineBtnStyle is null)
            {
                m_inlineBtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fixedHeight = 20 * ResolutionMultiplier,
                    fixedWidth = 20 * ResolutionMultiplier,
                    margin = new RectOffset(0, 0, 1, 1),
                };
            }
        }

        public void OnIdChanged(InstanceID currentId)
        {
            if (currentId.TransportLine != 0)
            {
                CurrentLine = currentId.TransportLine;
                m_lineActivityOptionsNames = m_lineActivityOptions.Select(x => x.ValueToI18n()).ToArray();
                m_currentLineData?.Dispose();
                m_currentLineData = LineData.FromLine(CurrentLine);
                Title = TransportManager.instance.GetLineName(CurrentLine);

                ITMLineVehicleSelectionWindow.Instance.OnIdChanged(currentId);
                ITMLineStopsWindow.Instance.OnIdChanged(currentId);
                ITMLineVehicleSelectionWindow.Instance.Visible = true;
                ITMLineStopsWindow.Instance.Visible = true;
                Visible = true;
            }
        }
        protected override void OnWindowDestroyed() => Instance = null;
        protected override void OnWindowClosed()
        {
            base.OnWindowClosed();
            ITMLineVehicleSelectionWindow.Instance.Visible = false;
            ITMLineStopsWindow.Instance.Visible = false;
        }
    }

}
