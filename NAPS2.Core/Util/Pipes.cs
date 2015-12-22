using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NAPS2.Util
{
    public static class Pipes
    {
        public const string MSG_SCAN_WITH_DEVICE = "SCAN_WDEV_";
        public const string MSG_KILL_PIPE_SERVER = "KILL_PIPE_SERVER";

        private const string PIPE_NAME = "NAPS2_PIPE_86a6ef67-742a-44ec-9ca5-64c5bddfd013";
        private const int TIMEOUT = 1000;

        private static bool _serverRunning;

        public static void SendMessage(string msg)
        {
            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.Out))
                {
                    //MessageBox.Show("Sending msg:" + msg);
                    pipeClient.Connect(TIMEOUT);
                    var streamString = new StreamString(pipeClient);
                    streamString.WriteString(msg);
                    //MessageBox.Show("Sent");
                }
            }
            catch (Exception e)
            {
                Log.ErrorException("Error sending message through pipe", e);
            }
        }

        public static void StartServer(Action<string> callback)
        {
            if (_serverRunning)
            {
                return;
            }
            var thread = new Thread(() =>
            {
                try
                {
                    using (var pipeServer = new NamedPipeServerStream(PIPE_NAME, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances))
                    {
                        while (true)
                        {
                            pipeServer.WaitForConnection();
                            var streamString = new StreamString(pipeServer);
                            var msg = streamString.ReadString();
                            //MessageBox.Show("Received msg:" + msg);
                            if (msg == MSG_KILL_PIPE_SERVER)
                            {
                                break;
                            }
                            callback(msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException("Error in named pipe server", ex);
                }
                _serverRunning = false;
            });
            _serverRunning = true;
            thread.Start();
        }

        public static void KillServer()
        {
            if (_serverRunning)
            {
                SendMessage(MSG_KILL_PIPE_SERVER);
            }
        }

        private class StreamString
        {
            private Stream ioStream;
            private UnicodeEncoding streamEncoding;

            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = new UnicodeEncoding();
            }

            public string ReadString()
            {
                int len;
                len = ioStream.ReadByte() * 256;
                len += ioStream.ReadByte();
                byte[] inBuffer = new byte[len];
                ioStream.Read(inBuffer, 0, len);

                return streamEncoding.GetString(inBuffer);
            }

            public int WriteString(string outString)
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();

                return outBuffer.Length + 2;
            }
        }
    }
}
