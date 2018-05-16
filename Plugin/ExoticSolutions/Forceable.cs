using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class Forceable : IEquatable<Forceable>
    {
        private enum ForceableType
        {
            part = 0,
            vessel = 1,
            celestialBody = 2
        }

        ForceableType forceableType;
        object forceTarget;

        public void wrap(Vessel vessel)
        {
            forceableType = ForceableType.vessel;
            forceTarget = vessel;
        }

        public void wrap(Part part)
        {
            forceableType = ForceableType.part;
            forceTarget = part;
        }

        public void wrap()
        {
            forceableType = ForceableType.celestialBody;
            forceTarget = null;
        }

        public Forceable(Vessel vessel)
        {
            wrap(vessel);
        }

        public Forceable(Part part)
        {
            wrap(part);
        }

        public Forceable()
        {
            wrap();
        }

        public void addForce(Vector3 force, ForceMode mode)
        {
            if(forceableType == ForceableType.part)
            {
                ((Part)forceTarget).Rigidbody.AddForce(force, mode);
            }
            else if(forceableType == ForceableType.vessel)
            {
                ((Vessel)forceTarget).addForce(force, mode);
            }
        }

        public float distanceFrom(Forceable target)
        {
            if(forceableType == ForceableType.part)
            {
                if(target.forceableType == ForceableType.part)
                {
                    return (((Part)forceTarget).transform.position - ((Part)target.forceTarget).transform.position).magnitude;
                }
                else if(target.forceableType == ForceableType.vessel)
                {
                    return (((Part)forceTarget).transform.position - ((Vessel)target.forceTarget).transform.position).magnitude;
                }
                else
                {
                    return (float)((Part)forceTarget).vessel.radarAltitude;
                }
            }
            else if(forceableType == ForceableType.vessel)
            {
                if (target.forceableType == ForceableType.part)
                {
                    return (((Vessel)forceTarget).transform.position - ((Part)target.forceTarget).transform.position).magnitude;
                }
                else if (target.forceableType == ForceableType.vessel)
                {
                    return (((Vessel)forceTarget).transform.position - ((Vessel)target.forceTarget).transform.position).magnitude;
                }
                else
                {
                    return (float)((Vessel)forceTarget).radarAltitude;
                }
            }
            else
            {
                if (target.forceableType == ForceableType.part)
                {
                    return (float)((Part)target.forceTarget).vessel.radarAltitude;
                }
                else if (target.forceableType == ForceableType.vessel)
                {
                    return (float)((Vessel)target.forceTarget).radarAltitude;
                }
                else
                {
                    return 0f;
                }
            }
        }

        public float distanceFrom(Part part)
        {
            if (forceableType == ForceableType.part)
            {
                return (((Part)forceTarget).transform.position - part.transform.position).magnitude;
            }
            else if (forceableType == ForceableType.vessel)
            {
                return (((Vessel)forceTarget).transform.position - part.transform.position).magnitude;
            }
            else
            {
                return (float)part.vessel.radarAltitude;
            }
        }

        public bool Equals(Forceable other)
        {
            if (forceTarget == other.forceTarget) return true;
            return false;
        }

        public Vector3 getSrfVelocity()
        {
            if (forceableType == ForceableType.part)
            {
                return ((Part)forceTarget).vessel.srf_velocity;
            }
            else if (forceableType == ForceableType.vessel)
            {
                return ((Vessel)forceTarget).srf_velocity;
            }
            else
            {
                return new Vector3(0, 0, 0);
            }
        }

        public Vector3 getObtVelocity()
        {
            if (forceableType == ForceableType.part)
            {
                return ((Part)forceTarget).vessel.obt_velocity;
            }
            else if (forceableType == ForceableType.vessel)
            {
                return ((Vessel)forceTarget).obt_velocity;
            }
            else
            {
                return new Vector3(0, 0, 0);
            }
        }

        public Vector3 getGraviticAcceleration()
        {
            if (forceableType == ForceableType.part)
            {
                return ((Part)forceTarget).vessel.graviticAcceleration;
            }
            else if (forceableType == ForceableType.vessel)
            {
                return ((Vessel)forceTarget).graviticAcceleration;
            }
            else
            {
                return new Vector3(0, 0, 0);
            }
        }

        public Vector3 getCentrifugalAcceleration()
        {
            if (forceableType == ForceableType.part)
            {
                return FlightGlobals.getCentrifugalAcc(((Part)forceTarget).transform.position, ((Part)forceTarget).vessel.mainBody);
            }
            else if (forceableType == ForceableType.vessel)
            {
                return FlightGlobals.getCentrifugalAcc(((Vessel)forceTarget).transform.position, ((Vessel)forceTarget).mainBody);
            }
            else
            {
                return new Vector3(0, 0, 0);
            }
        }

        public double getVesselMass()
        {
            if (forceableType == ForceableType.part)
            {
                return ((Part)forceTarget).vessel.totalMass;
            }
            else if (forceableType == ForceableType.vessel)
            {
                return ((Vessel)forceTarget).totalMass;
            }
            else
            {
                return 0;
            }
        }
    }
}
