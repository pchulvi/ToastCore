using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public int HowManyBreads()
        {
            Pantry pantry = _context.Pantries.FirstOrDefault();

            return pantry.NumberOfBreads;
        }


        /// <summary>
        /// Update the number of breads in our pantry
        /// </summary>
        /// <param name="nBreads">Number of breads</param>
        [HttpPut("/api/pantry/breads/{nBreads}")]
        public void PutBreads(int nBreads)
        {
            Pantry pantry = _context.Pantries.FirstOrDefault();
            pantry.NumberOfBreads = nBreads;

            _context.SaveChanges();
        }

        /// <summary>
        /// Get a number of breads for the toaster
        /// </summary>
        /// <param name="nBreads">Number of breads. It can't be more than 2 breads in toaster</param>

        [HttpGet("/api/pantry/breads/{nBreads}")]
        public int GetBreads(int nBreads)
        {
            //if (nBreads < 1) throw new Exception("The number of breads can't be 0 or less 0.");

            if (nBreads > 2)
            {
                HttpContext.Response.ContentType = "text/plain";
                HttpContext.Response.WriteAsync("The number of breads can't be more than 2");
            }

            Pantry pantry = _context.Pantries.FirstOrDefault();
            pantry.NumberOfBreads = pantry.NumberOfBreads - nBreads;


            if (pantry.NumberOfBreads < 0)
            {
                HttpContext.Response.ContentType = "text/plain";
                HttpContext.Response.WriteAsync(String.Format("Insufficient breads for toasting. There are {0} breads now in pantry.", pantry.NumberOfBreads));
            }


            _context.SaveChanges();

            return nBreads;
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
        /// <returns>PantryStatus Object</returns>
        [HttpGet("/api/pantry/status")]
        public PantryStatus GetStatus()
        {
            int howManyBreads = this.HowManyBreads();

            if (howManyBreads == 0) return PantryStatus.Empty;
            if (howManyBreads <= 10) return PantryStatus.AlmostEmpty;
            if (howManyBreads > 90) return PantryStatus.Full;

            return PantryStatus.Normal;
        }

        /// <summary>
        /// Buy breads in Supermarket
        /// </summary>
        /// <param name="nBreads"></param>
        [HttpPost("/api/pantry/breads/buy/{nBreads}")]
        public void BuyToSupermarket(int nBreads)
        {
            int breads = new SuperMarketController().SellBread(nBreads);

            int howmanybreadsNow = this.HowManyBreads();
            this.PutBreads(howmanybreadsNow + breads);
        }
    }
}