using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    return error(new Error { Xml = e.ToXml() });
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
                    return error(new Error { Xml = e.ToXml() });
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
