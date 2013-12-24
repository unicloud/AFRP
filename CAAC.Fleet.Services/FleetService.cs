using System.Collections.ObjectModel;
using CAAC.Fleet.Services.Web;
using CAAC.Infrastructure;
using MessageOperationLibrary.Events;
using Microsoft.Windows.Data.DomainServices;
using Ria.Common;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using UniCloud.Fleet.Models;
using MessageOperationLibrary.EventAggregatorRepository;

namespace CAAC.Fleet.Services
{
    public interface IFleetService
    {


        #region 加载完成属性

        bool AllXmlConfigLoaded { get; }
        bool AllOwnerLoaded { get; }
        bool AllAircraftCategoryLoaded { get; }
        bool AllAircraftTypeLoaded { get; }
        bool AllAircraftLoaded { get; }
        bool AllPlanAircraftLoaded { get; }
        bool AllOperationHistoryLoaded { get; }
        bool AllOwnershipHistoryLoaded { get; }
        bool AllAircraftBusinessLoaded { get; }
        bool AllActionCategoryLoaded { get; }
        bool AllAnnualLoaded { get; }
        bool AllPlanLoaded { get; }
        bool AllRequestLoaded { get; }
        bool AllApprovalDocLoaded { get; }
        bool AllMailAddressLoaded { get; }

        #endregion

        #region 公共

        string CAACID { get; }
        Airlines CurrentAirlines { get; }
        FleetDomainContext Context { get; }
        EntityContainer EntityContainer { get; }
        void SubmitChanges();
        void SubmitChanges(Action<ServiceSubmitChangesResult> callback, object state);
        void SubmitChanges(string success, string fail, Action<ServiceSubmitChangesResult> callback, object state);

        void RejectChanges();
        void LoadXmlConfig(QueryBuilder<XmlConfig> query, Action<ServiceLoadResult<XmlConfig>> callback, object state);//加载基础配置

        #endregion

        #region 参与者

        void LoadOwner(QueryBuilder<Owner> query, Action<ServiceLoadResult<Owner>> callback, object state);//加载所有权人
        void LoadManager(QueryBuilder<Manager> query, Action<ServiceLoadResult<Manager>> callback, object state);//加载管理者
        void LoadAirlines(QueryBuilder<Airlines> query, Action<ServiceLoadResult<Airlines>> callback, object state);//加载航空公司
        void LoadManufacturer(QueryBuilder<Manufacturer> query, Action<ServiceLoadResult<Manufacturer>> callback, object state);//加载制造商

        #endregion

        #region 飞机

        void LoadAircraftCategory(QueryBuilder<AircraftCategory> query, Action<ServiceLoadResult<AircraftCategory>> callback, object state);//加载飞机类别
        void LoadAircraftType(QueryBuilder<AircraftType> query, Action<ServiceLoadResult<AircraftType>> callback, object state);//加载飞机机型
        void LoadAircraft(QueryBuilder<Aircraft> query, Action<ServiceLoadResult<Aircraft>> callback, object state);//加载飞机
        void LoadPlanAircraft(QueryBuilder<PlanAircraft> query, Action<ServiceLoadResult<PlanAircraft>> callback, object state);//加载计划飞机
        void LoadApprovalHistory(QueryBuilder<ApprovalHistory> query, Action<ServiceLoadResult<ApprovalHistory>> callback, object state);//加载批文历史
        void LoadOperationHistory(QueryBuilder<OperationHistory> query, Action<ServiceLoadResult<OperationHistory>> callback, object state);//加载运营权历史
        void LoadPlanHistory(QueryBuilder<PlanHistory> query, Action<ServiceLoadResult<PlanHistory>> callback, object state);//加载计划历史
        void LoadAircraftBusiness(QueryBuilder<AircraftBusiness> query, Action<ServiceLoadResult<AircraftBusiness>> callback, object state);//加载飞机商业数据
        void LoadOwnershipHistory(QueryBuilder<OwnershipHistory> query, Action<ServiceLoadResult<OwnershipHistory>> callback, object state);//加载所有权历史
        #endregion

