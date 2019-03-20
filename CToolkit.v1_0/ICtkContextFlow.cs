using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0
{

    /// <summary>

    /// </summary>
    public interface ICtkContextFlow
    {
        int CfInit();
        int CfLoad();
        int CfUnLoad();
        int CfFree();
    }




    /// <Note>
    /// 若與IDisposable並用, 需注意CfFree的定義
    /// IDispose.Dispose() 一般會執行自己的回收 GC.SuppressFinalize(this);
    /// 也就是真正釋放自己
    /// 
    /// 那CfFree應該釋放誰? Member 還是該包含 'this' ?
    /// 
    /// (1)
    /// 邏輯上來講, CfFree還在 'this' 程序中(雖然Dispose也是) 因此應該只釋放Member
    /// 因此解構子通常也是寫 this.Dispose(false);
    /// 不對自己進行回收, 畢竟解構子己在回收程序中
    /// 
    /// (2)
    /// 而Member釋放完以後, 其實也不應該留下Resource, 應該是會自然被GC回收
    /// 
    /// =>即
    /// CfFree 一般只要像解構子一樣, 釋放Member後, 由GC自行回收
    /// 而 Dispose(void) 則是給外面程式使用
    /// 
    /// </Note>
}
