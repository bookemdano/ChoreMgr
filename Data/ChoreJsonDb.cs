using ChoreMgr.Models;
using ChoreMgr.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChoreMgr.Data
{
    public class ChoreJsonDb
    {
        private IList<Transaction>? _transactions;
        private IList<Job>? _jobs;
        private IList<JobLog>? _jobLogs;
        private IChoreJsonDbSettings _settings;

        public ChoreJsonDb(IChoreJsonDbSettings settings)
        {
            _settings = settings;
        }

        internal IList<JobLog> GetJobLogs()
        {
            if (_jobLogs == null)
                _jobLogs = ReadJsonDb<JobLog>();
            return _jobLogs;

        }
        #region Low-level
        IList<T> ReadJsonDb<T>()
        {
            if (!File.Exists(GetFile<T>()))
                return new List<T>();
            
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
        string ArchiveDirectory
        {
            get
            {
                return Path.Combine(_settings.Directory, "archive");
            }
        }
        bool WriteJsonDb<T>(IList<T> objs)
        {
            // write backup
            WriteJsonDb<T>(objs, FileHelper.CreateDatedFilename(ArchiveDirectory, GetFilename<T>(), "json"));

            return WriteJsonDb<T>(objs, GetFile<T>());
        }
        bool WriteJsonDb<T>(IList<T> objs, string filename)
        {
            var str = JsonConvert.SerializeObject(objs, Formatting.Indented);
            File.WriteAllText(filename, str);
            return true;
        }

        #endregion  //Low-level

        public override string ToString()
        {
            return $"Source:{_settings.Directory} UseDevTables:{_settings.UseDevTables}";
        }

        #region Transaction table

        IList<Transaction> GetTransactions()
        {
            if (_transactions == null)
            {
                _transactions = ReadJsonDb<Transaction>();
            }
            return _transactions;

        }
        private void SaveTransactions()
        {
            WriteJsonDb<Transaction>(GetTransactions());
        }
        internal Transaction? GetTransaction(string id)
        {
            return GetTransactions().FirstOrDefault(t => t.Id == id);
        }


        public List<TransactionModel> GetTransactionModels() => GetTransactions().Select(t => new TransactionModel(t)).ToList();

        public Transaction CreateTransaction(Transaction transaction, string? userName)
        {
            if (transaction.Id == null)
                transaction.Id = Guid.NewGuid().ToString();
            GetTransactions().Add(transaction);
            SaveTransactions();
            return transaction;
        }
        public void UpdateTransaction(Transaction transaction, string? userName)
        {
            if (transaction.Id == null)
                return;
            var foundTransaction = GetTransaction(transaction.Id);
            if (foundTransaction == null)
                return;
            foundTransaction.Update(transaction);
            SaveTransactions();
        }

        void RemoveTransaction(Transaction transaction, string? userName)
        {
            GetTransactions().Remove(transaction);
            SaveTransactions();
        }

        public void RemoveTransaction(string id, string? userName)
        {
            var transaction = GetTransaction(id);
            if (transaction != null)
                RemoveTransaction(transaction, userName);
        }
        void BackupTransactions()
        {
            DanLogger.Log($"BackupTransactions {this}");
            File.WriteAllText(FileHelper.CreateDatedFilename(ArchiveDirectory, "notes", "txt"), $"Service:{this}");
            WriteJsonDb<Transaction>(GetTransactions(), FileHelper.CreateDatedFilename(ArchiveDirectory, GetFilename<Transaction>(), "json"));

            // to csv
            var outs = new List<string>();
            outs.Add(Transaction.CsvHeader());
            outs.AddRange(GetTransactions().Select(t => t.ToCsv()));
            File.WriteAllLines(FileHelper.CreateDatedFilename(ArchiveDirectory, GetFilename<Transaction>(), ".csv"), outs);
        }

        #endregion

        #region Job table

        IList<Job> GetJobs()
        {
            if (_jobs == null)
            {
                _jobs = ReadJsonDb<Job>();
            }
            return _jobs;

        }
        public List<JobModel> GetJobModels() => GetJobs().Select(j => new JobModel(j)).ToList();

        public Job? GetJobFromDb(string? id)
        {
            if (id == null)
                return null;
            return ReadJsonDb<Job>().FirstOrDefault(job => job.Id == id);
        }

        public JobModel? GetJobModel(string id, bool includeLogs = false)
        {
            var job = GetJobs().FirstOrDefault(job => job.Id == id);
            if (job == null)
                return null;
            var rv = new JobModel(job);
            if (rv != null && includeLogs)
                rv.Logs = _jobLogs.Where(j => j.JobId == job.Id).OrderByDescending(j => j.Updated).ToList();
            return rv;
        }

        public Job CreateJob(Job job, string? userName)
        {
            if (job.Id == null)
                job.Id = Guid.NewGuid().ToString();
            AddToJobLog(job, null, userName);
            GetJobs().Add(job);
            SaveJobs();
            return job;
        }

        private void SaveJobs()
        {
            WriteJsonDb<Job>(GetJobs());
        }

        public void UpdateJob(Job job, string? userName)
        {
            var foundJob = GetJobs().FirstOrDefault(j => j.Id == job.Id);
            if (foundJob == null)
                return;
            AddToJobLog(job, foundJob, userName);
            if (job.IntervalDays == null && job.LastDone != null)
                RemoveJob(foundJob, userName);   // we are done with this job
            else if (foundJob != null)
            {

                foundJob.Update(job);
                SaveJobs();
            }
        }
        void RemoveJob(Job job, string? userName)
        {
            AddToJobLog(null, job, userName);
            GetJobs().Remove(job);
            SaveJobs();
        }

        public void RemoveJob(string id, string? userName)
        {
            var job = GetJobs().FirstOrDefault(job => job.Id == id);
            if (job != null)
                RemoveJob(job, userName);
        }
        void BackupJobs()
        {
            DanLogger.Log($"BackupJobs {this}");
            File.WriteAllText(FileHelper.CreateDatedFilename(ArchiveDirectory, "notes", "txt"), $"Service:{this}");
            WriteJsonDb<Job>(GetJobs(), FileHelper.CreateDatedFilename(ArchiveDirectory, GetFilename<Job>(), "json"));
            WriteJsonDb<JobLog>(GetJobLogs(), FileHelper.CreateDatedFilename(ArchiveDirectory, GetFilename<JobLog>(), "json"));

            // to csv
            var outs = new List<string>();
            outs.Add(Job.CsvHeader());
            outs.AddRange(GetJobs().Select(j => j.ToCsv()));
            File.WriteAllLines(FileHelper.CreateDatedFilename(ArchiveDirectory, GetFilename<Job>(), ".csv"), outs);

            outs = new List<string>();
            outs.Add($"Id,Updated,JobId,JobName,Note,DoneDate,User");
            var jobLogs = GetJobLogs();
            foreach (var jobLog in jobLogs)
                outs.Add($"{jobLog.Id},{jobLog.Updated},{jobLog.JobId},{jobLog.JobName},{jobLog.Note},{jobLog.DoneDate?.ToShortDateString()},{jobLog.User}");
            File.WriteAllLines(FileHelper.CreateDatedFilename(ArchiveDirectory, GetFilename<JobLog>(), ".csv"), outs);

        }

        #endregion

        #region JobLog table

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
            WriteJsonDb<JobLog>(_jobLogs);
        }

        #endregion
        internal void ProdSync()
        {
            Backup();
            // copy prod context to dev context for testing
            var prodService = CloneFromProd();
            WriteJsonDb<Job>(prodService.GetJobs());
            WriteJsonDb<JobLog>(prodService.GetJobLogs());
            _jobs = ReadJsonDb<Job>();
            _jobLogs = ReadJsonDb<JobLog>();
            WriteJsonDb<Transaction>(prodService.GetTransactions());
            _transactions = ReadJsonDb<Transaction>();
        }

        ChoreJsonDb CloneFromProd()
        {
            var prodSettings = new ChoreJsonDbSettings(_settings);
            prodSettings.UseDevTables = false;
            return new ChoreJsonDb(prodSettings);
        }

        #region Logging
        void AddToJobLog(Job? job, Job? oldJob, string? userName)
        {
            var jobLog = new JobLog();
            jobLog.Updated = DateTime.Now;
            jobLog.JobId = (job?.Id ?? oldJob?.Id)??"-";
            jobLog.JobName = job?.Name ?? oldJob?.Name;
            jobLog.Note = JobModel.DeltaString(oldJob, job);
            jobLog.User = userName;
            DanLogger.Log($"UPDATE ts:{jobLog.Updated} id:{jobLog.JobId} name:{jobLog.JobName} note:{jobLog.Note} user:{jobLog.User}");
            CreateJobLog(jobLog);
        }

        internal void Backup()
        {
            DanLogger.Log($"Backup {this}");
            BackupJobs();
            BackupTransactions();
        }
        #endregion 
    }
}
