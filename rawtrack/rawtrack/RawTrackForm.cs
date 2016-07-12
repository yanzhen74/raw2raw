using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace rawtrack
{
    public partial class RawTrackForm : Form
    {
        public RawTrackForm()
        {
            InitializeComponent();
        }

        private void RawTrackForm_Load(object sender, EventArgs e)
        {
            this.listBoxInFile.Items.Add("this is one test");
            this.listBoxInFile.Items.Add("this is two test");
        }

        #region 增删、调整顺序
        private void buttonDel_Click(object sender, EventArgs e)
        {
            for (int n = listBoxInFile.SelectedItems.Count-1; n >= 0; n--)
            {
                this.listBoxInFile.Items.Remove(this.listBoxInFile.SelectedItems[n]);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.RestoreDirectory = true;
            ofd.Filter = "dat files (*.dat)|*.dat|All files (*.*)|*.*";
            ofd.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (this.listBoxInFile.Items.Contains(file))
                        continue;
                    this.listBoxInFile.Items.Add(file);
                }
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (this.listBoxInFile.SelectedItems.Count > 0)
            {
                for (int i = 0; i < this.listBoxInFile.Items.Count; i++)
                {
                    if (this.listBoxInFile.Items[i] == this.listBoxInFile.SelectedItems[0])
                    {
                        if (i > 0)
                        {
                            object tmp = this.listBoxInFile.Items[i].ToString().Clone();
                            this.listBoxInFile.Items[i] = this.listBoxInFile.Items[i - 1];
                            this.listBoxInFile.Items[i - 1] = tmp;
                            this.listBoxInFile.SetSelected(i - 1, true);
                            this.listBoxInFile.SetSelected(i, false);
                        }
                        break;
                    }
                }
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (this.listBoxInFile.SelectedItems.Count > 0)
            {
                for (int i = 0; i < this.listBoxInFile.Items.Count; i++)
                {
                    if (this.listBoxInFile.Items[i] == this.listBoxInFile.SelectedItems[0])
                    {
                        if (i < this.listBoxInFile.Items.Count-1)
                        {
                            object tmp = this.listBoxInFile.Items[i].ToString().Clone();
                            this.listBoxInFile.Items[i] = this.listBoxInFile.Items[i + 1];
                            this.listBoxInFile.Items[i + 1] = tmp;
                            this.listBoxInFile.SetSelected(i + 1, true);
                            this.listBoxInFile.SetSelected(i, false);
                        }
                        break;
                    }
                }
            }

        }
        #endregion
    }
}
