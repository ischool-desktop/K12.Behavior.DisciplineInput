using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using FISCA.LogAgent;
using K12.Data;

namespace K12.Behavior.DisciplineInput
{
    public partial class SimpleTextForm : BaseForm
    {
        public string ChangedText = "";

        public SimpleTextForm()
        {
            InitializeComponent();
            this.Text = "批次調整審查回覆";
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.ChangedText = textBoxX1.Text.Trim();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
