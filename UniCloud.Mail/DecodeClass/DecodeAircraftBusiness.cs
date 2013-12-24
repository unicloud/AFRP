using System.Linq;
using UniCloud.Fleet.Business.Plan;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail.DecodeClass
{
    public class DecodeAircraftBusiness : DecodeObject
    {
        public DecodeAircraftBusiness()
            : base()
        {
        }


        #region 初始化数据
        protected override void InitData()
        {
            _FullTypeName = "UniCloud.Fleet.Models.AircraftBusiness";
            _TypeName = "AircraftBusiness";
        }
        #endregion

        #region 分解商业数据权历史对象

        public override void Decode(object obj)
        {

            AircraftBusiness NewAircraftBusiness = (AircraftBusiness)obj;
            //获取原商业数据权历史
            AircraftBusiness OriginAircraftBusiness = this._FE.AircraftBusinesses.Where(q =>
                    q.AircraftBusinessID == NewAircraftBusiness.AircraftBusinessID).FirstOrDefault();

            //商业数据对应的飞机
            Aircraft NewAircraft = NewAircraftBusiness.Aircraft;
            //更新商业数据历史对应的飞机
            if (NewAircraft != null)
            {
                UpdateAircraft(NewAircraft, true);
                NewAircraftBusiness.AircraftID = NewAircraft.AircraftID;
            }
            NewAircraftBusiness.Aircraft = null;

            //飞机已经存在
            if (OriginAircraftBusiness != null)
            {
                //更新商业数据
                this.Update<AircraftBusiness>(_FE, OriginAircraftBusiness, NewAircraftBusiness);
            }
            else
            {
                //增加商业数据
                this._FE.AircraftBusinesses.Add(NewAircraftBusiness);
            }
            //更新前一条商业数据
            UpdatepreviousAircraftBusiness(NewAircraftBusiness);
        }

        private void UpdatepreviousAircraftBusiness(AircraftBusiness obj)
        {
            LogicHistoryHandle.SetPreviousrAircraftBusiness(obj.Aircraft, obj,this._FE);
        }



        #endregion
    }
}
