using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GradeApp
{
    /// <summary>Строка для таблиц с оценками (хранит ссылку на исходную запись).</summary>
    public class GradeRow
    {
        public GradeRow(Grade grade)
        {
            Source = grade;
        }

        public Grade Source { get; }
        public string Student => SampleData.StudentName(Source.StudentId);
        public string Disc => SampleData.DisciplineName(Source.DisciplineId);
        public DateTime Date => Source.Date;
        public string Type => Source.Type;
        public int Value => Source.Value;
        public DateTime? DeletedAt => Source.DeletedAt;
    }

    public partial class TeacherWindow : Window
    {
        private readonly Teacher? _teacher;

        public TeacherWindow()
        {
            InitializeComponent();

            _teacher = Session.CurrentTeacher;
            if (_teacher == null)
            {
                MessageBox.Show("Нет авторизованного преподавателя.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Loaded += (_, _) => Logout_Click(this, new RoutedEventArgs());
                return;
            }

            HeaderText.Text = "Преподаватель: " + _teacher.FullName;
            TypeCombo.ItemsSource = SampleData.GradeTypes;

            DisciplineList.ItemsSource = MyDisciplines();
            StudentViewCombo.ItemsSource = MyStudents();
            GroupViewCombo.ItemsSource = MyGroups();

            if (DisciplineList.Items.Count > 0) DisciplineList.SelectedIndex = 0;
            if (StudentViewCombo.Items.Count > 0) StudentViewCombo.SelectedIndex = 0;
            if (GroupViewCombo.Items.Count > 0) GroupViewCombo.SelectedIndex = 0;

            ResetEditor();
            RefreshDeleted();
        }

        // ---------- Данные, доступные текущему преподавателю ----------

        private IEnumerable<TeacherAssignment> MyAssignments =>
            SampleData.Assignments.Where(a => a.TeacherId == _teacher!.Id);

        /// <summary>Дисциплины, которые ведёт авторизованный преподаватель.</summary>
        private List<Discipline> MyDisciplines()
        {
            var ids = MyAssignments.Select(a => a.DisciplineId).Distinct().ToList();
            return SampleData.Disciplines.Where(d => ids.Contains(d.Id)).ToList();
        }

        /// <summary>Группы, в которых работает преподаватель.</summary>
        private List<Group> MyGroups()
        {
            var ids = MyAssignments.Select(a => a.GroupId).Distinct().ToList();
            return SampleData.Groups.Where(g => ids.Contains(g.Id)).ToList();
        }

        /// <summary>Группы, в которых преподаватель ведёт выбранную дисциплину.</summary>
        private List<Group> GroupsForDiscipline(int disciplineId)
        {
            var ids = MyAssignments.Where(a => a.DisciplineId == disciplineId)
                                   .Select(a => a.GroupId).Distinct().ToList();
            return SampleData.Groups.Where(g => ids.Contains(g.Id)).ToList();
        }

        private List<Student> MyStudents()
        {
            var groupIds = MyGroups().Select(g => g.Id).ToList();
            return SampleData.Students.Where(s => groupIds.Contains(s.GroupId))
                                      .OrderBy(s => s.FullName).ToList();
        }

        // ---------- Вкладка «Выставление оценок» ----------

        private void DisciplineList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DisciplineList.SelectedItem is Discipline d)
            {
                GroupCombo.ItemsSource = GroupsForDiscipline(d.Id);
                if (GroupCombo.Items.Count > 0) GroupCombo.SelectedIndex = 0;
            }
            RefreshGrades();
        }

        private void GroupCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupCombo.SelectedItem is Group g)
            {
                StudentCombo.ItemsSource = SampleData.Students
                    .Where(s => s.GroupId == g.Id)
                    .OrderBy(s => s.FullName)
                    .ToList();
                if (StudentCombo.Items.Count > 0) StudentCombo.SelectedIndex = 0;
            }
            else
            {
                StudentCombo.ItemsSource = null;
            }
            RefreshGrades();
        }

        private void StudentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshGrades();
        }

        private void RefreshGrades()
        {
            if (DisciplineList.SelectedItem is Discipline d &&
                StudentCombo.SelectedItem is Student st)
            {
                GradesGrid.ItemsSource = SampleData.ActiveGrades
                    .Where(g => g.StudentId == st.Id && g.DisciplineId == d.Id)
                    .OrderBy(g => g.Date)
                    .Select(g => new GradeRow(g))
                    .ToList();
            }
            else
            {
                GradesGrid.ItemsSource = null;
            }
            ResetEditor();
        }

        private void GradesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GradesGrid.SelectedItem is GradeRow row)
            {
                TypeCombo.SelectedItem = SampleData.GradeTypes.Contains(row.Type) ? row.Type : null;
                DateBox.SelectedDate = row.Date;
                GradeBox.Text = row.Value.ToString();
                ModeText.Text = $"Изменение оценки «{row.Value}» ({row.Type}, {row.Date:dd.MM.yyyy}). " +
                                "Очистите поле «Оценка» и нажмите «Сохранить», чтобы удалить запись.";
            }
            else
            {
                ResetEditor();
            }
        }

        private void NewGrade_Click(object sender, RoutedEventArgs e)
        {
            GradesGrid.SelectedItem = null;
            ResetEditor();
            GradeBox.Focus();
        }

        private void ResetEditor()
        {
            if (GradesGrid.SelectedItem != null) return;

            TypeCombo.SelectedIndex = 0;
            DateBox.SelectedDate = DateTime.Today;
            GradeBox.Text = "";
            ModeText.Text = "Новая оценка. Чтобы изменить или удалить существующую, выберите её в таблице.";
        }

        private void SaveGrade_Click(object sender, RoutedEventArgs e)
        {
            if (DisciplineList.SelectedItem is not Discipline discipline)
            {
                Warn("Выберите дисциплину.");
                return;
            }
            if (StudentCombo.SelectedItem is not Student student)
            {
                Warn("Выберите студента.");
                return;
            }

            var selected = (GradesGrid.SelectedItem as GradeRow)?.Source;
            var text = GradeBox.Text.Trim();

            // Пустое поле — удаление записи (мягкое: запись помечается удалённой).
            if (text.Length == 0)
            {
                if (selected == null)
                {
                    Warn("Введите оценку от 2 до 5 или выберите в таблице оценку, которую нужно удалить.");
                    return;
                }

                var answer = MessageBox.Show(
                    $"Удалить оценку «{selected.Value}» ({selected.Type}, {selected.Date:dd.MM.yyyy}) " +
                    $"у студента {student.FullName}?\nЕё можно будет восстановить на вкладке «Удалённые оценки».",
                    "Удаление оценки", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer != MessageBoxResult.Yes) return;

                selected.IsDeleted = true;
                selected.DeletedAt = DateTime.Now;
                RefreshAll();
                return;
            }

            if (!int.TryParse(text, out var value) || value < 2 || value > 5)
            {
                Warn("Оценка должна быть целым числом от 2 до 5.");
                return;
            }
            if (TypeCombo.SelectedItem is not string type)
            {
                Warn("Выберите тип работы.");
                return;
            }
            if (DateBox.SelectedDate is not DateTime date)
            {
                Warn("Укажите дату.");
                return;
            }
            if (date.Date > DateTime.Today)
            {
                Warn("Дата оценки не может быть в будущем.");
                return;
            }

            if (selected != null)
            {
                selected.Value = value;
                selected.Type = type;
                selected.Date = date.Date;
            }
            else
            {
                SampleData.Grades.Add(new Grade
                {
                    Id = SampleData.NextGradeId(),
                    StudentId = student.Id,
                    DisciplineId = discipline.Id,
                    Value = value,
                    Type = type,
                    Date = date.Date
                });
            }

            RefreshAll();
            ModeText.Text = selected != null ? "Оценка изменена." : "Оценка добавлена.";
        }

        private static void Warn(string message) =>
            MessageBox.Show(message, "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void RefreshAll()
        {
            GradesGrid.SelectedItem = null;
            RefreshGrades();
            RefreshStudentView();
            RefreshGroupView();
            RefreshDeleted();
        }

        // ---------- Вкладка «Оценки студента» ----------

        private void StudentViewCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            RefreshStudentView();

        private void RefreshStudentView()
        {
            StudentAllGradesGrid.ItemsSource = StudentViewCombo.SelectedItem is Student st
                ? SampleData.ActiveGrades
                    .Where(g => g.StudentId == st.Id)
                    .OrderBy(g => SampleData.DisciplineName(g.DisciplineId)).ThenBy(g => g.Date)
                    .Select(g => new GradeRow(g))
                    .ToList()
                : null;
        }

        // ---------- Вкладка «Оценки группы» ----------

        private void GroupViewCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            RefreshGroupView();

        private void RefreshGroupView()
        {
            if (GroupViewCombo.SelectedItem is Group grp)
            {
                var ids = SampleData.Students.Where(s => s.GroupId == grp.Id).Select(s => s.Id).ToList();
                GroupGradesGrid.ItemsSource = SampleData.ActiveGrades
                    .Where(g => ids.Contains(g.StudentId))
                    .Select(g => new GradeRow(g))
                    .OrderBy(r => r.Student).ThenBy(r => r.Disc).ThenBy(r => r.Date)
                    .ToList();
            }
            else
            {
                GroupGradesGrid.ItemsSource = null;
            }
        }

        // ---------- Вкладка «Удалённые оценки» ----------

        private void RefreshDeleted()
        {
            var disciplineIds = MyDisciplines().Select(d => d.Id).ToList();
            var studentIds = MyStudents().Select(s => s.Id).ToList();

            DeletedGradesGrid.ItemsSource = SampleData.Grades
                .Where(g => g.IsDeleted &&
                            disciplineIds.Contains(g.DisciplineId) &&
                            studentIds.Contains(g.StudentId))
                .OrderByDescending(g => g.DeletedAt)
                .Select(g => new GradeRow(g))
                .ToList();
        }

        private void RestoreGrade_Click(object sender, RoutedEventArgs e)
        {
            if (DeletedGradesGrid.SelectedItem is not GradeRow row)
            {
                Warn("Выберите оценку для восстановления.");
                return;
            }

            row.Source.IsDeleted = false;
            row.Source.DeletedAt = null;
            RefreshAll();
            MessageBox.Show("Оценка восстановлена.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;
            new LoginWindow().Show();
            Close();
        }
    }
}
