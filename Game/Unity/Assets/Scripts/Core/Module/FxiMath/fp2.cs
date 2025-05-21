//
// FixPointCS
//
// Copyright(c) Jere Sanisalo, Petri Kero
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
using FixPointCS;
using System;
using System.Runtime.CompilerServices;

namespace FixMath
{
    /// <summary>
    /// Vector2 struct with signed 32.32 fixed point components.
    /// </summary>
    [Serializable]
    public struct fp2 : IEquatable<fp2>
    {
        // Constants
        public static fp2 Zero     { [MethodImpl(fixmath.AggressiveInlining)] get { return new fp2(Fixed64.Zero, Fixed64.Zero); } }
        public static fp2 One      { [MethodImpl(fixmath.AggressiveInlining)] get { return new fp2(Fixed64.One, Fixed64.One); } }
        public static fp2 Down     { [MethodImpl(fixmath.AggressiveInlining)] get { return new fp2(Fixed64.Zero, Fixed64.Neg1); } }
        public static fp2 Up       { [MethodImpl(fixmath.AggressiveInlining)] get { return new fp2(Fixed64.Zero, Fixed64.One); } }
        public static fp2 Left     { [MethodImpl(fixmath.AggressiveInlining)] get { return new fp2(Fixed64.Neg1, Fixed64.Zero); } }
        public static fp2 Right    { [MethodImpl(fixmath.AggressiveInlining)] get { return new fp2(Fixed64.One, Fixed64.Zero); } }
        public static fp2 AxisX    { [MethodImpl(fixmath.AggressiveInlining)] get { return new fp2(Fixed64.One, Fixed64.Zero); } }
        public static fp2 AxisY    { [MethodImpl(fixmath.AggressiveInlining)] get { return new fp2(Fixed64.Zero, Fixed64.One); } }

        // Raw components
        public long RawX;
        public long RawY;

        // fp accessors
        public fp X { get { return fp.FromRaw(RawX); } set { RawX = value.Raw; } }
        public fp Y { get { return fp.FromRaw(RawY); } set { RawY = value.Raw; } }

        public fp2(fp x, fp y)
        {
            RawX = x.Raw;
            RawY = y.Raw;
        }

        // raw ctor only for internal usage
        private fp2(long x, long y)
        {
            RawX = x;
            RawY = y;
        }

        public static fp2 FromRaw(long rawX, long rawY) { return new fp2(rawX, rawY); }
        public static fp2 FromInt(int x, int y) { return new fp2(Fixed64.FromInt(x), Fixed64.FromInt(y)); }
        public static fp2 FromFloat(float x, float y) { return new fp2(Fixed64.FromFloat(x), Fixed64.FromFloat(y)); }
        public static fp2 FromDouble(double x, double y) { return new fp2(Fixed64.FromDouble(x), Fixed64.FromDouble(y)); }

        public static fp2 operator -(fp2 a) { return new fp2(-a.RawX, -a.RawY); }
        public static fp2 operator +(fp2 a, fp2 b) { return new fp2(a.RawX + b.RawX, a.RawY + b.RawY); }
        public static fp2 operator -(fp2 a, fp2 b) { return new fp2(a.RawX - b.RawX, a.RawY - b.RawY); }
        public static fp2 operator *(fp2 a, fp2 b) { return new fp2(Fixed64.Mul(a.RawX, b.RawX), Fixed64.Mul(a.RawY, b.RawY)); }
        public static fp2 operator /(fp2 a, fp2 b) { return new fp2(Fixed64.DivPrecise(a.RawX, b.RawX), Fixed64.DivPrecise(a.RawY, b.RawY)); }
        public static fp2 operator %(fp2 a, fp2 b) { return new fp2(a.RawX % b.RawX, a.RawY % b.RawY); }

