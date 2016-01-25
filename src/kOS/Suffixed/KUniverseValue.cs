﻿using kOS.Safe.Compilation.KS;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Exceptions;
using kOS.Safe.Utilities;

namespace kOS.Suffixed
{
    public class KUniverseValue : Structure
    {
        private readonly SharedObjects shared;
        
        public KUniverseValue(SharedObjects shared)
        {
            this.shared = shared;
            InitializeSuffixes();
        }
        
        public void InitializeSuffixes()
        {
            AddSuffix("CANREVERT", new Suffix<bool>(CanRevert));
            AddSuffix("CANREVERTTOLAUNCH", new Suffix<bool>(CanRevertToLaunch));
            AddSuffix("CANREVERTTOEDITOR", new Suffix<bool>(CanRevvertToEditor));
            AddSuffix("REVERTTOLAUNCH", new NoArgsSuffix(RevertToLaunch));
            AddSuffix("REVERTTOEDITOR", new NoArgsSuffix(RevertToEditor));
            AddSuffix("REVERTTO", new OneArgsSuffix<string>(RevertTo));
            AddSuffix("ORIGINEDITOR", new Suffix<string>(OriginatingEditor));
            AddSuffix("DEFAULTLOADDISTANCE", new Suffix<LoadDistanceValue>(() => new LoadDistanceValue(PhysicsGlobals.Instance.VesselRangesDefault)));
            AddSuffix("ACTIVEVESSEL", new SetSuffix<VesselTarget>(() => new VesselTarget(FlightGlobals.ActiveVessel, shared), SetActiveVessel));
            AddSuffix("FORCESETACTIVEVESSEL", new OneArgsSuffix<VesselTarget>(ForceSetActiveVessel));
            AddSuffix("HOURSPERDAY", new Suffix<int>(GetHoursPerDay));
            AddSuffix("DEBUGLOG", new OneArgsSuffix<string>(DebugLog));
        }

        public void RevertToLaunch()
        {
            if (CanRevertToLaunch())
            {
                FlightDriver.RevertToLaunch();
            }
            else throw new KOSCommandInvalidHereException(LineCol.Unknown(), "REVERTTOLAUNCH", "When revert is disabled", "When revert is enabled");
        }

        public void RevertToEditor()
        {
            if (CanRevvertToEditor())
            {
                EditorFacility fac = ShipConstruction.ShipType;
                FlightDriver.RevertToPrelaunch(fac);
            }
            else throw new KOSCommandInvalidHereException(LineCol.Unknown(), "REVERTTOEDITOR", "When revert is disabled", "When revert is enabled");
        }

        public void RevertTo(string editor)
        {
            if (CanRevvertToEditor())
            {
                EditorFacility fac;
                switch (editor.ToUpper())
                {
                    case "VAB":
                        fac = EditorFacility.VAB;
                        break;
                    case "SPH":
                        fac = EditorFacility.SPH;
                        break;
                    default:
                        fac = EditorFacility.None;
                        break;
                }
                FlightDriver.RevertToPrelaunch(fac);
            }
            else throw new KOSCommandInvalidHereException(LineCol.Unknown(), "REVERTTO", "When revert is disabled", "When revert is enabled");
        }

        public bool CanRevert()
        {
            return FlightDriver.CanRevert;
        }

        public bool CanRevertToLaunch()
        {
            return FlightDriver.CanRevertToPostInit && HighLogic.CurrentGame.Parameters.Flight.CanRestart;
        }

        public bool CanRevvertToEditor()
        {
            return FlightDriver.CanRevertToPrelaunch &&
                HighLogic.CurrentGame.Parameters.Flight.CanLeaveToEditor &&
                ShipConstruction.ShipConfig != null;
        }

        public string OriginatingEditor()
        {
            if (ShipConstruction.ShipConfig != null)
            {
                EditorFacility fac = ShipConstruction.ShipType;
                return fac.ToString().ToUpper();
            }
            return "";
        }

        public void SetActiveVessel(VesselTarget vesselTarget)
        {
            Vessel vessel = vesselTarget.Vessel;
            if (!vessel.isActiveVessel)
            {
                FlightGlobals.SetActiveVessel(vessel);
            }
        }

        public void ForceSetActiveVessel(VesselTarget vesselTarget)
        {
            Vessel vessel = vesselTarget.Vessel;
            if (!vessel.isActiveVessel)
            {
                FlightGlobals.ForceSetActiveVessel(vessel);
            }
        }
        
        public int GetHoursPerDay()
        {
            return GameSettings.KERBIN_TIME ? TimeSpan.HOURS_IN_KERBIN_DAY : TimeSpan.HOURS_IN_EARTH_DAY;
        }
        
        public void DebugLog(string message)
        {
            SafeHouse.Logger.Log("(KUNIVERSE:DEBUGLOG) " + message);
        }
    }
}
