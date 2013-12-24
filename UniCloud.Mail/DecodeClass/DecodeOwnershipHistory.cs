using System.Linq;
using UniCloud.Fleet.Business.Plan;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail.DecodeClass
{
    public class DecodeOwnershipHistory : DecodeObject
    {
        public DecodeOwnershipHistory()
            : base()
        {
        }

        #region 初始化数据
        protected override void InitData()
        {
            _FullTypeName = "UniCloud.Fleet.Models.OwnershipHistory";
            _TypeName = "OwnershipHistory";
        }
        #endregion

        #region 分解所有权历史对象

        public override void Decode(object obj)
        {

            OwnershipHistory NewOwnershipHistory = (OwnershipHistory)obj;

            //处理航空公司
            NewOwnershipHistory.Owner = null;
            //所有权历史对应的飞机
            Aircraft NewAircraft = NewOwnershipHistory.Aircraft;
            //更新所有权历史对应的飞机
            if (NewAircraft != null)
            {
                //更新飞机
                UpdateAircraft(NewAircraft, true);
                NewOwnershipHistory.AircraftID = NewAircraft.AircraftID;
            }
            NewOwnershipHistory.Aircraft = null;

            //获取原拥有权历史
            OwnershipHistory OriginOwnershipHistory = this._FE.OwnershipHistories.Where(q =>
                    q.OwnershipHistoryID == NewOwnershipHistory.OwnershipHistoryID).FirstOrDefault();
            //拥有权历史已经存在
            if (OriginOwnershipHistory != null)
            {
                //更新所有权历史
                this.Update<OwnershipHistory>(_FE, OriginOwnershipHistory, NewOwnershipHistory);
            }
            else
            {
                //增加所有权历史
                this._FE.OwnershipHistories.Add(NewOwnershipHistory);
            }
            //更新上一条所有权历史
            UpdatepreviousOwnershipHistory(NewOwnershipHistory);
        }
        private void UpdatepreviousOwnershipHistory(OwnershipHistory obj)
        {
            LogicHistoryHandle.SetPreviousOwershipHistory(obj.Aircraft, obj,this._FE);
        }
        #endregion
    }
}
