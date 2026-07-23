// 
//     Kerbal Engineer Redux
// 
//     Copyright (C) 2016 CYBUTEK
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  

namespace KerbalEngineer.Editor
{
    using System;
    using System.Collections.Generic;
    using Extensions;
    using Helpers;
    using KeyBinding;
    using KSP.UI.Screens;
    using Unity;
using UnityEngine;
using KerbalEngineer.Unity.Localization;
    using KeyBinding = global::KeyBinding;

    public class BuildOverlayPartInfo : MonoBehaviour
    {
        private static bool clickToOpen = true;
        private static bool namesOnly;
        private static bool visible = true;

        private readonly List<PartInfoItem> infoItems = new List<PartInfoItem>();

        private Rect position;
        private Part selectedPart;
        private bool showInfo;
        private bool skipFrame, resizeWindowNextFrame = false;
        private PointerHoverDetector stageUiPointerHoverDetector;

        public static bool ClickToOpen
        {
            get
            {
                return clickToOpen;
            }

            set
            {
                clickToOpen = value;
            }
        }

        public static bool Hidden { get; set; }

        public static bool NamesOnly
        {
            get
            {
                return namesOnly;
            }

            set
            {
                namesOnly = value;
            }
        }

        public static bool Visible
        {
            get
            {
                return visible;
            }

            set
            {
                visible = value;
            }
        }

        protected void OnGUI()
        {
            try
            {
                if (!Visible || Hidden || selectedPart == null || (EditorPanels.Instance.IsMouseOver() && IsPointerOverStaging() == false))
                {
                    return;
                }

                position = GUILayout.Window(GetInstanceID(), position, Window, string.Empty, BuildOverlay.WindowStyle);
            }
            catch (Exception ex)
            {
                MyLogger.Exception(ex);
            }
        }

        protected void Update()
        {
            try
            {
                if (!Visible || Hidden || EditorLogic.RootPart == null || (EditorPanels.Instance.IsMouseOver() && IsPointerOverStaging() == false))
                {
                    return;
                }

                position.x = Mathf.Clamp(Input.mousePosition.x + 16.0f, 0.0f, Screen.width - position.width);
                position.y = Mathf.Clamp(Screen.height - Input.mousePosition.y, 0.0f, Screen.height - position.height);
                if (position.x < Input.mousePosition.x + 20.0f)
                {
                    position.y = Mathf.Clamp(position.y + 20.0f, 0.0f, Screen.height - position.height);
                }

                if (position.x < Input.mousePosition.x + 16.0f && position.y < Screen.height - Input.mousePosition.y)
                {
                    position.x = Input.mousePosition.x - 3 - position.width;
                }

                RaycastHit rayHit;
                Part part = null;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rayHit))
                {
                    // MyLogger.Log("Raycast returned true");
                    part = rayHit.transform.GetComponent<Part>();
                }
                else
                {
                    // MyLogger.Log("Raycast returned false");
                    part = EditorLogic.fetch.ship.parts.Find(p => p.HighlightActive) ?? EditorLogic.SelectedPart;
                }

