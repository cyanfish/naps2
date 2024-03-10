using System.Reflection;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Remoting;

internal static class RemotingHelper
{
    public static void HandleErrors(Error error)
    {
        if (error != null && !string.IsNullOrEmpty(error.Type))
        {
            var exception = MakeExceptionObject(error);
            exception.PreserveStackTrace();
            throw exception;
        }
    }

    private static Exception MakeExceptionObject(Error error)
    {
        var exceptionType = Assembly.GetAssembly(typeof(ScanDriverException))!.GetType(error.Type, false);
        var exception = CreateExceptionType(exceptionType);
        var messageField =
            typeof(Exception).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);
        var stackTraceField = typeof(Exception).GetField("_stackTraceString",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var innerExceptionField = typeof(Exception).GetField("_innerException",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var typePrefix = exceptionType == null ? $"{error.Type}: " : "";
        messageField?.SetValue(exception, typePrefix + error.Message);
        stackTraceField?.SetValue(exception, error.StackTrace);
        if (error.InnerException != null)
        {
            innerExceptionField?.SetValue(exception, MakeExceptionObject(error.InnerException));
        }
        return exception;
    }

    private static Exception CreateExceptionType(Type? exceptionType)
    {
        if (exceptionType != null && typeof(ScanDriverException).IsAssignableFrom(exceptionType))
        {
            try
            {
                return (Exception) Activator.CreateInstance(exceptionType)!;
            }
            catch (Exception)
            {
                // If the exception is not constructable, just use the default Exception type
            }
        }
        return new Exception();
    }

    public static Error ToError(Exception e) =>
        new()
        {
            Type = e.GetType().FullName,
            Message = e.Message,
            StackTrace = e.StackTrace,
            InnerException = e.InnerException != null ? ToError(e.InnerException) : null
        };
}