using System;

namespace NAPS2.Config
{
    public class StubConfigProvider<TConfig> : IConfigProvider<TConfig>
    {
        private readonly TConfig _config;

        public StubConfigProvider(TConfig config)
        {
            _config = config;
        }

        // TODO: This is broken (for SDK users, who are the only ones using this class).
        // TODO: Config providers must not return null but the stub object might have null properties.
        // TODO: A couple possible solutions:
        // - Have defined defaults (e.g. PdfMetadata.Default) with non-null properties, that are used as a backup parameter in this stub.
        // - Implement materialization from IConfigProvider<T> to T, and use that to replace/augment the Child() extension.
        //   This would be preferable in that it exposes the cleanest API. It would be different behavior in that it's a snapshot, but that's probably a good thing.
        // Side note, PdfSettings should be subdivided into PdfExportOptions (that excludes ui-only settings). And evaluate similar child configs.
        public T Get<T>(Func<TConfig, T?> func) where T : class => func(_config);// ?? throw new InvalidOperationException();

        public T Get<T>(Func<TConfig, T?> func) where T : struct => func(_config) ?? default; // ?? throw new InvalidOperationException();
    }
}
