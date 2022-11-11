using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    public abstract class BasicStatisticsTableView<D> : IGUIVerticalITab where D : BasicReportData, new()
    {
        public abstract string TabDisplayName { get; }

        protected Func<ushort> GetCurrentLine { get; }
        protected Func<ushort> GetCurrentStop { get; }
        protected Func<ushort> GetCurrentVehicle { get; }

        protected readonly List<D> loadedData = new List<D>();

        protected LoadedDataType loadedReportType;

        private static GUIStyle cachedCellStyle;
        public BasicStatisticsTableView(Func<ushort> getCurrentLine, Func<ushort> getCurrentStop, Func<ushort> getCurrentVehicle)
        {
            GetCurrentLine = getCurrentLine;
            GetCurrentStop = getCurrentStop;
            GetCurrentVehicle = getCurrentVehicle;
        }

        private uint lastUpdateFrame;

        public void DrawArea(Vector2 tabAreaSize)
        {
            if (cachedCellStyle is null)
            {
                cachedCellStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
            if (lastUpdateFrame + 23 < SimulationManager.instance.m_referenceFrameIndex)
            {
                lastUpdateFrame = SimulationManager.instance.m_referenceFrameIndex;
                loadedData.Clear();
                if (GetCurrentStop() is ushort x && x != 0)
                {
                    loadedData.AddRange(GetStopData(x));
                    loadedReportType = LoadedDataType.Stop;
                }
                else if (GetCurrentVehicle() is ushort v && v != 0)
                {
                    loadedData.AddRange(GetVehicleData(v));
                    loadedReportType = LoadedDataType.Vehicle;
                }
                else if (GetCurrentLine() is ushort w && w != 0)
                {
                    loadedData.AddRange(GetLineData(w));
                    loadedReportType = LoadedDataType.Line;
                }

                if (loadedData.Count == 0)
                {
                    loadedReportType = LoadedDataType.None;
                }
                m_totalizer = new D();
                if (loadedReportType != LoadedDataType.None)
                {
                    loadedData.Take(loadedData.Count - 1).ForEach(y => AddToTotalizer(m_totalizer, y));
                }
            }
            if (loadedData.Count == 0)
            {
                GUILayout.Label(Str.itm_statisticsTable_dataNotAvailableForThisFilter, cachedCellStyle);
                return;
            }

            var columnSizes = (tabAreaSize.x - 140 * GUIWindow.ResolutionMultiplier) / ColumnsDescriptors.Count;
            #region header
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(Str.itm_statisticsTable_periodTitle, cachedCellStyle, GUILayout.Width(100));
                foreach (var funcPair in ColumnsDescriptors)
                {
                    GUILayout.Label(funcPair.First(), cachedCellStyle, GUILayout.Width(columnSizes));
                }
            }
            #endregion

            #region current
            using (new GUILayout.HorizontalScope())
            {
                var currentData = loadedData.Last();
                GUILayout.Label(string.Format(Str.itm_statisticsTable_currentLineColumnValue, GetStartDate(currentData)), cachedCellStyle, GUILayout.Width(100 * GUIWindow.ResolutionMultiplier), GUILayout.Height(40 * GUIWindow.ResolutionMultiplier));

                foreach (var funcPair in ColumnsDescriptors)
                {
                    GUILayout.Label(funcPair.Second(currentData), cachedCellStyle, GUILayout.Width(columnSizes), GUILayout.Height(40 * GUIWindow.ResolutionMultiplier));
                }
            }
            #endregion
            #region total
            GUIKwyttoCommons.Space(0);
            var rect = GUILayoutUtility.GetLastRect();
            GUI.DrawTexture(new Rect(rect.position + new Vector2(0, cachedCellStyle.padding.top), new Vector2(tabAreaSize.x, 40 * GUIWindow.ResolutionMultiplier + cachedCellStyle.padding.top + cachedCellStyle.padding.bottom)), GUIKwyttoCommons.darkGreenTexture);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(ModInstance.Controller.m_isRealTimeEnabled? Str.itm_statisticsTable_totalLast24h : Str.itm_statisticsTable_totalLast16w, cachedCellStyle, GUILayout.Width(100), GUILayout.Height(40));

                foreach (var funcPair in ColumnsDescriptors) 
                {
                    GUILayout.Label(funcPair.Second(m_totalizer), cachedCellStyle, GUILayout.Width(columnSizes), GUILayout.Height(40 * GUIWindow.ResolutionMultiplier));
                }
            }
            #endregion
            GUIKwyttoCommons.Space(8);
            using (var scroll = new GUILayout.ScrollViewScope(m_scroll))
            {
                #region data
                for (int i = loadedData.Count - 2; i >= 0; i--)
                {
                    if (i % 2 == 1)
                    {
                        GUIKwyttoCommons.Space(0);
                        var rect2 = GUILayoutUtility.GetLastRect();
                        GUI.DrawTexture(new Rect(rect2.position + new Vector2(0, cachedCellStyle.padding.top), new Vector2(tabAreaSize.x, 40 + cachedCellStyle.padding.top + cachedCellStyle.padding.bottom)), GUIKwyttoCommons.blackTexture);
                    }
                    D data = loadedData[i];
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(GetDateFormatted(data), cachedCellStyle, GUILayout.Width(100 * GUIWindow.ResolutionMultiplier), GUILayout.Height(40 * GUIWindow.ResolutionMultiplier));
                        foreach (var funcPair in ColumnsDescriptors)
                        {
                            GUILayout.Label(funcPair.Second(data), cachedCellStyle, GUILayout.Width(columnSizes), GUILayout.Height(40 * GUIWindow.ResolutionMultiplier));
                        }
                    }
                }
                #endregion
                m_scroll = scroll.scrollPosition;
            }

        }
        private Vector2 m_scroll;
        protected abstract List<D> GetLineData(ushort lineId);
        protected abstract List<D> GetStopData(ushort stopId);
        protected abstract List<D> GetVehicleData(ushort vehicleId);
        protected abstract void AddToTotalizer(D totalizer, D data);

        protected D m_totalizer;
        public abstract List<Tuple<Func<string>, Func<D, string>>> ColumnsDescriptors { get; }

        public string GetDateFormatted(D data)
        {
            var realtimeEnabled = ModInstance.Controller.m_isRealTimeEnabled;
            return $"{data.StartDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo)}\n{data.EndDate.ToString(realtimeEnabled ? "t" : "d", LocaleManager.cultureInfo)}";
        }
        public string GetStartDate(D data) => $"{data.StartDate.ToString(ModInstance.Controller.m_isRealTimeEnabled ? "t" : "d", LocaleManager.cultureInfo)}";
        public string FloatToHour(float time) => $"{time:00}:{(time % 1 * 60):00}";

        public void Reset()
        {
            loadedData.Clear();
            lastUpdateFrame = 0;
        }
        protected enum LoadedDataType
        {
            None,
            Line,
            Vehicle,
            Stop
        }
    }
}
