namespace NAPS2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FChooseIcon));
            this.iconList = new System.Windows.Forms.ListView();
            this.ilProfileIcons = new NAPS2.ILProfileIcons(this.components);
            this.SuspendLayout();
            // 
            // iconList
            // 
            resources.ApplyResources(this.iconList, "iconList");
            this.iconList.MultiSelect = false;
            this.iconList.Name = "iconList";
            this.iconList.UseCompatibleStateImageBehavior = false;
            this.iconList.ItemActivate += new System.EventHandler(this.listView1_ItemActivate);
            // 
            // FChooseIcon
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.iconList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FChooseIcon";
            this.Load += new System.EventHandler(this.FChooseIcon_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView iconList;
        private ILProfileIcons ilProfileIcons;
    }
}
