namespace UniCloud.Fleet.Models.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Xml.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<UniCloud.Fleet.Models.FleetEntities>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(UniCloud.Fleet.Models.FleetEntities context)
        {
            this._context = context;

            #region ��Ҫ����ά�������ݣ��ɷ���ά���ú�ͳһ�ַ��������û�

            // Ӧ�û�������
            CreateXMLConfig();
            // �����ַ
            CreateMailAddress();
            // ����Ȩ��
            CreateOwner();
            // ������
            CreateManager();
            // ���չ�˾
            CreateAirlines();
            // ������
            CreateManufacturer();
            // �ɻ����
            CreateAircraftCategory();
            // ����
            CreateAircraftType();
            // ��������
            CreateDeliveryRisk();
            // ������� 
            CreateActionCategory();
            // �滮
            CreateProgramming();
            // ���
            CreateAnnual();
            //��ɫ����
            CreatColorSetting();
            #endregion

        }

        private UniCloud.Fleet.Models.FleetEntities _context;

        private const string CT_More250 = "B962079E-C968-46E4-99A8-24771F5C79CD";
        private const string CT_More100 = "E2629307-6770-4F2E-B104-224BF79D3DF1";
        private const string CT_Less100 = "5BA59D84-632C-4635-B7F0-B6B7D7FACC2B";
        private const string CT_CargoBig = "34E4C7B3-6A30-4BEC-AC91-334310839EAC";
        private const string CT_CargoSmall = "984C4402-6FE7-42AC-894D-1C60624B2B09";

        private const string Manu_B = "DC520EC0-8A90-4B4E-A982-E321D379380A";
        private const string Manu_A = "9F14444A-228D-4681-9B33-835AB10B608C";
        private const string Manu_CRJ = "EC88B5C8-A882-4724-8B0C-ED1CE44DE736";
        private const string Manu_DO = "338CE613-E742-4A4A-85AC-7744A4D38F47";
        private const string Manu_BRA = "2A2B2E9B-ED8F-4D00-9D12-CF264012206B";
        private const string Manu_MA = "AF10EF52-0CD3-4A4F-8AEB-03979F1B62A3";
        private const string Manu_CA = "BC67BB37-3C52-4C6A-96FD-5EF8E88D19EA";
        private const string Manu_TU = "40A9F375-A798-4B0C-A004-CCD890FDD658";


        /// <summary>
        /// Ӧ�û�������
        /// </summary>
        private void CreateXMLConfig()
        {
            var xmlConfigList = new List<XmlConfig>
            {
                //��������
                new XmlConfig
                {
                    XmlConfigID = Guid.NewGuid(),
                    ConfigType = "��������",
                    VersionNumber = 1,
                    XmlContent = new XElement("AgeDeploy",
                                 new XElement("Item", new XAttribute("Start", "0"), new XAttribute("End", "2"), "0��2��֮��"),
                                 new XElement("Item", new XAttribute("Start", "2"), new XAttribute("End", "5"), "2��5��֮��"),
                                 new XElement("Item", new XAttribute("Start", "5"), new XAttribute("End", "7"), "5��7��֮��"),
                                 new XElement("Item", new XAttribute("Start", "7"), new XAttribute("End", "50"), "7��50��֮��")
                                 )
                }
            };
            xmlConfigList.ForEach(m => this._context.XmlConfigs.AddOrUpdate(u => u.XmlConfigID, m));
        }

        private void CreatColorSetting()
        {
            var xmlConfigList = new List<XmlConfig>
            {
                //��ɫ����
                new XmlConfig
                {
                    XmlConfigID = Guid.NewGuid(),
                    ConfigType = "��ɫ����",
                    VersionNumber = 1,
                    XmlContent = new XElement("FleetColorSet",
                                 new XElement("ColorSet",
                                 new XElement("Type",new XAttribute("TypeName", "���չ�˾"), 
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#FF339933")),
                                 new XElement("Item", new XAttribute("Name", "������"), new XAttribute("Color", "#FF00ABA9")),
                                 new XElement("Item", new XAttribute("Name", "�"), new XAttribute("Color", "#FF996600")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#FFF09609")),
                                 new XElement("Item", new XAttribute("Name", "������"), new XAttribute("Color", "#FFA200FF")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#FFFF7F50")),
                                 new XElement("Item", new XAttribute("Name", "�Ϻ�"), new XAttribute("Color", "#FFFF0097")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#FF8CBF26")),
                                 new XElement("Item", new XAttribute("Name", "�Ϻ�"), new XAttribute("Color", "#FF1BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "���캽"), new XAttribute("Color", "#FFE671B8")),
                                 new XElement("Item", new XAttribute("Name", "�ú�"), new XAttribute("Color", "#E8A200FF")),
                                 new XElement("Item", new XAttribute("Name", "���»�"), new XAttribute("Color", "#E8339933")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#E8F09609")),
                                 new XElement("Item", new XAttribute("Name", "���ӽ�"), new XAttribute("Color", "#E8FF7F50")),
                                 new XElement("Item", new XAttribute("Name", "�׶�"), new XAttribute("Color", "#E81BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "�캽"), new XAttribute("Color", "#E800ABA9")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#E8FF0097")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#E8996600")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#E88CBF26")),
                                 new XElement("Item", new XAttribute("Name", "ɽ��"), new XAttribute("Color", "#E8E671B8")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#D01BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "�ӱ���"), new XAttribute("Color", "#D0339933")),
                                 new XElement("Item", new XAttribute("Name", "�ɶ���"), new XAttribute("Color", "#D0F09609")),
                                 new XElement("Item", new XAttribute("Name", "�Ҹ���"), new XAttribute("Color", "#D0A200FF")),
                                 new XElement("Item", new XAttribute("Name", "���غ�"), new XAttribute("Color", "#D0FF7F50")),
                                 new XElement("Item", new XAttribute("Name", "�ʺ�"), new XAttribute("Color", "#D000ABA9")),
                                 new XElement("Item", new XAttribute("Name", "�¿�"), new XAttribute("Color", "#D0FF0097")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#D08CBF26")),
                                 new XElement("Item", new XAttribute("Name", "���"), new XAttribute("Color", "#D0996600")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#D0E671B8")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#B81BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#B8339933")),
                                 new XElement("Item", new XAttribute("Name", "˳��"), new XAttribute("Color", "#B8F09609")),
                                 new XElement("Item", new XAttribute("Name", "�Ѻ�"), new XAttribute("Color", "#B8FF7F50")),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#B800ABA9")),
                                 new XElement("Item", new XAttribute("Name", "�л���"), new XAttribute("Color", "#B8A200FF"))
                                 ),
                                 new XElement("Type",new XAttribute("TypeName", "����"), 
                                 new XElement("Item", new XAttribute("Name", "250�����Ͽͻ�"), new XAttribute("Color", "#FF339933")),
                                 new XElement("Item", new XAttribute("Name", "100-200���ͻ�"), new XAttribute("Color", "#FF1BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "100�����¿ͻ�"), new XAttribute("Color", "#FFFF7F50")),
                                 new XElement("Item", new XAttribute("Name", "���ͻ���"), new XAttribute("Color", "#FFF09609")),
                                 new XElement("Item", new XAttribute("Name", "���ͻ���"), new XAttribute("Color", "#FFA200FF"))
                                 ),
                                 new XElement("Type",new XAttribute("TypeName", "����"), 
                                 new XElement("Item", new XAttribute("Name", "A380"), new XAttribute("Color", "#FF339933")),
                                 new XElement("Item", new XAttribute("Name", "A350"), new XAttribute("Color", "#B500ABA3")),
                                 new XElement("Item", new XAttribute("Name", "B747-400P"), new XAttribute("Color", "#FF1BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "B747-400C"), new XAttribute("Color", "#FFF09609")),
                                 new XElement("Item", new XAttribute("Name", "B747-8"), new XAttribute("Color", "#FFA200FF")),
                                 new XElement("Item", new XAttribute("Name", "B777-200A"), new XAttribute("Color", "#FFFF7F50")),
                                 new XElement("Item", new XAttribute("Name", "B777-200B"), new XAttribute("Color", "#FF8CBF26")),
                                 new XElement("Item", new XAttribute("Name", "B777-300"), new XAttribute("Color", "#E88CBF26")),
                                 new XElement("Item", new XAttribute("Name", "B777-300ER"), new XAttribute("Color", "#FFFF0097")),
                                 new XElement("Item", new XAttribute("Name", "B767-300"), new XAttribute("Color", "#FF996600")),
                                 new XElement("Item", new XAttribute("Name", "B767-300ER"), new XAttribute("Color", "#E8996600")),
                                 new XElement("Item", new XAttribute("Name", "B787-8"), new XAttribute("Color", "#E8339933")),
                                 new XElement("Item", new XAttribute("Name", "B787-9"), new XAttribute("Color", "#E8F09609")),
                                 new XElement("Item", new XAttribute("Name", "A340-300"), new XAttribute("Color", "#FF00ABA9")),
                                 new XElement("Item", new XAttribute("Name", "A340-600"), new XAttribute("Color", "#D08CBF26")),
                                 new XElement("Item", new XAttribute("Name", "A300-600"), new XAttribute("Color", "#FFE671B8")),
                                 new XElement("Item", new XAttribute("Name", "A330-200"), new XAttribute("Color", "#E81BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "A330-300"), new XAttribute("Color", "#D0339933")),
                                 new XElement("Item", new XAttribute("Name", "B757-200"), new XAttribute("Color", "#E8FF0097")),
                                 new XElement("Item", new XAttribute("Name", "B737-300"), new XAttribute("Color", "#B8339933")),
                                 new XElement("Item", new XAttribute("Name", "B737-400"), new XAttribute("Color", "#E800ABA9")),
                                 new XElement("Item", new XAttribute("Name", "B737-700"), new XAttribute("Color", "#B88CBF26")),
                                 new XElement("Item", new XAttribute("Name", "B737-800"), new XAttribute("Color", "#D01BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "B737-900"), new XAttribute("Color", "#D0996600")),
                                 new XElement("Item", new XAttribute("Name", "B737-8"), new XAttribute("Color", "#E8E671B8")),
                                 new XElement("Item", new XAttribute("Name", "A319"), new XAttribute("Color", "#D0F09609")),
                                 new XElement("Item", new XAttribute("Name", "A320"), new XAttribute("Color", "#E8FF7F50")),
                                 new XElement("Item", new XAttribute("Name", "A321"), new XAttribute("Color", "#B8996600")),
                                 new XElement("Item", new XAttribute("Name", "A320NEO"), new XAttribute("Color", "#B8F09609")),
                                 new XElement("Item", new XAttribute("Name", "C919"), new XAttribute("Color", "#A0996600")),
                                 new XElement("Item", new XAttribute("Name", "CRJ200"), new XAttribute("Color", "#D0E671B8")),
                                 new XElement("Item", new XAttribute("Name", "CRJ700"), new XAttribute("Color", "#D0FF7F50")),
                                 new XElement("Item", new XAttribute("Name", "CRJ900"), new XAttribute("Color", "#E8A200FF")),
                                 new XElement("Item", new XAttribute("Name", "DO328"), new XAttribute("Color", "#B81BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "ERJ145"), new XAttribute("Color", "#D000ABA9")),
                                 new XElement("Item", new XAttribute("Name", "EMB190"), new XAttribute("Color", "#D0FF0097")),
                                 new XElement("Item", new XAttribute("Name", "MA60"), new XAttribute("Color", "#B8FF0097")),
                                 new XElement("Item", new XAttribute("Name", "MA600"), new XAttribute("Color", "#A08CBF26")),
                                 new XElement("Item", new XAttribute("Name", "ARJ21"), new XAttribute("Color", "#B8E671B8")),
                                 new XElement("Item", new XAttribute("Name", "B777F"), new XAttribute("Color", "#D0A200FF")),
                                 new XElement("Item", new XAttribute("Name", "B747-200F"), new XAttribute("Color", "#A0E671B8")),
                                 new XElement("Item", new XAttribute("Name", "B747-400F"), new XAttribute("Color", "#A01BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "A300F"), new XAttribute("Color", "#A0339933")),
                                 new XElement("Item", new XAttribute("Name", "MD11F"), new XAttribute("Color", "#A0F09609")),
                                 new XElement("Item", new XAttribute("Name", "B757-200F"), new XAttribute("Color", "#B8FF7F50")),
                                 new XElement("Item", new XAttribute("Name", "B737-300QC"), new XAttribute("Color", "#B8A200FF")),
                                 new XElement("Item", new XAttribute("Name", "B737F"), new XAttribute("Color", "#B800ABA9")),
                                 //CreateAircraftTypeû�еĻ��ͣ�
                                 new XElement("Item", new XAttribute("Name", "C-100"), new XAttribute("Color", "#A000ABA9")),
                                 new XElement("Item", new XAttribute("Name", "C-300"), new XAttribute("Color", "#A0FF0097")),
                                 new XElement("Item", new XAttribute("Name", "A330F"), new XAttribute("Color", "#A0A200FF")),
                                 new XElement("Item", new XAttribute("Name", "B767F"), new XAttribute("Color", "#A0E671B8")),
                                 new XElement("Item", new XAttribute("Name", "TU204"), new XAttribute("Color", "#E8A200FF"))
                                 ),
                                 new XElement("Type",new XAttribute("TypeName", "������ʽ"), 
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#FF1BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "��������"), new XAttribute("Color", "#FFFF7F50")),
                                 new XElement("Item", new XAttribute("Name", "��Ӫ����"), new XAttribute("Color", "#FF339933")),
                                 new XElement("Item", new XAttribute("Name", "ʪ��"), new XAttribute("Color", "#FFF09609")),
                                 new XElement("Item", new XAttribute("Name", "��Ӫ��������"), new XAttribute("Color", "#FFA200FF")),
                                 new XElement("Item", new XAttribute("Name", "ʪ������"), new XAttribute("Color", "#FF00ABA9"))
                                 ),
                                 new XElement("Type",new XAttribute("TypeName", "����"),
                                 new XElement("Item", new XAttribute("Name", "0��2��֮��"), new XAttribute("Color", "#FF1BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "2��5��֮��"), new XAttribute("Color", "#FF339933")),
                                 new XElement("Item", new XAttribute("Name", "5��7��֮��"), new XAttribute("Color", "#FFF09609")),
                                 new XElement("Item", new XAttribute("Name", "7��50��֮��"), new XAttribute("Color", "#FFFF7F50"))
                                 ),
                                 new XElement("Type",new XAttribute("TypeName", "��Ӧ��"),
                                 new XElement("Item", new XAttribute("Name", "���ڹ�Ӧ��"), new XAttribute("Color", "#FF1BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "���⹩Ӧ��"), new XAttribute("Color", "#FFFF7F50"))
                                 ),
                                 new XElement("Type",new XAttribute("TypeName", "�����仯"), 
                                 new XElement("Item", new XAttribute("Name", "�ɻ������ӣ�"), new XAttribute("Color", "#FF00ABA9")),
                                 new XElement("Item", new XAttribute("Name", "��λ�����ӣ�"), new XAttribute("Color", "#FF339933")),
                                 new XElement("Item", new XAttribute("Name", "���������ӣ�"), new XAttribute("Color", "#FFFF0097")),
                                 new XElement("Item", new XAttribute("Name", "�ɻ���"), new XAttribute("Color", "#FF8CBF26")),
                                 new XElement("Item", new XAttribute("Name", "��λ��"), new XAttribute("Color", "#FF1BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "������"), new XAttribute("Color", "#FFF09609"))
                                 ),
                                 new XElement("Type",new XAttribute("TypeName", "������"),
                                 new XElement("Item", new XAttribute("Name", "����"), new XAttribute("Color", "#FFFF7F50")),
                                 new XElement("Item", new XAttribute("Name", "���пͳ�"), new XAttribute("Color", "#FF1BA1E2")),
                                 new XElement("Item", new XAttribute("Name", "�Ӱ͵�"), new XAttribute("Color", "#FFA200FF")),
                                 new XElement("Item", new XAttribute("Name", "�����"), new XAttribute("Color", "#FF00ABA9")),
                                 new XElement("Item", new XAttribute("Name", "�������չ�ҵ��˾"), new XAttribute("Color", "#FFF09609")),
                                 new XElement("Item", new XAttribute("Name", "�����ɻ���ҵ��˾"), new XAttribute("Color", "#FF339933")),
                                 new XElement("Item", new XAttribute("Name", "�й��̷�"), new XAttribute("Color", "#FF8CBF26")),
                                 new XElement("Item", new XAttribute("Name", "����˹ͼ���з�˾"), new XAttribute("Color", "#FF1BA1E2"))
                            )
                         )
                    )
                                 
                }
            };
            xmlConfigList.ForEach(m => this._context.XmlConfigs.AddOrUpdate(u => u.XmlConfigID, m));
        }

        /// <summary>
        /// �����ַ
        /// 1��2012��8��15�ճ�ʼ����
        /// </summary>
        private void CreateMailAddress()
        {
            //var mailAddresses = new List<MailAddress>
            //{
            //    new MailAddress{ MailAddressID=Guid.Parse("20CDF05C-396F-4A61-9D6B-080015BE0D8F"),
            //                    OwnerID = Guid.Parse("31A9DE51-C207-4A73-919C-21521F17FEF9"),
            //                    SmtpHost = "mail.3g365.com",
            //                    SendPort = 25,
            //                    Pop3Host = "mail.3g365.com",
            //                    ReceivePort = 110,
            //                    LoginUser ="unicloud@3g365.com",
            //                    LoginPassword ="soft123"
            //    }
            //};
            //mailAddresses.ForEach(m => this._context.MailAddresses.AddOrUpdate(u => u.MailAddressID, m));
        }

        /// <summary>
        /// ����Ȩ��
        /// </summary>
        private void CreateOwner()
        {

        }

        /// <summary>
        /// ������
        /// 1��2012��8��15�ճ�ʼ����
        /// </summary>
        private void CreateManager()
        {
            var managers = new List<Manager>
            {
                new Manager{ OwnerID=Guid.Parse("31A9DE51-C207-4A73-919C-21521F17FEF9"),
                    Name="�񺽾�"},
                new Manager{ OwnerID=Guid.Parse("EDE13A50-2C89-489A-A44E-B8C690D990AB"),
                    Name="����ί"}
            };
            managers.ForEach(m => this._context.Owners.AddOrUpdate(u => u.OwnerID, m));
        }

        /// <summary>
        /// ���չ�˾
        /// 1��2012��8��15�ճ�ʼ����
        /// </summary>
        private void CreateAirlines()
        {
            var airlines = new List<Airlines>
            {
                new Airlines{ OwnerID=Guid.Parse("119F5D5B-201D-4B65-BFB5-9B19A706A2EE"), 
                    Name="�й����ʺ��չɷ����޹�˾", 
                    ShortName="����", 
                    AirlinesType=AirlinesType.TransportAirline, 
                    IATACode="CA", 
                    ICAOCode="CCA"},
                new Airlines{ OwnerID=Guid.Parse("204311DE-1F2B-4518-AE5F-6E1904BC413A"),
                    Name="�й����ʻ��˺������޹�˾",
                    ShortName="������",
                    AirlinesType=AirlinesType.TransportAirline, 
                    IATACode="CA",
                    ICAOCode="CAO"},
                new Airlines{ OwnerID=Guid.Parse("49A84FC4-0718-4C17-89F9-E7CCA762B1F2"),
                    Name="���ں����������ι�˾",
                    ShortName="�",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="ZH",
                    ICAOCode="CSZ"},
                new Airlines{ OwnerID=Guid.Parse("D99FB662-E8F9-4712-A2EF-FF430CA552D4"),
                    Name="�����������޹�˾",
                    ShortName="����",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="KY",
                    ICAOCode="KNA"},
                new Airlines{ OwnerID=Guid.Parse("25D3DAC6-D1E4-4013-9CE0-FEFDB812EF5B"),
                    Name="���������������ι�˾",
                    ShortName="������",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="CA",
                    ICAOCode="CCD"},

                new Airlines{ OwnerID=Guid.Parse("41051A60-0225-4A18-8691-E52249E48751"), 
                    Name="�й��������չɷ����޹�˾", 
                    ShortName="����", 
                    AirlinesType=AirlinesType.TransportAirline,  
                    IATACode="MU", 
                    ICAOCode="CES"},
                new Airlines{ OwnerID=Guid.Parse("1F073BD8-727B-4B7E-BF92-0453E92F3EC9"),
                    Name="�Ϻ��������޹�˾",
                    ShortName="�Ϻ�",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="FM",
                    ICAOCode="CSH"},
                new Airlines{ OwnerID=Guid.Parse("E9A6FAD3-BE81-4A5C-B3F8-E666A21118D0"),
                    Name="�й����Ϻ������޹�˾",
                    ShortName="����",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="KN",
                    ICAOCode="CUA"},
                new Airlines{ OwnerID=Guid.Parse("90B55F6D-0F9F-4B33-94B9-BC91E689AB99"),
                    Name="�й����˺������޹�˾",
                    ShortName="�л���",
                    AirlinesType=AirlinesType.TransportAirline, 
                    IATACode="CK", 
                    ICAOCode="CHY"},

                new Airlines{ OwnerID=Guid.Parse("BBC333AA-E00C-4D87-83F5-4AE9727B800D"),
                    Name="�й��Ϸ����չɷ����޹�˾",
                    ShortName="�Ϻ�",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="CZ",
                    ICAOCode="CSN"},
                new Airlines{ OwnerID=Guid.Parse("FE4A8301-2080-4D47-BBA4-57994FFFBC06"),
                    Name="���캽���������ι�˾",
                    ShortName="���캽",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="0Q",
                    ICAOCode="CQN"},
                new Airlines{ OwnerID=Guid.Parse("9DE9595C-911F-41AA-B1B0-F1CCA9D458FF"),
                    Name="���ź������޹�˾",
                    ShortName="�ú�",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="MF",
                    ICAOCode="CXA"},

                new Airlines{ OwnerID=Guid.Parse("F1AE6CE4-8CAE-4831-B1B6-403237AC11F1"),
                    Name="���»��������޹�˾",
                    ShortName="���»�",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="CN",
                    ICAOCode="GDC"},
                new Airlines{ OwnerID=Guid.Parse("DB322284-5AF7-4948-832F-66C8551EBB70"),
                    Name="���Ϻ��չɷ����޹�˾",
                    ShortName="����",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="HU",
                    ICAOCode="CHH"},
                new Airlines{ OwnerID=Guid.Parse("DF7522EB-1E2F-4760-A517-76E6CB56A016"),
                    Name="���ӽ����˺������޹�˾", 
                    ShortName="���ӽ�", 
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="Y8", 
                    ICAOCode="YZR"},
                new Airlines{ OwnerID=Guid.Parse("A99AF511-B59B-464E-9CF3-FB02E31A84A2"),
                    Name="�׶����չɷ����޹�˾",
                    ShortName="�׶�",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="JD",
                    ICAOCode="DER"},
                new Airlines{ OwnerID=Guid.Parse("94D4A39C-71DD-4BAF-82CF-7BFB1562051B"),
                    Name="��򺽿��������ι�˾", 
                    ShortName="�캽", 
                    AirlinesType=AirlinesType.TransportAirline, 
                    IATACode="GS",
                    ICAOCode="GCR"},
                new Airlines{ OwnerID=Guid.Parse("A7153179-9FB1-43D3-A3E8-5759F426CC0C"),
                    Name="�������������������ι�˾", 
                    ShortName="����", 
                    AirlinesType=AirlinesType.TransportAirline, 
                    IATACode="8L",
                    ICAOCode="LKE"} ,
                new Airlines{ OwnerID=Guid.Parse("004B37C1-70B8-4071-98AB-0BB73C466D00"),
                    Name="���������������ι�˾",
                    ShortName="����",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="PN",
                    ICAOCode="CHB"},

                new Airlines{ OwnerID=Guid.Parse("1978ADFC-A2FD-40CC-9A26-6DEDB55C335F"),
                    Name="�Ĵ����չɷ����޹�˾",
                    ShortName="����",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="3U",
                    ICAOCode="CSC"},

                new Airlines{ OwnerID=Guid.Parse("1A1CF03E-C2B9-47A9-B783-CD241A3D494F"),
                    Name="ɽ�����չɷ����޹�˾",
                    ShortName="ɽ��",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="SC",
                    ICAOCode="CDG"},

                new Airlines{ OwnerID=Guid.Parse("98D7C37E-BF02-4E2A-88A8-C2101EF4547B"),
                    Name="���ĺ������޹�˾",
                    ShortName="����",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="G5",
                    ICAOCode="HXA"},

                new Airlines{ OwnerID=Guid.Parse("568919F3-0199-472C-9F7A-354BB8EFBFEE"),
                    Name="�ӱ��������޹�˾",
                    ShortName="�ӱ���",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="NS",
                    ICAOCode="DBH"},

                new Airlines{ OwnerID=Guid.Parse("80B1ABC4-0CB0-4E8D-9C86-F86866B20486"),
                    Name="�ɶ��������޹�˾",
                    ShortName="�ɶ���",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="EU",
                    ICAOCode="UEA"},

                new Airlines{ OwnerID=Guid.Parse("FF0A172F-AAB1-44FB-9261-09F16446B3C5"),
                    Name="�Ҹ��������޹�˾",
                    ShortName="�Ҹ���",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="JR",
                    ICAOCode="JOY"},

                new Airlines{ OwnerID=Guid.Parse("1376B72F-583D-40AB-B9E1-375098EB2A1A"),
                    Name="���غ������޹�˾",
                    ShortName="���غ�",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="TV",
                    ICAOCode="TBA"},

                new Airlines{ OwnerID=Guid.Parse("C1DDFDC0-5337-4519-B8BD-A8D2B2226396"),
                    Name="�й��������������������ι�˾",
                    ShortName="�ʺ�", 
                    AirlinesType=AirlinesType.TransportAirline, 
                    IATACode="8Y", 
                    IsCurrent = true,
                    ICAOCode="CYZ"},

                new Airlines{ OwnerID=Guid.Parse("2A11460A-A4BD-4718-B7BA-4A9145F240E6"),
                    Name="�¿��������޹�˾",
                    ShortName="�¿�",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="BK",
                    ICAOCode="OKA"},

                new Airlines{ OwnerID=Guid.Parse("6C0A32F7-50C5-46D6-9D8B-E0D30C96CC8C"),
                    Name="���ﺽ�����޹�˾",
                    ShortName="����",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="9C",
                    ICAOCode="CQH"},

                new Airlines{ OwnerID=Guid.Parse("F1C6EA69-C195-406A-B359-702563D149FE"), 
                    Name="�����ʻ��˺����������ι�˾",
                    ShortName="���", 
                    AirlinesType=AirlinesType.TransportAirline, 
                    IATACode="JI",
                    ICAOCode="JAE"},

                new Airlines{ OwnerID=Guid.Parse("34BB8897-3860-4BDE-9B77-3924EF131544"),
                    Name="���ڶ����������޹�˾",
                    ShortName="����", 
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="J5", 
                    ICAOCode="EPA"},

                new Airlines{ OwnerID=Guid.Parse("09B24BF5-EFD9-44B0-93FD-8A4664F6F431"),
                    Name="�Ϻ����麽�����޹�˾",
                    ShortName="����",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="HO",
                    ICAOCode="DKH"},

                new Airlines{ OwnerID=Guid.Parse("D0D559B2-CF6E-413C-8F87-0A87F41BE073"),
                    Name="���ӹ��ʻ��˺������޹�˾",
                    ShortName="����",
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="GD",
                    ICAOCode="GSC"},

                new Airlines{ OwnerID=Guid.Parse("8BAF2578-CF2F-4386-998D-BD37FD031D8C"),
                    Name="˳�ẽ�����޹�˾",
                    ShortName="˳��", 
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="O3", 
                    ICAOCode="CSS"},

                new Airlines{ OwnerID=Guid.Parse("DEC75B84-042C-4471-B1AC-C6F268E62F0F"),
                    Name="�Ѻ͵�ͨ�������޹�˾",
                    ShortName="�Ѻ�", 
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="UW", 
                    ICAOCode="UTP"},

                new Airlines{ OwnerID=Guid.Parse("E4630DA1-4103-494B-8E7E-5F9C8B92AFC3"),
                    Name="�������ʻ��˺������޹�˾",
                    ShortName="����", 
                    AirlinesType=AirlinesType.TransportAirline,
                    IATACode="GJ", 
                    ICAOCode="CDC"},
            };
            airlines.ForEach(a => this._context.Owners.AddOrUpdate(u => u.OwnerID, a));
        }

        /// <summary>
        /// ������
        /// 1��2012��8��15�ճ�ʼ����
        /// </summary>
        private void CreateManufacturer()
        {
            var manufacturers = new List<Manufacturer>
            {
                new Manufacturer{ OwnerID=Guid.Parse(Manu_B),
                    Name="����"},
                new Manufacturer{ OwnerID=Guid.Parse(Manu_A),
                    Name="���пͳ�"},
                new Manufacturer{ OwnerID=Guid.Parse(Manu_CRJ),
                    Name="�Ӱ͵�"},
                new Manufacturer{ OwnerID=Guid.Parse(Manu_DO),
                    Name="�����"},
                new Manufacturer{ OwnerID=Guid.Parse(Manu_BRA),
                    Name="�������չ�ҵ��˾"},
                new Manufacturer{ OwnerID=Guid.Parse(Manu_MA),
                    Name="�����ɻ���ҵ��˾"},
                new Manufacturer{ OwnerID=Guid.Parse(Manu_CA),
                    Name="�й��̷�"},
                new Manufacturer{ OwnerID=Guid.Parse(Manu_TU),
                    Name="����˹ͼ���з�˾"},
            };
            manufacturers.ForEach(m => this._context.Owners.AddOrUpdate(u => u.OwnerID, m));
        }

        /// <summary>
        /// �ɻ����
        /// 1��2012��8��15�ճ�ʼ����
        /// </summary>

        private void CreateAircraftCategory()
        {
            var aircraftCategories = new List<AircraftCategory>
            {
                new AircraftCategory{AircraftCategoryID=Guid.Parse(CT_More250),
                    Category="�ͻ�",
                    Regional="250�����Ͽͻ�"},
                new AircraftCategory{AircraftCategoryID=Guid.Parse(CT_More100),
                    Category="�ͻ�", 
                    Regional="100-200���ͻ�"},
                new AircraftCategory{AircraftCategoryID=Guid.Parse(CT_Less100), 
                    Category="�ͻ�",
                    Regional="100�����¿ͻ�"},
                new AircraftCategory{AircraftCategoryID=Guid.Parse(CT_CargoBig),
                    Category="����",
                    Regional="���ͻ���"},
                new AircraftCategory{AircraftCategoryID=Guid.Parse(CT_CargoSmall),
                    Category="����", 
                    Regional="���ͻ���"},
            };
            aircraftCategories.ForEach(a => this._context.AircraftCategories.AddOrUpdate(u => u.AircraftCategoryID, a));
        }

        /// <summary>
        /// ����
        /// 1��2012��8��15�ճ�ʼ����
        /// </summary>
        private void CreateAircraftType()
        {
            var aircraftTypes = new List<AircraftType>
            {
                // 250�����Ͽͻ�
                new AircraftType{AircraftTypeID=Guid.Parse("AB65EE49-D110-40F1-B3CE-52CADB0C6B81"),
                    Name="A380",
                    ManufacturerID=Guid.Parse(Manu_A),
                    AircraftCategoryID=Guid.Parse(CT_More250)},

                new AircraftType{AircraftTypeID=Guid.Parse("53871626-C2FE-4B15-9E78-752AA4620ED6"),
                    Name="A350",
                    ManufacturerID=Guid.Parse(Manu_A),
                    AircraftCategoryID=Guid.Parse(CT_More250)},

                new AircraftType{AircraftTypeID=Guid.Parse("638F6F24-226B-4B58-8280-EEDDB305B951"),
                    Name="B747-400P", 
                    ManufacturerID=Guid.Parse(Manu_B),
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("D1DC0C25-769B-4CBB-9EB8-6E22AFA7FD85"),
                    Name="B747-400C",
                    ManufacturerID=Guid.Parse(Manu_B),
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("EA12EB5F-C5F9-43C4-82B3-2FFEC3E598A7"),
                    Name="B747-8",
                    ManufacturerID=Guid.Parse(Manu_B),
                    AircraftCategoryID=Guid.Parse(CT_More250)},

                new AircraftType{AircraftTypeID=Guid.Parse("63616869-C36E-4DD6-A1C2-4AD9E2B8F403"),
                    Name="B777-200A", 
                    ManufacturerID=Guid.Parse(Manu_B),
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("CA7C4ECA-170D-4894-B6E9-8CB7F0AA59D9"), 
                    Name="B777-200B", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("2D722386-5B3F-4547-B41E-F5020DE70B30"), 
                    Name="B777-300", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("20719661-A03E-41D3-865E-C183C1B9E512"), 
                    Name="B777-300ER", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},
 
               new AircraftType{AircraftTypeID=Guid.Parse("24BADE01-E624-4C84-B8B1-388FECC9D773"), 
                    Name="B767-300", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("09219721-7F63-4790-AD85-75BB62D298FE"), 
                    Name="B767-300ER", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},

                new AircraftType{AircraftTypeID=Guid.Parse("6FA3FF32-AF8B-4CD3-B973-275ADB536CBF"), 
                    Name="B787-8",
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("BBB22D28-A9CD-415A-BF0D-F49B68972DC7"), 
                    Name="B787-9",
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},
 
                new AircraftType{AircraftTypeID=Guid.Parse("416734C4-9720-4777-B273-E46F0AED5DEA"), 
                    Name="A340-300", 
                    ManufacturerID=Guid.Parse(Manu_A), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("3632F0D9-6F47-4A80-AF54-7A37D18FC3AB"),
                    Name="A340-600", 
                    ManufacturerID=Guid.Parse(Manu_A),
                    AircraftCategoryID=Guid.Parse(CT_More250)},

                new AircraftType{AircraftTypeID=Guid.Parse("8070D8B1-FA34-4854-9D8D-9250564CEA61"),
                    Name="A300-600", 
                    ManufacturerID=Guid.Parse(Manu_A), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("5C690CB2-2D33-4006-858B-0BE610E9CB47"), 
                    Name="A330-200",
                    ManufacturerID=Guid.Parse(Manu_A), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},
                new AircraftType{AircraftTypeID=Guid.Parse("F8BFBD1E-F2AC-40F2-9C29-A09ECB360DA2"), 
                    Name="A330-300",
                    ManufacturerID=Guid.Parse(Manu_A), 
                    AircraftCategoryID=Guid.Parse(CT_More250)},

                // 100-200���ͻ�
               new AircraftType{AircraftTypeID=Guid.Parse("1826BD57-733C-4679-95C8-7DFFEFAB38F4"), 
                    Name="B757-200", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("A0F3A212-C6C9-48B2-998D-47E052CDA3C1"),
                    Name="B737-300",
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("95D515CD-4DDD-4AC1-AA0C-8D83AD8339F2"),
                    Name="B737-400", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("E8E95A6F-26BA-4EA4-A7D6-360FA6DE1CCD"), 
                    Name="B737-700", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("8571E7E1-387A-4570-9158-A1DFB656CA9F"), 
                    Name="B737-800", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("A8018768-C88A-4CDC-9297-4C4996E970FB"), 
                    Name="B737-900", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("5103CE46-9301-46A8-8D6A-054B8C6025C1"), 
                    Name="B737-8", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},

                new AircraftType{AircraftTypeID=Guid.Parse("967CC88E-C664-4555-A147-DE17F5A697EE"), 
                    Name="A319",
                    ManufacturerID=Guid.Parse(Manu_A), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("EF5DD798-C16D-47CD-A588-ABD257A6B6B6"), 
                    Name="A320", 
                    ManufacturerID=Guid.Parse(Manu_A), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("562DCC8F-9577-4956-86E2-F3A3A0A3EEA2"), 
                    Name="A321", 
                    ManufacturerID=Guid.Parse(Manu_A), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("A4552254-2AF2-43C6-9284-83D1B3D8A7E0"), 
                    Name="A320NEO", 
                    ManufacturerID=Guid.Parse(Manu_A), 
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                new AircraftType{AircraftTypeID=Guid.Parse("799EA50A-2AAF-479D-B297-7A14271599E8"), 
                    Name="C919", 
                    ManufacturerID=Guid.Parse(Manu_CA),
                    AircraftCategoryID=Guid.Parse(CT_More100)},
                // 100�����¿ͻ�
                new AircraftType{AircraftTypeID=Guid.Parse("4EB297C7-DCBC-4EB1-9956-590D4E514351"), 
                    Name="CRJ200", 
                    ManufacturerID=Guid.Parse(Manu_CRJ),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
                new AircraftType{AircraftTypeID=Guid.Parse("55F5D629-959A-4DC9-A653-730140832BAF"), 
                    Name="CRJ700", 
                    ManufacturerID=Guid.Parse(Manu_CRJ),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
                new AircraftType{AircraftTypeID=Guid.Parse("59346863-901F-4C3B-8FD4-3DDF5C824E9D"), 
                    Name="CRJ900", 
                    ManufacturerID=Guid.Parse(Manu_CRJ),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
 
                new AircraftType{AircraftTypeID=Guid.Parse("EA47622C-2096-4A53-8460-B54B91916459"), 
                    Name="C-100", 
                    ManufacturerID=Guid.Parse(Manu_DO),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
                new AircraftType{AircraftTypeID=Guid.Parse("7BBE194A-15E8-4F3A-A4CA-3DADC609DF1A"), 
                    Name="C-300", 
                    ManufacturerID=Guid.Parse(Manu_DO),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},

                new AircraftType{AircraftTypeID=Guid.Parse("D33F2C06-C8EC-45B9-A6FC-9C0E0090B699"), 
                    Name="DO328", 
                    ManufacturerID=Guid.Parse(Manu_DO),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
                new AircraftType{AircraftTypeID=Guid.Parse("3E03BCC5-0AAE-4A20-BD9B-F0FE34B2491F"),
                    Name="ERJ145",
                    ManufacturerID=Guid.Parse(Manu_BRA),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
                new AircraftType{AircraftTypeID=Guid.Parse("B21BF122-2B66-447C-A9FD-B68FA377A931"),
                    Name="EMB190",
                    ManufacturerID=Guid.Parse(Manu_BRA),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
                new AircraftType{AircraftTypeID=Guid.Parse("992AC72E-BB26-4672-A6DC-DFC667048F76"), 
                    Name="MA60",
                    ManufacturerID=Guid.Parse(Manu_MA),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
                new AircraftType{AircraftTypeID=Guid.Parse("C3520CBA-5429-4F5A-A824-B09ADF11E686"), 
                    Name="MA600",
                    ManufacturerID=Guid.Parse(Manu_MA),
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
                new AircraftType{AircraftTypeID=Guid.Parse("BE6EACB6-F873-4540-9926-0C843282F567"), 
                    Name="ARJ21", 
                    ManufacturerID=Guid.Parse(Manu_CA), 
                    AircraftCategoryID=Guid.Parse(CT_Less100)},
                // ���ͻ���
                new AircraftType{AircraftTypeID=Guid.Parse("6D46FF0A-5282-4B47-8E48-5A04605FBDEF"),
                    Name="B777F", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_CargoBig)},
                new AircraftType{AircraftTypeID=Guid.Parse("83A17593-BDB8-42E0-8617-D82495F807E8"),
                    Name="B747-200F", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_CargoBig)},
                new AircraftType{AircraftTypeID=Guid.Parse("66B1397E-8A1C-4DB5-9DFE-03E99A284218"), 
                    Name="B747-400F",
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_CargoBig)},
                new AircraftType{AircraftTypeID=Guid.Parse("5EDF08E9-2921-4F9E-89FE-82ADC14547E1"),
                    Name="A300F", 
                    ManufacturerID=Guid.Parse(Manu_A),
                    AircraftCategoryID=Guid.Parse(CT_CargoBig)},
                new AircraftType{AircraftTypeID=Guid.Parse("6BCFE715-07C2-4E7E-AAFF-F77A80D21603"),
                    Name="A330F", 
                    ManufacturerID=Guid.Parse(Manu_A),
                    AircraftCategoryID=Guid.Parse(CT_CargoBig)},
                new AircraftType{AircraftTypeID=Guid.Parse("9E686699-D573-4D58-977D-C3AB236F542A"), 
                    Name="MD11F", 
                    ManufacturerID=Guid.Parse("DC520EC0-8A90-4B4E-A982-E321D379380A"), 
                    AircraftCategoryID=Guid.Parse(CT_CargoBig)},
                new AircraftType{AircraftTypeID=Guid.Parse("CC0522AB-12D5-4664-9A81-EA2165D1088A"), 
                    Name="B767F",
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_CargoBig)},
                // ���ͻ���
                new AircraftType{AircraftTypeID=Guid.Parse("FF0FCFA1-A88E-45BF-8B01-5427950ED36B"), 
                    Name="B757-200F", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_CargoSmall)},
                new AircraftType{AircraftTypeID=Guid.Parse("AAB69123-F6CB-4FFD-804A-09538237D96A"), 
                    Name="TU204", 
                    ManufacturerID=Guid.Parse(Manu_TU), 
                    AircraftCategoryID=Guid.Parse(CT_CargoSmall)},
                //new AircraftType{AircraftTypeID=Guid.Parse("4F02B363-6358-4932-8F77-83AAE14B7DD4"),
                //    Name="B737-300QC", 
                //    ManufacturerID=Guid.Parse(Manu_B), 
                //    AircraftCategoryID=Guid.Parse(CT_CargoSmall)},
                new AircraftType{AircraftTypeID=Guid.Parse("0B6163E7-2969-4E38-A5DA-42DF5200918E"), 
                    Name="B737F", 
                    ManufacturerID=Guid.Parse(Manu_B), 
                    AircraftCategoryID=Guid.Parse(CT_CargoSmall)},
            };
            aircraftTypes.ForEach(a => this._context.AircraftTypes.AddOrUpdate(u => u.AircraftTypeID, a));
        }

        /// <summary>
        /// ��������
        /// </summary>
        private void CreateDeliveryRisk()
        {

        }

        /// <summary>
        /// �������
        /// 1��2012��8��15�ճ�ʼ����
        /// </summary>
        private void CreateActionCategory()
        {
            var actionCategories = new List<ActionCategory>
            {
                new ActionCategory{ ActionCategoryID=Guid.Parse("8C58622E-01E3-4F61-B34D-619D3FB432AF"), 
                    ActionType="����",
                    ActionName="����", 
                    NeedRequest=true},
                new ActionCategory{ ActionCategoryID=Guid.Parse("7228CC67-572F-472B-9F0A-B7BBDCA7BEE1"), 
                    ActionType="����",
                    ActionName="��������", 
                    NeedRequest=true},
                new ActionCategory{ ActionCategoryID=Guid.Parse("4B69B41A-D528-4591-9409-7BBEBC283A0C"),
                    ActionType="����", 
                    ActionName="��Ӫ����",
                    NeedRequest=true},
                new ActionCategory{ ActionCategoryID=Guid.Parse("297BD0FE-229B-41FE-95F6-2E8FDD4FB591"), 
                    ActionType="����", 
                    ActionName="ʪ��", 
                    NeedRequest=true},
                new ActionCategory{ ActionCategoryID=Guid.Parse("EBDF8AC3-7C5C-42C4-9913-D11FDA4128AD"), 
                    ActionType="����", 
                    ActionName="��Ӫ��������", 
                    NeedRequest=true},
                new ActionCategory{ ActionCategoryID=Guid.Parse("CAA1009D-93BA-458C-AD9E-39B13884FDF0"), 
                    ActionType="����", 
                    ActionName="ʪ������", 
                    NeedRequest=true},
                new ActionCategory{ ActionCategoryID=Guid.Parse("A02C29D9-5B94-4FEF-BE75-022EA65BFAE5"),
                    ActionType="�˳�", 
                    ActionName="����"},
                new ActionCategory{ ActionCategoryID=Guid.Parse("5340C3A7-E17C-4683-99C4-45AE9D931EA0"),
                    ActionType="�˳�",
                    ActionName="����"},
                new ActionCategory{ ActionCategoryID=Guid.Parse("E765FE8C-5ED8-4466-BE63-FE64ED748378"),
                    ActionType="�˳�",
                    ActionName="����"},
                new ActionCategory{ ActionCategoryID=Guid.Parse("BC429864-AFA3-43C4-9ADD-C5CA1BD9E409"), 
                    ActionType="�˳�", 
                    ActionName="����"},
                new ActionCategory{ ActionCategoryID=Guid.Parse("3005491F-F9AB-47A8-8305-267B42C69930"), 
                    ActionType="���", 
                    ActionName="һ���װ", 
                    NeedRequest=true},
                new ActionCategory{ ActionCategoryID=Guid.Parse("8A7C3384-C6C1-47B8-A719-05BBB05523BB"), 
                    ActionType="���",
                    ActionName="�͸Ļ�", 
                    NeedRequest=true},
                new ActionCategory{ ActionCategoryID=Guid.Parse("BEDC8356-BFF7-46E4-9753-5C66F580D325"), 
                    ActionType="���", 
                    ActionName="���Ŀ�",
                    NeedRequest=true},
                new ActionCategory{ ActionCategoryID=Guid.Parse("D7C68663-7F77-4DF3-B953-76DE32035F5D"),
                    ActionType="���",
                    ActionName="�ۺ���������"},
                new ActionCategory{ ActionCategoryID=Guid.Parse("3260D2F8-5160-494B-99E3-0E4F2EDD9672"), 
                    ActionType="���",
                    ActionName="�ۺ�Ӫ����"},
                new ActionCategory{ ActionCategoryID=Guid.Parse("F8FC06A0-75B4-4FCC-A7F5-1592315E6794"),
                    ActionType="���", 
                    ActionName="��ת��"},
            };
            actionCategories.ForEach(a => this._context.ActionCategories.AddOrUpdate(u => u.ActionCategoryID, a));
        }

        /// <summary>
        /// �滮
        /// 1��2012��8��15�ճ�ʼ����
        /// </summary>
        private void CreateProgramming()
        {
            var programmings = new List<Programming>
            {
                new Programming { ProgrammingID = Guid.Parse("58F6DAA2-F09F-4991-9D04-25ED94916A75"), 
                    Name = "ʮ����滮", 
                    StartDate = new DateTime(2011, 1, 1), 
                    EndDate = new DateTime(2015, 12, 31) },
                new Programming { ProgrammingID = Guid.Parse("A408C4AF-40C4-4944-A5A7-A3FFA3F765D2"),
                    Name = "ʮ����滮", 
                    StartDate = new DateTime(2016, 1, 1), 
                    EndDate = new DateTime(2020, 12, 31) },
                new Programming { ProgrammingID = Guid.Parse("AC883FB1-6F61-4BE5-AECD-08D624FE536F"),
                    Name = "ʮ����滮", 
                    StartDate = new DateTime(2021, 1, 1),
                    EndDate = new DateTime(2025, 12, 31) },
            };
            programmings.ForEach(p => this._context.Programmings.AddOrUpdate(u => u.ProgrammingID, p));
        }

        /// <summary>
        /// ���
        /// 1��2012��8��15�ճ�ʼ����
        /// </summary>
        private void CreateAnnual()
        {
            var annuals = new List<Annual>
            {
                // ʮ����滮
                new Annual{ AnnualID=Guid.Parse("7FB161A0-03A9-469E-8760-9BB71F077CF9"), 
                    ProgrammingID=Guid.Parse("58F6DAA2-F09F-4991-9D04-25ED94916A75"),
                    Year=2011},
                new Annual{ AnnualID=Guid.Parse("C538D43C-7767-4702-9E6C-2CC6DDD7763A"),
                    ProgrammingID=Guid.Parse("58F6DAA2-F09F-4991-9D04-25ED94916A75"),
                    Year=2012,
                    IsOpen=true},
                new Annual{ AnnualID=Guid.Parse("3B33DB65-A404-4D77-9885-F259854D0FC4"), 
                    ProgrammingID=Guid.Parse("58F6DAA2-F09F-4991-9D04-25ED94916A75"),
                    Year=2013},
                new Annual{ AnnualID=Guid.Parse("CA19B813-4945-4016-8A4A-EAD22088EA67"), 
                    ProgrammingID=Guid.Parse("58F6DAA2-F09F-4991-9D04-25ED94916A75"),
                    Year=2014},
                new Annual{ AnnualID=Guid.Parse("661195B2-BCD8-4112-9F75-354636753767"),
                    ProgrammingID=Guid.Parse("58F6DAA2-F09F-4991-9D04-25ED94916A75"),
                    Year=2015},
                // ʮ����滮
                new Annual{ AnnualID=Guid.Parse("41ED96E5-6040-49BB-89BF-9124866010FB"),
                    ProgrammingID=Guid.Parse("A408C4AF-40C4-4944-A5A7-A3FFA3F765D2"),
                    Year=2016},
                new Annual{ AnnualID=Guid.Parse("601154A5-F40E-4D3A-B7AE-ABA177DA85EF"),
                    ProgrammingID=Guid.Parse("A408C4AF-40C4-4944-A5A7-A3FFA3F765D2"),
                    Year=2017},
                new Annual{ AnnualID=Guid.Parse("764F8A6E-8A62-417A-9360-17E89473CBA0"),
                    ProgrammingID=Guid.Parse("A408C4AF-40C4-4944-A5A7-A3FFA3F765D2"),
                    Year=2018},
                new Annual{ AnnualID=Guid.Parse("E025438F-6FE6-4257-9FBE-3C3761B38DCA"),
                    ProgrammingID=Guid.Parse("A408C4AF-40C4-4944-A5A7-A3FFA3F765D2"),
                    Year=2019},
                new Annual{ AnnualID=Guid.Parse("4504ED7B-31E3-4D44-9725-CE357240FCB5"), 
                    ProgrammingID=Guid.Parse("A408C4AF-40C4-4944-A5A7-A3FFA3F765D2"),
                    Year=2020},
                // ʮ����滮
                new Annual{ AnnualID=Guid.Parse("D6E690B8-CDA6-479F-A4EA-15B2D9D5DD71"),
                    ProgrammingID=Guid.Parse("AC883FB1-6F61-4BE5-AECD-08D624FE536F"),
                    Year=2021},
                new Annual{ AnnualID=Guid.Parse("10126124-EA36-4684-A93B-1FB81A586EED"),
                    ProgrammingID=Guid.Parse("AC883FB1-6F61-4BE5-AECD-08D624FE536F"),
                    Year=2022},
                new Annual{ AnnualID=Guid.Parse("F37A6748-444E-4D28-9543-1EAECDC39927"),
                    ProgrammingID=Guid.Parse("AC883FB1-6F61-4BE5-AECD-08D624FE536F"),
                    Year=2023},
                new Annual{ AnnualID=Guid.Parse("96E0DDBC-0DE7-4DB8-BE4B-4BEF4141FA7A"),
                    ProgrammingID=Guid.Parse("AC883FB1-6F61-4BE5-AECD-08D624FE536F"),
                    Year=2024},
                new Annual{ AnnualID=Guid.Parse("08ED931A-94F7-4006-B501-96E5BB609E3C"),
                    ProgrammingID=Guid.Parse("AC883FB1-6F61-4BE5-AECD-08D624FE536F"),
                    Year=2025},
            };
            annuals.ForEach(a => this._context.Annuals.AddOrUpdate(u => u.AnnualID, a));
        }

    }
}
