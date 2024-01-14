using RWCustom;

namespace Lunacy
{
    public static class LCustom
    {
        // Only modifies values outside the range
        public static float LerpMapClamp(float value, float fromA, float fromB, float toA, float toB)
        {
            if ((value > toA && value < toB) || value == toA || value == toB)
            {
                return value;
            }
            return Custom.LerpMap(value, fromA, fromB, toA, toB);
        }
    }
}
