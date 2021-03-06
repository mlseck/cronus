﻿using DatabaseEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cronus.Models;
using Cronus.ViewModels;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;
using System.Data.Entity;
using Cronus.Login;
using System.Data.Entity.Core.Objects;

namespace Cronus.Controllers
{
    public class HomeController : Controller
    {
        private CronusDatabaseEntities db = new CronusDatabaseEntities();
        HomeViewModel myModel = new HomeViewModel();


        private readonly IProjectRepository projectRepository;
        private readonly IActivityRepository activityRepository;



        // If you are using Dependency Injection, you can delete the following constructor
        public HomeController() : this(new ProjectRepository(), new ActivityRepository())
        {
        }

        public HomeController(IProjectRepository projectRepository, IActivityRepository activityRepository)
        {
            this.projectRepository = projectRepository;
            this.activityRepository = activityRepository;
        }

        [System.Web.Mvc.AllowAnonymous]
        public ActionResult Index()
        {
            HomeViewModel myModel = new HomeViewModel();
            try
            {
                var emp = UserManager.User.employeeID;
                myModel.currentWeekEndDate = (ExtensionMethods.Next(DateTime.Now, DayOfWeek.Sunday));
                var query = from hw in db.hoursworkeds
                            where hw.TimePeriod_employeeID == emp && DbFunctions.TruncateTime(hw.TimePeriod_periodEndDate) == myModel.currentWeekEndDate.Date
                            select hw;
                myModel.HoursWorked = query.ToList();
                // If Timeperiod was already approved, set Viewbag isApproved to true
                var isApprovedQuery = from a in db.employeetimeperiods
                                      where a.Employee_employeeID == emp && DbFunctions.TruncateTime(a.TimePeriod_periodEndDate) == myModel.currentWeekEndDate.Date
                                      select a;
                if (isApprovedQuery.Any())
                {
                        myModel.isApproved = isApprovedQuery.First().isApproved;
                }
                // Get list of active projects that group is working on 
                List<project> groupProjects = GetGroupProjects(emp.ToString());
                foreach(var hrworked in myModel.HoursWorked)
                {
                    hrworked.ProjectList = groupProjects;
                }
                myModel.ProjectList = groupProjects;
            }
            catch (Exception ex)
            {
                var exception = ex.Message;
            }
            return View(myModel);
        }

        public JsonResult GetLastWeek(DateTime currentWeek) {
            DateTime lastWeek = currentWeek.AddDays(-7).Date;
            var getPrevious = from hw in db.hoursworkeds
                              where hw.TimePeriod_employeeID == UserManager.User.employeeID && DbFunctions.TruncateTime(hw.TimePeriod_periodEndDate) == lastWeek
                              select hw;
            List<hoursworked> lastWeekHours = new List<hoursworked>();
            List<hoursworked> returnHours = new List<hoursworked>();
            lastWeekHours = getPrevious.ToList();
            foreach (var hour in lastWeekHours)
            {
                returnHours.Add(new hoursworked()
                {
                    Activity_activityID = hour.Activity_activityID,
                    currentDay = hour.date.DayOfWeek,
                    Project_projectID = hour.Project_projectID,
                });
            }
            return Json(returnHours, JsonRequestBehavior.AllowGet);

        }

        public List<project> GetGroupProjects(string empID)
        {
            var dbGrps = db.groups.ToList();
            employee employee = db.employees.Find(@UserManager.User.employeeID);
            var groups = (from g in dbGrps where g.employees.Contains(employee) select g).ToList();
            List<project> groupProjects = new List<project>();
            foreach (var group in groups)
            {
                foreach (var proj in group.projects)
                {
                    if (proj.projectActive == 1)
                    {
                        groupProjects.Add(proj);
                    }
                }
            }
            return groupProjects;
        }

