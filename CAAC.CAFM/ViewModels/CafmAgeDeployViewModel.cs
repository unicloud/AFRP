using CAAC.Fleet.Services;
using MessageOperationLibrary.ApplicationModel.Eventing;
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmAgeDeployViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmAgeDeployViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmAgeDeployViewModel()
        {

            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
            CreatAgeDeployCollection();
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmAgeDeployView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmAgeDeployView>();
        private RadGridView AgeDeployGridView;//机龄配置列表

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
            this.AgeDeployGridView = _view.AgeDeploy as RadGridView;
            //定义数据编辑的事件（控制保存、放弃修改按钮的可用性）
            AgeDeployGridView.CellEditEnded += new EventHandler<GridViewCellEditEndedEventArgs>(AgeDeployGridView_CellEditEnded);
        }

        /// <summary>
        /// 控制保存、放弃修改按钮的可用性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AgeDeployGridView_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            if (e.Cell.Column.Header.ToString() != "对应颜色" && Convert.ToUInt32(e.NewData) != Convert.ToUInt32(e.OldData))
            {
                this.isChanged = true;
            }
            else if (e.Cell.Column.Header.ToString() == "对应颜色")
            {
                this.isChanged = true;
            }
        }

        #endregion

        #endregion

        #region ViewModel

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

        #region ViewModel 属性 AgeDeployCollection --机龄配置的集合

        private List<AgeDeploy> _AgeDeployCollection = new List<AgeDeploy>();
        /// <summary>
        /// 机龄配置的集合
        /// </summary>
        public List<AgeDeploy> AgeDeployCollection
        {
            get { return _AgeDeployCollection; }
            set
            {
                _AgeDeployCollection = value;
                this.RaisePropertyChanged(() => this.AgeDeployCollection);
            }
        }

        #endregion

        #endregion

        #region Command

        #region ViewModel 命令 -- 创建新区间

        public DelegateCommand<object> AddCommand { get; private set; }
        private void OnAdd(object sender)
        {
            AgeDeploy agedeploy = new AgeDeploy();
            List<AgeDeploy> collection = new List<AgeDeploy>();
            collection.AddRange(AgeDeployCollection);
            collection.Add(agedeploy);
            this.AgeDeployCollection = collection;
            this.AgeDeployGridView.SelectedItem = agedeploy;
            //设置保存、放弃修改按钮的可用性
            this.isChanged = true;
        }
        private bool _canAdd = true;
        bool CanAdd(object sender)
        {
            return this._canAdd;
        }

        #endregion

        #region ViewModel 命令 -- 移除区间

        public DelegateCommand<object> RemoveCommand { get; private set; }
        private void OnRemove(object sender)
        {
            AgeDeploy selectedagedeploy = AgeDeployGridView.SelectedItem as AgeDeploy;
            if (selectedagedeploy != null)
            {
                int selectindex = AgeDeployCollection.IndexOf(selectedagedeploy);
                List<AgeDeploy> collection = new List<AgeDeploy>();
                collection.AddRange(AgeDeployCollection);
                collection.Remove(selectedagedeploy);
                this.AgeDeployCollection = collection;
                //设置选中行
                if (AgeDeployCollection.Count() > 0 && selectindex > 0)
                {
                    if (AgeDeployCollection.Count() > selectindex)
                    {
                        this.AgeDeployGridView.SelectedItem = AgeDeployCollection[selectindex];
                    }
                    else
                    {
                        this.AgeDeployGridView.SelectedItem = AgeDeployCollection[AgeDeployCollection.Count() - 1];
                    }
                }

                //设置保存、放弃修改按钮的可用性
                this.isChanged = true;
            }
        }
        private bool _canRemove = true;
        bool CanRemove(object sender)
        {
            return this._canRemove;
        }

        #endregion


        #region ViewModel 命令 -- 保存

        public DelegateCommand<object> SaveCommand { get; private set; }
        private void OnSave(object sender)
        {
            SaveAgeDeploXml();
            //设置保存、放弃修改按钮的可用性
            this.isChanged = false;
            this._view.Tag = true;
        }
        private bool _canSave = false;
        bool CanSave(object sender)
        {
            return this._canSave;
        }

        #endregion


        #region ViewModel 命令 -- 放弃更改

        public DelegateCommand<object> AbortCommand { get; private set; }
        private void OnAbort(object sender)
        {
            CreatAgeDeployCollection();
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

        /// <summary>
        /// 将机龄配置XML转换成机龄配置对象的集合
        /// </summary>
        /// <returns></returns>
        private void CreatAgeDeployCollection()
        {
            List<AgeDeploy> collection = new List<AgeDeploy>();//将机龄配置XML转换成机龄配置对象的集合

            if (this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "机龄配置") != null)
            {
                XElement xelement = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "机龄配置").XmlContent;

                XElement agecolor = null;
                XmlConfig colorconfig = this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置");
                if (colorconfig != null && colorconfig.XmlContent.Descendants("Type").Any(p => p.Attribute("TypeName").Value == "机龄"))
                {
                    agecolor = colorconfig.XmlContent.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "机龄");
                }
                if (xelement != null)
                {
                    foreach (var item in xelement.Descendants("Item"))
                    {
                        AgeDeploy agedeploy = new AgeDeploy();
                        agedeploy.Name = item.Value;
                        agedeploy.StartYear = Convert.ToUInt32(item.Attribute("Start").Value);
                        agedeploy.EndYear = Convert.ToUInt32(item.Attribute("End").Value);
                        if (agecolor != null)
                        {
                            agedeploy.Color = agecolor.Descendants("Item")
                                .FirstOrDefault(p => p.Attribute("Name").Value == agedeploy.Name).Attribute("Color").Value;
                        }
                        collection.Add(agedeploy);
                    }
                }
            }
            AgeDeployCollection = collection;
        }


        /// <summary>
        /// 将机龄配置对象的集合转换成机龄配置XML
        /// </summary>
        /// <returns></returns>
        private void SaveAgeDeploXml()
        {
            //将机龄配置对象的集合转换成机龄配置XML
            XElement AgeDeploy = new XElement("AgeDeploy");
            //机龄范围的颜色节点
            XElement ColorNode = new XElement("Type", new XAttribute("TypeName", "机龄"));
            foreach (AgeDeploy agedeploy in AgeDeployCollection)
            {
                string name = agedeploy.StartYear + "至" + agedeploy.EndYear + "年之间";
                //供应商节点
                XElement ItemNode = new XElement("Item", new XAttribute("Start", agedeploy.StartYear), new XAttribute("End", agedeploy.EndYear), name);
                AgeDeploy.Add(ItemNode);

                //颜色节点
                XElement ChildNode = new XElement("Item", new XAttribute("Name", name), new XAttribute("Color", agedeploy.Color));
                ColorNode.Add(ChildNode);
            }


            if (this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "机龄配置") != null)
            {
                this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "机龄配置").XmlContent = AgeDeploy;
            }
            else
            {
                XmlConfig xmlconfig = new XmlConfig();
                xmlconfig.XmlConfigID = Guid.NewGuid();
                xmlconfig.ConfigType = "机龄配置";
                xmlconfig.VersionNumber = 1;
                xmlconfig.XmlContent = AgeDeploy;
                this._service.EntityContainer.GetEntitySet<XmlConfig>().Add(xmlconfig);
            }
            XElement colorxelement = XElement.Parse(this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置").ConfigContent);
            colorxelement.Descendants("Type").FirstOrDefault(p => p.Attribute("TypeName").Value == "机龄").Remove();
            colorxelement.Add(ColorNode);
            this.ViewXmlConfig.FirstOrDefault(p => p.ConfigType == "颜色配置").XmlContent = colorxelement;
            //this._service.SubmitChanges(CAFMStrings.SaveSuccess, CAFMStrings.SaveFail, sm => { }, null);
            this._service.SubmitChanges();
        }

        #endregion

        #region Class

        /// <summary>
        /// 机龄配置的对象
        /// </summary>
        public class AgeDeploy
        {
            public AgeDeploy()
            {
                this.Color = new CommonMethod().GetRandomColor();
            }
            public string Name { get; set; }//机龄区间的名称
            public uint StartYear { get; set; }//开始年份
            public uint EndYear { get; set; }//结束年份
            public string Color { get; set; }//颜色
        }

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            SubscribeEvent();
            ViewModelInitializer();

            this.SaveCommand = new DelegateCommand<object>(this.OnSave, this.CanSave);
            this.AddCommand = new DelegateCommand<object>(this.OnAdd, this.CanAdd);
            this.RemoveCommand = new DelegateCommand<object>(this.OnRemove, this.CanRemove);
            this.AbortCommand = new DelegateCommand<object>(this.OnAbort, this.CanAbort);

        }

        #endregion

        #region 订阅事件处理

        private void SubscribeEvent()
        {
            //EventAggregatorRepository.EventAggregator.GetEvent<IsLoadingEvent<CafmFleetQueryViewModel>>().Subscribe(this.OnLoad, ThreadOption.UIThread);
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

        }

        #endregion

        #region IConfirmNavigationRequest Members

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        #endregion


    }

}
