using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail
{
   public class EncodeModel
    {
        private FleetEntities _FE;

        public EncodeModel()
        {
            _FE = new FleetEntities();
        }

        public EncodeModel(string conn)
        {
            _FE = new FleetEntities(conn);
        }

       ///
       /// 
       public object NewSerialObj(object obj)
       {
           if (obj != null)
           {
               MemoryStream ms = new MemoryStream();
               IFormatter formatter = new BinaryFormatter();
               formatter.Serialize(ms, obj);
               ms.Position = 0;
               return formatter.Deserialize(ms);
           }
           else
           {
               return null;
           }
         
       }
        /// <summary>
        /// 传输申请
        /// </summary>
        /// <param name="currentRequest"></param>
        public object EncodeRequest(Guid currentRequest)
        {
            // 获取需要发送的对象
            var obj = _FE.Requests.Where(r => r.RequestID == currentRequest).
                Include(r => r.ApprovalHistories).FirstOrDefault();
            return NewSerialObj(obj);
        }

        /// <summary>
        /// 传输计划
        /// </summary>
        /// <param name="currentPlan"></param>
        public object EncodePlan(Guid currentPlan)
        {
            // 获取需要发送的对象
            var obj = _FE.Plans.Where(p => p.PlanID == currentPlan).
                Include(p => p.PlanHistories).
                Include(p => p.PlanHistories.Select(q => q.PlanAircraft.Aircraft)).
                FirstOrDefault();
            return NewSerialObj(obj);
        }

        /// <summary>
        /// 传输批文
        /// </summary>
        /// <param name="currentApprovalDoc"></param>
        public object EncodeApprovalDoc(Guid currentApprovalDoc)
        {
            // 获取需要发送的对象
            var obj = _FE.ApprovalDocs.Where(a => a.ApprovalDocID == currentApprovalDoc).
                Include(a => a.Requests).Include(a => a.Requests.Select(q => q.ApprovalHistories)).FirstOrDefault();
            return NewSerialObj(obj);
        }

        /// <summary>
        /// 传输运营历史
        /// </summary>
        /// <param name="currentAircraft"></param>
        public object EncodeOperationHistory(Guid currentOperationHistory)
        {
            // 获取需要发送的对象
            var obj = _FE.OperationHistories.Where(a => a.OperationHistoryID == currentOperationHistory).
                Include(a => a.Aircraft.PlanAircrafts).Include(a=>a.Aircraft.Owner).FirstOrDefault();
            return NewSerialObj(obj);
        }


        /// <summary>
        /// 传输商业数据
        /// </summary>
        /// <param name="currentAircraft"></param>
        public object EncodeAircraftBusiness(Guid currentAircraftBusiness)
        {
            // 获取需要发送的对象
            var obj = _FE.AircraftBusinesses.Where(a => a.AircraftBusinessID == currentAircraftBusiness).
                Include(a => a.Aircraft.PlanAircrafts).Include(a => a.Aircraft.Owner).FirstOrDefault();
            return NewSerialObj(obj);
        }

        /// <summary>
        /// 传输所有权历史
        /// </summary>
        /// <param name="currentAircraft"></param>
        public object EncodeOwnershipHistory(Guid currentOwnershipHistory)
        {
            // 获取需要发送的对象
            var obj = _FE.OwnershipHistories.Where(a => a.OwnershipHistoryID == currentOwnershipHistory).
                Include(a => a.Aircraft.PlanAircrafts).Include(a => a.Aircraft.Owner).FirstOrDefault();
            return NewSerialObj(obj);
        }

    }
}
