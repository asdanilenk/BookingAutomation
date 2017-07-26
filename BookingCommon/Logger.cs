using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCommon
{
    public class Logger
    {
        StreamWriter file;
        public Logger(String name)
        {
            file = new StreamWriter(@"C:\screen\log.txt", true);
            file.WriteLine($"Starting script {name} at {DateTime.Now}");
        }

        public void WriteLine(String line)
        {
            if (file != null)
            {
                file.WriteLine(line);
            }
            Console.WriteLine(line);
        }

        public void Close()
        {
            if (file != null)
            {
                file.Flush();
                file.Close();
                file = null;
            }
        }
    }
}
