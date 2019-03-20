using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0
{
    public interface ICtkContextFlowRun : ICtkContextFlow
    {
        bool CfIsRunning { get; set; }

        /// <summary>
        /// 會執行一次特定功能的method
        /// Exec: 執行特定功能, 若有需要, 可自行重複執行此作業
        /// </summary>
        /// <returns></returns>
        int CfExec();

        /// <summary>
        /// 會持續執行特定功能的method
        /// Run: 持續跑下去, 被呼叫後會留在這個method直到結束
        /// 若不做事, 請直接return
        /// </summary>
        /// <returns></returns>
        int CfRun();

        /// <summary>
        /// 會持續執行特定功能的method
        /// 需實作非同步作業, e.q. 開啟一個Thread/Task後離開函式
        /// </summary>
        /// <returns></returns>
        int CfRunAsyn();
    }
}
