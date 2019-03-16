using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NAPS2.Logging;
using NAPS2.Util;

namespace NAPS2.Worker
{
    public static class GrpcHelper
    {
        public static void HandleErrors(Error error)
        {
            if (!string.IsNullOrEmpty(error?.Xml))
            {
                var knownExceptionTypes = new Type[] { }; // TODO
                var exception = error.Xml.FromXml<Exception>();
                exception.PreserveStackTrace();
                throw exception;
            }
            else if (!string.IsNullOrEmpty(error?.Name))
            {
                throw new Exception($"GRPC endpoint error {error.Name}: {error.Message}");
            }
        }

        private static Error ToError(Exception e)
        {
            var error = new Error();
            try
            {
                error.Xml = e.ToXml();
            }
            catch (Exception serializerEx)
            {
                Log.ErrorException("Error serializing exception object", serializerEx);
                Log.ErrorException("Original exception", e);
            }
            error.Name = e.GetType().Name;
            error.Message = e.Message;
            return error;
        }

        public static Task<TResponse> WrapFunc<TResponse>(Func<TResponse> action, Func<Error, TResponse> error) =>
            Task.Run(() =>
            {
                try
                {
                    return action();
                }
                catch (Exception e)
                {
                    return error(ToError(e));
                }
            });

        public static Task<TResponse> WrapFunc<TResponse>(Func<Task<TResponse>> action, Func<Error, TResponse> error) =>
            Task.Run(async () =>
            {
                try
                {
                    return await action();
                }
                catch (Exception e)
                {
                    return error(ToError(e));
                }
            });

        public static Task WrapAction(Action action, Action<Error> error) =>
            Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    error(new Error { Xml = e.ToXml() });
                }
            });
    }
}
