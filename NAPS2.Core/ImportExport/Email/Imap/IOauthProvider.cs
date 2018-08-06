using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NAPS2.ImportExport.Email.Imap
{
    public interface IOauthProvider
    {
        string OauthUrl(string state, string redirectUri);

        OauthToken AcquireToken(string code, string redirectUri);

        void RefreshToken(OauthToken token);
    }
}
