using Cudafy.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CToolkit.v1_0.Numeric
{


    public class CtkNumContext
    {
        protected CtkCudafyContext m_cudafyContext = new CtkCudafyContext();
        public bool IsUseCudafy = true;
        public CtkCudafyContext CudafyContext { get { return m_cudafyContext; } }



        public Complex[] FftForward(Complex[] input)
        {
            if (IsUseCudafy)
            {
                var fftD = this.FftForwardJustD(CtkNumConverter.ToCudafy(input));
                if (fftD != null)
                    return CtkNumConverter.ToSysComplex(fftD);
            }


            var result = new Complex[input.Length];
            Array.Copy(input, result, input.Length);
            MathNet.Numerics.IntegralTransforms.Fourier.Forward(result
                , MathNet.Numerics.IntegralTransforms.FourierOptions.Matlab);
            return result;
        }

        public Complex[] FftForward(double[] input)
        {
            var result = new Complex[input.Length];
            for (int idx = 0; idx < result.Length; idx++)
                result[idx] = new Complex(input[idx], 0);

            return this.FftForward(result);
        }

        public Complex[] FftForward(IEnumerable<double> input) { return this.FftForward(CtkNumConverter.ToSysComplex(input)); }

        public Complex[] FftForward(IEnumerable<Complex> input) { return this.FftForward(input.ToArray()); }

        /// <summary>
        /// FFT: 
        /// fft(wave1->wave2) = fft(wave1->0) + fft(wave2->0)
        /// fft(wave1->wave2) = fft(wave1->0) + fft(0->wave2)
        /// fft(wave1+wave2) = fft(wave1) + fft(wave2)
        /// fft(wave1->wave2) = 0.5 * fft(wave1) + 0.5 * fft(wave2) @ matlab FFT
        /// </summary>
        public ComplexD[] FftForwardD(ComplexD[] input)
        {
            if (IsUseCudafy)
            {
                var fftD = this.FftForwardJustD(input);
                if (fftD != null)
                    return fftD;
            }

            var result = CtkNumConverter.ToSysComplex(input);
            MathNet.Numerics.IntegralTransforms.Fourier.Forward(result);
            return CtkNumConverter.ToCudafy(result);
        }

        public ComplexD[] FftForwardD(IEnumerable<double> input) { return FftForwardD(CtkNumConverter.ToCudafy(input)); }

        public ComplexD[] FftForwardD(IEnumerable<ComplexD> input) { return FftForwardD(input.ToArray()); }

        public ComplexD[] FftForwardD(IEnumerable<Complex> input) { return FftForwardD(CtkNumConverter.ToCudafy(input)); }

        /// <summary>
        /// Return 正確的振幅, 注意 x 軸 Mag 左右對稱
        /// </summary>
        public List<Complex> SpectrumFft(IEnumerable<Complex> fft)
        {
            var result = new List<Complex>();
            var scale = 2.0 / fft.Count();// Math.Net 要選 Matlab FFT 才會用這個
            foreach (var val in fft)
                result.Add(new Complex(val.Real * scale, val.Imaginary * scale));
            return result;
        }

        public List<ComplexD> SpectrumFftD(IEnumerable<ComplexD> fft)
        {
            var result = new List<ComplexD>();
            var scale = 2.0 / fft.Count();// Math.Net 要選 Matlab FFT 才會用這個
            foreach (var val in fft)
                result.Add(new ComplexD(val.x * scale, val.y * scale));
            return result;
        }

        public List<Complex> SpectrumHalfFft(IEnumerable<Complex> fft)
        {
            var result = new List<Complex>();
            var scale = 2.0 / fft.Count();// Math.Net 要選 Matlab FFT 才會用這個
            var ary = fft.ToArray();
            for (int idx = 0; idx < ary.Length / 2; idx++)
                result.Add(ary[idx] * scale);
            return result;
        }

        public List<ComplexD> SpectrumHalfFftD(IEnumerable<ComplexD> fft)
        {
            var result = new List<ComplexD>();
            var scale = 2.0 / fft.Count();// Math.Net 要選 Matlab FFT 才會用這個
            var ary = fft.ToArray();
            for (int idx = 0; idx < ary.Length / 2; idx++)
                result.Add(new ComplexD(ary[idx].x * scale, ary[idx].y * scale));
            return result;
        }

        public List<Complex> SpectrumTime(IEnumerable<double> time)
        {
            var fft = this.FftForward(time);
            return this.SpectrumFft(fft);
        }

        public List<Complex> SpectrumTime(IEnumerable<Complex> time)
        {
            var fft = this.FftForward(time);
            return this.SpectrumFft(fft);
        }

        ComplexD[] FftForwardJustD(ComplexD[] input)
        {
            try
            {
                return this.CudafyContext.FftForward(input);
            }
            catch (CtkCudafyCannotUseException ex)
            {
                IsUseCudafy = false;
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            catch (Cudafy.CudafyCompileException ex)
            {
                IsUseCudafy = false;
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                IsUseCudafy = false;
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            catch (NotSupportedException ex)
            {
                IsUseCudafy = false;
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            return null;
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