        #region 机队管理

        void LoadActionCategory(QueryBuilder<ActionCategory> query, Action<ServiceLoadResult<ActionCategory>> callback, object state);//加载操作类别

        #region 年度

        void LoadAnnual(QueryBuilder<Annual> query, Action<ServiceLoadResult<Annual>> callback, object state);//加载年度

        #endregion

        #region 计划

        void LoadPlan(QueryBuilder<Plan> query, Action<ServiceLoadResult<Plan>> callback, object state);//加载计划

        #endregion

        #region 申请

        void LoadRequest(QueryBuilder<Request> query, Action<ServiceLoadResult<Request>> callback, object state);//加载申请，包括明细及引进方式
        void LoadApprovalDoc(QueryBuilder<ApprovalDoc> query, Action<ServiceLoadResult<ApprovalDoc>> callback, object state);//加载批文内容
        void LoadManaApprovalHistory(QueryBuilder<ManaApprovalHistory> query, Action<ServiceLoadResult<ManaApprovalHistory>> callback, object state);//加载批文内容
        #endregion

        #region XmlConfig生成

        //更新XmlConfig内容
        void UpdateXmlConfigContent(string XmlConfigType, Action<InvokeCompletedResult> callback, object state);

        //更新XmlConfig标志
        void UpdateAllXmlConfigFlag();

        #endregion

        #endregion

        #region 邮件

        void LoadMailAddress(QueryBuilder<MailAddress> query, Action<ServiceLoadResult<MailAddress>> callback, object state);//加载基础配置

        #endregion

    }

