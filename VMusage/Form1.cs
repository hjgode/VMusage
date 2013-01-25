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

        //panel to hold all memorybars
        Panel mainPanel;
        memorybar2[] panels;
        System.Drawing.Color myGreen = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));

        public Form1()
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

            //start the background tasks
            vmiThread = new vmInfoThread();
            vmiThread.updateEvent += new vmInfoThread.updateEventHandler(vmiThread_updateEvent);
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
            panels[0].Maximum = (int)memorystatus.MemoryInfo.getTotalPhys()/1000000;
            panels[0].Value = total;
            panels[0].Text = "total";
            this.mainPanel.Controls.Add(panels[0]);
        }
        delegate void updateBarDelegate(int idx, string sName, int newValue);
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
        void vmiThread_updateEvent(object sender, procVMinfoEventArgs eventArgs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (VMusage.procVMinfo vm in eventArgs.procVMlist)
            {
                sb.Append (vm.ToString()+"\r\n");
                updateBar((int)vm.slot, vm.name, (int)vm.memusage);
            }
            setText(sb.ToString());
            int mPhys = (int)memorystatus.MemoryInfo.getTotalPhys() / 1000000;
            setTitle("total=" + eventArgs.totalMemoryInUse.ToString() + "/" + mPhys.ToString());
            updateBar(0, "total", (int)(eventArgs.totalMemoryInUse / 1000000));
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
            vmiThread.Dispose();
            System.Threading.Thread.Sleep(500);
        }
    }
}