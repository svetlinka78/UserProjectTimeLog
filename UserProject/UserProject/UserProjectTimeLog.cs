
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace UserProject
{

    public interface IUserProjectTimeLog
    {
        Task RunAsync(IList<TimeLogModel> timelog);
        void Run(IList<UserModel> users, IList<ProjectModel> projects);
        List<dynamic> GetData(int pageNumber, int pageRows, bool checkCount);
        Task GetDataAsync();
        void Clean();
    }
    //DatabaseLayer
    public class UserProjectTimeLog : IUserProjectTimeLog
    {

        private readonly UserProjectTimeLogContext _context;

        public UserProjectTimeLog(UserProjectTimeLogContext context)
        {
            _context = context;
        }
        public async Task RunAsync(IList<TimeLogModel> timelogs)
        {
            // here must be transactions storeProcedure used  -not with foreach list

            foreach (var timelog in timelogs)
            {
                int numberOfRowsAffected = -1;

                DbParameter outputParam = null;


                numberOfRowsAffected =
                    await _context.GetStoredProc("dbo.InsertTimeLog")
                .SqlParams("UserId", timelog.UserId)
                .SqlParams("ProjectId", timelog.ProjectId)
                .SqlParams("DH", timelog.DH)
                .SqlParams("outputParam", (dbParam) =>
                {
                    dbParam.Direction = System.Data.ParameterDirection.Output;
                    dbParam.DbType = System.Data.DbType.Int16;
                    outputParam = dbParam;
                })

                .ExecuteSpNonQueryAsync();

                int outputParamValue = (short)outputParam.Value;

            }

        }
        public void Run(IList<UserModel> users, IList<ProjectModel> projects)
        {

            foreach (var u in users)
            {
                _context.GetStoredProc("dbo.InsertUser")
                 .SqlParams("Name", u.Name)
                 .SqlParams("Id", u.Id)
                 .ExecuteSp((handler) =>
                 {
                     var userRusult = handler.ReaderToList<User>();

                 });
            }

            foreach (var p in projects)
            {
                _context.GetStoredProc("dbo.InsertProject")
                  .SqlParams("Name", p.Name)
                  .SqlParams("Id", p.Id)
                  .ExecuteSp((handler) =>
                  {
                      var userRusult = handler.ReaderToList<Project>();

                  });
            }

        }

        public void Clean()
        {
            //this have to be with callback with affected rows
            _context.GetStoredProc("dbo.TruncateTable")
                    .SqlParams("Name", "All")
             .ExecuteSp((handler) =>
             {
                 var userRusult = handler.ReaderToList<User>();
                 handler.NextResult();
                 var projectResults = handler.ReaderToList<Project>();
                 handler.NextResult();
                 var timeLogResults = handler.ReaderToList<TimeLog>();

             });

        }

        public List<dynamic> GetData(int pageNumber, int pageRows, bool checkCount)
        {
            List<dynamic> result = null;
            _context.GetStoredProc("dbo.GetUPTL")
                .SqlParams("PageNumber",pageNumber)
                .SqlParams("RowsOfPage",pageRows)
                .SqlParams("CheckCount",(checkCount == true) ? 1: 0)

                .ExecuteSp((handler) => {

                    var resultPage = handler.ReaderToList<UPTLPage>();
                    handler.NextResult();

                    var resultModel = handler.ReaderToList<UPTLModel>();
                    result = resultPage.MergeList(resultModel);

                });

            return result;
        }
        public async Task GetDataAsync()
        {
            await _context
                .GetStoredProc("dbo.GetUPTL")
                .ExecuteSpNonQueryAsync();
        }

        //BusinessLayer
        public interface IBusinessLayer
        {
            Task RunAsync(IList<TimeLogModel> timeLog);
            void Run(IList<UserModel> data1, IList<ProjectModel> data2);
            Task GetDataAsync();
            List<dynamic> GetData(int pageNumber, int pageRows, bool checkCount);
            void Clean();
        }
        public class BusinessLayer : IBusinessLayer
        {
            private readonly IUserProjectTimeLog _dataAccess;

            public BusinessLayer(IUserProjectTimeLog dataAccess)
            {
                _dataAccess = dataAccess;
            }

            //public IUserProjectTimeLog DataAccess => _dataAccess;

            public async Task RunAsync(IList<TimeLogModel> timelog)
            {
                await _dataAccess.RunAsync(timelog);
            }

            public async Task GetDataAsync()
            {
                await _dataAccess.GetDataAsync();
            }

            public List<dynamic> GetData(int pageNumber, int pageRows, bool checkCount)
            {
                return _dataAccess.GetData(pageNumber,pageRows, checkCount);
            }
            public void Run(IList<UserModel> data1, IList<ProjectModel> data2)
            {
                _dataAccess.Run(data1, data2);
            }
            public void Clean()
            {
                _dataAccess.Clean();
            }
        }

        //Application
        public class Seeder
        {
            private readonly IList<UserModel> _users;
            private readonly IList<ProjectModel> _projects;

            private readonly IBusinessLayer _bl;

            public Seeder(IBusinessLayer bl)
            {
                _bl = bl;
                _users = new List<UserModel>();
                _projects = new List<ProjectModel>();


                for (var x = 0; x <= 2000; x++)
                {
                    _users.Add(new UserModel { Id = x, Name = "NA_" + x });
                }
                for (var x = 0; x <= 200; x++)
                {

                    _projects.Add(new ProjectModel { Id = x, Name = "PA_" + x });
                }

            }

            public async Task SeedAsync(DateTime dh)
            {
                var data = SimulateData(dh);
                await _bl.RunAsync(data);
            }
           
            public async Task SeedAsyncBC(DateTime dh)
            {
                using (BlockingCollection<IList<TimeLogModel>> bc = new BlockingCollection<IList<TimeLogModel>>())
                {
                    await Task.Run(async () =>
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            var data = SimulateData(dh.AddHours(i));
                            bc.Add(data);
                            await Task.Delay(100);
                        }


                        bc.CompleteAdding();
                    });

                    foreach (var item in bc.GetConsumingEnumerable())
                    {
                        await _bl.RunAsync(item);

                    }
                }
            }

            public void Seed()
            {
                _bl.Run(_users, _projects);
            }

            public void Clean()
            {
                _bl.Clean();
            }

            readonly Dictionary<int, UserModel> _simulated = new Dictionary<int, UserModel>();
            readonly Dictionary<int, ProjectModel> _simulatedPr = new Dictionary<int, ProjectModel>();
            //readonly object _locker = new object();



            private IList<TimeLogModel> SimulateData(DateTime dh)
            {

                //lock (_locker)
                //{
                var random = new Random(Environment.TickCount);
                var count = random.Next(5, 10); //from 5 to 10 users
                var users =
                    _users.Where(x => !_simulated.ContainsKey(x.Id))
                        .OrderBy(x => Guid.NewGuid())
                        .Take(count)
                        .ToList();
                foreach (var u in users)
                {
                    _simulated.Add(u.Id, u);
                }

                var projects =
                    _projects.Where(x => !_simulatedPr.ContainsKey(x.Id))
                        .OrderBy(x => Guid.NewGuid())
                        .Take(count)
                        .ToList();
                foreach (var p in projects)
                {
                    _simulatedPr.Add(p.Id, p);
                }

                var timelogs = users.Select(u =>
                  new TimeLogModel
                  {
                      ProjectId = random.Next(0, projects.Count - 1),
                      UserId = u.Id,
                      DH = DatetimeConvert(dh, random)
                  }).ToList();


                return timelogs;
                // }
            }

            private Func<DateTime, Random, string> DatetimeConvert = (date, random) =>
            {
                var dh = date.AddHours(1).AddMinutes(random.Next(120)).ToString("u").Replace(" ", "T");
                return dh;
            };
        }
    }
}

