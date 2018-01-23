using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ToastCore.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Net;
//using Microsoft.AspNetCore.Cors;

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
        /// <response code="200">Current status of the IToast</response>
        [HttpGet("/api/toaster/GetCurrentStatus")]
        //[EnableCors("MyPolicy")]
        public IActionResult GetCurrentStatus()
        {
            return StatusCode(200, _context.Toasters.FirstOrDefault().Status);
        }

        /// <summary>
        /// Number of toasts made by our current toaster
        /// </summary>
        /// <returns>Number of toasts made by our current toaster</returns>
        /// <response code="200">Number of toasts made by the IToast</response>
        [HttpGet("/api/toaster/HowManyToastsMade")]
        public IActionResult HowManyToastsMade()
        {
            return StatusCode(200, _context.Toasters.FirstOrDefault().ToastsMade);
        }

        /// <summary>
        /// Set time for the toaster
        /// </summary>
        /// <param name="time">Number of seconds</param>
        /// <returns>Number of seconds set</returns>
        /// <response code="200">Number of seconds set</response>
        [HttpPatch("/api/toaster/SetTime/{time}")]
        public IActionResult SetTime(int time)
        {
            Toaster toaster = _context.Toasters.FirstOrDefault();
            toaster.Time = time;
            toaster.Profile = Profile.NoProfile;

            _context.Entry(toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }

            return StatusCode(200, toaster.Time.ToString()); 
        }

        /// <summary>
        /// Set toasting profile 
        /// </summary>
        /// <param name="profile">Profile with different time settings</param>
        /// <returns>Profile set</returns>
        /// <response code="200">Profile set</response>
        /// <response code="417">Profile error</response>
        [HttpPatch("/api/toaster/SetProfile/{profile}")]
        public IActionResult SetProfile(Profile profile)
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
                    return StatusCode(417, "Profile error.");
            }

            _context.Entry(toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }

            return StatusCode(200, toaster.Profile.ToString());
        }

        /// <summary>
        /// Set number of toasts 
        /// </summary>
        /// <param name="numToasts">Number of toasts. Maximum 2</param>
        /// <returns>Toasts set</returns>
        /// <response code="200">Number of toasts in the toaster</response>
        /// <response code="417">Error in the number of toasts</response>
        [HttpPatch("/api/toaster/SetToasts/{numToasts}")]
        public IActionResult SetToasts(int numToasts)
        {
            if (numToasts > 2)
            {
                return StatusCode(417, "The maximum number of toasts is 2.");
            }

            Toaster toaster = _context.Toasters.FirstOrDefault();
            PantryController pantry = new PantryController(_context);

            if (pantry.HowManyBreads() >= 0)
            {
                toaster.NumToasts = pantry.pGetBreads(numToasts);
            }

            _context.Entry(toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }

            return StatusCode(200, toaster.NumToasts.ToString());
        }

        /// <summary>
        /// Turns On/Off the toaster
        /// </summary>
        /// <param name="status">Starts or stops the toaster</param>
        /// <returns>Current status</returns>
        /// <response code="200">Message with the current status of the IToast</response>
        /// <response code="417">Error: No bread in the Toaster</response>
        [HttpPut("api/toasters/toast/{status}")]
        public IActionResult Toast(Status status)
        {
            Toaster toaster = _context.Toasters.FirstOrDefault();

            if (toaster.Status == status) return StatusCode(200, String.Format("IToast status is: {0}", toaster.Status));

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
                    {
                        return StatusCode(417, "Error: No bread in the Toaster.");
                    }                        
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
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
            return StatusCode(200, String.Format("IToast status is: {0}", toaster.Status));
        }

        /// <summary>
        /// Toasts with a specific number of toasts and time 
        /// </summary>
        /// <param name="numToasts">Number of toasts</param>
        /// <param name="time">Time in seconds</param>
        /// <returns>Current status of the toaster</returns>
        /// <response code="200">Message with the current status of the IToast</response>
        [HttpPut("api/toaster/toast/numToasts/{numToasts}/time/{time}")]
        public IActionResult Toast(int numToasts, int time)
        {
            try
            {
                ObjectResult res = new ObjectResult(null);

                res = (ObjectResult)SetToasts(numToasts);
                if (res.StatusCode.Value != 200) throw new Exception(res.Value.ToString());
                
                res = (ObjectResult)SetTime(time);
                if (res.StatusCode.Value != 200) throw new Exception(res.Value.ToString());

                return Toast(Status.On);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Eror: " + ex.Message);
            }

        }

    }
}