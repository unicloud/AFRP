namespace UniCloud.Security.Models.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SecurityEntities>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(SecurityEntities context)
        {
            var uniCloud = new Applications
            {
                ApplicationName = "UniCloud",
                LoweredApplicationName = "UniCloud".ToLower()
            };
            context.Applications.Add(uniCloud);

            #region 1、UniAuth：统一权限管理

            var uniAuth = new Applications
            {
                ApplicationName = "UniAuth",
                LoweredApplicationName = "UniAuth".ToLower(),
                ModuleName = "UniAuthModule",
                ViewNameSpace = "UniCloud.UniAuth.Views.",
                Description = "统一权限管理"
            };
            context.Applications.AddOrUpdate(a => a.ApplicationName, uniAuth);

            var uniAuthFuncs = new FunctionItem
            {
                Application = uniAuth,
                LevelCode = "01",
                Name = "统一权限管理",
                ViewName = null,
                Description = null,
                IsLeaf = false,
                IsButton = false,
                SubItems = new List<FunctionItem>
                {

                    #region 应用架构

                    new FunctionItem
                    {
                        Application = uniAuth,
                        LevelCode = "01.01",
                        Name = "应用架构",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                            new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.01.01",
                                Name="应用", 
                                ViewName=null,
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.01.02",
                                Name="角色", 
                                ViewName=null,
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.01.03",
                                Name="功能", 
                                ViewName=null,
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                        }
                    },

                    #endregion

                    #region 授权

                    new FunctionItem
                    {
                        Application = uniAuth,
                        LevelCode = "01.02",
                        Name = "授权",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                              new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.02.01",
                                Name="修改密码", 
                                ViewName="UserChangePwdView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                              new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.02.02",
                                Name="编辑用户", 
                                ViewName="UserMaintainView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=uniAuth,
                                        LevelCode="01.02.02.01",
                                        Name="添加用户",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=uniAuth,
                                        LevelCode="01.02.02.02",
                                        Name="移除用户",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                  
                                },
                            },
                            new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.02.03",
                                Name="用户角色", 
                                ViewName="UserRoleAllotView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.02.04",
                                Name="角色功能", 
                                ViewName="RolesFunctionMtnView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=uniAuth,
                                        LevelCode="01.02.04.01",
                                        Name="添加角色",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=uniAuth,
                                        LevelCode="01.02.04.02",
                                        Name="移除角色",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                  
                                },
                            },
                        }
                    },

                    #endregion

                }
            };
            context.FunctionItems.Add(uniAuthFuncs);

            #endregion

            #region 2、AFRP：航空公司机队资源规划

            var afrp = new Applications
            {
                ApplicationName = "AFRP",
                LoweredApplicationName = "AFRP".ToLower(),
                ModuleName = "AFRPModule",
                ViewNameSpace = "UniCloud.AFRP.Views.",
                Description = "机队资源规划"
            };
            context.Applications.Add(afrp);

            var afrpFuncs = new FunctionItem
            {
                Application = afrp,
                LevelCode = "02",
                Name = "机队资源规划",
                ViewName = null,
                Description = null,
                IsLeaf = false,
                IsButton = false,
                SubItems = new List<FunctionItem>
                {

                    #region 计划编制

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.01",
                        Name = "编制计划",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.01.01",
                                Name="准备", 
                                ViewName="AfrpPlanPrepareView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.01.01",
                                        Name="准备年度计划",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                }
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.01.02",
                                Name="制订", 
                                ViewName="AfrpPlanLayView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.02.01",
                                        Name="创建新版本计划",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.02.02",
                                        Name="添加计划项",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.02.03",
                                        Name="移除计划项",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.02.04",
                                        Name="提交审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.02.05",
                                        Name="审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                },
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.01.03",
                                Name="报送", 
                                ViewName="AfrpPlanSendView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.03.01",
                                        Name="添加附件",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.03.02",
                                        Name="发送",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                },
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.01.04",
                                Name="发布", 
                                ViewName="AfrpPlanPublishView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.04.01",
                                        Name="提交审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.04.02",
                                        Name="审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.04.03",
                                        Name="发送",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.04.04",
                                        Name="撤销发布",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                },
                            },
                        }
                    },

                    #endregion

                    #region 执行计划

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.02",
                        Name = "执行计划",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.02.01",
                                Name="申请批文", 
                                ViewName="AfrpRequestView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                      new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.01",
                                        Name="创建新申请",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.02",
                                        Name="添加附件",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.03",
                                        Name="提交审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.04",
                                        Name="审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.05",
                                        Name="发送",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                       new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.06",
                                        Name="修改申请",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                },
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.02.02",
                                Name="维护批文", 
                                ViewName="AfrpApprovalView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.01",
                                        Name="创建新批文",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.02",
                                        Name="添加附件",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.03",
                                        Name="提交审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.04",
                                        Name="审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.05",
                                        Name="发送",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.06",
                                        Name="修改批文",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                },
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.02.03",
                                Name="完成计划", 
                                ViewName="AfrpDeliverView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.03.01",
                                        Name="完成计划",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.03.02",
                                        Name="提交审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.03.03",
                                        Name="审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.03.04",
                                        Name="发送",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.03.05",
                                        Name="修改完成",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                },
                            },
                        }
                    },

                    #endregion

                    #region 机队数据

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.03",
                        Name = "机队数据",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.03.01",
                                Name="变更所有权", 
                                ViewName="AfrpAircraftOwnershipView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.01",
                                        Name="创建新所有权",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.02",
                                        Name="移除所有权",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.03",
                                        Name="提交审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.04",
                                        Name="审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.05",
                                        Name="发送",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                      new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.06",
                                        Name="修改所有权",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                },
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.03.02",
                                Name="分配飞机运力", 
                                ViewName="AfrpFleetAllotView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.02.01",
                                        Name="创建新运力分配",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.02.02",
                                        Name="提交审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.02.03",
                                        Name="审核",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },                                   
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.02.04",
                                        Name="修改运力分配",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },                                   
                                },
                            },
                        }
                    },

                    #endregion

                    #region 分析计划执行
                
                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.04",
                        Name = "分析计划执行",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.04.01",
                                Name="计划", 
                                ViewName="AfrpPlanQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.04.02",
                                Name="计划执行", 
                                ViewName="AfrpPlanPerformView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.04.03",
                                Name="申请", 
                                ViewName="AfrpRequestQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.04.04",
                                Name="批文", 
                                ViewName="AfrpApprovalQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                        }
                    },

                    #endregion

                    #region 统计分析

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.05",
                        Name = "统计分析",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {

                            #region 运力趋势

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.01",
                                Name="飞机运力变化", 
                                ViewName="AfrpFleetTrendAllView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.02",
                                Name="客机运力变化", 
                                ViewName="AfrpFleetTrendPnrView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.03",
                                Name="货机运力变化", 
                                ViewName="AfrpFleetTrendCargoView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.04",
                                Name="在册飞机", 
                                ViewName="AfrpFleetRegisteredView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region 飞机来源

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.11",
                                Name="引进方式", 
                                ViewName="AfrpImportTypeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region 机队结构

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.21",
                                Name="机队结构", 
                                ViewName="AfrpFleetStructureView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region 机龄

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.31",
                                Name="飞机机龄", 
                                ViewName="AfrpFleetAgeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region 飞机档案

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.41",
                                Name="飞机数据", 
                                ViewName="AfrpFleetQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                        }
                    },

                    #endregion

                    #region 系统管理

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.06",
                        Name = "系统管理",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.06.01",
                                Name="维护邮件账号", 
                                ViewName="AfrpMailSettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            {
                                Application=afrp, 
                                LevelCode="02.06.02",
                                Name="维护基础颜色", 
                                ViewName="AfrpColorSettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            {
                                Application=afrp, 
                                LevelCode="02.06.03",
                                Name="维护供应商", 
                                ViewName="AfrpSupplierSettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.06.03.01",
                                        Name="增加供应商",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.06.03.02",
                                        Name="移除供应商",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                },
                            },
                            new FunctionItem
                            {
                                Application=afrp, 
                                LevelCode="02.06.04",
                                Name="维护分公司", 
                                ViewName="AfrpFilialeSettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                                SubItems=new List<FunctionItem>
                                {
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.06.04.01",
                                        Name="增加分公司",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.06.04.02",
                                        Name="移除分公司",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                },
                            },
                            new FunctionItem
                            {
                                Application=afrp, 
                                LevelCode="02.06.05",
                                Name="维护子公司", 
                                ViewName="AfrpSubsidiarySettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                        }
                    },

                    #endregion

                }
            };
            context.FunctionItems.Add(afrpFuncs);

            #endregion

            #region 3、CAFM：民航局机队管理

            var cafm = new Applications
            {
                ApplicationName = "CAFM",
                LoweredApplicationName = "CAFM".ToLower(),
                ModuleName = "CAFMModule",
                ViewNameSpace = "CAAC.CAFM.Views.",
                Description = "民航机队管理"
            };
            context.Applications.AddOrUpdate(a => a.ApplicationName, cafm);

            var cafmFuncs = new FunctionItem
            {
                Application = cafm,
                LevelCode = "03",
                Name = "民航机队管理",
                ViewName = null,
                Description = null,
                IsLeaf = false,
                IsButton = false,
                SubItems = new List<FunctionItem>
                {

                    #region 管理计划

                    new FunctionItem
                    {
                        Application = cafm,
                        LevelCode = "03.01",
                        Name = "管理计划",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.01",
                                Name="客机计划", 
                                ViewName="CafmPlanPnrView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.02",
                                Name="货机计划", 
                                ViewName="CafmPlanCargoView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.03",
                                Name="检查计划发布", 
                                ViewName="CafmPlanPublishView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.04",
                                Name="计划执行", 
                                ViewName="CafmPlanPerformView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.05",
                                Name="查询计划", 
                                ViewName="CafmPlanQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.06",
                                Name="分析计划执行", 
                                ViewName="CafmPerformAnalyseView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                        }
                    },

                    #endregion

                    #region 评审申请

                    new FunctionItem
                    {
                        Application = cafm,
                        LevelCode = "03.02",
                        Name = "评审申请",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.02.01",
                                Name="录入批文", 
                                ViewName="CafmApprovalView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.02.02",
                                Name="检查批文匹配", 
                                ViewName="CafmApprovalCheckView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.02.03",
                                Name="管理申请", 
                                ViewName="CafmRequestView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.02.04",
                                Name="管理批文", 
                                ViewName="CafmApprovalManageView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                        }
                    },

                    #endregion

                    #region 统计分析

                    new FunctionItem
                    {
                        Application = cafm,
                        LevelCode = "03.03",
                        Name = "统计分析",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {

                            #region 运力趋势

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.01",
                                Name="飞机运力变化", 
                                ViewName="CafmFleetTrendAllView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.02",
                                Name="客机运力变化", 
                                ViewName="CafmFleetTrendPnrView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.03",
                                Name="货机运力变化", 
                                ViewName="CafmFleetTrendCargoView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.04",
                                Name="在册飞机", 
                                ViewName="CafmFleetRegisteredView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region 飞机来源

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.11",
                                Name="引进方式", 
                                ViewName="CafmImportTypeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.12",
                                Name="供应商", 
                                ViewName="CafmSupplierView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.13",
                                Name="制造商", 
                                ViewName="CafmManufacturerView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region 机队结构

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.21",
                                Name="飞机机型", 
                                ViewName="CafmAircraftTypeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region 机龄

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.31",
                                Name="飞机机龄", 
                                ViewName="CafmFleetAgeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region 飞机档案

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.41",
                                Name="飞机数据", 
                                ViewName="CafmFleetQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                        }
                    },

                    #endregion

                    #region 系统管理

                    new FunctionItem
                    {
                        Application = cafm,
                        LevelCode = "03.04",
                        Name = "系统管理",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.04.01",
                                Name="日志", 
                                ViewName="AfrpLogView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                             new FunctionItem
                            {
                                Application=cafm, 
                                LevelCode="03.04.02",
                                Name="维护基础颜色", 
                                ViewName="CafmColorSettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            {
                                Application=cafm, 
                                LevelCode="03.04.03",
                                Name="维护邮件账号", 
                                ViewName="CafmMailSettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            }
                        }
                    },

                    #endregion

                }
            };
            context.FunctionItems.Add(cafmFuncs);

            #endregion

            #region 增加管理员用户

            // 角色功能
            var uniAuthRole = new Roles
                {
                    Application = uniAuth,
                    RoleName = "系统管理员",
                    LoweredRoleName = "系统管理员"
                };
            context.Roles.Add(uniAuthRole);
            uniAuth.FunctionItems.ToList()
                   .ForEach(
                       f =>
                       context.FunctionsInRoles.Add(new FunctionsInRoles
                           {
                               FunctionItem = f,
                               IsValid = true,
                               Role = uniAuthRole
                           }));

            var afrpRole = new Roles
                {
                    Application = afrp,
                    RoleName = "系统管理员",
                    LoweredRoleName = "系统管理员"
                };
            context.Roles.Add(afrpRole);
            afrp.FunctionItems.ToList()
                .ForEach(
                    f =>
                    context.FunctionsInRoles.Add(new FunctionsInRoles
                        {
                            FunctionItem = f,
                            IsValid = true,
                            Role = afrpRole
                        }));

            var cafmRole = new Roles
                {
                    Application = cafm,
                    RoleName = "系统管理员",
                    LoweredRoleName = "系统管理员"
                };
            context.Roles.Add(cafmRole);
            cafm.FunctionItems.ToList()
                .ForEach(
                    f =>
                    context.FunctionsInRoles.Add(new FunctionsInRoles
                        {
                            FunctionItem = f,
                            IsValid = true,
                            Role = cafmRole
                        }));

            // 用户角色
            var users = new Users
                {
                    UserName = "admin",
                    Application = uniCloud,
                    CreateDate = DateTime.Now,
                    PasswordFormat = 1,
                    LoweredUserName = "admin",
                    Password = "AltwSfIKtTOy1O6k2S6Q8oBWq6A=",
                    IsApproved = true,
                    IsLockedOut = false,
                    LastLockoutDate = DateTime.Now,
                    FailedPasswordAttemptCount = 0,
                    FailedPasswordAttemptWindowStart = DateTime.Now,
                    FailedPasswordAnswerAttemptCount = 0,
                    FailedPasswordAnswerAttemptWindowStart = DateTime.Now,
                    LastActivityDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    LastPasswordChangedDate = DateTime.Now,
                    UserInRoles = new List<UserInRole>
                        {
                            new UserInRole {IsValid = true, Roles = uniAuthRole},
                            new UserInRole {IsValid = true, Roles = afrpRole},
                            new UserInRole {IsValid = true, Roles = cafmRole},
                        }
                };
            context.Users.Add(users);

            // 用户密码历史
            var pHistory = new PasswordHistory
                {
                    Users = users,
                    Password = users.Password,
                    CreateDate = DateTime.Now,
                };
            context.PasswordHistories.Add(pHistory);

            #endregion

        }
    }
}
