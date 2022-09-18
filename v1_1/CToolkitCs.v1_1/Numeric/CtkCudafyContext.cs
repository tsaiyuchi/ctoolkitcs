using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using Cudafy.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkitCs.v1_1.Numeric
{
    public class CtkCudafyContext : IDisposable
    {
        CudafyModule m_km;
        public CudafyModule Km { get { if (m_km == null) this.Init(); return m_km; } }
        GPGPU m_gpu;
        public GPGPU Gpu { get { if (this.m_gpu == null) this.Init(); return m_gpu; } }





        ~CtkCudafyContext() { this.Dispose(false); }


        public int Init()
        {

            this.m_km = CudafyTranslator.Cudafy();

            CudafyModes.Target = eGPUType.Cuda;
            var tgCount = CudafyHost.GetDeviceCount(CudafyModes.Target);


            if (tgCount <= 0)
            {
                CudafyModes.Target = eGPUType.OpenCL;
                tgCount = CudafyHost.GetDeviceCount(CudafyModes.Target);
            }

            if (tgCount <= 0)
            {
                CudafyModes.Target = eGPUType.Emulator;
                tgCount = CudafyHost.GetDeviceCount(CudafyModes.Target);
            }


            if (tgCount <= 0)
                throw new CtkCudafyCannotUseException("無法使用Cudafy");

            for (int idx = 0; idx < tgCount; idx++)
            {
                try
                {
                    this.m_gpu = CudafyHost.GetDevice(CudafyModes.Target, idx);
                    this.m_gpu.LoadModule(Km);
                    return 0;
                }
                catch (Cudafy.CudafyCompileException) { }
            }

            throw new Exception("Cudafy buidling fail.");

        }





        public ComplexD[] FftForward(ComplexD[] input)
        {
            var dev_cm = Gpu.CopyToDevice(input);

            var ifftData = new ComplexD[input.Length];
            var dev_ifftData = Gpu.CopyToDevice(ifftData);
            Cudafy.Maths.FFT.GPGPUFFT gpuFFT = Cudafy.Maths.FFT.GPGPUFFT.Create(Gpu);
            Cudafy.Maths.FFT.FFTPlan1D fft_1d = gpuFFT.Plan1D(
                Cudafy.Maths.FFT.eFFTType.Complex2Complex,
                Cudafy.Maths.FFT.eDataType.Double,
                input.Length,
                1);

            fft_1d.Execute<ComplexD, ComplexD>(dev_cm, dev_ifftData, true);
            Gpu.CopyFromDevice(dev_ifftData, ifftData);

            return ifftData;
        }



        #region Dispose

        bool disposed = false;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any managed objects here.
            }

            // Free any unmanaged objects here.
            //
            this.DisposeSelf();
            disposed = true;
        }




        public void DisposeSelf()
        {

        }

        #endregion

    }
}
