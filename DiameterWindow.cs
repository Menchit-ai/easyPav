using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace easyPav
{
    public class PavingForm : System.Windows.Forms.Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public PavingForm()
        {
            InitializeComponent();
            // Set Rhino as the owner of this window
            var rhinoMainWindowHandle = Rhino.RhinoApp.MainWindowHandle();
            SetParent(this.Handle, rhinoMainWindowHandle);
        }


        private BindingSource diameterWindowBindingSource;
        public NumericUpDownFix diameter_value;
        private Label diameter_label;
        private Label label1;
        private System.ComponentModel.IContainer components;

        public double get_diameter()
        {
            return (double)this.diameter_value.Value;
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            diameterWindowBindingSource = new BindingSource(components);
            diameter_value = new NumericUpDownFix();
            diameter_label = new Label();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)diameterWindowBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)diameter_value).BeginInit();
            SuspendLayout();
            // 
            // diameterWindowBindingSource
            // 
            diameterWindowBindingSource.DataSource = typeof(Form);
            // 
            // diameter_value
            // 
            diameter_value.DecimalPlaces = 2;
            diameter_value.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            diameter_value.Location = new System.Drawing.Point(72, 18);
            diameter_value.Name = "diameter_value";
            diameter_value.Size = new System.Drawing.Size(96, 23);
            diameter_value.TabIndex = 0;
            diameter_value.Value = new decimal(new int[] { 1, 0, 0, 0 });
            diameter_value.ValueChanged += diameter_value_ValueChanged;
            // 
            // diameter_label
            // 
            diameter_label.AutoSize = true;
            diameter_label.BackColor = System.Drawing.SystemColors.Window;
            diameter_label.Location = new System.Drawing.Point(12, 20);
            diameter_label.Name = "diameter_label";
            diameter_label.Size = new System.Drawing.Size(54, 15);
            diameter_label.TabIndex = 2;
            diameter_label.Text = "diameter";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 46);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(163, 15);
            label1.TabIndex = 3;
            label1.Text = "Afficher diamètres des pierres";
            label1.Click += label1_Click;
            // 
            // PavingForm
            // 
            AutoSize = true;
            ClientSize = new System.Drawing.Size(226, 294);
            Controls.Add(label1);
            Controls.Add(diameter_label);
            Controls.Add(diameter_value);
            Name = "PavingForm";
            Text = "EzPav";
            TopMost = true;
            Load += DiameterWindow_Load;
            ((System.ComponentModel.ISupportInitialize)diameterWindowBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)diameter_value).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void decrement_diameter(object sender, EventArgs e)
        {
            this.diameter_value.Value -= this.diameter_value.Increment;
        }

        private void increment_diameter(object sender, EventArgs e)
        {
            this.diameter_value.Value += this.diameter_value.Increment;
        }

        private void diameter_value_ValueChanged(object sender, EventArgs e)
        {
            if (this.diameter_value.Value > this.diameter_value.Maximum)
            {
                this.diameter_value.Value = this.diameter_value.Maximum;
            }
            if (this.diameter_value.Value < this.diameter_value.Minimum)
            {
                this.diameter_value.Value = this.diameter_value.Minimum;
            }
        }

        private void DiameterWindow_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

public class NumericUpDownFix : System.Windows.Forms.NumericUpDown
{
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        HandledMouseEventArgs hme = e as HandledMouseEventArgs;
        if (hme != null)
            hme.Handled = true;

        if (e.Delta > 0)
            this.Value += this.Increment;
        else if ((e.Delta < 0) && (this.Value - this.Increment >= 0))
            this.Value -= this.Increment;
    }
}