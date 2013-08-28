namespace NAPS2.WinForms
{
    partial class FRecover
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
            this.lblPrompt = new System.Windows.Forms.Label();
            this.btnRecover = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblPrompt
            // 
            this.lblPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrompt.Location = new System.Drawing.Point(12, 9);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(372, 46);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "{0} images scanned on {1} at {2} may not have been saved, and are recoverable. Do" +
    " you want to recover them?";
            // 
            // btnRecover
            // 
            this.btnRecover.Location = new System.Drawing.Point(12, 58);
            this.btnRecover.Name = "btnRecover";
            this.btnRecover.Size = new System.Drawing.Size(120, 30);
            this.btnRecover.TabIndex = 1;
            this.btnRecover.Text = "Recover";
            this.btnRecover.UseVisualStyleBackColor = true;
            this.btnRecover.Click += new System.EventHandler(this.btnRecover_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(138, 58);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(120, 30);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(264, 58);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Not Now";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FRecover
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 100);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnRecover);
            this.Controls.Add(this.lblPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FRecover";
            this.Text = "Recover Scanned Images";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.Button btnRecover;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnCancel;
    }
}