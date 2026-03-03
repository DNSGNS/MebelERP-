using System.Text.Json;

namespace MyApp1;

public static class UserSessionService
{
    // Путь к файлу внутри защищенной папки приложения
    private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, "user_session.json");

    // 1. Метод сохранения данных
    public static void SaveUser(WorkMans user)
    {
        try
        {
            var json = JsonSerializer.Serialize(user);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения сессии: {ex.Message}");
        }
    }

    // 2. Метод загрузки данных (возвращает null, если файла нет)
    public static WorkMans? LoadUser()
    {
        if (!File.Exists(FilePath))
            return null;

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<WorkMans>(json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки сессии: {ex.Message}");
            return null;
        }
    }

    // 3. Метод удаления данных (Выход)
    public static void ClearSession()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }

    // 4. Проверка, авторизован ли пользователь
    public static bool IsLoggedIn => File.Exists(FilePath);
}