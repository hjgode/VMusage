using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DataAccessVM;

namespace VMusageRecvr
{
    public partial class frmMain : Form
    {
        RecvBroadcst recvr;
        Queue<VMusage.procVMinfo> dataQueue;
        DataAccess dataAccess; 
        
        bool _bAllowGUIupdate = true;
        bool bAllowGUIupdate
        {
            get { return _bAllowGUIupdate; }
            set
            {
                _bAllowGUIupdate = value;
                if (_bAllowGUIupdate)
                    recvr.StartReceive();
                else
                    recvr.StopReceive();
            }
        }
        public frmMain()
        {
            InitializeComponent();
            //the plot graph
            c2DPushGraph1.AutoAdjustPeek = true;
            c2DPushGraph1.MaxLabel = "32";
            c2DPushGraph1.MaxPeekMagnitude = 32;
            c2DPushGraph1.MinPeekMagnitude = 0;
            c2DPushGraph1.MinLabel = "0";

            dataQueue = new Queue<VMusage.procVMinfo>();

            dataAccess = new DataAccess(this.dataGridView1, ref dataQueue);
            
            recvr = new RecvBroadcst();
            recvr.onUpdate += new RecvBroadcst.delegateUpdate(recvr_onUpdate);
            recvr.onUpdateBulk += new RecvBroadcst.delegateUpdateBulk(recvr_onUpdateBulk);
            recvr.onEndOfTransfer += new RecvBroadcst.delegateEndOfTransfer(recvr_onEndOfTransfer);

            recvr.onUpdateMem += new RecvBroadcst.delegateUpdateMem(recvr_onUpdateMem);
        }

        void recvr_onUpdateMem(object sender, VMusage.MemoryInfoHelper data)
        {
            addLog(data.ToString());
        }

        void recvr_onEndOfTransfer(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("<EOT>");
            if (!bAllowGUIupdate)
                return;

            //get data
            uint iUser = dataAccess.lastTotaMemUse;
            dataAccess.lastTotaMemUse = 0;
            //DateTime dtUser = dataAccess.lastMemMeasure;
            if (iUser != 0)
                updateGraph(iUser);

            //asign the chart control
            //DataView dView = new DataView(
            //    dsData.Tables["Processes"],
            //    "ProcID=3197842474",
            //    "theTime",
            //    DataViewRowState.CurrentRows);

            //chart1.Series[0].XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Time;
            //string sFormat = chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            //chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            //chart1.Series[0].Points.DataBindXY(dView, "theTime", dView, "user");
            //            chart1.DataSource = dsData.Tables["Processes"];
        }
        delegate void updateGraphCallback(uint val);
        void updateGraph(uint val)
        {
            if (this.c2DPushGraph1.InvokeRequired)
            {
                updateGraphCallback d = new updateGraphCallback(updateGraph);
                this.Invoke(d, new object[] { val });
            }
            else
            {
                val = (uint)(val / 1000000f);
                c2DPushGraph1.MaxLabel = c2DPushGraph1.MaxPeekMagnitude.ToString();
                c2DPushGraph1.AddLine(42, Color.Red);
                c2DPushGraph1.Push((int)val, 42);
                c2DPushGraph1.UpdateGraph();
            }
        }
        public void myDispose()
        {
            dataAccess.Dispose();

            recvr.onUpdate -= recvr_onUpdate;
            recvr.StopReceive();
            recvr.Dispose();
            recvr = null;

            base.Dispose();
        }
        //################### data display etc...


