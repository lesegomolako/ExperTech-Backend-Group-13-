﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Dynamic;
using System.Data.Entity;
using ExperTech_Api.Models;
using System.Web.Http.Cors;
using System.Web;

namespace ExperTech_Api.Controllers
{
    public class EmployeeController : ApiController
    {
        public ExperTechEntities db = new ExperTechEntities();

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("api/Employees/getEmployee")]
        [HttpGet]

        //*****************************read employee*************************************
        public List<dynamic> getEmployee()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getEmployeeID(db.Employees.ToList());
        }

        private List<dynamic> getEmployeeID(List<Employee> forEmployee)
        {
            List<dynamic> dynamicEmployees = new List<dynamic>();
            foreach (Employee employeename in forEmployee)
            {
                dynamic dynamicEmployee = new ExpandoObject();
                dynamicEmployee.EmployeeID = employeename.EmployeeID;
                dynamicEmployee.Name = employeename.Name;
                dynamicEmployee.Surname = employeename.Surname;
                dynamicEmployee.ContactNo = employeename.ContactNo;
                dynamicEmployee.Email = employeename.Email;

                dynamicEmployees.Add(dynamicEmployee);
            }
            return dynamicEmployees;
        }

        //*********************************Refiloe's stuff****************************

        [Route("api/Employees/DisplayEmployeeSchedule")]
        [HttpGet]
        public dynamic DisplayEmployeeSchedule(string SessionID)
        {
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser != null)
            {
                if (findUser.RoleID == 3)
                {
                    int findEmpID = db.Employees.Where(zz => zz.UserID == findUser.UserID).Select(zz => zz.EmployeeID).FirstOrDefault();
                    db.Configuration.ProxyCreationEnabled = false;
                    List<EmployeeSchedule> findSchedule = db.EmployeeSchedules.Where(zz => zz.EmployeeID == findEmpID).ToList();
                    return GetSchedule(findSchedule);
                }
                else
                {
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Error = "authorized";
                    toReturn.Message = "User is not authorized";
                    return toReturn;
                }
            }
            else
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is not valid";
                return toReturn;
            }
        }

        private dynamic GetSchedule(List<EmployeeSchedule> Modell)
        {
            List<Date> Dates = db.Dates.ToList();
            List<dynamic> getList = new List<dynamic>();
            dynamic result = new ExpandoObject();

            for (int j = 0; j < Dates.Count; j++)
            {
                dynamic newObject = new ExpandoObject();
                newObject.DateID = Dates[j].DateID;
                newObject.Dates = Dates[j].Date1;
                List<dynamic> getTimes = new List<dynamic>();

                foreach (EmployeeSchedule Items in Modell)
                {
                    if (Items.DateID == Dates[j].DateID)
                    {
                        if (Items.StatusID == 1)
                        {
                            dynamic ScheduleObject = new ExpandoObject();
                            ScheduleObject.BookingID = Items.BookingID;
                            ScheduleObject.TimeID = Items.TimeID;
                            Timeslot findTime = db.Timeslots.Where(zz => zz.TimeID == Items.TimeID).FirstOrDefault();
                            ScheduleObject.StartTime = findTime.StartTime;
                            ScheduleObject.EndTime = findTime.EndTime;
                            ScheduleObject.StatusID = Items.StatusID;
                            getTimes.Add(ScheduleObject);
                        }
                        else if(Items.StatusID == 2)
                        {
                            dynamic ScheduleObject = new ExpandoObject();
                            ScheduleObject.BookingID = Items.BookingID;
                            ScheduleObject.TimeID = Items.TimeID;
                            Timeslot findTime = db.Timeslots.Where(zz => zz.TimeID == Items.TimeID).FirstOrDefault();
                            ScheduleObject.StartTime = findTime.StartTime;
                            ScheduleObject.EndTime = findTime.EndTime;
                            ScheduleObject.StatusID = Items.StatusID;
                            getTimes.Add(ScheduleObject);
                        }
                        else
                        {
                            dynamic ScheduleObject = new ExpandoObject();
                            ScheduleObject.BookingID = Items.BookingID;
                            ScheduleObject.TimeID = Items.TimeID;
                            Timeslot findTime = db.Timeslots.Where(zz => zz.TimeID == Items.TimeID).FirstOrDefault();
                            ScheduleObject.StartTime = findTime.StartTime;
                            ScheduleObject.EndTime = findTime.EndTime;
                            ScheduleObject.StatusID = Items.StatusID;
                            getTimes.Add(ScheduleObject);
                        }

                    }
                }
                newObject.Times = getTimes;
                getList.Add(newObject);
            }

            return getList;
        }

        //*************************read employee availability details*********************
        [Route("api/Employees/getTime")]
        [HttpGet]
        public dynamic getTime()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Timeslot> findTime = db.Timeslots.ToList();
            return findTime;
        }

        [Route("api/Employee/getDate")]
        [HttpGet]
        public List<Date> getDate()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Date> findDate = db.Dates.ToList();
            return findDate;
        }

        //***********************************************view availability******************************************************
        [Route("api/Emoloyee/ViewAvailability")]
        [HttpGet]
        public List<dynamic> ViewAvailability()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return ViewAvailabilityID(db.EmployeeSchedules.ToList());
        }

        private List<dynamic> ViewAvailabilityID(List<EmployeeSchedule> forES)
        {
            List<dynamic> dynamicESs = new List<dynamic>();
            foreach(EmployeeSchedule esname in forES)
            {
                dynamic dynamicES = new ExpandoObject();
                dynamicES.TimeID = esname.TimeID;
                dynamicES.Booking = esname.Booking;
                dynamicES.BookingID = esname.BookingID;
                dynamicES.DateID = esname.DateID;
                dynamicES.Schedule = esname.Schedule;
                dynamicES.ScheduleStatu = esname.ScheduleStatu;
                dynamicES.Employee = esname.Employee;
                dynamicES.EmployeeID = esname.EmployeeID;

                dynamicESs.Add(dynamicES);
            }
            return dynamicESs;
        }
        //********************************************final availability******************************************************
        public class Availably
        {
            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }
            public int StartTimeID { get; set; }
            public int EndTimeID { get; set; }

            public int Avail { get; set; }


        }
        [Route("api/Employee/EmployeeAvailability")]
        [HttpPost]
        public dynamic EmployeeAvailability([FromBody]Availably Stuff, string SessionID)
        {
            //var httpRequest = HttpContext.Current.Request;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser != null)
            {
                try
                {
                    DateTime StartDate = Stuff.StartDate;
                    DateTime EndDate = Stuff.EndDate;


                    int Avail = Stuff.Avail;

                    //TimeSpan StartTime = Convert.ToDateTime(Stuff.StartTime);
                    //TimeSpan EndTime = Convert.ToDateTime(Stuff.EndTime);

                    int StartDateID = db.Dates.Where(zz => zz.Date1 == StartDate.Date).Select(zz => zz.DateID).FirstOrDefault();
                    int EndDateID = db.Dates.Where(zz => zz.Date1 == EndDate.Date).Select(zz => zz.DateID).FirstOrDefault();

                    int StartTimeID = Stuff.StartTimeID;
                    int EndTimeID = Stuff.EndTimeID;

                    for (int j = StartDateID; j < EndDateID; j++)
                    {
                        List<EmployeeSchedule> findSchedule = db.EmployeeSchedules.Where(zz => zz.DateID == j).ToList();
                        for (int kk = StartTimeID; kk < EndTimeID; kk++)
                        {
                            for (int k = 0; k < findSchedule.Count; k++)
                            {
                                if (Avail == 1)
                                {

                                    if (findSchedule[k].TimeID == kk)
                                    {
                                        findSchedule[k].StatusID = 1;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        findSchedule[k].StatusID = 2;
                                        db.SaveChanges();
                                    }

                                }
                                else if(Avail == 2)
                                {
                                    if (findSchedule[k].TimeID == kk)
                                    {
                                        findSchedule[k].StatusID = 2;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        findSchedule[k].StatusID = 1;
                                        db.SaveChanges();
                                    }
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                    return "success";
                }
                catch (Exception err)
                {
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Error = "error";
                    toReturn.Message = err.Message;
                    return toReturn;
                }
            }
            else
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is not vallid";
                return toReturn;
            }
        }
        //*******************************************update employee type*********************************************
        [Route("api/Employees/updateEST")]
        [HttpPut]
        public object updateEST([FromBody] ServiceType forEST)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                ServiceType serviceType = db.ServiceTypes.Find(forEST.TypeID);   //lol
                if (serviceType != null)
                {
                    serviceType.TypeID = forEST.TypeID;
                    serviceType.Name = forEST.Name;
                    serviceType.Description = forEST.Description;
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return Ok(forEST);
        }

        [Route("api/Employees/RetrieveEmployeeBooking")]
        [HttpGet]
        public dynamic RetrieveEmployeeBooking(string SessionID)
        {
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser != null)
            {
                int EmployeeID = db.Employees.Where(zz => zz.UserID == findUser.UserID).Select(zz => zz.EmployeeID).FirstOrDefault();
                db.Configuration.ProxyCreationEnabled = false;
                List<EmployeeSchedule> findBookings = db.EmployeeSchedules.Include(zz => zz.Booking).Where(zz => zz.StatusID == 3 && zz.EmployeeID == EmployeeID).ToList();
                //List<Booking> findBookings = db.Bookings.Include(zz => zz.BookingLines).Include(zz => zz.EmployeeSchedules).Include(zz => zz.Client)
                //    .Include(zz => zz.DateRequesteds).Include(zz => zz.BookingNotes).Where(zz => zz.EmployeeSchedules.Contains(EmployeeID)).ToList();
                return formatBookings(findBookings);
            }
            else
            {
                return "Session is no longer valid";
            }
        }

        private dynamic formatBookings(List<EmployeeSchedule> Modell)
        {
            List<dynamic> BookingList = new List<dynamic>();
            foreach (EmployeeSchedule items in Modell)
            {
                if (items.Booking.StatusID == 4)
                {
                    dynamic BookingObject = new ExpandoObject();
                    BookingObject.BookingID = items.BookingID;
                    BookingObject.BookingStatusID = items.Booking.StatusID;
                    BookingObject.BookingStatus = db.BookingStatus.Where(zz => zz.StatusID == items.Booking.StatusID).Select(zz => zz.Status).FirstOrDefault();
                    BookingObject.Client = db.Clients.Where(zz => zz.ClientID == items.Booking.ClientID).Select(zz => zz.Name).FirstOrDefault();


                    List<dynamic> newList = new List<dynamic>();
                    dynamic newObject = new ExpandoObject();
                    newObject.DateID = items.DateID;
                    newObject.Employee = db.Employees.Where(zz => zz.EmployeeID == items.EmployeeID).Select(zz => zz.Name).FirstOrDefault();

                    DateTime getDate = db.Dates.Where(zz => zz.DateID == items.DateID).Select(zz => zz.Date1).FirstOrDefault();
                    newObject.Dates = getDate;

                    TimeSpan getTime = db.Timeslots.Where(zz => zz.TimeID == items.TimeID).Select(zz => zz.StartTime).FirstOrDefault();
                    newObject.StartTime = getTime;

                    newObject.EndTime = db.Timeslots.Where(zz => zz.TimeID == items.TimeID).Select(zz => zz.EndTime).FirstOrDefault();
                    newObject.Status = db.ScheduleStatus.Where(zz => zz.StatusID == items.StatusID).Select(zz => zz.Status).FirstOrDefault();

                    DateTime makeDT = getDate + getTime;
                    newObject.DateTime = makeDT;
                    newList.Add(newObject);
                    BookingObject.BookingSchedule = newList;

                    List<BookingLine> findLine = db.BookingLines.Where(zz => zz.BookingID == items.BookingID).ToList();

                    List<dynamic> getLines = new List<dynamic>();
                    foreach (BookingLine lineItems in findLine)
                    {
                        dynamic lineObject = new ExpandoObject();
                        lineObject.Service = db.Services.Where(zz => zz.ServiceID == lineItems.ServiceID).Select(zz => zz.Name).FirstOrDefault(); ;
                        lineObject.Option = db.ServiceOptions.Where(zz => zz.OptionID == lineItems.OptionID).Select(zz => zz.Name).FirstOrDefault(); ;

                        getLines.Add(lineObject);
                    }
                    BookingObject.BookingLines = getLines;


                    BookingList.Add(BookingObject);
                }
            }

            return BookingList;
        }

        [Route("api/Employees/deleteEmployee")]
        [HttpDelete]
        public dynamic deleteEmployee(string SessionID, int EmployeeID)
        {
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if(findUser == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is not valid";
                return toReturn;
            }

            Employee findEmployee = db.Employees.Include(zz => zz.EmployeeSchedules).Include(zz => zz.EmployeeServiceTypes)
                .Include(zz => zz.EmployeeAuditTrails).Where(zz => zz.EmployeeID == EmployeeID).FirstOrDefault();

            List<EmployeeServiceType> findServiceTypes = db.EmployeeServiceTypes.Where(zz => zz.EmployeeID == EmployeeID).ToList();
            List<EmployeeSchedule> findSchedule = db.EmployeeSchedules.Where(zz => zz.EmployeeID == EmployeeID).ToList();
            List<EmployeeAuditTrail> findAudit = db.EmployeeAuditTrails.Where(zz => zz.EmployeeID == EmployeeID).ToList();
            User findEmpUser = db.Users.Where(zz => zz.UserID == findEmployee.UserID).FirstOrDefault();

            bool UserDeleted = false;
            bool ScheduleDeleted = false;
            bool ServiceTypesDeleted = false;
            bool AuditDeleted = false;


            if(findEmployee != null)
            {
                foreach(EmployeeSchedule items in findSchedule)
                {
                    if(items.BookingID != null)
                    {
                        Booking findBooking = db.Bookings.Where(zz => zz.BookingID == items.BookingID).FirstOrDefault();
                        findBooking.StatusID = 5;
                        db.SaveChanges();
                    }
                }

                if(findServiceTypes != null)
                {
                    db.EmployeeServiceTypes.RemoveRange(findServiceTypes);
                    ServiceTypesDeleted = true;
                   
                }

                if(findSchedule != null)
                {
                    db.EmployeeSchedules.RemoveRange(findSchedule);
                    ScheduleDeleted = true;
                }

                if(findAudit != null)
                {
                    db.EmployeeAuditTrails.RemoveRange(findAudit);
                    AuditDeleted = true;
                }

                if (findEmpUser != null)
                {
                    db.Users.Remove(findEmpUser);
                    UserDeleted = true;
                }

                if(UserDeleted && AuditDeleted && ServiceTypesDeleted && ScheduleDeleted)
                {
                    db.Employees.Remove(findEmployee);
                    db.SaveChanges();

                    return "success";
                }
                else
                {
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Error = "deletion";
                    toReturn.Message = "Error deleting Employee";
                    return toReturn;
                }

                
            }
            else
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "not found";
                toReturn.Message = "Employee details invalid";
                return toReturn;
            }
        }
    }
}
