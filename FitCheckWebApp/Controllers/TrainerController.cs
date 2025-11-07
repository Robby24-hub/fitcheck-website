using System.Security.Claims;
using FitCheckWebApp.DataAccess;
using FitCheckWebApp.Helpers;
using FitCheckWebApp.Models;
using FitCheckWebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



namespace FitCheckWebApp.Controllers
{
    public class TrainerController : Controller
    {

        [Authorize(Roles = "trainer")]
        public IActionResult TrainerClass()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUser = AccountManager.FindById(userId);


            var trainerClasses = ClassManager.GetAllClassesForTrainer(userId);

            var classesByDay = trainerClasses
                .GroupBy(c => c.Day)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => new ClassDisplayViewModel
                    {
                        Id = c.Id,
                        Type = FormatClassType(c.Type),
                        InstructorName = c.InstructorName ?? "Unassigned",
                        Time = c.Time,
                        DurationMinutes = c.DurationMinutes,
                        ParticipantLimit = c.ParticipantLimit,
                        ParticipantsCount = c.ParticipantsCount
                    })
                    .OrderBy(c => c.Time)
                    .ToList()
                );

            var model = new ClassesUserViewModel
            {
                ClassesByDay = classesByDay,
                TrainerName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "Trainer"
            };

            return View(model);

        }

        //Helpers:

        private string FormatClassType(ClassType type)
        {
            return System.Text.RegularExpressions.Regex.Replace(type.ToString(), "([a-z])([A-Z])", "$1 $2");
        }

    }
}
