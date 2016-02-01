using System.Collections.Specialized;
using System.Configuration;
using Quartz;
using System;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System.Linq;
using System.Collections.Generic;
using Common.Logging;
using RFFCFinancialCommon;
//using Rte.Model.Entities;

namespace HelloWorldQuartzDotNet
{
    class ScheduledJob : IScheduledJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScheduledJob));

        string sServer = "25.64.10.83";
        int iPort = 555;
        string sScheduler ="QuartzScheduler";
        string sInstanceName = "RFFCQuartzScheduler";
        QuartzCommon quartzCommon = new QuartzCommon();

        public void Run()
        {

            ScheduleHelloWorld(sServer, iPort, sScheduler, sInstanceName);
            ScheduleSample(sServer, iPort, sScheduler, sInstanceName);

        }

//            // Get an instance of the Quartz.Net scheduler
//            var qScheduler = quartzCommon.GetScheduler(sServer, iPort, sScheduler, sInstanceName);

            //            // Start the scheduler if its in standby
            //            if (!qScheduler.IsStarted)
            //            {
            //                Console.WriteLine("STARTING scheduler: {0}", qScheduler.SchedulerName);
            //                qScheduler.Start();
            //            }
            //            else
            //            {
            //                Console.WriteLine("Scheduler: {0} is already RUNNING at {1}", qScheduler.SchedulerName, quartzCommon.GetMetaData(qScheduler));
            ////                Console.WriteLine("Scheduler: {0} running jobs: {1}" schd.SchedulerName, schd.GetCurrentlyExecutingJobs)
            //            }

            //            var jobHelloWorld = CreateHelloWorldJob();
            //            //// Define the Job to be scheduled
            //            //var job = JobBuilder.Create<HelloWorldJob>()
            //            //    .WithIdentity("WriteHelloToConsole", "IT")
            //            //    .RequestRecovery()
            //            //    .Build();

            //            var triggerHelloWorld = CreateHelloWorldTrigger();
            //            //// Associate a trigger with the Job
            //            //var trigger = (ICronTrigger)TriggerBuilder.Create()
            //            //    .WithIdentity("WriteHelloToConsole", "IT")
            //            //    .WithCronSchedule("0 0/1 * 1/1 * ? *") // visit http://www.cronmaker.com/ Queues the job every minute
            //            //    .StartAt(DateTime.UtcNow)
            //            //    .WithPriority(1)
            //            //    .Build();
            //            if (qScheduler.CheckExists(new JobKey("HelloWorldJob", "IT")))
            //            {
            //                qScheduler.DeleteJob(new JobKey("HelloWorldJob", "IT"));
            //            }



            //            //Console.WriteLine("{0} Jobs are scheduled for '{1}'", qScheduler.get"WriteHelloToConsole", schedule.ToString("r"));
            //            //Console.WriteLine("Job '{0}' scheduled for '{1}'", "WriteHelloToConsole", qScheduler.ToString("r"));

            //            //Validate the sample job is no longer resident
            //            if (qScheduler.CheckExists(new JobKey("sampleJob", "sampleGroup")))
            //            {
            //                qScheduler.DeleteJob(new JobKey("sampleJob", "sampleGroup"));
            //            }
            //            // Validate that the job doesn't already exists
            //            if (qScheduler.CheckExists(new JobKey("WriteHelloToConsole", "IT")))
            //            {
            //                qScheduler.DeleteJob(new JobKey("WriteHelloToConsole", "IT"));
            //            }
            //            var schedule = qScheduler.ScheduleJob(job, trigger);
            //            Console.WriteLine("Job '{0}' scheduled for '{1}'", "WriteHelloToConsole", schedule.ToString("r"));

            //            // schd.Start();
            //        }

        private void ScheduleSample(string sServer, int iPort, string sScheduler, string sInstanceName)
        {
            // Get an instance of the Quartz.Net scheduler
            var qScheduler = quartzCommon.GetScheduler(sServer, iPort, sScheduler, sInstanceName);

            // Start the scheduler if its in standby
            if (!qScheduler.IsStarted)
            {
                Console.WriteLine("STARTING scheduler: {0}", qScheduler.SchedulerName);
                qScheduler.Start();
            }
            else
            {
                Console.WriteLine("Scheduler: {0} is already RUNNING at {1}", qScheduler.SchedulerName, quartzCommon.GetMetaData(qScheduler));
                //                Console.WriteLine("Scheduler: {0} running jobs: {1}" schd.SchedulerName, schd.GetCurrentlyExecutingJobs)
            }

            var jobSample = CreateSampleJob();
            var triggerSample = CreateHelloWorldTrigger(jobSample);

            //Validate the sample job is no longer resident
            if (qScheduler.CheckExists(new JobKey("sampleJob", "sampleGroup")))
            {
                qScheduler.DeleteJob(new JobKey("sampleJob", "sampleGroup"));
            }
            var schedule = qScheduler.ScheduleJob(jobSample, triggerSample);
            Console.WriteLine("Job '{0}' scheduled for '{1}'", "sampleJob", schedule.ToString("r"));

            // schd.Start();
        }

        private IJobDetail CreateSampleJob()
        {
            var job = JobBuilder.Create<HelloWorldJob>()
                .WithIdentity("Sample", "SampleGroup")
                .RequestRecovery()
                .Build();
            return job;
        }
        private ICronTrigger CreateSampleTrigger()
        {
            // Associate a trigger with the Job
            var trigger = (ICronTrigger)TriggerBuilder.Create()
                .WithIdentity("Sample", "SampleGroup")
                .WithCronSchedule("0 0/1 * 1/1 * ? *") // visit http://www.cronmaker.com/ Queues the job every minute
                .StartAt(DateTime.UtcNow)
                .WithPriority(1)
                .Build();
            return trigger;
        }


        private void ScheduleHelloWorld(string sServer, int iPort, string sScheduler, string sInstanceName)
        {
            // Get an instance of the Quartz.Net scheduler
            var qScheduler = quartzCommon.GetScheduler(sServer, iPort, sScheduler, sInstanceName);

            // Start the scheduler if its in standby
            if (!qScheduler.IsStarted)
            {
                Console.WriteLine("STARTING scheduler: {0}", qScheduler.SchedulerName);
                qScheduler.Start();
            }
            else
            {
                Console.WriteLine("Scheduler: {0} is already RUNNING at {1}", qScheduler.SchedulerName, quartzCommon.GetMetaData(qScheduler));
//                Console.WriteLine("Scheduler: {0} running jobs: {1}" schd.SchedulerName, schd.GetCurrentlyExecutingJobs)
            }

            var jobHelloWorld = CreateHelloWorldJob();

            JobDataMap map = jobHelloWorld.JobDataMap;
            map.Put("msg", "Your remotely added job has executed!");

            var triggerHelloWorld = CreateHelloWorldTrigger(jobHelloWorld);
            if (qScheduler.CheckExists(new JobKey("HelloWorldJob", "IT")))
            {
                qScheduler.DeleteJob(new JobKey("HelloWorldJob", "IT"));
            }
            var schedule = qScheduler.ScheduleJob(jobHelloWorld, triggerHelloWorld);
            Console.WriteLine("Job '{0}' scheduled for '{1}'", "WriteHelloToConsole", schedule.ToString("r"));
            
            // schd.Start();
        }

        private IJobDetail CreateHelloWorldJob()
        {
            var job = JobBuilder.Create<HelloWorldJob>()
                .WithIdentity("WriteHelloToConsole", "IT")
                .RequestRecovery()
                .Build();
            return job;
        }
        private ICronTrigger CreateHelloWorldTrigger(IJobDetail ijdJob)
        {
            // Associate a trigger with the Job
            var trigger = (ICronTrigger)TriggerBuilder.Create()
                .WithIdentity("WriteHelloToConsole", "IT")
                .ForJob(ijdJob.Key)
                .WithCronSchedule("0 0/1 * 1/1 * ? *") // visit http://www.cronmaker.com/ Queues the job every minute
                .StartAt(DateTime.UtcNow)
                .WithPriority(1)
                .Build();
            return trigger;
        }


    }
}