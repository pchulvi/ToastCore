using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ToastCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Cors;

namespace ToastCore.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ToasterController : Controller
    {
        private readonly ToastCoreContext _context;
        private readonly PantryController _pantry;
        private Toaster _toaster;

        /// <summary>
        /// Controller of our IToast
        /// </summary>
        /// <param name="context">Context of the database</param>
        public ToasterController(ToastCoreContext context)
        {
            _context = context;
            _pantry = new PantryController(_context);
            _toaster = context.Toasters.FirstOrDefault();
        }

        #region GET Methods

        /// <summary>
        /// Gets Toaster info
        /// </summary>
        /// <returns>Toaster data</returns>
        [HttpGet("/api/toaster/GetToasters")]
        [EnableCors("MyPolicy")]
        public IQueryable<Toaster> GetToaster()
        {
            return _context.Toasters;
        }

        /// <summary>
        /// Current status of the toaster
        /// </summary>
        /// <returns>Current status of the toaster</returns>
        /// <response code="200">Current status of the IToast</response>
        [HttpGet("/api/toaster/GetCurrentStatus")]
        [EnableCors("MyPolicy")]
        public IActionResult GetCurrentStatus()
        {
            return StatusCode(200, _toaster.Status);
        }

        /// <summary>
        /// Number of toasts made by our current toaster
        /// </summary>
        /// <returns>Number of toasts made by our current toaster</returns>
        /// <response code="200">Number of toasts made by the IToast</response>
        [HttpGet("/api/toaster/HowManyToastsMade")]
        [EnableCors("MyPolicy")]
        public IActionResult HowManyToastsMade()
        {
            return StatusCode(200, _toaster.ToastsMade);
        }

        #endregion

        #region PATCH Methods

        /// <summary>
        /// Set time for the toaster
        /// </summary>
        /// <param name="time">Number of seconds</param>
        /// <returns>Number of seconds set</returns>
        /// <response code="200">Number of seconds set</response>
        [HttpPatch("/api/toaster/SetTime/{time}")]
        [EnableCors("MyPolicy")]
        public IActionResult SetTime(int time)
        {
            _toaster.Time = time;
            _toaster.Profile = Profile.NoProfile;

            _context.Entry(_toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }

            return StatusCode(200, _toaster.Time.ToString()); 
        }

        /// <summary>
        /// Set toasting profile 
        /// </summary>
        /// <param name="profile">Profile with different time settings</param>
        /// <returns>Profile set</returns>
        /// <response code="200">Profile set</response>
        /// <response code="417">Profile error</response>
        [HttpPatch("/api/toaster/SetProfile/{profile}")]
        [EnableCors("MyPolicy")]
        public IActionResult SetProfile(Profile profile)
        {
            _toaster.Profile = profile;

            switch (profile)
            {
                case Profile.NoProfile:
                    _toaster.Time = 0;
                    break;
                case Profile.Low:
                    _toaster.Time = 90;
                    break;
                case Profile.Normal:
                    _toaster.Time = 180;
                    break;
                case Profile.High:
                    _toaster.Time = 360;
                    break;
                case Profile.Burnt:
                    _toaster.Time = 600;
                    break;
                default:
                    return StatusCode(417, "Profile error.");
            }

            _context.Entry(_toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }

            return StatusCode(200, _toaster.Profile.ToString());
        }

        /// <summary>
        /// Set number of toasts 
        /// </summary>
        /// <param name="numToasts">Number of toasts. Maximum 2</param>
        /// <returns>Toasts set</returns>
        /// <response code="200">Number of toasts in the toaster</response>
        /// <response code="417">Error in the number of toasts</response>
        [HttpPatch("/api/toaster/SetToasts/{numToasts}")]
        [EnableCors("MyPolicy")]
        public IActionResult SetToasts(int numToasts)
        {
            if (numToasts > 2)
            {
                return StatusCode(417, "The maximum number of toasts is 2.");
            }

            if (_pantry.HowManyBreads() >= 0)
            {
                _toaster.NumToasts = _pantry.pGetBreads(numToasts);
            }

            _context.Entry(_toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }

            return StatusCode(200, _toaster.NumToasts.ToString());
        }

        #endregion

        #region PUT Methods

        /// <summary>
        /// Resets the IToast to its starting values
        /// </summary>
        /// <returns>Status of the IToast</returns>
        /// <response code="200">Message with the current status of the IToast</response>
        /// <response code="417">Error: IToast is already Off</response>
        [HttpPut("api/toasters/reset")]
        [EnableCors("MyPolicy")]
        public IActionResult ResetToaster()
        {
            if (_toaster.Status == Status.Off) return StatusCode(417, String.Format("IToast status is: {0}", _toaster.Status));

            ObjectResult res = new ObjectResult(null);

            res = (ObjectResult)Toast(Status.Off);
            if (res.StatusCode.Value != 200) throw new Exception(res.Value.ToString());

            return StatusCode(200, String.Format("IToast status is: {0}", _toaster.Status));
        }

        /// <summary>
        /// Turns On/Off the toaster
        /// </summary>
        /// <param name="status">Starts or stops the toaster</param>
        /// <returns>Current status</returns>
        /// <response code="200">Message with the current status of the IToast</response>
        /// <response code="417">Error: No bread in the Toaster</response>
        [HttpPut("api/toasters/toast/{status}")]
        [EnableCors("MyPolicy")]
        public IActionResult Toast(Status status)
        {
            if (_toaster.Status == status) return StatusCode(200, String.Format("IToast status is: {0}", _toaster.Status));

            _toaster.Status = status;

            _context.Entry(_toaster).State = EntityState.Modified;

            switch (_toaster.Status)
            {
                case Status.On:
                    if (_toaster.NumToasts > 0)
                    {
                        _toaster.ToastsMade += _toaster.NumToasts;
                        _toaster.TimeStart = DateTime.Now.ToString();
                        _toaster.TimeEnd = DateTime.Now.AddSeconds(_toaster.Time).ToString();
                    }
                    else
                    {
                        return StatusCode(417, "Error: No bread in the Toaster.");
                    }                        
                    break;

                default:
                    _toaster.NumToasts = 0;
                    _toaster.Profile = Profile.NoProfile;
                    _toaster.Time = 0;
                    _toaster.TimeStart = new DateTime().ToString();
                    _toaster.TimeEnd = new DateTime().ToString();
                    break;
            }

            _context.Entry(_toaster).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
            return StatusCode(200, String.Format("IToast status is: {0}", _toaster.Status));
        }

        /// <summary>
        /// Toasts with a specific number of toasts and time 
        /// </summary>
        /// <param name="numToasts">Number of toasts</param>
        /// <param name="time">Time in seconds</param>
        /// <returns>Current status of the toaster</returns>
        /// <response code="200">Message with the current status of the IToast</response>
        [HttpPut("api/toaster/toast/numToasts/{numToasts}/time/{time}")]
        [EnableCors("MyPolicy")]
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

        #endregion
    }
}