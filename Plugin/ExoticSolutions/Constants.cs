using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    static class Constants
    {
        private static PartResourceDefinition pEEDefinition;
        public static PartResourceDefinition EEDefinition
        {
            get
            {
                if (pEEDefinition == null)
                    pEEDefinition = PartResourceLibrary.Instance.GetDefinition("ExoticEnergies");
                return pEEDefinition;
            }
        }

        private static PartResourceDefinition pEMDefinition;
        public static PartResourceDefinition EMDefinition
        {
            get
            {
                if (pEMDefinition == null)
                    pEMDefinition = PartResourceLibrary.Instance.GetDefinition("ExoticMaterials");
                return pEMDefinition;
            }
        }

        private static PartResourceDefinition pECDefinition;
        public static PartResourceDefinition ECDefinition
        {
            get
            {
                if (pECDefinition == null)
                    pECDefinition = PartResourceLibrary.Instance.GetDefinition("ElectricCharge");
                return pECDefinition;
            }
        }

        public static ExoticSolutionsScenario exoticSolutionScenario;
    }
}