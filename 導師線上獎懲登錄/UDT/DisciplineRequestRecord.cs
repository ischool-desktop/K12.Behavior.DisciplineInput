using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using System.Xml.Linq;

namespace K12.Behavior.DisciplineInput
{
    [TableName("discipline_request")]
    public class DisciplineRequestRecord : ActiveRecord
    {
        [Field(Field = "detail", Indexed = false)]
        public string Detail { get; set; }

        [Field(Field = "merit_flag", Indexed = false)]
        public int MeritFlag { get; set; }
        [Field(Field = "occur_date", Indexed = false)]
        public DateTime OccurDate { get; set; }
        [Field(Field = "occur_place", Indexed = false)]
        public string OccurPlace { get; set; }

        [Field(Field = "reason", Indexed = false)]
        public string Reason { get; set; }

        [Field(Field = "ref_student_id", Indexed = false)]
        public int RefStudentId { get; set; }

        [Field(Field = "ref_teacher_id", Indexed = false)]
        public int RefTeacherId { get; set; }

        [Field(Field = "register_date", Indexed = false)]
        public DateTime RegisterDate { get; set; }

        [Field(Field = "return_message", Indexed = false)]
        public string ReturnMessage { get; set; }

        [Field(Field = "status", Indexed = false)]
        //0 : 未核可，1：可，2：不可
        public string Status { get; set; }

        private bool isParsed = false;
        public int MeritA;
        public int MeritB;
        public int MeritC;
        public int DemeritA;
        public int DemeritB;
        public int DemeritC;
        private void parse()
        { 

            if (!string.IsNullOrWhiteSpace(Detail))
            {
                XElement xe = XElement.Parse(Detail);
                if (xe.Element("Merit") != null)
                {
                     int.TryParse(xe.Element("Merit").Attribute("A").Value,out this.MeritA);
                     int.TryParse(xe.Element("Merit").Attribute("B").Value,out this.MeritB);
                     int.TryParse(xe.Element("Merit").Attribute("C").Value,out this.MeritC);
                }
                if (xe.Element("Demerit") != null)
                {
                    int.TryParse(xe.Element("Demerit").Attribute("A").Value, out this.DemeritA);
                    int.TryParse(xe.Element("Demerit").Attribute("B").Value, out this.DemeritB);
                    int.TryParse(xe.Element("Demerit").Attribute("C").Value, out this.DemeritC);
                }
            }
        }
        public string DisciplineString
        {
            get
            {
                if (!this.isParsed)
                    this.parse();
                string result = "";
                if (this.MeritFlag  == 1)
                {
                    if (this.MeritA > 0)
                        result += string.Format("大功:{0}", this.MeritA);
                    if (this.MeritB > 0)
                        result += string.Format("小功:{0}", this.MeritB);
                    if (this.MeritC > 0)
                        result += string.Format("嘉獎:{0}", this.MeritC);
                }
                else if (this.MeritFlag == 0)
                {
                    if (this.DemeritA > 0)
                        result += string.Format("大過:{0}", this.DemeritA);
                    if (this.DemeritB > 0)
                        result += string.Format("小過:{0}", this.DemeritB);
                    if (this.DemeritC > 0)
                        result += string.Format("警告:{0}", this.DemeritC);
                }
                return result;
            }
        }
    }
}
