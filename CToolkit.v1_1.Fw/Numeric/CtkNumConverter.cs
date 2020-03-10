using Cudafy.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace CToolkit.v1_1.Numeric
{
    public class CtkNumConverter
    {
        public static ComplexD[] ToCudafy(Complex[] input)
        {
            var result = new ComplexD[input.Length];
            ToCudafy(input, result);
            return result;
        }
        public static ComplexD[] ToCudafy(IEnumerable<Complex> input)
        {
            var result = new ComplexD[input.Count()];
            ToCudafy(input.ToArray(), result);
            return result;
        }
        public static void ToCudafy(Complex[] input, ComplexD[] output) { CtkMemcpyUtil.StructsCopy(input, output); }

        public static ComplexD[] ToCudafy(double[] input)
        {
            var result = new ComplexD[input.Length];
            ToCudafy(input, result);
            return result;
        }
        public static ComplexD[] ToCudafy(IEnumerable<double> input)
        {
            var result = new ComplexD[input.Count()];
            ToCudafy(input.ToArray(), result);
            return result;
        }
        public static void ToCudafy(double[] input, ComplexD[] output)
        {
            for (int idx = 0; idx < input.Length; idx++)
                output[idx] = new ComplexD(input[idx], 0);
        }

        public static double[] ToImginary(Complex[] input)
        {
            var result = new double[input.Length];
            ToImgine(input, result);
            return result;
        }
        public static double[] ToImginary(IEnumerable<Complex> input) { return ToImginary(input.ToArray()); }
        public static double[] ToImginary(ComplexD[] input)
        {
            var result = new double[input.Length];
            ToImginary(input, result);
            return result;
        }

        public static double[] ToImginary(IEnumerable<ComplexD> input) { return ToImginary(input.ToArray()); }
        public static void ToImginary(ComplexD[] input, double[] output)
        {
            for (int idx = 0; idx < input.Length; idx++)
            {
                var val = input[idx];
                output[idx] = val.y;
            }
        }
        public static void ToImgine(Complex[] input, double[] output)
        {
            for (int idx = 0; idx < input.Length; idx++)
                output[idx] = input[idx].Imaginary;
        }

        public static double[] ToMagnitude(Complex[] input)
        {
            var result = new double[input.Length];
            ToMagnitude(input, result);
            return result;
        }
        public static double[] ToMagnitude(IEnumerable<Complex> input) { return ToMagnitude(input.ToArray()); }
        public static void ToMagnitude(Complex[] input, double[] output)
        {
            for (int idx = 0; idx < input.Length; idx++)
                output[idx] = input[idx].Magnitude;
        }

        public static double[] ToMagnitude(ComplexD[] input)
        {
            var result = new double[input.Length];
            ToMagnitude(input, result);
            return result;
        }
        public static double[] ToMagnitude(IEnumerable<ComplexD> input) { return ToMagnitude(input.ToArray()); }
        public static void ToMagnitude(ComplexD[] input, double[] output)
        {
            for (int idx = 0; idx < input.Length; idx++)
            {
                var val = input[idx];
                output[idx] = Math.Sqrt(val.x * val.x + val.y * val.y);
            }
        }

        public static double[] ToReal(Complex[] input)
        {
            var result = new double[input.Length];
            ToReal(input, result);
            return result;
        }
        public static double[] ToReal(IEnumerable<Complex> input) { return ToReal(input.ToArray()); }
        public static void ToReal(Complex[] input, double[] output)
        {
            for (int idx = 0; idx < input.Length; idx++)
                output[idx] = input[idx].Real;
        }

        public static double[] ToReal(ComplexD[] input)
        {
            var result = new double[input.Length];
            ToReal(input, result);
            return result;
        }
        public static double[] ToReal(IEnumerable<ComplexD> input) { return ToReal(input.ToArray()); }
        public static void ToReal(ComplexD[] input, double[] output)
        {
            for (int idx = 0; idx < input.Length; idx++)
            {
                var val = input[idx];
                output[idx] = val.x;
            }
        }

        public static Complex[] ToSysComplex(ComplexD[] input)
        {
            var result = new Complex[input.Length];
            ToSysComplex(input, result);
            return result;
        }
        public static Complex[] ToSysComplex(IEnumerable<ComplexD> input)
        {
            var result = new Complex[input.Count()];
            ToSysComplex(input.ToArray(), result);
            return result;
        }
        public static void ToSysComplex(ComplexD[] input, Complex[] output) { CtkMemcpyUtil.StructsCopy(input, output); }

        public static Complex[] ToSysComplex(double[] input)
        {
            var result = new Complex[input.Length];
            ToSysComplex(input, result);
            return result;
        }
        public static Complex[] ToSysComplex(IEnumerable<double> input)
        {
            var result = new Complex[input.Count()];
            ToSysComplex(input.ToArray(), result);
            return result;
        }
        public static void ToSysComplex(double[] input, Complex[] output)
        {
            for (int idx = 0; idx < input.Length; idx++)
                output[idx] = new Complex(input[idx], 0);
        }

        public static Complex ToSysComplex(ComplexD d) { return new Complex(d.x, d.y); }
        public static Complex ToSysComplex(ComplexF d) { return new Complex(d.x, d.y); }



    }
}
