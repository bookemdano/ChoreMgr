using ChoreMgr.Models;
using ChoreMgr.Utils;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace ChoreMgr.Data
{
    // mongo in container https://www.mongodb.com/compatibility/docker#:~:text=Running%20MongoDB%20as%20a%20Docker%20Container%20You%20can,version%20in%20detached%20mode%20%28as%20a%20background%20process%29.
    // docker run --name mongodb -d -p 27018:27017 -v F:\OneDrive\data\mongo\ChoreDB:/data/db mongo
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

        public override string ToString()
        {
            return $"Name:{_settings.DatabaseName} ConnectionString:{_settings.ConnectionString} UseDevTables:{_settings.UseDevTables}";
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

        public Job CreateJob(Job job, bool log)
        {
            if (log)
                AddToJobLog(job, null);
            _jobs.InsertOne(job);
            if (log)
                LogAllJobs();
            return job;
        }

        public void UpdateJob(string id, Job job)
        {
            var foundJob = GetJob(id);
            AddToJobLog(job, foundJob);
            if (job.IntervalDays == null && job.LastDone != null)
                RemoveJob(job, true);
            else
                _jobs.ReplaceOne(j => j.Id == id, job);
    
            LogAllJobs();
        }
        public void RemoveJob(Job job, bool log)
        {
            if (log)
                AddToJobLog(null, job);
            _jobs.DeleteOne(j => j.Id == job.Id);
            if (log)
                LogAllJobs();
        }

        public void RemoveJob(string id, bool log)
        {
            RemoveJob(GetJob(id), log);
        }

        #endregion

        internal ChoreService CloneProd()
        {
            var prodSettings = new ChoreDatabaseSettings(_settings);
            prodSettings.UseDevTables = false;
            prodSettings.ConnectionString = "mongodb://127.0.0.1:27017";
            return new ChoreService(prodSettings);
        }

        internal void Backup()
        {
            var dir = FileHelper.CreateDatedFolder(@"f:\onedrive\archive\ChoreMgr");
            var ts = DateTime.Now.ToString("yyyyMMdd HHmmss");
            DanLogger.Log($"Backup {dir} {ts} {this}");
            File.WriteAllText(Path.Combine(dir, $"notes {ts}.txt"), $"Service:{this}");
            File.WriteAllText(Path.Combine(dir, $"{this.JobTableName} {ts}.json"), JsonConvert.SerializeObject(GetJobs(), Formatting.Indented));
            File.WriteAllText(Path.Combine(dir, $"{this.JobTableName}Logs {ts}.json"), JsonConvert.SerializeObject(GetJobLogs(), Formatting.Indented));
        }

        internal void Restore()
        {
            throw new NotImplementedException("Uh, yeah- someone should write this.");
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
            var dir = FileHelper.CreateDatedFolder(@"f:\onedrive\archive\ChoreMgr");
            if (!Directory.EnumerateFiles(dir).Any())
                Backup();

        }
        #endregion 
    }
}
