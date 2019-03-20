using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace CToolkit.v1_0.Web
{
    public class WcViewEditTextBox : System.Web.UI.WebControls.TextBox, IViewEditControl
    {
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (this.m_veMode == ViewEditControlMode.View)
            { writer.Write(this.Text); }
            else
            { base.Render(writer); }
        }

        #region IViewEditControl

        ViewEditControlMode m_veMode = ViewEditControlMode.Edit;
        void IViewEditControl.SetVeMode(ViewEditControlMode mode)
        { this.m_veMode = mode; }
        ViewEditControlMode IViewEditControl.GetVeMode()
        { return this.m_veMode; }

        #endregion
    }
}