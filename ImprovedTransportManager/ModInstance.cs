extern alias UUI;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("0.1.0.3")]
namespace ImprovedTransportManager
{
    public class ModInstance : BasicIUserMod<ModInstance, ITMMainController>
    {
        public override string SimpleName { get; } = "Improved Transport Manager";
        public override string SafeName { get; } = "ImprovedTransportManager";
        public override string Description { get; } = Str.root_modDescription;

        public override string Acronym => "ITM";

        public override Color ModColor => ColorExtensions.FromRGB("180f7d");

        protected override void SetLocaleCulture(CultureInfo culture) => Str.Culture = culture;

        private IUUIButtonContainerPlaceholder[] cachedUUI;
        public override IUUIButtonContainerPlaceholder[] UUIButtons => cachedUUI ?? (cachedUUI = new IUUIButtonContainerPlaceholder[]
        {
             LinesListingBtn =  new UUIWindowButtonContainerPlaceholder(
             buttonName: $"{SimpleName} - AAA",
             tooltip: $"{SimpleName} - {Str.itm_linesListingWindow_tilte}",
             iconPath: "LinesListIcon",
             windowGetter: ()=>LinesListingUI.Instance
             ),
             CitySettingsBtn = new UUIWindowButtonContainerPlaceholder(
             buttonName: $"{SimpleName} - BBB",
             tooltip: $"{SimpleName} - {Str.itm_citySettings_title}",
             iconPath: "CitySettingsIcon",
             windowGetter: ()=>ITMCitySettingsGUI.Instance
             ),
             new UUIWindowButtonContainerPlaceholder(
             buttonName: $"{SimpleName} - CCC",
             tooltip: $"{SimpleName} - {Str.itm_statistics_title}",
             iconPath: "StatisticsIcon",
             windowGetter: ()=>ITMStatisticsGUI.Instance
             )
        }.Where(x => x != null).ToArray());

        internal static UUIWindowButtonContainerPlaceholder CitySettingsBtn { get; private set; }
        internal static UUIWindowButtonContainerPlaceholder LinesListingBtn { get; private set; }

        protected override Dictionary<ulong, string> IncompatibleModList { get; } = new Dictionary<ulong, string>
        {
            [1312767991] = "Transport Lines Manager"
        };

        protected override List<string> IncompatibleDllModList { get; } = new List<string>
        {
            "KlyteTransportLinesManager",
            "TransportLinesManager",
        };

    }
}
