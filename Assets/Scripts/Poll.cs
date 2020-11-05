﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class Poll
{
    public Vector3 postition;
    public List<float> yHeights; //0 is the top most Y value while n is the lowest Y value
    public short pointCount;

    public Poll(Vector3 _pos, List<float> _yheights)
    {
        postition = _pos;
        yHeights = _yheights;
        pointCount = (short)yHeights.Count;
    }
}