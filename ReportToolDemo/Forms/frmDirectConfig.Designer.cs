namespace ReportToolDemo.ReportServiceTool.Forms
{
    partial class frmDirectConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDirectConfig));
            this.txtFTPPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ckbPwd = new System.Windows.Forms.CheckBox();
            this.txtLoginPwd = new System.Windows.Forms.TextBox();
            this.txtLoginName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFTPIP = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnConfigOk = new System.Windows.Forms.Button();
            this.btnConfigCancel = new System.Windows.Forms.Button();
            this.txtDirectFTPPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.rbtnPasv = new System.Windows.Forms.RadioButton();
            this.rbtnPort = new System.Windows.Forms.RadioButton();
            this.label30 = new System.Windows.Forms.Label();
            this.groupBox10.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtFTPPort
            // 
            this.txtFTPPort.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtFTPPort.Location = new System.Drawing.Point(156, 61);
            this.txtFTPPort.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPPort.Name = "txtFTPPort";
            this.txtFTPPort.Size = new System.Drawing.Size(205, 23);
            this.txtFTPPort.TabIndex = 24;
            this.txtFTPPort.Text = "2121";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(44, 64);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 14);
            this.label5.TabIndex = 23;
            this.label5.Text = "FTP端口：";
            // 
            // ckbPwd
            // 
            this.ckbPwd.AutoSize = true;
            this.ckbPwd.BackColor = System.Drawing.Color.Transparent;
            this.ckbPwd.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ckbPwd.Location = new System.Drawing.Point(369, 128);
            this.ckbPwd.Margin = new System.Windows.Forms.Padding(4);
            this.ckbPwd.Name = "ckbPwd";
            this.ckbPwd.Size = new System.Drawing.Size(15, 14);
            this.ckbPwd.TabIndex = 22;
            this.ckbPwd.UseVisualStyleBackColor = false;
            this.ckbPwd.CheckedChanged += new System.EventHandler(this.ckbPwd_CheckedChanged);
            // 
            // txtLoginPwd
            // 
            this.txtLoginPwd.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtLoginPwd.Location = new System.Drawing.Point(156, 123);
            this.txtLoginPwd.Margin = new System.Windows.Forms.Padding(4);
            this.txtLoginPwd.Name = "txtLoginPwd";
            this.txtLoginPwd.PasswordChar = '*';
            this.txtLoginPwd.Size = new System.Drawing.Size(205, 23);
            this.txtLoginPwd.TabIndex = 19;
            this.txtLoginPwd.Text = "123456";
            // 
            // txtLoginName
            // 
            this.txtLoginName.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtLoginName.Location = new System.Drawing.Point(156, 92);
            this.txtLoginName.Margin = new System.Windows.Forms.Padding(4);
            this.txtLoginName.Name = "txtLoginName";
            this.txtLoginName.Size = new System.Drawing.Size(205, 23);
            this.txtLoginName.TabIndex = 18;
            this.txtLoginName.Text = "FTPTEST";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(44, 128);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 14);
            this.label6.TabIndex = 17;
            this.label6.Text = "登录密码：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(44, 95);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 14);
            this.label4.TabIndex = 16;
            this.label4.Text = "登录名：";
            // 
            // txtFTPIP
            // 
            this.txtFTPIP.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtFTPIP.Location = new System.Drawing.Point(156, 30);
            this.txtFTPIP.Margin = new System.Windows.Forms.Padding(4);
            this.txtFTPIP.Name = "txtFTPIP";
            this.txtFTPIP.Size = new System.Drawing.Size(205, 23);
            this.txtFTPIP.TabIndex = 15;
            this.txtFTPIP.Text = "192.168.100.117";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(44, 33);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 14);
            this.label3.TabIndex = 14;
            this.label3.Text = "FTP服务IP：";
            // 
            // btnConfigOk
            // 
            this.btnConfigOk.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnConfigOk.Image = ((System.Drawing.Image)(resources.GetObject("btnConfigOk.Image")));
            this.btnConfigOk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfigOk.Location = new System.Drawing.Point(203, 188);
            this.btnConfigOk.Margin = new System.Windows.Forms.Padding(4);
            this.btnConfigOk.Name = "btnConfigOk";
            this.btnConfigOk.Size = new System.Drawing.Size(94, 37);
            this.btnConfigOk.TabIndex = 25;
            this.btnConfigOk.Text = "确认";
            this.btnConfigOk.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConfigOk.UseVisualStyleBackColor = true;
            this.btnConfigOk.Click += new System.EventHandler(this.btnConfigOk_Click);
            // 
            // btnConfigCancel
            // 
            this.btnConfigCancel.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnConfigCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnConfigCancel.Image")));
            this.btnConfigCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfigCancel.Location = new System.Drawing.Point(319, 188);
            this.btnConfigCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnConfigCancel.Name = "btnConfigCancel";
            this.btnConfigCancel.Size = new System.Drawing.Size(94, 37);
            this.btnConfigCancel.TabIndex = 26;
            this.btnConfigCancel.Text = "取消";
            this.btnConfigCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConfigCancel.UseVisualStyleBackColor = true;
            this.btnConfigCancel.Click += new System.EventHandler(this.btnConfigCancel_Click);
            // 
            // txtDirectFTPPath
            // 
            this.txtDirectFTPPath.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtDirectFTPPath.Location = new System.Drawing.Point(46, 205);
            this.txtDirectFTPPath.Margin = new System.Windows.Forms.Padding(4);
            this.txtDirectFTPPath.Name = "txtDirectFTPPath";
            this.txtDirectFTPPath.Size = new System.Drawing.Size(82, 23);
            this.txtDirectFTPPath.TabIndex = 28;
            this.txtDirectFTPPath.Text = "/PUSH/20170721";
            this.txtDirectFTPPath.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(43, 187);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 14);
            this.label1.TabIndex = 27;
            this.label1.Text = "直连FTP目录：";
            this.label1.Visible = false;
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.rbtnPasv);
            this.groupBox10.Controls.Add(this.rbtnPort);
            this.groupBox10.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox10.Location = new System.Drawing.Point(214, 149);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(147, 29);
            this.groupBox10.TabIndex = 30;
            this.groupBox10.TabStop = false;
            // 
            // rbtnPasv
            // 
            this.rbtnPasv.AutoSize = true;
            this.rbtnPasv.Location = new System.Drawing.Point(74, 9);
            this.rbtnPasv.Name = "rbtnPasv";
            this.rbtnPasv.Size = new System.Drawing.Size(71, 16);
            this.rbtnPasv.TabIndex = 16;
            this.rbtnPasv.Text = "被动模式";
            this.rbtnPasv.UseVisualStyleBackColor = true;
            // 
            // rbtnPort
            // 
            this.rbtnPort.AutoSize = true;
            this.rbtnPort.Checked = true;
            this.rbtnPort.Location = new System.Drawing.Point(3, 9);
            this.rbtnPort.Name = "rbtnPort";
            this.rbtnPort.Size = new System.Drawing.Size(71, 16);
            this.rbtnPort.TabIndex = 15;
            this.rbtnPort.TabStop = true;
            this.rbtnPort.Text = "主动模式";
            this.rbtnPort.UseVisualStyleBackColor = true;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label30.Location = new System.Drawing.Point(155, 160);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(53, 12);
            this.label30.TabIndex = 29;
            this.label30.Text = "连接模式";
            // 
            // frmDirectConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 234);
            this.Controls.Add(this.groupBox10);
            this.Controls.Add(this.label30);
            this.Controls.Add(this.txtDirectFTPPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnConfigCancel);
            this.Controls.Add(this.btnConfigOk);
            this.Controls.Add(this.txtFTPPort);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ckbPwd);
            this.Controls.Add(this.txtLoginPwd);
            this.Controls.Add(this.txtLoginName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtFTPIP);
            this.Controls.Add(this.label3);
            this.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmDirectConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "直连FTP服务配置";
            this.Load += new System.EventHandler(this.frmDirectConfig_Load);
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFTPPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox ckbPwd;
        private System.Windows.Forms.TextBox txtLoginPwd;
        private System.Windows.Forms.TextBox txtLoginName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtFTPIP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnConfigOk;
        private System.Windows.Forms.Button btnConfigCancel;
        private System.Windows.Forms.TextBox txtDirectFTPPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.RadioButton rbtnPasv;
        private System.Windows.Forms.RadioButton rbtnPort;
        private System.Windows.Forms.Label label30;
    }
}