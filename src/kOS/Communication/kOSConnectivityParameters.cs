using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace kOS.Communication
{
    public class kOSConnectivityParameters : GameParameters.CustomParameterNode
    {
        private static kOSConnectivityParameters instance;
        public static kOSConnectivityParameters Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = HighLogic.CurrentGame.Parameters.CustomParams<kOSConnectivityParameters>();
                }
                return instance;
            }
        }

        public const string SELECT_DIALOG_TEXT = "kOS has detected that there are one or more new connectivity " +
            "managers available, or that your selected manager is no longer available.  A connectivity manager " +
            "determines how kOS will interact with a communications network with regard to both inter-vessel and " +
            "vessel-kerbin communication and file transfer.  Please select a manager to use.";

        private List<string> availableConnectivityManagers;

        [GameParameters.CustomIntParameterUI("")]
        public int version = 0;

        [GameParameters.CustomStringParameterUI("")]
        public string knownHandlerList = "PermitAllConnectivityManager";

        [GameParameters.CustomStringParameterUI("CONNECTIVITY MANAGER", autoPersistance = false)]
        public string connectivityHandlerTitle = "";

        [GameParameters.CustomParameterUI("Selected", toolTip = "The currently slected manager.  The slider will progress\n" + 
                                                                "through the list of available managers below, in order.\n" + 
                                                                "The manager you select will determine how kOS will interact\n" + 
                                                                "with stock or mod communications networks.  See the communications\n" + 
                                                                "documentation for details on each manager.")]
        public string connectivityHandler = "StockConnectivityManager";

        [GameParameters.CustomStringParameterUI("Available managers", autoPersistance = false)]
        public string connectivityHandlerList = "connectivityHandlerList";

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false;
            }
        }

        public override string DisplaySection { get { return "kOS"; } }

        public override string Section
        {
            get
            {
                return "kOS";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 1;
            }
        }

        public override string Title
        {
            get
            {
                return "Connectivity";
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            Safe.Utilities.SafeHouse.Logger.SuperVerbose("kOSConnectivityParameters.OnLoad()");
            base.OnLoad(node);
            instance = null;
            if (HighLogic.CurrentGame != null || HighLogic.LoadedSceneIsGame)
            {
                instance = this;
            }
        }

        public override IList ValidValues(MemberInfo member)
        {
            if (member.Name == "connectivityHandler")
            {
                if (availableConnectivityManagers == null)
                {
                    availableConnectivityManagers = ConnectivityManager.GetStringList();
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    for (int i = 0; i < availableConnectivityManagers.Count; ++i)
                    {
                        sb.AppendLine(" * " + availableConnectivityManagers[i]);
                    }
                    connectivityHandlerList = sb.ToString();
                }
                return availableConnectivityManagers;
            }
            else if (member.Name == "connectivityHandlerList")
            {
                if (availableConnectivityManagers == null)
                {
                    availableConnectivityManagers = ConnectivityManager.GetStringList();
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < availableConnectivityManagers.Count; ++i)
                    {
                        sb.AppendLine(" * " + availableConnectivityManagers[i]);
                    }
                    connectivityHandlerList = sb.ToString();
                }
            }
            return null;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "connectivityHandler" && HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                return false;
            }
            return base.Interactible(member, parameters);
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "version" || member.Name == "knownHandlerList")
            {
                return false;
            }
            if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                if (member.Name == "connectivityHandlerTitle")
                {
                    connectivityHandlerTitle = "You will be prompted to select a handler when the game loads.";
                }
                else
                {
                    return false;
                }
            }
            return base.Enabled(member, parameters);
        }

        public void CheckNewManagers()
        {
            Safe.Utilities.SafeHouse.Logger.SuperVerbose("kOSConnectivityParameters.CheckNewManagers()");
            var availableConnectivityManagers = ConnectivityManager.GetStringHash();
            var knownHandlers = knownHandlerList.Split(',').ToList();
            if (!availableConnectivityManagers.Contains(connectivityHandler) || !availableConnectivityManagers.IsSubsetOf(knownHandlers))
            {
                List<DialogGUIBase> options = new List<DialogGUIBase>();
                foreach (var name in availableConnectivityManagers)
                {
                    string text;
                    if (name.Equals(connectivityHandler))
                    {
                        text = connectivityHandler + " (Selected)";
                    }
                    else
                    {
                        text = name;
                    }
                    options.Add(new DialogGUIButton(text, () => { connectivityHandler = name; }, true));
                }
                Module.kOSSettingsChecker.QueueDialog(
                    // Anchor positions represents the position in the dialog that will be anchored
                    // The range is 0.0f-1.0f and for example (0.5f, 1.0f) represents the dialog
                    // being anchored in the middle (x-direction) and at the top of the dialog (y-direction).
                    // Whereas (0.5f, 0.0f) would do the same, but at the bottom of the dialog (y-direction).

                    // The center position configured for the Dialog (first two parameters Rect) represents
                    // the location on screen where the anchored position will appear.
                    // The range is 0.0f-1.0f with (0.0f, 0.0f) representing lower left corner.

                    // The part which is really hard to explain, other than emperically, is that
                    // the anchor position and the center position on screen need to be the
                    // same to produce a UI location that is mostly invariant for UI scaling.
                    // For example, using an anchor of (0.5f, 0.5f) and center position (0.5, 1.0f)
                    // correctly results in a dialog at the top of the screen, with the center
                    // of the dialog at the edge of the screen. Decreasing the UI scaling to
                    // 80% moves the dialog towards the center of the screen, while 120% moves
                    // the dialog entirely offscreen.

                    // In x-direction the window is positioned at the center of screen with 0.5f
                    // In y-direction the window is positioned below center of screen with 0.1f
                    // This to prevent overlap with RemoteTech window (see #2456) also at a
                    // resolution of (considering that RemoteTech doesn't apply UI scaling):
                    // * 1920x1080 using 100% UI scaling
                    // * 3840x2160 using 200% UI scaling
                    // Previous values of (1.5f, 1.5f) for the anchor only give undefined behavior
                    // causing the window to appear offscreen at higher scale factors such as 180%
                    0.5f, 0.1f,
                    new MultiOptionDialog(
                        "Select Dialog",
                        SELECT_DIALOG_TEXT,
                        "kOS",
                        HighLogic.UISkin,
                        // when using Rect width must be specified, height is automatically determined
                        new Rect(0.5f, 0.1f, 300.0f, 0.0f),
                        options.ToArray()));
            }
            availableConnectivityManagers.UnionWith(knownHandlers);
            knownHandlerList = string.Join(",", availableConnectivityManagers.ToArray());
        }
    }
}
