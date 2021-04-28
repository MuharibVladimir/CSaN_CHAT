using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class Message
    {
        public const char CONNECTION = '0';
        public const char DISCONNECTION = '1';
        public const char MESSAGE = '2';
        public const char GET_HISTORY = '3';
        public const char SHOW_HISTORY = '4';
        public char code { get; }
        public string data { get; }

        public Message(char messageCode, string messageData)
        {
            code = messageCode;
            data = messageData;
        }
    }
}
