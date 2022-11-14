using HarmonyLib;
using Kwytto.Utils;
using MonoMod.Utils;
using System;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

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

        [XmlElement("budgetGroups")]
        public ushort[] BudgetGroups
        {
            get => budgetGroups; set
            {
                budgetGroups = value.Length == 0 ? new ushort[] { 100 } : value.Select(x => (ushort)Mathf.Clamp(x, 0, 500)).ToArray();
            }
        }
        private byte[] defaultValue = new byte[24];

        [XmlElement("defaultBudgetGroups")]
        public byte[] DefaultValue
        {
            get => defaultValue; set
            {
                if (defaultValue?.Length != 24)
                {
                    defaultValue = new byte[24];
                }
                else
                {
                    defaultValue = value;
                }
            }
        }

        [XmlElement("overrideBudgetGroups")]
        public SimpleNonSequentialList<byte[]> OverrideValues
        {
            get => overrideValues; set
            {
                overrideValues = new SimpleNonSequentialList<byte[]>();
                overrideValues.AddRange(value.Where(x => x.Value.Length == 24 && x.Key >= 0 && x.Key <= 6).ToDictionary(x => x.Key, x => x.Value));
            }
        }

        internal ushort BaseBudget { get => BudgetGroups[0]; set => BudgetGroups[0] = value; }

        private SimpleNonSequentialList<byte[]> overrideValues = new SimpleNonSequentialList<byte[]>();

        private ushort[] budgetGroups = new ushort[] { 100 };

        internal ushort GetBudgetAtWeekHour(DayOfWeek referenceWeekday, uint refHour)
        {
            if (refHour >= 24)
            {
                return ushort.MaxValue;
            }
            byte targetGroup;
            switch (Type)
            {
                case BudgetType.Fixed: return BudgetGroups[0];
                case BudgetType.PerHour:
                    targetGroup = DefaultValue[refHour];
                    break;
                case BudgetType.PerHourAndWeek:
                    targetGroup = OverrideValues.TryGetValue((long)referenceWeekday, out var list) ? list[refHour] : DefaultValue[refHour];
                    break;
                default:
                    return ushort.MaxValue;
            }
            return targetGroup < BudgetGroups.Length ? BudgetGroups[targetGroup] : BudgetGroups[0];
        }

        internal void AddGroup(ushort value)
        {
            budgetGroups = budgetGroups.AddItem(value).ToArray();
        }

        internal void RemoveGroup(int idx)
        {
            budgetGroups = budgetGroups.Where((_, i) => i != idx).ToArray();
            defaultValue = defaultValue.Select(x => (byte)(x == idx ? 0 : x > idx ? x - 1 : x)).ToArray();
            foreach (var key in overrideValues.Keys.ToArray())
            {
                overrideValues[key] = overrideValues[key].Select(x => (byte)(x == idx ? 0 : x > idx ? x - 1 : x)).ToArray();
            }
        }
        internal void IncrementGroupAt(int v, int i)
        {
            if (v == -1) //Base
            {
                defaultValue[i]++;
                defaultValue[i] %= (byte)budgetGroups.Length;
            }
            else if (overrideValues.ContainsKey(v))
            {
                overrideValues[v][i]++;
                overrideValues[v][i] %= (byte)budgetGroups.Length;
            }
        }
    }
}