    [Export(typeof(IFleetService))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class FleetService : IFleetService
    {
        private readonly FleetDomainContext _context = new FleetDomainContext();
        private Airlines currentAirlines;

        #region 加载完成属性

        private bool _allXmlConfigLoaded, _allOwnerLoaded, _allAircraftCategoryLoaded, _allAircraftTypeLoaded, _allAircraftLoaded,
            _allPlanAircraftLoaded, _allOperationHistoryLoaded, _allOwnershipHistoryLoaded, _allAircraftBusinessLoaded, _allActionCategoryLoaded, _allAnnualLoaded,
            _allPlanLoaded, _allRequestLoaded, _allApprovalDocLoaded, _AllManaApprovalLoaded, _allMailAddressLoaded;

        public bool AllXmlConfigLoaded { get { return this._allXmlConfigLoaded; } }
        public bool AllOwnerLoaded { get { return this._allOwnerLoaded; } }
        public bool AllAircraftCategoryLoaded { get { return this._allAircraftCategoryLoaded; } }
        public bool AllAircraftTypeLoaded { get { return this._allAircraftTypeLoaded; } }
        public bool AllAircraftLoaded { get { return this._allAircraftLoaded; } }
        public bool AllPlanAircraftLoaded { get { return this._allPlanAircraftLoaded; } }
        public bool AllOperationHistoryLoaded { get { return this._allOperationHistoryLoaded; } }
        public bool AllOwnershipHistoryLoaded { get { return this._allOwnershipHistoryLoaded; } }
        public bool AllAircraftBusinessLoaded { get { return this._allAircraftBusinessLoaded; } }
        public bool AllActionCategoryLoaded { get { return this._allActionCategoryLoaded; } }
        public bool AllAnnualLoaded { get { return this._allAnnualLoaded; } }
        public bool AllPlanLoaded { get { return this._allPlanLoaded; } }
        public bool AllRequestLoaded { get { return this._allRequestLoaded; } }
        public bool AllApprovalDocLoaded { get { return this._allApprovalDocLoaded; } }
        public bool AllManaApprovalLoaded { get { return this._AllManaApprovalLoaded; } }
        public bool AllMailAddressLoaded { get { return this._allMailAddressLoaded; } }

        #endregion

        #region 公共

        public Airlines CurrentAirlines { get { return this.currentAirlines; } }

        public string CAACID
        {
            get
            {
                return "31A9DE51-C207-4A73-919C-21521F17FEF9";
            }
        }

        public FleetDomainContext Context
        {
            get { return this._context; }
        }

        public EntityContainer EntityContainer
        {
            get { return this._context.EntityContainer; }
        }

        public void SubmitChanges()
        {
            this._context.SubmitChanges();
            this._context.UpdateAllXmlConfigFlag();
            this._allXmlConfigLoaded = false;
        }

        public void SubmitChanges(Action<ServiceSubmitChangesResult> callback, object state)
        {
            this._context.SubmitChanges(sm =>
            {
                callback(this.SubmitResult(sm));
                this._context.UpdateAllXmlConfigFlag();
                this._allXmlConfigLoaded = false;
            }, state);
        }

        /// <summary>
        /// 保存实体变化
        /// </summary>
        /// <param name="success">保存成功所显示消息</param>
        /// <param name="fail">保存失败所显示消息</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        public void SubmitChanges(string success, string fail, Action<ServiceSubmitChangesResult> callback, object state)
        {
            var message = new MessageOperationLibrary.Events.MessageEvent();
            this._context.SubmitChanges(sm =>
            {
                if (sm.Error != null)
                {
                    message.Message = fail;
                    message.MessageType = MessageType.Fail;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(message);
                    //EventAggregatorRepository.EventAggregator.GetEvent<OprationMessageEvent>().Publish(Tuple.Create(OperationMessageType.Fail, fail));
                }
                else if (!sm.IsCanceled)
                {
                    message.Message = success;
                    message.MessageType = MessageType.Success;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(message);
                    //    EventAggregatorRepository.EventAggregator.GetEvent<OprationMessageEvent>().Publish(Tuple.Create(OperationMessageType.Success, success));
                }
                callback(this.SubmitResult(sm));
                //TODO 更改state，不需要每次都去刷新
                this._context.UpdateAllXmlConfigFlag();
                this._allXmlConfigLoaded = false;
            }, state);
        }

        public void RejectChanges()
        {
            this._context.RejectChanges();
        }

        public void LoadXmlConfig(QueryBuilder<XmlConfig> query, Action<ServiceLoadResult<XmlConfig>> callback, object state)
        {
            if (!this._allXmlConfigLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetXmlConfigQuery()), lo =>
                {
                    this._allXmlConfigLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<XmlConfig>()));
            }
        }

        private void Load<T>(EntityQuery<T> query, Action<LoadOperation<T>> callback, object state) where T : Entity
        {
            this._context.Load(query, lo =>
            {
                callback(lo);
            }, state);
        }

        private ServiceLoadResult<T> CreateResult<T>(LoadOperation<T> op, bool returnEditableCollection = false) where T : Entity
        {
            if (op.HasError)
            {
                op.MarkErrorAsHandled();
            }
            return new ServiceLoadResult<T>(
                returnEditableCollection ? new EntityList<T>(this.EntityContainer.GetEntitySet<T>(), op.Entities) : op.Entities,
                op.TotalEntityCount,
                op.ValidationErrors,
                op.Error,
                op.IsCanceled,
                op.UserState);
        }

        private ServiceLoadResult<T> CreateResult<T>(EntitySet<T> entities) where T : Entity
        {
            return new ServiceLoadResult<T>(
                entities,
                null);
        }

        private ServiceSubmitChangesResult SubmitResult(SubmitOperation sm)
        {
            if (sm.HasError)
            {
                sm.MarkErrorAsHandled();
            }
            return new ServiceSubmitChangesResult(sm.ChangeSet, sm.EntitiesInError, sm.Error, sm.CanCancel, sm.UserState);
        }

