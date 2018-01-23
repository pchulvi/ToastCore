using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToastCore.Models;

namespace ToastCore.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PantryController : Controller
    {
        private readonly ToastCoreContext _context;

        public PantryController(ToastCoreContext context)
        {
            _context = context;
        }

        /// GET: HowManyBreads
        /// <summary>
        /// How many breads are in our pantry
        /// </summary>
        /// <returns>A number of breads (as int)</returns>
        [HttpGet("/api/pantry/howmanybreads")]
        [EnableCors("MyPolicy")]
        public int HowManyBreads()
        {
            Pantry pantry = _context.Pantries.FirstOrDefault();

            return pantry.NumberOfBreads;
        }


        /// <summary>
        /// Update the number of breads in our pantry
        /// </summary>
        /// <param name="nBreads">Number of breads</param>
        /// <response code="200">Ok. Returns nbreads</response>
        /// <response code="500">Error 500</response>
        [HttpPut("/api/pantry/breads/{nBreads}")]
        public IActionResult PutBreads(int nBreads)
        {
            Pantry pantry = _context.Pantries.FirstOrDefault();
            pantry.NumberOfBreads = nBreads;


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
        public IActionResult GetBreads(int nBreads)
        {
            //if (nBreads < 1) throw new Exception("The number of breads can't be 0 or less 0.");

            if (nBreads > 2)
            {
                return StatusCode(417, "The number of breads can't be more than 2");
            }
          

            //if (rdo < 0)
            //{
            //    return StatusCode(417, "Insufficient breads for toasting. There are {0} breads now in pantry.");
            //}

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


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public int pGetBreads(int breadsArg)
        {
            Pantry pantry = _context.Pantries.FirstOrDefault();

            int rdo = pantry.NumberOfBreads - breadsArg;

            try
            {
                pantry.NumberOfBreads = rdo;
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
        [HttpGet("/api/pantry/hasbread")]
        public bool HasBread()
        {
            return this.HowManyBreads() > 0;
        }

        /// <summary>
        /// Actual status of our pantry: 0 = Empty, 1 = AlmostEmpy, 2 = Normal or 3 = Full
        /// </summary>
        /// <returns>PantryStatus Enumeration</returns>
        /// <response code="200">Ok. Returns StatusPantry</response>
        /// <response code="500">Error 500</response>
        [HttpGet("/api/pantry/status")]
        public IActionResult GetStatus()
        {
            Pantry pantry = _context.Pantries.FirstOrDefault();

            int howManyBreads = pantry.NumberOfBreads;

            PantryStatus pStatus = pantry.Status;

            if (howManyBreads == 0) pStatus=PantryStatus.Empty;
            if (howManyBreads <= 10) pStatus= PantryStatus.AlmostEmpty;
            if (howManyBreads > 90) pStatus= PantryStatus.Full;

            pStatus=PantryStatus.Normal;

            pantry.Status = pStatus;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }

            return StatusCode(200, pantry.Status.ToString());
        }

        /// <summary>
        /// Buy breads in Supermarket
        /// </summary>
        /// <param name="nBreads"></param>
        /// <returns>Returns total breads bought</returns>
        [HttpPost("/api/pantry/breads/buy/{nBreads}")]
        public IActionResult BuyToSupermarket(int nBreads)
        {
            // int breads = Int32.Parse(new SuperMarketController().SellBread(nBreads).ToString());

            int breads = 0;
            try
            {
                breads = new SuperMarketController().getBreads(nBreads);
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

            this.PutBreads(total);

            return StatusCode(200, total);
        }
    }
}