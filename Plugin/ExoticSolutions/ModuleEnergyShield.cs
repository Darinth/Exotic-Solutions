using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleEnergyShield : PartModule
    {
        public override void OnAwake()
        {
            base.OnAwake();
            GameObject shieldObject = gameObject.GetChild("SphereShield");
            KSPLog.print("shieldObject: " + shieldObject);
            shieldObject.SetActive(false);
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUncommand = false, guiName = "LOL Shield", name = "LOLShield", requireFullControl = true)]
        public void LOLShield()
        {
            GameObject colliderParent = gameObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            //GameObject LOLShield = new GameObject();
            //LOLShield.name = "LOLShield";
            GameObject shieldCollider = new GameObject();
            //LOLShield.transform.parent = gameObject.GetChild("model").transform;
            SphereCollider newCollider = shieldCollider.AddComponent<UnityEngine.SphereCollider>();
            newCollider.radius = 10;
            shieldCollider.name = "collider_shield";
            shieldCollider.transform.parent = colliderParent.transform;
            shieldCollider.transform.localPosition = new Vector3(0, 0, 0);
            shieldCollider.transform.localScale = new Vector3(1, 1, 1);

            newCollider.center = gameObject.GetChild("collider_hex").GetComponent<MeshCollider>().bounds.center;
            GameObject sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            MeshRenderer sphereRenderer = sphereObject.GetComponent<MeshRenderer>();
            //sphereRenderer.transform.localScale = new Vector3(20, 20, 20);
            //sphereObject.transform.position = new Vector3(0, 0, 0);
            sphereObject.transform.localScale = new Vector3(20, 20, 20);
            sphereObject.transform.parent = colliderParent.transform;
            sphereObject.transform.localPosition = new Vector3(0, 0, 0);
            SphereCollider logCollider = newCollider;
            GameObject logGO = shieldCollider;
            KSPLog.print("logCollider: " + logCollider);
            KSPLog.print("attachedRigidbody: " + logCollider.attachedRigidbody);
            KSPLog.print("bounds: " + logCollider.bounds);
            KSPLog.print("contactOffset: " + logCollider.contactOffset);
            KSPLog.print("enabled: " + logCollider.enabled);
            KSPLog.print("gameObject: " + logCollider.gameObject);
            KSPLog.print("hideFlags: " + logCollider.hideFlags);
            KSPLog.print("isTrigger: " + logCollider.isTrigger);
            KSPLog.print("material: " + logCollider.material);
            KSPLog.print("name: " + logCollider.name);
            KSPLog.print("tag: " + logCollider.tag);
            KSPLog.print("tag: " + logCollider.radius);
            KSPLog.print("GO active: " + logGO.active);
            KSPLog.print("GO activeInHierarchy: " + logGO.activeInHierarchy);
            KSPLog.print("GO activeSelf: " + logGO.activeSelf);
            KSPLog.print("GO hideFlags: " + logGO.hideFlags);
            KSPLog.print("GO isStatic: " + logGO.isStatic);
            KSPLog.print("GO layer: " + logGO.layer);
            KSPLog.print("GO name: " + logGO.name);
            KSPLog.print("GO scene: " + logGO.scene);
            KSPLog.print("GO tag: " + logGO.tag);
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUncommand = false, guiName = "Print Collider", name = "PrintCollider", requireFullControl = true)]
        public void PrintCollider()
        {
            MeshCollider logCollider = gameObject.GetChild("collider_hex").GetComponent<MeshCollider>();
            GameObject logGO = gameObject.GetChild("collider_hex");
            KSPLog.print("logCollider: " + logCollider);
            KSPLog.print("attachedRigidbody: " + logCollider.attachedRigidbody);
            KSPLog.print("bounds: " + logCollider.bounds);
            KSPLog.print("contactOffset: " + logCollider.contactOffset);
            KSPLog.print("convex: " + logCollider.convex);
            KSPLog.print("enabled: " + logCollider.enabled);
            KSPLog.print("gameObject: " + logCollider.gameObject);
            KSPLog.print("hideFlags: " + logCollider.hideFlags);
            KSPLog.print("inflateMesh: " + logCollider.inflateMesh);
            KSPLog.print("isTrigger: " + logCollider.isTrigger);
            KSPLog.print("material: " + logCollider.material);
            KSPLog.print("name: " + logCollider.name);
            KSPLog.print("tag: " + logCollider.tag);
            KSPLog.print("GO active: " + logGO.active);
            KSPLog.print("GO activeInHierarchy: " + logGO.activeInHierarchy);
            KSPLog.print("GO activeSelf: " + logGO.activeSelf);
            KSPLog.print("GO hideFlags: " + logGO.hideFlags);
            KSPLog.print("GO isStatic: " + logGO.isStatic);
            KSPLog.print("GO layer: " + logGO.layer);
            KSPLog.print("GO name: " + logGO.name);
            KSPLog.print("GO scene: " + logGO.scene);
            KSPLog.print("GO tag: " + logGO.tag);
        }

    }
}
