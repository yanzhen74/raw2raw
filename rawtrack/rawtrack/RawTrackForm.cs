using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using rawtrack.TrackData;
using System.Threading;
using System.IO;

namespace rawtrack
{
    public partial class RawTrackForm : Form
    {
        Thread threadDoTrack = null;
        int nFileProcessed = 0;

        public RawTrackForm()
        {
            InitializeComponent();
        }

        private void RawTrackForm_Load(object sender, EventArgs e)
        {
            this.textBoxOutFile.Text = "e:\\out.dat";
        }

        #region 增删、调整顺序
        private void buttonDel_Click(object sender, EventArgs e)
        {
            for (int n = listBoxInFile.SelectedItems.Count - 1; n >= 0; n--)
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
                        if (i < this.listBoxInFile.Items.Count - 1)
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

        private void buttonGo_Click(object sender, EventArgs e)
        {
            this.timerTrack.Start();
            this.progressBarTrack.Maximum = this.listBoxInFile.Items.Count * 10;
            this.progressBarTrack.Value = 0;
            threadDoTrack = new Thread(new ThreadStart(DoTrack));
            threadDoTrack.IsBackground = true;
            threadDoTrack.Start();

            //MessageBox.Show("well done!");
        }

        private void DoTrack()
        {
            TrackDataFrame tdf = new TrackDataFrame();
            tdf.OutInfo = this.GetTrackInfo();
            nFileProcessed = 0;
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < this.listBoxInFile.Items.Count; i++)
                {
                    if (!File.Exists(this.listBoxInFile.Items[i].ToString()))
                    {
                        continue;
                    }

                    FileStream inFileStream = null;
                    BinaryReader br = null;
                    try
                    {
                        // 打开输入文件
                        inFileStream = new FileStream(this.listBoxInFile.Items[i].ToString(), FileMode.Open);
                        br = new BinaryReader(inFileStream);

                        tdf.TrackFile(br);
                        br.Close();
                        br = null;
                        inFileStream.Close();
                        inFileStream = null;
                        nFileProcessed++;
                    }
                    catch (ThreadAbortException e)
                    {
                        tdf.Close();
                        if (br != null)
                        {
                            br.Close();
                        }
                        if (inFileStream != null)
                        {
                            inFileStream.Close();
                        }
                        return;
                    }
                    Thread.Sleep(300);
                }
            }
            tdf.Close();
            MessageBox.Show("well done!");
        }


        private TrackInfo GetTrackInfo()
        {
            TrackInfo ti = new TrackInfo();
            ti.outFileName = this.textBoxOutFile.Text;
            return ti;
        }

        private void buttonSaveFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.RestoreDirectory = true;
            sfd.Filter = "dat files (*.dat)|*.dat|All files (*.*)|*.*";
            sfd.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBoxOutFile.Text = sfd.FileName;
            }

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (threadDoTrack == null)
            {
                return;
            }
            this.timerTrack.Stop();
            threadDoTrack.Abort();
        }

        private void timerTrack_Tick(object sender, EventArgs e)
        {
            this.progressBarTrack.Value = this.nFileProcessed;
            double percent = (1.0 * this.nFileProcessed / this.progressBarTrack.Maximum) * 100.0;
            this.labelProgress.Text = percent.ToString("f2") + "%";
        }
    }
}
