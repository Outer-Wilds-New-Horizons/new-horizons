using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Props
{
    public class NHRaftController : MonoBehaviour
    {
        RaftController raft;

        public void OnEnable()
        {
            raft = GetComponent<RaftController>();
            raft._fluidDetector.OnEnterFluid += OnEnterFluid;
        }

        public void OnDisable()
        {
            raft._fluidDetector.OnEnterFluid -= OnEnterFluid;
        }

        private void OnEnterFluid(FluidVolume volume)
        {
            if (volume.GetFluidType() == FluidVolume.Type.WATER)
            {
                raft._fluidDetector._alignmentFluid = volume;
            }
        }
    }
}
