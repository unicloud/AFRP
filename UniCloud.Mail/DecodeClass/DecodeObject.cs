using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail.DecodeClass
{
    public abstract class DecodeObject : IDecodeObject
    {

        protected FleetEntities _FE;
        protected string _FullTypeName = "";
        protected string _TypeName = "";

        public DecodeObject()
        {
           // _FE = new FleetEntities();
            InitData();
        }

        //public DecodeObject(FleetEntities fe)
        //{
        //    _FE = fe;
        //    InitData();
        //}

        public void SetDbContext(DbContext context)
        {
            this._FE = (FleetEntities)context;
        }

        protected abstract void InitData();

        public abstract void Decode(object obj);

        public virtual bool CanDecode(string objTypeName)
        {
            bool blCan = false;
            if (_FullTypeName != "")
            {
                blCan = objTypeName == _FullTypeName;
            }
            if (!blCan && _TypeName != "")
            {
                blCan = blCan || objTypeName.Contains("."+ _TypeName);
            }
            return blCan;
        }

        #region 更新计划飞机和飞机数据

        //更新计划飞机
        protected void UpdatePlanAircrafts(ICollection<PlanAircraft> NewPlanAircrafts, Aircraft NewAircraft)
        {
            if (NewPlanAircrafts != null)
            {
                foreach (PlanAircraft NewPlanAircraft in NewPlanAircrafts)
                {
                    if (NewPlanAircraft != null)
                    {
                        PlanAircraft OriginPlanAircraft = this._FE.PlanAircrafts.Where(q => q.PlanAircraftID == NewPlanAircraft.PlanAircraftID).FirstOrDefault();

                        //计划飞机数据处理
                        NewPlanAircraft.PlanHistories = null;
                        NewPlanAircraft.ApprovalHistories = null;
                        NewPlanAircraft.Aircraft = null;
                        NewPlanAircraft.AircraftID = NewAircraft.AircraftID;

                        //原来为空,新增
                        if (OriginPlanAircraft == null)
                        {
                            this._FE.PlanAircrafts.Add(NewPlanAircraft);
                        }
                        //原来已有,更新
                        else
                        {
                            this.Update<PlanAircraft>(_FE, OriginPlanAircraft, NewPlanAircraft);
                        }
                    }
                }
            }
        }

        //更新计划飞机
        protected void UpdatePlanAircraft(PlanAircraft NewPlanAircraft)
        {
            if (NewPlanAircraft != null)
            {
                //处理航空公司
                NewPlanAircraft.Airlines = null;
                //计划飞机对应的飞机
                Aircraft NewAircraft = NewPlanAircraft.Aircraft;
                //更新计划飞机对应的飞机
                if (NewAircraft != null)
                {
                    NewAircraft.Airlines = null;
                    UpdateAircraft(NewAircraft, false);
                    NewPlanAircraft.AircraftID = NewAircraft.AircraftID;
                }
                //判断一下，计划飞机对应的飞机是否已经存在于数据库
                else if (NewPlanAircraft.AircraftID != null)
                {
                    Aircraft OriginAircraft = this._FE.Aircrafts.Where(q => q.AircraftID == NewPlanAircraft.AircraftID).FirstOrDefault();
                    if (OriginAircraft == null)
                    {
                        NewPlanAircraft.AircraftID = null;
                    }
                }
                NewPlanAircraft.Aircraft = null;
                NewPlanAircraft.PlanHistories = null;

                //获取原计划飞机
                PlanAircraft OriginPlanAircraft = this._FE.PlanAircrafts.Where(q => q.PlanAircraftID == NewPlanAircraft.PlanAircraftID).FirstOrDefault();

                //原来为空,新增
                if (OriginPlanAircraft == null)
                {
                    this._FE.PlanAircrafts.Add(NewPlanAircraft);
                }
                //原来已有,更新
                else
                {
                    this.Update<PlanAircraft>(_FE, OriginPlanAircraft, NewPlanAircraft);
                }
            }
        }

        //更新飞机
        protected void UpdateAircraft(Aircraft NewAircraft, bool NeedUpdatePlanAircraft)
        {
            if (NewAircraft != null)
            {
                //通过序列号查找飞机
                Aircraft OriginAircraft;
                if (NewAircraft.SerialNumber != null || NewAircraft.SerialNumber != "")
                {
                    OriginAircraft = this._FE.Aircrafts.Where(q => q.SerialNumber == NewAircraft.SerialNumber).FirstOrDefault();
                }
                else
                {
                    OriginAircraft = this._FE.Aircrafts.Where(q => q.AircraftID == NewAircraft.AircraftID).FirstOrDefault();
                }

                //飞机数据处理
                NewAircraft.AircraftBusinesses = null;
                NewAircraft.OperationHistories = null;
                NewAircraft.OwnershipHistorys = null;
                NewAircraft.Airlines = null;
                ICollection<PlanAircraft> planAircrafts = NewAircraft.PlanAircrafts; 
                //更新计划飞机
                if (NeedUpdatePlanAircraft && planAircrafts != null)
                {
                    if (OriginAircraft != null)
                    {
                        UpdatePlanAircrafts(planAircrafts, OriginAircraft);
                    }
                    else
                    {
                        UpdatePlanAircrafts(planAircrafts, NewAircraft);
                    }
                    
                }
                NewAircraft.PlanAircrafts = null;
                //Replace Aircraft Owner
                ReplaceAircraftOwner(NewAircraft);
                
                //原来为空,新增
                if (OriginAircraft == null)
                {
                    //增加飞机数据
                    this._FE.Aircrafts.Add(NewAircraft);
                }
                //原来已有,更新
                else
                {
                    //原飞机ID不可以更改
                    NewAircraft.AircraftID = OriginAircraft.AircraftID;
                    this.Update<Aircraft>(_FE, OriginAircraft, NewAircraft);
                }
            }
        }

        #endregion

        #region 更新所有权人

        protected void ReplaceAircraftOwner(Aircraft NewAircraft)
        {
            //自有飞机不替换供应商
            if (NewAircraft.AirlinesID == NewAircraft.OwnerID)
            {
                return;
            }
            Owner AircraftOwner = NewAircraft.Owner;
            Owner ReplaceOwner;

            //飞机供应商需要传送到民航局， 需要修改客户端
            if (AircraftOwner != null)
            {
                if (AircraftOwner.SupplierType == 2) //国外
                {
                    ReplaceOwner = GetOutsideSupplier();
                }
                else //国内
                {
                    ReplaceOwner = GetInsideSupplier();
                };
            }
            else //否则当做国内供应商
            {
                ReplaceOwner = GetSupplierByOwnerID(NewAircraft.OwnerID);
            }

            //替换供应商
            if (ReplaceOwner != null) //有找到，则替换为国内/国外供应商
            {
                NewAircraft.OwnerID = ReplaceOwner.OwnerID;
            }
            else //没有找到供应商，就当做自由
            {
                NewAircraft.OwnerID = NewAircraft.AirlinesID; 
            }
            NewAircraft.Owner = null;
        }

        private Owner GetSupplier(string OwnerName, int SupplierType)
        {
            Owner ReplaceOwner;
            ReplaceOwner = this._FE.Owners.Where(q => q.ShortName == OwnerName).FirstOrDefault();

            //原来为空,新增
            if (ReplaceOwner == null)
            {
                ReplaceOwner = new Owner();
                ReplaceOwner.OwnerID = Guid.NewGuid();
                ReplaceOwner.Name = OwnerName;
                ReplaceOwner.ShortName = OwnerName;
                ReplaceOwner.SupplierType = SupplierType;
                //增加供应商数据
                this._FE.Owners.Add(ReplaceOwner);
            }

            return ReplaceOwner;
        }

        private Owner GetOutsideSupplier()
        {
            string OwnerName;
            int SupplierType;

            OwnerName = "国外";
            SupplierType = 2;
            return GetSupplier(OwnerName, SupplierType);
       }

         
        private Owner GetInsideSupplier()
        {
            string OwnerName;
            int SupplierType;

            OwnerName = "国内";
            SupplierType = 1;

            return GetSupplier(OwnerName, SupplierType);
        }

        private Owner GetSupplierByOwnerID(Guid? OwnerID)
        {
            if (OwnerID == null)
            {
                return GetInsideSupplier();
            }
            else
            {
                Owner OriginOwner;
                OriginOwner = this._FE.Owners.Where(q => q.OwnerID == OwnerID).FirstOrDefault();
                if (OriginOwner == null)
                {
                    return GetInsideSupplier();
                }
                else
                {
                    if (OriginOwner.SupplierType == 2) //国外
                    {
                        return GetOutsideSupplier();
                    }
                    else //国内
                    {
                        return GetInsideSupplier();
                    };
                }
            }
        }

        #endregion

        public void Update<T>(DbContext context, T entity) where T : class
        {
            Contract.Requires(context != null);
            Contract.Requires(entity != null);

            // Note: changing the state to Modified on a detached entity is the same as
            // calling Attach and then setting state to Modified.
            context.Entry(entity).State = EntityState.Modified;
        }

        public void Update<T>(DbContext context, T current, T original) where T : class
        {
            Contract.Requires(context != null);
            Contract.Requires(current != null);
            Contract.Requires(original != null);

            var entryCurrent = context.Entry(current);
            var entryOriginal = context.Entry(original);

            
            entryCurrent.State = EntityState.Unchanged;
            entryCurrent.CurrentValues.SetValues(original);
            

            var properties = TypeDescriptor.GetProperties(typeof(T));
            var attributes = TypeDescriptor.GetAttributes(typeof(T));

            foreach (var propertyName in entryCurrent.CurrentValues.PropertyNames)
            {
                System.Data.Entity.Infrastructure.DbPropertyEntry property = entryCurrent.Property(propertyName);
                System.Data.Entity.Infrastructure.DbPropertyEntry Originproperty = entryOriginal.Property(propertyName);
                if (property != null && Originproperty != null)
                {
                    property.CurrentValue = Originproperty.CurrentValue;
                    if (property.CurrentValue != null)
                    {
                        if (!property.CurrentValue.Equals(property.OriginalValue))
                        {
                            property.IsModified = true;
                        }
                    }
                    else if (property.CurrentValue != property.OriginalValue)
                    {
                        property.IsModified = true;
                    }
                }

            }

            // Not sure what is going on here.  If you get to this point and the state is not Modified,
            // then it means all the values of current are the same as those of original and no property
            // was set to Modified explicitly in the loop above.  This means that there should be nothing
            // to write to the database.  If you then set the state to Modified it means all the properties
            // will be marked as Modified which means they will ALL be written to the database.  So it seems
            // likely that the following lines should not be here.
            if (entryCurrent.State != EntityState.Modified)
            {
                entryCurrent.State = EntityState.Modified;
            }
        }


        //public void Update<T>(DbContext context, T current, T original) where T : class
        //{
        //    Contract.Requires(context != null);
        //    Contract.Requires(current != null);
        //    Contract.Requires(original != null);

        //    var entry = context.Entry(current);
        //    entry.State = EntityState.Unchanged;
        //    // entry.OriginalValues.SetValues(original);
        //    entry.CurrentValues.SetValues(original);

        //    var properties = TypeDescriptor.GetProperties(typeof(T));
        //    var attributes = TypeDescriptor.GetAttributes(typeof(T));

        //    foreach (var propertyName in entry.CurrentValues.PropertyNames)
        //    {
        //        var descriptor = properties[propertyName];
        //        if (descriptor != null &&
        //            descriptor.Attributes[typeof(RoundtripOriginalAttribute)] == null &&
        //            attributes[typeof(RoundtripOriginalAttribute)] == null &&
        //            descriptor.Attributes[typeof(ExcludeAttribute)] == null)
        //        {
        //            entry.Property(propertyName).IsModified = true;
        //        }
        //    }

        //    // Not sure what is going on here.  If you get to this point and the state is not Modified,
        //    // then it means all the values of current are the same as those of original and no property
        //    // was set to Modified explicitly in the loop above.  This means that there should be nothing
        //    // to write to the database.  If you then set the state to Modified it means all the properties
        //    // will be marked as Modified which means they will ALL be written to the database.  So it seems
        //    // likely that the following lines should not be here.
        //    if (entry.State != EntityState.Modified)
        //    {
        //        entry.State = EntityState.Modified;
        //    }
        //}
    }
}
