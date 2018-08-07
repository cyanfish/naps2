using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NAPS2.Config;
using Newtonsoft.Json.Linq;

namespace NAPS2.ImportExport.Email.Imap
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

        public override OauthToken Token
        {
            get => userConfigManager.Config.EmailSetup?.OutlookWebToken;
            protected set
            {
                userConfigManager.Config.EmailSetup = userConfigManager.Config.EmailSetup ?? new EmailSetup();
                userConfigManager.Config.EmailSetup.OutlookWebToken = value;
                userConfigManager.Save();
            }
        }

        public override string User
        {
            get => userConfigManager.Config.EmailSetup?.OutlookWebUser;
            protected set
            {
                userConfigManager.Config.EmailSetup = userConfigManager.Config.EmailSetup ?? new EmailSetup();
                userConfigManager.Config.EmailSetup.OutlookWebUser = value;
                userConfigManager.Save();
            }
        }

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

        protected override string Scope => "https://outlook.office.com/mail.readwrite https://outlook.office.com/mail.send offline_access";

        protected override string CodeEndpoint => "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";

        protected override string TokenEndpoint => "https://login.microsoftonline.com/common/oauth2/v2.0/token";

        #endregion

        #region Api Methods

        protected override string GetUser()
        {
            return "";
            //var resp = GetAuthorized("https://www.googleapis.com/gmail/v1/users/me/profile");
            //return resp.Value<string>("emailAddress");
        }

        public string UploadDraft(string messageRaw)
        {
            var resp = PostAuthorized($"https://www.googleapis.com/upload/gmail/v1/users/{User}/drafts?uploadType=multipart", messageRaw, "message/rfc822");
            return resp.Value<string>("id");
        }

        #endregion
    }
}
