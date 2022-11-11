using Kwytto.Utils;
using System;
using System.Xml.Serialization;

namespace ImprovedTransportManager
{
    public class BudgetEntryXml 
    {
        public enum BudgetType
        {
            Fixed,
            PerHour,
            PerHourAndWeek
        }

        [XmlAttribute("type")]
        public BudgetType Type { get; set; }

        private uint baseBudget = 100;

        private TimeableList<BudgetEntryItemXml> m_cachedBaseBudgetHour;
        private TimeableList<BudgetEntryItemXml> CachedBaseBudgetHour
        {
            get
            {
                if (m_cachedBaseBudgetHour is null)
                {
                    m_cachedBaseBudgetHour = new TimeableList<BudgetEntryItemXml>();
                    m_cachedBaseBudgetHour.Add(new BudgetEntryItemXml { Value = BaseBudget });
                }
                return m_cachedBaseBudgetHour;
            }
        }

        [XmlAttribute("baseBudget")]
        public uint BaseBudget
        {
            get => baseBudget; set
            {
                baseBudget = value;
                m_cachedBaseBudgetHour = null;
            }
        }

        [XmlElement("default")]
        public TimeableList<BudgetEntryItemXml> DefaultValue = new TimeableList<BudgetEntryItemXml>();

        [XmlElement("overrides")]
        public SimpleNonSequentialList<TimeableList<BudgetEntryItemXml>> OverrideValues = new SimpleNonSequentialList<TimeableList<BudgetEntryItemXml>>();

        internal TimeableList<BudgetEntryItemXml> GetWeekDayTable(DayOfWeek referenceWeekday)
        {
            switch (Type)
            {
                case BudgetType.Fixed: return CachedBaseBudgetHour;
                case BudgetType.PerHour:
                    return DefaultValue;
                case BudgetType.PerHourAndWeek:
                    if (OverrideValues.TryGetValue((long)referenceWeekday, out var list))
                    {
                        return list;
                    }
                    return DefaultValue;
            }
            return null;
        }
    }

    public class BudgetEntryItemXml : UintValueHourEntryXml<BudgetEntryItemXml> { }


}
