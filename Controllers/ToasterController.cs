using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ToastCore.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace ToastCore.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ToasterController : Controller
    {
        private readonly ToastCoreContext _context;

        public ToasterController(ToastCoreContext context)
        {
            _context = context;
        }



        /// <summary>
        /// Example Get method
        /// </summary>
        /// <returns>Example data</returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "toasterrr", "olrait" };
        }
        
        /// <summary>
        /// Gets Toaster info
        /// </summary>
        /// <returns>Toaster data</returns>
        [HttpGet("/api/toaster/GetToasters")]
        public IQueryable<Toaster> GetToasters()
        {
            return _context.Toasters;
        }

        /// <summary>
        /// Current status of the toaster
        /// </summary>
        /// <returns>Current status of the toaster</returns>
        [HttpGet("/api/toaster/GetCurrentStatus")]
        public Status GetCurrentStatus()
        {
            return _context.Toasters.FirstOrDefault().Status;
        }

        [HttpGet("/api/toaster/HowManyToastsMade")]
        public int HowManyToastsMade()
        {
            return _context.Toasters.FirstOrDefault().ToastsMade;
        }

        [HttpPatch("/api/toaster/SetTime/{time}")]
        public int SetTime(int time)
        {
            Toaster toaster = _context.Toasters.FirstOrDefault();
            toaster.Time = time;
            toaster.Profile = Profile.NoProfile;

            _context.Entry(toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return time;
        }

        [HttpPatch("/api/toaster/SetProfile/{profile}")]
        public Profile SetProfile(Profile profile)
        {
            Toaster toaster = _context.Toasters.FirstOrDefault();
            toaster.Profile = profile;

            switch (profile)
            {
                case Profile.NoProfile:
                    toaster.Time = 0;
                    break;
                case Profile.Low:
                    toaster.Time = 90;
                    break;
                case Profile.Normal:
                    toaster.Time = 180;
                    break;
                case Profile.High:
                    toaster.Time = 360;
                    break;
                case Profile.Burnt:
                    toaster.Time = 600;
                    break;

                default:
                    throw new Exception("Profile error.");
            }

            _context.Entry(toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return toaster.Profile;
        }

        [HttpPatch("/api/toaster/SetToasts/{numToasts}")]
        public int SetToasts(int numToasts)
        {
            if (numToasts > 2) throw new Exception("The maximum number of toasts is 2.");

            Toaster toaster = _context.Toasters.FirstOrDefault();
            PantryController pantry = new PantryController(_context);

            if (pantry.HowManyBreads() >= 0)
            {
                toaster.NumToasts = pantry.GetBreads(numToasts);
            }

            _context.Entry(toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return toaster.NumToasts;
        }

        [HttpPut("api/toasters/toast/{status}")]
        public Status Toast(Status status)
        {
            Toaster toaster = _context.Toasters.FirstOrDefault();

            if (toaster.Status == status) return status;

            toaster.Status = status;

            _context.Entry(toaster).State = EntityState.Modified;

            switch (toaster.Status)
            {
                case Status.On:
                    if (toaster.NumToasts > 0)
                    {
                        toaster.ToastsMade += toaster.NumToasts;
                        toaster.TimeStart = DateTime.Now.ToString();
                        toaster.TimeEnd = DateTime.Now.AddSeconds(toaster.Time).ToString();
                    }
                    else
                        throw new Exception("No bread in the Toaster.");
                    break;

                default:
                    toaster.NumToasts = 0;
                    toaster.Profile = Profile.NoProfile;
                    toaster.TimeStart = new DateTime().ToString();
                    toaster.TimeEnd = new DateTime().ToString();
                    break;
            }

            _context.Entry(toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return toaster.Status;
        }

        /// <summary>
        /// Toasts with a specific number of toasts and time 
        /// </summary>
        /// <param name="numToasts"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        [HttpPut("api/toaster/toast/numToasts/{numToasts}/time/{time}")]
        public Status Toast(int numToasts, int time)
        {
            try
            {
                SetToasts(numToasts);
                SetTime(time);

                return Toast(Status.On);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}