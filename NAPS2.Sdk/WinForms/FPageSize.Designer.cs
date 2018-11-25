namespace NAPS2.WinForms
{
    partial class FPageSize
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FPageSize));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.textboxWidth = new System.Windows.Forms.TextBox();
            this.labelX = new System.Windows.Forms.Label();
            this.textboxHeight = new System.Windows.Forms.TextBox();
            this.comboUnit = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // textboxWidth
            // 
            resources.ApplyResources(this.textboxWidth, "textboxWidth");
            this.textboxWidth.Name = "textboxWidth";
            // 
            // labelX
            // 
            resources.ApplyResources(this.labelX, "labelX");
            this.labelX.Name = "labelX";
            // 
            // textboxHeight
            // 
            resources.ApplyResources(this.textboxHeight, "textboxHeight");
            this.textboxHeight.Name = "textboxHeight";
            // 
            // comboUnit
            // 
            this.comboUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUnit.FormattingEnabled = true;
            resources.ApplyResources(this.comboUnit, "comboUnit");
            this.comboUnit.Name = "comboUnit";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // comboName
            // 
            this.comboName.FormattingEnabled = true;
            resources.ApplyResources(this.comboName, "comboName");
            this.comboName.Name = "comboName";
            this.comboName.SelectionChangeCommitted += new System.EventHandler(this.comboName_SelectionChangeCommitted);
            this.comboName.TextChanged += new System.EventHandler(this.comboName_TextChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // btnDelete
            // 
            resources.ApplyResources(this.btnDelete, "btnDelete");
            this.btnDelete.Image = global::NAPS2.Icons.cross_small;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // FPageSize
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboUnit);
            this.Controls.Add(this.textboxHeight);
            this.Controls.Add(this.labelX);
            this.Controls.Add(this.textboxWidth);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FPageSize";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox textboxWidth;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.TextBox textboxHeight;
        private System.Windows.Forms.ComboBox comboUnit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnDelete;

    }
}