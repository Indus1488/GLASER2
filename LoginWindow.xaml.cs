using System.Windows;

namespace GradeApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var isTeacher = TeacherRadio.IsChecked == true;
            var user = SampleData.Authenticate(LoginBox.Text, isTeacher ? UserRole.Teacher : UserRole.Student);

            if (user == null)
            {
                MessageBox.Show(
                    isTeacher ? "Этот логин не относится к преподавателям." : "Этот логин не относится к студентам.",
                    "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Session.CurrentUser = user;

            Window next = user.Role == UserRole.Teacher
                ? new TeacherWindow()
                : new StudentWindow();

            next.Show();
            Close();
        }
    }
}
