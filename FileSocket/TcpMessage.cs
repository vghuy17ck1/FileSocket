namespace FileSocket
{
    public class TcpMessage
    {
        private static string tagBegin = "<BEGIN>";
        private static string tagEnd = "<END>";

        public static string Parse(string message)
        {
            int length = message.LastIndexOf(tagEnd) - tagBegin.Length;
            return message.Substring(tagBegin.Length, length);
        }

        public static string Pack(string content)
        {
            return tagBegin + content + tagEnd;
        }
    }
}
