using System;
using System.ComponentModel.DataAnnotations;

namespace ToastCore.Models
{
    /// <summary>
    /// Toaster class
    /// </summary>
    public class Toaster
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Status of the toaster
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// Time set at the toaster
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Profile type
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// Time of start toasting
        /// </summary>
        public string TimeStart { get; set; }

        /// <summary>
        /// Time of end toasting
        /// </summary>
        public string TimeEnd { get; set; }

        /// <summary>
        /// Total number of toasts made
        /// </summary>
        public int ToastsMade { get; set; }

        /// <summary>
        /// Current number of toasts to make
        /// </summary>
        public int NumToasts { get; set; }

    }
}