using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.Util;
using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Oauth
{
    public class OutlookWebOauthProvider : OauthProvider
    {
        private readonly IUserConfigManager userConfigManager;

        private OauthClientCreds creds;

        public OutlookWebOauthProvider(IUserConfigManager userConfigManager)
        {
            this.userConfigManager = userConfigManager;
        }

        #region Authorization

        public override OauthToken Token => userConfigManager.Config.EmailSetup?.OutlookWebToken;

        public override string User => userConfigManager.Config.EmailSetup?.OutlookWebUser;

        protected override OauthClientCreds ClientCreds
        {
            get
            {
                if (creds == null)
                {
                    var credObj = JObject.Parse(Encoding.UTF8.GetString(NAPS2.ClientCreds.microsoft_credentials));
                    creds = new OauthClientCreds(credObj.Value<string>("client_id"), credObj.Value<string>("client_secret"));
                }
                return creds;
            }
        }

        protected override string Scope => "https://outlook.office.com/mail.readwrite https://outlook.office.com/mail.send https://outlook.office.com/user.read offline_access";

        protected override string CodeEndpoint => "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";

        protected override string TokenEndpoint => "https://login.microsoftonline.com/common/oauth2/v2.0/token";

        protected override void SaveToken(OauthToken token, bool refresh)
        {
            userConfigManager.Config.EmailSetup = userConfigManager.Config.EmailSetup ?? new EmailSetup();
            userConfigManager.Config.EmailSetup.OutlookWebToken = token;
            if (!refresh)
            {
                userConfigManager.Config.EmailSetup.OutlookWebUser = GetEmailAddress();
                userConfigManager.Config.EmailSetup.ProviderType = EmailProviderType.OutlookWeb;
            }
            userConfigManager.Save();
        }

        #endregion

        #region Api Methods

        public string GetEmailAddress()
        {
            var resp = GetAuthorized("https://outlook.office.com/api/v1.0/me");
            return resp.Value<string>("Id");
        }

        public async Task<string> UploadDraft(string messageRaw, ProgressHandler progressCallback, CancellationToken cancelToken)
        {
            var resp = await PostAuthorized("https://outlook.office.com/api/v1.0/me/messages", messageRaw, "application/json", progressCallback, cancelToken);
            return resp.Value<string>("WebLink");
        }

        #endregion
    }
}
