using ToastCore.Models;
using System;
using System.Linq;

namespace ToastCore.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ToastCoreContext context)
        {
            //context.Database.EnsureDeleted(); //Execute this command when updating the model
            context.Database.EnsureCreated();


            if (!context.Toasters.Any())
            {
                var toaster = new Toaster
                {
                    Profile = 0,
                    Status = Status.Off,
                    NumToasts = 0,
                    Time = 0,
                    TimeStart = new DateTime().ToString(),
                    TimeEnd = new DateTime().ToString(),
                    ToastsMade = 0
                };

                context.Toasters.Add(toaster);
                context.SaveChanges();
            }         

            if (!context.Pantries.Any())
            {
                var pantry = new Pantry
                {
                    NumberOfBreads = 100,
                    Status = PantryStatus.Full
                };

                context.Pantries.Add(pantry);
                context.SaveChanges();
            }

            
        }
    }
}
