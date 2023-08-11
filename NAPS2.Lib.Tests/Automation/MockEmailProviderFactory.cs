using NAPS2.ImportExport.Email;
using NAPS2.Sdk.Tests;
using NSubstitute;

namespace NAPS2.Lib.Tests.Automation;

public class MockEmailProviderFactory : IEmailProviderFactory
{
    private Exception _assertException;

    public MockEmailProviderFactory(Action<EmailMessage> messageAsserts)
    {
        EmailProviderMock.SendEmail(Arg.Any<EmailMessage>(), Arg.Any<ProgressHandler>())
            .Returns(x =>
            {
                var message = (EmailMessage) x[0];
                try
                {
                    messageAsserts.Invoke(message);
                }
                catch (Exception ex)
                {
                    ex.PreserveStackTrace();
                    _assertException = ex;
                }
                return Task.FromResult(true);
            });
    }

    public IEmailProvider Create(EmailProviderType type) => EmailProviderMock;

    public IEmailProvider Default => EmailProviderMock;

    public IEmailProvider EmailProviderMock { get; } = Substitute.For<IEmailProvider>();

    public void CheckAsserts()
    {
        if (_assertException != null)
        {
            throw _assertException;
        }
    }

    public void VerifyExactlyOneMessageSent()
    {
        EmailProviderMock.Received().SendEmail(Arg.Any<EmailMessage>(), Arg.Any<ProgressHandler>());
        EmailProviderMock.ReceivedCallsCount(1);
    }
}