        public static fp2 operator +(fp a, fp2 b) { return new fp2(a.Raw + b.RawX, a.Raw + b.RawY); }
        public static fp2 operator +(fp2 a, fp b) { return new fp2(a.RawX + b.Raw, a.RawY + b.Raw); }
        public static fp2 operator -(fp a, fp2 b) { return new fp2(a.Raw - b.RawX, a.Raw - b.RawY); }
        public static fp2 operator -(fp2 a, fp b) { return new fp2(a.RawX - b.Raw, a.RawY - b.Raw); }
        public static fp2 operator *(fp a, fp2 b) { return new fp2(Fixed64.Mul(a.Raw, b.RawX), Fixed64.Mul(a.Raw, b.RawY)); }
        public static fp2 operator *(fp2 a, fp b) { return new fp2(Fixed64.Mul(a.RawX, b.Raw), Fixed64.Mul(a.RawY, b.Raw)); }
        public static fp2 operator /(fp a, fp2 b) { return new fp2(Fixed64.DivPrecise(a.Raw, b.RawX), Fixed64.DivPrecise(a.Raw, b.RawY)); }
        public static fp2 operator /(fp2 a, fp b) { return new fp2(Fixed64.DivPrecise(a.RawX, b.Raw), Fixed64.DivPrecise(a.RawY, b.Raw)); }
        public static fp2 operator %(fp a, fp2 b) { return new fp2(a.Raw % b.RawX, a.Raw % b.RawY); }
        public static fp2 operator %(fp2 a, fp b) { return new fp2(a.RawX % b.Raw, a.RawY % b.Raw); }

        public static bool operator ==(fp2 a, fp2 b) { return a.RawX == b.RawX && a.RawY == b.RawY; }
        public static bool operator !=(fp2 a, fp2 b) { return a.RawX != b.RawX || a.RawY != b.RawY; }

