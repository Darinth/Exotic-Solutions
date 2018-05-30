using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleEnergyShield : PartModule, IAirstreamShield
    {
        GameObject shieldObject;

        [KSPField(isPersistant = true)]
        bool shieldOn = false;

        bool updateShield = true;

        public override void OnAwake()
        {
            base.OnAwake();
            shieldObject = gameObject.GetChild("SphereShield");
            KSPLog.print("shieldObject: " + shieldObject);
            //shieldObject.SetActive(false);
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            /*if (!shieldOn)
            {
                part.DragCubes.SetCubeWeight("Default", 1);
                part.DragCubes.SetCubeWeight("Shield", 0);
                shieldObject.SetActive(false);
            }
            else
            {
                part.DragCubes.SetCubeWeight("Default", 0);
                part.DragCubes.SetCubeWeight("Shield", 1);
            }*/
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiActiveUncommand = false, guiName = "LOL Shield", name = "LOLShield", requireFullControl = true)]
        public void LOLShield()
        {
            shieldOn = !shieldOn;
            updateShield = true;
            //shieldObject.transform.localPosition = gameObject.GetChild("ksp_s_processorSmall_fbx").transform.localPosition;
            //shieldObject.transform.localPosition = part.boundsCentroidOffset;
            //shieldObject.transform.localPosition = part.CenterOfDisplacement;
            //Vector3 velocity = part.Rigidbody.velocity;
            //shieldObject.SetActive(true);
            //part.Rigidbody.velocity = velocity;
            //foreach(Part p in vessel.parts)
            //{
            //p.AddShield(this);
            //}
        }

        public void FixedUpdate()
        {
            if (updateShield)
            {
                if (shieldOn)
                {
                    updateShield = false;
                    part.DragCubes.SetCubeWeight("Default", 0);
                    part.DragCubes.SetCubeWeight("Shield", 1);
                    shieldObject.SetActive(true);
                    part.Update();
                }
                else
                {
                    updateShield = false;
                    part.DragCubes.SetCubeWeight("Default", 1);
                    part.DragCubes.SetCubeWeight("Shield", 0);
                    shieldObject.SetActive(false);
                    part.Update();
                }
            }

        }

        public bool ClosedAndLocked()
        {
            return true;
        }

        public Vessel GetVessel()
        {
            return vessel;
        }

        public Part GetPart()
        {
            return part;
        }
    }
}
