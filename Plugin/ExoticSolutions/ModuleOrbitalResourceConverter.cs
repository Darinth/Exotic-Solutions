using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExoticSolutions
{
    class ModuleOrbitalResourceConverter : ModuleResourceConverter
    {
        public override bool IsSituationValid()
        {
            return vessel.geeForce < 0.01;
        }
    }
}
