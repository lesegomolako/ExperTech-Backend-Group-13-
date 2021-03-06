﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Text;
using System.Dynamic;
using ExperTech_Api.Models;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Data.Entity;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace ExperTech_Api.Controllers
{
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        
        public static void SMS(string body, string cell)
        {
            // Find your Account Sid and Token at twilio.com/console
            // DANGER! This is insecure. See http://twil.io/secure
            const string accountSid = "ACa877daffacbbb1f20121558340d54b46";
            const string authToken = "254deeb3c88430c6e6735ee970cacf96";

            TwilioClient.Init(accountSid, authToken);

            try
            {
                var message = MessageResource.Create(
                    body: body,
                    from: new Twilio.Types.PhoneNumber("+12057841821"),
                    to: new Twilio.Types.PhoneNumber(cell)
                );
                Console.WriteLine(message.Sid);
            }
            catch
            {
                
            }

            
        }

        

        public class Passwords
        {
            public string newPassword { get; set; }

            public string oldPassword { get; set; }
        }

        [Route("ChangePassword")]
        [HttpPost]
        public dynamic ChangePassword([FromBody]Passwords change, string SessionID)
        {
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if(findUser == null)
            {
                return SessionError();
            }

            try
            {
                var OldPass = GenerateHash(ApplySomeSalt(change.oldPassword));
                var newPass = GenerateHash(ApplySomeSalt(change.newPassword));

                if (OldPass == findUser.Password)
                {
                    findUser.Password = newPass;
                    db.SaveChanges();
                    return "success";
                }
                else
                {
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Error = "password";
                    toReturn.Message = "The old password entered is incorrect";
                    return toReturn;
                }
            }
            catch(Exception err)
            {
                return err.Message;
            }
        }



        public ExperTechEntities db = new ExperTechEntities();

        [Route("getProfile")]
        [HttpGet]
        public dynamic getProfile(string SessionID)
        {

            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser != null)
            {
                int UserID = findUser.UserID;
                int RoleID = findUser.RoleID;
                switch(RoleID)
                {
                    case 1:
                        User findClient = db.Users.Include(zz => zz.Clients).Where(zz => zz.UserID == UserID).FirstOrDefault();
                        return findProfile(findClient);

                    case 2:
                        User findAdmin = db.Users.Include(zz => zz.Admins).Where(zz => zz.UserID == UserID).FirstOrDefault();
                        return findProfile(findAdmin);

                    case 3:
                        User findEmp = db.Users.Include(zz => zz.Employees).Where(zz => zz.UserID == UserID).FirstOrDefault();
                        return findProfile(findEmp);

                    default:
                        return "User not found";
                }
                
            }
            else
            {
                return "Session is not valid";
            }

        }

        

        public static User CheckUser(string SessionID)
        {
            ExperTechEntities db = new ExperTechEntities();
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if(findUser != null)
            {
                return findUser;
            }
            else
            {
                return null;
            }
        }

        public static dynamic SessionError()
        {
            dynamic toReturn = new ExpandoObject();
            toReturn.Error = "session";
            toReturn.Message = "Session is no longer valid";
            return toReturn;
        }

        private dynamic findProfile(User Modell)
        {
            if(Modell.RoleID == 1)
            {
                dynamic newObject = new ExpandoObject();
                newObject.UserID = Modell.UserID;
                newObject.RoleID = Modell.RoleID;
                List<dynamic> newList = new List<dynamic>();
                foreach(Client items in Modell.Clients)
                {
                    dynamic myObject = new ExpandoObject();
                    myObject.Name = items.Name;
                    myObject.Surname = items.Surname;
                    myObject.Email = items.Email;
                    myObject.ContactNo = items.ContactNo;
                    newList.Add(myObject);
                }
                newObject.Clients = newList;
                return newObject;   
            }
            else if (Modell.RoleID == 2)
            {
                dynamic newObject = new ExpandoObject();
                newObject.UserID = Modell.UserID;
                newObject.RoleID = Modell.RoleID;
                List<dynamic> newList = new List<dynamic>();
                foreach (Admin items in Modell.Admins)
                {
                    dynamic myObject = new ExpandoObject();
                    myObject.Name = items.Name;
                    myObject.Surname = items.Surname;
                    myObject.Email = items.Email;
                    myObject.ContactNo = items.ContactNo;
                    myObject.Owner = items.Owner;
                    newList.Add(myObject);
                }
                newObject.Admins = newList;
                return newObject;
            }
            else if (Modell.RoleID == 3)
            {
                dynamic newObject = new ExpandoObject();
                newObject.UserID = Modell.UserID;
                newObject.RoleID = Modell.RoleID;
                List<dynamic> newList = new List<dynamic>();
                foreach (Employee items in Modell.Employees)
                {
                    dynamic myObject = new ExpandoObject();
                    myObject.Name = items.Name;
                    myObject.Surname = items.Surname;
                    myObject.Email = items.Email;
                    myObject.ContactNo = items.ContactNo;
                    newList.Add(myObject);
                }
                newObject.Employees = newList;
                return newObject;
            }

            return "User not found";
        }

        [Route("updateProfile")]
        [System.Web.Mvc.HttpPost]

        public dynamic updateProfile([FromBody] User forUser)
        {
            db.Configuration.ProxyCreationEnabled = false;
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                User findUser = db.Users.Where(zz => zz.SessionID == forUser.SessionID).FirstOrDefault();

                if (findUser != null)
                {
                    switch (forUser.RoleID)
                    {
                        case 1:
                            {
                                Client editClient = db.Clients.Where(zz => zz.UserID == findUser.UserID).FirstOrDefault();
                                foreach (Client items in forUser.Clients)
                                {
                                    editClient.Name = items.Name;
                                    editClient.Surname = items.Surname;
                                    editClient.Email = items.Email;
                                    editClient.ContactNo = items.ContactNo;
                                }
                                db.SaveChanges();
                                return "success";
                            }
                        case 2:
                            {
                                Admin editAdmin = db.Admins.Where(zz => zz.UserID == findUser.UserID).FirstOrDefault();
                                foreach (Admin items in forUser.Admins)
                                {
                                    editAdmin.Name = items.Name;
                                    editAdmin.Surname = items.Surname;
                                    editAdmin.Email = items.Email;
                                    editAdmin.ContactNo = items.ContactNo;
                                }
                                db.SaveChanges();
                                return "success";
                            }
                        case 3:
                            {
                                Employee editEmp = db.Employees.Where(zz => zz.UserID == findUser.UserID).FirstOrDefault();
                                foreach (Employee items in forUser.Employees)
                                {
                                    editEmp.Name = items.Name;
                                    editEmp.Surname = items.Surname;
                                    editEmp.Email = items.Email;
                                    editEmp.ContactNo = items.ContactNo;
                                }
                                db.SaveChanges();
                                return "success";
                            }
                        default:
                            return "Profile details are invalid";
                    }


                }
                return "Session is no longer valid";
            }
            catch (Exception err)
            {
                return err.Message;
            }

        }

        [Route("ForgotPassword")]
        [HttpGet]
        public dynamic ForgotPassword(string Username)
        {
            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.Username == Username).FirstOrDefault();
            string SessionID;
            //string body = "Click the link below to reset your passoword:" + "\n" + "http://localhost:4200/reset?SessionID=";
            string body = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/EmailTemplates/ResetPassword.html"));
            
            if (findUser != null)
            {
                try
                {
                    switch (findUser.RoleID)
                    {
                        case 1:
                            Client findClient = db.Clients.Where(zz => zz.UserID == findUser.UserID).FirstOrDefault();
                            Guid g = Guid.NewGuid();
                            findUser.SessionID = g.ToString();
                            db.SaveChanges();
                            SessionID = findUser.SessionID;
                            body = body.Replace("#SessionID#", SessionID).Replace("#Name#", findClient.Name + " " + findClient.Surname);
                            string findEmail = findClient.Email;
                            Email(body, findEmail, "Reset Password");
                            return "success";
                        case 2:
                            Admin findAdmin = db.Admins.Where(zz => zz.UserID == findUser.UserID).FirstOrDefault();
                            Guid f = Guid.NewGuid();
                            findUser.SessionID = f.ToString();
                            db.SaveChanges();
                            SessionID = findUser.SessionID;
                            body = body.Replace("#SessionID#", SessionID).Replace("#Name#", findAdmin.Name + " " + findAdmin.Surname);
                            string findAEmail = findAdmin.Email;
                            Email(body, findAEmail, "Reset Password");
                            return "success";
                        case 3:
                            Employee findEmployee = db.Employees.Where(zz => zz.UserID == findUser.UserID).FirstOrDefault();
                            Guid h = Guid.NewGuid();
                            findUser.SessionID = h.ToString();
                            db.SaveChanges();
                            SessionID = findUser.SessionID;
                            body = body.Replace("#SessionID#", SessionID).Replace("#Name#", findEmployee.Name + " " + findEmployee.Surname);
                            string findEmpEmail = findEmployee.Email;
                            Email(body, findEmpEmail, "Reset Password");
                            return "success";
                        default:
                            return "User not found";
                    }
                }
                catch(Exception err)
                {
                    return err.Message;
                }
            }
            else
            {
                return "User not found";
            }

          
        }

        [Route("Logout")]
        [HttpGet]
        public dynamic Logout(string SessionID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            if (SessionID != null)
            {
                User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
                findUser.SessionID = null;
                db.SaveChanges();
            }
            return "success";
        }

        //private dynamic ForgotEmail(string SessionID, string Email, string Subject )
        //{
        //    try
        //    {
        //        MailMessage message = new MailMessage();
        //        SmtpClient smtp = new SmtpClient();
        //        message.From = new MailAddress("hairexhilartion@gmail.com");
        //        message.To.Add(new MailAddress(Email));
        //        message.Subject = "Exhiliration Hair & Beauty " + Subject;
        //        message.IsBodyHtml = false;
        //        message.Body = "Click the link below to reset your passoword:" + "\n" + "http://localhost:4200/reset?SessionID=" + SessionID;
        //        smtp.Port = 587;
        //        smtp.Host = "smtp.gmail.com";
        //        smtp.EnableSsl = true;
        //        smtp.EnableSsl = true;
        //        smtp.UseDefaultCredentials = false;
        //        smtp.Credentials = new NetworkCredential("hairexhilartion@gmail.com", "@Exhilaration1");
        //        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        smtp.Send(message);

        //        return "success";
        //    }
        //    catch (Exception err)
        //    {
        //        return err.Message;
        //    }
        //}

        [Route("ResetPassword")]
        [HttpPost]
        public dynamic ResetPassword([FromBody]User Modell)
        {
            if (Modell.SessionID != null)
            {
                User findUser = db.Users.Where(zz => zz.SessionID == Modell.SessionID).FirstOrDefault();
                if (findUser == null)
                {
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Error = "session";
                    toReturn.Message = "Session is not valid";
                    return toReturn;
                }

                try
                {
                    findUser.Password = GenerateHash(ApplySomeSalt(Modell.Password));
                    findUser.SessionID = null;
                    db.SaveChanges();

                    return "success";
                }
                catch (Exception err)
                {
                    return err.Message;
                }
            }
            else
            {
                return "invalid details";
            }
        }

        //****************************************check session***********************************************
        [Route("ValidSession")]
        [HttpGet]
        public bool ValidSession(string SessionID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if(findUser != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [Route("getUserID")]
        [HttpPost]
        public int getUserID(string seshin)
        {
            db.Configuration.ProxyCreationEnabled = false;

            User findUser = db.Users.Where(zz => zz.SessionID == seshin).FirstOrDefault();
            if (findUser != null)
            {
                if (findUser.RoleID == 1)
                {
                    return db.Clients.Where(zz => zz.UserID == findUser.UserID).Select(zz => zz.ClientID).FirstOrDefault();
                }

                if (findUser.RoleID == 2)
                {
                    return db.Admins.Where(zz => zz.UserID == findUser.UserID).Select(zz => zz.AdminID).FirstOrDefault();
                }

                if (findUser.RoleID == 3)
                {
                    return db.Employees.Where(zz => zz.UserID == findUser.UserID).Select(zz => zz.EmployeeID).FirstOrDefault();
                }
            }

            return 0;

        }

        //*****************************************login*******************************************************
        [Route("Login")]
        [HttpPost]
        public dynamic Login([FromBody]User user)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var hash = GenerateHash(ApplySomeSalt(user.Password));
            User findUser = db.Users.Where(zz => zz.Username == user.Username && zz.Password == hash).FirstOrDefault();
            dynamic toReturn = new ExpandoObject();

            if (findUser != null)
            {
                Guid g = Guid.NewGuid();
                findUser.SessionID = g.ToString();
                db.Entry(findUser).State = EntityState.Modified;

                db.SaveChanges();
                toReturn.Message = "success";
                string sesh = g.ToString();
                toReturn.SessionID = sesh;
                toReturn.RoleID = findUser.RoleID;
                return toReturn;
            }
            toReturn.Error = "Incorrect username and password";
            return toReturn;

        }
        //*****************************************check role***************************************************
        [Route("checkRole")]
        [HttpPost]
        public dynamic checkRole(string seshin)
        {

            string sessions = seshin;
            db.Configuration.ProxyCreationEnabled = false;
            var user = db.Users.Where(rr => rr.SessionID == sessions).FirstOrDefault();

            if (user != null)
            {
                if (user.RoleID == 1) // client
                {
                    return "client";
                }
                else if (user.RoleID == 2) // admin
                {
                    return "admin";
                }
                else if (user.RoleID == 3) //employee
                {
                    return "employee";
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //dynamic toReturn = new ExpandoObject();
                //toReturn.Error = "Guid is not valid";
                return "error";
            }
        }
        //*******************************************user setup*******************************************
        [Route("userSetup")]
        [HttpPut]
        public dynamic userSetup([FromBody] User forsetup)
        {
            db.Configuration.ProxyCreationEnabled = false;
            dynamic toReturn = new ExpandoObject();
            var i = forsetup;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                User usr = db.Users.Where(zz => zz.SessionID == forsetup.SessionID).FirstOrDefault();             

                 var hash = GenerateHash(ApplySomeSalt(forsetup.Password));
                 usr.Username = forsetup.Username;
                 usr.Password = hash;

                 usr.SessionID = Guid.NewGuid().ToString(); ;
                 db.SaveChanges();
                 toReturn.Message = "success";
                 toReturn.SessionID = usr.SessionID;
                 toReturn.RoleID = usr.RoleID;                  
                
                return toReturn;
            }
            catch (Exception)
            {
                return toReturn.Error = "Session is no longer valid";
            }
            
        }
        //************************************employee availability******************************
        [Route("getTime")]
        [HttpGet]
        public dynamic getTime()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Timeslot> findTime = db.Timeslots.ToList();
            return findTime;
        }
        [Route("getDate")]
        [HttpGet]
        public List<Date> getDate()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Date> findDate = db.Dates.ToList();
            return findDate;
        }
        //**********************************read employee type*************************************
        [Route("api/Employee/getEmployeeType")]
        [HttpGet]
        public List<dynamic> getEmployeeType()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getEmployeeTypeID(db.ServiceTypes.ToList());
        }
        private List<dynamic> getEmployeeTypeID(List<ServiceType> forEST)
        {
            List<dynamic> dymaminEmplType = new List<dynamic>();
            foreach (ServiceType ESTname in forEST)
            {
                dynamic dynamicEST = new ExpandoObject();
                dynamicEST.TypeID = ESTname.TypeID;
                dynamicEST.Name = ESTname.Name;
                dymaminEmplType.Add(dynamicEST);
            }
            return dymaminEmplType;
        }
        //*******************************registration stuff******************************************
        public static void Email(string body, string Email, string Subject)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("hairexhilartion@gmail.com");
                message.To.Add(new MailAddress(Email));
                message.Subject = "Exhiliration Hair & Beauty | " + Subject;
                message.IsBodyHtml = true;
                message.Body = body;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("hairexhilartion@gmail.com", "@Exhilaration1");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch
            {
                throw;
            }
        }
        //***********************************register admin********************************************
        [Route("RegisterAdmin")]
        [HttpPost]
        public dynamic RegisterAdmin(User Modell)
        {
            try 
            {
                User UserObject = new User();
                UserObject.Username = generateUser();
                UserObject.Password = generatePassword(50);             
                UserObject.RoleID = Modell.RoleID;              
                Guid g = Guid.NewGuid();
                UserObject.SessionID = g.ToString();
                db.Users.Add(UserObject);
                db.SaveChanges();

                int UserID = UserObject.UserID;
                string SessionID = UserObject.SessionID;


                foreach (Admin AdminData in Modell.Admins)
                {
                    AdminData.UserID = UserID;
                    db.Admins.Add(AdminData);
                    db.SaveChanges();

                    string name = AdminData.Name + " " + AdminData.Surname;
                    string body = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/EmailTemplates/Register.html"));
                    body = body.Replace("#Name#", name).Replace("#SessionID#", SessionID);
                    
                    Email(body, AdminData.Email, "Registration");
                }
                
                return "success";
            }
            catch (Exception err)
            {
                return err.Message ;
            }
        }

        //***********************************register employee and admin********************************************
        [Route("RegisterEmployee")]
        [HttpPost]
        public dynamic RegisterEmployee(EmployeeData Modell)
        {
            try
            {
                User UserObject = new User();
                UserObject.Username = generateUser();
                UserObject.Password = generatePassword(300);
                //UserObject.Password = GenerateHash(Password);

                UserObject.RoleID = Modell.UserData.RoleID;

                Guid g = Guid.NewGuid();
                UserObject.SessionID = g.ToString();
                db.Users.Add(UserObject);
                db.SaveChanges();
                db.Entry(UserObject).GetDatabaseValues();

                int UserID = UserObject.UserID;
                string SessionID = UserObject.SessionID;

                

                foreach (Employee EmployeesData in Modell.UserData.Employees)
                {
                    EmployeesData.UserID = UserID;
                    db.Employees.Add(EmployeesData);
                    db.SaveChanges();

                    populateTimes(EmployeesData.EmployeeID);

                    foreach (int types in Modell.ServiceTypes)
                    {
                        EmployeeServiceType newEmpType = new EmployeeServiceType();
                        newEmpType.EmployeeID = EmployeesData.EmployeeID;
                        newEmpType.TypeID = types;
                        db.EmployeeServiceTypes.Add(newEmpType);
                        db.SaveChanges();
                    }
                    string name = EmployeesData.Name + " " + EmployeesData.Surname;
                    string body = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/EmailTemplates/Register.html"));
                    body = body.Replace("#Name#", name).Replace("#SessionID#", SessionID);

                    Email(body, EmployeesData.Email, "Reigstration");
                }
              
                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        public class EmployeeData
        {
            public User UserData { get; set; }
            public ICollection<int> ServiceTypes { get; set; }
        }
        //***************************generate user*************************************
        private static string generateUser()
        {
            string uname = "";

            char[] lower = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'm', 'n', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            char[] upper = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

            int low = lower.Length;
            int up = upper.Length;

            var random = new Random();

            uname += lower[random.Next(0, low)].ToString();
            uname += lower[random.Next(0, up)].ToString();

            uname += upper[random.Next(0, low)].ToString();
            uname += upper[random.Next(0, up)].ToString();

            return uname;
        }
        //*********************************generate password****************************
        [Route("generatePassword")]
        [HttpPost]
        public string generatePassword(int Length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            Random rndm = new Random();
            while (0 < Length--)
            {
                res.Append(valid[rndm.Next(valid.Length)]);
            }
            return res.ToString();
        }
        //*************************hashing and other stuff that we mught not even use, lmao*********************
        public static string ApplySomeSalt(string input)
        {
            return input += "plokijuhygwaesrdtfyguhmnzxnvhfjdkslaowksjdienfhvbg";
        }

        public static string GenerateHash(string inputStr)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(inputStr);
            byte[] hash = sha256.ComputeHash(bytes);

            return getStringFromHash(hash);
        }
        public static string getStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int k = 0; k < hash.Length; k++)
            {
                result.Append(hash[k].ToString("X2"));
            }
            return result.ToString();
        }

        //**************************************make sale payment******************************
        [Route("salePayment")]
        [HttpPut]
        public object salePayment([FromBody] Sale sayle)
        {
            try
            {
                Sale findSale = db.Sales.Where(zz => zz.SaleID == sayle.SaleID).FirstOrDefault();
                findSale.StatusID = 1;
                findSale.Payment = sayle.Payment;
                findSale.PaymentTypeID = sayle.PaymentTypeID;
             // findSale.Description = sayle.Description;
                db.SaveChanges();
                return findSale;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }
        //***********************************make booking paynent*****************************
        

        [Route("bookingPayment")]
        [HttpPost]
        public dynamic bookingPayment(dynamic bkings)
        {
            try
            {
                string SessionID = bkings.SessionID;
                User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
                if (findUser != null)
                {
                    int BookingID = (int)bkings.BookingID;
                    int? PaymentTypeID = (int?)bkings.PaymentTypeID;
                    decimal Price = (decimal)bkings.Price;
                    int? PackageID = (int?)bkings.PackageID;
                    int? SaleID = (int?)bkings.SaleID;
                    Booking findBookings = db.Bookings.Where(zz => zz.BookingID == BookingID).FirstOrDefault();

                    if (SaleID != null && PackageID != null && PaymentTypeID == null)
                    {
                        
                        ClientPackage clientPackage = db.ClientPackages.Include(zz => zz.PackageInstances).Where(zz => zz.SaleID == SaleID && zz.PackageID == PackageID).FirstOrDefault();
                        List<PackageInstance> getInstance = clientPackage.PackageInstances.ToList();
                        int LineID = db.BookingLines.Where(zz => zz.BookingID == findBookings.BookingID).Select(zz => zz.LineID).FirstOrDefault();

                        for (int j=0; j<getInstance.Count; j++)
                        {
                            if(getInstance[j].StatusID == 1)
                            {
                                PackageInstance thisInstance = getInstance[j];
                                thisInstance.StatusID = 2;
                                thisInstance.LineID = LineID;
                                thisInstance.Date = DateTime.Now;
                                db.SaveChanges();
                                break;
                            }
                        }

                        findBookings.StatusID = 6;
                        findBookings.SaleID = SaleID;

                        

                        db.SaveChanges();
                        return "success";
                    }
                    else
                    {
                        

                        Sale makeSale = new Sale();
                        makeSale.PaymentTypeID = PaymentTypeID;
                        makeSale.Payment = Price;
                        makeSale.SaleTypeID = 2;
                        makeSale.ClientID = findBookings.ClientID;
                        makeSale.StatusID = 2;
                        makeSale.Date = DateTime.Now;
                        db.Sales.Add(makeSale);
                        db.SaveChanges();

                        findBookings.StatusID = 6;
                        findBookings.SaleID = makeSale.SaleID;


                        db.SaveChanges();
                        return "success";
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
            catch (Exception err)
            {
                return err.Message;
            }
        }
        //***************************read payment type********************************
        [Route("getPaymentType")]
        [HttpGet]
        public List<dynamic> getPaymentType()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getPaymentTypeID(db.PaymentTypes.ToList());
        }
        private List<dynamic> getPaymentTypeID(List<PaymentType> forPT)
        {
            List<dynamic> dynamicPTs = new List<dynamic>();
            foreach (PaymentType pt in forPT)
            {
                dynamic dynamicPT = new ExpandoObject();
                dynamicPT.PaymentTypeID = pt.PaymentTypeID;
                dynamicPT.Sales = pt.Sales;
                dynamicPT.Type = pt.Type;

                dynamicPTs.Add(dynamicPT);
            }
            return dynamicPTs;
        }
        //*******************service package shandis for displaying purposes*************************
        [Route("getservicePackage")]
        [HttpGet]
        public List<dynamic> getservicePackage()
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getServicePackageID(db.ServicePackages.ToList());
        }
        private List<dynamic> getServicePackageID(List<ServicePackage> forSP)
        {
            List<dynamic> dynamicSPs = new List<dynamic>();
            foreach (ServicePackage spname in forSP)
            {
                dynamic dynamicSP = new ExpandoObject();
                dynamicSP.ServiceID = spname.ServiceID;
                dynamicSP.Service = db.Services.Where(zz => zz.ServiceID == spname.ServiceID).Select(zz => zz.Name).FirstOrDefault();
                dynamicSP.PackageID = spname.PackageID;
                //dynamicSP.Name = spname.Name;
                dynamicSP.Description = spname.Description;
                dynamicSP.Price = spname.Price;
                dynamicSP.Quantity = spname.Quantity;

                dynamicSPs.Add(dynamicSP);
            }
            return dynamicSPs;
        }

        //*************************ACTIVATE SERVICE PACKAGE*****************************
        [Route("activeSP")]
        [HttpPost]
        public dynamic activeSP([FromBody]Sale Modell, string SessionID) //remember
        {
            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "Session is not valid";
                return toReturn;
            }

            try
            {
                DateTime today = DateTime.Now;
                var findPackage = Modell.ClientPackages.FirstOrDefault();

                ClientPackage checkDuplicate = db.ClientPackages.Where(zz => zz.PackageID == findPackage.PackageID && zz.ClientID == Modell.ClientID
                && zz.Active == true).FirstOrDefault();

                if(checkDuplicate != null)
                {
                    if(checkDuplicate.ExpiryDate < today)
                    {
                        checkDuplicate.Active = false;
                        db.SaveChanges();
                    }
                    else
                    {
                        return "duplicate";
                    }
                    
                }

                // string sp = "Activate Service Package";
                DateTime Now = DateTime.Now;
                Sale sales = new Sale();

                sales.ClientID = Modell.ClientID;
                sales.Payment = Modell.Payment;
                sales.SaleTypeID = 3;
                sales.PaymentTypeID = Modell.PaymentTypeID;
                sales.StatusID = 2;
                sales.Date = Now;

                db.Sales.Add(sales);
                db.SaveChanges();

                int SaleID = sales.SaleID;

                //ading to client sdjfnjvn thingy
                foreach (ClientPackage items in Modell.ClientPackages)
                {
                    ClientPackage CP = new ClientPackage();
                    CP.SaleID = SaleID;
                    CP.PackageID = items.PackageID;
                    CP.Date = Now;
                    CP.ClientID = Modell.ClientID;
                    CP.Active = true;
                    int Duration = db.ServicePackages.Where(zz => zz.PackageID == items.PackageID).Select(zz => zz.Duration).FirstOrDefault();
                    CP.ExpiryDate = Now.AddMonths(Duration);
                    db.ClientPackages.Add(CP);
                    db.SaveChanges();

                    //**********instance for the service package*****************
                    int loop = db.ServicePackages.Where(zz => zz.PackageID == items.PackageID).Select(zz => zz.Quantity).FirstOrDefault(); ;
                    for (int j = 0; j < loop; j++)
                    {
                        PackageInstance addInstance = new PackageInstance();
                        addInstance.PackageID = items.PackageID;
                        addInstance.SaleID = SaleID;
                        addInstance.StatusID = 1;
                        db.PackageInstances.Add(addInstance);
                        db.SaveChanges();
                    }
                }

                return "success";
            }
            catch(Exception err)
            {
                return err.Message;
            }


        }
        //****************************user details******************************
        [Route("getUser")]
        [HttpGet]
        public List<dynamic> getUser([FromBody] User forUser)
        {
            db.Configuration.ProxyCreationEnabled = false;
            return getUserID(db.Users.ToList());
            // return getAdminID(db.Admins.ToList());
        }
        private List<dynamic> getUserID(List<User> forUser)
        {
            List<dynamic> dynamicUsers = new List<dynamic>();
            foreach (User username in forUser)
            {
                dynamic dynamicUser = new ExpandoObject();
                dynamicUser.UserID = username.UserID;
                dynamicUser.Username = username.Username;
                dynamicUser.Password = username.Password;
                dynamicUsers.Add(dynamicUser);
            }
            return dynamicUsers;
        }

        //************************checks user role*******************************
        [Route("CheckRole")]
        [HttpPost]
        public dynamic CheckRole(dynamic seshID)
        {
            string sessionID = seshID.token;
            db.Configuration.ProxyCreationEnabled = false;
            var user = db.Users.Where(zz => zz.SessionID == sessionID).FirstOrDefault();

            if (user != null)
            {
                if (user.RoleID == 1)
                {
                    return "client";
                }
                else if (user.RoleID == 2)
                {
                    return "admin";
                }
                else
                {
                    return "employee";
                }
            }
            else
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "Guid is no longer valid";
                return toReturn;
            }
        }

       
        private dynamic populateTimes(int EmployeeID)
        {
            db.Configuration.ProxyCreationEnabled = false;

            List<Schedule> ScheduleList = db.Schedules.ToList();
            List<EmployeeSchedule> newList = new List<EmployeeSchedule>();
            for (int j = 0; j < ScheduleList.Count; j++)
            {
                int thisDateID = ScheduleList[j].DateID;
                int thisTimeID = ScheduleList[j].TimeID;
                EmployeeSchedule newSchedge = db.EmployeeSchedules.Where(zz => zz.DateID == thisDateID && zz.TimeID == thisTimeID && zz.EmployeeID == EmployeeID).FirstOrDefault();
                if (newSchedge == null)
                {
                    EmployeeSchedule items = new EmployeeSchedule();
                    items.EmployeeID = EmployeeID;
                    items.DateID = thisDateID;
                    items.TimeID = thisTimeID;
                    items.StatusID = 2;
                    newList.Add(items);
                }

            }
            db.EmployeeSchedules.AddRange(newList);

            db.SaveChanges();
            return "success";
        }

        public class AuditTrailParams
        {
            public int LoggedInID { get; set; }
            public string OldData { get; set; }
            public string NewData { get; set; }
            public string TablesAffected { get; set; }
            public string TransactionType { get; set; }
            public int AuthorizedID { get; set; }
            public DateTime Date { get; set; }


        }

        public static void AdminAuditTrail(AuditTrailParams data)
        {
            ExperTechEntities db = new ExperTechEntities();
            AdminAuditTrail createTrail = new AdminAuditTrail();
            createTrail.AdminID = data.LoggedInID;
            createTrail.OldData = data.OldData;
            createTrail.NewData = data.NewData;
            createTrail.TablesAffected = data.TablesAffected;
            createTrail.TransactionType = data.TransactionType;
            createTrail.Date = DateTime.Now;
            createTrail.AuthorizedBy = data.AuthorizedID;
            db.AdminAuditTrails.Add(createTrail);
            db.SaveChanges();
        }

        public static void ClientAuditTrail(AuditTrailParams data)
        {
            ExperTechEntities db = new ExperTechEntities();
            ClientAuditTrail createTrail = new ClientAuditTrail();
            createTrail.ClientID = data.LoggedInID;
            createTrail.OldData = data.OldData;
            createTrail.NewData = data.NewData;
            createTrail.TablesAffected = data.TablesAffected;
            createTrail.TransactionType = data.TransactionType;
            createTrail.Date = DateTime.Now;
            db.ClientAuditTrails.Add(createTrail);
            db.SaveChanges();
        }

        public static void EmployeeAuditTrail(AuditTrailParams data)
        {
            ExperTechEntities db = new ExperTechEntities();
            EmployeeAuditTrail createTrail = new EmployeeAuditTrail();
            createTrail.EmployeeID = data.LoggedInID;
            createTrail.OldData = data.OldData;
            createTrail.NewData = data.NewData;
            createTrail.TablesAffected = data.TablesAffected;
            createTrail.TransactionType = data.TransactionType;
            createTrail.Date = DateTime.Now;
            db.EmployeeAuditTrails.Add(createTrail);
            db.SaveChanges();
        }


    }


}
