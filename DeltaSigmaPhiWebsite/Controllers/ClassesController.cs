﻿namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, Academics")]
    public class ClassesController : BaseController
    {
        public ClassesController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        public ActionResult Index()
        {
            var classes = uow.ClassesRepository.SelectAll();
            return View(classes.ToList());
        }
        
        public ActionResult Create()
        {
            ViewBag.DepartmentId = new SelectList(uow.DepartmentsRepository.SelectAll(), "DepartmentId", "DepartmentName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ClassId,DepartmentId,CourseShorthand,CourseName")] Class @class)
        {
            if (ModelState.IsValid)
            {
                uow.ClassesRepository.Insert(@class);
                uow.Save();
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentId = new SelectList(uow.DepartmentsRepository.SelectAll(), "DepartmentId", "DepartmentName", @class.DepartmentId);
            return View(@class);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Class @class = uow.ClassesRepository.SingleById(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(uow.DepartmentsRepository.SelectAll(), "DepartmentId", "DepartmentName", @class.DepartmentId);
            return View(@class);
        }

        // POST: Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ClassId,DepartmentId,CourseShorthand,CourseName")] Class @class)
        {
            if (ModelState.IsValid)
            {
                uow.ClassesRepository.Update(@class);
                uow.Save();
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentId = new SelectList(uow.DepartmentsRepository.SelectAll(), "DepartmentId", "DepartmentName", @class.DepartmentId);
            return View(@class);
        }

        // GET: Classes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Class @class = uow.ClassesRepository.SingleById(id);
            if (@class == null)
            {
                return HttpNotFound();
            }
            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            uow.ClassesRepository.DeleteById(id);
            uow.Save();
            return RedirectToAction("Index");
        }
    }
}
