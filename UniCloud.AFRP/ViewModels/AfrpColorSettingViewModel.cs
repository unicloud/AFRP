using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.Prism.Regions;
using UniCloud.Infrastructure;
using Microsoft.Practices.Prism.Events;
using Telerik.Windows;
using UniCloud.Fleet.Services;
using Microsoft.Practices.ServiceLocation;
using Telerik.Windows.Controls;
using System.ComponentModel;
using UniCloud.Fleet.Models;
using System.Collections.Generic;
using Microsoft.Windows.Data.DomainServices;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using MessageOperationLibrary.EventAggregatorRepository;


namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpColorSettingViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class AfrpColorSettingViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public AfrpColorSettingViewModel()
        {
            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
            this.confirmExitInteractionRequest = new InteractionRequest<ConfirmViewModel>();
        }

        #region Local


        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        UniCloud.AFRP.Views.AfrpColorSettingView _view = ServiceLocator.Current.GetInstance<UniCloud.AFRP.Views.AfrpColorSettingView>();
        private RadGridView AirLineRadGridView, RegionalRadGridView, AircraftTypeRadGridView, AircraftAgeRadGridView, ImportTypeRadGridView, AircraftTrendRadGridView;
        private List<xmlItem> airlinelist = new List<xmlItem>();
        private List<xmlItem> restairlinelist = new List<xmlItem>();//用来保存展示没被用到的航空公司
        private List<xmlItem> regionallist = new List<xmlItem>();
        private List<xmlItem> aircrafttypelist = new List<xmlItem>();
        private List<xmlItem> importtypelist = new List<xmlItem>();
        private List<xmlItem> aircraftagelist = new List<xmlItem>();
        private List<xmlItem> aircraftrendlist = new List<xmlItem>();

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
            this.ImportTypeRadGridView = _view.ImportTypeRadGridView as RadGridView;
            this.AircraftAgeRadGridView = _view.AircraftAgeRadGridView as RadGridView;
            this.AircraftTrendRadGridView = _view.AircraftTrendRadGridView as RadGridView;
            //定义数据编辑的事件（控制保存、放弃修改按钮的可用性）
            AirLineRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            RegionalRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            AircraftTypeRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            ImportTypeRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
            AircraftAgeRadGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(FleetColorSetGridView_CellEditEnded);
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

        #endregion

        #endregion

        #region ViewModel

        #region 加载颜色配置的集合 ColorSettingXml

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
            get { return this._AirLineList; }
            set
            {
                if (this._AirLineList != value)
                {
                    _AirLineList = value;
                    this.RaisePropertyChanged(() => this.AirLineList);
                }
            }
        }
        #endregion

        #region ViewModel 属性 RestAirLineList --暂时用不上的航空公司颜色配置集合

        private List<xmlItem> _RestAirLineList;
        /// <summary>
        /// 暂时用不上的航空公司颜色配置集合
        /// </summary>
        public List<xmlItem> RestAirLineList
        {
            get { return this._RestAirLineList; }
            set
            {
                _RestAirLineList = value;
                this.RaisePropertyChanged(() => this.RestAirLineList);
            }
        }
        #endregion

        #region ViewModel 属性 RegionalList --座级颜色配置集合

        private List<xmlItem> _RegionalList;
        /// <summary>
        /// 座级颜色配置集合
        /// </summary>
        public List<xmlItem> RegionalList
        {
            get { return this._RegionalList; }
            set
            {

                _RegionalList = value;
                this.RaisePropertyChanged(() => this.RegionalList);
            }
        }
        #endregion

        #region ViewModel 属性 AircraftTypeList --机型颜色配置集合

        private List<xmlItem> _AircraftTypeList;
        /// <summary>
        /// 机型颜色配置集合
        /// </summary>
        public List<xmlItem> AircraftTypeList
        {
            get { return this._AircraftTypeList; }
            set
            {
                _AircraftTypeList = value;
                this.RaisePropertyChanged(() => this.AircraftTypeList);
            }
        }
        #endregion

        #region ViewModel 属性 ImportTypeList --引进方式颜色配置集合

        private List<xmlItem> _ImportTypeList;
        /// <summary>
        /// 引进方式颜色配置集合
        /// </summary>
        public List<xmlItem> ImportTypeList
        {
            get { return this._ImportTypeList; }
            set
            {
                _ImportTypeList = value;
                this.RaisePropertyChanged(() => this.ImportTypeList);
            }
        }
        #endregion

        #region ViewModel 属性 AircraftAgeList --机龄颜色配置集合

        private List<xmlItem> _AircraftAgeList;
        /// <summary>
        /// 机龄颜色配置集合
        /// </summary>
        public List<xmlItem> AircraftAgeList
        {
            get { return this._AircraftAgeList; }
            set
            {
                _AircraftAgeList = value;
                this.RaisePropertyChanged(() => this.AircraftAgeList);
            }
        }
        #endregion

        #region ViewModel 属性 AircraftTrendList --运力字段颜色配置集合

        private List<xmlItem> _AircraftTrendList;
        /// <summary>
        /// 运力字段颜色配置集合
        /// </summary>
        public List<xmlItem> AircraftTrendList
        {
            get { return this._AircraftTrendList; }
            set
            {
                _AircraftTrendList = value;
                this.RaisePropertyChanged(() => this.AircraftTrendList);
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
            importtypelist.Clear();
            aircraftagelist.Clear();
            aircraftrendlist.Clear();
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
                                if (ValidName(item.Attribute("Name").Value))//判断是否为当前航空公司或其子公司
                                {
                                    xmlItem xmlItem = new xmlItem();
                                    xmlItem.Name = item.Attribute("Name").Value;
                                    xmlItem.Color = item.Attribute("Color").Value; ;
                                    airlinelist.Add(xmlItem);
                                }
                                else
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
                                xmlItem xmlItem = new xmlItem();
                                xmlItem.Name = item.Attribute("Name").Value;
                                xmlItem.Color = item.Attribute("Color").Value; ;
                                aircraftrendlist.Add(xmlItem);
                            }
                            AircraftTrendList = aircraftrendlist;
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
            SaveColorSetingXml();
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
        public void SaveColorSetingXml()
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
            //引进类型节点
            XElement ImportTypeNode = new XElement("Type", new XAttribute("TypeName", "引进方式"));
            ColorSet.Add(ImportTypeNode);
            foreach (xmlItem xmlItem in ImportTypeList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                ImportTypeNode.Add(ItemNode);
            }
            //机龄节点
            XElement AircraftAgeNode = new XElement("Type", new XAttribute("TypeName", "机龄"));
            ColorSet.Add(AircraftAgeNode);
            foreach (xmlItem xmlItem in AircraftAgeList)
            {
                XElement ItemNode = new XElement("Item", new XAttribute("Name", xmlItem.Name), new XAttribute("Color", xmlItem.Color));
                AircraftAgeNode.Add(ItemNode);
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
                ColorSettingXml.XmlContent = FleetColorSet;
            }
            this._service.SubmitChanges(CAFMStrings.SaveSuccess, CAFMStrings.SaveFail, sm => { }, null);
        }
        #endregion

        #region ViewModel 方法 ToColor --将XML中获取的颜色整数值转换成color类型
        /// <summary>
        /// ToColor将XML中获取的颜色整数值转换成color类型
        /// </summary>
        /// <param name="formercolor"></param>
        /// <returns></returns>
        public Color ToColor(Int32 formercolor)
        {
            Int32 r = formercolor % 256;
            Int32 b = formercolor / 65536;
            Int32 g = ((formercolor % 65536) - r) / 256;
            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }
        #endregion


        #region ViewModel 方法 ValidName --判断读取的航空公司颜色XML对应的航空公司为当前航空公司或其子公司

        /// <summary>
        /// 判断读取的航空公司颜色XML对应的航空公司为当前航空公司或其子公司
        /// </summary>
        private bool ValidName(string name)
        {
            var currentAirline = this._service.CurrentAirlines;
            bool valid = false;
            if (name == currentAirline.ShortName || this._service.EntityContainer.GetEntitySet<Owner>().OfType<Airlines>().Any(p => p.MasterID != null && p.ShortName == name)) valid = true;
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
