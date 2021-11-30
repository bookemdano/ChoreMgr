using ChoreMgr.Models;
using MongoDB.Driver;

namespace ChoreMgr.Data
{
    public class ChoreService
    {
        private readonly IMongoCollection<Job> _jobs;
        private readonly IMongoCollection<JobLog> _jobLogs;
        private IChoreDatabaseSettings _settings;

        public ChoreService(IChoreDatabaseSettings settings)
        {
            _settings = settings;
            var client = new MongoClient(settings.ConnectionString);
            var db = client.GetDatabase(settings.DatabaseName);

            _jobs = db.GetCollection<Job>(JobTableName);
            _jobLogs = db.GetCollection<JobLog>(JobTableName + "Log");
        }
        public string JobTableName
        {
            get
            {
                var jobTableName = "Job";
                if (UseDevTables)
                    jobTableName = "dev_" + jobTableName;
    
                return jobTableName;
            }
        }

        public bool UseDevTables
        {
            get
            {
                return _settings.UseDevTables;
            }
        }

        #region Job table

        public List<Job> GetJobs() => _jobs.Find(job => true).ToList();

        public Job GetJob(string id) => _jobs.Find<Job>(job => job.Id == id).FirstOrDefault();

        public Job CreateJob(Job job)
        {
            AddToJobLog(job, null);
            _jobs.InsertOne(job);
            LogAllJobs();
            return job;
        }

        public void UpdateJob(string id, Job job)
        {
            var foundJob = GetJob(id);
            AddToJobLog(job, foundJob);
            if (job.IntervalDays == null && job.LastDone != null)
                RemoveJob(job);
            else
                _jobs.ReplaceOne(j => j.Id == id, job);
    
            LogAllJobs();
        }
        public void RemoveJob(Job job)
        {
            AddToJobLog(null, job);
            _jobs.DeleteOne(j => j.Id == job.Id);
            LogAllJobs();
        }

        public void RemoveJob(string id)
        {
            RemoveJob(GetJob(id));
        }

        #endregion

        internal ChoreService CloneProd()
        {
            var prodSettings = new ChoreDatabaseSettings(_settings);
            prodSettings.UseDevTables = false;
            return new ChoreService(prodSettings);
        }

        #region JobLog table

        public List<JobLog> GetJobLogs() => _jobLogs.Find(j => true).ToList();

        public JobLog CreateJobLog(JobLog jobLog)
        {
            _jobLogs.InsertOne(jobLog);
            return jobLog;
        }

        public void RemoveJobLog(string id) => _jobLogs.DeleteOne(j => j.Id == id);

        #endregion

        #region Logging
        void AddToJobLog(Job? job, Job? oldJob)
        {
            var jobLog = new JobLog();
            jobLog.Updated = DateTime.Now;
            jobLog.JobId = job?.Id ?? oldJob?.Id;
            jobLog.JobName = job?.Name ?? oldJob?.Name;
            jobLog.Note = Job.DeltaString(oldJob, job);
            DanLogger.Log($"updated:{jobLog.Updated} id:{jobLog.JobId} name:{jobLog.JobName} note:{jobLog.Note}");
            CreateJobLog(jobLog);
        }
        
        // this is just because I don't trust Mongo
        private void LogAllJobs()
        {
            var outs = new List<string>();
            outs.Add($"Id,Name,IntervalDays,LastDone,NextDo");
            var jobs = GetJobs();
            foreach (var job in jobs)
                outs.Add($"{job.Id},{job.Name},{job.IntervalDays},{job.LastDone?.ToShortDateString()},{job.NextDo?.ToShortDateString()}");
            File.WriteAllLines($"{JobTableName} {DateTime.Today.ToString("yyyy-MM-dd")}.csv", outs);
        }
        #endregion 
    }
}
