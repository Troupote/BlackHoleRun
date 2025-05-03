using System;
using UnityEngine;

namespace BHR
{
    [Flags]
    public enum AllowedPlayerInput { NONE = 0, FIRST_PLAYER = 0 << 1, SECOND_PLAYER = 1 << 1, BOTH = FIRST_PLAYER | SECOND_PLAYER }
}
