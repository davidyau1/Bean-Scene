﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReservationProject.Data;
using ReservationProject.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationProject.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin, Staff")]
    public class ReservationController : AdminAreaBaseController
    {
        private readonly PersonService _personService;
        private readonly ILogger<ReservationController> _logger;
        private IMapper _mapper;

        public ReservationController(ApplicationDbContext context, IMapper mapper, PersonService personService, ILogger<ReservationController> logger)
          : base(context)
        {
            _mapper = mapper;
            _personService = personService;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Reservation Index at {time}", DateTime.UtcNow);
            Trace.Listeners.Add(new TextWriterTraceListener("MyOutput.log"));
            var reservation = await _context.Reservations
                .Include(rs => rs.ReservationSource)
                .Include(rst => rst.ReservationStatus)
                .Include(sr => sr.Sitting.Restaurant)
                .Include(s => s.Sitting)
                .Include(p => p.Person)
                .OrderBy(reservation => reservation.Id)
                .ToArrayAsync();
            Debug.Assert(reservation is not null, "Reservation is not null");
            Debug.WriteLineIf(reservation is  null, "Reservation is null");
            Trace.Close();
            

            return View(reservation);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("[HttpGet] Reservation at {time}", DateTime.UtcNow);

            var sourceList = await _context.ReservationSources.Select(s => new
            {
                Value = s.Id,
                Display = s.Name
            }).ToArrayAsync();

            var sittingList = await _context.Sittings.Select(r => new
            {
                Value = r.Id,
                Display = $"{r.Name} {r.StartTime.ToString("h:mm tt")} - {r.EndTime.ToString("h:mm tt")}"
            }).ToArrayAsync();

            _logger.LogInformation("Parsing in the create model for Reservations to render SelectLists");
            var model = new Models.Reservation.Create

            {
                ReservationSource = new SelectList(sourceList.ToList(), "Value", "Display"),               
                Sitting = new SelectList(sittingList.ToList(), "Value", "Display")

            };

            

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Models.Reservation.Create model)
        {
            _logger.LogInformation("[HttpPost] Reservation at {time}", DateTime.UtcNow);
            if (ModelState.IsValid)
            {
                try
                {
                    var sitting = await _context.Sittings.FindAsync(model.SittingId);
                    if (sitting.IsClosed == false)
                    {
                        var person = new Person
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email.ToLower(),
                            Phone = model.Phone
                        };
                        person = await _personService.UpsertPersonAsync(person, true);

                        var r = _mapper.Map<Data.Reservation>(model);
                        r.PersonId = person.Id;
                        r.ReservationStatusId =1;
                        _context.Reservations.Add(r);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Submit Reservation {time}", DateTime.UtcNow);

                        return RedirectToAction(nameof(Index));

                    }
                }
                catch(Exception)
                {
                    StackTrace st = new StackTrace(true);
                    for (int i = 0; i < st.FrameCount; i++)
                    {
                        StackFrame sf = st.GetFrame(i);
                        Console.WriteLine();
                        Console.WriteLine("High up the call stack, Method {0}");
                        sf.GetMethod();

                        Console.WriteLine("High up the call stack, Line Number: {0}");
                        sf.GetFileName();
                    }


                    StatusCode(500);
                }

            }

            var sourceList = await _context.ReservationSources.Select(s => new
            {
                Value = s.Id,
                Display = s.Name
            }).ToArrayAsync();
            var sittingList = await _context.Sittings.Select(r => new
            {
                Value = r.Id,
                Display = r.Name
            }).ToArrayAsync();
            model = new Models.Reservation.Create

            {
                ReservationSource = new SelectList(sourceList.ToList(), "Value", "Display"),
                Sitting = new SelectList(sittingList.ToList(), "Value", "Display")

            };
            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {

            try
            {
                _logger.LogWarning("If Reservation Id is equal to null, return to StatusCode 400");
                if(id == null)
                {
                    return StatusCode(400, "Id Required");
                }


                var selReservation = await _context.Reservations
                    .Include(rs => rs.ReservationSource)
                    .Include(rst => rst.ReservationStatus)
                    .Include(s => s.Sitting)
                    .Include(p => p.Person)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);

                if(selReservation == null)
                {
                    return NotFound();
                }

                var reservationStatusOptions = await _context.ReservationStatuses.Select(rs => new
                {
                    Value = rs.Id,
                    Display = rs.Name
                })
                .ToArrayAsync();

                var sourceList = await _context.ReservationSources.Select(s => new
                {
                    Value = s.Id,
                    Display = s.Name
                }).ToArrayAsync();

                var sittingList = await _context.Sittings.Where(s => s.StartTime.Date == selReservation.Sitting.StartTime.Date).Select(r => new
                {
                    Value = r.Id,
                    Display = r.Name
                }).ToArrayAsync();

                

                var m = _mapper.Map<Models.Reservation.Update>(selReservation);
                m.ResDate = selReservation.StartTime;
                m.ResTime = selReservation.StartTime;
                m.ReservationSources = new SelectList(sourceList, "Value", "Display");
                m.ReservationStatuses = new SelectList(reservationStatusOptions, "Value", "Display");
                m.Sittings = new SelectList(sittingList, "Value", "Display");
                m.Person.Id = m.PersonId;

                return View(m);

            }
            catch (Exception)
            {
                _logger.LogError("Reservation Error");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Models.Reservation.Update m)
        {
            if (id != m.Id)
            {
                return NotFound();
            }

            var reservationStatusOptions = await _context.ReservationStatuses.Select(rs => new
            {
                Value = rs.Id,
                Display = rs.Name
            })
                .ToArrayAsync();

            var sourceList = await _context.ReservationSources.Select(s => new
            {
                Value = s.Id,
                Display = s.Name
            }).ToArrayAsync();

            var sittingList = await _context.Sittings.Select(s => new
            {
                Value = s.Id,
                Display = $"{s.Name} {s.StartTime.ToString("h:mm tt")} - {s.EndTime.ToString("h:mm tt")}"
            }).ToArrayAsync();


            if (!ModelState.IsValid)
            {
                return View(m);
            }

            try
            {
                var r = _mapper.Map<Data.Reservation>(m);
                _context.Update<Reservation>(r);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        public JsonResult GetTimes(int id)
        {
            Task<Sitting> SittingTimes = _context.Sittings.FirstOrDefaultAsync(s => s.Id == id);
            SittingTimes.Wait();
            var sitting = SittingTimes.Result;

            List<DateTime> dates = new List<DateTime>();

            if (sitting != null)
            {
                dates.Add(sitting.StartTime);
                dates.Add(sitting.EndTime);
            }
            return Json(dates);
        }

        [HttpPost]
        public JsonResult GetSittings(string date)
        {
            var myDate = DateTime.Parse(date).Date;
            var sittingsList = _context.Sittings.Where(s => s.StartTime.Date == DateTime.Parse(date).Date);

            return Json(sittingsList);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {

            if (!id.HasValue)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(rs => rs.ReservationSource)
                .Include(rst => rst.ReservationStatus)
                .Include(sr => sr.Sitting.Restaurant)
                .Include(s => s.Sitting)
                .Include(p => p.Person)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }
        [HttpGet]
        public async Task<IActionResult> SittingIndex()
        {
            var sitting = await _context.Sittings
                .Include(r => r.Restaurant)
                .OrderBy(sitting => sitting.StartTime).ToArrayAsync();
            return View(sitting);
        }
        [HttpGet]
        public async Task<IActionResult> ReservationViaSitting(int? id)
        {
            var reservation = await _context.Reservations
             .Include(rs => rs.ReservationSource)
             .Include(rst => rst.ReservationStatus)
             .Include(sr => sr.Sitting.Restaurant)
             .Include(s => s.Sitting)
             .Include(p => p.Person)
             .OrderBy(reservation => reservation.Id)
             .Where(reservation => reservation.SittingId==id)
             .ToArrayAsync();
            return View(reservation);

        }


    }
}

