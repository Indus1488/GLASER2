using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GradeApp
{
    public partial class StudentWindow : Window
    {
        private const string AllDisciplines = "Все дисциплины";
        private readonly Student? _student;

        public StudentWindow()
        {
            InitializeComponent();

            _student = Session.CurrentStudent;
            if (_student == null)
            {
                MessageBox.Show("Нет авторизованного студента.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Loaded += (_, _) => Logout_Click(this, new RoutedEventArgs());
                return;
            }

            HeaderText.Text = $"Студент: {_student.FullName} ({SampleData.GroupName(_student.GroupId)})";

            // Студент видит только свои оценки (удалённые не показываются).
            var disciplines = new List<string> { AllDisciplines };
            disciplines.AddRange(MyGrades()
                .Select(g => SampleData.DisciplineName(g.DisciplineId))
                .Distinct()
                .OrderBy(n => n));

            DisciplineFilter.ItemsSource = disciplines;
            DisciplineFilter.SelectedIndex = 0;
        }

        private IEnumerable<Grade> MyGrades() =>
            SampleData.ActiveGrades.Where(g => g.StudentId == _student!.Id);

        private void DisciplineFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var filter = DisciplineFilter.SelectedItem as string;

            var rows = MyGrades()
                .Select(g => new GradeRow(g))
                .Where(r => filter == null || filter == AllDisciplines || r.Disc == filter)
                .OrderBy(r => r.Disc).ThenBy(r => r.Date)
                .ToList();

            GradesGrid.ItemsSource = rows;
            AverageText.Text = rows.Count > 0
                ? $"Средний балл: {rows.Average(r => r.Value):0.00}"
                : "Оценок пока нет";
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;
            new LoginWindow().Show();
            Close();
        }
    }
}
