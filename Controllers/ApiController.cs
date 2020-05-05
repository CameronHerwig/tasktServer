﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using tasktServer.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace tasktServer.Controllers
{
    [Produces("application/json")]
    public class ApiController : Controller
    {

        #region Test API for Workers

        [HttpGet("/api/Test")]
        public IActionResult TestConnection()
        {
            return Ok("Hello World!");
        }

        #endregion

        #region Metrics API for Workers

        [HttpGet("/api/Workers/Metrics/Authorized")]
        public IActionResult GetAuthorizedWorkers()
        {
            using var context = new TasktDatabaseContext();
            var workers = context.Workers.Count();
            return Ok(workers + " known worker(s)");
        }

        #endregion

        #region Data API for Workers

        [HttpGet("/api/Workers/All")]
        public IActionResult GetAllWorkers()
        {
            using var context = new TasktDatabaseContext();
            var workers = context.Workers.ToList();

            //get pools
            var workerPools = context.WorkerPools.Include(f => f.PoolWorkers);

            foreach (var pool in workerPools)
            {
                if (pool.PoolWorkers.Count == 0)
                    continue;

                var worker = new Worker
                {
                    WorkerID = pool.WorkerPoolID,
                    UserName = string.Concat("Pool '", pool.WorkerPoolName, "'")
                };

                workers.Add(worker);
            }

            return Ok(workers);
        }

        [HttpGet("/api/Workers/Top")]
        public IActionResult GetTopWorkers()
        {
            using var context = new TasktDatabaseContext();
            var topWorkerList = new List<TopWorker>();

            var groupedWorker = context.Tasks.Where(f => f.TaskStarted >= DateTime.Now.AddDays(-1)).GroupBy(f => f.WorkerID).OrderByDescending(f => f.Count());

            try
            {
                foreach (var wrkr in groupedWorker)
                {
                    var workerInfo = context.Workers.Where(f => f.WorkerID == wrkr.Key).FirstOrDefault();

                    string userName = "Unknown";
                    if (!(workerInfo is null))
                    {
                        userName = workerInfo.UserName + " (" + workerInfo.MachineName + ")";
                    }

                    topWorkerList.Add(new TopWorker
                    {
                        WorkerID = wrkr.Key,
                        UserName = userName,
                        TotalTasks = wrkr.Count(),
                        RunningTasks = wrkr.Where(f => f.Status == "Running").Count(),
                        CompletedTasks = wrkr.Where(f => f.Status == "Completed").Count(),
                        ClosedTasks = wrkr.Where(f => f.Status == "Closed").Count(),
                        ErrorTasks = wrkr.Where(f => f.Status == "Error").Count()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure while retrieving top workers: " + ex.Message);
            }

            return Ok(topWorkerList);

        }

        [HttpGet("/api/Workers/New")]
        public IActionResult AddNewWorker(string userName, string machineName)
        {
            using var context = new TasktDatabaseContext();
            var newWorker = new Worker
            {
                UserName = userName,
                MachineName = machineName,
                LastCheckIn = DateTime.Now,
                Status = Models.WorkerStatus.Pending
            };
            context.Workers.Add(newWorker);
            context.SaveChanges();
            return Ok(newWorker);
        }

        [HttpGet("/api/Workers/CheckIn")]
        public IActionResult CheckInWorker(Guid workerID, bool engineBusy)
        {
            using var context = new TasktDatabaseContext();
            var targetWorker = context.Workers.Where(f => f.WorkerID == workerID).FirstOrDefault();

            if (targetWorker is null)
            {
                return BadRequest();
            }
            else
            {
                targetWorker.LastCheckIn = DateTime.Now;
                Task scheduledTask = null;
                PublishedScript publishedScript = null;

                if (!engineBusy)
                {

                    scheduledTask = context.Tasks.Where(f => f.WorkerID == workerID && f.Status == "Scheduled").FirstOrDefault();

                    if (scheduledTask != null)
                    {
                        //worker directly scheduled

                        publishedScript = context.PublishedScripts.Where(f => f.PublishedScriptID.ToString() == scheduledTask.Script).FirstOrDefault();

                        if (publishedScript != null)
                        {
                            scheduledTask.Status = "Deployed";
                        }
                        else
                        {
                            scheduledTask.Status = "Deployment Failed";
                        }
                    }
                    else
                    {
                        //check if any pool tasks

                        var workerPools = context.WorkerPools
                            .Include(f => f.PoolWorkers)
                            .Where(f => f.PoolWorkers.Any(s => s.WorkerID == workerID)).ToList();

                        foreach (var pool in workerPools)
                        {
                            scheduledTask = context.Tasks.Where(f => f.WorkerID == pool.WorkerPoolID && f.Status == "Scheduled").FirstOrDefault();

                            if (scheduledTask != null)
                            {
                                //worker directly scheduled

                                publishedScript = context.PublishedScripts.Where(f => f.PublishedScriptID.ToString() == scheduledTask.Script).FirstOrDefault();

                                if (publishedScript != null)
                                {
                                    scheduledTask.Status = "Deployed";
                                }
                                else
                                {
                                    scheduledTask.Status = "Deployment Failed";
                                }

                                break;
                            }
                        }
                    }
                }

                context.SaveChanges();

                return Ok(new CheckInResponse
                {
                    Worker = targetWorker,
                    ScheduledTask = scheduledTask,
                    PublishedScript = publishedScript
                });

            }

        }

        #endregion

        //Coming soon...
        //[HttpGet("/api/Workers/Pending")]
        //public IActionResult GetPendingWorkers()
        //{
        //    using (TasktDatabaseContext context = new TasktDatabaseContext()())
        //    {
        //        //Todo: Change to workers table
        //        var workers = context.Workers.Where(f => f.Status == Models.WorkerStatus.Pending).Count();
        //        return Ok(workers + " pending workers");
        //    }

        //}

        //Coming soon...
        //[HttpGet("/api/Workers/Revoked")]
        //public IActionResult GetRevokedWorkers()
        //{
        //    using (TasktDatabaseContext context = new TasktDatabaseContext()())
        //    {
        //        //Todo: Change to workers table
        //        var workers = context.Workers.Where(f => f.Status == Models.WorkerStatus.Revoked).Count();
        //        return Ok(workers + " revoked workers");
        //    }

        //}

        #region Metrics API for Tasks

        [HttpGet("/api/Tasks/Metrics/Completed")]
        public IActionResult GetCompletedTasks([FromQuery]DateTime? startDate = null)
        {
            startDate ??= DateTime.Today;

            using var context = new TasktDatabaseContext();
            var completedTasks = context.Tasks.Where(f => f.Status == "Completed").Where(f => f.TaskStarted >= startDate).Count();
            return Ok(completedTasks + " completed");
        }

        [HttpGet("/api/Tasks/Metrics/Closed")]
        public IActionResult GetClosedTasks([FromQuery]DateTime? startDate = null)
        {
            startDate ??= DateTime.Today;

            using var context = new TasktDatabaseContext();
            var completedTasks = context.Tasks.Where(f => f.Status == "Closed").Where(f => f.TaskStarted >= startDate).Count();
            return Ok(completedTasks + " closed");
        }

        [HttpGet("/api/Tasks/Metrics/Errored")]
        public IActionResult GetErroredTasks([FromQuery]DateTime? startDate = null)
        {
            startDate ??= DateTime.Today;

            using var context = new TasktDatabaseContext();
            var erroredTasks = context.Tasks.Where(f => f.Status == "Error").Where(f => f.TaskStarted >= startDate).Count();
            return Ok(erroredTasks + " errored");
        }

        [HttpGet("/api/Tasks/Metrics/Running")]
        public IActionResult GetRunningTasks([FromQuery]DateTime? startDate = null)
        {
            startDate ??= DateTime.Today;

            using var context = new TasktDatabaseContext();
            var runningTasks = context.Tasks.Where(f => f.Status == "Running").Where(f => f.TaskStarted >= startDate).Count();
            return Ok(runningTasks + " running");

        }

        #endregion

        #region Data API for Tasks

        [HttpGet("/api/Tasks/All")]
        public IActionResult GetAllTasks()
        {
            using var context = new TasktDatabaseContext();
            var runningTasks = context.Tasks.ToList().OrderByDescending(f => f.TaskStarted);
            return Ok(runningTasks);

        }

        [HttpGet("/api/Tasks/New")]
        public IActionResult NewTask(Guid workerID, string taskName, string userName, string machineName)
        {
            //Todo: Add Auth Check, Change to HTTPPost and validate workerID is valid

            using var context = new TasktDatabaseContext();
            //var workerExists = context.Workers.Where(f => f.WorkerID == workerID).Count() > 0;

            //if (!workerExists)
            //{
            //    //Todo: Create Alert
            //    return Unauthorized();
            //}

            //close out any stale tasks
            var staleTasks = context.Tasks.Where(f => f.WorkerID == workerID && f.Status == "Running");
            foreach (var task in staleTasks)
            {
                task.Status = "Closed";
            }

            Models.Task newTask = new Models.Task
            {
                WorkerID = workerID,
                UserName = userName,
                MachineName = machineName,
                TaskStarted = DateTime.Now,
                Status = "Running",
                ExecutionType = "Local",
                Script = taskName
            };

            var entry = context.Tasks.Add(newTask);
            context.SaveChanges();
            return Ok(newTask);
        }

        [HttpGet("/api/Tasks/Update")]
        public IActionResult UpdateTask(Guid taskID, string status, /* Guid workerID, */ string userName, string machineName, string remark)
        {
            //Todo: Needs Testing
            using var context = new TasktDatabaseContext();
            var taskToUpdate = context.Tasks.Where(f => f.TaskID == taskID).FirstOrDefault();

            if (status == "Completed")
            {
                taskToUpdate.TaskFinished = DateTime.Now;
            }

            taskToUpdate.UserName = userName;
            taskToUpdate.MachineName = machineName;
            taskToUpdate.Remark = remark;
            taskToUpdate.Status = status;
            context.SaveChanges();
            return Ok(taskToUpdate);

        }

        [HttpPost("/api/Tasks/Schedule")]
        public IActionResult ScheduleTask([FromBody] NewTaskRequest request)
        {
            //Todo: Add Auth Check, Change to HTTPPost and validate workerID is valid
            if (request is null)
            {
                return BadRequest();
            }

            using var context = new TasktDatabaseContext();
            var publishedScript = context.PublishedScripts.Where(f => f.PublishedScriptID == request.PublishedScriptID).FirstOrDefault();

            if (publishedScript == null)
            {
                return BadRequest();
            }

            //find worker
            var workerRecord = context.Workers.Where(f => f.WorkerID == request.WorkerID).FirstOrDefault();

            //if worker wasnt found then search for pool

            if (workerRecord == null)
            {
                //find from pool
                var poolExists = context.WorkerPools.Any(s => s.WorkerPoolID == request.WorkerID);

                //if pool wasnt found
                if (!poolExists)
                {
                    //return bad request
                    return BadRequest();
                }
            }

            //create new task
            var newTask = new Task
            {
                WorkerID = request.WorkerID,
                TaskStarted = DateTime.Now,
                Status = "Scheduled",
                ExecutionType = "Remote",
                Script = publishedScript.PublishedScriptID.ToString(),
                Remark = "Scheduled by tasktServer"
            };

            var entry = context.Tasks.Add(newTask);
            context.SaveChanges();
            return Ok(newTask);

        }

        #endregion
        [HttpGet("/api/Scripts/All")]
        public IActionResult GetAllPublishedScripts()
        {
            using var context = new TasktDatabaseContext();
            //var publishedScripts = context.PublishedScripts.ToList().OrderBy(f => f.WorkerID);
            //var workers = context.Workers.Include(d => context.Workers.Where(f => f.WorkerID == d.WorkerID));

            //context.PublishedScripts.Include(context.Workers.ToList());

            var scripts = (from publishedScripts in context.PublishedScripts
                           join worker in context.Workers on publishedScripts.WorkerID equals worker.WorkerID
                           select new PublishedScript
                           {
                               FriendlyName = publishedScripts.FriendlyName,
                               PublishedOn = publishedScripts.PublishedOn,
                               PublishedScriptID = publishedScripts.PublishedScriptID,
                               ScriptData = publishedScripts.ScriptData,
                               ScriptType = publishedScripts.ScriptType,
                               WorkerID = publishedScripts.WorkerID,
                               MachineName = worker.MachineName,
                               WorkerName = worker.UserName

                           }).ToList();

            return Ok(scripts);
        }

        [HttpPost("/api/Scripts/Publish")]
        public IActionResult PublishScript([FromBody] PublishedScript script)
        {
            using var context = new TasktDatabaseContext();
            if (script.OverwriteExisting)
            {
                var currentItem = context.PublishedScripts.Where(f => f.WorkerID == script.WorkerID && f.FriendlyName == script.FriendlyName).OrderByDescending(f => f.PublishedOn).FirstOrDefault();
                currentItem.PublishedOn = DateTime.Now;
                currentItem.ScriptData = script.ScriptData;
                context.SaveChanges();
                return Ok("The script has been updated on the server.");
            }
            else
            {
                script.PublishedOn = DateTime.Now;
                context.PublishedScripts.Add(script);
                context.SaveChanges();
                return Ok("The script has been successfully published.");
            }

        }

        [HttpGet("/api/Scripts/Exists")]
        public IActionResult ScriptExistsCheck([FromQuery]Guid workerID, string friendlyName)
        {
            using var context = new TasktDatabaseContext();
            var exists = context.PublishedScripts.Where(f => f.WorkerID == workerID && f.FriendlyName == friendlyName).Any();
            return Ok(exists);
        }

        [HttpGet("/api/Assignments/All")]
        public IActionResult GetAllAssignments()
        {
            using var context = new TasktDatabaseContext();
            var assignments = context.Assignments.ToList().OrderByDescending(f => f.NewTaskDue);
            return Ok(assignments);
        }

        [HttpPost("/api/Assignments/Add")]
        public IActionResult AddAssignment([FromBody] Assignment assignment)
        {
            using var context = new TasktDatabaseContext();
            context.Assignments.Add(assignment);
            context.SaveChanges();
            return Ok(assignment);
        }

        [HttpPost("/api/BotStore/Add")]
        public IActionResult AddDataToBotStore([FromBody] BotStoreModel storeData)
        {

            using var context = new TasktDatabaseContext();
            if (!context.Workers.Any(f => f.WorkerID == storeData.LastUpdatedBy))
            {
                return Unauthorized();
            }

            if (context.BotStore.Any(f => f.BotStoreName == storeData.BotStoreName))
            {
                var existingItem = context.BotStore.Where(f => f.BotStoreName == storeData.BotStoreName).FirstOrDefault();
                existingItem.BotStoreValue = storeData.BotStoreValue;
                existingItem.LastUpdatedOn = DateTime.Now;
                existingItem.LastUpdatedBy = storeData.LastUpdatedBy;
            }
            else
            {
                storeData.LastUpdatedOn = DateTime.Now;
                context.BotStore.Add(storeData);
            }

            context.SaveChanges();
            return Ok(storeData);
        }

        [HttpPost("/api/BotStore/Get")]
        public IActionResult GetDataBotStore([FromBody] BotStoreRequest requestData)
        {

            using var context = new TasktDatabaseContext();
            if (!context.Workers.Any(f => f.WorkerID == requestData.WorkerID))
            {
                return Unauthorized();
            }

            var requestedItem = context.BotStore.Where(f => f.BotStoreName == requestData.BotStoreName).FirstOrDefault();

            if (requestedItem == null)
            {
                return NotFound();
            }

            return requestData.Type switch
            {
                BotStoreRequest.RequestType.BotStoreValue => Ok(requestedItem.BotStoreValue),
                BotStoreRequest.RequestType.BotStoreModel => Ok(requestedItem),
                _ => StatusCode(400),
            };
        }
    }
}
