using System;

namespace prisma.core
{
    public class ExceptionToken : Exception
    {
        public int CodeHttp { get; set; }
        public ExceptionToken(string msg, int codeHttp) : base(msg)
        {
            CodeHttp = codeHttp;
        }
    }
}
