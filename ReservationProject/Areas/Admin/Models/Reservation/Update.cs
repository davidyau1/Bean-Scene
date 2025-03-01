﻿using Microsoft.AspNetCore.Mvc.Rendering;
using ReservationProject.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationProject.Areas.Admin.Models.Reservation
{
    public class Update
    {
        [Required]
        public int Id { get; set; }

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

        [Required]
        [Display(Name = "Duration of Reservation")]
        public int Duration { get; set; } //Minutes
        [Required]
        [Display(Name = "Number of Guests")]
        public int Guests { get; set; }
        [Display(Name = "Additional Notes")]
        public string Note { get; set; }


        //Relationships
        [Required]
        [Display(Name = "Status of Reservation")]
        public int ReservationStatusId { get; set; }
        public ReservationStatus ReservationStatus { get; set; }
        [Required]
        [Display(Name = "Source of Reservation")]
        public int ReservationSourceId { get; set; }
        public ReservationSource ReservationSource { get; set; }
        [Required]
        [Display(Name = "Sitting of Reservation")]
        public int SittingId { get; set; }
        public Data.Sitting Sitting { get; set; }

        public int PersonId { get; set; }
        public Person Person { get; set; }


        [Display(Name = "Status of Reservation")]
        public SelectList ReservationStatuses { get; set; }
        [Display(Name = "Source of Reservation")]
        public SelectList ReservationSources { get; set; }
        [Display(Name = "Sitting of Reservation")]
        public SelectList Sittings { get; set; }
    }
}
