using System.IO;
using System.Runtime.InteropServices.Automation;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using UniCloud.Fleet.Models;

namespace UniCloud.Fleet.Services
{
    public class AttachmentOperation
    {
        private const string Path = @"c:\UniCloud\AttachDoc\";

        #region 上传附件

        public static void UploadAttachment<T>(T obj)
        {
            var ofd = new OpenFileDialog {Multiselect = false, Filter = "可用文件|*.docx;*.xls;*.xlsx;*.doc;*.pdf"};
            if (ofd.ShowDialog() == true)
            {
                Stream fs = ofd.File.OpenRead();
                var buffbyte = new byte[fs.Length];
                fs.Read(buffbyte, 0, (int)fs.Length);
                fs.Close();
                if (obj is Plan)
                {
                    var plan = obj as Plan;
                    plan.AttachDoc = buffbyte;
                    plan.AttachDocFileName = ofd.File.Name;
                }
                else if (obj is Request)
                {
                    var req = obj as Request;
                    req.AttachDoc = buffbyte;
                    req.AttachDocFileName = ofd.File.Name;
                }
                else
                {
                    var approvalDoc = obj as ApprovalDoc;
                    if (approvalDoc != null)
                    {
                        approvalDoc.AttachDoc = buffbyte;
                        approvalDoc.ApprovalDocFileName = ofd.File.Name;
                    }
                }
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
            if (obj is Request)
            {
                var req = obj as Request;
                filename = Path + Regex.Replace(req.AttachDocFileName, @"[^\w\.]", "");
                buffbyte = req.AttachDoc;
            }
            else if (obj is Plan)
            {
                var plan = obj as Plan;
                filename = Path + Regex.Replace(plan.AttachDocFileName, @"[^\w\.]", "");
                buffbyte = plan.AttachDoc;
            }
            else
            {
                var approvalDoc = obj as ApprovalDoc;
                filename = Path + Regex.Replace(approvalDoc.ApprovalDocFileName, @"[^\w\.]", "");
                buffbyte = approvalDoc.AttachDoc;
            }

            try
            {
                var fs = new FileStream(filename, FileMode.Create);
                if (obj is Request)
                {
                    var req = obj as Request;
                    fs.Write(buffbyte, 0, req.AttachDoc.Length);
                }
                else if (obj is Plan)
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
            catch (IOException)
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
