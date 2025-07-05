using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra.Complex;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CToolkitCs.v1_2Core.Numeric
{


    public class CtkNumContext
    {
        public bool IsUseCudafy = true;







        public Complex[] OpFftForward(double[] input)
        {
            var rtn = CtkNumUtil.ToSysComplex(input);
            return this.OpFftForward(rtn);
        }

        public Complex[] OpFftForward(Complex[] input)
        {
            var rtn = (Complex[])input.Clone();
            Fourier.Forward(rtn, FourierOptions.Default);
            return rtn;
        }



        /// <summary>
        /// Return 正確的振幅, 注意 x 軸 Mag 左右對稱
        /// </summary>
        public Complex[] OpSpectrumFft(Complex[] fft)
        {
            var rtn = new DenseVector(fft);//it is Clone
            var scale = 2.0 / fft.Length;// Math.Net 要選 Matlab FFT 才會用這個
            rtn *= scale;
            return rtn.ToArray();
        }



        public Complex[] OpSpectrumFftHalf(Complex[] fft)
        {
            var rtn = new Complex[fft.Length / 2];
            var scale = 2.0 / fft.Count();// Math.Net 要選 Matlab FFT 才會用這個

            for (int idx = 0; idx < rtn.Length; idx++)
                rtn[idx] = fft[idx] * scale;
            return rtn;
        }


        public Complex[] SpectrumTime(double[] time)
        {
            var fft = this.OpFftForward(time);
            return this.OpSpectrumFft(fft);
        }

        public Complex[] SpectrumTime(Complex[] time)
        {
            var fft = this.OpFftForward(time);
            return this.OpSpectrumFft(fft);
        }

        public double[] SpectrumTimeMag(double[] time)
        {
            var fft = this.OpFftForward(time);
            var sepctrum = this.OpSpectrumFft(fft);
            return CtkNumUtil.ToMagnitude(sepctrum);
        }


        public Complex[] SpectrumTimeHalf(double[] time)
        {
            var fft = this.OpFftForward(time);
            return this.OpSpectrumFftHalf(fft);
        }
        public Complex[] SpectrumTimeHalf(Complex[] time)
        {
            var fft = this.OpFftForward(time);
            return this.OpSpectrumFftHalf(fft);
        }

        public double[] SpectrumTimeHalfMag(double[] time)
        {
            var fft = this.OpFftForward(time);
            var sepctrum = this.OpSpectrumFftHalf(fft);
            return CtkNumUtil.ToMagnitude(sepctrum);
        }



        #region Static

        static Dictionary<string, CtkNumContext> singletonMapper = new Dictionary<string, CtkNumContext>();
        public static CtkNumContext GetOrCreate(string key = "")
        {
            if (singletonMapper.ContainsKey(key)) return singletonMapper[key];
            var rs = new CtkNumContext();
            singletonMapper[key] = rs;
            return rs;
        }

        #endregion



    }
}
