using ColossalFramework.Globalization;
using ImprovedTransportManager.Utility;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    internal class VehicleData : IDisposable
    {

        private Color m_vehicleColor;
        private Texture2D m_cachedBg;
        private GUIStyle m_cachedStyle;
        public ushort m_nextStop;
        public int m_nextStopIdx;
        public int m_progressItemIdx;
        public VehicleStopProgressState m_progressState;
        public int m_capacity;
        public int m_passengers;
        public float m_profitAllTime;
        public float m_profitLastWeek;
        public float m_profitCurrentWeek;

        public ushort VehicleId { get; set; }
        public Color VehicleColor
        {
            get => m_vehicleColor; set
            {
                if (m_vehicleColor != value)
                {
                    m_vehicleColor = value;
                    GameObject.Destroy(m_cachedBg);
                    m_cachedBg = null;
                    m_cachedStyle = null;
                }
            }
        }
        public GUIStyle CachedStyle
        {
            get
            {
                if (m_cachedStyle == null)
                {
                    var contrast = m_vehicleColor.ContrastColor();
                    m_cachedStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        margin = new RectOffset(0, 0, -4, -4),
                        contentOffset = new Vector2(0, -4),
                        padding = new RectOffset(0, 0, -4, -4),
                        normal =
                            {
                                textColor =contrast,
                                background = CachedBG
                            },
                        fontSize = Mathf.CeilToInt(.9f * ITMLineStopsWindow.Instance.DefaultSize),
                        hover = {
                            textColor =Color.white  ,
                            background = GUIKwyttoCommons.darkTransparentTexture
                        },
                    };
                }
                return m_cachedStyle;
            }
        }
        public Texture2D CachedBG
        {
            get
            {
                if (m_cachedBg is null)
                {
                    m_cachedBg = TextureUtils.NewSingleColorForUI(VehicleColor);
                }
                return m_cachedBg;
            }
        }
        public string VehicleName => ITMLineUtils.GetEffectiveVehicleName(VehicleId);

        public float StationPositionMultiplierY => m_nextStopIdx + ((float)m_progressState * .25f);
        public Vector2 GetPositionOffset(float maxX, float stationHeight) => new Vector2((maxX * .75f) - (m_progressItemIdx % 4 * maxX * .25f), (stationHeight * StationPositionMultiplierY) + (Mathf.Floor(m_progressItemIdx * .25f) * 18) - 9f);

        public void Dispose()
        {
            GameObject.Destroy(m_cachedBg);
        }

        internal string GetContentFor(VehicleShowDataType contentType)
        {
            switch (contentType)
            {
                case VehicleShowDataType.PassengerCapacity: return $"{m_passengers}/{m_capacity}";
                case VehicleShowDataType.Identifier: return VehicleName;
                case VehicleShowDataType.ProfitAllTime: return m_profitAllTime.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
                case VehicleShowDataType.ProfitLastWeek: return m_profitLastWeek.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
                case VehicleShowDataType.ProfitCurrentWeek: return m_profitCurrentWeek.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            }
            return "";
        }
    }


}

