using System;
using System.Collections.Generic;
using UnityEngine;

namespace test
{
    public class Poll
    {
        public Vector3 postition;
        public List<float> yHeights;
        public List<float> yBlockedHeights;

        public Poll(Vector3 _pos, List<float> _yheights, List<float> _yBlockedHeights = null)
        {
            postition = _pos;
            yHeights = _yheights;
            yBlockedHeights = _yBlockedHeights;
        }
    }
}