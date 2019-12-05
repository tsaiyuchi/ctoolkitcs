using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_0.Config
{
    public abstract class CtkConfigBase
    {

        public void SaveXml(string fn) { CtkUtil.SaveToXmlFile(this, fn); }

        public static T LoadXml<T>(string fn) where T : class, new() { return CtkUtil.LoadXmlOrNew<T>(fn); }


    }
}
