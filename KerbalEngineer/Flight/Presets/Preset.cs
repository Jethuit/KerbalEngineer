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

using System.Linq;
using System.Text.RegularExpressions;
using KerbalEngineer.Unity.Localization;

#endregion

namespace KerbalEngineer.Flight.Presets
{
    public class Preset
    {
        #region Properties

        public string Abbreviation { get; set; }

        public string FileName
        {
            get { return Regex.Replace(this.Name, @"[^\d\w]", string.Empty) + ".xml"; }
        }

        public bool IsHud { get; set; }

        public bool IsHudBackground { get; set; }

        public string Name { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public string DisplayName
        {
            get
            {
                string canonicalName;
                switch (this.Abbreviation)
                {
                    case "ORBT": canonicalName = "ORBITAL"; break;
                    case "SURF": canonicalName = "SURFACE"; break;
                    case "VESL": canonicalName = "VESSEL"; break;
                    case "RDZV": canonicalName = "RENDEZVOUS"; break;
                    case "BURN": canonicalName = "MANEUVER"; break;
                    case "LAND": canonicalName = "LANDING"; break;
                    case "HUD 1": canonicalName = "HUD 1"; break;
                    case "HUD 2": canonicalName = "HUD 2"; break;
                    default: return this.Name;
                }

                if (!string.Equals(this.Name, canonicalName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return this.Name;
                }

                return Loc.Get("#KER_Section_" + this.Abbreviation.Replace(" ", string.Empty) + "_Name", this.Name);
            }
        }

        public string[] ReadoutNames { get; set; }

        #endregion

        #region Methods: public

        public override string ToString()
        {
            return this.Name + this.ReadoutNames.Select(r => "\n\t" + r);
        }

        #endregion
    }
}
