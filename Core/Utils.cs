﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Jay.SourceBuilderHelpers;

public static class Utils
{
    public static int Clamp(this int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}