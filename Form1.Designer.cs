namespace NovoCommunication
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.MonitorTimer = new System.Windows.Forms.Timer(this.components);
            this.QueueBox = new System.Windows.Forms.ComboBox();
            this.PpxBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // MonitorTimer
            // 
            this.MonitorTimer.Enabled = true;
            this.MonitorTimer.Tick += new System.EventHandler(this.MonitorTimer_Tick);
            // 
            // QueueBox
            // 
            this.QueueBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.QueueBox.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "8",
            "16",
            "32",
            "64",
            "128"});
            this.QueueBox.Location = new System.Drawing.Point(130, 103);
            this.QueueBox.Name = "QueueBox";
            this.QueueBox.Size = new System.Drawing.Size(77, 20);
            this.QueueBox.TabIndex = 6;
            // 
            // PpxBox
            // 
            this.PpxBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PpxBox.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "8",
            "16",
            "32",
            "64",
            "128",
            "256",
            "512"});
            this.PpxBox.Location = new System.Drawing.Point(130, 60);
            this.PpxBox.Name = "PpxBox";
            this.PpxBox.Size = new System.Drawing.Size(77, 20);
            this.PpxBox.TabIndex = 4;
            this.PpxBox.SelectedIndexChanged += new System.EventHandler(this.PpxBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 18);
            this.label2.TabIndex = 5;
            this.label2.Text = "Xfers to Queue";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Packets per Xfer";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 450);
            this.Controls.Add(this.QueueBox);
            this.Controls.Add(this.PpxBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer MonitorTimer;
        private System.Windows.Forms.ComboBox QueueBox;
        private System.Windows.Forms.ComboBox PpxBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}

