using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Components.Quantum
{
    /// <summary>
    /// exists because MultiStateQuantumObject only checks visibility on the current state,
    /// whereas this one also checks on each new state, in case they are bigger
    /// </summary>
    public class NHMultiStateQuantumObject : MultiStateQuantumObject
    {
        public override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
        {
            for (int i = 0; i < _prerequisiteObjects.Length; i++)
            {
                if (!_prerequisiteObjects[i].HasCollapsed())
                {
                    return false;
                }
            }
            int stateIndex = _stateIndex;
            if (_stateIndex == -1 && _initialState != -1)
            {
                _stateIndex = _initialState;
            }
            else if (_sequential)
            {
                _stateIndex = _reverse ? _stateIndex - 1 : _stateIndex + 1;
                if (_loop)
                {
                    if (_stateIndex < 0)
                    {
                        _stateIndex = _states.Length - 1;
                    }
                    else if (_stateIndex > _states.Length - 1)
                    {
                        _stateIndex = 0;
                    }
                }
                else
                {
                    _stateIndex = Mathf.Clamp(_stateIndex, 0, _states.Length - 1);
                }
            }
            else
            {

                // Iterate over list of possible states to find a valid state to collapse to
                // current state is excluded, and states are randomly ordered using a weighted random roll to prioritize states with higher probability
                // NOTE: they aren't actually pre-sorted into this random order, this random ordering is done on the fly using RollState

                List<int> indices = new List<int>();
                for (var i = 0; i < _states.Length; i++) if (i != stateIndex) indices.Add(i);

                var previousIndex = stateIndex;

                do
                {
                    previousIndex = _stateIndex;
                    _stateIndex = RollState(stateIndex, indices);
                    if (previousIndex >= 0 && previousIndex < _states.Length) _states[previousIndex].SetVisible(visible: false);
                    _states[_stateIndex].SetVisible(visible: true);

                    NHLogger.LogVerbose($"MultiStateQuantumObject - Trying to change state {_stateIndex}");

                    indices.Remove(_stateIndex);
                } while (!CurrentStateIsValid() && indices.Count > 0);
            }

            var stateIndexIsValid = stateIndex >= 0 && stateIndex < _states.Length;
            if (stateIndexIsValid) _states[stateIndex].SetVisible(visible: false);

            _states[_stateIndex].SetVisible(visible: true);
            if (!CurrentStateIsValid() && stateIndexIsValid)
            {
                _states[_stateIndex].SetVisible(visible: false);
                _states[stateIndex].SetVisible(visible: true);
                _stateIndex = stateIndex;
                return false;
            }

            if (_sequential && !_loop && _stateIndex == _states.Length - 1)
            {
                SetActivation(active: false);
            }
            return true;
        }

        public bool CurrentStateIsValid()
        {
            var isPlayerEntangled = IsPlayerEntangled();
            var illumination = CheckIllumination();
            // faster than full CheckVisibility
            var visibility = CheckVisibilityInstantly();
            var playerInside = CheckPointInside(Locator.GetPlayerCamera().transform.position);
            // does not check probe, but thats okay

            var notEntangledCheck = illumination ? visibility : playerInside;
            var isVisible = isPlayerEntangled ? illumination : notEntangledCheck;
            // I think this is what the above two lines simplify to but I don't want to test this:
            // illumination ? visibility || isPlayerEntangled : playerInside

            return !isVisible;
        }

        public int RollState(int excludeIndex, List<int> indices)
        {
            var stateIndex = excludeIndex;

            // this function constructs a sort of segmented range:
            // 0                   3     5  6
            // +-------------------+-----+--+
            // | state 1           | s2  |s3|
            // +-------------------+-----+--+
            //
            // num is the max value of this range (min is always 0)
            // num2 is a random point on this range
            //
            // num3 and num4 track the bounds of the current segment being considered
            // num3 is the min value, num4 is the max. for example, if num3=5 then num4=6
            //
            // the second for looop uses num3 and num4 to figure out which segment num2 landed in

            int num = 0;
            foreach (int j in indices)
            {
                if (j != stateIndex)
                {
                    _probabilities[j] = _states[j].GetProbability();
                    num += _probabilities[j];
                }
            }
            int num2 = Random.Range(0, num);
            int num3 = 0;
            int num4 = 0;
            foreach (int k in indices)
            {
                if (k != stateIndex)
                {
                    num3 = num4;
                    num4 += _probabilities[k];
                    if (_probabilities[k] > 0 && num2 >= num3 && num2 < num4)
                    {
                        return k;
                    }
                }
            }

            return indices[indices.Count - 1];
        }
    }
}
