using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA;
using FISCA.Presentation;
using FISCA.Permission;

namespace K12.Behavior.DisciplineInput
{
    public class Program
    {
        [MainMethod()]
        static public void Main()
        {
            FISCA.ServerModule.AutoManaged("http://module.ischool.com.tw/module/193005/K12.Behavior.DisciplineInput/udm.xml");


            RibbonBarItem item = MotherForm.RibbonBarItems["學務作業", "線上作業"];
            item["設定"]["教師獎懲登錄設定"].Enable = Permissions.教師獎懲登錄設定權限;
            item["設定"].Image = Properties.Resources.設定;
            item["設定"]["教師獎懲登錄設定"].Click += delegate
            {
                new InputDateSettingForm().ShowDialog();
            };
            Catalog detail1 = RoleAclSource.Instance["學務作業"];
            detail1.Add(new RibbonFeature(Permissions.教師獎懲登錄設定, "教師獎懲登錄設定"));


            item = MotherForm.RibbonBarItems["學務作業", "線上作業"];
            item["核可教師獎懲建議"].Image = Properties.Resources.核可教師獎懲建議;
            item["核可教師獎懲建議"].Size = RibbonBarButton.MenuButtonSize.Medium;
            item["核可教師獎懲建議"].Enable = Permissions.核可教師獎懲建議權限;
            item["核可教師獎懲建議"].Click += delegate
            {
                    new ApprovedForm().ShowDialog();
            };
            detail1 = RoleAclSource.Instance["學務作業"];
            detail1.Add(new RibbonFeature(Permissions.核可教師獎懲建議, "核可教師獎懲建議"));
        }
    }
}
