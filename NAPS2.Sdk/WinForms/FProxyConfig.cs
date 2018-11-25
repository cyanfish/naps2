using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.ClientServer;
using NAPS2.Lang.Resources;
using NAPS2.Scan;

namespace NAPS2.WinForms
{
    public partial class FProxyConfig : FormBase
    {
        public FProxyConfig()
        {
            InitializeComponent();
        }

        public ScanProxyConfig ProxyConfig { get; set; }

        public bool UseProxy { get; set; }
        
        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(comboName)
                    .WidthToForm()
                .Bind(txtIP)
                    .WidthTo(() => Width * 2 / 3)
                .Bind(txtPort)
                    .WidthTo(() => Width / 3)
                .Bind(lblPort)
                    .LeftTo(() => txtPort.Left)
                .Bind(txtPort, btnCancel, btnOK, btnDelete)
                    .RightToForm()
                .Activate();

            ProxyConfig = ProxyConfig ?? new ScanProxyConfig();
            cbUseProxy.Checked = UseProxy;
            UpdateDropdown();
            UpdateControls();

            new Thread(() => ServerDiscovery.SendBroadcast((computerName, ep) =>
            {
                SafeInvoke(() =>
                {
                    if (string.IsNullOrWhiteSpace(txtIP.Text))
                    {
                        // TODO: Maybe add all responses to dropdown/saved
                        ProxyConfig = new ScanProxyConfig
                        {
                            Name = computerName,
                            Ip = ep.Address.ToString(),
                            Port = ep.Port
                        };
                        UpdateControls();
                    }
                });
            })).Start();
        }

        private void UpdateDropdown()
        {
            comboName.Items.Clear();
            foreach (var proxyConfig in UserConfigManager.Config.SavedProxies.OrderBy(x => x.Name))
            {
                comboName.Items.Add(proxyConfig.Name);
            }
        }

        private void UpdateControls()
        {
            comboName.Text = ProxyConfig.Name ?? "";
            txtIP.Text = ProxyConfig.Ip ?? "";
            txtPort.Text = ProxyConfig.Port?.ToString() ?? "";

            comboName.Enabled = txtIP.Enabled = txtPort.Enabled = cbUseProxy.Checked;
        }

        private void comboName_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var savedProxies = UserConfigManager.Config.SavedProxies;
            var proxyConfig = savedProxies.FirstOrDefault(x => x.Name == (string)comboName.SelectedItem);
            if (proxyConfig != null)
            {
                ProxyConfig = proxyConfig;
                UpdateControls();
            }
        }

        private void comboName_TextChanged(object sender, EventArgs e)
        {
            var savedProxies = UserConfigManager.Config.SavedProxies;
            btnDelete.Enabled = savedProxies.Any(x => x.Name == comboName.Text);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!cbUseProxy.Checked)
            {
                ProxyConfig = null;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            const NumberStyles numberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingSign;
            if (string.IsNullOrWhiteSpace(txtIP.Text) || txtIP.Text.Contains('/'))
            {
                txtIP.Focus();
                return;
            }
            if (!int.TryParse(txtPort.Text, numberStyle, CultureInfo.CurrentCulture, out int port))
            {
                txtPort.Focus();
                return;
            }
            ProxyConfig = new ScanProxyConfig
            {
                Name = comboName.Text,
                Ip = txtIP.Text,
                Port = port
            };
            if (!string.IsNullOrWhiteSpace(comboName.Text))
            {
                var savedProxies = UserConfigManager.Config.SavedProxies;
                savedProxies.RemoveAll(x => x.Name == ProxyConfig.Name);
                savedProxies.Add(ProxyConfig);
                UserConfigManager.Save();
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(string.Format(MiscResources.ConfirmDelete, comboName.Text), MiscResources.Delete, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                var savedProxies = UserConfigManager.Config.SavedProxies;
                savedProxies.RemoveAll(x => x.Name == comboName.Text);
                UserConfigManager.Save();

                ProxyConfig = new ScanProxyConfig();
                UpdateDropdown();
                UpdateControls();
            }
        }

        private void cbUseProxy_CheckedChanged(object sender, EventArgs e)
        {
            UseProxy = cbUseProxy.Checked;
            UpdateControls();
        }
    }
}
