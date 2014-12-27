using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Behavior.DisciplineInput
{
    class Permissions
    {
        public static string 教師獎懲登錄設定 { get { return "K12.Behavior.DisciplineInput.Config.cs"; } }
        public static bool 教師獎懲登錄設定權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[教師獎懲登錄設定].Executable;
            }
        }
        public static string 核可教師獎懲建議 { get { return "K12.Sports.FitnessInput.Edit.cs"; } }
        public static bool 核可教師獎懲建議權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[核可教師獎懲建議].Executable;
            }
        }
    }
}
