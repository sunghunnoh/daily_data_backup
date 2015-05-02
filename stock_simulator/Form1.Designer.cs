namespace stock_simulator
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.Login_xing = new System.Windows.Forms.Button();
            this.resultList = new System.Windows.Forms.ListBox();
            this.stock_code = new System.Windows.Forms.Button();
            this.save_data = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.margin = new System.Windows.Forms.Label();
            this.loss = new System.Windows.Forms.Label();
            this.count = new System.Windows.Forms.Label();
            this.date_table = new System.Windows.Forms.Button();
            this.simulator2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.statistics_test = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Login_xing
            // 
            this.Login_xing.Location = new System.Drawing.Point(12, 12);
            this.Login_xing.Name = "Login_xing";
            this.Login_xing.Size = new System.Drawing.Size(75, 23);
            this.Login_xing.TabIndex = 0;
            this.Login_xing.TabStop = false;
            this.Login_xing.Text = "Login_xing";
            this.Login_xing.UseVisualStyleBackColor = true;
            this.Login_xing.Click += new System.EventHandler(this.Login_xing_Click);
            // 
            // resultList
            // 
            this.resultList.FormattingEnabled = true;
            this.resultList.ItemHeight = 12;
            this.resultList.Location = new System.Drawing.Point(12, 64);
            this.resultList.Name = "resultList";
            this.resultList.Size = new System.Drawing.Size(603, 340);
            this.resultList.TabIndex = 3;
            this.resultList.TabStop = false;
            this.resultList.SelectedIndexChanged += new System.EventHandler(this.resultList_SelectedIndexChanged);
            // 
            // stock_code
            // 
            this.stock_code.Location = new System.Drawing.Point(114, 12);
            this.stock_code.Name = "stock_code";
            this.stock_code.Size = new System.Drawing.Size(84, 23);
            this.stock_code.TabIndex = 5;
            this.stock_code.TabStop = false;
            this.stock_code.Text = "stock_code";
            this.stock_code.UseVisualStyleBackColor = true;
            this.stock_code.Click += new System.EventHandler(this.stock_code_Click);
            // 
            // save_data
            // 
            this.save_data.Location = new System.Drawing.Point(327, 12);
            this.save_data.Name = "save_data";
            this.save_data.Size = new System.Drawing.Size(75, 23);
            this.save_data.TabIndex = 7;
            this.save_data.TabStop = false;
            this.save_data.Text = "ALL DB";
            this.save_data.UseVisualStyleBackColor = true;
            this.save_data.Click += new System.EventHandler(this.save_data_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(94, 451);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(89, 21);
            this.textBox2.TabIndex = 2;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(94, 478);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(89, 21);
            this.textBox3.TabIndex = 3;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(94, 424);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(89, 21);
            this.textBox4.TabIndex = 1;
            // 
            // margin
            // 
            this.margin.AutoSize = true;
            this.margin.Location = new System.Drawing.Point(16, 454);
            this.margin.Name = "margin";
            this.margin.Size = new System.Drawing.Size(44, 12);
            this.margin.TabIndex = 12;
            this.margin.Text = "margin";
            this.margin.Click += new System.EventHandler(this.label1_Click);
            // 
            // loss
            // 
            this.loss.AutoSize = true;
            this.loss.Location = new System.Drawing.Point(16, 481);
            this.loss.Name = "loss";
            this.loss.Size = new System.Drawing.Size(29, 12);
            this.loss.TabIndex = 13;
            this.loss.Text = "loss";
            this.loss.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // count
            // 
            this.count.AutoSize = true;
            this.count.Location = new System.Drawing.Point(16, 427);
            this.count.Name = "count";
            this.count.Size = new System.Drawing.Size(36, 12);
            this.count.TabIndex = 14;
            this.count.Text = "count";
            // 
            // date_table
            // 
            this.date_table.Location = new System.Drawing.Point(225, 12);
            this.date_table.Name = "date_table";
            this.date_table.Size = new System.Drawing.Size(75, 23);
            this.date_table.TabIndex = 15;
            this.date_table.TabStop = false;
            this.date_table.Text = "date_table";
            this.date_table.UseVisualStyleBackColor = true;
            this.date_table.Click += new System.EventHandler(this.date_table_Click);
            // 
            // simulator2
            // 
            this.simulator2.Location = new System.Drawing.Point(64, 569);
            this.simulator2.Name = "simulator2";
            this.simulator2.Size = new System.Drawing.Size(75, 23);
            this.simulator2.TabIndex = 16;
            this.simulator2.Text = "simulation";
            this.simulator2.UseVisualStyleBackColor = true;
            this.simulator2.Click += new System.EventHandler(this.simulator2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 508);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 12);
            this.label1.TabIndex = 18;
            this.label1.Text = "stock_num";
            this.label1.Click += new System.EventHandler(this.label1_Click_2);
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(94, 505);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(89, 21);
            this.textBox5.TabIndex = 4;
            this.textBox5.TextChanged += new System.EventHandler(this.textBox5_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(429, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 19;
            this.button1.TabStop = false;
            this.button1.Text = "DB update";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // statistics_test
            // 
            this.statistics_test.Location = new System.Drawing.Point(206, 569);
            this.statistics_test.Name = "statistics_test";
            this.statistics_test.Size = new System.Drawing.Size(141, 23);
            this.statistics_test.TabIndex = 20;
            this.statistics_test.Text = "statistics_test";
            this.statistics_test.UseVisualStyleBackColor = true;
            this.statistics_test.Click += new System.EventHandler(this.statistics_test_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(94, 532);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(89, 21);
            this.textBox1.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 535);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 12);
            this.label2.TabIndex = 22;
            this.label2.Text = "stable count";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(206, 422);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(141, 23);
            this.button2.TabIndex = 23;
            this.button2.Text = "구매 simulation";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(363, 422);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(141, 23);
            this.button3.TabIndex = 24;
            this.button3.Text = "개별 simulation";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(521, 550);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(141, 23);
            this.button4.TabIndex = 25;
            this.button4.Text = "CODE sort";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(521, 13);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(121, 23);
            this.button5.TabIndex = 26;
            this.button5.Text = "update_raw_data";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(567, 427);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 27;
            this.button6.TabStop = false;
            this.button6.Text = "ALL DB";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 613);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.statistics_test);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.simulator2);
            this.Controls.Add(this.date_table);
            this.Controls.Add(this.count);
            this.Controls.Add(this.loss);
            this.Controls.Add(this.margin);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.save_data);
            this.Controls.Add(this.stock_code);
            this.Controls.Add(this.resultList);
            this.Controls.Add(this.Login_xing);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Login_xing;
        private System.Windows.Forms.ListBox resultList;
        private System.Windows.Forms.Button stock_code;
        private System.Windows.Forms.Button save_data;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label margin;
        private System.Windows.Forms.Label loss;
        private System.Windows.Forms.Label count;
        private System.Windows.Forms.Button date_table;
        private System.Windows.Forms.Button simulator2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button statistics_test;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;

    }
}