                if (part != null)
                {
                    if (!part.Equals(selectedPart))
                    {
                        selectedPart = part;
                        ResetInfo();
                    }

                    if (NamesOnly || skipFrame)
                    {
                        skipFrame = false;
                        resizeWindowNextFrame = true;
                        return;
                    }

                    if (resizeWindowNextFrame) {
                        position.height = 0.0f;
                        resizeWindowNextFrame = false;
                    }

                    if (!showInfo && Input.GetKeyDown(KeyBinder.PartInfoShowHide))
                    {
                        showInfo = true;
                    }
                    else if (ClickToOpen && showInfo && Input.GetKeyDown(KeyBinder.PartInfoShowHide))
                    {
                        ResetInfo();
                    }

                    if (showInfo)
                    {
                        PartInfoItem.Release(infoItems);
                        infoItems.Clear();
                        SetCostInfo();
                        SetMassItems();
                        SetResourceItems();
                        SetEngineInfo();
                        SetAlternatorInfo();
                        SetGimbalInfo();
                        SetRcsInfo();
                        SetParachuteInfo();
                        SetSasInfo();
                        SetReactionWheelInfo();
                        SetSolarPanelInfo();
                        SetGeneratorInfo();
                        SetDecouplerInfo();
                        SetTransmitterInfo();
                        SetScienceExperimentInfo();
                        SetScienceContainerInfo();
                        SetSingleActivationInfo();
                    }
                }
                else
                {
                    selectedPart = null;
                }
            }
            catch (Exception ex)
            {
                MyLogger.Exception(ex);
            }
        }

        private bool IsPointerOverStaging()
        {
            if (stageUiPointerHoverDetector == null)
            {
                stageUiPointerHoverDetector = StageManager.Instance.scrollRect.gameObject.AddOrGetComponent<PointerHoverDetector>();
            }

            if (stageUiPointerHoverDetector != null)
            {
                return stageUiPointerHoverDetector.IsPointerHovering;
            }

            return false;
        }

        private void ResetInfo()
        {
            showInfo = !clickToOpen;
            skipFrame = true;
            position.width = namesOnly || clickToOpen ? 0.0f : 200.0f;
            position.height = 0.0f;
        }

        private void SetAlternatorInfo()
        {
            ModuleAlternator moduleAlternator = PartExtensions.GetModule<ModuleAlternator>(selectedPart);
            if (moduleAlternator != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_Alternator", "Alternator")));
                for (int i = 0; i < moduleAlternator.resHandler.outputResources.Count; ++i)
                {
                    var moduleResource = moduleAlternator.resHandler.outputResources[i];
                    infoItems.Add(PartInfoItem.Create("\t" + GetResourceDisplayName(moduleResource.name), moduleResource.rate.ToRate()));
                }
            }
        }

        private void SetCostInfo()
        {
            infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_Cost", "COST"), Units.ConcatF(PartExtensions.GetCostDry(selectedPart), PartExtensions.GetCostWet(selectedPart))));
        }

        private void SetDecouplerInfo()
        {
            var protoModuleDecoupler = PartExtensions.GetProtoModuleDecoupler(selectedPart);
            if (protoModuleDecoupler != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_EjectionForce", "Ejection Force"), protoModuleDecoupler.EjectionForce.ToForce()));
                if (protoModuleDecoupler.IsOmniDecoupler)
                {
                    infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_OmniDirectional", "Omni-directional")));
                }
            }
        }

        private void SetEngineInfo()
        {
            var protoModuleEngine = PartExtensions.GetProtoModuleEngine(selectedPart);
            if (protoModuleEngine != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_Thrust", "THRUST"), Units.ToForce(protoModuleEngine.MinimumThrust, protoModuleEngine.MaximumThrust)));
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_Isp", "ISP"), Units.ConcatF(protoModuleEngine.GetSpecificImpulse(1.0f), protoModuleEngine.GetSpecificImpulse(0.0f)) + "s"));
                if (protoModuleEngine.Propellants.Count > 0)
                {
                    infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_Propellants", "Propellants")));

                    float totalRatio = 0.0f;
                    for (int i = 0; i < protoModuleEngine.Propellants.Count; ++i)
                    {
                        totalRatio = totalRatio + protoModuleEngine.Propellants[i].ratio;
                    }

                    for (int i = 0; i < protoModuleEngine.Propellants.Count; ++i)
                    {
                        var propellant = protoModuleEngine.Propellants[i];
                        infoItems.Add(PartInfoItem.Create("\t" + GetResourceDisplayName(propellant.name), (propellant.ratio / totalRatio).ToPercent()));
                    }
                }
            }
        }

        private void SetGeneratorInfo()
        {
            var moduleGenerator = PartExtensions.GetModule<ModuleGenerator>(selectedPart);
            if (moduleGenerator != null)
            {
                if (moduleGenerator.resHandler.inputResources.Count > 0)
                {
                    infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_GeneratorInput", "Generator Input")));
                    for (int i = 0; i < moduleGenerator.resHandler.inputResources.Count; ++i)
                    {
                        var generatorResource = moduleGenerator.resHandler.inputResources[i];
                        infoItems.Add(PartInfoItem.Create("\t" + GetResourceDisplayName(generatorResource.name), generatorResource.rate.ToRate()));
                    }
                }

                if (moduleGenerator.resHandler.outputResources.Count > 0)
                {
                    infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_GeneratorOutput", "Generator Output")));
                    for (int i = 0; i < moduleGenerator.resHandler.outputResources.Count; ++i)
                    {
                        var generatorResource = moduleGenerator.resHandler.outputResources[i];
                        infoItems.Add(PartInfoItem.Create("\t" + GetResourceDisplayName(generatorResource.name), generatorResource.rate.ToRate()));
                    }
                }

                if (moduleGenerator.isAlwaysActive)
                {
                    infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_GeneratorAlwaysActive", "Generator is Always Active")));
                }
            }
        }

        private void SetGimbalInfo()
        {
            var moduleGimbal = PartExtensions.GetModule<ModuleGimbal>(selectedPart);
            if (moduleGimbal != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_ThrustVectoring", "Thrust Vectoring"), moduleGimbal.gimbalRange.ToString("F2")));
            }
        }

        private void SetMassItems()
        {
            if (selectedPart.physicalSignificance == Part.PhysicalSignificance.FULL)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_Mass", "MASS"), Units.ToMass(PartExtensions.GetDryMass(selectedPart), PartExtensions.GetWetMass(selectedPart))));
            }
        }

        private void SetParachuteInfo()
        {
            var moduleParachute = PartExtensions.GetModule<ModuleParachute>(selectedPart);
            if (moduleParachute != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_DeployedDrag", "Deployed Drag"), Units.ConcatF(moduleParachute.semiDeployedDrag, moduleParachute.fullyDeployedDrag)));
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_DeploymentAltitude", "Deployment Altitude"), moduleParachute.deployAltitude.ToDistance()));
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_DeploymentPressure", "Deployment Pressure"), moduleParachute.minAirPressureToOpen.ToString("F2")));
            }
        }

        private void SetRcsInfo()
        {
            var moduleRcs = PartExtensions.GetModule<ModuleRCS>(selectedPart);
            if (moduleRcs != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_ThrusterPower", "Thruster Power"), moduleRcs.thrusterPower.ToForce()));
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_SpecificImpulse", "Specific Impulse"), Units.ConcatF(moduleRcs.atmosphereCurve.Evaluate(1.0f), moduleRcs.atmosphereCurve.Evaluate(0.0f)) + "s"));
            }
        }

        private void SetReactionWheelInfo()
        {
            var moduleReactionWheel = PartExtensions.GetModule<ModuleReactionWheel>(selectedPart);
            if (moduleReactionWheel != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_ReactionWheelTorque", "Reaction Wheel Torque")));
                infoItems.Add(PartInfoItem.Create("\t" + Loc.Get("#KER_UI_PartInfo_Pitch", "Pitch"), moduleReactionWheel.PitchTorque.ToTorque()));
                infoItems.Add(PartInfoItem.Create("\t" + Loc.Get("#KER_UI_PartInfo_Roll", "Roll"), moduleReactionWheel.RollTorque.ToTorque()));
                infoItems.Add(PartInfoItem.Create("\t" + Loc.Get("#KER_UI_PartInfo_Yaw", "Yaw"), moduleReactionWheel.YawTorque.ToTorque()));
                for (int i = 0; i < moduleReactionWheel.resHandler.inputResources.Count; ++i)
                {
                    var moduleResource = moduleReactionWheel.resHandler.inputResources[i];
                    infoItems.Add(PartInfoItem.Create("\t" + GetResourceDisplayName(moduleResource.name), moduleResource.rate.ToRate()));
                }
            }
        }

        private void SetResourceItems()
        {
            bool visibleResources = false;
            for (int i = 0; i < selectedPart.Resources.dict.Count; ++i)
            {
                if (selectedPart.Resources.dict.At(i).hideFlow == false)
                {
                    visibleResources = true;
                    break;
                }
            }

            if (visibleResources)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_Resources", "RESOURCES")));
                for (int i = 0; i < selectedPart.Resources.dict.Count; ++i)
                {
                    var partResource = selectedPart.Resources.dict.At(i);

                    if (partResource.hideFlow == false)
                    {
                        infoItems.Add(PartResourceExtensions.GetDensity(partResource) > 0
                                          ? PartInfoItem.Create("\t" + Loc.ResourceName(partResource.info.name, partResource.info.displayName), "(" + PartResourceExtensions.GetMass(partResource).ToMass() + ") " + partResource.amount.ToString("F1"))
                                          : PartInfoItem.Create("\t" + Loc.ResourceName(partResource.info.name, partResource.info.displayName), partResource.amount.ToString("F1")));
                    }
                }
            }
        }

        private void SetSasInfo()
        {
            if (PartExtensions.HasModule<ModuleSAS>(selectedPart))
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_SasEquipped", "SAS Equipped")));
            }
        }

        private void SetScienceContainerInfo()
        {
            if (PartExtensions.HasModule<ModuleScienceContainer>(selectedPart))
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_ScienceContainer", "Science Container")));
            }
        }

        private void SetScienceExperimentInfo()
        {
            var moduleScienceExperiment = PartExtensions.GetModule<ModuleScienceExperiment>(selectedPart);
            if (moduleScienceExperiment != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_ScienceExperiment", "Science Experiment"), moduleScienceExperiment.experimentActionName));
                infoItems.Add(PartInfoItem.Create("\t" + Loc.Get("#KER_UI_PartInfo_TransmitEfficiency", "Transmit Efficiency"), moduleScienceExperiment.xmitDataScalar.ToPercent()));
                if (moduleScienceExperiment.rerunnable == false)
                {
                    infoItems.Add(PartInfoItem.Create("\t" + Loc.Get("#KER_UI_PartInfo_SingleUsage", "Single Usage")));
                }
            }
        }

        private void SetSingleActivationInfo()
        {
            if (PartExtensions.HasModule<ModuleAnimateGeneric>(selectedPart, m => m.isOneShot))
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_SingleActivation", "Single Activation")));
            }
        }

        private void SetSolarPanelInfo()
        {
            var moduleDeployableSolarPanel = PartExtensions.GetModule<ModuleDeployableSolarPanel>(selectedPart);
            if (moduleDeployableSolarPanel != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_ChargeRate", "Charge Rate"), moduleDeployableSolarPanel.chargeRate.ToRate()));
                if (moduleDeployableSolarPanel.isBreakable)
                {
                    infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_Breakable", "Breakable")));
                }

                if (moduleDeployableSolarPanel.trackingBody == Sun.Instance)
                {
                    infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_SunTracking", "Sun Tracking")));
                }
            }
        }

        private void SetTransmitterInfo()
        {
            var moduleDataTransmitter = PartExtensions.GetModule<ModuleDataTransmitter>(selectedPart);
            if (moduleDataTransmitter != null)
            {
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_PacketSize", "Packet Size"), moduleDataTransmitter.packetSize.ToString("F2") + " Mits"));
                infoItems.Add(PartInfoItem.Create(Loc.Get("#KER_UI_PartInfo_Bandwidth", "Bandwidth"), (moduleDataTransmitter.packetInterval * moduleDataTransmitter.packetSize).ToString("F2") + "Mits/sec"));

                // TODO: allow for multiple consumed resources
                infoItems.Add(PartInfoItem.Create(GetResourceDisplayName(moduleDataTransmitter.GetConsumedResources()[0].name), moduleDataTransmitter.packetResourceCost.ToString("F2") + Loc.Get("#KER_UI_PartInfo_PerPacket", "/Packet")));
            }
        }

        private static string GetResourceDisplayName(string resourceName)
        {
            PartResourceDefinition definition = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return Loc.ResourceName(resourceName, definition == null ? resourceName : definition.displayName);
        }

        private void Window(int windowId)
        {
            try
            {
                GUILayout.Label(selectedPart.partInfo.title, BuildOverlay.TitleStyle);
                if (showInfo)
                {
                    for (int i = 0; i < infoItems.Count; ++i)
                    {
                        var partInfoItem = infoItems[i];
                        GUILayout.Space(2.0f);
                        GUILayout.BeginHorizontal();
                        if (partInfoItem.Value != null)
                        {
                            GUILayout.Label(partInfoItem.Name + ":", BuildOverlay.NameStyle);
                            GUILayout.Space(25.0f);
                            GUILayout.Label(partInfoItem.Value, BuildOverlay.ValueStyle);
                        }
                        else
                        {
                            GUILayout.Label(partInfoItem.Name, BuildOverlay.NameStyle);
                        }

                        GUILayout.EndHorizontal();
                    }
                }
                else if (clickToOpen && namesOnly == false)
                {
                    GUILayout.Space(2.0f);
                    GUILayout.Label(Loc.Get("#KER_UI_ClickForMoreInfo", "Click [<<1>>] to show more info...", ToString(KeyBinder.PartInfoShowHide)), BuildOverlay.NameStyle);
                }
            }
            catch (Exception ex)
            {
                MyLogger.Exception(ex);
            }
        }

        private string ToString(KeyCode keyCode) {
            switch (keyCode) {
                case KeyCode.Mouse0: return "Left Mouse";
                case KeyCode.Mouse1: return "Right Mouse";
                case KeyCode.Mouse2: return "Middle Mouse";
                case KeyCode.Mouse3: return "Mouse Button 4";
                case KeyCode.Mouse4: return "Mouse Button 5";
                case KeyCode.Mouse5: return "Mouse Button 6";
                case KeyCode.Mouse6: return "Mouse Button 7";
            }

            return keyCode.ToString();
        }
    }
}
