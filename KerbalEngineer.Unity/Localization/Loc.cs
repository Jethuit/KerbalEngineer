//
//     Kerbal Engineer Redux
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//

namespace KerbalEngineer.Unity.Localization
{
    using System;
    using KSP.Localization;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    ///     Centralised access to KSP localisation with a safe English fallback.
    /// </summary>
    public static class Loc
    {
        /// <summary>
        ///     Gets whether the active catalogue requests the lighter CJK text style.
        /// </summary>
        public static bool UseNormalFont
        {
            get
            {
                try
                {
                    if (string.Equals(Localizer.CurrentLanguage, "zh-cn", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    // Fall back to the catalogue flag while the localizer is starting.
                }

                return string.Equals(Get("#KER_UI_UseNormalFont", "0"), "1", StringComparison.Ordinal);
            }
        }

        public static string Get(string tag, string englishFallback, params object[] args)
        {
            string value = null;

            try
            {
                value = Localizer.GetStringByTag(tag);
            }
            catch (Exception)
            {
                // The localisation database may not be ready during very early startup.
            }

            if (string.IsNullOrEmpty(value) || value == tag)
            {
                value = englishFallback;
            }

            if (args == null || args.Length == 0)
            {
                return value;
            }

            try
            {
                return Localizer.Format(value, args);
            }
            catch (Exception)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string replacement = Convert.ToString(args[i]);
                    value = value.Replace("<<" + (i + 1) + ">>", replacement);
                    value = value.Replace("{" + i + "}", replacement);
                }

                return value;
            }
        }

        /// <summary>
        ///     Avoids simulated bold rendering for catalogues whose glyphs become too heavy.
        /// </summary>
        public static FontStyle FontStyleFor(FontStyle preferred)
        {
            return UseNormalFont && preferred == FontStyle.Bold ? FontStyle.Normal : preferred;
        }

        /// <summary>
        ///     Applies rich-text emphasis unless the active catalogue requests normal-weight text.
        /// </summary>
        public static string Emphasise(string value)
        {
            return UseNormalFont ? value : "<b>" + value + "</b>";
        }

        /// <summary>
        ///     Gets a localised resource name while keeping the resource's internal name stable.
        /// </summary>
        public static string ResourceName(string internalName, string configuredDisplayName = null)
        {
            string fallback = ResolveConfiguredName(configuredDisplayName, internalName);
            string tag = null;

            switch (internalName)
            {
                case "Oxygen": tag = "#KER_Resource_Oxygen"; break;
                case "Food": tag = "#KER_Resource_Food"; break;
                case "Water": tag = "#KER_Resource_Water"; break;
                case "CarbonDioxide": tag = "#KER_Resource_CarbonDioxide"; break;
                case "Waste": tag = "#KER_Resource_Waste"; break;
                case "WasteWater": tag = "#KER_Resource_WasteWater"; break;
                case "LithiumHydroxide": tag = "#KER_Resource_LithiumHydroxide"; break;
                case "ElectricCharge": tag = "#KER_Resource_ElectricCharge"; break;
            }

            return tag == null ? fallback : Get(tag, fallback);
        }

        /// <summary>
        ///     Removes a trailing English alias from a localised body name, for example "地球 (Earth)".
        /// </summary>
        public static string BodyName(string displayName)
        {
            if (!UseNormalFont || string.IsNullOrEmpty(displayName) || !displayName.EndsWith(")", StringComparison.Ordinal))
            {
                return displayName;
            }

            int open = displayName.LastIndexOf(" (", StringComparison.Ordinal);
            if (open <= 0)
            {
                return displayName;
            }

            for (int i = open + 2; i < displayName.Length - 1; i++)
            {
                char character = displayName[i];
                if (character > 127)
                {
                    return displayName;
                }
            }

            return displayName.Substring(0, open);
        }

        /// <summary>
        ///     Localises immutable labels embedded in the shipped AssetBundle.
        /// </summary>
        public static void Apply(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Text[] labels = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                Text label = labels[i];
                if (label == null)
                {
                    continue;
                }

                label.fontStyle = FontStyleFor(label.fontStyle);

                switch (label.text)
                {
                    case "CONTROL BAR":
                        label.text = Get("#KER_UI_FlightMenu_ControlBar", "CONTROL BAR");
                        break;
                    case "SHOW ENGINEER":
                        label.text = Get("#KER_UI_FlightMenu_ShowEngineer", "SHOW ENGINEER");
                        break;
                    case "EDIT":
                        label.text = Get("#KER_UI_Edit", "EDIT");
                        break;
                    case "Close":
                    case "CLOSE":
                        label.text = Get("#KER_UI_Close", label.text);
                        break;
                    case "SETTINGS":
                        label.text = Get("#KER_UI_Settings", "SETTINGS");
                        break;
                    case "SECTION":
                        label.text = Get("#KER_UI_Section", "SECTION");
                        break;
                    case "ENGINEER":
                        label.text = Get("#KER_UI_Engineer", "ENGINEER");
                        break;
                    case "NEW CUSTOM SECTION":
                        label.text = Get("#KER_UI_NewCustomSection", "NEW CUSTOM SECTION");
                        break;
                }
            }
        }

        private static string ResolveConfiguredName(string configuredDisplayName, string fallback)
        {
            if (string.IsNullOrEmpty(configuredDisplayName))
            {
                return fallback;
            }

            if (configuredDisplayName[0] == '#')
            {
                return Get(configuredDisplayName, fallback);
            }

            return configuredDisplayName;
        }
    }
}
