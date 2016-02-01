using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorldQuartzDotNet
{
    public interface IScheduledJob
    {
        void Run();
    }
}
