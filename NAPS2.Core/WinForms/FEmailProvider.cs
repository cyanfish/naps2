using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Email.Oauth;
using NAPS2.Scan;

namespace NAPS2.WinForms
{
    public partial class FEmailProvider : FormBase
    {
        private readonly GmailOauthProvider gmailOauthProvider;
        private readonly OutlookWebOauthProvider outlookWebOauthProvider;
        private readonly SystemEmailClients systemEmailClients;

        private List<EmailProviderWidget> providerWidgets;
        private string[] systemClientNames;
        private string defaultSystemClientName;

        public FEmailProvider(GmailOauthProvider gmailOauthProvider, OutlookWebOauthProvider outlookWebOauthProvider, SystemEmailClients systemEmailClients)
        {
            this.gmailOauthProvider = gmailOauthProvider;
            this.outlookWebOauthProvider = outlookWebOauthProvider;
            this.systemEmailClients = systemEmailClients;

            InitializeComponent();
        }

        private void FEmailProvider_Load(object sender, EventArgs e)
        {
            providerWidgets = new List<EmailProviderWidget>();
            systemClientNames = systemEmailClients.GetNames();
            defaultSystemClientName = systemEmailClients.GetDefaultName();

            foreach (var clientName in systemClientNames.OrderBy(x => x == defaultSystemClientName ? 0 : 1))
            {
                providerWidgets.Add(new EmailProviderWidget
                {
                    ProviderType = EmailProviderType.System,
                    ProviderIcon = systemEmailClients.GetIcon(clientName) ?? Icons.mail_yellow,
                    ProviderName = clientName,
                    ClickAction = () => ChooseSystem(clientName)
                });
            }

            if (gmailOauthProvider.HasClientCreds)
            {
                providerWidgets.Add(new EmailProviderWidget
                {
                    ProviderType = EmailProviderType.Gmail,
                    ProviderIcon = Icons.gmail,
                    ProviderName = EmailProviderType.Gmail.Description(),
                    ClickAction = () => ChooseOauth(gmailOauthProvider)
                });
            }

            if (outlookWebOauthProvider.HasClientCreds)
            {
                providerWidgets.Add(new EmailProviderWidget
                {
                    ProviderType = EmailProviderType.OutlookWeb,
                    ProviderIcon = Icons.outlookweb,
                    ProviderName = EmailProviderType.OutlookWeb.Description(),
                    ClickAction = () => ChooseOauth(outlookWebOauthProvider)
                });
            }

            //providerWidgets.Add(new EmailProviderWidget
            //{
            //    ProviderType = EmailProviderType.CustomSmtp,
            //    ProviderIcon = Icons.email_setting,
            //    ProviderName = EmailProviderType.CustomSmtp.Description(),
            //    ClickAction = ChooseCustomSmtp
            //});

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
            UserConfigManager.Config.EmailSetup = UserConfigManager.Config.EmailSetup ?? new EmailSetup();
            UserConfigManager.Config.EmailSetup.SystemProviderName = clientName;
            UserConfigManager.Config.EmailSetup.ProviderType = EmailProviderType.System;
            UserConfigManager.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ChooseOauth(OauthProvider provider)
        {
            var authForm = FormFactory.Create<FAuthorize>();
            authForm.OauthProvider = provider;
            if (authForm.ShowDialog() == DialogResult.OK)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
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
    }
}
