using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using Ria.Common;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Data;
using System.ServiceModel.DomainServices.Client;
using System.Collections;
using Telerik.Windows.Controls;
using Microsoft.Practices.Prism.ViewModel;
using UniCloud.Fleet.Models;
using CAAC.Fleet.Services;
using CAAC.CAFM.Converters;
using Telerik.Windows;
using System.Windows.Media;
using CAAC.Infrastructure;
using MessageOperationLibrary.EventAggregatorRepository;


namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmColorSettingViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmColorSettingViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmColorSettingViewModel()
        {
            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
            this.confirmExitInteractionRequest = new InteractionRequest<ConfirmViewModel>();
        }

        #region Local


        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        CAAC.CAFM.Views.CafmColorSettingView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmColorSettingView>();
        private RadGridView AirLineRadGridView, RegionalRadGridView, AircraftTypeRadGridView, AircraftAgeRadGridView;
        private RadGridView ImportTypeRadGridView, AircraftTrendRadGridView, SupplierRadGridView, ManufactureRadGridView;
        private List<xmlItem> airlinelist = new List<xmlItem>();
        private List<xmlItem> restairlinelist = new List<xmlItem>();
        private List<xmlItem> regionallist = new List<xmlItem>();
        private List<xmlItem> aircrafttypelist = new List<xmlItem>();
        private List<xmlItem> aircraftagelist = new List<xmlItem>();
        private List<xmlItem> aircrafttrendlist = new List<xmlItem>();
        private List<xmlItem> importtypelist = new List<xmlItem>();
        private List<xmlItem> supplierlist = new List<xmlItem>();
        private List<xmlItem> manufacturerlist = new List<xmlItem>();
        #region Property

        private bool _isChanged = false;
        /// <summary>
        /// 判断数据是否更改(用于控制界面的按钮）
        /// </summary>
        public bool isChanged
        {
            get { return this._isChanged; }
            private set
            {
                if (this._isChanged != value)
                {
                    this._isChanged = value;

                    this._canSave = value;
                    this._canAbort = value;
                    this.SaveCommand.RaiseCanExecuteChanged();
                    this.AbortCommand.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Method

        /// <summary>
        /// 跟踪实体变化，控制保存、放弃按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntityContainer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasChanges")
            {
                //this._canSave = this._service.EntityContainer.HasChanges;
                //this._canAbort = this._service.EntityContainer.HasChanges;
                //this.SaveCommand.RaiseCanExecuteChanged();
                //this.AbortCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 刷新按钮状态
        /// </summary>
        private void RefreshButtonState()
        {

        }

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.AirLineRadGridView = _view.AirLineRadGridView as RadGridView;
            this.RegionalRadGridView = _view.RegionalRadGridView as RadGridView;
            this.AircraftTypeRadGridView = _view.AircraftTypeRadGridView as RadGridView;
            this.AircraftAgeRadGridView = _view.AircraftAgeRadGridView as RadGridView;
            this.ImportTypeRadGridView = _view.ImportTypeRadGridView as RadGridView;
            this.SupplierRadGridView = _view.SupplierRadGridView as RadGridView;
            this.ManufactureRadGridView = _view.ManufactureRadGridView as RadGridView;
            this.AircraftTrendRadGridView = _view.AircraftTrendRadGridView as RadGridView;
            //定义数据编辑的事件（控制保存、放弃修改按钮的可用性）
            AirLineRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            RegionalRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            AircraftTypeRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            AircraftAgeRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            ImportTypeRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            SupplierRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            ManufactureRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            AircraftTrendRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
        }

        /// <summary>
        /// 控制保存、放弃修改按钮的可用性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FleetColorSetGridView_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            if (Convert.ToString(e.NewData) != Convert.ToString(e.OldData))
            {
                this.isChanged = true;
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            if (!this.IsBusy)
            {
                CreatColorSetList();
            }
        }

        #endregion

        #endregion

        #region ViewModel

        #region 加载颜色配置的集合 AgeDeployXml

        /// <summary>
        /// 加载颜色配置的集合
        /// </summary>
        public XmlConfig ColorSettingXml
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<XmlConfig>().FirstOrDefault(p => p.ConfigType == "颜色配置");
            }
        }

        #endregion

        #region 加载实体集合 XmlConfig

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<XmlConfig> ViewXmlConfig
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<XmlConfig>();
            }
        }

        private bool _isBusyXmlConfig = true;
        public bool IsBusyXmlConfig
        {
            get { return this._isBusyXmlConfig; }
            private set
            {
                if (this._isBusyXmlConfig != value)
                {
                    this._isBusyXmlConfig = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadXmlConfig()
        {
            this.IsBusy = true;
            this._service.LoadXmlConfig(new QueryBuilder<XmlConfig>(), lo =>
            {
                this.IsBusyXmlConfig = false;
                this.IsBusy = this.IsBusyXmlConfig;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewXmlConfig);
                    InitializeData();
                    CreatColorSetList();
                }
            }, null);
        }

        #endregion


        #region Property

        #region ViewModel 属性 IsBusy

        private bool _isBusy;
        public bool IsBusy
        {
            get { return this._isBusy; }
            private set
            {
                if (this._isBusy != value)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => this.IsBusy);

                }
            }
        }

        #endregion

        #region ViewModel 属性 当页面发生修改，提醒用户是否保存

        private readonly InteractionRequest<ConfirmViewModel> confirmExitInteractionRequest;
        /// <summary>
        /// ViewModel 属性 当页面发生修改，提醒用户是否保存
        /// </summary>
        public IInteractionRequest ConfirmExitInteractionRequest
        {
            get { return this.confirmExitInteractionRequest; }
        }

        #endregion

        #region ViewModel 属性 --选择颜色配置的类目集合

        private List<xmlItem> _AirLineList;
        /// <summary>
        /// 航空公司颜色配置集合
        /// </summary>
        public List<xmlItem> AirLineList
        {
            get { return _AirLineList; }
            set
            {

                if (AirLineList != value)
                {
                    _AirLineList = value;
                    this.RaisePropertyChanged(() => this.AirLineList);
                }
            }
        }

        private List<xmlItem> _RestAirLineList;
        /// <summary>
        /// 不用上报的航空公司颜色配置集合
        /// </summary>
        public List<xmlItem> RestAirLineList
        {
            get { return _RestAirLineList; }
            set
            {

                if (RestAirLineList != value)
                {
                    _RestAirLineList = value;
                    this.RaisePropertyChanged(() => this.RestAirLineList);
                }
            }
        }

        private List<xmlItem> _RegionalList;
        /// <summary>
        /// 座级颜色配置集合
        /// </summary>
        public List<xmlItem> RegionalList
        {
            get { return _RegionalList; }
            set
            {
                if (RegionalList != value)
                {
                    _RegionalList = value;
                    this.RaisePropertyChanged(() => this.RegionalList);
                }
            }
        }

        private List<xmlItem> _AircraftTypeList;
        /// <summary>
        /// 机型颜色配置集合
        /// </summary>
        public List<xmlItem> AircraftTypeList
        {
            get { return _AircraftTypeList; }
            set
            {

                if (AircraftTypeList != value)
                {
                    _AircraftTypeList = value;
                    this.RaisePropertyChanged(() => this.AircraftTypeList);
                }
            }
        }

        private List<xmlItem> _AircraftAgeList;
        /// <summary>
        /// 机型颜色配置集合
        /// </summary>
        public List<xmlItem> AircraftAgeList
        {
            get { return _AircraftAgeList; }
            set
            {

                if (AircraftAgeList != value)
                {
                    _AircraftAgeList = value;
                    this.RaisePropertyChanged(() => this.AircraftAgeList);
                }
            }
        }

        private List<xmlItem> _AircraftTrendList;
        /// <summary>
        /// 机型颜色配置集合
        /// </summary>
        public List<xmlItem> AircraftTrendList
        {
            get { return _AircraftTrendList; }
            set
            {

                if (AircraftTrendList != value)
                {
                    _AircraftTrendList = value;
                    this.RaisePropertyChanged(() => this.AircraftTrendList);
                }
            }
        }

        private List<xmlItem> _ImportTypeList;
        /// <summary>
        /// 引进方式颜色配置集合
        /// </summary>
        public List<xmlItem> ImportTypeList
        {
            get { return _ImportTypeList; }
            set
            {

                if (ImportTypeList != value)
                {
                    _ImportTypeList = value;
                    this.RaisePropertyChanged(() => this.ImportTypeList);
                }
            }
        }

        private List<xmlItem> _SupplierList;
        /// <summary>
        /// 供应商颜色配置集合
        /// </summary>
        public List<xmlItem> SupplierList
        {
            get { return _SupplierList; }
            set
            {

                if (SupplierList != value)
                {
                    _SupplierList = value;
                    this.RaisePropertyChanged(() => this.SupplierList);
                }
            }
        }

        private List<xmlItem> _ManufacturerList;
        /// <summary>
        /// 制造商颜色配置集合
        /// </summary>
        public List<xmlItem> ManufacturerList
        {
            get { return _ManufacturerList; }
            set
            {

                if (ManufacturerList != value)
                {
                    _ManufacturerList = value;
                    this.RaisePropertyChanged(() => this.ManufacturerList);
                }
            }
        }

        #endregion


        #region 颜色配置XML文件的读取
        private void CreatColorSetList()
        {

            airlinelist.Clear();
            restairlinelist.Clear();
            regionallist.Clear();
            aircrafttypelist.Clear();
            aircraftagelist.Clear();
            aircrafttrendlist.Clear();
            importtypelist.Clear();
            supplierlist.Clear();
            manufacturerlist.Clear();
            var xmlconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
            if (xmlconfig != null)
            {
                XElement xelement = xmlconfig.XmlContent;
                if (xelement != null)
                {
                    foreach (XElement type in xelement.Descendants("Type"))
                    {
                        if (type.Attribute("TypeName").Value == "航空公司")
                        {
                            foreach (XElement item in type.Descendants("Item"))
                            {
                                if (ValidName(item.Attribute("Name").Value))//判断是否是当前需要上报的航空公司
                                {
                                    xmlItem xmlItem = new xmlItem();
                                    xmlItem.Name = item.Attribute("Name").Value;
                                    xmlItem.Color = item.Attribute("Color").Value; ;
                                    airlinelist.Add(xmlItem);
                                }
                                else//如果暂时不用上报则将其颜色配置保留,但不显示在界面
                                {
                                    xmlItem xmlItem = new xmlItem();
                                    xmlItem.Name = item.Attribute("Name").Value;
                                    xmlItem.Color = item.Attribute("Color").Value; ;
                                    restairlinelist.Add(xmlItem);
                                }
                            }
                            AirLineList = airlinelist;
                            RestAirLineList = restairlinelist;
                        }
                        else if (type.Attribute("TypeName").Value == "座级")
                        {
                            foreach (XElement item in type.Descendants("Item"))
                            {
                                xmlItem xmlItem = new xmlItem();
                                xmlItem.Name = item.Attribute("Name").Value;
                                xmlItem.Color = item.Attribute("Color").Value; ;
                                regionallist.Add(xmlItem);
                            }
                            RegionalList = regionallist;
                        }
                        else if (type.Attribute("TypeName").Value == "机型")
                        {
                            foreach (XElement item in type.Descendants("Item"))
                            {
                                xmlItem xmlItem = new xmlItem();
                                xmlItem.Name = item.Attribute("Name").Value;
                                xmlItem.Color = item.Attribute("Color").Value; ;
                                aircrafttypelist.Add(xmlItem);
                            }
                            AircraftTypeList = aircrafttypelist;
                        }
                        else if (type.Attribute("TypeName").Value == "机龄")
                        {
                            foreach (XElement item in type.Descendants("Item"))
                            {
                                xmlItem xmlItem = new xmlItem();
                                xmlItem.Name = item.Attribute("Name").Value;
                                xmlItem.Color = item.Attribute("Color").Value; ;
                                aircraftagelist.Add(xmlItem);
                            }
                            AircraftAgeList = aircraftagelist;
                        }
                        else if (type.Attribute("TypeName").Value == "运力变化")
                        {
                            foreach (XElement item in type.Descendants("Item"))
                            {
                                if (!(item.Attribute("Name").Value.IndexOf("（子）") != -1))
                                {
                                    xmlItem xmlItem = new xmlItem();
                                    xmlItem.Name = item.Attribute("Name").Value;
                                    xmlItem.Color = item.Attribute("Color").Value; ;
                                    aircrafttrendlist.Add(xmlItem);
                                }
                            }
                            AircraftTrendList = aircrafttrendlist;
                        }
                        else if (type.Attribute("TypeName").Value == "引进方式")
                        {
                            foreach (XElement item in type.Descendants("Item"))
                            {
                                xmlItem xmlItem = new xmlItem();
                                xmlItem.Name = item.Attribute("Name").Value;
                                xmlItem.Color = item.Attribute("Color").Value; ;
                                importtypelist.Add(xmlItem);
                            }
                            ImportTypeList = importtypelist;
                        }
                        else if (type.Attribute("TypeName").Value == "供应商")
                        {
                            foreach (XElement item in type.Descendants("Item"))
                            {
                                xmlItem xmlItem = new xmlItem();
                                xmlItem.Name = item.Attribute("Name").Value;
                                xmlItem.Color = item.Attribute("Color").Value; ;
                                supplierlist.Add(xmlItem);
                            }
                            SupplierList = supplierlist;
                        }
                        else if (type.Attribute("TypeName").Value == "制造商")
                        {
                            foreach (XElement item in type.Descendants("Item"))
                            {
                                xmlItem xmlItem = new xmlItem();
                                xmlItem.Name = item.Attribute("Name").Value;
                                xmlItem.Color = item.Attribute("Color").Value; ;
                                manufacturerlist.Add(xmlItem);
                            }
                            ManufacturerList = manufacturerlist;
                        }
                    }
                }
            }

        }
        #endregion

        #endregion

        #region Command


        #region ViewModel 命令 -- 保存

        public DelegateCommand<object> SaveCommand { get; private set; }
        private void OnSave(object sender)
        {
            SaveColorSettingXml();
            //设置保存、放弃修改按钮的可用性
            this.isChanged = false;
        }
        private bool _canSave = false;
        bool CanSave(object sender)
        {
            return this._canSave;
        }

        #endregion

        #region ViewModel 命令 -- 取消更改

        public DelegateCommand<object> AbortCommand { get; private set; }
        private void OnAbort(object sender)
        {
            this._service.RejectChanges();
            CreatColorSetList();
            this.AirLineRadGridView.GroupDescriptors.Reset();
            this.RegionalRadGridView.GroupDescriptors.Reset();
            this.AircraftTypeRadGridView.GroupDescriptors.Reset();
            this.AircraftAgeRadGridView.GroupDescriptors.Reset();
            this.ImportTypeRadGridView.GroupDescriptors.Reset();
            this.AircraftTrendRadGridView.GroupDescriptors.Reset();
            this.SupplierRadGridView.GroupDescriptors.Reset();
            this.ManufactureRadGridView.GroupDescriptors.Reset();
            //设置保存、放弃修改按钮的可用性
            this.isChanged = false;
        }
        private bool _canAbort = false;
        bool CanAbort(object sender)
        {
            return this._canAbort;
        }

        #endregion



        #endregion

        #region Methods


        #region ViewModel 方法 SaveColorSetingXml --更改颜色配置XML

        /// <summary>
        /// 将颜色配置XML转换成颜色配置对象的集合
        /// </summary>
        /// <returns></returns>
        public void SaveColorSettingXml()
        {
            //将颜色配置对象的集合转换成颜色配置XML
            XElement FleetColorSet = new XElement("FleetColorSet");
            XElement ColorSet = new XElement("ColorSet");
            FleetColorSet.Add(ColorSet);
            //航空公司节点
            XElement AirLineNode = new XElement("Type", new XAttribute("TypeName", "航空公司"));
            ColorSet.Add(AirLineNode);
            foreach (xmlItem xmlItem in AirLineList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                AirLineNode.Add(ItemNode);
            }
            foreach (xmlItem xmlItem in RestAirLineList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                AirLineNode.Add(ItemNode);
            }
            //座级节点
            XElement AircraftRegionalNode = new XElement("Type", new XAttribute("TypeName", "座级"));
            ColorSet.Add(AircraftRegionalNode);
            foreach (xmlItem xmlItem in RegionalList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                AircraftRegionalNode.Add(ItemNode);
            }
            //机型节点
            XElement AircraftTypeNode = new XElement("Type", new XAttribute("TypeName", "机型"));
            ColorSet.Add(AircraftTypeNode);
            foreach (xmlItem xmlItem in AircraftTypeList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                AircraftTypeNode.Add(ItemNode);
            }
            //机龄节点
            XElement AircraftAgeNode = new XElement("Type", new XAttribute("TypeName", "机龄"));
            ColorSet.Add(AircraftAgeNode);
            foreach (xmlItem xmlItem in AircraftAgeList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                AircraftAgeNode.Add(ItemNode);
            }
            //引进类型节点
            XElement ImportTypeNode = new XElement("Type", new XAttribute("TypeName", "引进方式"));
            ColorSet.Add(ImportTypeNode);
            foreach (xmlItem xmlItem in ImportTypeList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                ImportTypeNode.Add(ItemNode);
            }
            //供应商节点
            XElement SupplierNode = new XElement("Type", new XAttribute("TypeName", "供应商"));
            ColorSet.Add(SupplierNode);
            foreach (xmlItem xmlItem in SupplierList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                SupplierNode.Add(ItemNode);
            }
            //制造商节点
            XElement ManafacturerNode = new XElement("Type", new XAttribute("TypeName", "制造商"));
            ColorSet.Add(ManafacturerNode);
            foreach (xmlItem xmlItem in ManufacturerList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                ManafacturerNode.Add(ItemNode);
            }
            //运力变化节点
            XElement AircraftTrendNode = new XElement("Type", new XAttribute("TypeName", "运力变化"));
            ColorSet.Add(AircraftTrendNode);
            foreach (xmlItem xmlItem in AircraftTrendList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                AircraftTrendNode.Add(ItemNode);
            }

            if (ColorSettingXml != null)
            {
                this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置").XmlContent = FleetColorSet;
            }
            this._service.SubmitChanges(CAFMStrings.SaveSuccess, CAFMStrings.SaveFail, sm => { }, null);
        }
        #endregion


        #region ViewModel 方法 ValidName --判断读取的航空公司颜色XML对应的航空公司为需要上报的航空公司

        /// <summary>
        /// 判断读取的航空公司颜色XML对应的航空公司为当前航空公司或其子公司
        /// </summary>
        private bool ValidName(string name)
        {
            IEnumerable<Airlines> airlines = this._service.EntityContainer.GetEntitySet<Owner>().OfType<Airlines>();
            bool valid = false;
            foreach (Airlines airline in airlines)
            {
                if (name == airline.ShortName && airline.IsValid == true) valid = true;
            }
            return valid;
        }
        #endregion
        #endregion

        #region Class

        /// <summary>
        ///XML读取的项
        /// </summary>
        public class xmlItem
        {
            public string Name { get; set; }
            public string Color { get; set; }
            public Color ChangeColor { get; set; }
        }

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            SubscribeEvent();
            ViewModelInitializer();
            this.SaveCommand = new DelegateCommand<object>(this.OnSave, this.CanSave);
            this.AbortCommand = new DelegateCommand<object>(this.OnAbort, this.CanAbort);
        }

        #endregion

        #region 订阅事件处理

        private void SubscribeEvent()
        {
            //EventAggregatorRepository.EventAggregator.GetEvent<IsLoadingEvent<ViewModel>>().Subscribe(this.OnLoad, ThreadOption.UIThread);
        }

        private void OnLoad(bool isLoading)
        {
            this.IsBusy = true;

        }

        #endregion

        #region INavigationAware Members

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadXmlConfig();
            this.AirLineRadGridView.GroupDescriptors.Reset();
            this.RegionalRadGridView.GroupDescriptors.Reset();
            this.AircraftTypeRadGridView.GroupDescriptors.Reset();
            this.AircraftAgeRadGridView.GroupDescriptors.Reset();
            this.ImportTypeRadGridView.GroupDescriptors.Reset();
            this.AircraftTrendRadGridView.GroupDescriptors.Reset();
            this.SupplierRadGridView.GroupDescriptors.Reset();
            this.ManufactureRadGridView.GroupDescriptors.Reset();
        }

        #endregion

        #region IConfirmNavigationRequest Members

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            if (this._canSave == true)
            {
                this.confirmExitInteractionRequest.Raise(
                     new ConfirmViewModel { ConfirmContent = CAFMStrings.ConfirmNavigateAwayFromRequestDetail, ConfirmTitle = CAFMStrings.ConfirmNavigateAwayFromRequestDetailTitle },
                     c =>
                     {
                         if (c.Result)
                         {
                             this._service.RejectChanges();
                             this.isChanged = false;
                             continuationCallback(true);
                         }
                         else 
                         { 
                             continuationCallback(false);
                             var navigation = new MessageOperationLibrary.Events.NavigationEvent(false);
                             MessageEventAggregatorRepository.EventAggregator.Publish<MessageOperationLibrary.Events.NavigationEvent>(navigation);
                         }
                     });

            }
            else
                continuationCallback(true);
        }

        #endregion
    }

}
