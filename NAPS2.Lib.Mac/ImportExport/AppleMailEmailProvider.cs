using NAPS2.ImportExport.Email;

namespace NAPS2.ImportExport;

internal class AppleMailEmailProvider : IAppleMailEmailProvider
{
    [DllImport("/System/Library/Frameworks/CoreServices.framework/CoreServices")]
    static extern IntPtr LSCopyDefaultHandlerForURLScheme(IntPtr urlScheme);

    public bool ShowInList => true;

    // Apple Mail only works if it's set as the default email reader
    public bool CanSelectInList
    {
        get
        {
            using (var scheme = new NSString("mailto"))
            {
                IntPtr resultPtr = LSCopyDefaultHandlerForURLScheme(scheme.Handle);
                if (resultPtr != IntPtr.Zero)
                {
                    var bundleId = Runtime.GetNSObject<NSString>(resultPtr);
                    return bundleId == "com.apple.mail";
                }
            }
            return false;
        }
    }

    public Task<bool> SendEmail(EmailMessage emailMessage, ProgressHandler progress = default)
    {
        return Task.Run(async () =>
        {
            EmailServiceDelegate d = null!;
            Invoker.Current.Invoke(() =>
            {
                var service = NSSharingService.GetSharingService(NSSharingServiceName.ComposeEmail);
                if (service == null)
                {
                    throw new InvalidOperationException("Could not get ComposeEmail sharing service");
                }
                if (emailMessage.Subject != null)
                {
                    service.Subject = emailMessage.Subject;
                }
                if (emailMessage.Recipients.Any())
                {
                    service.Recipients = emailMessage.Recipients.Select(x => (NSObject) new NSString(x.Address))
                        .ToArray();
                }
                var items = new List<NSObject>();
                if (emailMessage.BodyText != null)
                {
                    items.Add(new NSString(emailMessage.BodyText));
                }
                foreach (var attachment in emailMessage.Attachments)
                {
                    items.Add(NSUrl.FromFilename(attachment.FilePath));
                }
                d = new EmailServiceDelegate();
                service.Delegate = d;
                service.PerformWithItems(items.ToArray());
            });
            return await d.Task;
        });
    }

    private class EmailServiceDelegate : NSSharingServiceDelegate
    {
        private readonly TaskCompletionSource<bool> _tcs = new();

        public override void DidShareItems(NSSharingService sharingService, NSObject[] items)
        {
            _tcs.SetResult(true);
        }

        public override void DidFailToShareItems(NSSharingService sharingService, NSObject[] items, NSError error)
        {
            _tcs.SetResult(false);
        }

        public Task<bool> Task => _tcs.Task;
    }
}