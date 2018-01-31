using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToastCore.Models;
using Microsoft.AspNetCore.Cors;

namespace ToastCore.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PantryController : Controller
    {
        private readonly ToastCoreContext _context;
        private readonly SuperMarketController _superMarket;
        private Pantry _pantry;

        public PantryController(ToastCoreContext context)
        {
            _context = context;
            _superMarket = new SuperMarketController();
            _pantry = context.Pantries.FirstOrDefault();
        }

        /// <summary>
        /// Sets the number of breads in our pantry
        /// </summary>
        /// <param name="nBreads">Number of breads</param>
        /// <response code="200">Ok. Returns nbreads</response>
        /// <response code="500">Error 500</response>
        [HttpPut("/api/pantry/breads/{nBreads}")]
        [EnableCors("MyPolicy")]
        public IActionResult PutBreads(int nBreads)
        {
            _pantry.NumberOfBreads = nBreads;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }

            return StatusCode(200, (int)nBreads);

        }

        /// <summary>
        /// Get a number of breads for the toaster
        /// </summary>
        /// <param name="nBreads">Number of breads. It can't be more than 2 breads in toaster</param>
        /// <response code="200">Ok. Returns nbreads</response>
        /// <response code="417">Error in GetBreads</response>
        /// <response code="500">Error 500</response>
        [HttpGet("/api/pantry/breads/{nBreads}")]
        [EnableCors("MyPolicy")]
        public IActionResult GetBreads(int nBreads)
        {
            if (nBreads > 2)
            {
                return StatusCode(417, "The number of breads can't be more than 2");
            }
          
            int breads = 0;
            try
            {
                breads = pGetBreads(nBreads);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
            
            return StatusCode(200, nBreads);
            
        }

        /// <summary>
        /// Discounts breads from Pantry
        /// </summary>
        /// <param name="breadsArg"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public int pGetBreads(int breadsArg)
        {
            int rdo = _pantry.NumberOfBreads - breadsArg;

            try
            {
                _pantry.NumberOfBreads = rdo;
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw ex;
            }

            return breadsArg;
        }

        /// <summary>
        /// Is there bread in pantry?
        /// </summary>
        /// <returns>Boolean</returns>
        /// <response code="200">Has bread the IToast?</response>
        [HttpGet("/api/pantry/hasbread")]
        [EnableCors("MyPolicy")]
        public IActionResult HasBread()
        {
            return StatusCode(200, (this.HowManyBreads() > 0).ToString());
        }

        /// <summary>
        /// Actual status of our pantry: 0 = Empty, 1 = AlmostEmpy, 2 = Normal or 3 = Full
        /// </summary>
        /// <returns>PantryStatus Enumeration</returns>
        /// <response code="200">Ok. Returns StatusPantry</response>
        /// <response code="500">Error 500</response>
        [HttpGet("/api/pantry/status")]
        [EnableCors("MyPolicy")]
        public IActionResult GetStatus()
        {
            int howManyBreads = _pantry.NumberOfBreads;

            PantryStatus pStatus = _pantry.Status;

            if (howManyBreads <= 10) pStatus= PantryStatus.AlmostEmpty;
            if (howManyBreads > 10 && howManyBreads <= 90) pStatus = PantryStatus.Normal;
            if (howManyBreads > 90) pStatus= PantryStatus.Full;
            if (howManyBreads == 0) pStatus = PantryStatus.Empty;

            _pantry.Status = pStatus;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }

            return StatusCode(200, _pantry.Status.ToString());
        }

        /// <summary>
        /// Buy breads in Supermarket
        /// </summary>
        /// <param name="nBreads"></param>
        /// <returns>Returns total breads bought</returns>
        [HttpPost("/api/pantry/breads/buy/{nBreads}")]
        [EnableCors("MyPolicy")]
        public IActionResult BuyToSupermarket(int nBreads)
        {
            int breads = 0;
            try
            {
                breads = _superMarket.getBreads(nBreads);
            }
            catch
            {
                return StatusCode(500, "I can't sell more than 60 breads");
            }

            int howmanybreadsNow = this.HowManyBreads();

            int total = howmanybreadsNow + breads;

            if (total > 100)
            {
                return StatusCode(500, "The total capacity of pantry is 100 breads");
            }

            ObjectResult res = new ObjectResult(null);

            res = (ObjectResult)this.PutBreads(total);
            if (res.StatusCode.Value != 200) return StatusCode(500, "Eror: " + res.Value.ToString());

            return StatusCode(200, total);
        }

        /// GET: HowManyBreads
        /// <summary>
        /// How many breads are in our pantry
        /// </summary>
        /// <returns>A number of breads (as int)</returns>
        //[ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("/api/pantry/howmanybreads")]
        [EnableCors("MyPolicy")]
        public int HowManyBreads()
        {
            return _pantry.NumberOfBreads;
        }

        ///// <summary>
        ///// How many breads are in our pantry
        ///// </summary>
        ///// <returns>A number of breads (as int)</returns>
        //[HttpGet]
        //[EnableCors("MyPolicy")]
        //public int HowManyBreadsInPantry()
        //{
        //    return _pantry.NumberOfBreads;
        //}
    }
}