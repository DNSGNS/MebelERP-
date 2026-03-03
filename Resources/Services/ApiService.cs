using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyApp1;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    public string BaseUrl => _httpClient.BaseAddress?.ToString() ?? "null";
    public string? LastError { get; private set; }

    public ApiService()

    {
        string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://127.0.0.1"
            : "https://127.0.0.1";

        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

        // Выносим настройки в конструктор, чтобы не дублировать
        _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Projects/{projectId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API DELETE ERROR: {ex.Message}");
            return false;
        }
    }


    public async Task<WorkMans?> GetUserByUsernameAsync(string username)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/Projects/user/{username}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WorkMans>(_jsonOptions);
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API REFRESH EXCEPTION: {ex.Message}");
            return null;
        }
    }

    public async Task<WorkMans?> LoginAsync(string username, string password)
    {
        try
        {
            var loginData = new LoginRequestDto { Username = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/Projects/login", loginData, _jsonOptions);

            LastError = null;

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WorkMans>(_jsonOptions);
            }

            var content = await response.Content.ReadAsStringAsync();
            LastError = $"Статус: {response.StatusCode}, Ответ: {content}";
            return null;
        }
        catch (Exception ex)
        {
            LastError = $"Исключение: {ex.Message}";
            return null;
        }
    }


    // 1. GET: Получить абсолютно все проекты со всеми вложенными данными
    public async Task<List<Project>> GetAllProjectsAsync()
    {
        try
        {
            // Вызываем базовый GET метод (без фильтра по имени)
            var response = await _httpClient.GetAsync("api/Projects");

            if (response.IsSuccessStatusCode)
            {
                // Используем те же настройки сериализации, что и в вашем примере
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles // Критично для загрузки сложных связей
                };

                var projects = await response.Content.ReadFromJsonAsync<List<Project>>(options);
                return projects ?? new List<Project>();
            }

            return new List<Project>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ОШИБКА ЗАГРУЗКИ ВСЕХ ПРОЕКТОВ: {ex.Message}");
            return new List<Project>();
        }
    }

    public async Task<List<Project>> GetProjectsByInstallerAsync(Guid installerId)
    {
        try
        {
            // Вызываем новый эндпоинт
            var response = await _httpClient.GetAsync($"api/Projects/by-installer/{installerId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Project>>(_jsonOptions)
                       ?? new List<Project>();
            }

            System.Diagnostics.Debug.WriteLine($"Ошибка: {response.StatusCode}");
            return new List<Project>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API ERROR: {ex.Message}");
            return new List<Project>();
        }
    }

    public async Task<List<Project>> GetNewProjectsAsync()
    {
        try
        {
            // Вызываем наш новый специфический маршрут
            var response = await _httpClient.GetAsync("api/Projects/filter/new");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };

                var projects = await response.Content.ReadFromJsonAsync<List<Project>>(options);
                return projects ?? new List<Project>();
            }

            return new List<Project>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ОШИБКА ЗАГРУЗКИ НОВЫХ ПРОЕКТОВ: {ex.Message}");
            return new List<Project>();
        }
    }

    public async Task<List<Project>> GetProjectsByCreatorAsync(string creatorName)
    {
        try
        {
            // Экранируем имя (на случай пробелов или спецсимволов)
            string encodedCreator = Uri.EscapeDataString(creatorName);

            var response = await _httpClient.GetAsync($"api/Projects/by-creator/{encodedCreator}");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles // Важно для вложенных данных
                };

                return await response.Content.ReadFromJsonAsync<List<Project>>(options);
            }

            return new List<Project>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ОШИБКА ЗАГРУЗКИ ПРОЕКТОВ АВТОРА: {ex.Message}");
            return new List<Project>();
        }
    }

    // Метод для регистрации брака по конкретной задаче
    public async Task<(bool Success, string Message)> ReportScrapAsync(Guid taskId, int scrapCount)
    {
        try
        {
            // Отправляем количество брака (scrapCount) как тело запроса (JSON int)
            // URL должен совпадать с маршрутом в вашем контроллере
            var response = await _httpClient.PostAsJsonAsync($"api/Projects/tasks/{taskId}/report-scrap", scrapCount, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Брак успешно зафиксирован. Создана задача на переделку.");
            }

            // Если сервер вернул ошибку (например, 400 Bad Request), пытаемся прочитать текст ошибки
            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, !string.IsNullOrEmpty(errorContent) ? errorContent : "Ошибка сервера при фиксации брака");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API REPORT SCRAP EXCEPTION: {ex.Message}");
            return (false, "Не удалось связаться с сервером");
        }
    }


    /// <summary>
    /// Пытается обновить проект. Если его нет (404) — создает новый.
    /// </summary>
    public async Task<bool> SaveProjectToDbAsync(Project projectApi)
    {
        try
        {
            // 1. Пытаемся обновить (PUT)
            // Мы предполагаем, что проект уже может быть в базе
            var response = await _httpClient.PutAsJsonAsync($"api/Projects/{projectApi.Id}", projectApi, _jsonOptions);

            // 2. Если обновление прошло успешно (200 или 204)
            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("Project updated successfully.");
                return true;
            }

            // 3. Если сервер ответил "404 Not Found", значит проекта еще нет в базе
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                System.Diagnostics.Debug.WriteLine("Project not found. Creating new...");

                // Пытаемся создать (POST)
                var createResponse = await _httpClient.PostAsJsonAsync("api/Projects", projectApi, _jsonOptions);

                if (createResponse.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Project created successfully.");
                    return true;
                }

                var errorContent = await createResponse.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"CREATE ERROR: {errorContent}");
            }
            else
            {
                // Если возникла другая ошибка (например, 400 или 500)
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"SERVER ERROR ({response.StatusCode}): {errorContent}");
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API UPSERT EXCEPTION: {ex.Message}");
            return false;
        }
    }


    // --- МЕТОДЫ ДЛЯ КАТЕГОРИЙ ---

    public async Task<List<ExpenseCategory>> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Projects/expense-categories"); // Этот метод мы делали ранее
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ExpenseCategory>>(_jsonOptions);
            }
            return new List<ExpenseCategory>();
        }
        catch { return new List<ExpenseCategory>(); }
    }

    public async Task<ExpenseCategory?> CreateCategoryAsync(string name)
    {
        try
        {
            var newCat = new ExpenseCategory { Name = name };
            var response = await _httpClient.PostAsJsonAsync("api/Projects/categories", newCat, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ExpenseCategory>(_jsonOptions);
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<bool> UpdateCategoryAsync(ExpenseCategory category)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Projects/categories/{category.Id}", category, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Projects/categories/{id}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }



    // 1. Получение данных для страницы расходов (2 таблицы сразу)
    public async Task<ExpensesDataDto?> GetExpensesInitialDataAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Projects/expenses-init");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ExpensesDataDto>(_jsonOptions);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading expenses: {ex.Message}");
            return null;
        }
    }



    public async Task<SalaryReportResponseDto?> GetSalaryReportAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Projects/salary-report");

            if (response.IsSuccessStatusCode)
            {
                // --- ИСПРАВЛЕНИЕ ОШИБКИ 1 ---
                // Мы читаем JSON сразу в клиентский класс SalaryReportResponse.
                // Благодаря совпадению имен свойств, JSON корректно разложится по полочкам.
                return await response.Content.ReadFromJsonAsync<SalaryReportResponseDto>(_jsonOptions);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки отчета: {ex.Message}");
            return null;
        }
    }


    // Создание
    public async Task<bool> CreateSalaryRecordAsync(SalaryReportItem item)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Projects/work", item, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка создания: {ex.Message}");
            return false;
        }
    }

    // Обновление по ID
    public async Task<bool> UpdateSalaryRecordAsync(SalaryReportItem item)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Projects/work/{item.Id}", item, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка обновления: {ex.Message}");
            return false;
        }
    }

    // Удаление по ID
    public async Task<bool> DeleteSalaryRecordAsync(Guid recordId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Projects/work/{recordId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка удаления: {ex.Message}");
            return false;
        }
    }



    /// <summary>
    /// Получает данные для планирования установок: проекты, всех рабочих и распределенных рабочих.
    /// </summary>
    public async Task<InstallationInfoResponse?> GetInstallationPlanningDataAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Projects/installation-info");

            if (response.IsSuccessStatusCode)
            {
                // Используем общие настройки _jsonOptions
                return await response.Content.ReadFromJsonAsync<InstallationInfoResponse>(_jsonOptions);
            }

            System.Diagnostics.Debug.WriteLine($"SERVER ERROR ({response.StatusCode})");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ОШИБКА ЗАГРУЗКИ ДАННЫХ УСТАНОВКИ: {ex.Message}");
            return null;
        }
    }



    public async Task<List<SalaryMonthReportDto>> GetMonthlySalariesAsync(int year, int month)
    {
        try
        {
            // Формируем URL с параметрами запроса
            var url = $"api/Projects/monthly?year={year}&month={month}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Используем стандартные настройки _jsonOptions (camelCase), 
                // так как в контроллере свойства DTO будут преобразованы в camelCase автоматически
                return await response.Content.ReadFromJsonAsync<List<SalaryMonthReportDto>>(_jsonOptions)
                       ?? new List<SalaryMonthReportDto>();
            }

            System.Diagnostics.Debug.WriteLine($"Ошибка сервера при получении зарплат: {response.StatusCode}");
            return new List<SalaryMonthReportDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка API GetMonthlySalaries: {ex.Message}");
            return new List<SalaryMonthReportDto>();
        }
    }

    public async Task<bool> SaveInstallationPlanAsync(ProjectManageData project)
    {
        try
        {
            var dto = ProjectMapper.MapToUpdateManagePage(project);
            var response = await _httpClient.PostAsJsonAsync("api/Projects/update-plan", dto, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving plan: {ex.Message}");
            return false;
        }
    }

    //old vers
    //public async Task<bool> UpdateProjectStatusAsync(Guid projectId, int status)
    //{
    //    try
    //    {
    //        var response = await _httpClient.PatchAsJsonAsync($"api/Projects/{projectId}/status", status, _jsonOptions);

    //        if (!response.IsSuccessStatusCode)
    //        {
    //            var error = await response.Content.ReadAsStringAsync();
    //            System.Diagnostics.Debug.WriteLine($"Ошибка обновления статуса: {error}");
    //        }

    //        return response.IsSuccessStatusCode;
    //    }
    //    catch (Exception ex)
    //    {
    //        System.Diagnostics.Debug.WriteLine($"API STATUS UPDATE EXCEPTION: {ex.Message}");
    //        return false;
    //    }
    //}



    public async Task<Expense> SaveExpenseAsync(Expense expense)
    {
        try
        {
            // Отправляем POST запрос на "expenses"
            // Используем PostAsJsonAsync (стандартный метод расширения HttpClient)
            var response = await _httpClient.PostAsJsonAsync("api/Projects/expenses", expense);

            if (response.IsSuccessStatusCode)
            {
                // Возвращаем сохраненный объект (с обновленным ID, если это была новая запись)
                return await response.Content.ReadFromJsonAsync<Expense>();
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving expense: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Projects/expenses/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting expense: {ex.Message}");
            return false;
        }
    }


    public async Task<List<WorkMans>> GetUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Projects/users");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<WorkMans>>(_jsonOptions);
            }
            return new List<WorkMans>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting users: {ex.Message}");
            return new List<WorkMans>();
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Projects/users/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting user: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> CreateUserAsync(WorkMans user)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Projects/users", user, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Ошибка создания пользователя: {error}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API CREATE USER EXCEPTION: {ex.Message}");
            return false;
        }
    }


    public async Task<bool> UpdateUserAsync(WorkMans user)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Projects/users/{user.Id}", user, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления пользователя: {error}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API UPDATE USER EXCEPTION: {ex.Message}");
            return false;
        }
    }


    // Метод для создания производственных заданий в БД
    public async Task<bool> GenerateProductionTasksAsync(Guid projectId)
    {
        try
        {
            // Отправляем POST запрос. Тело пустое (null), так как ID передается в URL
            var response = await _httpClient.PostAsync($"api/Projects/{projectId}/generate-tasks", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Ошибка создания задач: {error}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API GENERATE TASKS EXCEPTION: {ex.Message}");
            return false;
        }
    }


    public async Task<List<ProductionTask>> GetProductionTasksForWorkerAsync(string workerName)
    {
        try
        {
            // Экранируем имя на случай спецсимволов или пробелов
            string encodedName = Uri.EscapeDataString(workerName);
            var response = await _httpClient.GetAsync($"api/Projects/production-tasks/for-worker?workerName={encodedName}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductionTask>>(_jsonOptions)
                       ?? new List<ProductionTask>();
            }

            System.Diagnostics.Debug.WriteLine($"Ошибка получения задач: {response.StatusCode}");
            return new List<ProductionTask>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API GET TASKS EXCEPTION: {ex.Message}");
            return new List<ProductionTask>();
        }
    }


    // Метод для изменения "Взято в работу"
    public async Task<(bool Success, string Message)> UpdateTaskTakeStatusWithResultAsync(Guid taskId, bool isTaken, string workerName)
    {
        try
        {
            var content = new TakeTaskRequest { IsTaken = isTaken, WorkerName = workerName };
            var response = await _httpClient.PatchAsJsonAsync($"api/Projects/{taskId}/task-take", content, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return (true, string.Empty);
            }

            // Читаем сообщение об ошибке от сервера (например, "Задача уже занята...")
            var errorText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return (false, errorText); // Тут будет текст из Conflict() на бэкенде
            }

            return (false, string.IsNullOrEmpty(errorText) ? "Ошибка сервера" : errorText);
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка связи: {ex.Message}");
        }
    }

    // Получить все завершенные проекты
    public async Task<List<CompletedProject>?> GetCompletedProjectsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Projects/completed");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<CompletedProject>>(_jsonOptions);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API ERROR (GetCompleted): {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteCompletedProjectsAsync(List<Guid> ids)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Projects/completed/delete-multiple", ids);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API ERROR (DeleteMultiple): {ex.Message}");
            return false;
        }
    }

    // Отправить проект в архив 
    public async Task<bool> CompleteProjectAsync(Guid projectId, List<string> installers, List<DateTime> installDates)
    {
        try
        {
            var requestDto = new CompleteProjectRequestDto
            {
                SelectedInstallers = installers ?? new List<string>(),
                InstallDates = installDates ?? new List<DateTime>()
            };

            var response = await _httpClient.PostAsJsonAsync($"api/Projects/{projectId}/complete", requestDto, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Ошибка архивации: {error}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API EXCEPTION (Complete): {ex.Message}");
            return false;
        }
    }
    // Метод для изменения статуса (Pending/Ready)
    public async Task<bool> UpdateTaskStatusAsync(Guid taskId, ProductionTaskStatus newStatus, string workerName)
    {
        try
        {
            var content = new UpdateStatusRequest { NewStatus = newStatus, WorkerName = workerName };
            var response = await _httpClient.PatchAsJsonAsync($"api/Projects/{taskId}/task-status", content, _jsonOptions);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API UPDATE STATUS EXCEPTION: {ex.Message}");
            return false;
        }
    }

    public async Task<int?> UpdateWarehouseStatusAsync(Guid projectId, bool material, bool furniture)
    {
        try
        {
            var dto = new { IsMaterialReady = material, IsFurnitureReady = furniture };
            var response = await _httpClient.PostAsJsonAsync(
    $"api/Projects/{projectId}/warehouse",
    dto,
    _jsonOptions);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content
                .ReadFromJsonAsync<WarehouseStatusResponseDto>(_jsonOptions);

            return result?.ProjectStatus;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API WAREHOUSE ERROR: {ex.Message}");
            return null;
        }
    }


    // Метод для получения финансового отчета за 3 года
    public async Task<List<MonthlyReportItem>> GetFinancialReportAsync()
    {
        try
        {
            string url = $"api/Projects/financial-report-all";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Используем наш новый класс MonthlyReportItem
                return await response.Content.ReadFromJsonAsync<List<MonthlyReportItem>>(_jsonOptions)
                       ?? new List<MonthlyReportItem>();
            }
            return new List<MonthlyReportItem>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            return new List<MonthlyReportItem>();
        }
    }

}