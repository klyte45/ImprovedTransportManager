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
        [XmlElement("Configurations")]
        public SimpleNonSequentialList<ITMTransportLineXml> Lines { get; set; } = new SimpleNonSequentialList<ITMTransportLineXml>();
        internal void SafeCleanEntry(ushort lineID) => Lines[lineID] = new ITMTransportLineXml();
        public ITMTransportLineXml SafeGetLine(uint lineId)
        {
            if (!Lines.ContainsKey(lineId))
            {
                Lines[lineId] = new ITMTransportLineXml();
            }
            return Lines[lineId];
        }

        public void Awake()
        {
            VehiclesIndexes.instance.PrefabsData
                .Select(x => x.Value.Info is VehicleInfo info ? Tuple.New(info.ToTST(), info) : null)
                .Where(x => x != null && x.First != default)
                .GroupBy(x => x.First)
                .ForEach(x => m_basicAssetsList[x.Key] = x.Select(y => y.Second).ToList());
        }

        public override string SaveId => $"K45_ITM_ITMTransportLineSettings";

        private readonly Dictionary<TransportSystemType, List<VehicleInfo>> m_basicAssetsList = new Dictionary<TransportSystemType, List<VehicleInfo>>();

        #region Groups
        [XmlIgnore]
        private Dictionary<TransportSystemType, SimpleNonSequentialList<HashSet<VehicleInfo>>> GroupsAssetList = new Dictionary<TransportSystemType, SimpleNonSequentialList<HashSet<VehicleInfo>>>();
        [XmlIgnore]
        private Dictionary<TransportSystemType, SimpleNonSequentialList<List<ushort>>> GroupsDepotList = new Dictionary<TransportSystemType, SimpleNonSequentialList<List<ushort>>>();
        [XmlIgnore]
        private Dictionary<TransportSystemType, SimpleNonSequentialList<UintValueHourEntryXml<BudgetEntryXml>>> GroupsBudgetList = new Dictionary<TransportSystemType, SimpleNonSequentialList<UintValueHourEntryXml<BudgetEntryXml>>>();
        [XmlIgnore]
        private Dictionary<TransportSystemType, SimpleNonSequentialList<UintValueHourEntryXml<TicketPriceEntryXml>>> GroupsTariffList = new Dictionary<TransportSystemType, SimpleNonSequentialList<UintValueHourEntryXml<TicketPriceEntryXml>>>();

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
                ConvertFromXmlFormat(value, GroupsDepotList, (x, tst) => x.Where(y => tst.IsFromSystem(buff[y].Info.m_buildingAI as DepotAI)).ToList());
            }
        }
        [XmlElement("GroupsBudget")]
        public SimpleNonSequentialList<UintValueHourEntryXml<BudgetEntryXml>> GroupsBudgetListXml
        {
            get => ConvertToXmlFormat(GroupsBudgetList, y => y.Value);
            set => ConvertFromXmlFormat(value, GroupsBudgetList, (x, tst) => x);
        }
        [XmlElement("GroupsTariffs")]
        public SimpleNonSequentialList<UintValueHourEntryXml<TicketPriceEntryXml>> GroupsTariffListXml
        {
            get => ConvertToXmlFormat(GroupsTariffList, y => y.Value);
            set => ConvertFromXmlFormat(value, GroupsTariffList, (x, tst) => x);
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
            return m_basicAssetsList[tsd].ToDictionary(x => x.GetUncheckedLocalizedTitle(), x => x);
        }
        public HashSet<VehicleInfo> GetEffectiveAssetsForLine(ushort lineId)
        {
            var config = SafeGetLine(lineId);
            if (config.AssetGroup != 0)
            {
                return SafeGetGroupData(GroupsAssetList, config.CachedTransportType, config.AssetGroup);
            }
            else
            {
                return config.SelfAssetList;
            }
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

    }
}
