using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VMusage
{
    public partial class Form1 : Form
    {
        vmInfoThread vmiThread;
        List<VMusage.procVMinfo> vmInfos;
        memorystatus.MemoryInfo.MEMORYSTATUS memInfoStatus;

        //panel to hold all memorybars
        Panel mainPanel;
        memorybar2[] panels;
        System.Drawing.Color myGreen = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));

        Timer hideMeTimer;
        public Form1(int iTimeout, bool bHide):this(iTimeout)
        {
            if (bHide)
            {
                //hide me
                hideMeTimer = new Timer();
                hideMeTimer.Interval = 1000;
                hideMeTimer.Tick += new EventHandler(hideMeTimer_Tick);
                hideMeTimer.Enabled = true;
            }
        }

        void hideMeTimer_Tick(object sender, EventArgs e)
        {
            hideMeTimer.Enabled = false;
            System.Process.Process.Hide(this);
        }

        public Form1(int iTimeout)
        {
            InitializeComponent();
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;

            this.tabControl1.TabPages[0].Controls.Add(mainPanel);
            panels = new memorybar2[33];    //we have slots 1 to 32 plus one total bar

            //VMhelper vmh = new VMhelper();
            //vmh.test();
            //vmh.ShowMemory();
            VMusage.CeGetProcVMusage vmInfo = new CeGetProcVMusage();
            vmInfos = vmInfo._procVMinfo;

            createPanels();
            
            foreach (VMusage.procVMinfo vm in vmInfos)
            {
                textBox1.Text += vm.ToString()+"\r\n";
                updateBar((int)vm.slot, vm.name, (int)vm.memusage);
            }

            //memorystatus.MemoryInfo.GlobalMemoryStatus(ref memInfoStatus);
            //updateBar(0, "total " + memInfoStatus.dwAvailVirtual / 1000000, (int)(memInfoStatus.dwTotalVirtual));
            updateTotalMemBar();

            //start the background tasks
            vmiThread = new vmInfoThread();
            vmiThread._iTimeOut = iTimeout*1000;
            vmiThread.updateEvent += new vmInfoThread.updateEventHandler(vmiThread_updateEvent);
            
        }
        /// <summary>
        /// default class uses 3 seconds timeout
        /// </summary>
        public Form1():this(3000)
        {
        }
        void createPanels()
        {
            mainPanel.Controls.Clear();
            int margin = 2;
            int capHeight = SystemInformation.MenuHeight + SystemInformation.BorderSize.Width;
            int w = mainPanel.ClientSize.Width - 32;  //subtract the scrollbar on right
            int h = mainPanel.ClientSize.Height - capHeight;
            int wP = w - (2 * margin); ;
            int hP = 32;// h / (panels.Length + 1 + margin);//need 32 panels with margin
            VMusage.procVMinfo [] vmInfoA = vmInfos.ToArray();

            //use bar 0 for total
            int total = 0;
            panels[0] = new memorybar2();
            panels[0].BackColor = myGreen;
            panels[0].ForeColor = Color.Red;
            panels[0].BorderStyle = BorderStyle.FixedSingle;
            panels[0].Bounds = new Rectangle (margin, (margin + hP) * 0, wP, hP);

            //vmInfo is for slot 1 to 32
            int idx = 0;
            for (int i = 1; i < panels.Length; i++)
            {
                panels[i] = new memorybar2();
                panels[i].BackColor = myGreen;// System.Drawing.Color.LightGreen;
                panels[i].ForeColor = Color.Red;
                panels[i].BorderStyle = BorderStyle.FixedSingle;
                panels[i].Bounds = new Rectangle (margin, (margin + hP) * i, wP, hP);
                idx = i - 1;
                panels[i].Text = vmInfoA[idx].slot.ToString() + ":" + vmInfoA[idx].name;
                panels[i].Value = vmInfoA[idx].memusage;
                total += (int)vmInfoA[idx].memusage;
                this.mainPanel.Controls.Add(panels[i]);
            }

            //the max for all slot panels is 32MB, but bar 0 shows the total values
            //and the max val (493000000) can exceed the panels width (ie 444)
            int maxWidth = (int)memorystatus.MemoryInfo.getTotalPhys() / 1000000;
            int scaleFactor = 0;
            while (maxWidth > panels[0].Width)
            {  //we need to scale this down or we get 0 result
                //System.Diagnostics.Debugger.Break();
                maxWidth = (int)((float)(maxWidth / 10f));
                scaleFactor++;
            }

            panels[0].Maximum = maxWidth;
            if(scaleFactor>0)
                panels[0].Value = (int)(memorystatus.MemoryInfo.getAvailPhys()/(scaleFactor*10));
            else
                panels[0].Value = (int)(memorystatus.MemoryInfo.getAvailPhys());
            panels[0].Text = "total";
            this.mainPanel.Controls.Add(panels[0]);
        }
        delegate void updateBarDelegate(int idx, string sName, int newValue);

        /// <summary>
        /// update a bar display
        /// </summary>
        /// <param name="idx">which bar to update</param>
        /// <param name="sName">text to display</param>
        /// <param name="newValue">new value (divided by 1000000! for MB)</param>
        void updateBar(int idx, string sName, int newValue)
        {
            if (this.InvokeRequired)
            {
                updateBarDelegate d = new updateBarDelegate(updateBar);
                this.Invoke(d, new object[] { idx, sName, newValue });
            }
            else
            {
                if (idx > panels.Length - 1)
                    return;
                if (idx != 0)
                {
                    panels[idx].Text = "slot" + idx.ToString() + ": " + sName;
                }
                else
                    panels[idx].Text = sName;

                panels[idx].Value = newValue;
                panels[idx].Refresh();
            }
        }

        void updateTotalMemBar()
        {
            memorystatus.MemoryInfo.GlobalMemoryStatus(ref memInfoStatus);

            //max is 32!
            uint uTotal = memInfoStatus.dwTotalPhys;         //is scaled by 1000000 in updateBar!
            uint uAvail = memInfoStatus.dwAvailPhys; //scale by 1000000

            int maxWidth = (int)uTotal / 1000000;
            int scaleFactor = 0;
            while (maxWidth > panels[0].Width)
            {  //we need to scale this down or we get 0 result
                //System.Diagnostics.Debugger.Break();
                maxWidth = (int)((float)(maxWidth / 10f));
                scaleFactor++;
            }
            panels[0].Maximum = maxWidth;

            uint newVal = uAvail;
            if (scaleFactor > 0)
                newVal = (uint)(uAvail / (scaleFactor * 10));

            updateBar(0, "total " + uAvail / 1000000 + "/" + uTotal / 1000000, (int)newVal);

        }

        void vmiThread_updateEvent(object sender, procVMinfoEventArgs eventArgs)
        {
            StringBuilder sb = new StringBuilder();
            //keep track of updated panels and clear empty ones!
            bool[] bBarEmpty = new bool[33];
            for (int i=0; i < 33; i++)
                bBarEmpty[i] = true;
            foreach (VMusage.procVMinfo vm in eventArgs.procVMlist)
            {
                if (!vm.name.EndsWith("empty"))
                {
                    sb.Append(vm.ToString() + "\r\n");
                    updateBar((int)vm.slot, vm.name, (int)vm.memusage);
                    bBarEmpty[(int)vm.slot] = false;
                }
            }

            for (int i = 1; i < 33; i++){   //do not touch bar 0
                if (bBarEmpty[i])
                    updateBar(i, "", 0);
            }
            setText(sb.ToString());
            
            updateTotalMemBar();

            //int mPhys = (int)memorystatus.MemoryInfo.getTotalPhys() / 1000000;
            //setTitle("total=" + eventArgs.totalMemoryInUse.ToString() + "/" + mPhys.ToString());
            //updateBar(0, "total", (int)(eventArgs.totalMemoryInUse / 1000000));
        }

        delegate void setTitleCallback(string text);
        public void setTitle(string text)
        {
            if (this.InvokeRequired)
            {
                setTitleCallback d = new setTitleCallback(setTitle);
                this.Invoke(d, new object[] { text });
            }
            else
                this.Text = text;
        }
        delegate void SetTextCallback(string text);
        public void setText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                textBox1.Text = text;
                textBox1.Refresh();
            }
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Exit?", "VMusage 1.1", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
            vmiThread.Dispose();
            System.Threading.Thread.Sleep(500);
        }
    }
}