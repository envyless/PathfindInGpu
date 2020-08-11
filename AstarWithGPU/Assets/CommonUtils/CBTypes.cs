using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CBType
{
    [System.Serializable]
    public struct boolean
    {
        public byte boolValue;

        public boolean(bool value)
        {
            boolValue = (byte)(value ? 1 : 0);
        }

        public static implicit operator bool(boolean value)
        {
            return value.boolValue == 1;
        }

        public static implicit operator boolean(bool value)
        {
            return new boolean(value);
        }

        public override string ToString()
        {
            if (boolValue == 1)
                return "true";

            return "false";
        }
    }
}