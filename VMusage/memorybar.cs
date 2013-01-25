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
    public partial class memorybar : Panel
    {
        public memorybar()
        {
            InitializeComponent();
            backgroundColor = this.BackColor;
            foregroundColor = Color.LightBlue;
            _Maximum = 100;
            _Minimum = 0;
            _Value = 50;
            this.Refresh();
        }

        int _Maximum;
        public int Maximum { set { _Maximum = value; } get { return _Maximum; } }
        int _Minimum;
        public int Minimum { set { _Minimum = value; } get { return _Minimum; } }
        int _Value = 0;
        public int Value { set; get; }
        public Color backgroundColor { set; get; }
        public Color foregroundColor { set; get; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(backgroundColor), 0, 0,
                                     this.Width, this.Height);
            e.Graphics.FillRectangle(new SolidBrush(foregroundColor), 0, 0,
                                     _Value, this.Height);
            base.OnPaint(e);
        }
    }
}
