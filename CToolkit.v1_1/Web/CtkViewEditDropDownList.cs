using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CToolkit.v1_1.Web
{
    //繼承DropDownList, 實作IViewEditControl
    public class WcViewEditDropDownList : System.Web.UI.WebControls.DropDownList, IViewEditControl
    {
        //重點, 覆寫Render
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            //若是View, 直接輸出
            if (this.m_veMode == ViewEditControlMode.View)
            { writer.Write(this.SelectedItem.Text); }
            else//否則執行原本的Render
            { base.Render(writer); }
        }

        #region IViewEditControl
        //要儲存Mode
        ViewEditControlMode m_veMode = ViewEditControlMode.Edit;
        void IViewEditControl.SetVeMode(ViewEditControlMode mode) { this.m_veMode = mode; }
        ViewEditControlMode IViewEditControl.GetVeMode() { return this.m_veMode; }
        #endregion
    }
}