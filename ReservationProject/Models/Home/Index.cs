﻿using Microsoft.AspNetCore.Mvc.Rendering;
using ReservationProject.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationProject.Models.Home
{
    public class Index
    {
        [Required(ErrorMessage = "Lastname: Required")]
        public string LastName { get; set; }
       
        [Required(ErrorMessage = "Firstname: Required")]
        public string FirstName { get; set; }
              
        [Required(ErrorMessage = "Email: Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone: Required")]
        public string Phone { get; set; }



        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Reservation Date")]
        public DateTime ResDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Reservation Time")]
        public DateTime ResTime { get; set; }

        //properties to create reservation
        public DateTime StartTime { get => ResDate.AddHours(ResTime.Hour).AddMinutes(ResTime.Minute); }




        //public DateTime StartTime { get; set; }
        public int Duration { get; set; } //Minutes
        public DateTime EndTime { get => StartTime.AddMinutes(Duration); } //Pre-defining EndTime (Adding our duration into our StartTime)
        [Required(ErrorMessage = "No of Guests: Required")]
        public int Guests { get; set; }
        public string Note { get; set; }
        public int ReservationStatusId { get; set; }
        public int ReservationSourceId { get; set; }

        [Display(Name = "Sitting Types")]
        public int SittingId { get; set; }
        public SelectList SittingTypes { get; set; }


    }
}
