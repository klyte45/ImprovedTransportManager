using ColossalFramework;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.UI;
using System.IO;
using UnityEngine;
using static Kwytto.LiteUI.KwyttoDialog;

namespace ImprovedTransportManager.UI
{
    public class ITMGeneralTab : IGUIVerticalITab
    {
        public string TabDisplayName => Str.itm_generalSettings_title;

        public void DrawArea(Vector2 tabAreaSize)
        {
            GUIKwyttoCommons.AddToggle(Str.itm_generalSettings_expertMode, ref ITMCitySettings.Instance.expertMode);
            GUILayout.Space(10);
            GUILayout.Label(Str.itm_generalSettings_defaultGeneralSettings);
            if (GUILayout.Button(Str.itm_generalSettings_exportCurrentToFile))
            {
                ITMCitySettings.ExportAsDefault();
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    buttons = new[]
                        {
                            SpaceBtn,
                            new ButtonDefinition
                            {
                                title = KStr.comm_goToFileLocation,
                                onClick = ()=> {
                                    Utils.OpenInFileBrowser( ITMCitySettings.DefaultsFilePath);
                                    return false;
                                }
                            },
                            new ButtonDefinition
                            {
                                title = KStr.comm_releaseNotes_Ok,
                                onClick = ()=>true
                            }
                        },
                    scrollText = string.Format(Str.itm_generalSettings_defaultCityFileExportedMessage, ITMCitySettings.DefaultsFilePath)
                });
            }
            if (File.Exists(ITMCitySettings.DefaultsFilePath) && GUILayout.Button(Str.itm_generalSettings_importCurrentFromFile))
            {

                ITMCitySettings.ImportFromDefault();
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    buttons = KwyttoDialog.basicOkButtonBar,
                    scrollText = Str.itm_generalSettings_defaultCityFileImportedMessage
                });
            }
            GUILayout.Space(10);
            GUILayout.Label(Str.itm_generalSettings_assetsSettings);
            if (GUILayout.Button(Str.itm_generalSettings_exportCurrentToFile))
            {
                ITMAssetSettings.ExportAsDefault();
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    buttons = new[]
                        {
                            SpaceBtn,
                            new ButtonDefinition
                            {
                                title = KStr.comm_goToFileLocation,
                                onClick = ()=> {
                                    Utils.OpenInFileBrowser( ITMAssetSettings.DefaultsFilePath);
                                    return false;
                                }
                            },
                            new ButtonDefinition
                            {
                                title = KStr.comm_releaseNotes_Ok,
                                onClick = ()=>true
                            }
                        },
                    scrollText = string.Format(Str.itm_generalSettings_defaultAssetFileExportedMessage, ITMAssetSettings.DefaultsFilePath)
                });
            }
            if (File.Exists(ITMAssetSettings.DefaultsFilePath) && GUILayout.Button(Str.itm_generalSettings_importCurrentFromFile))
            {
                ITMAssetSettings.ImportFromDefault();
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    buttons = KwyttoDialog.basicOkButtonBar,
                    scrollText = Str.itm_generalSettings_defaultAssetFileImportedMessage
                });
            }
        }

        public void Reset()
        {
        }
    }
}
