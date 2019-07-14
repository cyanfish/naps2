using System;

namespace NAPS2.Util
{
    /// <summary>
    /// A base interface for objects capable of displaying error output.
    /// </summary>
    public abstract class ErrorOutput
    {
        private static ErrorOutput _default = new StubErrorOutput();

        public static ErrorOutput Default
        {
            get
            {
                TestingContext.NoStaticDefaults();
                return _default;
            }
            set => _default = value;
        }

        public abstract void DisplayError(string errorMessage);

        public abstract void DisplayError(string errorMessage, string details);

        public abstract void DisplayError(string errorMessage, Exception exception);
    }
}
