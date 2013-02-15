namespace NAPS
{
    partial class FChooseIcon
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
            this.iconList = new System.Windows.Forms.ListView();
            this.ilProfileIcons = new NAPS.ILProfileIcons(this.components);
            this.SuspendLayout();
            // 
            // iconList
            // 
            this.iconList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iconList.Location = new System.Drawing.Point(0, 0);
            this.iconList.MultiSelect = false;
            this.iconList.Name = "iconList";
            this.iconList.Size = new System.Drawing.Size(316, 302);
            this.iconList.TabIndex = 0;
            this.iconList.UseCompatibleStateImageBehavior = false;
            this.iconList.ItemActivate += new System.EventHandler(this.listView1_ItemActivate);
            // 
            // FChooseIcon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 302);
            this.Controls.Add(this.iconList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FChooseIcon";
            this.Text = "FChooseIcon";
            this.Load += new System.EventHandler(this.FChooseIcon_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView iconList;
        private ILProfileIcons ilProfileIcons;
    }
}