using FitCheckWebApp.Models;

namespace FitCheckWebApp.ViewModels
{
    public class ClassesUserViewModel
    {
        public Dictionary<DayOfWeekClass, List<ClassDisplayViewModel>> ClassesByDay { get; set; } = new();
        public bool HasActiveMembership { get; set; }
        public string? MembershipPlan { get; set; }
    }

    public class ClassDisplayViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public TimeSpan Time { get; set; }
        public int DurationMinutes { get; set; }
        public int ParticipantLimit { get; set; }
        public int ParticipantsCount { get; set; }
        public bool IsFull => ParticipantsCount >= ParticipantLimit;
    }
}