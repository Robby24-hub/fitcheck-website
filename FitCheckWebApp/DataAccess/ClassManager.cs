using MySql.Data.MySqlClient;
using FitCheckWebApp.Models;
using System.Collections.Generic;

namespace FitCheckWebApp.DataAccess
{
    public class ClassManager
    {
        private static readonly string connectionString = "server=localhost;user id=root;password=;database=fitcheckdb;";

        public static List<Class> GetAllClasses()
        {
            try
            {
                var classes = new List<Class>();
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                SELECT c.*, 
                       CONCAT(a.FirstName, ' ', a.LastName) AS InstructorName
                FROM Class c
                LEFT JOIN Account a ON c.AccountID = a.Id
            ";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    string typeValue = reader.GetString("Type").Replace(" ", "");
                                    string dayValue = reader.GetString("Day");

                                    var cls = new Class
                                    {
                                        Id = reader.GetInt32("Id"),
                                        AccountID = reader.GetInt32("AccountID"),
                                        InstructorName = reader.IsDBNull(reader.GetOrdinal("InstructorName"))
                                            ? "Unassigned"
                                            : reader.GetString("InstructorName"),
                                        Type = Enum.Parse<ClassType>(typeValue, ignoreCase: true),
                                        Day = Enum.Parse<DayOfWeekClass>(dayValue, ignoreCase: true),
                                        Time = reader.GetTimeSpan("Time"),
                                        DurationMinutes = reader.GetInt32("DurationMinutes"),
                                        ParticipantLimit = reader.GetInt32("ParticipantLimit"),
                                        ParticipantsCount = reader.GetInt32("ParticipantsCount")
                                    };
                                    classes.Add(cls);
                                }
                                catch (Exception ex)
                                {
                                    // Log the problematic row
                                    Console.WriteLine($"Error parsing class ID {reader.GetInt32("Id")}: {ex.Message}");
                                    Console.WriteLine($"Type value: '{reader.GetString("Type")}', Day value: '{reader.GetString("Day")}'");
                                    // Skip this row and continue
                                    continue;
                                }
                            }
                        }
                    }
                }
                return classes;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while retrieving all classes: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all classes: {ex.Message}", ex);
            }
        }

        public static Class? GetClassById(int id)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
        SELECT c.*, 
               CONCAT(a.FirstName, ' ', a.LastName) AS InstructorName
        FROM Class c
        LEFT JOIN Account a ON c.AccountID = a.Id
        WHERE c.Id = @Id
    ";
                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Class
                    {
                        Id = reader.GetInt32("Id"),
                        AccountID = reader.GetInt32("AccountID"),
                        InstructorName = reader.IsDBNull(reader.GetOrdinal("InstructorName"))
                            ? "Unassigned"
                            : reader.GetString("InstructorName"),
                        Type = Enum.Parse<ClassType>(reader.GetString("Type").Replace(" ", ""), ignoreCase: true),
                        Day = Enum.Parse<DayOfWeekClass>(reader.GetString("Day"), ignoreCase: true),
                        Time = reader.GetTimeSpan("Time"),
                        DurationMinutes = reader.GetInt32("DurationMinutes"),
                        ParticipantLimit = reader.GetInt32("ParticipantLimit"),
                        ParticipantsCount = reader.GetInt32("ParticipantsCount")
                    };
                }
                return null;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while retrieving class by ID: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving class by ID: {ex.Message}", ex);
            }
        }

        public static bool IncrementParticipantCount(int classId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                using var transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                try
                {
                    cmd.CommandText = @"
            SELECT ParticipantsCount, ParticipantLimit 
            FROM Class 
            WHERE Id = @Id
        ";
                    cmd.Parameters.AddWithValue("@Id", classId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            transaction.Rollback();
                            return false;
                        }

                        int currentCount = reader.GetInt32("ParticipantsCount");
                        int limit = reader.GetInt32("ParticipantLimit");

                        if (currentCount >= limit)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                    UPDATE Class 
                    SET ParticipantsCount = ParticipantsCount + 1 
                    WHERE Id = @Id 
                    AND ParticipantsCount < ParticipantLimit
                ";
                    cmd.Parameters.AddWithValue("@Id", classId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while incrementing participant count: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error incrementing participant count: {ex.Message}", ex);
            }
        }

        public static int CountClassesByDay(DayOfWeekClass day)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "SELECT COUNT(*) FROM Class WHERE Day = @Day";
                cmd.Parameters.AddWithValue("@Day", day.ToString());

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while counting classes by day: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error counting classes by day: {ex.Message}", ex);
            }
        }

        public static int CountClassesByDayExcludingId(DayOfWeekClass day, int excludeId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "SELECT COUNT(*) FROM Class WHERE Day = @Day AND Id != @Id";
                cmd.Parameters.AddWithValue("@Day", day.ToString());
                cmd.Parameters.AddWithValue("@Id", excludeId);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while counting classes by day excluding ID: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error counting classes by day excluding ID: {ex.Message}", ex);
            }
        }

        public static void AddClass(Class cls)
        {
            try
            {
                int existingClassCount = CountClassesByDay(cls.Day);
                if (existingClassCount >= 3)
                {
                    throw new InvalidOperationException($"Cannot add class. Maximum of 3 classes allowed per day. {cls.Day} already has {existingClassCount} classes.");
                }

                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                INSERT INTO Class (AccountID, Type, Day, Time, DurationMinutes, ParticipantLimit, ParticipantsCount)
                VALUES (@AccountID, @Type, @Day, @Time, @DurationMinutes, @ParticipantLimit, @ParticipantsCount)
            ";

                cmd.Parameters.AddWithValue("@AccountID", cls.AccountID);
                cmd.Parameters.AddWithValue("@Type", cls.Type.ToString());
                cmd.Parameters.AddWithValue("@Day", cls.Day.ToString());
                cmd.Parameters.AddWithValue("@Time", cls.Time);
                cmd.Parameters.AddWithValue("@DurationMinutes", cls.DurationMinutes);
                cmd.Parameters.AddWithValue("@ParticipantLimit", cls.ParticipantLimit);
                cmd.Parameters.AddWithValue("@ParticipantsCount", cls.ParticipantsCount);

                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while adding class: {ex.Message}", ex);
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions as-is
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding class: {ex.Message}", ex);
            }
        }

        public static void UpdateClass(Class cls)
        {
            try
            {
                int existingClassCount = CountClassesByDayExcludingId(cls.Day, cls.Id);
                if (existingClassCount >= 3)
                {
                    throw new InvalidOperationException($"Cannot update class. Maximum of 3 classes allowed per day. {cls.Day} already has {existingClassCount} other classes.");
                }

                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
                UPDATE Class 
                SET AccountID=@AccountID, 
                    Type=@Type, 
                    Day=@Day,
                    Time=@Time, 
                    DurationMinutes=@DurationMinutes,
                    ParticipantLimit=@ParticipantLimit, 
                    ParticipantsCount=@ParticipantsCount 
                WHERE Id=@Id
            ";

                cmd.Parameters.AddWithValue("@AccountID", cls.AccountID);
                cmd.Parameters.AddWithValue("@Type", cls.Type.ToString());
                cmd.Parameters.AddWithValue("@Day", cls.Day.ToString());
                cmd.Parameters.AddWithValue("@Time", cls.Time);
                cmd.Parameters.AddWithValue("@DurationMinutes", cls.DurationMinutes);
                cmd.Parameters.AddWithValue("@ParticipantLimit", cls.ParticipantLimit);
                cmd.Parameters.AddWithValue("@ParticipantsCount", cls.ParticipantsCount);
                cmd.Parameters.AddWithValue("@Id", cls.Id);

                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while updating class: {ex.Message}", ex);
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions as-is
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating class: {ex.Message}", ex);
            }
        }

        public static int CountUpcomingClassesToday()
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                // Get today's day name (e.g., "Monday", "Tuesday")
                string today = DateTime.Now.DayOfWeek.ToString();

                // Get current time
                TimeSpan currentTime = DateTime.Now.TimeOfDay;

                cmd.CommandText = @"
                SELECT COUNT(*) 
                FROM Class 
                WHERE Day = @Day 
                AND Time > @CurrentTime
            ";
                cmd.Parameters.AddWithValue("@Day", today);
                cmd.Parameters.AddWithValue("@CurrentTime", currentTime);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while counting upcoming classes: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error counting upcoming classes: {ex.Message}", ex);
            }
        }

        public static bool DeleteClass(int classId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Class WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", classId);

                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while deleting class: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting class: {ex.Message}", ex);
            }
        }

        public static List<Class> GetAllClassesForTrainer(int trainerId)
        {
            try
            {
                var allClasses = GetAllClasses();
                return allClasses.Where(c => c.AccountID == trainerId).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving classes for trainer: {ex.Message}", ex);
            }
        }



        public static bool AddUserToClass(int userId, int classId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
            INSERT INTO UserClassJoin (UserId, ClassId) 
            VALUES (@UserId, @ClassId)
        ";
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@ClassId", classId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (MySqlException ex)
            {

                if (ex.Message.Contains("Duplicate"))
                {
                    return false;
                }
                throw new Exception($"Database error while adding user to class: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding user to class: {ex.Message}", ex);
            }
        }

        public static bool RemoveUserFromClass(int userId, int classId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
            DELETE FROM UserClassJoin 
            WHERE UserId = @UserId AND ClassId = @ClassId
        ";
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@ClassId", classId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while removing user from class: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing user from class: {ex.Message}", ex);
            }
        }

        public static HashSet<int> GetUserJoinedClassIds(int userId)
        {
            try
            {
                var joinedClassIds = new HashSet<int>();
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
            SELECT ClassId 
            FROM UserClassJoin 
            WHERE UserId = @UserId
        ";
                cmd.Parameters.AddWithValue("@UserId", userId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    joinedClassIds.Add(reader.GetInt32("ClassId"));
                }

                return joinedClassIds;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while getting user joined classes: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user joined classes: {ex.Message}", ex);
            }
        }

        public static bool IsUserJoinedClass(int userId, int classId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"
            SELECT COUNT(*) 
            FROM UserClassJoin 
            WHERE UserId = @UserId AND ClassId = @ClassId
        ";
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@ClassId", classId);

                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while checking user class join: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking user class join: {ex.Message}", ex);
            }
        }

        public static bool DecrementParticipantCount(int classId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();

                using var transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                try
                {
                    cmd.CommandText = @"
                SELECT ParticipantsCount 
                FROM Class 
                WHERE Id = @Id
            ";
                    cmd.Parameters.AddWithValue("@Id", classId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            transaction.Rollback();
                            return false;
                        }

                        int currentCount = reader.GetInt32("ParticipantsCount");

                        if (currentCount <= 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                UPDATE Class 
                SET ParticipantsCount = ParticipantsCount - 1 
                WHERE Id = @Id 
                AND ParticipantsCount > 0
            ";
                    cmd.Parameters.AddWithValue("@Id", classId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Database error while decrementing participant count: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error decrementing participant count: {ex.Message}", ex);
            }
        }

    }
}