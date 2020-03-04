using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.TriggerDiagram
{
    public interface ICtkTdNode
    {
        /// <summary>
        /// 唯一識別碼
        /// </summary>
        String CtkTdIdentifier { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        String CtkTdName { get; set; }

    }
}
