using System;
using Common.Logging;
using System.Threading;

namespace HelloWorldQuartzDotNet
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                while (true)
                {
                    var sj = new ScheduledJob();
                    sj.Run();

                    Console.WriteLine(@"{0}Check Quartz.net\Trace\application.log.txt for Job updates{0}",
                                        Environment.NewLine);

                    Console.WriteLine("{0}Press Ctrl^C to close the window. The job will continue " +
                                        "to run via Quartz.Net windows service, " +
                                        "see job activity in the Quartz.Net Trace file...{0}",
                                        Environment.NewLine);

                    Thread.Sleep(10000 * 100000);    
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception while scheduling job", ex);
                Console.WriteLine("Failed: Message: {0} Inner: {1}", ex.Message, ex.InnerException);
                Console.ReadKey();
            }
        }
    }
}
