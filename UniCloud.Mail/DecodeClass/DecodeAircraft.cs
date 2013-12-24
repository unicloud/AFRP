using System.Linq;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail.DecodeClass
{
    public class DecodeAircraft : DecodeObject
    {
        public DecodeAircraft()
            : base()
        {
        }

        #region 初始化数据
        protected override void InitData()
        {
         //   _FullTypeName = "UniCloud.Fleet.Models.Aircraft";
         //   _TypeName = "Aircraft";
        }
        #endregion

        #region 解析飞机对象

        public override void Decode(object obj)
        {
            Aircraft NewAircraft = (Aircraft)obj;
            //更新飞机所有权历史、拥有权历史和商业数据 TODO
            NewAircraft.OperationHistories = null;
            NewAircraft.OwnershipHistorys = null;
            NewAircraft.AircraftBusinesses = null;
            //处理航空公司
            NewAircraft.Airlines = null;
            //获取原飞机
            Aircraft OriginAircraft = this._FE.Aircrafts.Where(q => q.AircraftID == NewAircraft.AircraftID).FirstOrDefault();
            //飞机已经存在
            if (OriginAircraft != null)
            {
                //更新飞机
                this.Update<Aircraft>(_FE, OriginAircraft, NewAircraft);
            }
            else
            {
                //增加飞机
                this._FE.Aircrafts.Add(NewAircraft);
            }
        }

        #endregion

    }
}
