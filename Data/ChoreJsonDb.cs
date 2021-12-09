using ChoreMgr.Models;
using ChoreMgr.Utils;
using Newtonsoft.Json;

namespace ChoreMgr.Data
{
    public class ChoreJsonDb
    {
        private readonly IList<Job> _jobs;
        private readonly IList<JobLog> _jobLogs;
        private IChoreJsonDbSettings _settings;

        public ChoreJsonDb(IChoreJsonDbSettings settings)
        {
            _settings = settings;

            _jobs = ReadJsonDb<Job>();
            _jobLogs = ReadJsonDb<JobLog>();
        }

        #region Low-level
        IList<T> ReadJsonDb<T>()
        {
            var str = File.ReadAllText(GetFile<T>());
            var rv = JsonConvert.DeserializeObject<IList<T>>(str);
            if (rv == null)
                rv = new List<T>();
            return rv;
        }
        string GetFile<T>()
        {
            return Path.Combine(_settings.Directory, GetFilename<T>() + ".json");
        }
        string GetFilename<T>()
        {
            var prefix = string.Empty;
            if (_settings.UseDevTables)
                prefix = "DEV_";
            return $"{prefix}{typeof(T).Name}";
        }
        bool WriteJsonDb<T>(IList<T> objs, bool withBackup)
        {
            if (withBackup)
            {
                var dir = FileHelper.CreateDatedFolder(Path.Combine(_settings.Directory, "archive"));
                var ts = DateTime.Now.ToString("yyyyMMdd HHmmss");
                WriteJsonDb<T>(objs, Path.Combine(dir, $"{GetFilename<T>()} {ts}.json"));
            }

            return WriteJsonDb<T>(objs, GetFile<T>());
        }
        bool WriteJsonDb<T>(IList<T> objs, string filename)
        {
            var str = JsonConvert.SerializeObject(objs);
            File.WriteAllText(filename, str);
            return true;
        }

        #endregion  //Low-level

        public override string ToString()
        {
            return $"Name:{_settings.Directory} UseDevTables:{_settings.UseDevTables}";
        }

        #region Job table

        public List<Job> GetJobs() => _jobs.ToList();

        public Job? GetJob(string id) => _jobs.FirstOrDefault(job => job.Id == id);

        public Job CreateJob(Job job, bool log)
        {
            if (log)
                AddToJobLog(job, null);
            if (job.Id == null)
                job.Id = Guid.NewGuid().ToString();
            _jobs.Add(job);
            SaveJobs(log);
            return job;
        }

        private void SaveJobs(bool backup)
        {
            WriteJsonDb<Job>(_jobs, backup);
        }

        public void UpdateJob(string id, Job job)
        {
            var foundJob = GetJob(id);  Returns same job!// 
            AddToJobLog(job, foundJob);
            if (job.IntervalDays == null && job.LastDone != null)
                RemoveJob(job, true);   // we are done with this job
            else if (foundJob != null)
            {
                foundJob.Update(job);
                SaveJobs(true);
            }
        }
        public void RemoveJob(Job job, bool log)
        {
            if (log)
                AddToJobLog(null, job);
            _jobs.Remove(job);
            SaveJobs(log);
        }

        public void RemoveJob(string id, bool log)
        {
            var job = GetJob(id);
            if (job != null)
                RemoveJob(job, log);
        }

        #endregion

        internal ChoreJsonDb CloneFromProd()
        {
            var prodSettings = new ChoreJsonDbSettings(_settings);
            prodSettings.UseDevTables = false;
            return new ChoreJsonDb(prodSettings);
        }

        internal void Restore()
        {
            throw new NotImplementedException("Uh, yeah- someone should write this.");
        }

        #region JobLog table

        public List<JobLog> GetJobLogs() => _jobLogs.ToList();

        public JobLog CreateJobLog(JobLog jobLog)
        {
            _jobLogs.Add(jobLog);
            if (jobLog.Id == null)
                jobLog.Id = Guid.NewGuid().ToString();
            SaveJobLogs();
            return jobLog;
        }

        public void RemoveJobLog(string id)
        {
            var jobLog = _jobLogs.FirstOrDefault(j => j.Id == id);
            if (jobLog != null)    
                _jobLogs.Remove(jobLog);

        }

        private void SaveJobLogs()
        {
            WriteJsonDb<JobLog>(_jobLogs, true);
        }

        #endregion

        #region Logging
        void AddToJobLog(Job? job, Job? oldJob)
        {
            var jobLog = new JobLog();
            jobLog.Updated = DateTime.Now;
            jobLog.JobId = (job?.Id ?? oldJob?.Id)??"-";
            jobLog.JobName = job?.Name ?? oldJob?.Name;
            jobLog.Note = Job.DeltaString(oldJob, job);
            DanLogger.Log($"updated:{jobLog.Updated} id:{jobLog.JobId} name:{jobLog.JobName} note:{jobLog.Note}");
            CreateJobLog(jobLog);
        }
        
        #endregion 
    }
}
