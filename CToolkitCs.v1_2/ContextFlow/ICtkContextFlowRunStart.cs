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
    public interface ICtkContextFlowRunStart : ICtkContextFlow
    {
        /*[d20210714] RunOnce, RunLoop 以及 RunLoopStart.
         * 在介面中只定義各Method存在, 宣告該Method的用途
         * 但實際用途是實作者決定, 實作者應自行注意如何符合定義.
         */

        bool CtkCfIsRunning { get; set; }


        /// <summary>
        /// 會持續執行特定功能的method
        /// 需實作非同步作業, e.q. 開啟一個Thread/Task後離開函式
        /// </summary>
        /// <returns></returns>
        int CtkCfRunLoopStart();
    }
}
