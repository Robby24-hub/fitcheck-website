namespace FitCheckWebApp.Models
{
    public class Class
    {
        public int Id { get; set; }
        public int AccountID { get; set; }  
        public DayOfWeekClass Day { get; set; }
        public ClassType Type { get; set; }
        public TimeSpan Time { get; set; }
        public int DurationMinutes { get; set; }
        public int ParticipantLimit { get; set; }
        public int ParticipantsCount { get; set; }

       
        public string? InstructorName { get; set; }
    }

    public enum ClassType
    {
        Yoga,
        Zumba,
        Pilates,
        CrossFit,
        HIIT,
        StrengthTraining,
        Cardio,
        DanceFitness
    }

    public enum DayOfWeekClass
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }
}