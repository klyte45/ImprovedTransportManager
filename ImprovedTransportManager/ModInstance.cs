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

[assembly: AssemblyVersion("1.0.0.*")]
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
             new UUIWindowButtonContainerPlaceholder(
             buttonName: $"{SimpleName}",
             tooltip: $"{SimpleName}",
             iconPath: "ModIcon",
             windowGetter: ()=>LinesListingUI.Instance
             )
        }.Where(x => x != null).ToArray());

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
