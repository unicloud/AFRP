using System.Linq;
using UniCloud.Fleet.Business.Plan;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail.DecodeClass
{
    public class DecodeOperationHistory : DecodeObject
    {
        public DecodeOperationHistory()
            : base()
        {
        }

        #region 初始化数据
        protected override void InitData()
        {
            _FullTypeName = "UniCloud.Fleet.Models.OperationHistory";
            _TypeName = "OperationHistory";
        }
        #endregion

        #region 分解运营权历史对象

        public override void Decode(object obj)
        {

            OperationHistory NewOperationHistory = (OperationHistory)obj;
            //处理航空公司
            NewOperationHistory.Airlines = null;
            //运行权历史对应的飞机
            Aircraft NewAircraft = NewOperationHistory.Aircraft;
            //更新运行权历史对应的飞机
            if (NewAircraft != null)
            {
                UpdateAircraft(NewAircraft, true);
                NewOperationHistory.AircraftID = NewAircraft.AircraftID;
            }
            NewOperationHistory.Aircraft = null;
            //获取原运营权历史
            OperationHistory OriginOperationHistory = this._FE.OperationHistories.Where(q =>
                    q.OperationHistoryID == NewOperationHistory.OperationHistoryID).FirstOrDefault();
            //运营历史已经存在
            if (OriginOperationHistory != null)
            {
                //更新运营权历史
                this.Update<OperationHistory>(_FE, OriginOperationHistory, NewOperationHistory);
            }
            else
            {
                //增加运营权历史
                this._FE.OperationHistories.Add(NewOperationHistory);
            }
            //更新上一条运营权历史数据
            UpdatepreviousOperationHistory(NewOperationHistory);

        }
        private void UpdatepreviousOperationHistory(OperationHistory obj)
        {
            LogicHistoryHandle.SetPreviousOperationHistory(obj.Aircraft, obj,this._FE);
        }

        #endregion

    }
}
