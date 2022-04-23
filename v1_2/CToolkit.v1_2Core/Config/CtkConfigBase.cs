using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_2Core.Config
{
    public abstract class CtkConfigBase
    {

        public void SaveXml(string fn) { CtkUtil.SaveXmlToFile(this, fn); }

        public static T LoadXml<T>(string fn) where T : class, new() { return CtkUtil.LoadXmlFromFileOrDefault<T>(fn); }


    }
}
