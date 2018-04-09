using System;
using System.Collections.Generic;
using System.Text;

namespace SardineFish.MTTask
{
    public static class LogUtility
    {
        public static void PrintProgressBar(double progress, int length)
        {
            Console.Write("[");
            int prgr = (int)(progress * length);
            for (var i = 0; i < prgr; i++)
            {
                Console.Write("=");
            }
            var left = length - prgr;
            if (left > 0)
            {
                Console.Write(">");
                left -= 1;
            }
            for (var i = 0; i < left; i++)
                Console.Write(" ");
            Console.Write("]" + (progress * 100).ToString("f2") + "%");
        }

        public static int PrintScrollText(string text, int maxLength, int startPos = 0)
        {
            if (text.Length < maxLength)
            {
                Console.Write(text);
                return 0;
            }
            text += "     ";
            StringBuilder sb = new StringBuilder();
            for (var i = startPos; i < startPos + maxLength; i++)
            {
                sb.Append(text[i % text.Length]);
            }
            Console.Write(sb.ToString());
            return (startPos + 1) % text.Length;
        }
    }
}
