using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging.Configuration;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using HelloWorldQuartzDotNet;

namespace RFFCFinancialCommon
{

    public class JobSchedule
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }
        public string TriggerType { get; set; }
        public string TriggerState { get; set; }
        public int Priority { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime NextFire { get; set; }
        public DateTime LastFire { get; set; }
    }

    public class GroupStatus
    {
        public string Group { get; set; }
        public bool IsJobGroupPaused { get; set; }
        public bool IsTriggerGroupPaused { get; set; }
    }

    public class QuartzCommon
    {

        /// <summary>
        /// This method retrieves meta data from a quartz scheduler.
        /// </summary>
        /// <param name="qScheduler"></param>
        /// <returns>string</returns>
        public string GetMetaData(IScheduler qScheduler)
        {
            var metaData = qScheduler.GetMetaData();

            return string.Format("{0}Name: '{1}'{0}InstanceID: '{2}'{0}Version: '{3}'{0}ThreadPoolSize: '{4}'{0}IsRemote: '{5}'{0}JobStoreName: '{6}'{0}SupportsPersistance: '{7}'{0}IsClustered: '{8}'{0}StandbyMode: '{9}'{0}NumberOfJobsExecuted: '{10}'{0}RunningSince: '{11}'",
                Environment.NewLine,
                metaData.SchedulerName,
                metaData.SchedulerInstanceId,
                metaData.Version,
                metaData.ThreadPoolSize,
                metaData.SchedulerRemote,
                metaData.JobStoreType.Name,
                metaData.JobStoreSupportsPersistence,
                metaData.JobStoreClustered,
                metaData.InStandbyMode,
                metaData.NumberOfJobsExecuted,
                metaData.RunningSince);
        }


        /// <summary>
        /// This method unschedules all jobs in a specified scheduler.
        /// </summary>
        /// <param name="qScheduler"></param>
        /// <returns>bool</returns>
        public bool UnscheduleAll(IScheduler qScheduler)
        {
            foreach (var group in qScheduler.GetTriggerGroupNames())
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = qScheduler.GetJobKeys(groupMatcher);

                foreach (var triggers in jobKeys.Select(jobKey => qScheduler.GetTriggersOfJob(jobKey)))
                {
                    return qScheduler.UnscheduleJobs(triggers.Select(t => t.Key).ToList());
                }
            }
            return false;
        }

        /// <summary>
        /// This method deletes all jobs in a specified scheduler.
        /// </summary>
        /// <param name="qScheduler"></param>
        public void DeleteAll(IScheduler qScheduler)
        {
            qScheduler.Clear();
        }

        /// <summary>
        /// This method places the scheduler in Stand-by Mode.
        /// </summary>
        /// <param name="qScheduler"></param>
        public void StandBy(IScheduler qScheduler)
        {
            if (!qScheduler.IsStarted || !qScheduler.IsShutdown)
            {
                qScheduler.Standby();
            }
        }

        /// <summary>
        /// This method places the scheduler in shutdown mode.
        /// True - wait for jobs to complete
        /// False - immediate shutdown
        /// </summary>
        /// <param name="qScheduler"></param>
        public void Shutdown(IScheduler qScheduler, bool bWait)
        {

            if (qScheduler.IsStarted)
            {
                qScheduler.Shutdown(bWait);
            }
        }



        /// <summary>
        /// This method reschedules a job.
        /// </summary>
        /// <param name="qScheduler"></param>
        /// <param name="sJobName"></param>
        /// <param name="sJobGroup"></param>
        /// <param name="sCronExpression"></param>
        /// <param name="iPriority"></param>
        public void RescheduleJob(IScheduler qScheduler, string sJobName, string sJobGroup, string sCronExpression, int iPriority)
        {
            // Build new trigger
            var trigger = (ICronTrigger)TriggerBuilder.Create()
                .WithIdentity(sJobName, sJobGroup)
                .WithCronSchedule(sCronExpression)
                .WithPriority(iPriority)
                //.StartAt(StartAt.ToUniversalTime())
                .Build();

            qScheduler.RescheduleJob(new TriggerKey(sJobName, sJobGroup), trigger);
        }


        /// <summary>
        /// This method retrieves a handle to a remote scheduler.
        /// </summary>
        /// <param name="sServer"></param>
        /// <param name="iPort"></param>
        /// <param name="sScheduler"></param>
        /// <param name="sInstanceName"></param>
        /// <returns>IScheduler</returns>
        public IScheduler GetScheduler(string sServer, int iPort, string sScheduler, string sInstanceName)
        {
            try
            {
                System.Collections.Specialized.NameValueCollection nvcProperties = new System.Collections.Specialized.NameValueCollection();
                nvcProperties["quartz.scheduler.instanceName"] = sInstanceName;
                //                properties["quartz.scheduler.instanceName"] = "RFFCQuartzScheduler";
                // set remoting expoter
                nvcProperties["quartz.scheduler.proxy"] = "true";
                //                properties["quartz.scheduler.proxy.address"] = string.Format("tcp://{0}:{1}/{2}", "25.64.10.83", "555", "QuartzScheduler");
                nvcProperties["quartz.scheduler.proxy.address"] = string.Format("tcp://{0}:{1}/{2}", sServer, iPort, sScheduler);
                // set thread pool info
                nvcProperties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
                nvcProperties["quartz.threadPool.threadCount"] = "5";
                nvcProperties["quartz.threadPool.threadPriority"] = "Normal";

                // Get a reference to the scheduler
                var sf = new StdSchedulerFactory(nvcProperties);

                return sf.GetScheduler();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Scheduler not available: '{0}'", ex.Message);
                throw;
            }
        }


        public bool UnscheduleJob(IScheduler qScheduler, string sJobName, string sJobGroup, string sCronExpression, int iPriority)
        {
            var jobKey = new JobKey(sJobName, sJobGroup);

            if (qScheduler.CheckExists(jobKey))
            {
                return qScheduler.UnscheduleJob(new TriggerKey(sJobName, sJobGroup));
            }
            return false;
        }


        public List<GroupStatus> GetGroups(IScheduler qScheduler)
        {
            var results = new List<GroupStatus>();
            foreach (var gp in qScheduler.GetJobGroupNames())
            {
                results.Add(new GroupStatus()
                {
                    Group = gp,
                    IsJobGroupPaused = qScheduler.IsJobGroupPaused(gp),
                    IsTriggerGroupPaused = qScheduler.IsTriggerGroupPaused(gp)
                });
            }
            return results;
        }


        public List<JobSchedule> GetSchedules(IScheduler qScheduler)
        {
            var jcs = new List<JobSchedule>();

            foreach (var group in qScheduler.GetJobGroupNames())
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = qScheduler.GetJobKeys(groupMatcher);

                foreach (var jobKey in jobKeys)
                {
                    var triggers = qScheduler.GetTriggersOfJob(jobKey);
                    foreach (var trigger in triggers)
                    {
                        var js = new JobSchedule();
                        js.Name = jobKey.Name;
                        js.Group = jobKey.Group;
                        js.TriggerType = trigger.GetType().Name;
                        js.TriggerState = qScheduler.GetTriggerState(trigger.Key).ToString();
                        js.Priority = trigger.Priority;

                        DateTimeOffset? startTime = trigger.StartTimeUtc;
                        js.StartTime = TimeZone.CurrentTimeZone.ToLocalTime(startTime.Value.DateTime);

                        DateTimeOffset? nextFireTime = trigger.GetNextFireTimeUtc();
                        if (nextFireTime.HasValue)
                        {
                            js.NextFire = TimeZone.CurrentTimeZone.ToLocalTime(nextFireTime.Value.DateTime);
                        }

                        DateTimeOffset? previousFireTime = trigger.GetPreviousFireTimeUtc();
                        if (previousFireTime.HasValue)
                        {
                            js.LastFire = TimeZone.CurrentTimeZone.ToLocalTime(previousFireTime.Value.DateTime);
                        }

                        jcs.Add(js);
                    }
                }
            }
            return jcs;
        }


        public List<JobSchedule> GetSchedules(IScheduler qScheduler, string groupName)
        {
            var jcs = new List<JobSchedule>();

            var groupMatcher = GroupMatcher<JobKey>.GroupContains(groupName);
            var jobKeys = qScheduler.GetJobKeys(groupMatcher);

            foreach (var jobKey in jobKeys)
            {
                var triggers = qScheduler.GetTriggersOfJob(jobKey);
                foreach (var trigger in triggers)
                {
                    var js = new JobSchedule();
                    js.Name = jobKey.Name;
                    js.Description = trigger.Description;
                    js.Group = jobKey.Group;
                    js.TriggerType = trigger.GetType().Name;
                    js.TriggerState = qScheduler.GetTriggerState(trigger.Key).ToString();
                    js.Priority = trigger.Priority;

                    DateTimeOffset? startTime = trigger.StartTimeUtc;
                    js.StartTime = TimeZone.CurrentTimeZone.ToLocalTime(startTime.Value.DateTime);

                    DateTimeOffset? nextFireTime = trigger.GetNextFireTimeUtc();
                    if (nextFireTime.HasValue)
                    {
                        js.NextFire = TimeZone.CurrentTimeZone.ToLocalTime(nextFireTime.Value.DateTime);
                    }

                    DateTimeOffset? previousFireTime = trigger.GetPreviousFireTimeUtc();
                    if (previousFireTime.HasValue)
                    {
                        js.LastFire = TimeZone.CurrentTimeZone.ToLocalTime(previousFireTime.Value.DateTime);
                    }

                    jcs.Add(js);
                }
            }
            return jcs;
        }


    }
}
