using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace VMusage
{
    public partial class memorybar2 : UserControl
    {
        public memorybar2()
        {
            InitializeComponent();
            _Maximum = 32;              //max number of 32MBytes in a slot
            _Minimum = 0;
            Value = 1456789/1000000;    //scale down to 1MB resolution
            Text = "Test";
            //this.Refresh();
        }

        string _text;
        public override string Text
        {
            get { return _text; }
            set { _text = value; }
        }
        public scaleModeValue scaleMode = scaleModeValue.relative;

        public enum scaleModeValue
        {
            absolute,
            relative,
        }

        int _Maximum;
        public int Maximum { 
            set {
                _Maximum = value; 
            } 
            get { return _Maximum; } 
        }
        int _Minimum;
        public int Minimum { set { _Minimum = value; } get { return _Minimum; } }

        float _value;
        public float @Value
        {
            set { 
                _value = value / 1000000;
            }    //scaling is done in MB
            get { return _value; }
        }
        //public Color backgroundColor
        //{
        //    set
        //    {
        //        BackColor = value;
        //    }
        //    get
        //    {
        //        return BackColor;
        //    }
        //}
        //public Color foregroundColor { set; get; }

        protected override void OnPaint(PaintEventArgs e)
        {

            //draw the background rectangle
            e.Graphics.FillRectangle(new SolidBrush(BackColor), 0, 0, (int)((float)(this.Width / _Maximum) * _Maximum), this.Height);
            //draw foreground rectangle
            if (scaleMode == scaleModeValue.relative)
            {
                e.Graphics.FillRectangle(new SolidBrush(ForeColor), 0, 0, (int)((float)(this.Width / _Maximum) * @Value), this.Height);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(ForeColor), 0, 0, (int)@Value, this.Height);
            }
            //draw text
            if (Text.Length > 0)
            {
                //e.Graphics.DrawString(Text, base.Font, new SolidBrush(Color.Black), new RectangleF(0f,0f,this.Width,this.Height));  
                StringFormat sf = new StringFormat();
                sf.Alignment=StringAlignment.Center;
                RectangleF rect = new RectangleF(0f, 0f, this.Width, this.Height);
                e.Graphics.DrawString(Text, base.Font, new SolidBrush(Color.Black), rect, sf);
            }

            base.OnPaint(e);
        }    
    }
}
