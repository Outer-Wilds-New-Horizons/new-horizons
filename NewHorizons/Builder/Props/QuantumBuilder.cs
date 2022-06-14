using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class QuantumBuilder
    {
        
        public static void Make(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod, PropModule.QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            // if a prop doesn't have a visibiilty volume, create a box volume using the prop's mesh bounds (if there are multiple mesh filters, use the min/max bounds accross all meshes)
            switch(quantumGroup.type)
            {
                case PropModule.QuantumGroupType.Sockets: MakeSocketGroup (go, sector, config, mod, quantumGroup, propsInGroup); return;
                case PropModule.QuantumGroupType.States:  MakeStateGroup  (go, sector, config, mod, quantumGroup, propsInGroup); return;
                case PropModule.QuantumGroupType.Shuffle: MakeShuffleGroup(go, sector, config, mod, quantumGroup, propsInGroup); return;
            }
        }
        
        public static void MakeSocketGroup(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod, PropModule.QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            // note: for the visibility boxes on quantum sockets, if there's only one prop that's part of this group, clone its visibility volume
            // otherwise, create a box according to the max and min dimensions across all props in this group (ie the box should be able to fit inside of it the visibility volume on any prop in this group)
            
        }

        public static void MakeStateGroup(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod, PropModule.QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            
        }

        public static void MakeShuffleGroup(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod, PropModule.QuantumGroupInfo quantumGroup, GameObject[] propsInGroup)
        {
            // if the type is shuffle, create a new game object that has a location average of all props' locations, and put a visibility volume on it that all props' visibility volumes fit inside of
            // then, make all props children of it
            
        }
    }
}
