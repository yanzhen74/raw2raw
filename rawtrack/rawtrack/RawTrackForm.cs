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
using Poac.Common;
using System.Xml;

namespace rawtrack
{
    public partial class RawTrackForm : Form
    {
        Thread threadDoTrack = null;
        int nFileProcessed = 0;
        ulong nByteProcessed = 0;
        ulong nByteFile = 0;
        private string configFileName = "rawtrackrec.xml";

        public RawTrackForm()
        {
            InitializeComponent();
        }

        private void RawTrackForm_Load(object sender, EventArgs e)
        {
            this.textBoxOutFile.Text = "g:\\toCTCC.raw";
        }

        #region 输入数据文件列表编辑，Save/Load/增删、调整顺序
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
            ofd.Filter = "raw files (*.raw)|*.raw|dat files (*.dat)|*.dat|All files (*.*)|*.*";
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

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            XmlDocument doc = XmlHelper.LoadXmlFile(configFileName);
            XmlNode root = doc.SelectSingleNode("/Root");

            // 
            this.listBoxInFile.Items.Clear();
            foreach (XmlNode FileNode in root.SelectNodes("FileNode"))
            {
                this.listBoxInFile.Items.Add(FileNode.InnerText);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            TrackInfo trackInfo = GetTrackInfo();
            PathUtil.SafeDeleteFile(configFileName);
            XmlDocument doc = XmlHelper.LoadXmlFile(configFileName);
            XmlNode root = doc.SelectSingleNode("/Root");
            ulong intotal = 0;

            // 保存到缺省文件中
            for (int i = 0; i < this.listBoxInFile.Items.Count; i++)
            {
                XmlNode FileNode = XmlHelper.AddChild(doc, root, "FileNode", this.listBoxInFile.Items[i].ToString());
                System.IO.FileInfo fileInfo = new FileInfo(this.listBoxInFile.Items[i].ToString());
                (FileNode as XmlElement).SetAttribute("inlength", fileInfo.Length.ToString());
                intotal += (ulong)fileInfo.Length;
                int outlength = (int)(1.0 * fileInfo.Length * trackInfo.outputFrameLength / trackInfo.inputFrameLength);
                (FileNode as XmlElement).SetAttribute("outlength", outlength.ToString());
                (FileNode as XmlElement).SetAttribute("downTimeS", (outlength * 8.0 / 1024.0 / 1024.0 * trackInfo.downSpeedMbps).ToString("f1"));
            }
            XmlNode SumNode = XmlHelper.AddChild(doc, root, "SumNode", "");
            (SumNode as XmlElement).SetAttribute("inlength", intotal.ToString());
            ulong outtotal = (ulong)(1.0 * intotal * trackInfo.outputFrameLength / trackInfo.inputFrameLength);
            (SumNode as XmlElement).SetAttribute("outlength", outtotal.ToString());
            TimeSpan ts = new TimeSpan((long)(outtotal * 8.0 / 1024.0 / 1024.0 / trackInfo.downSpeedMbps) * 1000 * 1000 *10);
            (SumNode as XmlElement).SetAttribute("downTimeS", ts.ToString());

            doc.Save(configFileName);
        }
        #endregion

        #region 转换
        private void buttonGo_Click(object sender, EventArgs e)
        {
            this.timerTrack.Start();
            this.progressBarTrack.Maximum = this.listBoxInFile.Items.Count;
            this.progressBarTrack.Value = 0;

            // 线程中工作
            threadDoTrack = new Thread(new ThreadStart(DoTrack));
            threadDoTrack.IsBackground = true;
            threadDoTrack.Start();
        }

        private void DoTrack()
        {
            TrackDataFrame tdf = new TrackDataFrame();
            tdf.OutInfo = this.GetTrackInfo();
            nFileProcessed = 0;
            for (int i = 0; i < this.listBoxInFile.Items.Count; i++)
            {

                if (!DoTrackOneFile(tdf, this.listBoxInFile.Items[i].ToString()))
                {
                    return;
                }
                nFileProcessed++;
                Thread.Sleep(10);
            }
            tdf.Close();
            MessageBox.Show("well done!");
        }

        private bool DoTrackOneFile(TrackDataFrame tdf, string inFile)
        {
            if (!File.Exists(inFile))
            {// 不存在相当于处理成功
                return true;
            }

            FileStream inFileStream = null;
            BinaryReader br = null;
            try
            {
                // 打开输入文件
                inFileStream = new FileStream(inFile, FileMode.Open);
                br = new BinaryReader(inFileStream);
                nByteFile = (ulong)br.BaseStream.Length;
                nByteProcessed = 0;

                bool bret = tdf.TrackFile(br, ref nByteProcessed);

                // 关闭输入文件
                br.Close();
                br = null;
                inFileStream.Close();
                inFileStream = null;

                // 一般表示输出文件无法打开等错误
                if (!bret)
                {
                    tdf.Close();
                    MessageBox.Show("out error!");
                    return false;
                }
            }
            catch (ThreadAbortException)
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
                return false;
            }
            catch (Exception)
            {
                ;// continue;
            }
            return true;
        }


        private TrackInfo GetTrackInfo()
        {
            TrackInfo ti = new TrackInfo();
            ti.outFileName = this.textBoxOutFile.Text;
            return ti;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            this.timerTrack.Stop();
            if (threadDoTrack == null)
            {
                return;
            }
            threadDoTrack.Abort();
        }

        private void timerTrack_Tick(object sender, EventArgs e)
        {
            this.progressBarTrack.Value = this.nFileProcessed;
            double percent = (1.0 * this.nFileProcessed / this.progressBarTrack.Maximum) * 100.0;
            double percentInFile = (1.0 * this.nByteProcessed / this.nByteFile) * 100.0;

            this.labelProgress.Text = "Sum:" + percent.ToString("f2") + "%[" + this.nFileProcessed.ToString() + "/" + this.progressBarTrack.Maximum.ToString() + "] "
                + "Cur:" + percentInFile.ToString("f2") + "%[" + (this.nByteProcessed / 1024.0 / 1024.0).ToString("f2") + "/" + (this.nByteFile / 1024.0 / 1024.0).ToString("f2") + "] ";
        }
        #endregion

        #region 输出文件设置
        private void buttonSaveFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.RestoreDirectory = true;
            sfd.Filter = "raw files (*.raw)|*.raw|dat files (*.dat)|*.dat|All files (*.*)|*.*";
            sfd.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBoxOutFile.Text = sfd.FileName;
            }

        }
        #endregion
    }
}
