using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using UniCloud.Fleet.Models;
using UniCloud.DatabaseHelper;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Threading;

namespace UniCloud.AFRP.Initializer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loading();
            log = LogManager.GetCurrentClassLogger();
            listAir.ItemsSource = airlines;
        }

        private static Logger log;
        private DateTime? nullDt = null;

        private bool isEnabledOp,
                     isEnabledReq,
                     isEnabledAppr,
                     isEnabledOwn,
                     isOpCodeGeted,
                     isReqCodeGeted,
                     isApprCodeGeted,
                     isOwnCodeGeted;

        private string opFileName,
                       reqFileName,
                       apprFileName,
                       ownFileName,
                       icaoCodeOp,
                       icaoCodeReq,
                       icaoCodeAppr,
                       icaoCodeOwn;

        private List<Airlines> airlines = new List<Airlines>();
        private Databasehelper dbHelper = new Databasehelper();

        private void Loading()
        {
            var target = new WpfRichTextBoxTarget();
            target.Name = "console";
            target.Layout = "${longdate:useUTC=true}|${level:uppercase=true}|::${message}";
            target.ControlName = "rtb";
            target.FormName = "MonitorWindow";
            target.AutoScroll = true;
            target.MaxLines = 100000;
            target.UseDefaultRowColoringRules = true;
            AsyncTargetWrapper asyncWrapper = new AsyncTargetWrapper();
            asyncWrapper.Name = "console";
            asyncWrapper.WrappedTarget = target;
            SimpleConfigurator.ConfigureForTargetLogging(asyncWrapper, LogLevel.Trace);

            // 获取航空公司ICAO代码列表
            using (var db = new FleetEntities(Conn.Default))
            {
                airlines = db.Owners.OfType<Airlines>().OrderBy(a => a.Name).ToList();
            }
        }

        /// <summary>
        /// 选择现役飞机文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOp_Click(object sender, RoutedEventArgs e)
        {
            this.isOpCodeGeted = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "CSV文件|*.csv";
            if (ofd.ShowDialog() == true)
            {
                if (!ofd.SafeFileName.StartsWith("现役飞机"))
                {
                    log.Error("所选文件[" + ofd.SafeFileName + "]非现役飞机！");
                    return;
                }
                log.Info("选中现役飞机文件：" + ofd.SafeFileName);
                opFileName = ofd.FileName;
                tbkOpFile.Text = ofd.SafeFileName;
                isEnabledOp = true;
                if (!this.isOpCodeGeted)
                {
                    string[] code = ofd.SafeFileName.Split('.');
                    icaoCodeOp = code[1];
                    this.isOpCodeGeted = true;
                }
            }
            if (isEnabledOp && isEnabledReq && isEnabledAppr && isEnabledOwn)
            {
                btnProcess.IsEnabled = true;
            }
        }

        /// <summary>
        /// 选择申请飞机文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            this.isReqCodeGeted = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "CSV文件|*.csv";
            if (ofd.ShowDialog() == true)
            {
                if (!ofd.SafeFileName.StartsWith("申请飞机"))
                {
                    log.Error("所选文件[" + ofd.SafeFileName + "]非申请飞机！");
                    return;
                }
                log.Info("选中现役飞机文件：" + ofd.SafeFileName);
                reqFileName = ofd.FileName;
                tbkReqFile.Text = ofd.SafeFileName;
                isEnabledReq = true;
                if (!this.isReqCodeGeted)
                {
                    string[] code = ofd.SafeFileName.Split('.');
                    icaoCodeReq = code[1];
                    this.isReqCodeGeted = true;
                }
            }
            if (isEnabledOp && isEnabledReq && isEnabledAppr && isEnabledOwn)
            {
                btnProcess.IsEnabled = true;
            }
        }

        /// <summary>
        /// 选择批文飞机文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAppr_Click(object sender, RoutedEventArgs e)
        {
            this.isApprCodeGeted = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "CSV文件|*.csv";
            if (ofd.ShowDialog() == true)
            {
                if (!ofd.SafeFileName.StartsWith("批文飞机"))
                {
                    log.Error("所选文件[" + ofd.SafeFileName + "]非批文飞机！");
                    return;
                }
                log.Info("选中现役飞机文件：" + ofd.SafeFileName);
                apprFileName = ofd.FileName;
                tbkApprFile.Text = ofd.SafeFileName;
                isEnabledAppr = true;
                if (!this.isApprCodeGeted)
                {
                    string[] code = ofd.SafeFileName.Split('.');
                    icaoCodeAppr = code[1];
                    this.isApprCodeGeted = true;
                }
            }
            if (isEnabledOp && isEnabledReq && isEnabledAppr && isEnabledOwn)
            {
                btnProcess.IsEnabled = true;
            }
        }

        /// <summary>
        /// 选择供应商文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOwn_Click(object sender, RoutedEventArgs e)
        {
            this.isOwnCodeGeted = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "CSV文件|*.csv";
            if (ofd.ShowDialog() == true)
            {
                if (!ofd.SafeFileName.StartsWith("所有权人"))
                {
                    log.Error("所选文件[" + ofd.SafeFileName + "]非所有权人！");
                    return;
                }
                log.Info("选中所有权人文件：" + ofd.SafeFileName);
                ownFileName = ofd.FileName;
                tbkOwnFile.Text = ofd.SafeFileName;
                isEnabledOwn = true;
                if (!this.isOwnCodeGeted)
                {
                    string[] code = ofd.SafeFileName.Split('.');
                    icaoCodeOwn = code[1];
                    this.isOwnCodeGeted = true;
                }
            }
            if (isEnabledOp && isEnabledReq && isEnabledAppr && isEnabledOwn)
            {
                btnProcess.IsEnabled = true;
            }
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            if (!(icaoCodeOp == icaoCodeReq && icaoCodeOp == icaoCodeAppr && icaoCodeOp == icaoCodeOwn))
            {
                log.Error("飞机数据不属于同一家公司！");
                return;
            }

            CreateAircraftData(icaoCodeOp, opFileName, reqFileName, apprFileName, ownFileName);

            this.isEnabledOp = false;
            this.isEnabledReq = false;
            this.isEnabledAppr = false;
            this.isEnabledOwn = false;
            btnProcess.IsEnabled = false;
        }

        /// <summary>
        /// 生成航空公司数据按钮操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAirlines_Click(object sender, RoutedEventArgs e)
        {
            var airline = this.listAir.SelectedItem as Airlines;

            var curAirline = DynamicData.GetAirlines().SingleOrDefault(a => a.ICAOCode == airline.ICAOCode);
            var planID =
                DynamicData.GetPlans().Where(p => p.AirlinesID == curAirline.OwnerID).Select(p => p.PlanID).
                    FirstOrDefault();
            var requestID =
                DynamicData.GetRequests().Where(p => p.AirlinesID == curAirline.OwnerID).Select(r => r.RequestID).
                    FirstOrDefault();

            // 生成航空公司数据
            BuildAirlinesData(curAirline, planID, requestID);

            // 生成测试数据
            //BuildTestData(curAirline.Name);
        }

        /// <summary>
        /// 生成民航局数据按钮操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCAAC_Click(object sender, RoutedEventArgs e)
        {
            // 生成民航局数据
            BuildCAACData();
        }

        /// <summary>
        /// 一次性生成所有航空公司数据按钮操作,
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAllAirlines_Click(object sender, RoutedEventArgs e)
        {
            if (!(sourceFilePath.Text == "" || destFilePath.Text == "" || dbFilePath.Text == ""))
            {
                //遍历所有的航空公司
                foreach (Airlines airline in airlines)
                {
                    //1、从initialDir拷贝初始化数据库到DATA下
                    //断开连接
                    dbHelper.KillSpId("AFRP");
                    //分离数据库
                    dbHelper.SpDatabase("AFRP");
                    System.IO.File.Copy(sourceFilePath.Text + "\\AFRP.mdf", dbFilePath.Text + "\\AFRP.mdf", true);
                    System.IO.File.Copy(sourceFilePath.Text + "\\AFRP_log.LDF", dbFilePath.Text + "\\AFRP_log.LDF", true);
                    //2、附加AFRP数据库
                    dbHelper.AddDataBase("AFRP", dbFilePath.Text + "\\AFRP.mdf");
                    //3、生成航空公司数据
                    var curAirline = DynamicData.GetAirlines().SingleOrDefault(a => a.ICAOCode == airline.ICAOCode);
                    var planID =
                        DynamicData.GetPlans().Where(p => p.AirlinesID == curAirline.OwnerID).Select(p => p.PlanID).
                            FirstOrDefault();
                    var requestID =
                        DynamicData.GetRequests().Where(p => p.AirlinesID == curAirline.OwnerID).Select(r => r.RequestID)
                            .FirstOrDefault();
                    BuildAirlinesData(curAirline, planID, requestID);

                    //4、断开连接,并分离数据库
                    dbHelper.KillSpId("AFRP");
                    dbHelper.SpDatabase("AFRP");
                    //5、拷贝数据库到目标文件夹
                    string destDir1 = String.Format(destFilePath.Text + "\\{0}\\AFRP.mdf", airline.ShortName);
                    string destDir2 = String.Format(destFilePath.Text + "\\{0}\\AFRP_log.LDF", airline.ShortName);
                    System.IO.File.Copy(dbFilePath.Text + "\\AFRP.mdf", destDir1, true);
                    System.IO.File.Copy(dbFilePath.Text + "\\AFRP_log.LDF", destDir2, true);
                    log.Info("拷贝[" + airline.Name + "]数据完成。");
                }
            }
        }




        /// <summary>
        /// 准备初始化飞机数据
        /// </summary>
        /// <param name="airlineCode">航空公司ICAO代码</param>
        /// <param name="opFileName">根据模板维护并已经转化为CSV文件的现役飞机数据</param>
        /// <param name="reqFileName">根据模板维护并已经转化为CSV文件的申请飞机数据</param>
        /// <param name="apprFileName">根据模板维护并已经转化为CSV文件的批文飞机数据</param>
        /// <param name="ownFileName">根据模板维护并已经转化为CSV文件的所有权人数据</param>
        private void CreateAircraftData(string airlineCode, string opFileName, string reqFileName, string apprFileName,
                                        string ownFileName)
        {
            using (var db = new FleetEntities(Conn.Default))
            {
                var owners = db.Owners.ToList();
                var airline = owners.OfType<Airlines>().SingleOrDefault(a => a.ICAOCode == airlineCode);
                var acTypes = db.AircraftTypes.ToList();
                var annuals = db.Annuals.ToList();
                var annual = annuals.SingleOrDefault(a => a.Year == 2012);
                var importCategories = db.ActionCategories.Where(a => a.ActionType == "引进").ToList();

                if (db.Aircrafts.Where(a => a.Airlines.OwnerID == airline.OwnerID).Any())
                {
                    log.Error("[" + airline.Name + "]数据已导入！");
                }
                else
                {
                    log.Info("开始导入[" + airline.Name + "]数据。");

                    #region 将数据导入飞机实体集合

                    // 现役飞机
                    string[] acArrayOp = File.ReadAllLines(opFileName, System.Text.Encoding.Default);
                    var oads =
                        acArrayOp.Skip(1).Where(a => !a.Trim().StartsWith(",")).Select(a => a.Split(',')).Select(
                            ac => new AircraftData
                                      {
                                          AircraftType = acTypes.SingleOrDefault(a => a.Name == ac[0].Trim()),
                                          SerialNumber = ac[1].Trim(),
                                          RegNumber = ac[2].Trim(),
                                          FactoryDate =
                                              string.IsNullOrWhiteSpace(ac[3].Trim())
                                                  ? nullDt
                                                  : DateTime.ParseExact(ac[3].Trim(), "yyyy/M/d",
                                                                        CultureInfo.InvariantCulture),
                                          ImportDate =
                                              string.IsNullOrWhiteSpace(ac[4].Trim())
                                                  ? nullDt
                                                  : DateTime.ParseExact(ac[4].Trim(), "yyyy/M/d",
                                                                        CultureInfo.InvariantCulture),
                                          ImportType =
                                              importCategories.SingleOrDefault(i => i.ActionName == ac[5].Trim()),
                                          TechReceiptDate =
                                              string.IsNullOrWhiteSpace(ac[6].Trim())
                                                  ? nullDt
                                                  : DateTime.ParseExact(ac[6].Trim(), "yyyy/M/d",
                                                                        CultureInfo.InvariantCulture),
                                          ReceiptDate =
                                              string.IsNullOrWhiteSpace(ac[7].Trim())
                                                  ? nullDt
                                                  : DateTime.ParseExact(ac[7].Trim(), "yyyy/M/d",
                                                                        CultureInfo.InvariantCulture),
                                          StartDate =
                                              string.IsNullOrWhiteSpace(ac[8].Trim())
                                                  ? nullDt
                                                  : DateTime.ParseExact(ac[8].Trim(), "yyyy/M/d",
                                                                        CultureInfo.InvariantCulture),
                                          StopDate =
                                              string.IsNullOrWhiteSpace(ac[9].Trim())
                                                  ? nullDt
                                                  : DateTime.ParseExact(ac[9].Trim(), "yyyy/M/d",
                                                                        CultureInfo.InvariantCulture),
                                          TechDeliveryDate =
                                              string.IsNullOrWhiteSpace(ac[10].Trim())
                                                  ? nullDt
                                                  : DateTime.ParseExact(ac[10].Trim(), "yyyy/M/d",
                                                                        CultureInfo.InvariantCulture),
                                          OnHireDate =
                                              string.IsNullOrWhiteSpace(ac[11].Trim())
                                                  ? nullDt
                                                  : DateTime.ParseExact(ac[11].Trim(), "yyyy/M/d",
                                                                        CultureInfo.InvariantCulture),
                                          OwnerNumber = int.Parse(ac[12].Trim()),
                                          SeatingCapacity =
                                              string.IsNullOrWhiteSpace(ac[13].Trim()) ? 0 : int.Parse(ac[13].Trim()),
                                          CarryingCapacity =
                                              string.IsNullOrWhiteSpace(ac[14].Trim())
                                                  ? 0
                                                  : decimal.Parse(ac[14].Trim())
                                      }).ToList();

                    // 申请飞机
                    string[] acArrayReq = File.ReadAllLines(reqFileName, System.Text.Encoding.Default);
                    var rads =
                        acArrayReq.Skip(1).Where(a => !a.Trim().StartsWith(",")).Select(a => a.Split(',')).Select(
                            ac => new RequestData
                                      {
                                          AircraftType = acTypes.SingleOrDefault(a => a.Name == ac[0].Trim()),
                                          ImportType =
                                              importCategories.SingleOrDefault(i => i.ActionName == ac[1].Trim()),
                                          Annual = annuals.SingleOrDefault(a => a.Year == int.Parse(ac[2].Trim())),
                                          Month = string.IsNullOrWhiteSpace(ac[3].Trim()) ? 1 : int.Parse(ac[3].Trim()),
                                          SeatingCapacity =
                                              string.IsNullOrWhiteSpace(ac[4].Trim()) ? 0 : int.Parse(ac[4].Trim()),
                                          CarryingCapacity =
                                              string.IsNullOrWhiteSpace(ac[5].Trim()) ? 0 : decimal.Parse(ac[5].Trim()),
                                          DocNumber = ac[6].Trim()
                                      }).ToList();

                    // 批文飞机
                    string[] acArrayAppr = File.ReadAllLines(apprFileName, System.Text.Encoding.Default);
                    var aads =
                        acArrayAppr.Skip(1).Where(a => !a.Trim().StartsWith(",")).Select(a => a.Split(',')).Select(
                            ac => new ApprovedData
                                      {
                                          AircraftType = acTypes.SingleOrDefault(a => a.Name == ac[0].Trim()),
                                          ImportType =
                                              importCategories.SingleOrDefault(i => i.ActionName == ac[1].Trim()),
                                          Annual = annuals.SingleOrDefault(a => a.Year == int.Parse(ac[2].Trim())),
                                          Month = string.IsNullOrWhiteSpace(ac[3].Trim()) ? 1 : int.Parse(ac[3].Trim()),
                                          SeatingCapacity =
                                              string.IsNullOrWhiteSpace(ac[4].Trim()) ? 0 : int.Parse(ac[4].Trim()),
                                          CarryingCapacity =
                                              string.IsNullOrWhiteSpace(ac[5].Trim()) ? 0 : decimal.Parse(ac[5].Trim()),
                                          ApprovalNumber = ac[6].Trim()
                                      }).ToList();

                    // 所有权人
                    string[] acArrayOwn = File.ReadAllLines(ownFileName, System.Text.Encoding.Default);
                    var aods =
                        acArrayOwn.Skip(1).Where(a => !a.Trim().StartsWith(",")).Select(a => a.Split(',')).Select(
                            ao => new OwnData
                                      {
                                          OwnNumber = int.Parse(ao[0].Trim()),
                                          OwnName = ao[1].Trim().Trim(),
                                          Owntype = int.Parse(ao[2].Trim())
                                      }).ToList();

                    #endregion

                    #region 初始化计划、申请

                    // 创建初始化计划
                    var plan = new Plan
                                   {
                                       PlanID = Guid.NewGuid(),
                                       Airlines = airline,
                                       Annual = annual,
                                       Title = "初始化计划",
                                       VersionNumber = 1,
                                       IsCurrentVersion = true,
                                       IsValid = true,
                                       CreateDate = new DateTime(2012, 1, 1),
                                       SubmitDate = new DateTime(2012, 1, 1),
                                       IsFinished = true,
                                       ManageFlagPnr = true,
                                       ManageFlagCargo = true,
                                       Status = (int)PlanStatus.Submited,
                                       PublishStatus = (int)PlanPublishStatus.Submited,
                                   };
                    db.Plans.Add(plan);

                    // 创建初始化申请
                    var request = new Request
                                      {
                                          RequestID = Guid.NewGuid(),
                                          Airlines = airline,
                                          Title = "初始化申请",
                                          CreateDate = new DateTime(2012, 1, 1),
                                          SubmitDate = new DateTime(2012, 1, 1),
                                          IsFinished = true,
                                          Status = (int)ReqStatus.Submited
                                      };
                    db.Requests.Add(request);

                    #endregion

                    if (aods.Any())
                    {
                        #region 导入所有权人数据

                        aods.ForEach(ad =>
                                         {
                                             if (ad.OwnNumber > 100)
                                             {
                                                 var own = owners.SingleOrDefault(o => o.Name == ad.OwnName);
                                                 own.SupplierType = 1;
                                                 ad.OwnGuID = own.OwnerID;
                                             }
                                             else
                                             {
                                                 var ownerID = Guid.NewGuid();
                                                 // 所有权人
                                                 var own = new Owner
                                                               {
                                                                   OwnerID = ownerID,
                                                                   Name = ad.OwnName,
                                                                   SupplierType = ad.Owntype == 0 ? 0 : ad.Owntype
                                                               };
                                                 db.Owners.Add(own);
                                                 ad.OwnGuID = ownerID;
                                             }
                                         });

                        #endregion
                    }

                    if (oads.Any())
                    {
                        #region 导入现役飞机数据

                        Owner owner;
                        oads.ForEach(ad =>
                                         {
                                             if (ad.OwnerNumber == 0)
                                             {
                                                 owner = airline;
                                             }
                                             else
                                             {
                                                 var ownId =
                                                     aods.SingleOrDefault(a => a.OwnNumber == ad.OwnerNumber).OwnGuID;
                                                 owner = db.Owners.Local.SingleOrDefault(o => o.OwnerID == ownId);
                                             }
                                             // 飞机数据
                                             var ac = new Aircraft
                                                          {
                                                              AircraftID = Guid.NewGuid(),
                                                              AircraftType = ad.AircraftType,
                                                              Owner = owner,
                                                              Airlines = airline,
                                                              ImportCategory = ad.ImportType,
                                                              RegNumber = ad.RegNumber,
                                                              SerialNumber = ad.SerialNumber,
                                                              CreateDate = DateTime.Now,
                                                              FactoryDate = ad.FactoryDate,
                                                              ImportDate = ad.ImportDate,
                                                              IsOperation = true,
                                                              SeatingCapacity = ad.SeatingCapacity,
                                                              CarryingCapacity = ad.CarryingCapacity,
                                                          };
                                             db.Aircrafts.Add(ac);

                                             // 计划飞机数据
                                             var pa = new PlanAircraft
                                                          {
                                                              PlanAircraftID = Guid.NewGuid(),
                                                              Aircraft = ac,
                                                              AircraftType = ad.AircraftType,
                                                              Airlines = airline,
                                                              IsLock = true,
                                                              IsOwn = true,
                                                              Status = (int)ManageStatus.Operation,
                                                          };
                                             db.PlanAircrafts.Add(pa);

                                             // 申请历史
                                             var rh = new ApprovalHistory
                                                          {
                                                              ApprovalHistoryID = Guid.NewGuid(),
                                                              PlanAircraft = pa,
                                                              Request = request,
                                                              ImportCategory = ad.ImportType,
                                                              SeatingCapacity = ad.SeatingCapacity,
                                                              CarryingCapacity = ad.CarryingCapacity,
                                                              Annual = annual,
                                                              RequestDeliverMonth = 1,
                                                              IsApproved = true,
                                                              ManaApprovalHistory =
                                                                  new ManaApprovalHistory { IsApproved = true }
                                                          };
                                             db.ApprovalHistories.Add(rh);

                                             // 运营权历史
                                             var oh = new OperationHistory
                                                          {
                                                              ApprovalHistory = rh,
                                                              Airlines = airline,
                                                              Aircraft = ac,
                                                              ImportCategory = ad.ImportType,
                                                              RegNumber = ac.RegNumber,
                                                              TechReceiptDate = ad.TechReceiptDate,
                                                              ReceiptDate = ad.ReceiptDate,
                                                              StartDate = ad.StartDate,
                                                              StopDate = ad.StopDate,
                                                              TechDeliveryDate = ad.TechDeliveryDate,
                                                              OnHireDate = ad.OnHireDate,
                                                              Status = (int)OpStatus.Submited
                                                          };
                                             db.OperationHistories.Add(oh);

                                             // 计划历史
                                             var ph = new OperationPlan
                                                          {
                                                              PlanHistoryID = Guid.NewGuid(),
                                                              PlanAircraft = pa,
                                                              Plan = plan,
                                                              ApprovalHistory = rh,
                                                              ActionCategory = ad.ImportType,
                                                              TargetCategory = ad.ImportType,
                                                              AircraftType = ad.AircraftType,
                                                              Annual = annual,
                                                              PerformMonth = 1,
                                                              SeatingCapacity = ad.SeatingCapacity,
                                                              CarryingCapacity = ad.CarryingCapacity,
                                                              IsValid = true,
                                                              IsSubmit = true,
                                                              OperationHistory = oh
                                                          };
                                             db.PlanHistories.Add(ph);

                                             // 商业数据历史
                                             db.AircraftBusinesses.Add(new AircraftBusiness
                                                                           {
                                                                               AircraftBusinessID = Guid.NewGuid(),
                                                                               Aircraft = ac,
                                                                               AircraftType = ad.AircraftType,
                                                                               ImportCategory = ad.ImportType,
                                                                               SeatingCapacity = ad.SeatingCapacity,
                                                                               CarryingCapacity = ad.CarryingCapacity,
                                                                               StartDate = ad.StartDate,
                                                                               Status = (int)OpStatus.Submited
                                                                           });

                                             //所有权历史
                                             db.OwnershipHistories.Add(new OwnershipHistory
                                                                           {
                                                                               OwnershipHistoryID = Guid.NewGuid(),
                                                                               Aircraft = ac,
                                                                               Owner = owner,
                                                                               StartDate = ad.StartDate,
                                                                               Status = (int)OpStatus.Submited
                                                                           });

                                         });

                        #endregion
                    }

                    if (rads.Any())
                    {
                        #region 导入申请飞机数据

                        rads.ForEach(ad =>
                                         {
                                             // 计划飞机数据
                                             var pa = new PlanAircraft
                                                          {
                                                              PlanAircraftID = Guid.NewGuid(),
                                                              AircraftType = ad.AircraftType,
                                                              Airlines = airline,
                                                              IsLock = true,
                                                              IsOwn = true,
                                                              Status = (int)ManageStatus.Request,
                                                          };
                                             db.PlanAircrafts.Add(pa);

                                             // 申请历史
                                             var rh = new ApprovalHistory
                                                          {
                                                              ApprovalHistoryID = Guid.NewGuid(),
                                                              PlanAircraft = pa,
                                                              Request = request,
                                                              ImportCategory = ad.ImportType,
                                                              SeatingCapacity = ad.SeatingCapacity,
                                                              CarryingCapacity = ad.CarryingCapacity,
                                                              Annual = ad.Annual,
                                                              RequestDeliverMonth = ad.Month,
                                                              Note = ad.DocNumber,
                                                          };
                                             db.ApprovalHistories.Add(rh);

                                             // 计划历史
                                             var ph = new OperationPlan
                                                          {
                                                              PlanHistoryID = Guid.NewGuid(),
                                                              PlanAircraft = pa,
                                                              Plan = plan,
                                                              ActionCategory = ad.ImportType,
                                                              TargetCategory = ad.ImportType,
                                                              AircraftType = ad.AircraftType,
                                                              Annual = ad.Annual,
                                                              PerformMonth = ad.Month,
                                                              SeatingCapacity = ad.SeatingCapacity,
                                                              CarryingCapacity = ad.CarryingCapacity,
                                                              IsValid = true,
                                                              IsSubmit = true,
                                                              ApprovalHistory = rh
                                                          };
                                             db.PlanHistories.Add(ph);

                                         });

                        #endregion
                    }

                    if (aads.Any())
                    {
                        #region 导入批文飞机数据

                        aads.ForEach(ad =>
                                         {
                                             // 计划飞机数据
                                             var pa = new PlanAircraft
                                                          {
                                                              PlanAircraftID = Guid.NewGuid(),
                                                              Airlines = airline,
                                                              AircraftType = ad.AircraftType,
                                                              IsLock = true,
                                                              IsOwn = true,
                                                              Status = (int)ManageStatus.Approval
                                                          };
                                             db.PlanAircrafts.Add(pa);

                                             // 申请历史
                                             var rh = new ApprovalHistory
                                                          {
                                                              ApprovalHistoryID = Guid.NewGuid(),
                                                              PlanAircraft = pa,
                                                              Request = request,
                                                              ImportCategory = ad.ImportType,
                                                              SeatingCapacity = ad.SeatingCapacity,
                                                              CarryingCapacity = ad.CarryingCapacity,
                                                              Annual = ad.Annual,
                                                              RequestDeliverMonth = ad.Month,
                                                              IsApproved = true,
                                                              Note = ad.ApprovalNumber,
                                                              ManaApprovalHistory = new ManaApprovalHistory
                                                                                        {
                                                                                            IsApproved = true
                                                                                        }
                                                          };
                                             db.ApprovalHistories.Add(rh);

                                             // 计划历史
                                             var ph = new OperationPlan
                                                          {
                                                              PlanHistoryID = Guid.NewGuid(),
                                                              PlanAircraft = pa,
                                                              Plan = plan,
                                                              ActionCategory = ad.ImportType,
                                                              TargetCategory = ad.ImportType,
                                                              AircraftType = ad.AircraftType,
                                                              Annual = ad.Annual,
                                                              PerformMonth = ad.Month,
                                                              SeatingCapacity = ad.SeatingCapacity,
                                                              CarryingCapacity = ad.CarryingCapacity,
                                                              IsValid = true,
                                                              IsSubmit = true,
                                                              ApprovalHistory = rh
                                                          };
                                             db.PlanHistories.Add(ph);

                                         });

                        #endregion
                    }

                    if (db.SaveChanges() > 0)
                    {
                        log.Info("导入[" + airline.Name + "]数据成功。");
                    }
                    else
                    {
                        log.Error("导入[" + airline.Name + "]数据失败！");
                    }
                }
            }
            btnProcess.IsEnabled = false;
        }

        /// <summary>
        /// 生成航空公司数据
        /// </summary>
        /// <param name="airline">航空公司</param>
        /// <param name="planID">初始化计划ID</param>
        /// <param name="requestID">初始化申请ID</param>
        private void BuildAirlinesData(Airlines airline, Guid planID, Guid requestID)
        {
            log.Info("开始生成[" + airline.Name + "]数据。");

            using (var db = new FleetEntities(Conn.LocalAfrp))
            {
                AircraftBusiness ab;
                List<OwnershipHistory> ohs = new List<OwnershipHistory>();
                var acs = DynamicData.GetAircrafts().Where(p => p.AirlinesID == airline.OwnerID).ToList();
                var os = DynamicData.GetOwnershipHistories();
                var owns = DynamicData.GetOwners();
                var airlines = DynamicData.GetAirlines();

                // 设置当前航空公司
                var curAirline = db.Owners.OfType<Airlines>().SingleOrDefault(a => a.OwnerID == airline.OwnerID);
                curAirline.IsCurrent = true;

                // 生成初始化计划
                DynamicData.GetPlans().Where(p => p.PlanID == planID).ToList().ForEach(p => db.Plans.Add(p));

                // 生成初始化申请
                DynamicData.GetRequests().Where(p => p.RequestID == requestID).ToList().ForEach(r => db.Requests.Add(r));

                // 生成所有权人
                // 1、获取航空公司的所有权历史集合
                acs.ForEach(a => ohs.Add(os.SingleOrDefault(o => o.AircraftID == a.AircraftID)));
                // 2、对航空公司所有权历史中的所有权人聚合后的集合执行业务逻辑
                ohs.Select(oh => oh.OwnerID).Distinct().ToList().ForEach(o =>
                                                                             {
                                                                                 // 3、从owns集合中查找所有权人，由于航空公司不在此集合中，如果owner为航空公司，返回为空。
                                                                                 var own =
                                                                                     owns.SingleOrDefault(
                                                                                         ow => ow.OwnerID == o);
                                                                                 // 4、只有返回非空时，才是需要添加到该航空公司的所有权人实体集合中的。
                                                                                 if (own != null)
                                                                                 {
                                                                                     db.Owners.Add(own);
                                                                                 }
                                                                             });

                // 生成所有权历史
                ohs.ForEach(o => db.OwnershipHistories.Add(o));

                // 生成计划飞机
                DynamicData.GetPlanAircrafts().Where(p => p.AirlinesID == airline.OwnerID).ToList().ForEach(
                    p => db.PlanAircrafts.Add(p));

                // 生成飞机、商业数据历史
                acs.ForEach(a =>
                                {
                                    ab =
                                        DynamicData.GetAircraftBusinesses().Where(b => b.AircraftID == a.AircraftID).
                                            FirstOrDefault();
                                    a.SeatingCapacity = ab.SeatingCapacity;
                                    a.CarryingCapacity = ab.CarryingCapacity;
                                    db.Aircrafts.Add(a);
                                    db.AircraftBusinesses.Add(ab);
                                });

                // 生成批文历史
                DynamicData.GetApprovalHistories().Where(a => a.RequestID == requestID).ToList().ForEach(
                    a => db.ApprovalHistories.Add(a));

                // 生成运营历史
                DynamicData.GetOperationHistories().Where(p => p.AirlinesID == airline.OwnerID).ToList().ForEach(
                    o => db.OperationHistories.Add(o));

                // 生成运营计划
                DynamicData.GetOperationPlans().Where(o => o.PlanID == planID).ToList().ForEach(
                    o => db.PlanHistories.Add(o));

                if (db.SaveChanges() > 0)
                {
                    log.Info("生成[" + airline.Name + "]数据成功。");
                }
                else
                {
                    log.Error("生成[" + airline.Name + "]数据失败！");
                }
            }
        }

        /// <summary>
        /// 生成民航局初始化数据
        /// </summary>
        private void BuildCAACData()
        {
            using (var db = new FleetEntities(Conn.LocalCafm))
            {
                log.Info("开始生成[民航局]数据。");

                var acs = DynamicData.GetAircrafts();
                List<Owner> owns = new List<Fleet.Models.Owner>();
                DynamicData.GetOwners().ForEach(o => owns.Add(o));
                DynamicData.GetAirlines().ForEach(a => owns.Add(a));

                var owners = new List<Owner>
                                 {
                                     new Owner
                                         {
                                             OwnerID = Guid.Parse("3A7FE4A7-4102-4DBB-937F-D1850B83923B"),
                                             Name = "国内供应商",
                                             ShortName = "",
                                             Description = "",
                                             SupplierType = 1,
                                         },
                                     new Owner
                                         {
                                             OwnerID = Guid.Parse("76B17B09-A452-41CE-811C-29688595EFB7"),
                                             Name = "国外供应商",
                                             ShortName = "",
                                             Description = "",
                                             SupplierType = 2,
                                         },
                                 };
                owners.ForEach(o => db.Owners.Add(o));

                DynamicData.GetPlans().ForEach(p => db.Plans.Add(p));
                DynamicData.GetRequests().ForEach(r => db.Requests.Add(r));
                DynamicData.GetPlanAircrafts().ForEach(p => db.PlanAircrafts.Add(p));
                acs.ForEach(a =>
                                {
                                    if (a.AirlinesID != a.OwnerID)
                                    {
                                        var owner = owns.SingleOrDefault(o => o.OwnerID == a.OwnerID);
                                        if (owner.SupplierType == 1)
                                        {
                                            a.OwnerID = owners[0].OwnerID;
                                        }
                                        else if (owner.SupplierType == 2)
                                        {
                                            a.OwnerID = owners[1].OwnerID;
                                        }
                                    }
                                    db.Aircrafts.Add(a);
                                });
                DynamicData.GetApprovalHistories().ForEach(a => db.ApprovalHistories.Add(a));
                DynamicData.GetOperationHistories().ForEach(o => db.OperationHistories.Add(o));
                DynamicData.GetOperationPlans().ForEach(o => db.PlanHistories.Add(o));
                DynamicData.GetOwnershipHistories().ForEach(o =>
                                                                {
                                                                    var ac =
                                                                        acs.SingleOrDefault(
                                                                            a => a.AircraftID == o.AircraftID);
                                                                    if (ac.AirlinesID != o.OwnerID)
                                                                    {
                                                                        var owner =
                                                                            owns.SingleOrDefault(
                                                                                own => own.OwnerID == o.OwnerID);
                                                                        if (owner.SupplierType == 1)
                                                                        {
                                                                            o.OwnerID = owners[0].OwnerID;
                                                                        }
                                                                        else if (owner.SupplierType == 2)
                                                                        {
                                                                            o.OwnerID = owners[1].OwnerID;
                                                                        }
                                                                    }
                                                                    db.OwnershipHistories.Add(o);
                                                                });
                DynamicData.GetAircraftBusinesses().ForEach(a => db.AircraftBusinesses.Add(a));

                if (db.SaveChanges() > 0)
                {
                    log.Info("生成[民航局]数据成功。");
                }
                else
                {
                    log.Error("生成[民航局]数据失败！");
                }
            }
        }

        #region 准备测试数据

        private void plan_Click(object sender, RoutedEventArgs e)
        {
            //生成计划
            BuildCAACPlanData();
        }

        /// <summary>
        /// 生成航空公司测试数据
        /// </summary>
        /// <param name="airlineName">航空公司名称</param>
        private void BuildTestData(string airlineName)
        {
            using (var db = new FleetEntities(Conn.LocalAfrp))
            {
                var plan = db.Plans.FirstOrDefault();
                var airline = db.Owners.OfType<Airlines>().SingleOrDefault(a => a.ICAOCode == "CXA");
                var annual = db.Annuals.SingleOrDefault(a => a.Year == 2012);
                var acType = db.AircraftTypes.SingleOrDefault(a => a.Name == "B737-800");
                var importType = db.ActionCategories.SingleOrDefault(a => a.ActionName == "购买");

                log.Info("开始生成[" + airlineName + "]测试数据。");

                new List<OperationPlan>
                    {
                        new OperationPlan
                            {
                                PlanHistoryID = Guid.NewGuid(),
                                PlanAircraft = new PlanAircraft
                                                   {
                                                       PlanAircraftID = Guid.NewGuid(),
                                                       Airlines = airline,
                                                       AircraftType = acType,
                                                       IsLock = true,
                                                       IsOwn = true,
                                                       Status = (int) ManageStatus.Plan
                                                   },
                                Plan = plan,
                                ActionCategory = importType,
                                TargetCategory = importType,
                                AircraftType = acType,
                                Annual = annual,
                                PerformMonth = 10,
                                SeatingCapacity = 164,
                                CarryingCapacity = 9M,
                                IsValid = true,
                                IsSubmit = true,
                            },
                        new OperationPlan
                            {
                                PlanHistoryID = Guid.NewGuid(),
                                PlanAircraft = new PlanAircraft
                                                   {
                                                       PlanAircraftID = Guid.NewGuid(),
                                                       Airlines = airline,
                                                       AircraftType = acType,
                                                       IsLock = true,
                                                       Status = (int) ManageStatus.Plan,
                                                       IsOwn = true,
                                                   },
                                Plan = plan,
                                ActionCategory = importType,
                                TargetCategory = importType,
                                AircraftType = acType,
                                Annual = annual,
                                PerformMonth = 10,
                                SeatingCapacity = 164,
                                CarryingCapacity = 9M,
                                IsValid = true,
                                IsSubmit = true,
                            },
                        new OperationPlan
                            {
                                PlanHistoryID = Guid.NewGuid(),
                                PlanAircraft = new PlanAircraft
                                                   {
                                                       PlanAircraftID = Guid.NewGuid(),
                                                       Airlines = airline,
                                                       AircraftType = acType,
                                                       IsLock = true,
                                                       Status = (int) ManageStatus.Plan
                                                   },
                                Plan = plan,
                                ActionCategory = importType,
                                TargetCategory = importType,
                                AircraftType = acType,
                                Annual = annual,
                                PerformMonth = 10,
                                SeatingCapacity = 164,
                                CarryingCapacity = 9M,
                                IsValid = true,
                                IsSubmit = true,
                            },
                        new OperationPlan
                            {
                                PlanHistoryID = Guid.NewGuid(),
                                PlanAircraft = new PlanAircraft
                                                   {
                                                       PlanAircraftID = Guid.NewGuid(),
                                                       Airlines = airline,
                                                       AircraftType = acType,
                                                       IsLock = true,
                                                       Status = (int) ManageStatus.Plan
                                                   },
                                Plan = plan,
                                ActionCategory = importType,
                                TargetCategory = importType,
                                AircraftType = acType,
                                Annual = annual,
                                PerformMonth = 10,
                                SeatingCapacity = 164,
                                CarryingCapacity = 9M,
                                IsValid = true,
                                IsSubmit = true,
                            },
                        new OperationPlan
                            {
                                PlanHistoryID = Guid.NewGuid(),
                                PlanAircraft = new PlanAircraft
                                                   {
                                                       PlanAircraftID = Guid.NewGuid(),
                                                       Airlines = airline,
                                                       AircraftType = acType,
                                                       IsLock = true,
                                                       Status = (int) ManageStatus.Plan
                                                   },
                                Plan = plan,
                                ActionCategory = importType,
                                TargetCategory = importType,
                                AircraftType = acType,
                                Annual = annual,
                                PerformMonth = 10,
                                SeatingCapacity = 164,
                                CarryingCapacity = 9M,
                                IsValid = true,
                                IsSubmit = true,
                            },
                        new OperationPlan
                            {
                                PlanHistoryID = Guid.NewGuid(),
                                PlanAircraft = new PlanAircraft
                                                   {
                                                       PlanAircraftID = Guid.NewGuid(),
                                                       Airlines = airline,
                                                       AircraftType = acType,
                                                       IsLock = true,
                                                       Status = (int) ManageStatus.Plan
                                                   },
                                Plan = plan,
                                ActionCategory = importType,
                                TargetCategory = importType,
                                AircraftType = acType,
                                Annual = annual,
                                PerformMonth = 10,
                                SeatingCapacity = 164,
                                CarryingCapacity = 9M,
                                IsValid = true,
                                IsSubmit = true,
                            },
                    }.ForEach(p => plan.PlanHistories.Add(p));

                if (db.SaveChanges() > 0)
                {
                    log.Info("生成[" + airlineName + "]测试数据成功。");
                }
                else
                {
                    log.Error("生成[" + airlineName + "]测试数据失败！");
                }
            }
        }

        /// <summary>
        /// 生成计划测试数据
        /// </summary>
        private void BuildCAACPlanData()
        {
            using (var db = new FleetEntities(Conn.TestCafm))
            {
                log.Info("开始生成[民航局]计划数据。");

                //获取所有航空公司
                var allAirlnes = db.Owners.OfType<Airlines>().ToList();
                //获取货机的航空公司
                var cargoAirlines =
                    allAirlnes.Where(
                        p =>
                        p.ShortName == "南航" || p.ShortName == "中货航" || p.ShortName == "扬子江" || p.ShortName == "国航" ||
                        p.ShortName == "顺丰").ToList();
                //获取客机的航空公司
                var airlines = allAirlnes.Except(cargoAirlines).ToList();
                var annuals = db.Annuals.OrderBy(t => t.Year).ToList();

                annuals.ForEach(t => t.IsOpen = false);
                var currentAnnual = annuals.First(p => p.Year == 2013);
                currentAnnual.IsOpen = true;

                //获取引进类型
                var actionCategories = db.ActionCategories.Where(p => p.ActionType == "引进").ToList();

                //获取座级
                var aircraftTypes1 = db.AircraftTypes.First(p => p.Name == "A380");
                var aircraftTypes2 = db.AircraftTypes.First(p => p.Name == "B757-200");
                var aircraftTypes3 = db.AircraftTypes.First(p => p.Name == "CRJ200");
                var aircraftTypes4 = db.AircraftTypes.First(p => p.Name == "B777F");
                var aircraftTypes5 = db.AircraftTypes.First(p => p.Name == "B757-200F");

                /// 计划
                var plans = new List<Plan>()
                                {
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[1],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[2],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = false,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[3],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[4],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = false,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[5],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[6],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[7],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = false,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[8],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[9],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[10],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = false,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[11],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[12],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = false,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[13],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[14],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[15],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[16],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[17],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = false,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[18],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[19],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = false,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = airlines[20],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = cargoAirlines[0],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = cargoAirlines[1],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = cargoAirlines[2],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = cargoAirlines[3],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                    new Plan
                                        {
                                            PlanID = Guid.NewGuid(),
                                            Airlines = cargoAirlines[4],
                                            Annual = annuals[2],
                                            Title = "2013年度机队资源规划",
                                            VersionNumber = 1,
                                            IsValid = true,
                                            IsFinished = true,
                                            SubmitDate = new DateTime(2012, 8, 1),
                                            PlanStatus = PlanStatus.Submited,
                                            IsCurrentVersion = true
                                        },
                                };
                plans.ForEach(pl => db.Plans.Add(pl));

                /// 计划历史
                var totalPlanHistories = new List<PlanHistory>
                                             {
                                                 #region 客机计划历史

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[0],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[1],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[2],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[3],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[4],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[5],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[6],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[7],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[8],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[9],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[10],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[11],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[12],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[13],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[14],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[15],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[16],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[17],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[18],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细
                                                 //2013 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 250座以上客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes1,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 100-200座客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes2,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },


                                                 //2013 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },

                                                 //2014 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2015 100座以下客机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[19],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes3,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

            
 
                                                 #endregion

                                                 #region 货机航空公司

                                                 #region 计划明细


                                                 //2013 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     }, 
                                                 //2014 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[20],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细


                                                 //2013 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     }, 
                                                 //2014 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[21],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细


                                                 //2013 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     }, 
                                                 //2014 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[22],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细


                                                 //2013 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     }, 
                                                 //2014 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[23],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion

                                                 #region 计划明细


                                                 //2013 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 //2014 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 大型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes4,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 //2013 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[2],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 50,
                                                         CarryingCapacity = 5,
                                                         IsValid = true
                                                     }, 
                                                 //2014 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[3],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 //2015 中型货机
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 2,
                                                         SeatingCapacity = 100,
                                                         CarryingCapacity = 10,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },
                                                 new OperationPlan
                                                     {
                                                         PlanHistoryID = Guid.NewGuid(),
                                                         Plan = plans[24],
                                                         ActionCategory = actionCategories[0],
                                                         TargetCategory = actionCategories[0],
                                                         AircraftType = aircraftTypes5,
                                                         Annual = annuals[4],
                                                         PerformMonth = 3,
                                                         SeatingCapacity = 150,
                                                         CarryingCapacity = 15,
                                                         IsValid = true
                                                     },

                                                 #endregion


                                                 #endregion
                                             };
                totalPlanHistories.ForEach(pl => db.PlanHistories.Add(pl));

                if (db.SaveChanges() > 0)
                {
                    log.Info("生成[民航局计划]数据成功。");
                }
                else
                {
                    log.Error("生成[民航局计划]数据失败！");
                }
            }
        }

        #endregion

        private void btnCompareDatabase_Click_1(object sender, RoutedEventArgs e)
        {
            using (var db = new FleetEntities(Conn.LocalCafm))
            {
                log.Info("开始比较！");

                using (var local = new FleetEntities(Conn.LocalAfrp))
                {
                    var afrpAircraft = local.Aircrafts;
                    afrpAircraft.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.Aircrafts.Any(p => p.AircraftID == f.AircraftID))
                            {
                                log.Info("飞机存在主键不一致！");

                            }

                        });
                    //计划
                    var afrpplan = local.Plans;
                    afrpplan.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.Plans.Any(p => p.PlanID == f.PlanID))
                            {
                                log.Info("计划存在主键不一致！");
                            }

                        });
                    //计划历史
                    var afrpPlanHistories = local.PlanHistories;
                    afrpPlanHistories.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.PlanHistories.Any(p => p.PlanHistoryID == f.PlanHistoryID))
                            {
                                log.Info("计划历史存在主键不一致！");
                            }

                        });

                    //计划飞机
                    var afrpPlanAircrafts = local.PlanAircrafts;
                    afrpPlanAircrafts.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.PlanAircrafts.Any(p => p.PlanAircraftID == f.PlanAircraftID))
                            {
                                log.Info("计划飞机存在主键不一致！");
                            }

                        });

                    //商业数据
                    var afrpAircraftBusinesses = local.AircraftBusinesses;
                    afrpAircraftBusinesses.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.AircraftBusinesses.Any(p => p.AircraftBusinessID == f.AircraftBusinessID))
                            {
                                log.Info("商业数据存在主键不一致！");
                            }

                        });
                    //批文
                    var afrpApprovalDocs = local.ApprovalDocs;
                    afrpApprovalDocs.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.ApprovalDocs.Any(p => p.ApprovalDocID == f.ApprovalDocID))
                            {
                                log.Info("批文存在主键不一致！");
                            }

                        });
                    //批文历史
                    var afrpApprovalHistories = local.ApprovalHistories;
                    afrpApprovalHistories.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.ApprovalHistories.Any(p => p.ApprovalHistoryID == f.ApprovalHistoryID))
                            {
                                log.Info("批文历史存在主键不一致！");
                            }

                        });
                    //运营历史
                    var afrpOperationHistories = local.OperationHistories;
                    afrpOperationHistories.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.OperationHistories.Any(p => p.OperationHistoryID == f.OperationHistoryID))
                            {
                                log.Info("运营历史存在主键不一致！");
                            }

                        });

                    ////所有者
                    //var afrpOwners = local.Owners;
                    //afrpOwners.ToList()
                    //    .ForEach(
                    //    f =>
                    //    {
                    //        if (!db.Owners.Any(p => p.OwnerID == f.OwnerID))
                    //        {
                    //            log.Info("所有者存在主键不一致！");
                    //        }

                    //    });
                    //所有者历史
                    var afrpOwnershipHistories = local.OwnershipHistories;
                    afrpOwnershipHistories.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.OwnershipHistories.Any(p => p.OwnershipHistoryID == f.OwnershipHistoryID))
                            {
                                log.Info("所有者历史存在主键不一致！");
                            }

                        });
                    //申请
                    var afrpRequests = local.Requests;
                    afrpRequests.ToList()
                        .ForEach(
                        f =>
                        {
                            if (!db.Requests.Any(p => p.RequestID == f.RequestID))
                            {
                                log.Info("申请存在主键不一致！");
                            }

                        });
                }
                log.Info("比较结束！");

            }
        }
    }
}