        public ActionResult AddLastWeekPartials(int entryDay, int projectID, int activityID)
        {
            var emp = UserManager.User.employeeID;
            var getProject = from proj in db.projects where proj.projectID == projectID select proj;
            hoursworked myHour = new hoursworked();
            myHour.project = getProject.First();
            myHour.Project_projectID = projectID;
            myHour.Activity_activityID = activityID;
            myHour.currentDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), entryDay.ToString());
            myHour.ProjectList = GetGroupProjects(emp);
            return PartialView("_hoursworkedrow", myHour);
        }

        [HttpPost]
        public ActionResult SubmitHours(HomeViewModel submittedHours, string submitButton)
        {
            var emp = UserManager.User.employeeID;
            DateTime AddToWeek = submittedHours.currentWeekEndDate.Date;
            // Check if timeperiod already exists
            // If not create it
            DateTime timePeriod = submittedHours.currentWeekEndDate.Date;
            if (db.timeperiods.Find(timePeriod) == null)
            {
                timeperiod newTimePeriod = new timeperiod();
                newTimePeriod.periodEndDate = timePeriod;
                new TimeperiodController().Create(newTimePeriod);
            }
            // Check if employee already has this timeperiod
            // If not create it
            var employeeTPexists = from tp in db.employeetimeperiods
                                   where tp.Employee_employeeID == emp && tp.TimePeriod_periodEndDate == timePeriod
                                   select tp;
            if (!employeeTPexists.Any())
            {
                employeetimeperiod etp = new employeetimeperiod();
                etp.Employee_employeeID = emp; etp.TimePeriod_periodEndDate = timePeriod;
                etp.isApproved = false;
                new EmployeeTimeperiodsController().Create(etp);
            }
            foreach (var entry in submittedHours.HoursWorked)
            {
                if (entry.isDeleted)
                {
                    hoursworked deleteEntry = db.hoursworkeds.Find(entry.entryID);
                    if (deleteEntry != null)
                    {
                        new HoursWorkedController().DeleteConfirmed(deleteEntry.entryID);
                    }
                }
                else if (entry.hours > 0 && entry.Activity_activityID != 0 && entry.Project_projectID != 0)
                {
                    if (entry.entryID == 0)
                    {
                        // Check if the activity was already logged on the current date
                        // If it was, just add hours to the entry
                        // Otherwise, create a new entry
                        hoursworked newEntry = new hoursworked();
                        DateTime periodEndDate = AddToWeek;
                        newEntry.hours = entry.hours; newEntry.Project_projectID = entry.Project_projectID; newEntry.Activity_activityID = entry.Activity_activityID; newEntry.date = ExtensionMethods.GetDateInWeek(periodEndDate, entry.currentDay);
                        newEntry.comments = entry.comments; newEntry.TimePeriod_employeeID = emp; newEntry.TimePeriod_periodEndDate = periodEndDate.Date;

                        if (ExtensionMethods.EntryExists(newEntry) == null)
                        {
                            new HoursWorkedController().Create(newEntry);
                        }
                        else
                        {
                            hoursworked existingEntry = ExtensionMethods.EntryExists(newEntry);
                            existingEntry.hours += newEntry.hours;
                            existingEntry.comments += ", " + newEntry.comments;
                            new HoursWorkedController().AddHours(existingEntry);
                        }
                    }
                    // Updating already existing entry
                    else
                    {
                        // If we get to this point, entry is already saved to DB, and we're editing it
                        hoursworked existingEntry = db.hoursworkeds.First(hw => hw.entryID == entry.entryID);
                        existingEntry.Activity_activityID = entry.Activity_activityID; existingEntry.activity = entry.activity;
                        existingEntry.Project_projectID = entry.Project_projectID; existingEntry.project = entry.project;
                        existingEntry.hours = entry.hours;
                        existingEntry.comments = entry.comments;
                        existingEntry.date = entry.date;
                        new HoursWorkedController().AddHours(existingEntry);
                    }
                }

            } 
            if (submitButton == "Previous")
            {
                submittedHours.currentWeekEndDate = submittedHours.currentWeekEndDate.AddDays(-7);
            }
            else if(submitButton == "Next")
            {
                submittedHours.currentWeekEndDate = submittedHours.currentWeekEndDate.AddDays(7);
            }

            ModelState.Clear();
            HomeViewModel myModel = new HomeViewModel();
            myModel.currentWeekEndDate = submittedHours.currentWeekEndDate;
            var query = from hw in db.hoursworkeds
                        where hw.TimePeriod_employeeID == UserManager.User.employeeID && DbFunctions.TruncateTime(hw.TimePeriod_periodEndDate) == myModel.currentWeekEndDate.Date
                        select hw;
            myModel.HoursWorked = query.ToList(); 
            foreach(var hw in myModel.HoursWorked){
                hw.currentDay = hw.date.DayOfWeek;
            }
            // If Timeperiod was already approved, set Viewbag isApproved to true
            var isApprovedQuery = from a in db.employeetimeperiods
                                  where a.Employee_employeeID == UserManager.User.employeeID && DbFunctions.TruncateTime(a.TimePeriod_periodEndDate) == myModel.currentWeekEndDate.Date
                                  select a;
            if (isApprovedQuery.Any())
            {
                myModel.isApproved = isApprovedQuery.First().isApproved;
            }
            // Get list of active projects that group is working on 
            List<project> groupProjects = GetGroupProjects(emp.ToString());
            foreach (var hrworked in myModel.HoursWorked)
            {
                hrworked.ProjectList = groupProjects;
            }
            myModel.ProjectList = groupProjects;

            return View("Index", myModel);
        }


        public ActionResult AddHourWorked(int entryDay)
        {
            var emp = UserManager.User.employeeID;
            hoursworked newHour = new hoursworked();
            newHour.currentDay = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), entryDay.ToString());
            newHour.ProjectList = GetGroupProjects(emp.ToString());
            return PartialView("_hoursworkedrow", newHour);
        }

        //Method will update the activities dropdownlist in index page when specific project is picked
        [HttpPost]
        public ActionResult FillActivities(int projectID)
        {
            List<activity> objactivity = new List<activity>();
            objactivity = db.projects.Find(projectID).activities.ToList();
            SelectList myActivities = new SelectList(objactivity, "activityID", "activityName", 0);
            return Json(myActivities);
        }

        public ActionResult Monthly()
        {
            MonthlyViewModel monthlyModel = new MonthlyViewModel();
            return View(monthlyModel);
        }


        //gets projects for the calender
        public JsonResult GetEvents(string empId)
        {
            //create an employee object mathcing the empId passed through
            employee emp = db.employees.Find(empId);
            var dbGrps = db.groups.ToList();
            //all groups where the employee is within the list of employees assigned to the group.
            var groups = (from s in dbGrps where s.employees.Contains(emp) select s).ToList();

            //list of projects to return
            List<project> returnProj = new List<project>();

            //for each group found for that employee
            foreach (group grp in groups)
            {
                group grpObject = db.groups.Find(grp.groupID);
                var dbProjects = db.projects.ToList();
                //you find a list of projects where the project has that group assigned to it. 
                List<project> projects = (from s in dbProjects where s.groups.Contains(grpObject) select s).ToList();

                foreach (project proj in projects)
                {
                    //for each of those projects, add them to the returnProj list. 
                    if (proj.projectActive == 1)
                    {
                        returnProj.Add(new project()
                        {
                            projectID = proj.projectID,
                            projectName = proj.projectName,
                            projectStartDate = proj.projectStartDate,
                            projectEndDate = proj.projectEndDate
                        });
                    }
                }
            }

            //return a list of projects defined above
            return Json(returnProj, JsonRequestBehavior.AllowGet);
        }

        //GetWeeklyHours
        [HttpGet]
        public JsonResult GetWeeklyHours(string empId, int monthOffSet)
        {
            //empId = "Amill";
            DateTime date = DateTime.Now.AddMonths(monthOffSet);
            var StartDate = new DateTime(date.Year, date.Month, 1);
            var EndDate = StartDate.AddMonths(1).AddDays(-1);
            double numDays = (EndDate - StartDate).TotalDays;
            var dailyHours = 0;
            bool isEnd = false;

            List<MonthlyViewModel> hrsWrkd = new List<MonthlyViewModel>();
            var dbHours = db.hoursworkeds.ToList();
            for (int i=0; i < numDays; i++)
            {
                List<hoursworked> hrs = (from s in dbHours where s.TimePeriod_employeeID == empId && s.date == StartDate.AddDays(i) select s).ToList();
                foreach(hoursworked hrsW in hrs)
                {
                    dailyHours = dailyHours + (int)hrsW.hours;
                }
                if (StartDate.AddDays(i) == EndDate.AddDays(-1))
                {
                     isEnd = true;
                }
                else
                {
                    isEnd = false;
                }
                hrsWrkd.Add(new MonthlyViewModel()
                {
                    HrsWorked = dailyHours.ToString(),
                    entryDate = StartDate.AddDays(i),
                    isLastDay = isEnd

                });
                dailyHours = 0;
            }
            return Json(hrsWrkd, JsonRequestBehavior.AllowGet);
    }


        //This will take in a project ID, and EmployeeID, and get the hours worked on each activity for a day
        [HttpGet]
        public JsonResult GetHoursWorkedPerDay(DateTime date, string empId)
        {
            //empId = "Amill";
            //Still need to pass through employee ID, this will do fore now
            // same with date
            //sempId = "Amill";
            //DateTime date = DateTime.Today.AddDays(-1);

            //creates a list of all hours worked for that employee, on the date passed through
            List<hoursworked> hrs = (from s in db.hoursworkeds where s.TimePeriod_employeeID == empId && s.date == date select s).ToList();
            //using this to add each entity of hours worked to store in a list.
            List<MonthlyViewModel> hrsWrkd = new List<MonthlyViewModel>();

            project proj;
            employee emp;

            //for each set of hours worked we have, we will get a list of activities that were worked on those hours.
            foreach (hoursworked hrsW in hrs)
            {
                
                List<activity> activities = (from s in this.activityRepository.All where s.activityID == hrsW.Activity_activityID select s).ToList();

                //for each of those activitires, we will find the project of the activity worked on, and add  all of 
                //the cooresponding varaibles we need to the HrsWrked list
                foreach (activity activ in activities)
                {
                    proj = db.projects.Find(hrsW.Project_projectID);
                    emp = db.employees.Find(empId);
                    hrsWrkd.Add(new MonthlyViewModel()
                    {
                        ActivityName = activ.activityName,
                        HrsWorked = hrsW.hours.ToString(),
                        ProjectName = proj.projectName,
                        //isAdmin = 
                    });
                }
            }

            //return a list of models that hold all the varaibles we need. Each object in the list stores activity name, hrs worked,
            //and project name on the date passed through date. 
            return Json(hrsWrkd, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //Return Favorite View with All activities and User's Favorites
        [HttpGet]
        public ActionResult Favorite()
        {
            var userloggedIn = UserManager.User.employeeID;
            ViewBag.Message = "Your Favorites Page";
            FavoriteViewModel myModel = new FavoriteViewModel();
            myModel.Activities = db.activities.ToList();
            myModel.Favorites = db.favorites.ToList();
            myModel.ActivityNames = new SelectList(myModel.Activities, "activityID", "activityName");
            var query = from a in db.activities
                        join b in db.favorites
                        on a.activityID equals b.Activity_activityID
                        where b.Employee_employeeID.Equals(userloggedIn)
                        select new FavoriteViewModel { SelectedActivity = a, SelectedFavorite = b };
            myModel.UserFavorites = new SelectList(query.ToArray(), "SelectedFavorite.favoriteID", "SelectedActivity.ActivityName");
            return View(myModel);
        }

        // Add Selected Activity to Favorite Table
        [HttpPost]
        public ActionResult AddFavorite(FavoriteViewModel favorite)
        {
            //Need to make sure it's not adding favorites that already exist
            ViewBag.Message = "Your Favorites Page";
            var userloggedIn = UserManager.User.employeeID;
            int selectedActivity = favorite.selectedActivityID;
            if (selectedActivity != 0)
            {  
                favorite AddFavorite = new favorite();
                AddFavorite.Activity_activityID = selectedActivity;
                AddFavorite.Employee_employeeID = userloggedIn;
                // Check if selected activity is already a favorite. If not, add to Favorite Table
                var existsQuery = from f in db.favorites
                                  where (f.Activity_activityID.Equals(AddFavorite.Activity_activityID) && f.Employee_employeeID.Equals(userloggedIn))
                                  select f;
                if (!existsQuery.Any())
                {
                    new FavoriteController().Create(AddFavorite);
                }

            }
            favorite.Activities = db.activities.ToList();
            favorite.ActivityNames = new SelectList(favorite.Activities, "activityID", "activityName");
            var query = from a in db.activities
                        join b in db.favorites
                        on a.activityID equals b.Activity_activityID
                        where b.Employee_employeeID.Equals(userloggedIn)
                        select new FavoriteViewModel { SelectedActivity = a, SelectedFavorite = b };
            favorite.UserFavorites = new SelectList(query.ToArray(), "SelectedFavorite.favoriteID", "SelectedActivity.ActivityName");
            return View("Favorite", favorite);
        }

        // Remove Selected Favorite from Table
        [HttpPost]
        public ActionResult RemoveFavorite(FavoriteViewModel favorite)
        {
            var userloggedIn = UserManager.User.employeeID;
            ViewBag.Message = "Your Favorites Page";
            if (favorite != null)
            {
                int[] removeFav = favorite.RemoveFavorites;
                if (removeFav!= null)
                {
                    for (int i = 0; i < removeFav.Length; i++)
                    {
                        if (removeFav[i] != 0)
                        {
                            new FavoriteController().DeleteConfirmed(removeFav[i]);
                        }
                    }
                }  
            }
            favorite = new FavoriteViewModel();
            favorite.Activities = db.activities.ToList();
            favorite.ActivityNames = new SelectList(favorite.Activities, "activityID", "activityName");
            var query = from a in db.activities
                        join b in db.favorites
                        on a.activityID equals b.Activity_activityID
                        where b.Employee_employeeID.Equals(userloggedIn)
                        select new FavoriteViewModel { SelectedActivity = a, SelectedFavorite = b };
            favorite.UserFavorites = new SelectList(query.ToArray(), "SelectedFavorite.favoriteID", "SelectedActivity.ActivityName");
            return View("Favorite", favorite);
        }

        [HttpGet]
        public ActionResult Workspace()
        {
            var groups = db.groups.Select(g => g).Where(g => g.employees.Select(e => e.employeeID).Contains(UserManager.User.employeeID)).ToList();
            return View(groups);
        }
    }

    public static class ExtensionMethods
    {
        public static DateTime Next(this DateTime from, DayOfWeek dayOfWeek)
        {
            if (from.DayOfWeek == DayOfWeek.Sunday)
            {
                return from;
            }
            int start = (int)from.DayOfWeek;
            int target = (int)dayOfWeek;
            if (target <= start)
                target += 7;
            return from.AddDays(target - start);
        }

        public static DateTime GetDateInWeek(this DateTime end, DayOfWeek dayOfWeek)
        {
            int day = (int)dayOfWeek;
            if (day == 0) { day = 7; }
            DateTime date = end.Date.AddDays(-(7 - day));
            return date;
        }
        public static hoursworked EntryExists(hoursworked entry)
        {
            var existsQuery = from hw in new CronusDatabaseEntities().hoursworkeds
                              where (hw.TimePeriod_employeeID.Equals(entry.TimePeriod_employeeID) && DbFunctions.TruncateTime(hw.date) == DbFunctions.TruncateTime(entry.date) && hw.Project_projectID == entry.Project_projectID && hw.Activity_activityID == entry.Activity_activityID)
                              select hw;
            if (existsQuery.Any())
            {
                return existsQuery.First();
            }
            return null;
        }

        
    }


    
}