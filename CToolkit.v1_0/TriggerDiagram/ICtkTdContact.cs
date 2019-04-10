using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.TriggerDiagram
{
    public interface ICtkTdContact
    {
        string CtkTdNodeIdentifier { get; set; }
        string CtkTdFieldName { get; set; }
    }
}
