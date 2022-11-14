using ColossalFramework.UI;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using ImprovedTransportManager.Xml;
using Kwytto.Data;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using static ImprovedTransportManager.TransportSystems.TransportSystemTypeExtensions;
using Random = System.Random;

namespace ImprovedTransportManager.Data
{

    [XmlRoot("ITMTransportLineSettings")]
    public class ITMTransportLineSettings : DataExtensionBase<ITMTransportLineSettings>
    {
        [XmlElement("LineConfigurations")]
        public SimpleNonSequentialList<ITMTransportLineXml> Lines { get; set; } = new SimpleNonSequentialList<ITMTransportLineXml>();
        internal void SafeCleanEntry(ushort lineID) => Lines[lineID] = new ITMTransportLineXml();
        public ITMTransportLineXml SafeGetLine(ushort lineId)
        {
            if (!Lines.ContainsKey(lineId))
            {
                Lines[lineId] = new ITMTransportLineXml
                {
                    CachedTransportType = TransportSystemTypeExtensions.FromLineId(lineId, false)
                };
            }
            return Lines[lineId];
        }


        [XmlElement("terminalStops")]
        public SimpleXmlHashSet<ushort> m_terminalStops { get; set; } = new SimpleXmlHashSet<ushort>();

        public ITMTransportLineSettings()
        {
            VehiclesIndexes.instance.PrefabsData
                .Select(x => x.Value.Info is VehicleInfo info && info.m_placementStyle != ItemClass.Placement.Procedural ? Tuple.New(info.ToTST(), info) : null)
                .Where(x => x != null && x.First != default && x.First.IsCitywide(x.Second.m_class.m_level))
                .GroupBy(x => x.First)
                .ForEach(x => m_basicAssetsList[x.Key] = x.Select(y => y.Second).ToList());
        }

        public override string SaveId => $"K45_ITM_ITMTransportLineSettings";

        private readonly Dictionary<TransportSystemType, List<VehicleInfo>> m_basicAssetsList = new Dictionary<TransportSystemType, List<VehicleInfo>>();

        #region Groups
        [XmlIgnore]
        private Dictionary<TransportSystemType, SimpleNonSequentialList<HashSet<VehicleInfo>>> GroupsAssetList = new Dictionary<TransportSystemType, SimpleNonSequentialList<HashSet<VehicleInfo>>>();
        [XmlIgnore]
        private Dictionary<TransportSystemType, SimpleNonSequentialList<HashSet<ushort>>> GroupsDepotList = new Dictionary<TransportSystemType, SimpleNonSequentialList<HashSet<ushort>>>();
        [XmlIgnore]
        private Dictionary<TransportSystemType, SimpleNonSequentialList<BudgetEntryXml>> GroupsBudgetList = new Dictionary<TransportSystemType, SimpleNonSequentialList<BudgetEntryXml>>();
        [XmlIgnore]
        private Dictionary<TransportSystemType, SimpleNonSequentialList<TicketPriceEntryXml>> GroupsFareList = new Dictionary<TransportSystemType, SimpleNonSequentialList<TicketPriceEntryXml>>();

        private O SafeGetGroupData<O>(Dictionary<TransportSystemType, SimpleNonSequentialList<O>> groupData, TransportSystemType tst, byte groupId) where O : class, new()
        {
            if (groupId == 0) return null;
            if (!groupData.ContainsKey(tst))
            {
                groupData[tst] = new SimpleNonSequentialList<O>();
            }
            if (!groupData[tst].ContainsKey(groupId))
            {
                groupData[tst][groupId] = new O();
            }
            return groupData[tst][groupId];
        }

        private SimpleNonSequentialList<X> ConvertToXmlFormat<O, X>(Dictionary<TransportSystemType, SimpleNonSequentialList<O>> source, Func<KeyValuePair<long, O>, X> converter)
        {
            var result = new SimpleNonSequentialList<X>();
            source.SelectMany(x => x.Value.Select(y => Tuple.New((uint)x.Key | (((uint)y.Key & BITMASK_FREE) << ROLL_FREE), converter(y)))).ForEach(x => result.Add(x.First, x.Second));
            return result;
        }
        private void ConvertFromXmlFormat<O, X>(SimpleNonSequentialList<X> source, Dictionary<TransportSystemType, SimpleNonSequentialList<O>> target, Func<X, TransportSystemType, O> converter)
        {
            target.Clear();
            source.ForEach(x =>
            {
                TransportSystemType tst = (TransportSystemType)(x.Key & ~MASK_FREE);
                byte idx = (byte)((x.Key & MASK_FREE) >> ROLL_FREE);
                if (idx == 0) return;
                if (!target.ContainsKey(tst))
                {
                    target[tst] = new SimpleNonSequentialList<O>();
                }
                target[tst][idx] = converter(x.Value, tst);
            });
        }

