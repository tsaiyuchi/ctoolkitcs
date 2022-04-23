using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.Web
{
    public class CtkWebUtil
    {
        /// <summary>
        /// 下載檔案
        /// </summary>
        public static void DownloadFile(System.Web.HttpContext context, string fileName, byte[] fileContent)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/save-as";
            context.Response.AddHeader("Content-Disposition", "attachment; filename="
                     + System.Web.HttpUtility.UrlEncode(fileName));

            //page.Response.Charset = "UTF-8";
            //page.Response.AddHeader("charset", "UTF-8");


            //If you need no-cache page, then uncomment.
            //page.Response.Buffer = true;
            //page.Response.Expires = 0;
            //page.Response.CacheControl = "no-cache";
            //page.Response.AddHeader("Pragma", "No-Cache");


            context.Response.OutputStream.Write(fileContent, 0, fileContent.Length);
        }
        public static void DownloadFile(System.Web.HttpContext context, string fileName, System.IO.Stream stream)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/save-as";
            context.Response.AddHeader("Content-Disposition", "attachment; filename="
                     + System.Web.HttpUtility.UrlEncode(fileName));
            byte[] buffer = new byte[2048];
            int bufCount = 0;

            while ((bufCount = stream.Read(buffer, 0, buffer.Length)) > 0)
            { context.Response.OutputStream.Write(buffer, 0, bufCount); }
        }
        public static void DownloadFile(System.Web.HttpContext context, string fileName, string filePath)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/save-as";
            context.Response.AddHeader("Content-Disposition", "attachment; filename="
                     + System.Web.HttpUtility.UrlEncode(fileName));


            context.Response.WriteFile(filePath);
        }


        /// <summary>
        /// 參考用
        /// </summary>
        public void GetUploadFile()
        {
            System.Web.HttpFileCollection files = System.Web.HttpContext.Current.Request.Files;
            System.Text.StringBuilder strMsg = new System.Text.StringBuilder();
            for (int iFile = 0; iFile < files.Count; iFile++)
            {
                bool fileOK = false;
                System.Web.HttpPostedFile postedFile = files[iFile];
                string fileName, fileExtension;
                fileName = System.IO.Path.GetFileName(postedFile.FileName);
                if (fileName != "")
                {
                    fileExtension = System.IO.Path.GetExtension(fileName);
                    String[] allowedExtensions = { ".doc", ".xls", ".rar", ".zip", ".wps", ".txt", "docx", "pdf", "xls", ".jpg" };
                    for (int i = 0; i < allowedExtensions.Length; i++)
                    {
                        if (fileExtension == allowedExtensions[i])
                        {
                            fileOK = true;
                            break;
                        }
                    }
                    //if (!fileOK) Label1.Text = "不支援此類型" + fileName;
                }
                if (fileOK)
                {
                    //postedFile.SaveAs(System.Web.HttpContext.Current.Request.MapPath("~/news/" + folder + "/attachment/") + fileName);
                    //postedFile.SaveAs(Server.MapPath("upload") + "/qq.jpg");
                    //if (attachment_filename == "") attachment_filename = fileName;
                    //else attachment_filename = attachment_filename + "|" + fileName;
                }
            }

        }


        public static void AlertMessage(System.Web.UI.Page ctrl, string strMessage)
        {
            //this.ClientScript.RegisterClientScriptBlock(this.GetType(), "msg", "<script>alert('" + strMessage + "');</script>");
            //System.Web.UI.ScriptManager.RegisterStartupScript(ctrl, ctrl.GetType(), "msg", "alert('" + strMessage + "');", true);
        }


        /// <summary>
        /// 設置Control的Click事件時, 會先Confirm
        /// </summary>
        public static void ClickSendValidConfirmDisabled(System.Web.UI.Page page, System.Web.UI.WebControls.WebControl ctrl, string confirmText)
        {
            string clientScript = ctrl.ClientID + ".disabled='disabled';" +
                     page.ClientScript.GetPostBackEventReference(ctrl, null) + ";";

            bool needValid = false;
            string validationGroup = null;
            if (ctrl is System.Web.UI.WebControls.IButtonControl)
            {
                System.Web.UI.WebControls.IButtonControl ibtnCtrl = ctrl as System.Web.UI.WebControls.IButtonControl;
                needValid = ibtnCtrl.CausesValidation;
                validationGroup = ibtnCtrl.ValidationGroup;
            }

            clientScript = "if(confirm('" + confirmText + "')){" + clientScript + "}";

            if (needValid)
            { clientScript = "if(Page_ClientValidate('" + validationGroup + "')){" + clientScript + "}"; }

            ctrl.Attributes["onclick"] = "try{" + clientScript + " }catch(ex){alert(ex.message);} return false;";
        }

        public static void ClickSendValidDisabled(System.Web.UI.Page page, System.Web.UI.WebControls.WebControl ctrl)
        {
            string clientScript = ctrl.ClientID + ".disabled='disabled';" +
                     page.ClientScript.GetPostBackEventReference(ctrl, null) + ";";

            bool needValid = false;
            string validationGroup = null;
            if (ctrl is System.Web.UI.WebControls.IButtonControl)
            {
                System.Web.UI.WebControls.IButtonControl ibtnCtrl = ctrl as System.Web.UI.WebControls.IButtonControl;
                needValid = ibtnCtrl.CausesValidation;
                validationGroup = ibtnCtrl.ValidationGroup;
            }
            if (needValid)
            { clientScript = "if(Page_ClientValidate('" + validationGroup + "')){" + clientScript + "}"; }

            ctrl.Attributes["onclick"] = "try{" + clientScript + " }catch(ex){alert(ex.message);} return false;";
        }

        public static void ClickSendDisabled(System.Web.UI.Page page, System.Web.UI.WebControls.WebControl ctrl)
        {
            string clientScript = ctrl.ClientID + ".disabled='disabled';" +
                       page.ClientScript.GetPostBackEventReference(ctrl, null) + ";";
            ctrl.Attributes["onclick"] = "try{" + clientScript + " }catch(ex){alert(ex.message);} return false;";
        }


    }
}