        public static fp2 Div(fp2 a, fp b) { long oob = Fixed64.Rcp(b.Raw); return new fp2(Fixed64.Mul(a.RawX, oob), Fixed64.Mul(a.RawY, oob)); }
        public static fp2 DivFast(fp2 a, fp b) { long oob = Fixed64.RcpFast(b.Raw); return new fp2(Fixed64.Mul(a.RawX, oob), Fixed64.Mul(a.RawY, oob)); }
        public static fp2 DivFastest(fp2 a, fp b) { long oob = Fixed64.RcpFastest(b.Raw); return new fp2(Fixed64.Mul(a.RawX, oob), Fixed64.Mul(a.RawY, oob)); }
        public static fp2 Div(fp2 a, fp2 b) { return new fp2(Fixed64.Div(a.RawX, b.RawX), Fixed64.Div(a.RawY, b.RawY)); }
        public static fp2 DivFast(fp2 a, fp2 b) { return new fp2(Fixed64.DivFast(a.RawX, b.RawX), Fixed64.DivFast(a.RawY, b.RawY)); }
        public static fp2 DivFastest(fp2 a, fp2 b) { return new fp2(Fixed64.DivFastest(a.RawX, b.RawX), Fixed64.DivFastest(a.RawY, b.RawY)); }
        public static fp2 SqrtPrecise(fp2 a) { return new fp2(Fixed64.SqrtPrecise(a.RawX), Fixed64.SqrtPrecise(a.RawY)); }
        public static fp2 Sqrt(fp2 a) { return new fp2(Fixed64.Sqrt(a.RawX), Fixed64.Sqrt(a.RawY)); }
        public static fp2 SqrtFast(fp2 a) { return new fp2(Fixed64.SqrtFast(a.RawX), Fixed64.SqrtFast(a.RawY)); }
        public static fp2 SqrtFastest(fp2 a) { return new fp2(Fixed64.SqrtFastest(a.RawX), Fixed64.SqrtFastest(a.RawY)); }
        public static fp2 RSqrt(fp2 a) { return new fp2(Fixed64.RSqrt(a.RawX), Fixed64.RSqrt(a.RawY)); }
        public static fp2 RSqrtFast(fp2 a) { return new fp2(Fixed64.RSqrtFast(a.RawX), Fixed64.RSqrtFast(a.RawY)); }
        public static fp2 RSqrtFastest(fp2 a) { return new fp2(Fixed64.RSqrtFastest(a.RawX), Fixed64.RSqrtFastest(a.RawY)); }
        public static fp2 Rcp(fp2 a) { return new fp2(Fixed64.Rcp(a.RawX), Fixed64.Rcp(a.RawY)); }
        public static fp2 RcpFast(fp2 a) { return new fp2(Fixed64.RcpFast(a.RawX), Fixed64.RcpFast(a.RawY)); }
        public static fp2 RcpFastest(fp2 a) { return new fp2(Fixed64.RcpFastest(a.RawX), Fixed64.RcpFastest(a.RawY)); }
        public static fp2 Exp(fp2 a) { return new fp2(Fixed64.Exp(a.RawX), Fixed64.Exp(a.RawY)); }
        public static fp2 ExpFast(fp2 a) { return new fp2(Fixed64.ExpFast(a.RawX), Fixed64.ExpFast(a.RawY)); }
        public static fp2 ExpFastest(fp2 a) { return new fp2(Fixed64.ExpFastest(a.RawX), Fixed64.ExpFastest(a.RawY)); }
        public static fp2 Exp2(fp2 a) { return new fp2(Fixed64.Exp2(a.RawX), Fixed64.Exp2(a.RawY)); }
        public static fp2 Exp2Fast(fp2 a) { return new fp2(Fixed64.Exp2Fast(a.RawX), Fixed64.Exp2Fast(a.RawY)); }
        public static fp2 Exp2Fastest(fp2 a) { return new fp2(Fixed64.Exp2Fastest(a.RawX), Fixed64.Exp2Fastest(a.RawY)); }
        public static fp2 Log(fp2 a) { return new fp2(Fixed64.Log(a.RawX), Fixed64.Log(a.RawY)); }
        public static fp2 LogFast(fp2 a) { return new fp2(Fixed64.LogFast(a.RawX), Fixed64.LogFast(a.RawY)); }
        public static fp2 LogFastest(fp2 a) { return new fp2(Fixed64.LogFastest(a.RawX), Fixed64.LogFastest(a.RawY)); }
        public static fp2 Log2(fp2 a) { return new fp2(Fixed64.Log2(a.RawX), Fixed64.Log2(a.RawY)); }
        public static fp2 Log2Fast(fp2 a) { return new fp2(Fixed64.Log2Fast(a.RawX), Fixed64.Log2Fast(a.RawY)); }
        public static fp2 Log2Fastest(fp2 a) { return new fp2(Fixed64.Log2Fastest(a.RawX), Fixed64.Log2Fastest(a.RawY)); }
        public static fp2 Sin(fp2 a) { return new fp2(Fixed64.Sin(a.RawX), Fixed64.Sin(a.RawY)); }
        public static fp2 SinFast(fp2 a) { return new fp2(Fixed64.SinFast(a.RawX), Fixed64.SinFast(a.RawY)); }
        public static fp2 SinFastest(fp2 a) { return new fp2(Fixed64.SinFastest(a.RawX), Fixed64.SinFastest(a.RawY)); }
        public static fp2 Cos(fp2 a) { return new fp2(Fixed64.Cos(a.RawX), Fixed64.Cos(a.RawY)); }
        public static fp2 CosFast(fp2 a) { return new fp2(Fixed64.CosFast(a.RawX), Fixed64.CosFast(a.RawY)); }
        public static fp2 CosFastest(fp2 a) { return new fp2(Fixed64.CosFastest(a.RawX), Fixed64.CosFastest(a.RawY)); }

