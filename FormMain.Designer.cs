namespace GeoAreasUpdate
{
    partial class FormMain
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtFileZip = new System.Windows.Forms.TextBox();
            this.btnElabora = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.txtLog = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelect
            // 
            this.btnSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelect.Image = ((System.Drawing.Image)(resources.GetObject("btnSelect.Image")));
            this.btnSelect.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnSelect.Location = new System.Drawing.Point(679, 51);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(85, 68);
            this.btnSelect.TabIndex = 0;
            this.btnSelect.Text = "Select file";
            this.btnSelect.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // txtFileZip
            // 
            this.txtFileZip.Enabled = false;
            this.txtFileZip.Location = new System.Drawing.Point(12, 72);
            this.txtFileZip.Name = "txtFileZip";
            this.txtFileZip.Size = new System.Drawing.Size(661, 20);
            this.txtFileZip.TabIndex = 1;
            // 
            // btnElabora
            // 
            this.btnElabora.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnElabora.Image = ((System.Drawing.Image)(resources.GetObject("btnElabora.Image")));
            this.btnElabora.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnElabora.Location = new System.Drawing.Point(84, 145);
            this.btnElabora.Name = "btnElabora";
            this.btnElabora.Size = new System.Drawing.Size(199, 68);
            this.btnElabora.TabIndex = 2;
            this.btnElabora.Text = "Elabora";
            this.btnElabora.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnElabora.UseVisualStyleBackColor = true;
            this.btnElabora.Click += new System.EventHandler(this.btnElabora_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(400, 342);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "test  connection";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(319, 342);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(75, 29);
            this.dataGridView1.TabIndex = 4;
            this.dataGridView1.Visible = false;
            // 
            // txtLog
            // 
            this.txtLog.Enabled = false;
            this.txtLog.Location = new System.Drawing.Point(12, 255);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(752, 183);
            this.txtLog.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(23, 239);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Lista file non caricati";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(308, 169);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(456, 23);
            this.progressBar1.TabIndex = 7;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnElabora);
            this.Controls.Add(this.txtFileZip);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Aggiornamnto Area Geo -Valuto-";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox txtFileZip;
        private System.Windows.Forms.Button btnElabora;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}

