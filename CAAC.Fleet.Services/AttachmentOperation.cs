using System.IO;
using System.Runtime.InteropServices.Automation;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using UniCloud.Fleet.Models;

namespace CAAC.Fleet.Services
{
    public class AttachmentOperation
    {
        private static string path = @"c:\UniCloud\AttachDoc\";

        #region 上传附件

        public static void UploadAttachment<T>(T obj)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "可用文件|*.docx;*.doc;*.pdf";
            if (ofd.ShowDialog() == true)
            {
                Stream fs = ofd.File.OpenRead();
                byte[] buffbyte = new byte[fs.Length];
                fs.Read(buffbyte, 0, (int)fs.Length);
                fs.Close();
                if (obj.GetType() == typeof(Request))
                {
                    var req = obj as Request;
                    req.AttachDoc = buffbyte;
                    req.AttachDocFileName = ofd.File.Name;
                }
                else if (obj.GetType() == typeof(Plan))
                {
                    var plan = obj as Plan;
                    plan.AttachDoc = buffbyte;
                    plan.AttachDocFileName = ofd.File.Name;
                }
                else
                {
                    var approvalDoc = obj as ApprovalDoc;
                    approvalDoc.AttachDoc = buffbyte;
                    approvalDoc.ApprovalDocFileName = ofd.File.Name;
                }
            }
            else
            {
                return;
            }
        }

        #endregion

        #region 打开附件

        /// <summary>
        /// 打开附件
        /// </summary>
        /// <typeparam name="T">附件类别</typeparam>
        /// <param name="obj"></param>
        public static void OpenAttachment<T>(T obj)
        {
            string filename;
            if (!Directory.Exists("C:\\UniCloud\\AttachDoc"))
            {
                // 文件夹不存在则创建
                Directory.CreateDirectory("C:\\UniCloud\\AttachDoc");
            }

            byte[] buffbyte;
            FileStream fs;
            if (obj.GetType() == typeof(Request))
            {
                var req = obj as Request;
                filename = path + Regex.Replace(req.AttachDocFileName, @"[^\w\.]", "");
                buffbyte = (byte[])(req.AttachDoc);
            }
            else if (obj.GetType() == typeof(Plan))
            {
                var plan = obj as Plan;
                filename = path + Regex.Replace(plan.AttachDocFileName, @"[^\w\.]", "");
                buffbyte = (byte[])(plan.AttachDoc);
            }
            else
            {
                var approvalDoc = obj as ApprovalDoc;
                filename = path + Regex.Replace(approvalDoc.ApprovalDocFileName, @"[^\w\.]", "");
                buffbyte = (byte[])(approvalDoc.AttachDoc);
            }

            try
            {
                fs = new FileStream(filename, FileMode.Create);
                if (obj.GetType() == typeof(Request))
                {
                    var req = obj as Request;
                    fs.Write(buffbyte, 0, req.AttachDoc.Length);
                }
                else if (obj.GetType() == typeof(Plan))
                {
                    var plan = obj as Plan;
                    fs.Write(buffbyte, 0, plan.AttachDoc.Length);
                }
                else
                {
                    var approvalDoc = obj as ApprovalDoc;
                    fs.Write(buffbyte, 0, approvalDoc.AttachDoc.Length);
                }
                fs.Close();

                using (dynamic shell = AutomationFactory.CreateObject("WScript.Shell"))
                {
                    if (File.Exists(filename))
                    {
                        shell.Run(filename);
                    }
                }
            }
            catch (System.IO.IOException)
            {
                using (dynamic shell = AutomationFactory.CreateObject("WScript.Shell"))
                {
                    if (File.Exists(filename))
                    {
                        shell.Run(filename);
                    }
                }
            }

        }

        #endregion

    }
}
