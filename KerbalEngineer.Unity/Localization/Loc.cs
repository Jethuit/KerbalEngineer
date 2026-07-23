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
    }
}
