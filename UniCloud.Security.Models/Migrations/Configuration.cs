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

            #region 1��UniAuth��ͳһȨ�޹���

            var uniAuth = new Applications
            {
                ApplicationName = "UniAuth",
                LoweredApplicationName = "UniAuth".ToLower(),
                ModuleName = "UniAuthModule",
                ViewNameSpace = "UniCloud.UniAuth.Views.",
                Description = "ͳһȨ�޹���"
            };
            context.Applications.AddOrUpdate(a => a.ApplicationName, uniAuth);

            var uniAuthFuncs = new FunctionItem
            {
                Application = uniAuth,
                LevelCode = "01",
                Name = "ͳһȨ�޹���",
                ViewName = null,
                Description = null,
                IsLeaf = false,
                IsButton = false,
                SubItems = new List<FunctionItem>
                {

                    #region Ӧ�üܹ�

                    new FunctionItem
                    {
                        Application = uniAuth,
                        LevelCode = "01.01",
                        Name = "Ӧ�üܹ�",
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
                                Name="Ӧ��", 
                                ViewName=null,
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.01.02",
                                Name="��ɫ", 
                                ViewName=null,
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.01.03",
                                Name="����", 
                                ViewName=null,
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                        }
                    },

                    #endregion

                    #region ��Ȩ

                    new FunctionItem
                    {
                        Application = uniAuth,
                        LevelCode = "01.02",
                        Name = "��Ȩ",
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
                                Name="�޸�����", 
                                ViewName="UserChangePwdView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                              new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.02.02",
                                Name="�༭�û�", 
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
                                        Name="����û�",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=uniAuth,
                                        LevelCode="01.02.02.02",
                                        Name="�Ƴ��û�",
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
                                Name="�û���ɫ", 
                                ViewName="UserRoleAllotView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=uniAuth, 
                                LevelCode="01.02.04",
                                Name="��ɫ����", 
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
                                        Name="��ӽ�ɫ",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=uniAuth,
                                        LevelCode="01.02.04.02",
                                        Name="�Ƴ���ɫ",
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

            #region 2��AFRP�����չ�˾������Դ�滮

            var afrp = new Applications
            {
                ApplicationName = "AFRP",
                LoweredApplicationName = "AFRP".ToLower(),
                ModuleName = "AFRPModule",
                ViewNameSpace = "UniCloud.AFRP.Views.",
                Description = "������Դ�滮"
            };
            context.Applications.Add(afrp);

            var afrpFuncs = new FunctionItem
            {
                Application = afrp,
                LevelCode = "02",
                Name = "������Դ�滮",
                ViewName = null,
                Description = null,
                IsLeaf = false,
                IsButton = false,
                SubItems = new List<FunctionItem>
                {

                    #region �ƻ�����

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.01",
                        Name = "���Ƽƻ�",
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
                                Name="׼��", 
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
                                        Name="׼����ȼƻ�",
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
                                Name="�ƶ�", 
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
                                        Name="�����°汾�ƻ�",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.02.02",
                                        Name="��Ӽƻ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.02.03",
                                        Name="�Ƴ��ƻ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.02.04",
                                        Name="�ύ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.02.05",
                                        Name="���",
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
                                Name="����", 
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
                                        Name="��Ӹ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.03.02",
                                        Name="����",
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
                                Name="����", 
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
                                        Name="�ύ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.04.02",
                                        Name="���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.04.03",
                                        Name="����",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.01.04.04",
                                        Name="��������",
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

                    #region ִ�мƻ�

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.02",
                        Name = "ִ�мƻ�",
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
                                Name="��������", 
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
                                        Name="����������",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.02",
                                        Name="��Ӹ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.03",
                                        Name="�ύ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.04",
                                        Name="���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.05",
                                        Name="����",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                       new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.01.06",
                                        Name="�޸�����",
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
                                Name="ά������", 
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
                                        Name="����������",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.02",
                                        Name="��Ӹ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.03",
                                        Name="�ύ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.04",
                                        Name="���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.05",
                                        Name="����",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.02.06",
                                        Name="�޸�����",
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
                                Name="��ɼƻ�", 
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
                                        Name="��ɼƻ�",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.03.02",
                                        Name="�ύ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.03.03",
                                        Name="���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.03.04",
                                        Name="����",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.02.03.05",
                                        Name="�޸����",
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

                    #region ��������

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.03",
                        Name = "��������",
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
                                Name="�������Ȩ", 
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
                                        Name="����������Ȩ",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.02",
                                        Name="�Ƴ�����Ȩ",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.03",
                                        Name="�ύ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.04",
                                        Name="���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.05",
                                        Name="����",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                      new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.01.06",
                                        Name="�޸�����Ȩ",
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
                                Name="����ɻ�����", 
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
                                        Name="��������������",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                    new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.02.02",
                                        Name="�ύ���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.02.03",
                                        Name="���",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },                                   
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.03.02.04",
                                        Name="�޸���������",
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

                    #region �����ƻ�ִ��
                
                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.04",
                        Name = "�����ƻ�ִ��",
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
                                Name="�ƻ�", 
                                ViewName="AfrpPlanQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.04.02",
                                Name="�ƻ�ִ��", 
                                ViewName="AfrpPlanPerformView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.04.03",
                                Name="����", 
                                ViewName="AfrpRequestQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.04.04",
                                Name="����", 
                                ViewName="AfrpApprovalQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                        }
                    },

                    #endregion

                    #region ͳ�Ʒ���

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.05",
                        Name = "ͳ�Ʒ���",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {

                            #region ��������

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.01",
                                Name="�ɻ������仯", 
                                ViewName="AfrpFleetTrendAllView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.02",
                                Name="�ͻ������仯", 
                                ViewName="AfrpFleetTrendPnrView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.03",
                                Name="���������仯", 
                                ViewName="AfrpFleetTrendCargoView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.04",
                                Name="�ڲ�ɻ�", 
                                ViewName="AfrpFleetRegisteredView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region �ɻ���Դ

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.11",
                                Name="������ʽ", 
                                ViewName="AfrpImportTypeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region ���ӽṹ

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.21",
                                Name="���ӽṹ", 
                                ViewName="AfrpFleetStructureView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region ����

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.31",
                                Name="�ɻ�����", 
                                ViewName="AfrpFleetAgeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region �ɻ�����

                            new FunctionItem
                            { 
                                Application=afrp, 
                                LevelCode="02.05.41",
                                Name="�ɻ�����", 
                                ViewName="AfrpFleetQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                        }
                    },

                    #endregion

                    #region ϵͳ����

                    new FunctionItem
                    {
                        Application = afrp,
                        LevelCode = "02.06",
                        Name = "ϵͳ����",
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
                                Name="ά���ʼ��˺�", 
                                ViewName="AfrpMailSettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            {
                                Application=afrp, 
                                LevelCode="02.06.02",
                                Name="ά��������ɫ", 
                                ViewName="AfrpColorSettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            {
                                Application=afrp, 
                                LevelCode="02.06.03",
                                Name="ά����Ӧ��", 
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
                                        Name="���ӹ�Ӧ��",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.06.03.02",
                                        Name="�Ƴ���Ӧ��",
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
                                Name="ά���ֹ�˾", 
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
                                        Name="���ӷֹ�˾",
                                        ViewName=null,
                                        Description=null,
                                        IsLeaf=false,
                                        IsButton=true
                                    },
                                     new FunctionItem
                                    {
                                        Application=afrp,
                                        LevelCode="02.06.04.02",
                                        Name="�Ƴ��ֹ�˾",
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
                                Name="ά���ӹ�˾", 
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

            #region 3��CAFM���񺽾ֻ��ӹ���

            var cafm = new Applications
            {
                ApplicationName = "CAFM",
                LoweredApplicationName = "CAFM".ToLower(),
                ModuleName = "CAFMModule",
                ViewNameSpace = "CAAC.CAFM.Views.",
                Description = "�񺽻��ӹ���"
            };
            context.Applications.AddOrUpdate(a => a.ApplicationName, cafm);

            var cafmFuncs = new FunctionItem
            {
                Application = cafm,
                LevelCode = "03",
                Name = "�񺽻��ӹ���",
                ViewName = null,
                Description = null,
                IsLeaf = false,
                IsButton = false,
                SubItems = new List<FunctionItem>
                {

                    #region ����ƻ�

                    new FunctionItem
                    {
                        Application = cafm,
                        LevelCode = "03.01",
                        Name = "����ƻ�",
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
                                Name="�ͻ��ƻ�", 
                                ViewName="CafmPlanPnrView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.02",
                                Name="�����ƻ�", 
                                ViewName="CafmPlanCargoView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.03",
                                Name="���ƻ�����", 
                                ViewName="CafmPlanPublishView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.04",
                                Name="�ƻ�ִ��", 
                                ViewName="CafmPlanPerformView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.05",
                                Name="��ѯ�ƻ�", 
                                ViewName="CafmPlanQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.01.06",
                                Name="�����ƻ�ִ��", 
                                ViewName="CafmPerformAnalyseView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                        }
                    },

                    #endregion

                    #region ��������

                    new FunctionItem
                    {
                        Application = cafm,
                        LevelCode = "03.02",
                        Name = "��������",
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
                                Name="¼������", 
                                ViewName="CafmApprovalView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.02.02",
                                Name="�������ƥ��", 
                                ViewName="CafmApprovalCheckView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.02.03",
                                Name="��������", 
                                ViewName="CafmRequestView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.02.04",
                                Name="��������", 
                                ViewName="CafmApprovalManageView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                        }
                    },

                    #endregion

                    #region ͳ�Ʒ���

                    new FunctionItem
                    {
                        Application = cafm,
                        LevelCode = "03.03",
                        Name = "ͳ�Ʒ���",
                        ViewName = null,
                        Description = null,
                        IsLeaf = false,
                        IsButton = false,
                        SubItems = new List<FunctionItem>
                        {

                            #region ��������

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.01",
                                Name="�ɻ������仯", 
                                ViewName="CafmFleetTrendAllView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.02",
                                Name="�ͻ������仯", 
                                ViewName="CafmFleetTrendPnrView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.03",
                                Name="���������仯", 
                                ViewName="CafmFleetTrendCargoView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.04",
                                Name="�ڲ�ɻ�", 
                                ViewName="CafmFleetRegisteredView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region �ɻ���Դ

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.11",
                                Name="������ʽ", 
                                ViewName="CafmImportTypeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.12",
                                Name="��Ӧ��", 
                                ViewName="CafmSupplierView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.13",
                                Name="������", 
                                ViewName="CafmManufacturerView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region ���ӽṹ

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.21",
                                Name="�ɻ�����", 
                                ViewName="CafmAircraftTypeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region ����

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.31",
                                Name="�ɻ�����", 
                                ViewName="CafmFleetAgeView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                            #region �ɻ�����

                            new FunctionItem
                            { 
                                Application=cafm, 
                                LevelCode="03.03.41",
                                Name="�ɻ�����", 
                                ViewName="CafmFleetQueryView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },

                            #endregion

                        }
                    },

                    #endregion

                    #region ϵͳ����

                    new FunctionItem
                    {
                        Application = cafm,
                        LevelCode = "03.04",
                        Name = "ϵͳ����",
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
                                Name="��־", 
                                ViewName="AfrpLogView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                             new FunctionItem
                            {
                                Application=cafm, 
                                LevelCode="03.04.02",
                                Name="ά��������ɫ", 
                                ViewName="CafmColorSettingView",
                                Description=null,
                                IsLeaf=true,
                                IsButton=false,
                            },
                            new FunctionItem
                            {
                                Application=cafm, 
                                LevelCode="03.04.03",
                                Name="ά���ʼ��˺�", 
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

            #region ���ӹ���Ա�û�

            // ��ɫ����
            var uniAuthRole = new Roles
                {
                    Application = uniAuth,
                    RoleName = "ϵͳ����Ա",
                    LoweredRoleName = "ϵͳ����Ա"
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
                    RoleName = "ϵͳ����Ա",
                    LoweredRoleName = "ϵͳ����Ա"
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
                    RoleName = "ϵͳ����Ա",
                    LoweredRoleName = "ϵͳ����Ա"
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

            // �û���ɫ
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

            // �û�������ʷ
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
