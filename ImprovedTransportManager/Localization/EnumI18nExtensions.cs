﻿using ImprovedTransportManager;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.UI;
using Kwytto.Localization;
using System;
using System.Linq;
namespace VehicleSkins.Localization
{
    internal static class EnumI18nExtensions
    {
        public static string ValueToI18n(this Enum variable, string variation = null)
        {
            switch (variable)
            {
                case LineActivityOptions l:
                    if (variation == "SHORT")
                    {
                        switch (l)
                        {
                            case LineActivityOptions.None: return Str.itm_Enum__LineActivityOptions_None_SHORT;
                            case LineActivityOptions.Day: return Str.itm_Enum__LineActivityOptions_Day_SHORT;
                            case LineActivityOptions.Night: return Str.itm_Enum__LineActivityOptions_Night_SHORT;
                            case LineActivityOptions.DayNight: return Str.itm_Enum__LineActivityOptions_DayNight_SHORT;
                        }
                    }
                    else
                    {
                        switch (l)
                        {
                            case LineActivityOptions.None: return Str.itm_Enum__LineActivityOptions_None;
                            case LineActivityOptions.Day: return Str.itm_Enum__LineActivityOptions_Day;
                            case LineActivityOptions.Night: return Str.itm_Enum__LineActivityOptions_Night;
                            case LineActivityOptions.DayNight: return Str.itm_Enum__LineActivityOptions_DayNight;
                        }
                    }
                    break;
                case VehicleShowDataType v:
                    switch (v)
                    {
                        case VehicleShowDataType.PassengerCapacity: return Str.itm_Enum__VehicleShowDataType_PassengerCapacity;
                        case VehicleShowDataType.Identifier: return Str.itm_Enum__VehicleShowDataType_Identifier;
                        case VehicleShowDataType.ProfitAllTime: return Str.itm_Enum__VehicleShowDataType_ProfitAllTime;
                        case VehicleShowDataType.ProfitLastWeek: return Str.itm_Enum__VehicleShowDataType_ProfitLastWeek;
                        case VehicleShowDataType.ProfitCurrentWeek: return Str.itm_Enum__VehicleShowDataType_ProfitCurrentWeek;
                    }
                    break;
                case BudgetEntryXml.BudgetType t:
                    switch (t)
                    {
                        case BudgetEntryXml.BudgetType.Fixed: return Str.itm_Enum__BudgetType_Fixed;
                        case BudgetEntryXml.BudgetType.PerHour: return Str.itm_Enum__BudgetType_PerHour;
                        case BudgetEntryXml.BudgetType.PerHourAndWeek: return Str.itm_Enum__BudgetType_PerHourAndWeek;
                    }
                    break;
            }
            return variable.ValueToI18nKwytto();
        }

        public static string[] GetAllValuesI18n<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<Enum>().Select(x => x.ValueToI18n()).ToArray();
    }
}
