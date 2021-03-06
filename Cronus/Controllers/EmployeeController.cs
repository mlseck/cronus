﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DatabaseEntities;
using Cronus.Models;
using System.Data.Entity.Validation;

namespace Cronus.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IFavoriteRepository favoriteRepository;
        private readonly IemployeeRepository employeeRepository;
        private readonly IGroupRepository groupRepository;


        // If you are using Dependency Injection, you can delete the following constructor
        public EmployeeController() : this(new EmployeeRepository(), new FavoriteRepository(), new GroupRepository())
        {
        }

        public EmployeeController(IemployeeRepository employeeRepository, IFavoriteRepository favoriteRepository, IGroupRepository groupRepository)
        {
            this.employeeRepository = employeeRepository;
            this.favoriteRepository = favoriteRepository;
            this.groupRepository = groupRepository;
        }

        //
        // GET: /employee/

        public ViewResult Index(string searchString)
        {
            ViewBag.PossibleGrous = groupRepository.All.ToList();

            //ViewBag.GroupManager = id;

            var employeeList = employeeRepository.AllIncluding(employee => employee.favorites);

            if (!String.IsNullOrEmpty(searchString))
            {
                employeeList = employeeList.Where(s => s.employeeLastName.Contains(searchString));
            }

            return View(employeeList);
        }

        public ActionResult EmployeeIndex(int id)
        {
            ViewBag.GroupId = id;

            var group = groupRepository.FindGroup(id);

            if (group == null)
            {
                var newEmployees = new List<employee>();

                return PartialView("_index", newEmployees.ToList());
            }
            string[] employeesIds = group.employees.Select(x => x.employeeID).ToArray();

            var employeess = from s in this.employeeRepository.All where employeesIds.Contains(s.employeeID) select s;

            return PartialView("_index", employeess.ToList());

        }

        //
        // GET: /employee/Details/5

        public ViewResult Details(string id)
        {
            return View(employeeRepository.Find(id));
        }

        //
        // GET: /employee/Create

        public ActionResult Create()
        {
            employee model = new employee
            {
                favoriteIds = new int[0]
            };

            model.privileges = new List<SelectListItem>
        {
            new SelectListItem { Text = "Administrator", Value = "1" },
            new SelectListItem { Text = "Employee", Value = "0" }
        };

            ViewBag.PossibleFavorites = favoriteRepository.All;

            return View(model);
        }

        //
        // POST: /employee/Create

        [HttpPost]
        public ActionResult Create(employee employee)
        {
            if (ModelState.IsValid)
            {

                if (employee.favoriteIds != null)
                {
                    employee.favorites = (from s in this.favoriteRepository.All where employee.favoriteIds.Contains(s.favoriteID) select s).ToList();
                }

                employeeRepository.InsertOrUpdate(employee);
                employeeRepository.Save();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.PossibleActivities = favoriteRepository.All;
                return View(employee);
            }
        }


        public ActionResult AddEmployees(int GroupId)
        {
            //iewBag.ProjectID = ProjectID;

            group model = groupRepository.FindGroup(GroupId);

            ViewBag.PossibleEmployees = employeeRepository.All;

            return PartialView("EmployeeCheckList", model);
        }




        [HttpPost]
        public ActionResult AddEmployees(group group)
        {
            if (ModelState.IsValid)
            {
                group originalGroup = this.groupRepository.FindGroup(group.groupID);

                originalGroup.groupName = group.groupName;

                //originalGroup.groupManager = group.groupManager;


                originalGroup.employees.Clear();

                if (group.employeeIds != null)
                {
                    originalGroup.employees = (from s in this.employeeRepository.All where @group.employeeIds.Contains(s.employeeID) select s).ToList();
                }

                groupRepository.InsertOrUpdate(originalGroup);

                try
                {
                    // Your code...
                    // Could also be before try if you know the exception occurs in SaveChanges

                    groupRepository.Save();
                }
                catch (DbEntityValidationException e)
                {

                }

                //EmployeeRepository.Save();




                string url = Url.Action("EmployeeIndex", "Employee", new { id = originalGroup.groupID });
                return Json(new { success = true, url = url });
            }
            else
            {
                ViewBag.PossibleEmployees = employeeRepository.All;
                return PartialView("AddEmployees", group);
            }
        }
        //
        // GET: /employee/Edit/5

        public ActionResult Edit(string id)
        {
            ViewBag.PossibleActivities = favoriteRepository.All;

            employee model = employeeRepository.Find(id);

            model.favoriteIds = (from s in model.favorites select s.favoriteID).ToArray();

            model.privileges = new List<SelectListItem>
        {
            new SelectListItem { Text = "Adminstrator", Value = "1" },
            new SelectListItem { Text = "Employee", Value = "0" }
        };

            return View(model);
        }

        //
        // POST: /employee/Edit/5

        [HttpPost]
        public ActionResult Edit(employee employee)
        {
            if (ModelState.IsValid)
            {
                employee originalemployee = this.employeeRepository.Find(employee.employeeID);

                originalemployee.employeeFirstName = employee.employeeFirstName;
                originalemployee.employeeLastName = employee.employeeLastName;
                originalemployee.employeeEmailAddress = employee.employeeEmailAddress;
                originalemployee.managesgroup = employee.managesgroup;
                originalemployee.employeeMaxHours = employee.employeeMaxHours;
                originalemployee.employeeMinHours = employee.employeeMinHours;
                originalemployee.employeePrivileges = employee.employeePrivileges;
                originalemployee.employeePwd = employee.employeePwd;
                //originalemployee. = employee.employeeGroupManaged;


                //originalemployee.favorites.Clear();

                //if (employee.favoriteIds != null)
                //{
                //    originalemployee.favorites = (from s in this.favoriteRepository.All where employee.favoriteIds.Contains(s.favoriteID) select s).ToList();
                //}

                employeeRepository.InsertOrUpdate(originalemployee);
                employeeRepository.Save();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.PossibleActivities = favoriteRepository.All;
                return View(employee);
            }

        }

        //
        // GET: /employee/Delete/5

        public ActionResult Delete(string id)
        {
            return View(employeeRepository.Find(id));
        }

        //
        // POST: /employee/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            employeeRepository.Delete(id);
            employeeRepository.Save();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                employeeRepository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
