namespace MyApp1
{
    public partial class MainPage : ContentPage
    {
        public WorkMans CurrentUser => App.CurrentUser;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            OnPropertyChanged(nameof(CurrentUser));
            ApplyAccess();
        }

        private void ApplyAccess()
        {
            var a = App.CurrentUser?.Access;

            if (a == null)
            {
                Navigation.PushAsync(new ProfilePage());
                return;
            }

            //if (a == null) return;

            // --- Оперативная работа ---
            var operationalButtons = new List<(string text, EventHandler handler)>();
            if (a.CanAccessSurveyor) operationalButtons.Add(("📐 ЗАМЕРЫ", OnSurveyorClicked));
            
            if (a.CanAccessWarehouse) operationalButtons.Add(("📦 СКЛАД", OnWarehouseClicked));
            if (a.CanAccessInstaller) operationalButtons.Add(("🛠 УСТАНОВКА", OnInstallerClicked));
            if (a.CanAccessWorkshop) operationalButtons.Add(("🏭 ЦЕХ", OnWorkshopClicked));
            if (a.CanAccessLayout) operationalButtons.Add(("📑 РАСКЛАДКА", OnLayoutClicked));
            if (a.CanAccessCutting) operationalButtons.Add(("✂️ РАСКРОЙ", OnCuttingClicked));
            if (a.CanAccessCalendar) operationalButtons.Add(("📅 КАЛЕНДАРЬ", OnCalendarClicked));
            if (a.CanAccessPersonalJournal) operationalButtons.Add(("📝 ЖУРНАЛ РАБОТ", OnMyJournalClicked));
            BuildButtonGrid(ContainerOperational, operationalButtons, "#2B3C51");
            SectionOperational.IsVisible = operationalButtons.Count > 0;

            // --- Управление проектами ---
            var projectButtons = new List<(string text, EventHandler handler)>();
            if (a.CanAccessActiveProjects) projectButtons.Add(("🚀 АКТИВНЫЕ", OnActiveProjectsClicked));
            if (a.CanAccessArchiveProjects) projectButtons.Add(("📁 АРХИВ", OnArchiveProjectsClicked));
            BuildButtonGrid(ContainerProjects, projectButtons, "#6750A4");
            SectionProjects.IsVisible = projectButtons.Count > 0;



            // --- Отчеты и производство ---
            var reportButtons = new List<(string text, EventHandler handler)>();
            if (a.CanAccessMonthlyReport) reportButtons.Add(("📊 ИТОГОВЫЙ ОТЧЕТ", OnMonthlyReportClicked));
            if (a.CanAccessSalaries) reportButtons.Add(("💰 ЗАРПЛАТЫ", OnSalaryClicked));
            if (a.CanAccessAllJournals) reportButtons.Add(("📚 ВСЕ ЖУРНАЛЫ РАБОТ", OnAllJournalsClicked));
            if (a.CanAccessExpenses) reportButtons.Add(("📉 ЖУРНАЛ РАСХОДОВ", OnExpensesClicked));

            BuildButtonGrid(ContainerReports, reportButtons, "#2B3C51");
            SectionReports.IsVisible = reportButtons.Count > 0;

            // --- Система ---
            var systemButtons = new List<(string text, EventHandler handler)>();
            if (a.CanAccessProfile) systemButtons.Add(("👤 ПРОФИЛЬ", OnProfileClicked));
            if (a.CanAccessUsersList) systemButtons.Add(("👥 ПОЛЬЗОВАТЕЛИ", OnUsersListClicked));
            BuildButtonGrid(ContainerSystem, systemButtons, "#607D8B");
            SectionSystem.IsVisible = systemButtons.Count > 0;
        }

        private void BuildButtonGrid(VerticalStackLayout container, List<(string text, EventHandler handler)> buttons, string color)
        {
            container.Children.Clear();

            for (int i = 0; i < buttons.Count; i += 2)
            {
                bool hasPair = i + 1 < buttons.Count;

                var grid = new Grid
                {
                    ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                    Margin = new Thickness(0, 3, 0, 0)
                };

                var btn1 = CreateButton(buttons[i].text, color, buttons[i].handler);
                Grid.SetColumn(btn1, 0);
                if (!hasPair)
                    Grid.SetColumnSpan(btn1, 2);
                grid.Children.Add(btn1);

                if (hasPair)
                {
                    var btn2 = CreateButton(buttons[i + 1].text, color, buttons[i + 1].handler);
                    Grid.SetColumn(btn2, 1);
                    grid.Children.Add(btn2);
                }

                container.Children.Add(grid);
            }
        }

        private Button CreateButton(string text, string color, EventHandler handler)
        {
            var btn = new Button
            {
                Text = text,
                BackgroundColor = Color.FromArgb(color),
                TextColor = Colors.White,
                HeightRequest = 52,
                CornerRadius = 10,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(3)
            };
            btn.Clicked += handler;
            return btn;
        }

        // --- БЛОК 1: ОПЕРАТИВНАЯ РАБОТА ---
        private async void OnSurveyorClicked(object sender, EventArgs e)
        {
            // Используем вашу логику: создаем пустой объект для расчетов
            var newOrder = new ObjectData();
            await Navigation.PushAsync(new MainCalculationPage());
        }

        private async void OnSalaryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SalaryMonthlyPage());
        }

        private async void OnWarehouseClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainCalculationWarehousePage());
        }

        private async void OnInstallerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainCalculationInstallerPage());
        }

        private async void OnWorkshopClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ObjectWorkshopEditorPage());
        }

        private async void OnCalendarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManageCalendarPage());
        }


        // --- БЛОК 2: УПРАВЛЕНИЕ ПРОЕКТАМИ ---
        private async void OnActiveProjectsClicked(object sender, EventArgs e)
        {
            // Страница менеджера 1
            await Navigation.PushAsync(new ManageMainPage());
        }

        private async void OnArchiveProjectsClicked(object sender, EventArgs e)
        {
            // Исправлено: Страница менеджера 2 (Архив)
            await Navigation.PushAsync(new CompletedOrdersPage());
        }


        // --- БЛОК 3: ЖУРНАЛЫ ---
        private async void OnMyJournalClicked(object sender, EventArgs e)
        {
            // Передаем имя текущего пользователя (OnNewClick)
            await Navigation.PushAsync(new SalaryReportPage(App.CurrentUser.Name));

        }

        private async void OnAllJournalsClicked(object sender, EventArgs e)
        {
            // Общий журнал без параметров (OnNwClick)
            await Navigation.PushAsync(new SalaryReportPage());
        }

        private async void OnExpensesClicked(object sender, EventArgs e)
        {
            // Журнал расходов (OnNewCli)
            await Navigation.PushAsync(new ExpensesPage());
        }


        // --- БЛОК 4: ПРОИЗВОДСТВО ---
        private async void OnMonthlyReportClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MonthlyReportPage());
        }

        private async void OnLayoutClicked(object sender, EventArgs e)
        {
            // Исправлено: Передаем необходимые аргументы (OnClickedLay)
            var newObj = new ObjectData();
            var newLay = new CuttingData();
            await Navigation.PushAsync(new LayoutPage(newObj, newLay));
        }

        private async void OnCuttingClicked(object sender, EventArgs e)
        {
            // Исправлено: Передаем необходимые аргументы (OnClickedCutting)
            var newObj = new ObjectData();
            var newLay = new CuttingData();
            await Navigation.PushAsync(new CuttingPage(newObj, newLay));
        }


        // --- БЛОК 5: СИСТЕМА ---
        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage());
        }

        private async void OnUsersListClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UsersListPage());
        }
    }
}