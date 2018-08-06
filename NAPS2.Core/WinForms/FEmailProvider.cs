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
using NAPS2.ImportExport.Email.Imap;
using NAPS2.Scan;

namespace NAPS2.WinForms
{
    public partial class FEmailProvider : FormBase
    {
        private readonly IEmailProviderFactory emailProviderFactory;
        private readonly GmailApi gmailApi;

        private List<EmailProviderWidget> providerWidgets;
        private string[] systemClientNames;
        private string defaultSystemClientName;

        public FEmailProvider(IEmailProviderFactory emailProviderFactory, GmailApi gmailApi)
        {
            this.emailProviderFactory = emailProviderFactory;
            this.gmailApi = gmailApi;

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
                    ProviderType = EmailProviderType.System,
                    ProviderIcon = GetSystemClientIcon(clientName),
                    ProviderName = clientName,
                    ClickAction = () => ChooseSystem(clientName)
                });
            }
            providerWidgets.Add(new EmailProviderWidget
            {
                ProviderType = EmailProviderType.Gmail,
                ProviderIcon = Icons.gmail,
                ProviderName = EmailProviderType.Gmail.Description(),
                ClickAction = ChooseGmail
                
            });
            providerWidgets.Add(new EmailProviderWidget
            {
                ProviderType = EmailProviderType.OutlookWeb,
                ProviderIcon = Icons.outlookweb,
                ProviderName = EmailProviderType.OutlookWeb.Description(),
                ClickAction = ChooseOutlookWeb
            });
            providerWidgets.Add(new EmailProviderWidget
            {
                ProviderType = EmailProviderType.CustomSmtp,
                ProviderIcon = null,
                ProviderName = EmailProviderType.CustomSmtp.Description(),
                ClickAction = ChooseCustomSmtp
            });

            // Put the configured provider at the top
            var defaultWidget = GetDefaultWidget();
            if (defaultWidget != null)
            {
                providerWidgets.Remove(defaultWidget);
                providerWidgets.Insert(0, defaultWidget);
            }
            
            ShowWidgets();
        }

        private void ChooseSystem(string clientName)
        {
            var setup = GetOrCreateSetup();
            setup.ProviderType = EmailProviderType.System;
            setup.SystemProviderName = clientName == defaultSystemClientName ? null : clientName;
            UserConfigManager.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ChooseGmail()
        {
            var authForm = FormFactory.Create<FAuthorize>();
            authForm.OauthProvider = gmailApi;
            authForm.ShowDialog();
            if (authForm.DialogResult == DialogResult.OK)
            {
                var setup = GetOrCreateSetup();
                setup.ProviderType = EmailProviderType.Gmail;
                setup.GmailToken = authForm.Token;
                setup.GmailUser = gmailApi.GetEmail(authForm.Token);
                UserConfigManager.Save();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void ChooseOutlookWeb()
        {
            throw new NotImplementedException();
        }

        private void ChooseCustomSmtp()
        {
            throw new NotImplementedException();
        }

        private EmailSetup GetOrCreateSetup()
        {
            if (UserConfigManager.Config.EmailSetup == null)
            {
                UserConfigManager.Config.EmailSetup = new EmailSetup();
            }
            return UserConfigManager.Config.EmailSetup;
        }

        private void ShowWidgets()
        {
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

        private EmailProviderWidget GetDefaultWidget()
        {
            var setup = UserConfigManager.Config.EmailSetup;
            foreach (var widget in providerWidgets)
            {
                if (widget.ProviderType == (setup?.ProviderType ?? EmailProviderType.System))
                {
                    if (widget.ProviderType == EmailProviderType.System)
                    {
                        // System providers need additional logic since there may be more than one
                        if (widget.ProviderName == setup?.SystemProviderName
                            || setup?.SystemProviderName == null && widget.ProviderName == defaultSystemClientName)
                        {
                            return widget;
                        }
                    }
                    else
                    {
                        return widget;
                    }
                }
            }
            return null;
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
