using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using UniCloud.Fleet.Models;


namespace TestMailService
{
    public class TestData
    {
        private FleetEntities _FE;

        public TestData()
        {
            _FE = new FleetEntities();
        }

        public TestData(string conn)
        {
            _FE = new FleetEntities(conn);
        }

        public Guid GetLastAircraftBusiness()
        {
            // 获取需要发送的对象
            var obj = _FE.AircraftBusinesses.ToList().LastOrDefault();
            if (obj != null)
            {
                return obj.AircraftBusinessID;
            }
            else
            {
                return new Guid();
            }
        }

        public Guid GetLastOperationHistory()
        {
            // 获取需要发送的对象
            var obj = _FE.OperationHistories.ToList().LastOrDefault();
            if (obj != null)
            {
                return obj.OperationHistoryID;
            }
            else
            {
                return new Guid();
            }
        }

        public Guid GetLastOwnershipHistory()
        {
            // 获取需要发送的对象
            var obj = _FE.OwnershipHistories.ToList().LastOrDefault();
            if (obj != null)
            {
                return obj.OwnershipHistoryID;
            }
            else
            {
                return new Guid();
            }
        }

        public Guid GetLastAircraftPlan()
        {
            // 获取需要发送的对象
            var obj = _FE.Plans.ToList().LastOrDefault();
            if (obj != null)
            {
                return obj.PlanID;
            }
            else
            {
                return new Guid();
            }
        }
    }
}
