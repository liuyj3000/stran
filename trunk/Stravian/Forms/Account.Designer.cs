namespace Stravian
{
	partial class NewAccount
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
			if(disposing && (components != null))
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
			this.button1 = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.button2 = new System.Windows.Forms.Button();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(91, 211);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(100, 34);
			this.button1.TabIndex = 5;
			this.button1.Text = "����";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(36, 94);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(31, 14);
			this.label4.TabIndex = 7;
			this.label4.Text = "����";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(36, 59);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(43, 14);
			this.label5.TabIndex = 6;
			this.label5.Text = "�û���";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(36, 24);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(43, 14);
			this.label6.TabIndex = 5;
			this.label6.Text = "������";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(117, 91);
			this.textBox1.Name = "textBox1";
			this.textBox1.PasswordChar = 'X';
			this.textBox1.Size = new System.Drawing.Size(275, 22);
			this.textBox1.TabIndex = 2;
			this.textBox1.UseSystemPasswordChar = true;
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(117, 56);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(275, 22);
			this.textBox2.TabIndex = 1;
			// 
			// button2
			// 
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Location = new System.Drawing.Point(237, 211);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(100, 34);
			this.button2.TabIndex = 6;
			this.button2.Text = "ȡ��";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "�Զ�̽��",
            "������",
            "�ն�����",
            "��¬��"});
			this.comboBox1.Location = new System.Drawing.Point(117, 126);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(275, 22);
			this.comboBox1.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(36, 129);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(31, 14);
			this.label1.TabIndex = 9;
			this.label1.Text = "����";
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(117, 21);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(275, 22);
			this.textBox3.TabIndex = 0;
			this.textBox3.Leave += new System.EventHandler(this.textBox3_Leave);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(36, 165);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(55, 14);
			this.label2.TabIndex = 12;
			this.label2.Text = "��ʾ����";
			// 
			// textBox4
			// 
			this.textBox4.Location = new System.Drawing.Point(117, 162);
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(275, 22);
			this.textBox4.TabIndex = 4;
			// 
			// NewAccount
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button2;
			this.ClientSize = new System.Drawing.Size(429, 270);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox4);
			this.Controls.Add(this.textBox3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.textBox2);
			this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "NewAccount";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "�ʺ�";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox4;

	}
}