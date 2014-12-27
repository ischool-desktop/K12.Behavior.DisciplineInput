using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using FISCA.LogAgent;
using K12.Data;
using System.Text;
using System.Data;
using DevComponents.DotNetBar;
using System.Linq;
using FISCA.Presentation.Controls;

namespace K12.Behavior.DisciplineInput
{
    public partial class ApprovedForm : FISCA.Presentation.Controls.BaseForm
    {
        private string FormText = School.DefaultSchoolYear + "學年度" + School.DefaultSemester + "學期 核可教師獎懲建議";
        private BackgroundWorker _lbgw, _rbgw;
        private bool _rIsDirty = false;
        public ApprovedForm()
        {
            InitializeComponent();
            this.Text = this.FormText;
            _lbgw = new BackgroundWorker();
            _lbgw.DoWork += new DoWorkEventHandler(_lbgw_DoWork);
            _lbgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_lbgw_RunWorkerCompleted);
            _rbgw = new BackgroundWorker();
            _rbgw.DoWork += new DoWorkEventHandler(_rbgw_DoWork);
            _rbgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_rbgw_RunWorkerCompleted);
            _rbgw.WorkerSupportsCancellation = true;

            //左邊
            _lbgw.RunWorkerAsync();
            ControlEnabled = false;
            this.Text = this.FormText + "(資料讀取中)";
        }
        private bool ControlEnabled
        {
            set
            {
                dataGridViewX1.Enabled = value;
                btnModify.Enabled = value;
            }
        }
        private void _lbgw_DoWork(object sender, DoWorkEventArgs e)
        {
            //取得有尚未核可記錄班級（名稱，id）
            tool._A.Select<DisciplineRequestRecord>();
            DataTable dt = tool._Q.Select(@"select class.class_name,class.id from $discipline_request 
join student on $discipline_request.ref_student_id = student.id
join class on student.ref_class_id = class.id
where $discipline_request.status = '0'
group by class.grade_year,class.id,class.display_order,class.class_name
order by class.grade_year,class.display_order,class.class_name");
            e.Result = dt;
        }
        private void _lbgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DataTable dt = (DataTable)e.Result;
            
            foreach (DataRow row in dt.Rows)
            {
                ButtonItem bi = new ButtonItem("" + row["id"], "" + row["class_name"]);
                bi.OptionGroup = "1";
                itemPanel1.Items.Add(bi);
            }
            itemPanel1.RecalcLayout();
            this.Text = this.FormText;
            if (itemPanel1.Items.Count > 0)
            {
                _rIsDirty = false;
                itemPanel1_ItemClick(itemPanel1.Items[0], null);
                //itemPanel1.SelectedItems = new List<BaseItem>() { itemPanel1.Items[0] };
            }
            ControlEnabled = true;
        }
        private void _rbgw_DoWork(object sender, DoWorkEventArgs e)
        {
            string classId = (string)e.Argument;
            if (e.Cancel)
            {
                e.Result = null;
                return;
            }
            DataTable dt = tool._Q.Select("select id from student where ref_class_id = '" + classId + "'");
            List<string> ids = new List<string>();
            ids.Add("'0'");
            foreach (DataRow item in dt.Rows)
            {
                ids.Add("'" + item["id"] + "'");
            }
            List<DisciplineRequestRecord> drrl = tool._A.Select<DisciplineRequestRecord>("ref_student_id in ("+string.Join(",",ids)+") and status = '0'");
            e.Result = new Tuple<
                List<DisciplineRequestRecord>,
                Dictionary<string, StudentRecord>,
                Dictionary<string, TeacherRecord>
                >
                (
               drrl,
               K12.Data.Student.SelectByIDs(drrl.Select(x => "" + x.RefStudentId).Distinct()).Distinct().ToDictionary(x => x.ID, x => x),
               K12.Data.Teacher.SelectByIDs(drrl.Select(x => "" + x.RefTeacherId).Distinct()).Distinct().ToDictionary(x => x.ID, x => x)
               );
        }
        private void _rbgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled )
            {
                dataGridViewX1.Rows.Clear();
               
                Tuple<List<DisciplineRequestRecord>, Dictionary<string, StudentRecord>, Dictionary<string, TeacherRecord>> tp = (Tuple<List<DisciplineRequestRecord>, Dictionary<string, StudentRecord>, Dictionary<string, TeacherRecord>>)e.Result;
                List<DisciplineRequestRecord> drrl = tp.Item1;
                Dictionary<string, StudentRecord> dsr = tp.Item2;
                Dictionary<string, TeacherRecord> dtr = tp.Item3;
                if (drrl.Count > 0)
                {
                    #region 更新畫面資料
                    dataGridViewX1.SuspendLayout();

                    foreach (DisciplineRequestRecord item in drrl)
                    {
                        DataGridViewRow _row = new DataGridViewRow();

                        StudentRecord sr = null;
                        TeacherRecord tr = null;
                        if (dsr.ContainsKey("" + item.RefStudentId))
                            sr = dsr["" + item.RefStudentId];
                        if (dtr.ContainsKey("" + item.RefTeacherId))
                            tr = dtr["" + item.RefTeacherId];
                        _row.CreateCells(dataGridViewX1);

                        _row.Cells[0].Value = item.OccurDate.ToShortDateString(); //獎懲日期
                        _row.Cells[6].Value = item.DisciplineString; //獎懲次數
                        _row.Cells[7].Value = item.Reason; //事由

                        if (sr != null && sr.Class != null)
                            _row.Cells[2].Value = sr.Class.Name; //班級
                        _row.Cells[3].Value = sr.SeatNo; //座號
                        _row.Cells[4].Value = sr.StudentNumber; //學號
                        _row.Cells[5].Value = sr.Name; //姓名

                        if (tr != null)
                            _row.Cells[1].Value = tr.Name + (!string.IsNullOrWhiteSpace(tr.Nickname) ? "(" + tr.Nickname + ")" : "");//登錄人員
                        _row.Tag = item;

                        _row.Cells[8].Value = "";
                        _row.Cells[9].Value = false;
                        dataGridViewX1.Rows.Add(_row);
                    }
                    dataGridViewX1.ResumeLayout();
                    #endregion
                    _rIsDirty = false;
                    if (dataGridViewX1.Rows.Count > 0)
                        dataGridViewX1.Rows[0].Selected = false;
                }
                else
                {
                    dataGridViewX1.Rows.Clear();
                    FISCA.Presentation.Controls.MsgBox.Show("查無獎懲資料!");
                    return;
                }
            }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //排序
        private int SortByClassAndSeatNo(DisciplineRecord attendX, DisciplineRecord attendy)
        {
            StudentRecord x = attendX.Student;
            StudentRecord y = attendy.Student;
            string 班級名稱1 = (x.Class == null ? "" : x.Class.Name) + "::";
            string 座號1 = (x.SeatNo.HasValue ? x.SeatNo.Value.ToString().PadLeft(2, '0') : "") + "::";
            string 班級名稱2 = (y.Class == null ? "" : y.Class.Name) + "::";
            string 座號2 = (y.SeatNo.HasValue ? y.SeatNo.Value.ToString().PadLeft(2, '0') : "") + "::";
            string 日期1 = attendX.OccurDate.ToShortDateString();
            string 日期2 = attendy.OccurDate.ToShortDateString();
            班級名稱1 += 座號1;
            班級名稱1 += 日期1;

            班級名稱2 += 座號2;
            班級名稱2 += 日期2;

            return 班級名稱1.CompareTo(班級名稱2);
        }

        private void itemPanel1_ItemClick(object sender, EventArgs e)
        {
            if (_rIsDirty && MsgBox.Show("尚未儲存，是否確認要變更班級?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
                return;

            BaseItem bi = (BaseItem)sender;
            if (_rbgw.IsBusy)
                _rbgw.CancelAsync();

            if (!_rbgw.IsBusy)
            {
                _rbgw.RunWorkerAsync(bi.Name);
                labelX1.Text = bi.Text;
            }
        }
        //批次核可
        private void 批次增加前置詞ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewX1.SelectedRows)
            {
                item.Cells[9].Value = true;
            }
        }
        //批次不核可
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewX1.SelectedRows)
            {
                item.Cells[9].Value = false;
            }
        }
        //批次調整審查回覆
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SimpleTextForm stf = new SimpleTextForm();
            stf.ShowDialog();
            if (stf.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                string tmp = stf.ChangedText;
                foreach (DataGridViewRow item in dataGridViewX1.SelectedRows)
                {
                    item.Cells[8].Value = tmp ;
                }
            }
        }
        //核可按鈕
        private void btnModify_Click(object sender, EventArgs e)
        {
            int approvedCount = 0, notApprovedCount = 0;

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if ((bool)row.Cells[colApproved.Index].Value == true)
                    approvedCount++;
                else if ((bool)row.Cells[colApproved.Index].Value == false)
                    notApprovedCount++;
            }
            List<string> msg = new List<string>();
            msg.Add("請確認是否儲存以下資料：");
            msg.Add("核可　：\t" + approvedCount + " 筆");
            msg.Add("不核可：\t" + notApprovedCount + " 筆");
            msg.Add("共　　：\t" + dataGridViewX1.Rows.Count + " 筆");
            if (dataGridViewX1.Rows.Count == 0 || MsgBox.Show(string.Join("\n",msg), MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
                return;
            List<DisciplineRecord> drl = new List<DisciplineRecord>();
            List<DisciplineRequestRecord> drrl = new List<DisciplineRequestRecord>();
            List<string> log = new List<string>();
            log.Add("學生編號,日期,理由,登錄日期,學年度,學期,類別,大功數,小功數,獎勵數,大過數,小過數,警告數");
            List<string> log2 = new List<string>();
            log2.Add("學生編號,老師編號,日期,理由,類別,大功數,小功數,獎勵數,大過數,小過數,警告數,審查回覆");
            foreach (DataGridViewRow item in dataGridViewX1.Rows)
            {
                DisciplineRequestRecord drr = item.Tag as DisciplineRequestRecord;

                drr.ReturnMessage = "" + item.Cells[8].Value;

                if ((bool)item.Cells[9].Value == true)
                {
                    drr.Status = "1";
                    DisciplineRecord dr = new DisciplineRecord();
                    dr.MeritFlag = "" + drr.MeritFlag;
                    dr.OccurDate = drr.OccurDate;
                    dr.Reason = drr.Reason;
                    dr.RefStudentID = "" + drr.RefStudentId;
                    dr.RegisterDate = DateTime.Now;
                    dr.SchoolYear = int.Parse(School.DefaultSchoolYear);
                    dr.Semester = int.Parse(School.DefaultSemester);
                    dr.MeritA = drr.MeritA;
                    dr.MeritB = drr.MeritB;
                    dr.MeritC = drr.MeritC;
                    dr.DemeritA = drr.DemeritA;
                    dr.DemeritB = drr.DemeritB;
                    dr.DemeritC = drr.DemeritC;
                    drl.Add(dr);
                    log.Add(dr.RefStudentID + "," + dr.OccurDate + "," + dr.Reason + "," + dr.RegisterDate + "," + dr.SchoolYear + "," + dr.Semester + "," + dr.MeritFlag + "," + dr.MeritA + "," + dr.MeritB + "," + dr.MeritC + "," + dr.DemeritA + "," + dr.DemeritB + "," + dr.DemeritC);
                }
                else
                    drr.Status = "2";
                log2.Add(drr.RefStudentId + "," + drr.RefTeacherId + "," + drr.OccurDate + "," + drr.Reason + "," + drr.MeritFlag + "," + drr.MeritA + "," + drr.MeritB + "," + drr.MeritC + "," + drr.DemeritA + "," + drr.DemeritB + "," + drr.DemeritC + "," + drr.ReturnMessage);
                drrl.Add(drr);
            }
            if (drl.Count > 0)
                ApplicationLog.Log("核可教師獎懲建議", "新增", "新增獎懲記錄共" + drl.Count + "筆\n明細：\n" + string.Join("\n", log));
            ApplicationLog.Log("核可教師獎懲建議", "核可", "核可教師獎懲建議共" + drrl.Count + "筆\n明細：\n" + string.Join("\n", log2));
            K12.Data.Discipline.Insert(drl);
            tool._A.SaveAll(drrl);

            labelX1.Text = "";
            _rIsDirty = false;
            itemPanel1.Items.Clear();
            dataGridViewX1.Rows.Clear();
            _lbgw.RunWorkerAsync();
        }
        private void colApproved_Click(object sender, EventArgs e)
        {
            _rIsDirty = true;
        }
        private void dataGridViewX1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            _rIsDirty = true;
        }
    }
}