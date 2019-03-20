using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace CToolkit.v1_0.Web
{
    //模式
    public enum ViewEditControlMode
    {
        Edit,
        View
    }
    //Interface
    public interface IViewEditControl
    {
        void SetVeMode(ViewEditControlMode mode);
        ViewEditControlMode GetVeMode();
    }

    public class ViewEditControl
    {
        //Set Mode
        public static void SetVeMode(ViewEditControlMode mode, System.Web.UI.Control ctrl)
        {
            foreach (System.Web.UI.Control loop in ctrl.Controls)
            {
                if (loop is IViewEditControl)
                { (loop as IViewEditControl).SetVeMode(mode); }
                SetVeMode(mode, loop);
            }
        }
    }


}