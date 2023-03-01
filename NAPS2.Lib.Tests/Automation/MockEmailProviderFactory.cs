using Moq;
using NAPS2.ImportExport.Email;

namespace NAPS2.Lib.Tests.Automation;

public class MockEmailProviderFactory : IEmailProviderFactory
{
    private Exception _assertException;

    public MockEmailProviderFactory(Action<EmailMessage> messageAsserts)
    {
        EmailProviderMock.Setup(x => x.SendEmail(It.IsAny<EmailMessage>(), It.IsAny<ProgressHandler>()))
            .Returns((EmailMessage message, ProgressHandler _) =>
            {
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

    public IEmailProvider Create(EmailProviderType type) => EmailProviderMock.Object;

    public IEmailProvider Default => EmailProviderMock.Object;

    public Mock<IEmailProvider> EmailProviderMock { get; } = new();

    public void CheckAsserts()
    {
        if (_assertException != null)
        {
            throw _assertException;
        }
    }

    public void VerifyExactlyOneMessageSent()
    {
        EmailProviderMock.Verify(x => x.SendEmail(It.IsAny<EmailMessage>(), It.IsAny<ProgressHandler>()));
        EmailProviderMock.VerifyNoOtherCalls();
    }
}