using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using NAPS2.ImportExport.Email;
using NAPS2.Scan;

namespace NAPS2.WinForms
{
    public partial class FEmailProvider : FormBase
    {
        private readonly IEmailProviderFactory emailProviderFactory;
        private List<EmailProviderWidget> providerWidgets;
        private string[] systemClientNames;
        private string defaultSystemClientName;

        public FEmailProvider(IEmailProviderFactory emailProviderFactory)
        {
            this.emailProviderFactory = emailProviderFactory;
            
            InitializeComponent();
        }

        private void FEmailProvider_Load(object sender, EventArgs e)
        {
            providerWidgets = new List<EmailProviderWidget>();
            systemClientNames = GetSystemClientNames();
            defaultSystemClientName = GetDefaultSystemClientName();

            foreach (var clientName in systemClientNames.OrderBy(x => x == defaultSystemClientName ? 0 : 1))
            {
                providerWidgets.Add(new EmailProviderWidget
                {
                    Provider = emailProviderFactory.Create(EmailProviderType.System),
                    ProviderIcon = GetSystemClientIcon(clientName),
                    ProviderName = clientName
                });
            }
            providerWidgets.Add(new EmailProviderWidget
            {
                Provider = emailProviderFactory.Create(EmailProviderType.Gmail),
                ProviderIcon = Icons.gmail,
                ProviderName = EmailProviderType.Gmail.Description()
            });
            providerWidgets.Add(new EmailProviderWidget
            {
                Provider = emailProviderFactory.Create(EmailProviderType.OutlookWeb),
                ProviderIcon = Icons.outlookweb,
                ProviderName = EmailProviderType.OutlookWeb.Description()
            });
            providerWidgets.Add(new EmailProviderWidget
            {
                Provider = emailProviderFactory.Create(EmailProviderType.CustomSmtp),
                ProviderIcon = null,
                ProviderName = EmailProviderType.CustomSmtp.Description()
            });

            // A lot of little fiddling here. This just makes the widgets display nicely
            int heightDiff = Height - panel1.Height;
            panel1.Height = 0;
            foreach (var widget in providerWidgets)
            {
                panel1.Controls.Add(widget);
                widget.Top = panel1.Height;
                widget.Width = panel1.Width;
                widget.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                panel1.Height += widget.Height - 1;
            }
            panel1.Height += 1;
            MaximumSize = new Size(MaximumSize.Width, panel1.Height + heightDiff);
            MinimumSize = new Size(MinimumSize.Width, panel1.Height + heightDiff);
        }

        private string GetDefaultSystemClientName()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Clients\Mail", false))
            {
                return key?.GetValue(null).ToString();
            }
        }

        private string[] GetSystemClientNames()
        {
            // TODO: Swallow errors
            using (var clientList = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\Mail", false))
            {
                return clientList?.GetSubKeyNames().Where(clientName =>
                {
                    using (var clientKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}"))
                    {
                        return clientKey?.GetValue("DllPath") != null;
                    }
                }).ToArray() ?? new string[0];
            }
        }

        private Image GetSystemClientIcon(string clientName)
        {
            using (var command = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Clients\Mail\{clientName}\shell\open\command", false))
            {
                string commandText = command?.GetValue(null).ToString() ?? "";
                if (!commandText.StartsWith("\""))
                {
                    return null;
                }
                string exePath = commandText.Substring(1, commandText.IndexOf("\"", 1, StringComparison.InvariantCulture) - 1);
                var icon = Icon.ExtractAssociatedIcon(exePath);
                return icon?.ToBitmap();
            }
        }
    }
}
