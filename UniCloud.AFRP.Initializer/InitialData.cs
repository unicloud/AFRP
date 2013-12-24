using System;
using UniCloud.Fleet.Models;

namespace UniCloud.AFRP.Initializer
{
    /// <summary>
    /// 现役飞机
    /// 包括运营权历史、商业数据历史、所有权历史
    /// </summary>
    public class AircraftData
    {
        public AircraftType AircraftType { get; set; } // 机型
        public string SerialNumber { get; set; } // 飞机序列号
        public string RegNumber { get; set; } // 机号
        public DateTime? FactoryDate { get; set; } // 出厂日期
        public DateTime? ImportDate { get; set; } // 引进日期
        public ActionCategory ImportType { get; set; } // 引进方式
        public DateTime? TechReceiptDate { get; set; } // 技术接收日期
        public DateTime? ReceiptDate { get; set; } // 接收日期
        public DateTime? StartDate { get; set; } // 运营日期
        public DateTime? StopDate { get; set; } // 退出停场日期
        public DateTime? TechDeliveryDate { get; set; } // 技术交付日期
        public DateTime? OnHireDate { get; set; } // 起租日期，适用于租赁
        public int OwnerNumber { get; set; } // 所有权人
        public int SeatingCapacity { get; set; } // 座位
        public decimal CarryingCapacity { get; set; } // 商载(吨)
    }

    /// <summary>
    /// 批文飞机
    /// 包括已有批文但尚未交付的飞机
    /// </summary>
    public class ApprovedData
    {
        public AircraftType AircraftType { get; set; } // 机型
        public ActionCategory ImportType { get; set; } // 引进方式
        public Annual Annual { get; set; } // 执行年度
        public int Month { get; set; } // 执行月份
        public int SeatingCapacity { get; set; } // 座位
        public decimal CarryingCapacity { get; set; } // 商载(吨)
        public string ApprovalNumber { get; set; } // 批文号
    }

    /// <summary>
    /// 申请飞机
    /// 已申请但尚无批文的飞机
    /// </summary>
    public class RequestData
    {
        public AircraftType AircraftType { get; set; } // 机型
        public ActionCategory ImportType { get; set; } // 引进方式
        public Annual Annual { get; set; } // 执行年度
        public int Month { get; set; } // 执行月份
        public int SeatingCapacity { get; set; } // 座位
        public decimal CarryingCapacity { get; set; } // 商载(吨)
        public string DocNumber { get; set; } // 航空公司申请的文号
    }

    /// <summary>
    /// 供应商数据
    /// </summary>
    public class OwnData
    {
        public int OwnNumber { get; set; } // 供应商编号
        public string OwnName { get; set; } // 供应商名称
        public int Owntype { get; set; } // 供应商类型，国内、国外
        public Guid OwnGuID { get; set; } // 供应商的Guid
    }
}