        public static fp2 Pow(fp2 a, fp b) { return new fp2(Fixed64.Pow(a.RawX, b.Raw), Fixed64.Pow(a.RawY, b.Raw)); }
        public static fp2 PowFast(fp2 a, fp b) { return new fp2(Fixed64.PowFast(a.RawX, b.Raw), Fixed64.PowFast(a.RawY, b.Raw)); }
        public static fp2 PowFastest(fp2 a, fp b) { return new fp2(Fixed64.PowFastest(a.RawX, b.Raw), Fixed64.PowFastest(a.RawY, b.Raw)); }
        public static fp2 Pow(fp a, fp2 b) { return new fp2(Fixed64.Pow(a.Raw, b.RawX), Fixed64.Pow(a.Raw, b.RawY)); }
        public static fp2 PowFast(fp a, fp2 b) { return new fp2(Fixed64.PowFast(a.Raw, b.RawX), Fixed64.PowFast(a.Raw, b.RawY)); }
        public static fp2 PowFastest(fp a, fp2 b) { return new fp2(Fixed64.PowFastest(a.Raw, b.RawX), Fixed64.PowFastest(a.Raw, b.RawY)); }
        public static fp2 Pow(fp2 a, fp2 b) { return new fp2(Fixed64.Pow(a.RawX, b.RawX), Fixed64.Pow(a.RawY, b.RawY)); }
        public static fp2 PowFast(fp2 a, fp2 b) { return new fp2(Fixed64.PowFast(a.RawX, b.RawX), Fixed64.PowFast(a.RawY, b.RawY)); }
        public static fp2 PowFastest(fp2 a, fp2 b) { return new fp2(Fixed64.PowFastest(a.RawX, b.RawX), Fixed64.PowFastest(a.RawY, b.RawY)); }

        public static fp Length(fp2 a) { return fp.FromRaw(Fixed64.Sqrt(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); }
        public static fp LengthFast(fp2 a) { return fp.FromRaw(Fixed64.SqrtFast(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); }
        public static fp LengthFastest(fp2 a) { return fp.FromRaw(Fixed64.SqrtFastest(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); }
        public static fp LengthSqr(fp2 a) { return fp.FromRaw(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY)); }
        public static fp2 Normalize(fp2 a) { fp ooLen = fp.FromRaw(Fixed64.RSqrt(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); return ooLen * a; }
        public static fp2 NormalizeFast(fp2 a) { fp ooLen = fp.FromRaw(Fixed64.RSqrtFast(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); return ooLen * a; }
        public static fp2 NormalizeFastest(fp2 a) { fp ooLen = fp.FromRaw(Fixed64.RSqrtFastest(Fixed64.Mul(a.RawX, a.RawX) + Fixed64.Mul(a.RawY, a.RawY))); return ooLen * a; }

        public static fp Dot(fp2 a, fp2 b) { return fp.FromRaw(Fixed64.Mul(a.RawX, b.RawX) + Fixed64.Mul(a.RawY, b.RawY)); }
        public static fp Distance(fp2 a, fp2 b) { return Length(a - b); }
        public static fp DistanceFast(fp2 a, fp2 b) { return LengthFast(a - b); }
        public static fp DistanceFastest(fp2 a, fp2 b) { return LengthFastest(a - b); }

        public static fp2 Min(fp2 a, fp2 b) { return new fp2(Fixed64.Min(a.RawX, b.RawX), Fixed64.Min(a.RawY, b.RawY)); }
        public static fp2 Max(fp2 a, fp2 b) { return new fp2(Fixed64.Max(a.RawX, b.RawX), Fixed64.Max(a.RawY, b.RawY)); }

        public static fp2 Clamp(fp2 a, fp min, fp max)
        {
            return new fp2(
                Fixed64.Clamp(a.RawX, min.Raw, max.Raw),
                Fixed64.Clamp(a.RawY, min.Raw, max.Raw));
        }

        public static fp2 Clamp(fp2 a, fp2 min, fp2 max)
        {
            return new fp2(
                Fixed64.Clamp(a.RawX, min.RawX, max.RawX),
                Fixed64.Clamp(a.RawY, min.RawY, max.RawY));
        }

        public static fp2 Lerp(fp2 a, fp2 b, fp t)
        {
            long tb = t.Raw;
            long ta = Fixed64.One - tb;
            return new fp2(
                Fixed64.Mul(a.RawX, ta) + Fixed64.Mul(b.RawX, tb),
                Fixed64.Mul(a.RawY, ta) + Fixed64.Mul(b.RawY, tb));
        }

        public bool Equals(fp2 other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is fp2))
                return false;
            return ((fp2)obj) == this;
        }

        public override string ToString()
        {
            return "(" + Fixed64.ToString(RawX) + ", " + Fixed64.ToString(RawY) + ")";
        }

        public override int GetHashCode()
        {
            return RawX.GetHashCode() ^ RawY.GetHashCode() * 7919;
        }
    }
}