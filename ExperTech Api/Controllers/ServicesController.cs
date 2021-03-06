﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Web.Http.Description;
using ExperTech_Api.Models;
using System.Dynamic;
using Microsoft.Ajax.Utilities;
using System.Runtime.Serialization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json.Linq;

namespace ExperTech_Api.Controllers
{
    public class ServicesController : ApiController
    {
        private ExperTechEntities db = new ExperTechEntities();


        //************************************Service Type************************************************
        [Route("api/Services/AddServiceType")]
        [HttpPost]
        public dynamic AddServiceType([FromBody] ServiceType Modell)
        {
            db.Configuration.ProxyCreationEnabled = false;
            try
            {
                ServiceType Verify = db.ServiceTypes.Where(zz => zz.Name == Modell.Name).FirstOrDefault();
                if (Verify == null)
                {
                    db.ServiceTypes.Add(Modell);
                    db.SaveChanges();
                    return "success";
                }
                else
                {
                    return "duplicate";
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        [Route("api/Services/GetServiceType")]
        [HttpGet]
        public List<dynamic> GetServiceType()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<ServiceType> myList = db.ServiceTypes.ToList();
            return ListServiceTypes(myList);
        }

        private List<dynamic> ListServiceTypes(List<ServiceType> Modell)
        {
            List<dynamic> makeList = new List<dynamic>();
            foreach (ServiceType Item in Modell)
            {
                dynamic newObject = new ExpandoObject();
                newObject.TypeID = Item.TypeID;
                newObject.Name = Item.Name;
                makeList.Add(newObject);
            }
            return makeList;
        }

        [Route("api/Services/UpdateServiceType")]
        [HttpPost]
        public dynamic UpdateServiceType(ServiceType Modell)
        {
            db.Configuration.ProxyCreationEnabled = false;
            ServiceType Update = db.ServiceTypes.Where(zz => zz.TypeID == Modell.TypeID).FirstOrDefault();
            if (Update != null)
            {

                Update.Name = Modell.Name;
                db.SaveChanges();
                return "success";
            }
            else
            {
                return "failed";
            }
        }

        [Route("api/Services/DeleteServiceType")]
        [HttpDelete]
        public dynamic DeleteServiceType(int TypeID, string SessionID)
        {
            var admin = db.Users.Where(zz => zz.SessionID == SessionID).ToList();
            if (admin == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is no longer available";
                return GetServiceType();
            }
            db.Configuration.ProxyCreationEnabled = false;
            List<EmployeeServiceType> findEmpType = db.EmployeeServiceTypes.Where(zz => zz.TypeID == TypeID).ToList();
            List<Service> findService = db.Services.Where(zz => zz.ServiceID == TypeID).ToList();

            try
            {
                if (findService.Count == 0)
                {

                    ServiceType findServiceType = db.ServiceTypes.Find(TypeID);

                    db.EmployeeServiceTypes.RemoveRange(findEmpType);

                    db.ServiceTypes.Remove(findServiceType);

                    db.SaveChanges();
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Message = "success";
                    return toReturn;

                }
                else
                {
                    bool isServicesDeleted = true;
                    int count = 0;
                    while(isServicesDeleted = true && count<findService.Count)
                    {
                        count++;
                        if(findService[count-1].Deleted == false)
                        {
                            isServicesDeleted = false;
                        }
                    }
                    if (isServicesDeleted == false)
                    {
                        dynamic toReturn = new ExpandoObject();
                        int countServ = findService.Count;
                        toReturn.Error = "dependencies";
                        toReturn.Message = "There are services that depend this on Service Type \nDelete those services first.";
                        return toReturn;
                    }
                    else
                    {
                        foreach(Service items in findService)
                        {
                            Service updateService = items;
                            updateService.TypeID = null;
                            db.SaveChanges();
                        }
                        dynamic toReturn = new ExpandoObject();
                        toReturn.Message = "success";
                        return toReturn;
                    }
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
          
        }

        [Route("api/Services/ViewServices")]
        [HttpGet]
        public dynamic ViewServices()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<ServiceType> getServices = db.ServiceTypes.Include(zz => zz.Services).ToList();
            return formatServices(getServices);
        }

        private dynamic formatServices(List<ServiceType> Types)
        {
            List<dynamic> toReturn = new List<dynamic>();
           foreach(ServiceType items in Types)
            {
                dynamic TypeObject = new ExpandoObject();
                TypeObject.Name = items.Name;

                List<dynamic> Services = new List<dynamic>();
                foreach(Service service in items.Services)
                {
                    dynamic ServObject = new ExpandoObject();
                    List<Service> findService = db.Services.Include(zz => zz.ServicePrices).Include(zz => zz.ServiceType)
                                    .Include(zz => zz.ServicePhotoes).Include(zz => zz.ServiceTypeOptions).Where(zz => zz.ServiceID == service.ServiceID).ToList();
                    ServObject = getServices(findService)[0];
                    Services.Add(ServObject);
                }
                if(Services.Count >0)
                TypeObject.Services = Services;

                toReturn.Add(TypeObject);
            }

            return toReturn;
        }
        //************************************Service************************************************
        //review the duplicate return
        [Route("api/Services/AddService")]
        [HttpPost]
        public dynamic AddService()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var httpRequest = HttpContext.Current.Request;
            string imageName = null;
            string SessionID = httpRequest["SessionID"];
            int findUser = db.Users.Where(zz => zz.SessionID == SessionID).Select(zz => zz.UserID).FirstOrDefault();

            if(findUser != 0 )
            {
                var postedFile = httpRequest.Files["Image"];
                if (postedFile != null)
                {
                    try
                    {
                        imageName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(postedFile.FileName.Length).ToArray()).Replace(" ", "-");
                        imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                        var FilePath = HttpContext.Current.Server.MapPath("~/Images/" + imageName);
                        postedFile.SaveAs(FilePath);
                    }
                    catch (Exception err)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Image was not saved (" + err.Message + ")");
                    }
                }

                dynamic Modell = JObject.Parse(httpRequest["Service"]);
                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    string serviceName = Modell.Name;
                    Service findService = db.Services.Where(zz => zz.Name == serviceName).FirstOrDefault();
                    if(findService != null)
                    {
                        //if duplicate, return duplicate
                        dynamic toReturns = new ExpandoObject();
                        toReturns.Message = "duplicate";
                        toReturns.Data = findService;
                        return "duplicate";
                    }
                    //first Save the service information
                    Service myObject = new Service();
                    myObject.Name = Modell.Name;
                    myObject.Description = Modell.Description;
                    myObject.Duration = (int)Modell.Duration;
                    myObject.TypeID = (int)Modell.TypeID;
                    db.Services.Add(myObject);
                    db.SaveChanges();

                    //retrieve the service ID from the info that were just saved

                    int ServiceID = myObject.ServiceID;

                    if (Modell.ServicePrices != null)
                    {
                        foreach (var Items in Modell.ServicePrices)
                        {
                            //Save the Service price object
                            ServicePrice PriceObject = new ServicePrice();
                            PriceObject.ServiceID = ServiceID;
                            PriceObject.Price = (decimal)Items.Price;
                            PriceObject.Date = DateTime.Now;
                            db.ServicePrices.Add(PriceObject);

                            db.SaveChanges();
                        }
                    }

                    if (Modell.ServiceTypeOptions != null)
                    {
                        foreach (var Items in Modell.ServiceTypeOptions)
                        {
                            //saves the OptionID and ServiceID into bride entity
                            ServiceTypeOption newObject = new ServiceTypeOption();
                            newObject.ServiceID = ServiceID;
                            int OptionID = (int)Items.OptionID;
                            newObject.OptionID = OptionID;
                            db.ServiceTypeOptions.Add(newObject);
                            db.SaveChanges();

                            foreach (var PriceItem in Items.ServicePrices)
                            {
                                ServicePrice PriceObject = new ServicePrice();
                                PriceObject.ServiceID = ServiceID;
                                PriceObject.OptionID = OptionID;
                                PriceObject.Price = PriceItem.Price;
                                PriceObject.Date = DateTime.Now;
                                db.ServicePrices.Add(PriceObject);

                                db.SaveChanges();
                            }
                        }

                  
                    }

                    if (Modell.ServicePhotoes != null)
                    {
                        foreach (var items in Modell.ServicePhotoes)
                        {
                            if (imageName != null)
                            {
                                ServicePhoto newPhoto = new ServicePhoto();
                                newPhoto.ServiceID = ServiceID;
                                newPhoto.Photo = imageName;
                                db.ServicePhotoes.Add(newPhoto);
                                db.SaveChanges();
                            }
                        }
                    }
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Message = "success";
                    toReturn.ServiceID = ServiceID;
                    return toReturn;
                }
                catch(Exception err)
                {
                    return err.Message;
                }
            }
            else
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is not valid";
                return "Session is not valid";
            }

           
        }

        //[Route("api/Services/AddService")]
        //[HttpPost]
        //public dynamic AddService([FromBody] Service Modell)
        //{
        //    db.Configuration.ProxyCreationEnabled = false;
        //    try
        //    {   //checks for dupicate service

        //        Service Verify = db.Services.Where(zz => zz.Name == Modell.Name).FirstOrDefault();
        //        if (Verify == null)
        //        {
        //            //if not duplicate, execute SaveService()
        //            //Service ServiceObject = FormatServices(Modell);
        //            return SaveService(Modell);
        //        }
        //        else
        //        {
        //            //if duplicate, return duplicate
        //            return "duplicate";
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        return err.Message;
        //    }
        //}

       
        //private dynamic SaveService(Service Modell)
        //{
        //    dynamic toReturn = new ExpandoObject();
        //    try
        //    {
        //        db.Configuration.ProxyCreationEnabled = false;
        //        //first Save the service information
        //        Service myObject = new Service();
        //        myObject.Name = Modell.Name;
        //        myObject.Description = Modell.Description;
        //        myObject.Duration = Modell.Duration;
        //        myObject.TypeID = Modell.TypeID;
        //        db.Services.Add(myObject);
        //        db.SaveChanges();

        //        //retrieve the service ID from the info that were just saved
        //        int ServiceID = db.Services.Where(zz => zz.Name == Modell.Name).Select(zz => zz.ServiceID).FirstOrDefault();

        //        if (Modell.ServicePrices != null)
        //        {
        //            foreach (ServicePrice Items in Modell.ServicePrices)
        //            {
        //                //Save the Service price object
        //                ServicePrice PriceObject = new ServicePrice();
        //                PriceObject.ServiceID = ServiceID;
        //                //PriceObject.OptionID = Items.OptionID;
        //                PriceObject.Price = Items.Price;
        //                PriceObject.Date = DateTime.Now;
        //                db.ServicePrices.Add(PriceObject);

        //                db.SaveChanges();
        //            }
        //        }

        //        if (Modell.ServiceTypeOptions != null)
        //        {
        //            foreach (ServiceTypeOption Items in Modell.ServiceTypeOptions)
        //            {
        //                //saves the OptionID and ServiceID into bride entity
        //                ServiceTypeOption newObject = new ServiceTypeOption();
        //                newObject.ServiceID = ServiceID;
        //                int OptionID = (int)Items.OptionID;
        //                newObject.OptionID = OptionID;
        //                db.ServiceTypeOptions.Add(newObject);
        //                db.SaveChanges();

        //                foreach (ServicePrice PriceItem in Items.ServicePrices)
        //                {
        //                    ServicePrice PriceObject = new ServicePrice();
        //                    PriceObject.ServiceID = ServiceID;
        //                    PriceObject.OptionID = OptionID;
        //                    PriceObject.Price = PriceItem.Price;
        //                    PriceObject.Date = DateTime.Now;
        //                    db.ServicePrices.Add(PriceObject);

        //                    db.SaveChanges();
        //                }

        //            }
        //        }
              
        //        toReturn.Message = "success";
        //        toReturn.ServiceID = ServiceID;
        //        return toReturn;
        //    }
        //    catch (Exception err)
        //    {
        //        return err.Message;
        //    }
        //}

        [Route("api/Services/DeleteService")]
        [HttpDelete]
        public dynamic DeleteService(int ServiceID, string SessionID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if(findUser == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is not valid";
                return toReturn;
            }

            try
            {
                Service find = db.Services.Where(zz => zz.ServiceID == ServiceID).FirstOrDefault();
                List<BookingLine> findLines = db.BookingLines.Where(zz => zz.ServiceID == ServiceID).ToList();
                List<ServicePrice> findPrices = db.ServicePrices.Where(zz => zz.ServiceID == ServiceID).ToList();
                List<ServiceTypeOption> findOptions = db.ServiceTypeOptions.Where(zz => zz.ServiceID == ServiceID).ToList();
                List<ServicePhoto> findPhotos = db.ServicePhotoes.Where(zz => zz.ServiceID == ServiceID).ToList();
                List<ServicePackage> findPackage = db.ServicePackages.Where(zz => zz.ServiceID == ServiceID).ToList();

                if (findLines.Count != 0 || findPackage.Count != 0)
                {
                    dynamic toReturn = new ExpandoObject();
                    toReturn.Error = "dependencies";
                    toReturn.Message = "Unable to delete service: There are bookings or packages linked to this service. \n";
                    return toReturn;
                }
                else
                {
                    if(findPrices.Count != 0)
                    db.ServicePrices.RemoveRange(findPrices);

                    if(findOptions.Count != 0)
                    db.ServiceTypeOptions.RemoveRange(findOptions);

                    if(findPhotos.Count != 0)
                    db.ServicePhotoes.RemoveRange(findPhotos);

                    db.Services.Remove(find);
                    db.SaveChanges();
                    return "success";
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }      


        [Route("api/Services/GetService")]
        [HttpGet]
        public List<dynamic> GetService()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Service> myList = db.Services.Include(zz => zz.ServicePrices).Include(zz => zz.ServiceType)
                                    .Include(zz => zz.ServicePhotoes).Include(zz => zz.ServiceTypeOptions).Where(zz => zz.Deleted == false).ToList();
            return getServices(myList);
        }

        private List<dynamic> getServices(List<Service> Modell)
        {
            List<dynamic> ServiceList = new List<dynamic>();
            foreach (Service Items in Modell)
            {
                dynamic newObject = new ExpandoObject();
                newObject.ServiceID = Items.ServiceID;
                newObject.Name = Items.Name;
                newObject.ServiceType = Items.ServiceType.Name;
                newObject.TypeID = Items.TypeID;
                newObject.Description = Items.Description;
                newObject.Duration = Items.Duration;
                if (Items.ServiceTypeOptions.Count > 0)
                    newObject.ServiceTypeOptions = getOptions(Items);
                else
                    newObject.ServicePrices = getSPrice(Items);

                List<dynamic> photoList = new List<dynamic>();
                foreach(ServicePhoto photo in Items.ServicePhotoes)
                {
                    dynamic photoObject = new ExpandoObject();
                    photoObject.PhotoID = photo.PhotoID;
                    photoObject.Photo = photo.Photo;
                    photoList.Add(photoObject);
                    string filePath = HttpContext.Current.Server.MapPath("~/Images/" + photo.Photo);
                    try
                    {
                        using(FileStream fileStream = new FileStream(filePath, FileMode.Open))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                fileStream.CopyTo(memoryStream);
                                Bitmap image = new Bitmap(1, 1);
                                image.Save(memoryStream, ImageFormat.Png);

                                byte[] byteImage = memoryStream.ToArray();
                                string base64String = Convert.ToBase64String(byteImage);
                                newObject.Image = "data:image/png;base64," + base64String;
                            }
                        }
                    }
                    catch
                    {
                        newObject.Image = "";
                    }

                }
                newObject.ServicePhotoes = photoList;
                ServiceList.Add(newObject);
            }
            return ServiceList;
        }


        private dynamic getOptions(Service Modell)
        {
            List<dynamic> myList = new List<dynamic>();
            foreach (ServiceTypeOption Items in Modell.ServiceTypeOptions)
            {
                dynamic newObject = new ExpandoObject();
                ServiceOption findOption = db.ServiceOptions.Where(zz => zz.OptionID == Items.OptionID).FirstOrDefault();
                newObject.Option = findOption.Name;
                newObject.OptionID = Items.OptionID;
                List<dynamic> ServicePrices = new List<dynamic>();
                foreach (ServicePrice PriceItem in Items.ServicePrices)
                {
                    dynamic PriceObject = new ExpandoObject();
                    PriceObject.Price = PriceItem.Price;

                    ServicePrices.Add(PriceObject);
                }
                newObject.ServicePrices = ServicePrices;
                myList.Add(newObject);
            }

            return myList;


        }

        private dynamic getSPrice(Service Modell)
        {
            List<dynamic> myList = new List<dynamic>();
            foreach (ServicePrice Items in Modell.ServicePrices)
            {
                dynamic newObject = new ExpandoObject();
                newObject.Price = Items.Price;
                if (Items.OptionID != null)
                {
                    newObject.OptionID = Items.OptionID;
                }
                myList.Add(newObject);
            }

            return myList;


        }


        [Route("api/Services/UpdateService")]
        [HttpPost]
        public dynamic UpdateService()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var httpRequest = HttpContext.Current.Request;
            string imageName = null;
            string SessionID = httpRequest["SessionID"];
            int findUser = db.Users.Where(zz => zz.SessionID == SessionID).Select(zz => zz.UserID).FirstOrDefault();

            if (findUser != 0)
            {
                var postedFile = httpRequest.Files["Image"];
                if (postedFile != null)
                {
                    try
                    {
                        imageName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(postedFile.FileName.Length).ToArray()).Replace(" ", "-");
                        imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                        var FilePath = HttpContext.Current.Server.MapPath("~/Images/" + imageName);
                        postedFile.SaveAs(FilePath);
                    }
                    catch (Exception err)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Image was not saved (" + err.Message + ")");
                    }
                }

                dynamic Modell = JObject.Parse(httpRequest["Service"]);
                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    //first Save the service information
                    int ServiceID = (int)Modell.ServiceID;
                    Service myObject = db.Services.Where(zz => zz.ServiceID == ServiceID).FirstOrDefault();
                    myObject.Name = Modell.Name;
                    myObject.Description = Modell.Description;
                    myObject.Duration = (int)Modell.Duration;
                    myObject.TypeID = (int)Modell.TypeID;
                    db.SaveChanges();

                    //remove all options from database first in case some were removed
                    List<ServiceTypeOption> findTypeOption = db.ServiceTypeOptions.Where(zz => zz.ServiceID == ServiceID).ToList();
                    List<dynamic> tempPrices = new List<dynamic>();
                    List<dynamic> tempLines = new List<dynamic>();
                    foreach (ServiceTypeOption items in findTypeOption)
                    {
                        List<ServicePrice> findPrices = db.ServicePrices.Where(zz => zz.ServiceID == ServiceID && zz.OptionID == items.OptionID).ToList();
                        List<BookingLine> findLines = db.BookingLines.Where(zz => zz.ServiceID == ServiceID && zz.OptionID == items.OptionID).ToList();

                        foreach(ServicePrice prices in findPrices)
                        {
                            dynamic tempPrice = new ExpandoObject();
                            tempPrice.PriceID = prices.PriceID;
                            tempPrice.ServiceID = ServiceID;
                            tempPrice.OptionID = prices.OptionID;
                            tempPrices.Add(tempPrice);

                            ServicePrice findPrice = prices;
                            findPrice.OptionID = null;
                        }
                        db.SaveChanges();

                        foreach(BookingLine lines in findLines)
                        {
                            dynamic tempLine = new ExpandoObject();
                            tempLine.LineID = lines.LineID;
                            tempLine.ServiceID = ServiceID;
                            tempLine.OptionID = lines.OptionID;
                            tempLines.Add(tempLine);

                            BookingLine findLine = lines;
                            findLine.OptionID = null;

                        }
                        db.SaveChanges();

                    }
                    db.ServiceTypeOptions.RemoveRange(findTypeOption);
                    db.SaveChanges();

                    if (Modell.ServicePrices != null)
                    {
                        foreach (var Items in Modell.ServicePrices)
                        {
                            //Save the Service price object
                            ServicePrice PriceObject = new ServicePrice();
                            PriceObject.ServiceID = ServiceID;
                            PriceObject.Price = (decimal)Items.Price;
                            PriceObject.Date = DateTime.Now;
                            db.ServicePrices.Add(PriceObject);

                            db.SaveChanges();
                        }
                    }

                    if (Modell.ServiceTypeOptions != null)
                    {
                        
                        foreach (var Items in Modell.ServiceTypeOptions)
                        {
                            //saves the OptionID and ServiceID into bride entity
                            int OptionID = (int)Items.OptionID;
                            ServiceTypeOption newObject = new ServiceTypeOption();
                            newObject.ServiceID = ServiceID;
                            newObject.OptionID = OptionID;
                            db.ServiceTypeOptions.Add(newObject);
                            db.SaveChanges();

                            foreach (var PriceItem in Items.ServicePrices)
                            {
                                ServicePrice PriceObject = new ServicePrice();
                                PriceObject.ServiceID = ServiceID;
                                PriceObject.OptionID = OptionID;
                                PriceObject.Price = PriceItem.Price;
                                PriceObject.Date = DateTime.Now;
                                db.ServicePrices.Add(PriceObject);

                                
                            }
                            db.SaveChanges();

                            foreach (var restorePrices in tempPrices)
                            {
                                int rServiceID = (int)restorePrices.ServiceID;
                                int rOptionID = (int)restorePrices.OptionID;

                                if (rServiceID == ServiceID && rOptionID == OptionID)
                                {
                                    int rPriceID = (int)restorePrices.PriceID;
                                    ServicePrice findSP = db.ServicePrices.Where(zz => zz.PriceID == rPriceID).FirstOrDefault();
                                    findSP.OptionID = rOptionID;
                                    db.SaveChanges();
                                }
                            }

                            foreach (var restoreLines in tempLines)
                            {
                                int rServiceID = (int)restoreLines.ServiceID;
                                int rOptionID = (int)restoreLines.OptionID;

                                if (rServiceID == ServiceID && rOptionID == OptionID)
                                {
                                    int rLineID = (int)restoreLines.LineID;
                                    BookingLine findBL = db.BookingLines.Where(zz => zz.LineID == rLineID).FirstOrDefault();
                                    findBL.OptionID = rOptionID;
                                    db.SaveChanges();
                                }
                            }
                        }


                    }

                    if (Modell.ServicePhotoes != null)
                    {
                        foreach (var items in Modell.ServicePhotoes)
                        {
                            if (imageName != null)
                            {
                                ServicePhoto newPhoto = db.ServicePhotoes.Where(zz => zz.ServiceID == ServiceID).FirstOrDefault();
                                newPhoto.Photo = imageName;
                                db.SaveChanges();
                            }
                        }
                    }
                  
                    return "success";
                }
                catch (Exception err)
                {
                    return err.Message;
                }
            }
            else
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is not valid";
                return "Session is not valid";
            }
        }


        //[Route("api/Services/UpdateService")]
        //[HttpPost]
        //public dynamic UpdateService([FromBody] Service Modell)
        //{
        //    try
        //    {
        //        Service findService = db.Services.Where(zz => zz.ServiceID == Modell.ServiceID).FirstOrDefault();
        //        findService.Name = Modell.Name;
        //        findService.Description = Modell.Description;
        //        findService.Duration = Modell.Duration;
        //        findService.TypeID = Modell.TypeID;
        //        db.SaveChanges();

        //        //it hits here
        //        foreach (ServicePrice Items in Modell.ServicePrices)
        //        {
        //            ServicePrice findPrice = db.ServicePrices.Where(zz => zz.PriceID == Items.PriceID && zz.Price == Items.Price).FirstOrDefault();
        //            if (findPrice == null)
        //            {
        //                ServicePrice PriceObject = new ServicePrice();
        //                PriceObject.ServiceID = findService.ServiceID;
        //                PriceObject.OptionID = Items.OptionID;
        //                PriceObject.Price = Items.Price;
        //                PriceObject.Date = DateTime.Now;
        //                db.ServicePrices.Add(PriceObject);
        //                db.SaveChanges();
        //            }
        //        }
        //        return "success";
        //    }
        //    catch (Exception err)
        //    {
        //        return err.Message;
        //    }
        //}
        //****************************************Service Option**************************************

        [Route("api/Services/AddServiceOption")]
        [HttpPost]
        public dynamic AddServiceOption(ServiceOption Modell)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.ServiceOptions.Add(Modell);
                db.SaveChanges();
                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }

        }



        [Route("api/Services/GetServiceOption")]
        [HttpGet]
        public List<ServiceOption> GetServiceOption()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<ServiceOption> mylist = db.ServiceOptions.Where(zz=> zz.Deleted == false).ToList();
            return mylist;
        }

        [Route("api/Services/UpdateServiceOption")]
        [HttpPut]
        public dynamic UpdateServiceOption(ServiceOption Modell)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                ServiceOption findOption = db.ServiceOptions.Where(zz => zz.OptionID == Modell.OptionID).FirstOrDefault();
                findOption.Name = Modell.Name;
                findOption.Duration = Modell.Duration;
                db.SaveChanges();
                return "success";


            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        [Route("api/Services/DeleteServiceOption")]
        [HttpDelete]
        public dynamic DeleteServiceOption(int OptionID, string SessionID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            User findUser = db.Users.Where(zz => zz.SessionID == SessionID).FirstOrDefault();
            if (findUser == null)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "session";
                toReturn.Message = "Session is not valid";
                return toReturn;
            }

            db.Configuration.ProxyCreationEnabled = false;
            ServiceOption findOption = db.ServiceOptions.Find(OptionID);
            List<ServiceTypeOption> findTypeOptions = db.ServiceTypeOptions.Where(zz => zz.OptionID == OptionID).ToList();

            if(findTypeOptions.Count != 0)
            {
                dynamic toReturn = new ExpandoObject();
                toReturn.Error = "dependencies";
                toReturn.Message = "Unable to delete service option: This option is linked to some services. \n";
                return toReturn;
            }
            else
            {
                db.ServiceOptions.Remove(findOption);
                db.SaveChanges();
                return "success";
            }
           
        }

        //******************************Service Package************************************
        [Route("api/Services/CreateServicePackage")]
        [HttpPost]
        public dynamic CreateServicePackage(ServicePackage Modell)
        {
            try
            {
                ServicePackage findPackage = db.ServicePackages.Where(zz => zz.ServiceID == Modell.ServiceID && zz.Quantity == Modell.Quantity).FirstOrDefault();

                if (findPackage == null)
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.ServicePackages.Add(Modell);
                    db.SaveChanges();
                    return "success";
                }
                else
                {
                    return "duplicate";
                }
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        [Route("api/Services/RetrieveServicePackage")]
        [HttpGet]
        public dynamic RetrieveServicePackage()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<ServicePackage> mylist = db.ServicePackages.Include(zz => zz.Service).Where(zz => zz.Deleted == false).ToList();
            return getPackage(mylist);
        }

        private dynamic getPackage(List<ServicePackage> Modell)
        {
            List<dynamic> thisList = new List<dynamic>();
            foreach (ServicePackage items in Modell)
            {
                dynamic myObject = new ExpandoObject();
                myObject.PackageID = items.PackageID;
                myObject.Name = items.Service.Name;
                myObject.Description = items.Description;
                myObject.Price = items.Price;
                myObject.Quantity = items.Quantity;
                myObject.Duration = items.Duration;

                thisList.Add(myObject);
            }

            return thisList;
        }

        [Route("api/Services/RemoveServicePackage")]
        [HttpDelete]
        public dynamic RemoveServicePackage(int PackageID)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                ServicePackage findPackage = db.ServicePackages.Where(zz => zz.PackageID == PackageID).FirstOrDefault();
                findPackage.Deleted = true;
                db.SaveChanges();
                return "success";
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

      
        
        //****************************************************testing*******************************************************
        [Route("api/Services/AddServicePhoto")]
        [HttpPost]
        public HttpResponseMessage AddServicePhoto()
        {
            var httpRequest = HttpContext.Current.Request;
            string imageName = "";

            try
            {
                var postedFile = httpRequest.Files["Image"];
                imageName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(postedFile.FileName.Length).ToArray()).Replace(" ", "-");
                imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
                var FilePath = HttpContext.Current.Server.MapPath("~/Images/" + imageName);
                postedFile.SaveAs(FilePath);
            }
            catch (Exception err)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Image was not saved (" + err.Message + ")");
            }

            try
            {
                int ServiceID = Convert.ToInt32(httpRequest["ServiceID"]);
                Service verify = db.Services.Where(zz => zz.ServiceID == ServiceID).FirstOrDefault();
                if (verify != null)
                {
       
                    if (imageName != null)
                    {
                        ServicePhoto photo = new ServicePhoto();
                        photo.ServiceID = ServiceID;
                        photo.Photo = imageName;

                        db.ServicePhotoes.Add(photo);
                        db.SaveChanges();
                    }

                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Image details are invalid");
            }

            return Request.CreateResponse(HttpStatusCode.Created);

        }

    }
}
