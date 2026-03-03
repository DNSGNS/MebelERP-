using System.Collections.ObjectModel;

namespace MyApp1
{
    public partial class App : Application
    {
        public static ObservableCollection<ProjectData> AllProjects { get; set; } = new();
        public static WorkMans CurrentUser { get; set; }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window();
            LoadApp(window);
            return window;
        }

        public void LoadApp(Window window)
        {
            var savedUser = UserSessionService.LoadUser();

            if (savedUser != null)
            {
                CurrentUser = savedUser;
                window.Page = new AppShell(); // Главное меню
            }
            else
            {
                window.Page = new NavigationPage(new AuthorizationPage());
            }
        }
    }
}