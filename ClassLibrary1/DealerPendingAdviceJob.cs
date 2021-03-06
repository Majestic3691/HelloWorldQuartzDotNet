﻿using Common.Logging;
using Quartz;
using System;

namespace RFFCScheduledJobs
{
    public class DealerPendingAdviceJob : IJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DealerPendingAdviceJob));

        public DealerPendingAdviceJob()
        {

        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Log.DebugFormat("{0}****{0}Job {1} fired @ {2} next scheduled for {3}{0}***{0}",
                                                                        Environment.NewLine,
                                                                        context.JobDetail.Key,
                                                                        context.FireTimeUtc.Value.ToString("r"),
                                                                        context.NextFireTimeUtc.Value.ToString("r"));


                Log.DebugFormat("{0}***{0}Dealer Pending Advice Report goes HERE!{0}***{0}", Environment.NewLine);
            }
            catch (Exception ex)
            {
                Log.DebugFormat("{0}***{0}Failed: {1}{0}***{0}", Environment.NewLine, ex.Message);
            }
        }


    }
}
