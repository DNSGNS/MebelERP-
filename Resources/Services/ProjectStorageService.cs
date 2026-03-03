using System.Collections.ObjectModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace MyApp1;

public static class ProjectStorageService
{
    // Имя файла во внутренней памяти приложения
    private static readonly string FileName = "projects_db.json";
    private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, FileName);

    public static async Task SaveProjectsAsync(ObservableCollection<ProjectData> projects)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string jsonString = JsonSerializer.Serialize(projects, options);
            await File.WriteAllTextAsync(FilePath, jsonString);
        }
        catch (Exception ex)
        {
            // Выводим ошибку в консоль, чтобы вы видели, на чем именно ломается
            System.Diagnostics.Debug.WriteLine($"ОШИБКА СОХРАНЕНИЯ: {ex}");
        }
    }

    public static async Task<ObservableCollection<ProjectData>> LoadProjectsAsync()
    {
        try
        {
            if (!File.Exists(FilePath)) return new();

            string jsonString = await File.ReadAllTextAsync(FilePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles, // Игнорируем зацикливания
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var projects = JsonSerializer.Deserialize<ObservableCollection<ProjectData>>(jsonString, options);
            return projects ?? new();
        }
        catch (Exception ex)
        {
            // ВАЖНО: это выведет реальную причину ошибки в консоль отладки
            System.Diagnostics.Debug.WriteLine($"ОШИБКА ДЕСЕРИАЛИЗАЦИИ: {ex.Message}");
            return null; // Возвращаем null, чтобы отличить пустой файл от ошибки
        }
    }
}