        delegate void addDataCallback(VMusage.procVMinfo vmdata);
        void addData(VMusage.procVMinfo vmdata)
        {
            if (this.dataGridView1.InvokeRequired)
            {
                addDataCallback d = new addDataCallback(addData);
                this.Invoke(d, new object[] { vmdata });
            }
            else
            {
                dataGridView1.SuspendLayout();
                //enqueue data to be saved to sqlite
                dataQueue.Enqueue(vmdata);

                if (bAllowGUIupdate)
                {
                    //dataAccess.addSqlData(procStats);

                    //dtProcesses.Rows.Clear();

                    dataAccess.addData(vmdata);

                    //release queue data
                    dataAccess.waitHandle.Set();

                    //object[] o = new object[7]{ procUsage.procStatistics. .procStatistics. [i].sApp, eventEntries[i].sArg, eventEntries[i].sEvent, 
                    //        eventEntries[i].sStartTime, eventEntries[i].sEndTime, eventEntries[i].sType, eventEntries[i].sHandle };
                }
                dataGridView1.Refresh();
                dataGridView1.ResumeLayout();
            }
        }

        void recvr_onUpdateBulk(object sender, List<VMusage.procVMinfo> data)
        {
            foreach (VMusage.procVMinfo pvmi in data)
                addData(pvmi);
        }

        void recvr_onUpdate(object sender, VMusage.procVMinfo data)
        {
            //string s = data.processID.ToString() + ", " +
            //        data.sName + ", " +
            //        data.procUsage.user.ToString() + ", " +
            //        data.duration.ToString();
            ////addLog(s);

            //System.Diagnostics.Debug.WriteLine( data.dumpStatistics() );
            addData(data);
        }
        delegate void SetTextCallback(string text);
        public void addLog(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.txtLog.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(addLog);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (txtLog.Text.Length > 4000)
                    txtLog.Text = "";
                txtLog.Text += text + "\r\n";
                txtLog.SelectionLength = 0;
                txtLog.SelectionStart = txtLog.Text.Length - 1;
                txtLog.ScrollToCaret();
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            myDispose();
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            myDispose();
            System.Threading.Thread.Sleep(1000);
            Application.Exit();
        }

        private void mnuExport2CSV_Click(object sender, EventArgs e)
        {
            bAllowGUIupdate = false;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.CheckPathExists = true;
            sfd.DefaultExt = "csv";
            sfd.AddExtension = true;
            sfd.Filter = "CSV|*.csv|All|*.*";
            sfd.FilterIndex = 0;
            sfd.InitialDirectory = Environment.CurrentDirectory;
            sfd.OverwritePrompt = true;
            sfd.RestoreDirectory = true;
            sfd.ValidateNames = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //dataAccess.ExportMemUsage2CSV(sfd.FileName);
                if (MessageBox.Show("That make take a while or two! Continue?", "Export to CSV", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Application.DoEvents();
                    this.Enabled = false;
                    dataAccess.ExportMemUsage2CSV2(sfd.FileName, "");
                    Cursor.Current = Cursors.Default;
                    this.Enabled = true;
                    //DataAccess da = new DataAccess();
                    //da.export2CSV2(sfd.FileName, sIP);
                    MessageBox.Show("Export finished");
                }
            }
            bAllowGUIupdate = true;
        }

        private void mnuImport_Click(object sender, EventArgs e)
        {
            bAllowGUIupdate = false;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Filter = "CSV TAB delimitted files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.* ";
            ofd.FilterIndex = 0;
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            ofd.SupportMultiDottedExtensions = true;
            ofd.Title = "Please open a TAB delimitted CSV file to import";
            ofd.ValidateNames = true;
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                ofd.Dispose();
                return;
            }
            if (MessageBox.Show("That make take a while or two! Continue?", "Import to CSV", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();
                this.Enabled = false;
                //dataAccess.ExportMemUsage2CSV2(sfd.FileName, "");
                int iCnt = dataAccess.ImportMemUsageFromCSV(ofd.FileName);
                Cursor.Current = Cursors.Default;
                this.Enabled = true;
                //DataAccess da = new DataAccess();
                //da.export2CSV2(sfd.FileName, sIP);
                MessageBox.Show("Import/Export finished. "+iCnt.ToString() +" lines processed");
            }
            bAllowGUIupdate = true;

        }
    }
}
