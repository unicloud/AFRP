using System;
using System.Collections.Generic;
using UniCloud.Fleet.Models;
using UniCloud.Mail.DecodeClass;

namespace UniCloud.Mail
{

    public class DecodeModel
    {
        private bool _DataChanged;
        private List<IDecodeObject> _DecodeObjects;
        private string _Conn = "";

        public DecodeModel()
        {
            _Conn = "";
            InitData();
        }

        public DecodeModel(string conn)
        {
            _Conn = conn;
            InitData();
        }

        private void InitData()
        {
            _DataChanged = false;
            _DecodeObjects = new List<IDecodeObject>();
            RegisterDecoder();
        }

        public Boolean DataChanged
        {
            get { return _DataChanged; }
        }

        private void RegisterDecoder()
        {
            _DecodeObjects.Add(new DecodeOperationPlan());
            _DecodeObjects.Add(new DecodeChangePlan());
            _DecodeObjects.Add(new DecodePlanHistory());
            _DecodeObjects.Add(new DecodePlan());
            _DecodeObjects.Add(new DecodeRequest());
            _DecodeObjects.Add(new DecodeApprovalDoc());
            _DecodeObjects.Add(new DecodeAircraftBusiness());
            _DecodeObjects.Add(new DecodeOperationHistory());
            _DecodeObjects.Add(new DecodeOwnershipHistory());
        }

        private IDecodeObject GetDecoder(object obj)
        {
            string strTypeName = obj.GetType().ToString();
            foreach (var IDO in _DecodeObjects)
            {
                if (IDO.CanDecode(strTypeName))
                {
                    return IDO;
                }
            }
            return null;
        }

        private FleetEntities CreateFleetEntity()
        {
            if (_Conn == "")
            {
             return  new FleetEntities();
            }
            else
            {
                return new FleetEntities(_Conn);
            }
        }

        #region 保存所有实体

        private bool SubmitChange(FleetEntities fe)
        {
            //保存数据
             if (_DataChanged)
            {
                try
                {
                    fe.SaveChanges();
                    return true;
                }
                catch(Exception ex)
                {
                    UniCloud.Log.WindowsLog.Write(ex.Message);
                    return false;
                }
            }
            return true;
        }

        public bool SaveObjects(List<ReceiverObject> ReceiverObjects)
        {
            bool blSaveOk = false;
            if (ReceiverObjects == null) return true;
            foreach (ReceiverObject ro in ReceiverObjects)
            {
                if (ro == null) continue;

                //默认保存成功
                bool blSaved = true;
                foreach (var obj in ro.Objects)
                {
                    if (obj == null) continue;
                    //获取保存对象
                    IDecodeObject IDO = GetDecoder(obj);
                    if (IDO != null)
                    {
                        try
                        {
                            using (FleetEntities fe = CreateFleetEntity())
                            {
                                IDO.SetDbContext(fe);
                                _DataChanged = true;
                                //分解对象数据
                                IDO.Decode(obj);
                                //提交数据
                                if (!SubmitChange(fe)) { blSaved = false; }
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            UniCloud.Log.WindowsLog.Write(ex.Message);
                            continue;
                        
                        }

                    }
                }
                ro.Saved = blSaved;
                blSaveOk = blSaveOk || blSaved;
            }
            return blSaveOk;
        }

        #endregion
    }
}
