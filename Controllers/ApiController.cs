using Microsoft.AspNetCore.Mvc;
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
            try
            {
                int workerCount;

                using (var context = new TasktDatabaseContext())
                {
                    workerCount = context.Workers.Count();
                }
                    
                return Ok(workerCount + " known worker(s)");
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        #endregion

        #region Data API for Workers

        [HttpGet("/api/Workers/All")]
        public IActionResult GetAllWorkers()
        {
            try
            {
                List<Worker> workerList;

                using (var context = new TasktDatabaseContext())
                {
                    workerList = context.Workers.ToList();

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

                        workerList.Add(worker);
                    }
                }

                return Ok(workerList);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        [HttpGet("/api/Workers/Top")]
        public IActionResult GetTopWorkers()
        {
            try
            {
                var topWorkerList = new List<TopWorker>();

                using (var context = new TasktDatabaseContext())
                {
                    var groupedWorkers = context.Tasks.Where(f => f.TaskStarted >= DateTime.Now.AddDays(-1)).ToList().GroupBy(f => f.WorkerID).OrderByDescending(f => f.Count());

                    foreach (var worker in groupedWorkers)
                    {
                        Worker workerInfo = context.Workers.Where(f => f.WorkerID == worker.Key).FirstOrDefault();

                        string userName = "Unknown";
                        if (!(workerInfo is null))
                        {
                            userName = workerInfo.UserName + " (" + workerInfo.MachineName + ")";
                        }

                        topWorkerList.Add(new TopWorker
                        {
                            WorkerID = worker.Key,
                            UserName = userName,
                            TotalTasks = worker.Count(),
                            RunningTasks = worker.Where(f => f.Status == "Running").Count(),
                            CompletedTasks = worker.Where(f => f.Status == "Completed").Count(),
                            ClosedTasks = worker.Where(f => f.Status == "Closed").Count(),
                            ErrorTasks = worker.Where(f => f.Status == "Error").Count()
                        });
                    }
                }

                return Ok(topWorkerList);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        [HttpGet("/api/Workers/New")]
        public IActionResult AddNewWorker(string userName, string machineName)
        {
            try
            {
                var newWorker = new Worker
                {
                    UserName = userName,
                    MachineName = machineName,
                    LastCheckIn = DateTime.Now,
                    Status = WorkerStatus.Pending
                };

                using (var context = new TasktDatabaseContext())
                {
                    context.Workers.Add(newWorker);
                    context.SaveChanges();
                }

                return Ok(newWorker);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }
            

        [HttpGet("/api/Workers/CheckIn")]
        public IActionResult CheckInWorker(Guid workerID, bool engineBusy)
        {
            try
            {
                var targetWorker = new Worker();
                var scheduledTask = new Task();
                var publishedScript = new PublishedScript();

                using (var context = new TasktDatabaseContext())
                {
                    targetWorker = context.Workers.Where(f => f.WorkerID == workerID).FirstOrDefault();

                    if (targetWorker is null)
                    {
                        return BadRequest();
                    }
                    else
                    {
                        targetWorker.LastCheckIn = DateTime.Now;

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
                    }
                }

                return Ok(new CheckInResponse
                {
                    Worker = targetWorker,
                    ScheduledTask = scheduledTask,
                    PublishedScript = publishedScript
                });
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
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

        [HttpGet("/api/Tasks/Metrics/Status")]
        public IActionResult GetStatusTaskCount([FromQuery] MetricRequest request)
        {
            try
            {
                request.StartDate ??= DateTime.Today;
                int taskCount;

                using (var context = new TasktDatabaseContext())
                {
                    taskCount = context.Tasks.Where(f => f.Status == request.Status.ToString()).Where(f => f.TaskStarted >= request.StartDate).Count();
                }

                return Ok(taskCount + " " + request.Status.ToString());
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        #endregion

        #region Data API for Tasks

        [HttpGet("/api/Tasks/All")]
        public IActionResult GetAllTasks()
        {
            try
            {
                var runningTasks = new List<Task>();

                using (var context = new TasktDatabaseContext())
                {
                    runningTasks = context.Tasks.OrderByDescending(f => f.TaskStarted).ToList();
                }

                return Ok(runningTasks);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            } 
        }

        [HttpGet("/api/Tasks/New")]
        public IActionResult NewTask(Guid workerID, string taskName, string userName, string machineName)
        {
            try
            {
                //Todo: Add Auth Check, Change to HTTPPost and validate workerID is valid
                var newTask = new Task
                {
                    WorkerID = workerID,
                    UserName = userName,
                    MachineName = machineName,
                    TaskStarted = DateTime.Now,
                    Status = "Running",
                    ExecutionType = "Local",
                    Script = taskName
                };

                using (var context = new TasktDatabaseContext())
                {
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

                    context.Tasks.Add(newTask);
                    context.SaveChanges();      
                }

                return Ok(newTask);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        [HttpGet("/api/Tasks/Update")]
        public IActionResult UpdateTask(Guid taskID, string status, /* Guid workerID, */ string userName, string machineName, string remark)
        {
            try
            {
                //Todo: Needs Testing
                var taskToUpdate = new Task();

                using (var context = new TasktDatabaseContext())
                {
                    taskToUpdate = context.Tasks.Where(f => f.TaskID == taskID).FirstOrDefault();

                    if (status == "Completed")
                    {
                        taskToUpdate.TaskFinished = DateTime.Now;
                    }

                    taskToUpdate.UserName = userName;
                    taskToUpdate.MachineName = machineName;
                    taskToUpdate.Remark = remark;
                    taskToUpdate.Status = status;

                    context.SaveChanges();
                }

                return Ok(taskToUpdate);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        [HttpPost("/api/Tasks/Schedule")]
        public IActionResult ScheduleTask([FromBody] NewTaskRequest request)
        {
            try
            {
                //Todo: Add Auth Check, Change to HTTPPost and validate workerID is valid
                if (request is null)
                {
                    return BadRequest();
                }

                using(var context = new TasktDatabaseContext())
                {
                    PublishedScript publishedScript = context.PublishedScripts.Where(f => f.PublishedScriptID == request.PublishedScriptID).FirstOrDefault();

                    if (publishedScript == null)
                    {
                        return BadRequest();
                    }

                    //find worker
                    Worker workerRecord = context.Workers.Where(f => f.WorkerID == request.WorkerID).FirstOrDefault();

                    //if worker wasnt found then search for pool

                    if (workerRecord == null)
                    {
                        //find from pool
                        bool poolExists = context.WorkerPools.Any(s => s.WorkerPoolID == request.WorkerID);

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

                    context.Tasks.Add(newTask);
                    context.SaveChanges();

                    return Ok(newTask);
                }             
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        #endregion
        [HttpGet("/api/Scripts/All")]
        public IActionResult GetAllPublishedScripts()
        {
            try
            {
                var scriptList = new List<PublishedScript>();

                using (var context = new TasktDatabaseContext())
                {
                    //var publishedScripts = context.PublishedScripts.ToList().OrderBy(f => f.WorkerID);
                    //var workers = context.Workers.Include(d => context.Workers.Where(f => f.WorkerID == d.WorkerID));

                    //context.PublishedScripts.Include(context.Workers.ToList());

                    scriptList = (from publishedScripts in context.PublishedScripts
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
                }

                return Ok(scriptList);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        [HttpPost("/api/Scripts/Publish")]
        public IActionResult PublishScript([FromBody] PublishedScript script)
        {
            try
            {
                using (var context = new TasktDatabaseContext())
                {
                    if (script.OverwriteExisting)
                    {
                        PublishedScript currentItem = context.PublishedScripts.Where(f => f.WorkerID == script.WorkerID && f.FriendlyName == script.FriendlyName).OrderByDescending(f => f.PublishedOn).FirstOrDefault();
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
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        [HttpGet("/api/Scripts/Exists")]
        public IActionResult ScriptExistsCheck([FromQuery]Guid workerID, string friendlyName)
        {
            try
            {
                bool exists;

                using (var context = new TasktDatabaseContext())
                {
                    exists = context.PublishedScripts.Where(f => f.WorkerID == workerID && f.FriendlyName == friendlyName).Any();  
                }

                return Ok(exists);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        [HttpGet("/api/Assignments/All")]
        public IActionResult GetAllAssignments()
        {
            try
            {
                IOrderedEnumerable<Assignment> assignments;

                using (var context = new TasktDatabaseContext())
                {
                    assignments = context.Assignments.ToList().OrderByDescending(f => f.NewTaskDue);
                }

                return Ok(assignments);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        [HttpPost("/api/Assignments/Add")]
        public IActionResult AddAssignment([FromBody] Assignment assignment)
        {
            try
            {
                if (assignment is null)
                {
                    return BadRequest();
                }

                using (var context = new TasktDatabaseContext())
                {
                    context.Assignments.Add(assignment);
                    context.SaveChanges();
                }
                
                return Ok(assignment);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }    
        }

        [HttpPost("/api/BotStore/Add")]
        public IActionResult AddDataToBotStore([FromBody] BotStoreModel storeData)
        {
            try
            {
                using (var context = new TasktDatabaseContext())
                {
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
                }

                return Ok(storeData);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }
        }

        [HttpPost("/api/BotStore/Get")]
        public IActionResult GetDataBotStore([FromBody] BotStoreRequest requestData)
        {
            try
            {
                var requestedItem = new BotStoreModel();

                using (var context = new TasktDatabaseContext())
                {
                    if (!context.Workers.Any(f => f.WorkerID == requestData.WorkerID))
                    {
                        return Unauthorized();
                    }

                    requestedItem = context.BotStore.Where(f => f.BotStoreName == requestData.BotStoreName).FirstOrDefault();

                    if (requestedItem == null)
                    {
                        return NotFound();
                    }
                }

                return requestData.Type switch
                {
                    BotStoreRequest.RequestType.BotStoreValue => Ok(requestedItem.BotStoreValue),
                    BotStoreRequest.RequestType.BotStoreModel => Ok(requestedItem),
                    _ => StatusCode(400),
                };
            }
            catch(Exception Ex)
            {
                return StatusCode(500, Ex.Message);
            }         
        }
    }
}
