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
    /// Signed 32.32 fixed point value struct.
    /// </summary>
    [Serializable]
    public struct fp : IComparable<fp>, IEquatable<fp>, IComparable
    {
        // Constants
        public static fp Neg1      { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.Neg1); } }
        public static fp Zero      { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.Zero); } }
        public static fp Half      { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.Half); } }
        
        public static fp _01      { [MethodImpl(fixmath.AggressiveInlining)] get { return One / 10; } }
        
        public static fp One       { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.One); } }
        public static fp Two       { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.Two); } }
        public static fp Pi        { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.Pi); } }
        public static fp Pi2       { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.Pi2); } }
        public static fp PiHalf    { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.PiHalf); } }
        public static fp E         { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.E); } }

        public static fp MinValue  { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.MinValue); } }
        public static fp MaxValue  { [MethodImpl(fixmath.AggressiveInlining)] get { return FromRaw(Fixed64.MaxValue); } }

        // Raw fixed point value
        public long Raw;

        // Construction
        [MethodImpl(fixmath.AggressiveInlining)] public static fp FromRaw(long raw) { fp v; v.Raw = raw; return v; }
        [MethodImpl(fixmath.AggressiveInlining)] public static fp FromInt(int v) { return FromRaw(Fixed64.FromInt(v)); }
        [MethodImpl(fixmath.AggressiveInlining)] public static implicit operator fp(int v) { return FromRaw(Fixed64.FromInt(v)); }
        [MethodImpl(fixmath.AggressiveInlining)] public static fp FromFloat(float v) { return FromRaw(Fixed64.FromFloat(v)); }
        [MethodImpl(fixmath.AggressiveInlining)] public static fp FromDouble(double v) { return FromRaw(Fixed64.FromDouble(v)); }
        // Conversions
        public static int FloorToInt(fp a) { return Fixed64.FloorToInt(a.Raw); }
        public static int CeilToInt(fp a) { return Fixed64.CeilToInt(a.Raw); }

        [MethodImpl(fixmath.AggressiveInlining)]
        public static explicit operator int(fp v)
        {
            return Fixed64.FloorToInt(v.Raw); 
        }
        
        
        public static int RoundToInt(fp a) { return Fixed64.RoundToInt(a.Raw); }
        public float Float { get { return Fixed64.ToFloat(Raw); } }
        public double Double { get { return Fixed64.ToDouble(Raw); } }

        // Creates the fixed point number that's a divided by b.
        public static fp Ratio(int a, int b) { return fp.FromRaw(((long)a << 32) / b); }
        // Creates the fixed point number that's a divided by 10.
        public static fp Ratio10(int a) { return fp.FromRaw(((long)a << 32) / 10); }
        // Creates the fixed point number that's a divided by 100.
        public static fp Ratio100(int a) { return fp.FromRaw(((long)a << 32) / 100); }
        // Creates the fixed point number that's a divided by 1000.
        public static fp Ratio1000(int a) { return fp.FromRaw(((long)a << 32) / 1000); }

        // Operators
        public static fp operator -(fp v1) { return FromRaw(-v1.Raw); }

        public static fp operator +(fp v1, fp v2) { return FromRaw(v1.Raw + v2.Raw); }
        public static fp operator -(fp v1, fp v2) { return FromRaw(v1.Raw - v2.Raw); }
        public static fp operator *(fp v1, fp v2) { return FromRaw(Fixed64.Mul(v1.Raw, v2.Raw)); }
        public static fp operator /(fp v1, fp v2) { return FromRaw(Fixed64.DivPrecise(v1.Raw, v2.Raw)); }
        public static fp operator %(fp v1, fp v2) { return FromRaw(Fixed64.Mod(v1.Raw, v2.Raw)); }

        public static fp operator +(fp v1, int v2) { return FromRaw(v1.Raw + Fixed64.FromInt(v2)); }
        public static fp operator +(int v1, fp v2) { return FromRaw(Fixed64.FromInt(v1) + v2.Raw); }
        public static fp operator -(fp v1, int v2) { return FromRaw(v1.Raw - Fixed64.FromInt(v2)); }
        public static fp operator -(int v1, fp v2) { return FromRaw(Fixed64.FromInt(v1) - v2.Raw); }
        public static fp operator *(fp v1, int v2) { return FromRaw(v1.Raw * (long)v2); }
        public static fp operator *(int v1, fp v2) { return FromRaw((long)v1 * v2.Raw); }
        public static fp operator /(fp v1, int v2) { return FromRaw(v1.Raw / (long)v2); }
        public static fp operator /(int v1, fp v2) { return FromRaw(Fixed64.DivPrecise(Fixed64.FromInt(v1), v2.Raw)); }
        public static fp operator %(fp v1, int v2) { return FromRaw(Fixed64.Mod(v1.Raw, Fixed64.FromInt(v2))); }
        public static fp operator %(int v1, fp v2) { return FromRaw(Fixed64.Mod(Fixed64.FromInt(v1), v2.Raw)); }

        public static fp operator ++(fp v1) { return FromRaw(v1.Raw + Fixed64.One); }
        public static fp operator --(fp v1) { return FromRaw(v1.Raw - Fixed64.One); }

        public static bool operator ==(fp v1, fp v2) { return v1.Raw == v2.Raw; }
        public static bool operator !=(fp v1, fp v2) { return v1.Raw != v2.Raw; }
        public static bool operator <(fp v1, fp v2) { return v1.Raw < v2.Raw; }
        public static bool operator <=(fp v1, fp v2) { return v1.Raw <= v2.Raw; }
        public static bool operator >(fp v1, fp v2) { return v1.Raw > v2.Raw; }
        public static bool operator >=(fp v1, fp v2) { return v1.Raw >= v2.Raw; }

        public static bool operator ==(int v1, fp v2) { return Fixed64.FromInt(v1) == v2.Raw; }
        public static bool operator ==(fp v1, int v2) { return v1.Raw == Fixed64.FromInt(v2); }
        public static bool operator !=(int v1, fp v2) { return Fixed64.FromInt(v1) != v2.Raw; }
        public static bool operator !=(fp v1, int v2) { return v1.Raw != Fixed64.FromInt(v2); }
        public static bool operator <(int v1, fp v2) { return Fixed64.FromInt(v1) < v2.Raw; }
        public static bool operator <(fp v1, int v2) { return v1.Raw < Fixed64.FromInt(v2); }
        public static bool operator <=(int v1, fp v2) { return Fixed64.FromInt(v1) <= v2.Raw; }
        public static bool operator <=(fp v1, int v2) { return v1.Raw <= Fixed64.FromInt(v2); }
        public static bool operator >(int v1, fp v2) { return Fixed64.FromInt(v1) > v2.Raw; }
        public static bool operator >(fp v1, int v2) { return v1.Raw > Fixed64.FromInt(v2); }
        public static bool operator >=(int v1, fp v2) { return Fixed64.FromInt(v1) >= v2.Raw; }
        public static bool operator >=(fp v1, int v2) { return v1.Raw >= Fixed64.FromInt(v2); }
        

        public static fp RadToDeg(fp a) { return FromRaw(Fixed64.Mul(a.Raw, 246083499198)); } // 180 / F64.Pi
        public static fp DegToRad(fp a) { return FromRaw(Fixed64.Mul(a.Raw, 74961320)); }     // F64.Pi / 180

        public static fp Div2(fp a) { return FromRaw(a.Raw >> 1); }
        public static fp Abs(fp a) { return FromRaw(Fixed64.Abs(a.Raw)); }
        public static fp Nabs(fp a) { return FromRaw(Fixed64.Nabs(a.Raw)); }
        public static int Sign(fp a) { return Fixed64.Sign(a.Raw); }
        public static fp Ceil(fp a) { return FromRaw(Fixed64.Ceil(a.Raw)); }
        public static fp Floor(fp a) { return FromRaw(Fixed64.Floor(a.Raw)); }
        public static fp Round(fp a) { return FromRaw(Fixed64.Round(a.Raw)); }
        public static fp Fract(fp a) { return FromRaw(Fixed64.Fract(a.Raw)); }
        public static fp Div(fp a, fp b) { return FromRaw(Fixed64.Div(a.Raw, b.Raw)); }
        public static fp DivFast(fp a, fp b) { return FromRaw(Fixed64.DivFast(a.Raw, b.Raw)); }
        public static fp DivFastest(fp a, fp b) { return FromRaw(Fixed64.DivFastest(a.Raw, b.Raw)); }
        public static fp SqrtPrecise(fp a) { return FromRaw(Fixed64.SqrtPrecise(a.Raw)); }
        public static fp Sqrt(fp a) { return FromRaw(Fixed64.Sqrt(a.Raw)); }
        public static fp SqrtFast(fp a) { return FromRaw(Fixed64.SqrtFast(a.Raw)); }
        public static fp SqrtFastest(fp a) { return FromRaw(Fixed64.SqrtFastest(a.Raw)); }
        public static fp RSqrt(fp a) { return FromRaw(Fixed64.RSqrt(a.Raw)); }
        public static fp RSqrtFast(fp a) { return FromRaw(Fixed64.RSqrtFast(a.Raw)); }
        public static fp RSqrtFastest(fp a) { return FromRaw(Fixed64.RSqrtFastest(a.Raw)); }
        public static fp Rcp(fp a) { return FromRaw(Fixed64.Rcp(a.Raw)); }
        public static fp RcpFast(fp a) { return FromRaw(Fixed64.RcpFast(a.Raw)); }
        public static fp RcpFastest(fp a) { return FromRaw(Fixed64.RcpFastest(a.Raw)); }
        public static fp Exp(fp a) { return FromRaw(Fixed64.Exp(a.Raw)); }
        public static fp ExpFast(fp a) { return FromRaw(Fixed64.ExpFast(a.Raw)); }
        public static fp ExpFastest(fp a) { return FromRaw(Fixed64.ExpFastest(a.Raw)); }
        public static fp Exp2(fp a) { return FromRaw(Fixed64.Exp2(a.Raw)); }
        public static fp Exp2Fast(fp a) { return FromRaw(Fixed64.Exp2Fast(a.Raw)); }
        public static fp Exp2Fastest(fp a) { return FromRaw(Fixed64.Exp2Fastest(a.Raw)); }
        public static fp Log(fp a) { return FromRaw(Fixed64.Log(a.Raw)); }
        public static fp LogFast(fp a) { return FromRaw(Fixed64.LogFast(a.Raw)); }
        public static fp LogFastest(fp a) { return FromRaw(Fixed64.LogFastest(a.Raw)); }
        public static fp Log2(fp a) { return FromRaw(Fixed64.Log2(a.Raw)); }
        public static fp Log2Fast(fp a) { return FromRaw(Fixed64.Log2Fast(a.Raw)); }
        public static fp Log2Fastest(fp a) { return FromRaw(Fixed64.Log2Fastest(a.Raw)); }

        public static fp Sin(fp a) { return FromRaw(Fixed64.Sin(a.Raw)); }
        public static fp SinFast(fp a) { return FromRaw(Fixed64.SinFast(a.Raw)); }
        public static fp SinFastest(fp a) { return FromRaw(Fixed64.SinFastest(a.Raw)); }
        public static fp Cos(fp a) { return FromRaw(Fixed64.Cos(a.Raw)); }
        public static fp CosFast(fp a) { return FromRaw(Fixed64.CosFast(a.Raw)); }
        public static fp CosFastest(fp a) { return FromRaw(Fixed64.CosFastest(a.Raw)); }
        public static fp Tan(fp a) { return FromRaw(Fixed64.Tan(a.Raw)); }
        public static fp TanFast(fp a) { return FromRaw(Fixed64.TanFast(a.Raw)); }
        public static fp TanFastest(fp a) { return FromRaw(Fixed64.TanFastest(a.Raw)); }
        public static fp Asin(fp a) { return FromRaw(Fixed64.Asin(a.Raw)); }
        public static fp AsinFast(fp a) { return FromRaw(Fixed64.AsinFast(a.Raw)); }
        public static fp AsinFastest(fp a) { return FromRaw(Fixed64.AsinFastest(a.Raw)); }
        public static fp Acos(fp a) { return FromRaw(Fixed64.Acos(a.Raw)); }
        public static fp AcosFast(fp a) { return FromRaw(Fixed64.AcosFast(a.Raw)); }
        public static fp AcosFastest(fp a) { return FromRaw(Fixed64.AcosFastest(a.Raw)); }
        public static fp Atan(fp a) { return FromRaw(Fixed64.Atan(a.Raw)); }
        public static fp AtanFast(fp a) { return FromRaw(Fixed64.AtanFast(a.Raw)); }
        public static fp AtanFastest(fp a) { return FromRaw(Fixed64.AtanFastest(a.Raw)); }
        public static fp Atan2(fp y, fp x) { return FromRaw(Fixed64.Atan2(y.Raw, x.Raw)); }
        public static fp Atan2Fast(fp y, fp x) { return FromRaw(Fixed64.Atan2Fast(y.Raw, x.Raw)); }
        public static fp Atan2Fastest(fp y, fp x) { return FromRaw(Fixed64.Atan2Fastest(y.Raw, x.Raw)); }
        public static fp Pow(fp a, fp b) { return FromRaw(Fixed64.Pow(a.Raw, b.Raw)); }
        public static fp PowFast(fp a, fp b) { return FromRaw(Fixed64.PowFast(a.Raw, b.Raw)); }
        public static fp PowFastest(fp a, fp b) { return FromRaw(Fixed64.PowFastest(a.Raw, b.Raw)); }

        public static fp Min(fp a, fp b) { return FromRaw(Fixed64.Min(a.Raw, b.Raw)); }
        public static fp Max(fp a, fp b) { return FromRaw(Fixed64.Max(a.Raw, b.Raw)); }
        public static fp Clamp(fp a, fp min, fp max) { return FromRaw(Fixed64.Clamp(a.Raw, min.Raw, max.Raw)); }
        public static fp Clamp01(fp a) { return FromRaw(Fixed64.Clamp(a.Raw, Fixed64.Zero, Fixed64.One)); }

        public static fp Lerp(fp a, fp b, fp t)
        {
            long tb = t.Raw;
            long ta = Fixed64.One - tb;
            return FromRaw(Fixed64.Mul(a.Raw, ta) + Fixed64.Mul(b.Raw, tb));
        }

        public bool Equals(fp other)
        {
            return (Raw == other.Raw);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is fp))
                return false;
            return ((fp)obj).Raw == Raw;
        }

        public int CompareTo(fp other)
        {
            if (Raw < other.Raw) return -1;
            if (Raw > other.Raw) return +1;
            return 0;
        }

        public override string ToString()
        {
            return Fixed64.ToString(Raw);
        }

        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj is fp other)
                return CompareTo(other);
            else if (obj is null)
                return 1;
            // don't allow comparisons with other numeric or non-numeric types.
            throw new ArgumentException("F64 can only be compared against another F64.");
        }
    }
}