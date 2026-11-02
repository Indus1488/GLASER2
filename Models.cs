using System;
using System.Collections.Generic;
using System.Linq;

namespace GradeApp
{
    public enum UserRole
    {
        Teacher,
        Student
    }

    public class Discipline
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    public class Teacher
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public override string ToString() => FullName;
    }

    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public int GroupId { get; set; }
        public override string ToString() => FullName;
    }

    /// <summary>Учётная запись: логин, пароль и привязка к преподавателю или студенту.</summary>
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public UserRole Role { get; set; }
        public int? TeacherId { get; set; }
        public int? StudentId { get; set; }
    }

    /// <summary>Какую дисциплину в какой группе ведёт преподаватель.</summary>
    public class TeacherAssignment
    {
        public int TeacherId { get; set; }
        public int DisciplineId { get; set; }
        public int GroupId { get; set; }
    }

    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int DisciplineId { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = "";

        /// <summary>Мягкое удаление: запись не стирается, а помечается удалённой.</summary>
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    /// <summary>Текущий авторизованный пользователь.</summary>
    public static class Session
    {
        public static User? CurrentUser { get; set; }

        public static Teacher? CurrentTeacher =>
            CurrentUser?.TeacherId is int id ? SampleData.Teachers.FirstOrDefault(t => t.Id == id) : null;

        public static Student? CurrentStudent =>
            CurrentUser?.StudentId is int id ? SampleData.Students.FirstOrDefault(s => s.Id == id) : null;
    }

    public static class SampleData
    {
        public static readonly string[] GradeTypes =
        {
            "Контрольная", "Тест", "Лабораторная", "Курсовая", "Устный ответ", "Экзамен"
        };

        public static List<Discipline> Disciplines = new List<Discipline>
        {
            new Discipline { Id = 1, Name = "Высшая терминалия" },
            new Discipline { Id = 2, Name = "Основы нейроведения" },
            new Discipline { Id = 3, Name = "Фронтендоведение" },
            new Discipline { Id = 4, Name = "Сишарпование" },
            new Discipline { Id = 5, Name = "Бэкендоведение" },
        };

        public static List<Group> Groups = new List<Group>
        {
            new Group { Id = 1, Name = "ИС-21" },
            new Group { Id = 2, Name = "ИС-22" },
            new Group { Id = 3, Name = "ПР-21" },
            new Group { Id = 4, Name = "3ИСиП2" },
        };

        public static List<Teacher> Teachers = new List<Teacher>
        {
            new Teacher { Id = 1, FullName = "Медведев Дмитрий Игоревич" },
            new Teacher { Id = 2, FullName = "Орлова О.О." },
            new Teacher { Id = 3, FullName = "Морозов М.М." },
        };

        public static List<Student> Students = new List<Student>
        {
            new Student { Id = 1, FullName = "Петров П.П.",    GroupId = 1 },
            new Student { Id = 2, FullName = "Сидоров С.С.",   GroupId = 1 },
            new Student { Id = 3, FullName = "Кузнецова А.А.", GroupId = 1 },
            new Student { Id = 4, FullName = "Смирнов Д.Д.",   GroupId = 2 },
            new Student { Id = 5, FullName = "Волкова Е.Е.",   GroupId = 2 },
            new Student { Id = 6, FullName = "Серегин Н.И.",   GroupId = 4 },
            new Student { Id = 7, FullName = "Главин П.В.",    GroupId = 4 },
            new Student { Id = 8, FullName = "Клавдия А.И.",   GroupId = 4 },
        };

        public static List<User> Users = new List<User>
        {
            new User { Id = 1,  Login = "ivanov",   Password = "12345", Role = UserRole.Teacher, TeacherId = 1 },
            new User { Id = 2,  Login = "orlova",   Password = "12345", Role = UserRole.Teacher, TeacherId = 2 },
            new User { Id = 3,  Login = "morozov",  Password = "12345", Role = UserRole.Teacher, TeacherId = 3 },
            new User { Id = 9,  Login = "seregin",  Password = "11111", Role = UserRole.Student, StudentId = 6 },
            new User { Id = 4,  Login = "petrov",   Password = "11111", Role = UserRole.Student, StudentId = 1 },
            new User { Id = 5,  Login = "sidorov",  Password = "11111", Role = UserRole.Student, StudentId = 2 },
            new User { Id = 6,  Login = "kuznecova",Password = "11111", Role = UserRole.Student, StudentId = 3 },
            new User { Id = 7,  Login = "smirnov",  Password = "11111", Role = UserRole.Student, StudentId = 4 },
            new User { Id = 8,  Login = "volkova",  Password = "11111", Role = UserRole.Student, StudentId = 5 },
            new User { Id = 10, Login = "glavin",   Password = "11111", Role = UserRole.Student, StudentId = 7 },
            new User { Id = 11, Login = "klavdiya", Password = "11111", Role = UserRole.Student, StudentId = 8 },
        };

        public static List<TeacherAssignment> Assignments = new List<TeacherAssignment>
        {
            // Медведев: высшая терминалия и фронтендоведение в ИС-21 и ИС-22, сишарпование в ИС-21
            new TeacherAssignment { TeacherId = 1, DisciplineId = 1, GroupId = 1 },
            new TeacherAssignment { TeacherId = 1, DisciplineId = 1, GroupId = 2 },
            new TeacherAssignment { TeacherId = 1, DisciplineId = 3, GroupId = 1 },
            new TeacherAssignment { TeacherId = 1, DisciplineId = 3, GroupId = 2 },
            new TeacherAssignment { TeacherId = 1, DisciplineId = 4, GroupId = 1 },
            // Орлова: основы нейроведения в ИС-22 и ПР-21
            new TeacherAssignment { TeacherId = 2, DisciplineId = 2, GroupId = 2 },
            new TeacherAssignment { TeacherId = 2, DisciplineId = 2, GroupId = 3 },
            // Морозов: бэкендоведение в ПР-21 и ИС-21
            new TeacherAssignment { TeacherId = 3, DisciplineId = 5, GroupId = 3 },
            new TeacherAssignment { TeacherId = 3, DisciplineId = 5, GroupId = 1 },
            // 3ИСиП2: высшая терминалия, фронтендоведение, сишарпование (Медведев), основы нейроведения (Орлова), бэкендоведение (Морозов)
            new TeacherAssignment { TeacherId = 1, DisciplineId = 1, GroupId = 4 },
            new TeacherAssignment { TeacherId = 1, DisciplineId = 3, GroupId = 4 },
            new TeacherAssignment { TeacherId = 1, DisciplineId = 4, GroupId = 4 },
            new TeacherAssignment { TeacherId = 2, DisciplineId = 2, GroupId = 4 },
            new TeacherAssignment { TeacherId = 3, DisciplineId = 5, GroupId = 4 },
        };

        public static List<Grade> Grades = new List<Grade>
        {
            new Grade { Id = 1,  StudentId = 1, DisciplineId = 1, Value = 5, Date = DateTime.Today.AddDays(-10), Type = "Контрольная" },
            new Grade { Id = 2,  StudentId = 1, DisciplineId = 1, Value = 4, Date = DateTime.Today.AddDays(-5),  Type = "Тест" },
            new Grade { Id = 3,  StudentId = 1, DisciplineId = 3, Value = 5, Date = DateTime.Today.AddDays(-3),  Type = "Лабораторная" },
            new Grade { Id = 4,  StudentId = 2, DisciplineId = 1, Value = 3, Date = DateTime.Today.AddDays(-10), Type = "Контрольная" },
            new Grade { Id = 5,  StudentId = 2, DisciplineId = 1, Value = 4, Date = DateTime.Today.AddDays(-5),  Type = "Тест" },
            new Grade { Id = 6,  StudentId = 3, DisciplineId = 1, Value = 5, Date = DateTime.Today.AddDays(-10), Type = "Контрольная" },
            new Grade { Id = 7,  StudentId = 1, DisciplineId = 4, Value = 5, Date = DateTime.Today.AddDays(-2),  Type = "Курсовая" },
            new Grade { Id = 8,  StudentId = 6, DisciplineId = 1, Value = 4, Date = DateTime.Today.AddDays(-10), Type = "Контрольная" },
            new Grade { Id = 9,  StudentId = 6, DisciplineId = 5, Value = 5, Date = DateTime.Today.AddDays(-4),  Type = "Лабораторная" },
            new Grade { Id = 10, StudentId = 7, DisciplineId = 1, Value = 3, Date = DateTime.Today.AddDays(-8),  Type = "Тест" },
            new Grade { Id = 11, StudentId = 7, DisciplineId = 2, Value = 4, Date = DateTime.Today.AddDays(-6),  Type = "Устный ответ" },
            new Grade { Id = 12, StudentId = 8, DisciplineId = 2, Value = 5, Date = DateTime.Today.AddDays(-7),  Type = "Контрольная" },
            new Grade { Id = 13, StudentId = 8, DisciplineId = 5, Value = 4, Date = DateTime.Today.AddDays(-1),  Type = "Тест" },
            new Grade { Id = 14, StudentId = 4, DisciplineId = 3, Value = 2, Date = DateTime.Today.AddDays(-9),  Type = "Лабораторная",
                        IsDeleted = true, DeletedAt = DateTime.Today.AddDays(-1) },
        };

        /// <summary>
        /// Вход без проверки: пароль не важен, при неизвестном логине — первый пользователь выбранной роли.
        /// Если логин принадлежит другой роли, возвращает null.
        /// </summary>
        public static User? Authenticate(string login, UserRole role)
        {
            var user = Users.FirstOrDefault(u =>
                string.Equals(u.Login, login.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user == null) return Users.First(u => u.Role == role);
            return user.Role == role ? user : null;
        }

        public static IEnumerable<Grade> ActiveGrades => Grades.Where(g => !g.IsDeleted);

        public static string DisciplineName(int id) => Disciplines.FirstOrDefault(d => d.Id == id)?.Name ?? "?";
        public static string StudentName(int id) => Students.FirstOrDefault(s => s.Id == id)?.FullName ?? "?";
        public static string GroupName(int id) => Groups.FirstOrDefault(g => g.Id == id)?.Name ?? "?";

        public static int NextGradeId() => Grades.Count == 0 ? 1 : Grades.Max(g => g.Id) + 1;
    }
}