        private InvokeCompletedResult InvokeResult(InvokeOperation io)
        {
            if (io.HasError)
            {
                io.MarkErrorAsHandled();
            }
            return new InvokeCompletedResult(io.Value, io.ValidationErrors);
        }

        #endregion

        #region 参与者

        public void LoadOwner(QueryBuilder<Owner> query, Action<ServiceLoadResult<Owner>> callback, object state)
        {
            if (!this._allOwnerLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetOwnerQuery()), lo =>
                {

                    this._allOwnerLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<Owner>()));
            }
        }

        public void LoadManager(QueryBuilder<Manager> query, Action<ServiceLoadResult<Manager>> callback, object state)
        {
            this.Load(query.ApplyTo(this._context.GetManagerQuery()), lo =>
            {
                callback(this.CreateResult(lo));
            }, state);
        }

        public void LoadAirlines(QueryBuilder<Airlines> query, Action<ServiceLoadResult<Airlines>> callback, object state)
        {
            this.Load(query.ApplyTo(this._context.GetAirLinesQuery()), lo =>
            {
                callback(this.CreateResult(lo));
            }, state);
        }

        public void LoadManufacturer(QueryBuilder<Manufacturer> query, Action<ServiceLoadResult<Manufacturer>> callback, object state)
        {
            this.Load(query.ApplyTo(this._context.GetManufacturerQuery()), lo =>
            {
                callback(this.CreateResult(lo));
            }, state);
        }

        #endregion

        #region 飞机

        public void LoadAircraftCategory(QueryBuilder<AircraftCategory> query, Action<ServiceLoadResult<AircraftCategory>> callback, object state)
        {
            if (!this._allAircraftCategoryLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAircraftCategoryQuery()), lo =>
                {

                    this._allAircraftCategoryLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<AircraftCategory>()));
            }
        }

        public void LoadAircraftType(QueryBuilder<AircraftType> query, Action<ServiceLoadResult<AircraftType>> callback, object state)
        {
            if (!this._allAircraftTypeLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAircraftTypeQuery()), lo =>
                {

                    this._allAircraftTypeLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<AircraftType>()));
            }
        }

        public void LoadAircraft(QueryBuilder<Aircraft> query, Action<ServiceLoadResult<Aircraft>> callback, object state)
        {
            if (!this._allAircraftLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAircraftQuery()), lo =>
                {
                    this._allAircraftLoaded = true;
                    this.currentAirlines = _context.Owners.OfType<Airlines>().FirstOrDefault(r => r.ICAOCode == AppConfig.orgCode);
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<Aircraft>()));
            }
        }

        public void LoadPlanAircraft(QueryBuilder<PlanAircraft> query, Action<ServiceLoadResult<PlanAircraft>> callback, object state)
        {
            if (!this._allPlanAircraftLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetPlanAircraftQuery()), lo =>
                {
                    this._allPlanAircraftLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<PlanAircraft>()));
            }
        }

        public void LoadApprovalHistory(QueryBuilder<ApprovalHistory> query, Action<ServiceLoadResult<ApprovalHistory>> callback, object state)
        {
            this.Load(query.ApplyTo(this._context.GetApprovalHistoryQuery()), lo =>
            {
                callback(this.CreateResult(lo));
            }, state);
        }

        public void LoadOperationHistory(QueryBuilder<OperationHistory> query, Action<ServiceLoadResult<OperationHistory>> callback, object state)
        {
            if (!this._allOperationHistoryLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetOperationHistoryQuery()), lo =>
                {
                    this._allOperationHistoryLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<OperationHistory>()));
            }
        }

        public void LoadOwnershipHistory(QueryBuilder<OwnershipHistory> query, Action<ServiceLoadResult<OwnershipHistory>> callback, object state)
        {
            if (!this._allOwnershipHistoryLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetOwnershipHistoryQuery()), lo =>
                {
                    this._allOwnershipHistoryLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<OwnershipHistory>()));
            }
        }

        public void LoadPlanHistory(QueryBuilder<PlanHistory> query, Action<ServiceLoadResult<PlanHistory>> callback, object state)
        {
            this.Load(query.ApplyTo(this._context.GetPlanHistoryQuery()), lo =>
            {
                callback(this.CreateResult(lo));
            }, state);
        }

        public void LoadAircraftBusiness(QueryBuilder<AircraftBusiness> query, Action<ServiceLoadResult<AircraftBusiness>> callback, object state)
        {
            if (!this._allAircraftBusinessLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAircraftBusinessQuery()), lo =>
                {
                    this._allAircraftBusinessLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<AircraftBusiness>()));
            }
        }

        #endregion

        #region 机队管理


        public void LoadActionCategory(QueryBuilder<ActionCategory> query, Action<ServiceLoadResult<ActionCategory>> callback, object state)
        {
            if (!this._allActionCategoryLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetActionCategoryQuery()), lo =>
                {

                    this._allActionCategoryLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<ActionCategory>()));
            }
        }

        #region 年度

        public void LoadAnnual(QueryBuilder<Annual> query, Action<ServiceLoadResult<Annual>> callback, object state)
        {
            if (!this._allAnnualLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAnnualQuery()), lo =>
                {

                    this._allAnnualLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<Annual>()));
            }
        }

        #endregion

        #region 计划

        public void LoadPlan(QueryBuilder<Plan> query, Action<ServiceLoadResult<Plan>> callback, object state)
        {
            if (!this._allPlanLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetPlanQuery()), lo =>
                {
                    this._allPlanLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<Plan>()));
            }
        }

        #endregion

        #region 申请

        public void LoadRequest(QueryBuilder<Request> query, Action<ServiceLoadResult<Request>> callback, object state)
        {
            if (!this._allRequestLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetRequestQuery()), lo =>
                {
                    this._allRequestLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<Request>()));
            }
        }

        public void LoadApprovalDoc(QueryBuilder<ApprovalDoc> query, Action<ServiceLoadResult<ApprovalDoc>> callback, object state)
        {
            if (!this._allApprovalDocLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetApprovalDocQuery()), lo =>
                {
                    this._allApprovalDocLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<ApprovalDoc>()));
            }
        }


        public void LoadManaApprovalHistory(QueryBuilder<ManaApprovalHistory> query, Action<ServiceLoadResult<ManaApprovalHistory>> callback, object state)
        {
            if (!this._AllManaApprovalLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetManaApprovalHistoryQuery()), lo =>
                {
                    this._AllManaApprovalLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<ManaApprovalHistory>()));
            }
        }

        #endregion

        #endregion

        #region 邮件

        public void LoadMailAddress(QueryBuilder<MailAddress> query, Action<ServiceLoadResult<MailAddress>> callback, object state)
        {
            if (!this._allMailAddressLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetMailAddressQuery()), lo =>
                {
                    this._allMailAddressLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(this.CreateResult(this.EntityContainer.GetEntitySet<MailAddress>()));
            }
        }

        #endregion

        #region XmlConfig生成

        //更新XmlConfig内容
        public void UpdateXmlConfigContent(string XmlConfigType, Action<InvokeCompletedResult> callback, object state)
        {
            this._context.UpdateXmlConfigContent(XmlConfigType, sm =>
                {
                    callback(this.InvokeResult(sm));
                }, state);
        }

        //更新XmlConfig标志
        public void UpdateAllXmlConfigFlag()
        {
            this._context.UpdateAllXmlConfigFlag();
        }

        #endregion

        #region "数据库备份与恢复"
        public bool BackupDataBase(string DatabaseName, string FilePath, string FileName)
        {
            this._context.BackupDataBase(DatabaseName, FilePath, FileName);
            return true;
        }

        public bool RestoreDataBase(string DatabaseName, string FilePath, string FileName)
        {
            this._context.RestoreDataBase(DatabaseName, FilePath, FileName);
            return true;
        }
        #endregion
    }
}
