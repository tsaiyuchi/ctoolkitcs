using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkitCs.v1_2.ContextFlow
{
    /// <summary>
    /// 建議:
    ///     RunLoop call RunOnce call RunSelf(自定)
    ///     RunLoopStart call RunSelf(自定)
    /// </summary>
    public interface ICtkContextFlowRun : ICtkContextFlowRunStart
    {

        /// <summary>
        /// 會執行一次特定功能的method
        /// Exec: 執行特定功能, 若有需要, 可自行重複執行此作業
        /// </summary>
        /// <returns></returns>
        int CtkCfRunOnce();

        /// <summary>
        /// 會持續執行特定功能的method
        /// Run: 持續跑下去, 被呼叫後會留在這個method直到結束
        /// 若不做事, 請直接return
        /// </summary>
        /// <returns></returns>
        int CtkCfRunLoop();

    }
}
