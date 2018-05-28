using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleEnergyShield : PartModule
    {
        GameObject shieldObject;

        public override void OnAwake()
        {
            base.OnAwake();
            shieldObject = gameObject.GetChild("SphereShield");
            KSPLog.print("shieldObject: " + shieldObject);
            shieldObject.SetActive(false);
            part.DragCubes.SetCubeWeight("Shield", 0);
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUncommand = false, guiName = "LOL Shield", name = "LOLShield", requireFullControl = true)]
        public void LOLShield()
        {
            Vector3 velocity = part.Rigidbody.velocity;
            shieldObject.SetActive(true);
            part.Rigidbody.velocity = velocity;
        }
    }
}
