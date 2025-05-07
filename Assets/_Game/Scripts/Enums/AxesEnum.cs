using System;
using UnityEngine;

namespace BHR
{
    [Flags]
    public enum Axes {NONE = 0, X = 1 << 0, Y = 1 << 1, Z = 1 << 2};
}
