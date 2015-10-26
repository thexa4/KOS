using kOS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace kOS.AddOns.MechJeb2
{
    /// <summary>
    /// Imported from MechJeb2
    /// </summary>
    public static class OrbitExtensions
    {
        //can probably be replaced with Vector3d.xzy?
        public static Vector3d SwapYZ(Vector3d v)
        {
            return v.xzy;
        }

        //
        // These "Swapped" functions translate preexisting Orbit class functions into world
        // space. For some reason, Orbit class functions seem to use a coordinate system
        // in which the Y and Z coordinates are swapped.
        //
        public static Vector3d SwappedOrbitalVelocityAtUT(this Orbit o, double UT)
        {
            return SwapYZ(o.getOrbitalVelocityAtUT(UT));
        }

        //position relative to the primary
        public static Vector3d SwappedRelativePositionAtUT(this Orbit o, double UT)
        {
            return SwapYZ(o.getRelativePositionAtUT(UT));
        }

        //position in world space
        public static Vector3d SwappedAbsolutePositionAtUT(this Orbit o, double UT)
        {
            return o.referenceBody.position + o.SwappedRelativePositionAtUT(UT);
        }

        //normalized vector perpendicular to the orbital plane
        //convention: as you look down along the orbit normal, the satellite revolves counterclockwise
        public static Vector3d SwappedOrbitNormal(this Orbit o)
        {
            return -SwapYZ(o.GetOrbitNormal()).normalized;
        }

        //normalized vector along the orbital velocity
        public static Vector3d Prograde(this Orbit o, double UT)
        {
            return o.SwappedOrbitalVelocityAtUT(UT).normalized;
        }

        //normalized vector pointing radially outward from the planet
        public static Vector3d Up(this Orbit o, double UT)
        {
            return o.SwappedRelativePositionAtUT(UT).normalized;
        }

        //normalized vector pointing radially outward and perpendicular to prograde
        public static Vector3d RadialPlus(this Orbit o, double UT)
        {
            return Vector3d.Exclude(o.Prograde(UT), o.Up(UT)).normalized;
        }

        //another name for the orbit normal; this form makes it look like the other directions
        public static Vector3d NormalPlus(this Orbit o, double UT)
        {
            return o.SwappedOrbitNormal();
        }

        //normalized vector parallel to the planet's surface, and pointing in the same general direction as the orbital velocity
        //(parallel to an ideally spherical planet's surface, anyway)
        public static Vector3d Horizontal(this Orbit o, double UT)
        {
            return Vector3d.Exclude(o.Up(UT), o.Prograde(UT)).normalized;
        }

        //normalized vector parallel to the planet's surface and pointing in the northward direction
        public static Vector3d North(this Orbit o, double UT)
        {
            return Vector3d.Exclude(o.Up(UT), (o.referenceBody.transform.up * (float)o.referenceBody.Radius) - o.SwappedRelativePositionAtUT(UT)).normalized;
        }

        //normalized vector parallel to the planet's surface and pointing in the eastward direction
        public static Vector3d East(this Orbit o, double UT)
        {
            return Vector3d.Cross(o.Up(UT), o.North(UT)); //I think this is the opposite of what it should be, but it gives the right answer
        }

        //distance from the center of the planet
        public static double Radius(this Orbit o, double UT)
        {
            return o.SwappedRelativePositionAtUT(UT).magnitude;
        }

        //mean motion is rate of increase of the mean anomaly
        public static double MeanMotion(this Orbit o)
        {
            if (o.eccentricity > 1)
            {
                return Math.Sqrt(o.referenceBody.gravParameter / Math.Abs(Math.Pow(o.semiMajorAxis, 3)));
            }
            else
            {
                // The above formula is wrong when using the RealSolarSystem mod, which messes with orbital periods.
                // This simpler formula should be foolproof for elliptical orbits:
                return 2 * Math.PI / o.period;
            }
        }

        //distance between two orbiting objects at a given time
        public static double Separation(this Orbit a, Orbit b, double UT)
        {
            return (a.SwappedAbsolutePositionAtUT(UT) - b.SwappedAbsolutePositionAtUT(UT)).magnitude;
        }

        //Gives the true anomaly (in a's orbit) at which a crosses its ascending node 
        //with b's orbit.
        //The returned value is always between 0 and 360.
        public static double AscendingNodeTrueAnomaly(this Orbit a, Orbit b)
        {
            Vector3d vectorToAN = Vector3d.Cross(a.SwappedOrbitNormal(), b.SwappedOrbitNormal());
            return a.TrueAnomalyFromVector(vectorToAN);
        }

        //Gives the true anomaly at which o crosses the equator going northwards, if o is east-moving,
        //or southwards, if o is west-moving.
        //The returned value is always between 0 and 360.
        public static double AscendingNodeEquatorialTrueAnomaly(this Orbit o)
        {
            Vector3d vectorToAN = Vector3d.Cross(o.referenceBody.transform.up, o.SwappedOrbitNormal());
            return o.TrueAnomalyFromVector(vectorToAN);
        }

        //For hyperbolic orbits, the true anomaly only takes on values in the range
        // -M < true anomaly < +M for some M. This function computes M.
        public static double MaximumTrueAnomaly(this Orbit o)
        {
            if (o.eccentricity < 1) return 180;
            else return 180 / Math.PI * Math.Acos(-1 / o.eccentricity);
        }

        //Returns the vector from the primary to the orbiting body at periapsis
        //Better than using Orbit.eccVec because that is zero for circular orbits
        public static Vector3d SwappedRelativePositionAtPeriapsis(this Orbit o)
        {
            Vector3d vectorToAN = Quaternion.AngleAxis(-(float)o.LAN, Planetarium.up) * Planetarium.right;
            Vector3d vectorToPe = Quaternion.AngleAxis((float)o.argumentOfPeriapsis, o.SwappedOrbitNormal()) * vectorToAN;
            return o.PeR * vectorToPe;
        }

        //Returns the vector from the primary to the orbiting body at apoapsis
        //Better than using -Orbit.eccVec because that is zero for circular orbits
        public static Vector3d SwappedRelativePositionAtApoapsis(this Orbit o)
        {
            Vector3d vectorToAN = Quaternion.AngleAxis(-(float)o.LAN, Planetarium.up) * Planetarium.right;
            Vector3d vectorToPe = Quaternion.AngleAxis((float)o.argumentOfPeriapsis, o.SwappedOrbitNormal()) * vectorToAN;
            Vector3d ret = -o.ApR * vectorToPe;
            if (double.IsNaN(ret.x))
            {
                Debug.LogError("OrbitExtensions.SwappedRelativePositionAtApoapsis got a NaN result!");
                Debug.LogError("o.LAN = " + o.LAN);
                Debug.LogError("o.inclination = " + o.inclination);
                Debug.LogError("o.argumentOfPeriapsis = " + o.argumentOfPeriapsis);
                Debug.LogError("o.SwappedOrbitNormal() = " + o.SwappedOrbitNormal());
            }
            return ret;
        }

        //Converts a direction, specified by a Vector3d, into a true anomaly.
        //The vector is projected into the orbital plane and then the true anomaly is
        //computed as the angle this vector makes with the vector pointing to the periapsis.
        //The returned value is always between 0 and 360.
        public static double TrueAnomalyFromVector(this Orbit o, Vector3d vec)
        {
            Vector3d oNormal = o.SwappedOrbitNormal();
            Vector3d projected = Vector3d.Exclude(oNormal, vec);
            Vector3d vectorToPe = o.SwappedRelativePositionAtPeriapsis();
            double angleFromPe = Vector3d.Angle(vectorToPe, projected);

            //If the vector points to the infalling part of the orbit then we need to do 360 minus the
            //angle from Pe to get the true anomaly. Test this by taking the the cross product of the
            //orbit normal and vector to the periapsis. This gives a vector that points to center of the 
            //outgoing side of the orbit. If vectorToAN is more than 90 degrees from this vector, it occurs
            //during the infalling part of the orbit.
            if (Math.Abs(Vector3d.Angle(projected, Vector3d.Cross(oNormal, vectorToPe))) < 90)
            {
                return angleFromPe;
            }
            else
            {
                return 360 - angleFromPe;
            }
        }

        //Computes the period of the phase angle between orbiting objects a and b.
        //This only really makes sense for approximately circular orbits in similar planes.
        //For noncircular orbits the time variation of the phase angle is only "quasiperiodic"
        //and for high eccentricities and/or large relative inclinations, the relative motion is
        //not really periodic at all.
        public static double SynodicPeriod(this Orbit a, Orbit b)
        {
            int sign = (Vector3d.Dot(a.SwappedOrbitNormal(), b.SwappedOrbitNormal()) > 0 ? 1 : -1); //detect relative retrograde motion
            return Math.Abs(1.0 / (1.0 / a.period - sign * 1.0 / b.period)); //period after which the phase angle repeats
        }

        //Computes the phase angle between two orbiting objects. 
        //This only makes sence if a.referenceBody == b.referenceBody.
        public static double PhaseAngle(this Orbit a, Orbit b, double UT)
        {
            Vector3d normalA = a.SwappedOrbitNormal();
            Vector3d posA = a.SwappedRelativePositionAtUT(UT);
            Vector3d projectedB = Vector3d.Exclude(normalA, b.SwappedRelativePositionAtUT(UT));
            double angle = Vector3d.Angle(posA, projectedB);
            if (Vector3d.Dot(Vector3d.Cross(normalA, posA), projectedB) < 0)
            {
                angle = 360 - angle;
            }
            return angle;
        }

        //Computes the angle between two orbital planes. This will be a number between 0 and 180
        //Note that in the convention used two objects orbiting in the same plane but in
        //opposite directions have a relative inclination of 180 degrees.
        public static double RelativeInclination(this Orbit a, Orbit b)
        {
            return Math.Abs(Vector3d.Angle(a.SwappedOrbitNormal(), b.SwappedOrbitNormal()));
        }

        public static Vector3d DeltaVToManeuverNodeCoordinates(this Orbit o, double UT, Vector3d dV)
        {
            return new Vector3d(Vector3d.Dot(o.RadialPlus(UT), dV),
                                Vector3d.Dot(-o.NormalPlus(UT), dV),
                                Vector3d.Dot(o.Prograde(UT), dV));
        }

        // Return the orbit of the parent body orbiting the sun
        public static Orbit TopParentOrbit(this Orbit orbit)
        {
            Orbit result = orbit;
            while (result.referenceBody != Planetarium.fetch.Sun)
            {
                result = result.referenceBody.orbit;
               
            }
            return result;
        }
    }
}