        [XmlElement("GroupsAsset")]
        public SimpleNonSequentialList<SimpleXmlList<string>> GroupAssetListXml
        {
            get => ConvertToXmlFormat(GroupsAssetList, y => new SimpleXmlList<string>(y.Value.Select(z => z.name)));
            set => ConvertFromXmlFormat(value, GroupsAssetList, (x, tst) => x.GetAllLoadedForType(tst));
        }

        [XmlElement("GroupsDepot")]
        public SimpleNonSequentialList<SimpleXmlList<ushort>> GroupsDepotListXml
        {
            get => ConvertToXmlFormat(GroupsDepotList, y => new SimpleXmlList<ushort>(y.Value));
            set
            {
                var buff = BuildingManager.instance.m_buildings.m_buffer;
                ConvertFromXmlFormat(value, GroupsDepotList, (x, tst) => new HashSet<ushort>(x.Where(y => tst.IsFromSystem(buff[y].Info.m_buildingAI as DepotAI))));
            }
        }
        [XmlElement("GroupsBudget")]
        public SimpleNonSequentialList<BudgetEntryXml> GroupsBudgetListXml
        {
            get => ConvertToXmlFormat(GroupsBudgetList, y => y.Value);
            set => ConvertFromXmlFormat(value, GroupsBudgetList, (x, tst) => x);
        }
        [XmlElement("GroupsFares")]
        public SimpleNonSequentialList<TicketPriceEntryXml> GroupsFareListXml
        {
            get => ConvertToXmlFormat(GroupsFareList, y => y.Value);
            set => ConvertFromXmlFormat(value, GroupsFareList, (x, tst) => x);
        }
        #endregion

        #region Asset List
        public List<VehicleInfo> GetBasicAssetListForLine(ushort lineId)
        {

            var tsd = FromLineId(lineId, false);
            return m_basicAssetsList.TryGetValue(tsd, out var result) ? result : null;
        }
        public HashSet<VehicleInfo> GetSelectedBasicAssetsForLine(ushort lineId) => SafeGetLine(lineId).SelfAssetList;
        public Dictionary<string, VehicleInfo> GetAllBasicAssetsForLine(ushort lineId)
        {
            var tsd = FromLineId(lineId, false);
            return m_basicAssetsList[tsd]
                .Select(x => Tuple.New(x.GetUncheckedLocalizedTitle(), x))
                .GroupBy(x => x.First)
                .SelectMany(x => x.Select((y, i) => i != 0 ? Tuple.New(y.First + $" ({i + 1})", y.Second) : y))
                .ToDictionary(x => x.First, x => x.Second);
        }
        public HashSet<VehicleInfo> GetEffectiveAssetsForLine(ushort lineId)
        {
            var config = SafeGetLine(lineId);
            return config.AssetGroup != 0
                ? SafeGetGroupData(GroupsAssetList, config.CachedTransportType, config.AssetGroup)
                : config.SelfAssetList;
        }
        public bool IsInfoAllowedToLine(VehicleInfo info, ushort lineId)
        {
            var assetList = GetEffectiveAssetsForLine(lineId);
            return assetList.Count == 0 || assetList.Contains(info);
        }

        public VehicleInfo GetAModel(ushort lineId)
        {
            VehicleInfo info = null;
            var assetList = GetEffectiveAssetsForLine(lineId);
            if (assetList.Count > 0)
            {
                info = assetList.ElementAt(new Random().Next(assetList.Count));
            }
            return info;
        }

        #endregion

        #region Depot List
        public HashSet<ushort> GetEffectiveDepotsForLine(ushort lineId)
        {
            var config = SafeGetLine(lineId);
            return config.DepotGroup != 0
                ? SafeGetGroupData(GroupsDepotList, config.CachedTransportType, config.AssetGroup)
                : config.AllowedDepots;
        }

        public ushort GetADepot(ushort lineId)
        {
            ushort depot = 0;
            var depotList = GetEffectiveDepotsForLine(lineId);
            if (depotList.Count > 0)
            {
                depot = depotList.ElementAt(new Random().Next(depotList.Count));
            }
            return depot;
        }

        internal ushort GetWeekdayHourValue(ushort lineId, DayOfWeek referenceWeekday, uint fullHour)
        {
            var config = SafeGetLine(lineId);
            if (config.BudgetGroup == 0)
            {
                return ushort.MaxValue;
            }
            var tst = FromLineId(lineId, false);
            return GroupsBudgetList.TryGetValue(tst, out var groupList) && groupList.TryGetValue(config.BudgetGroup, out var group)
                ? group.GetBudgetAtWeekHour(referenceWeekday, fullHour)
                : ushort.MaxValue;
        }
        #endregion

        #region Budget
        public BudgetEntryXml GetBudgetGroup(TransportSystemType type, int group)
        {
            if (group == 0)
            {
                return null;
            }
            if (!GroupsBudgetList.TryGetValue(type, out var groups))
            {
                GroupsBudgetList[type] = groups = new SimpleNonSequentialList<BudgetEntryXml>();
            }
            if (!groups.TryGetValue(group, out var groupData))
            {
                groups[group] = groupData = new BudgetEntryXml();
            }
            return groupData;
        }
        #endregion
    }
}
