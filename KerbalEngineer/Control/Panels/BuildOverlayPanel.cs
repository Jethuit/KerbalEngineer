// 
//     Kerbal Engineer Redux
// 
//     Copyright (C) 2014 CYBUTEK
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
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#region Using Directives

using KerbalEngineer.Editor;
using KerbalEngineer.Unity.Localization;

using UnityEngine;

#endregion

namespace KerbalEngineer.Control.Panels
{
    public class BuildOverlayPanel : IControlPanel
    {
        #region Properties

        public string Name
        {
            get { return Loc.Get("#KER_UI_BuildOverlay", "Build Overlay"); }
        }

        #endregion

        #region Methods: public

        public void Draw()
        {
            GUILayout.Label(Loc.Get("#KER_UI_BuildOverlay", "Build Overlay"), ControlCentre.Title);
            DrawPartInfo();
            GUILayout.Space(10.0f);
            DrawDisplays();
        }

        #endregion

        #region Methods: private

        private static void DrawPartInfo()
        {
            GUILayout.Label(Loc.Get("#KER_UI_PartInformation", "Part Information (Hover Tooltips)"), ControlCentre.Label);
            GUILayout.BeginHorizontal();
            BuildOverlayPartInfo.Visible = GUILayout.Toggle(BuildOverlayPartInfo.Visible, Loc.Get("#KER_UI_Visible", "Visible"), ControlCentre.Button, GUILayout.Width(150.0f));
            if (BuildOverlayPartInfo.Visible)
            {
                BuildOverlayPartInfo.NamesOnly = GUILayout.Toggle(BuildOverlayPartInfo.NamesOnly, Loc.Get("#KER_UI_ShowNamesOnly", "Show Names Only"), ControlCentre.Button, GUILayout.Width(150.0f));
                if (!BuildOverlayPartInfo.NamesOnly)
                {
                    BuildOverlayPartInfo.ClickToOpen = GUILayout.Toggle(BuildOverlayPartInfo.ClickToOpen, Loc.Get("#KER_UI_ClickToOpen", "Click To Open"), ControlCentre.Button, GUILayout.Width(150.0f));
                }
            }
            
            
            GUILayout.EndHorizontal();
        }

        private static void DrawDisplays()
        {
            GUILayout.Label(Loc.Get("#KER_UI_InformationalDisplays", "Informational Displays"), ControlCentre.Label);
            GUILayout.BeginHorizontal();
            BuildOverlayVessel.Visible = GUILayout.Toggle(BuildOverlayVessel.Visible, Loc.Get("#KER_UI_VesselDetails", "Vessel Details"), ControlCentre.Button, GUILayout.Width(150.0f));
            BuildOverlayResources.Visible = GUILayout.Toggle(BuildOverlayResources.Visible, Loc.Get("#KER_UI_ResourcesList", "Resources List"), ControlCentre.Button, GUILayout.Width(150.0f));
            GUILayout.EndHorizontal();
        }

        #endregion
    }
}
