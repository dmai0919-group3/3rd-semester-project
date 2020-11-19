using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group3.Semester3.WebApp.Helpers
{
    public static class Messenger
    {
        public static List<string> Messages { get; private set; }
        
        static Messenger()
        {
            Messages = new List<string>();
        }

        public static void addMessage(string message)
        {
            Messages.Add(message);
        }
    }
}
