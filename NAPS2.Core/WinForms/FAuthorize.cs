using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using NAPS2.ImportExport.Email.Imap;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FAuthorize : FormBase
    {
        private readonly ThreadFactory threadFactory;

        private string state;
        private int port;
        private HttpListener listener;

        public FAuthorize(ThreadFactory threadFactory)
        {
            this.threadFactory = threadFactory;

            RestoreFormState = false;
            InitializeComponent();
        }

        public IOauthProvider OauthProvider { get; set; }

        public OauthToken Token { get; private set; }

        private void FAuthorize_Load(object sender, EventArgs e)
        {
            MaximumSize = new Size(Math.Max(lblWaiting.Width + 142, 272), Height);
            MinimumSize = new Size(Math.Max(lblWaiting.Width + 142, 272), Height);

            InitState();
            OpenSocket();
            OpenOauthUrl();
        }

        private void InitState()
        {
            byte[] buffer = new byte[16];
            SecureStorage.CryptoRandom.Value.GetBytes(buffer);
            state = string.Join("", buffer.Select(b => b.ToString("x")));
            // There's a possible race condition here with the port, but meh
            port = GetUnusedPort();
        }

        private void linkTryAgain_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            listener.Abort();
            OpenSocket();
            OpenOauthUrl();
        }

        private void OpenSocket()
        {
            threadFactory.StartThread(() =>
            {
                listener = new HttpListener();
                listener.Prefixes.Add(RedirectUri);
                listener.Start();
                while (true)
                {
                    var ctx = listener.GetContext();
                    var queryString = ctx.Request.QueryString;

                    string responseString = "<script>location.href = 'about:blank';</script>";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);
                    var response = ctx.Response;
                    response.ContentLength64 = responseBytes.Length;
                    response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                    response.OutputStream.Close();

                    string requestState = queryString.Get("state");
                    if (requestState == state)
                    {
                        string code = queryString.Get("code");
                        Token = OauthProvider.AcquireToken(code, RedirectUri);
                        break;
                    }
                }
                listener.Stop();
                
                Invoke(() =>
                {
                    DialogResult = DialogResult.OK;
                    Close();
                });
            });
        }

        private string RedirectUri => $"http://127.0.0.1:{port}/";

        private void OpenOauthUrl()
        {
            Process.Start(OauthProvider.OauthUrl(state, RedirectUri));
        }
        
        private static int GetUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private void FAuthorize_FormClosed(object sender, FormClosedEventArgs e)
        {
            listener?.Abort();
        }
    }
}
