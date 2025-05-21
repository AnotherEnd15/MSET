using FixMath;

namespace ET
{
    public static class MathHelper
    {
        public static fp GetAngle(fp2 dir)
        {
            dir = fp2.Normalize(dir);
            var degree = fp.RadToDeg(fp.Atan2(dir.Y, dir.X));
            while (degree < 0)
            {
                degree += 360;
            }

            while (degree >= 360)
            {
                degree -= 360;
            }

            return degree;
        }
    }
}