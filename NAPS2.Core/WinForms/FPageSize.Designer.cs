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
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.textboxWidth = new System.Windows.Forms.TextBox();
            this.LabelX = new System.Windows.Forms.Label();
            this.textboxHeight = new System.Windows.Forms.TextBox();
            this.comboUnit = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.ComboName = new System.Windows.Forms.ComboBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.BtnDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // textboxWidth
            // 
            resources.ApplyResources(this.textboxWidth, "textboxWidth");
            this.textboxWidth.Name = "textboxWidth";
            // 
            // LabelX
            // 
            resources.ApplyResources(this.LabelX, "LabelX");
            this.LabelX.Name = "LabelX";
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
            // Label1
            // 
            resources.ApplyResources(this.Label1, "Label1");
            this.Label1.Name = "Label1";
            // 
            // ComboName
            // 
            this.ComboName.FormattingEnabled = true;
            resources.ApplyResources(this.ComboName, "ComboName");
            this.ComboName.Name = "ComboName";
            this.ComboName.SelectionChangeCommitted += new System.EventHandler(this.ComboName_SelectionChangeCommitted);
            this.ComboName.TextChanged += new System.EventHandler(this.ComboName_TextChanged);
            // 
            // Label2
            // 
            resources.ApplyResources(this.Label2, "Label2");
            this.Label2.Name = "Label2";
            // 
            // BtnDelete
            // 
            resources.ApplyResources(this.BtnDelete, "BtnDelete");
            this.BtnDelete.Image = global::NAPS2.Icons.cross_small;
            this.BtnDelete.Name = "BtnDelete";
            this.BtnDelete.UseVisualStyleBackColor = true;
            this.BtnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // FPageSize
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnDelete);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.ComboName);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.comboUnit);
            this.Controls.Add(this.textboxHeight);
            this.Controls.Add(this.LabelX);
            this.Controls.Add(this.textboxWidth);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FPageSize";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.TextBox textboxWidth;
        private System.Windows.Forms.Label LabelX;
        private System.Windows.Forms.TextBox textboxHeight;
        private System.Windows.Forms.ComboBox comboUnit;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.ComboBox ComboName;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.Button BtnDelete;

    }
}