namespace iottree_client_demo
{
    partial class FormDemo
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnStartPump = new Button();
            label1 = new Label();
            panelLvlC = new Panel();
            panLvl = new Panel();
            labelLvl = new Label();
            btnStopPump = new Button();
            tbUrl = new TextBox();
            labelErr = new Label();
            label3 = new Label();
            btnStartClient = new Button();
            btnStopClient = new Button();
            tbClientId = new TextBox();
            label4 = new Label();
            tbTagPaths = new TextBox();
            label5 = new Label();
            tbTagVals = new TextBox();
            labelInf = new Label();
            labelRunST = new Label();
            panelRunST = new Panel();
            SuspendLayout();
            // 
            // btnStartPump
            // 
            btnStartPump.Location = new Point(345, 469);
            btnStartPump.Name = "btnStartPump";
            btnStartPump.Size = new Size(142, 38);
            btnStartPump.TabIndex = 0;
            btnStartPump.Text = "Start Pump";
            btnStartPump.UseVisualStyleBackColor = true;
            btnStartPump.Click += btnStartPump_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(73, 286);
            label1.Name = "label1";
            label1.Size = new Size(76, 17);
            label1.TabIndex = 1;
            label1.Text = "Water Level";
            // 
            // panelLvlC
            // 
            panelLvlC.BackColor = Color.Black;
            panelLvlC.Location = new Point(238, 259);
            panelLvlC.Name = "panelLvlC";
            panelLvlC.Size = new Size(35, 200);
            panelLvlC.TabIndex = 2;
            // 
            // panLvl
            // 
            panLvl.BackColor = Color.FromArgb(0, 192, 192);
            panLvl.Location = new Point(238, 359);
            panLvl.Name = "panLvl";
            panLvl.Size = new Size(35, 100);
            panLvl.TabIndex = 3;
            // 
            // labelLvl
            // 
            labelLvl.AutoSize = true;
            labelLvl.Location = new Point(155, 286);
            labelLvl.Name = "labelLvl";
            labelLvl.Size = new Size(32, 17);
            labelLvl.TabIndex = 4;
            labelLvl.Text = "###";
            // 
            // btnStopPump
            // 
            btnStopPump.Location = new Point(519, 469);
            btnStopPump.Name = "btnStopPump";
            btnStopPump.Size = new Size(142, 38);
            btnStopPump.TabIndex = 5;
            btnStopPump.Text = "Stop Pump";
            btnStopPump.UseVisualStyleBackColor = true;
            btnStopPump.Click += btnStopPump_Click;
            // 
            // tbUrl
            // 
            tbUrl.Location = new Point(114, 12);
            tbUrl.Name = "tbUrl";
            tbUrl.Size = new Size(287, 23);
            tbUrl.TabIndex = 6;
            tbUrl.Text = "http://localhost:9092";
            // 
            // labelErr
            // 
            labelErr.AutoSize = true;
            labelErr.ForeColor = Color.Red;
            labelErr.Location = new Point(141, 237);
            labelErr.Name = "labelErr";
            labelErr.Size = new Size(23, 17);
            labelErr.TabIndex = 7;
            labelErr.Text = "---";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 15);
            label3.Name = "label3";
            label3.Size = new Size(96, 17);
            label3.TabIndex = 8;
            label3.Text = "IOTTree Server";
            // 
            // btnStartClient
            // 
            btnStartClient.Location = new Point(620, 10);
            btnStartClient.Name = "btnStartClient";
            btnStartClient.Size = new Size(86, 25);
            btnStartClient.TabIndex = 9;
            btnStartClient.Text = "Start Client";
            btnStartClient.UseVisualStyleBackColor = true;
            btnStartClient.Click += btnStartClient_Click;
            // 
            // btnStopClient
            // 
            btnStopClient.Location = new Point(712, 10);
            btnStopClient.Name = "btnStopClient";
            btnStopClient.Size = new Size(86, 25);
            btnStopClient.TabIndex = 10;
            btnStopClient.Text = "Stop Client";
            btnStopClient.UseVisualStyleBackColor = true;
            btnStopClient.Click += btnStopClient_Click;
            // 
            // tbClientId
            // 
            tbClientId.Location = new Point(465, 12);
            tbClientId.Name = "tbClientId";
            tbClientId.Size = new Size(130, 23);
            tbClientId.TabIndex = 11;
            tbClientId.Text = "ui-client-001";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(407, 14);
            label4.Name = "label4";
            label4.Size = new Size(57, 17);
            label4.TabIndex = 12;
            label4.Text = "Client ID";
            // 
            // tbTagPaths
            // 
            tbTagPaths.Location = new Point(114, 41);
            tbTagPaths.Multiline = true;
            tbTagPaths.Name = "tbTagPaths";
            tbTagPaths.ScrollBars = ScrollBars.Vertical;
            tbTagPaths.Size = new Size(287, 160);
            tbTagPaths.TabIndex = 13;
            tbTagPaths.Text = "watertank.ch1.aio.wl_val\r\nwatertank.ch1.aio.m1\r\nwatertank.ch1.dio.pstart\r\nwatertank.ch1.dio.pstop\r\nwatertank.ch1.dio.p_running\r\nwatertank.ch1._driver_run\r\nwatertank.ch1._conn_ready";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 44);
            label5.Name = "label5";
            label5.Size = new Size(59, 17);
            label5.TabIndex = 14;
            label5.Text = "Tag Path";
            // 
            // tbTagVals
            // 
            tbTagVals.Location = new Point(407, 41);
            tbTagVals.Multiline = true;
            tbTagVals.Name = "tbTagVals";
            tbTagVals.ReadOnly = true;
            tbTagVals.ScrollBars = ScrollBars.Vertical;
            tbTagVals.Size = new Size(391, 160);
            tbTagVals.TabIndex = 15;
            // 
            // labelInf
            // 
            labelInf.AutoSize = true;
            labelInf.Location = new Point(141, 215);
            labelInf.Name = "labelInf";
            labelInf.Size = new Size(23, 17);
            labelInf.TabIndex = 16;
            labelInf.Text = "---";
            // 
            // labelRunST
            // 
            labelRunST.AutoSize = true;
            labelRunST.Location = new Point(540, 328);
            labelRunST.Name = "labelRunST";
            labelRunST.Size = new Size(32, 17);
            labelRunST.TabIndex = 17;
            labelRunST.Text = "###";
            // 
            // panelRunST
            // 
            panelRunST.Location = new Point(472, 313);
            panelRunST.Name = "panelRunST";
            panelRunST.Size = new Size(59, 45);
            panelRunST.TabIndex = 18;
            // 
            // FormDemo
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(809, 526);
            Controls.Add(panelRunST);
            Controls.Add(labelRunST);
            Controls.Add(labelInf);
            Controls.Add(tbTagVals);
            Controls.Add(label5);
            Controls.Add(tbTagPaths);
            Controls.Add(label4);
            Controls.Add(tbClientId);
            Controls.Add(btnStopClient);
            Controls.Add(btnStartClient);
            Controls.Add(label3);
            Controls.Add(labelErr);
            Controls.Add(tbUrl);
            Controls.Add(btnStopPump);
            Controls.Add(labelLvl);
            Controls.Add(panLvl);
            Controls.Add(panelLvlC);
            Controls.Add(label1);
            Controls.Add(btnStartPump);
            MaximizeBox = false;
            Name = "FormDemo";
            Text = "IOTTree Client Demo";
            FormClosing += FormDemo_FormClosing;
            Load += FormDemo_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStartPump;
        private Label label1;
        private Panel panelLvlC;
        private Panel panLvl;
        private Label labelLvl;
        private Button btnStopPump;
        private TextBox tbUrl;
        private Label labelErr;
        private Label label3;
        private Button btnStartClient;
        private Button btnStopClient;
        private TextBox tbClientId;
        private Label label4;
        private TextBox tbTagPaths;
        private Label label5;
        private TextBox tbTagVals;
        private Label labelInf;
        private Label labelRunST;
        private Panel panelRunST;
    }
}
