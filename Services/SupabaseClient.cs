using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PonyvilleSchool2._0.Models;
using PonyvilleSchool2._0.Models.Administrator;
using RestSharp;
using System.Windows;
using System.Xml.Linq;

namespace PonyvilleSchool2._0.Services
{
    public class SupabaseClient
    {
        private readonly RestClient client;

        private static readonly string BaseURL = "https://phntjkxxmszbsvfnlvnh.supabase.co";
        private static readonly string APIkey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBobnRqa3h4bXN6YnN2Zm5sdm5oIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjM2NjgzMDAsImV4cCI6MjA3OTI0NDMwMH0.TGM-bcU1-5Xwztdj0zL_gahoTI-XanCIlvz032fAlu8";
        public enum AuthResult
        {
            Failed,
            Success,
            NeedVerification

        } //Результаты авторизации
        public SupabaseClient()
        {
            client = new RestClient(BaseURL); //Инициализация клиента RestRequest
        } //Инициализация клиента SupabaseClient

        //Основные части запроса//
        private static RestRequest CreateRequest(string endpoint, Method method = Method.Post) //Структура запроса к Supabase
        {
            var request = new RestRequest(endpoint, method);
            request.AddHeader("apikey", APIkey);
            request.AddHeader("Authorization", $"Bearer {APIkey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Prefer", "return=minimal");
            return request;
        }
        protected async Task<RestResponse> ExecuteRpc(string rpcName, object? body = null) //Выполнение функций
        {
            try
            {
                var request = CreateRequest($"/rest/v1/rpc/{rpcName}");

                if (body != null)
                    request.AddJsonBody(body);

                var response = await client.ExecuteAsync(request);

                Logger.Write(
                    Logger.LogLevel.Info,
                    "SupabaseClient",
                    rpcName,
                    $"SUCCESS | {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                Logger.Write(
                    Logger.LogLevel.Error,
                    "SupabaseClient",
                    rpcName,
                    $"FAILURE | {ex.Message}");
                return null;
            }
        }
        private async Task<T?> ExecuteRpcAndDeserialize<T>(string rpcName, object? body = null) //Десереализация объекта
        {
            try
            {
                var response = await ExecuteRpc(rpcName, body);

                if (!response.IsSuccessful)
                {
                   Logger.Write(
                   Logger.LogLevel.Error,
                   "SupabaseClient",
                   rpcName,
                   $"FAILURE | {response.StatusCode}");
                   return default;
                }
  
                Logger.Write(
                   Logger.LogLevel.Info,
                   "SupabaseClient",
                   rpcName,
                   $"SUCCESS | {response.StatusCode}");

                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception ex)
            {
                Logger.Write(
                   Logger.LogLevel.Error,
                   "SupabaseClient",
                   rpcName,
                   $"FAILURE | {ex.Message}");
                return default;
            }
        }

        //Запросы аутентификации//
        public async Task<AuthResult> AuthenticateUser(string p_login, string p_password) //Авторизация
        {
            var response = await ExecuteRpc("auth_user", new 
            { 
                p_login,
                p_password 
            });

            var rawJson = JObject.Parse(response.Content);

            if (rawJson["id"] == null ||
                rawJson["id"]?.Type == JTokenType.Null)
            {
                return AuthResult.Failed;
            }

            if (!(rawJson["verified"]?.Value<bool>() ?? false))
            {
                AppState.Instance.CurrentUser.name =
                    rawJson["name"]?.ToString();

                return AuthResult.NeedVerification;
            }

            AppState.Instance.CurrentUser =
                rawJson.ToObject<User>();

            return AuthResult.Success;
        }
        public async Task<bool> RegisterUser(string p_login, string p_password, string p_name, string p_token) //Регистрация
        {
            var response = await ExecuteRpc("register_user", new
            {
                p_login,
                p_password,
                p_name,
                p_token
            });
            if (!response.IsSuccessful)
                return false;

            var raw = JObject.Parse(response.Content);

            return raw["success"]?.Value<bool>() ?? false;
        }
        public async Task<bool> CheckToken(string p_token) //Проверка действительности токена
        {
            var response = await ExecuteRpc("check_token", new
            {
                p_token
            });
            try
            {
                var rawJson = JObject.Parse(response.Content);

                if (rawJson["id"] == null || rawJson["id"].Type == JTokenType.Null)
                {
                    return false;
                }
                else
                {
                    AppState.Instance.CurrentUser = rawJson.ToObject<User>();
                    return true;
                }
            }
            catch
            {
                return false;
            }        
        }
        public async Task<bool> VerifyToken(string p_login, string p_token, string p_type)
        {
            var response = await ExecuteRpc("verify_token", new
            {
                p_login,
                p_token,
                p_type
            });
            if (!response.IsSuccessful)
                return false;

            var raw = JObject.Parse(response.Content);

            return raw["success"]?.Value<bool>() ?? false;
        } //Подтверждение токена

        //Запросы, производящие десериализацию данных//
        public Task<List<CourseAnalytics>?> GetCourseAnalytics() //Полученые доступных курсов с прогрессом пользователя
        {
            return ExecuteRpcAndDeserialize<List<CourseAnalytics>>(
                "get_courses_analytics");
        }
        public Task<List<UserStat>?> GetUsersStats() //Полученые доступных курсов с прогрессом пользователя
        {
            return ExecuteRpcAndDeserialize<List<UserStat>>(
                "get_users_stats");
        }
        public Task<List<Course>?> GetCoursesData(int? p_user_id) //Полученые доступных курсов с прогрессом пользователя
        {
            return ExecuteRpcAndDeserialize<List<Course>>(
                "get_courses_data",
                new { p_user_id });
        }
        public Task<List<CourseTask>?> GetTasksData(int p_course_id, int? p_user_id) //Получение заданий выбранного курса с результатами пользователя
        {
            return ExecuteRpcAndDeserialize<List<CourseTask>>(
            "get_tasks_data",
            new { p_course_id, p_user_id });
        }
        public async Task<List<CompletedTaskInfo>?> GetCompletedTasks(int? userId) //Получение прогресса пользователя
        {
            return await ExecuteRpcAndDeserialize<List<CompletedTaskInfo>>(
                "get_completed_tasks",
                new
                {
                    p_user_id = userId
                });
        }
        public async Task<List<Achievement>?> GetAchievementStats(int? userId)
        {
            return await ExecuteRpcAndDeserialize<List<Achievement>>(
                "get_user_achievements",
                new
                {
                    p_user_id = userId
                });
        } //Получение прогресса пользователя
        public async Task<ResetTokenResult?> CreateToken(string p_login, string p_token, string p_type)
        {
            var response = await ExecuteRpc(
                "create_token",
                new
                {
                    p_login,
                    p_token,
                    p_type
                });
            try
            {
                return JsonConvert
                    .DeserializeObject<ResetTokenResult>(
                        response.Content);
            }
            catch
            {
                return null;
            }
        } //Создание токенов подтверждений
        public async Task<BlockResponse?> GetBlockData(int p_id) //Получение данных блока
        {
            var response = await ExecuteRpc(
                "get_block_data",
                new
                {
                    p_id
                });
            try
            {
                return JsonConvert
                   .DeserializeObject<BlockResponse>(
                       response.Content);
            }
            catch
            {
                return null;
            }
        }
        public async Task<CourseProgress?> GetCourseData(int? p_user_id, int p_course_id) //Обновление данных курса
        {
            var response = await ExecuteRpc(
                "get_course_data",
                new
                {
                    p_user_id,
                    p_course_id
                });
            try
            {
                return JsonConvert
                    .DeserializeObject<CourseProgress>(
                        response.Content);
            }
            catch
            {
                return null;
            }
        }
        public async Task<ProfileStats?> GetProfileStats(int? userId)
        {
            return await ExecuteRpcAndDeserialize<ProfileStats>(
                "get_profile_stats",
                new
                {
                    p_user_id = userId
                });
        } //Получение прогресса пользователя
        //Запросы, возвращающие bool//
        public async Task<bool> CreateToken(string p_token, int? p_user_id) //Создание токена для автоматической авторизации
        {
            var response = await ExecuteRpc(
                "create_session_token",
                new { p_token, p_user_id });

            return response.IsSuccessful;
        }
        public async Task<bool> DeleteToken(string p_token) //Удаление токена для автоматической авторизации
        {
            var response = await ExecuteRpc(
                "delete_token",
                new { p_token });

            return response.IsSuccessful;
        }
        public async Task<bool> SubmitResult(int? p_user_id, int p_task_id, int p_score, int p_course_id)
        {
            var response = await ExecuteRpc(
                "submit_result",
                new { p_user_id, p_task_id, p_score, p_course_id });

            return response.IsSuccessful;
        } //Отправка результата в базу данных
        public async Task<bool> ChangePassword(string p_login, string p_password)
        {
            var response = await ExecuteRpc(
                "change_password",
                new { p_login, p_password });

            return response.IsSuccessful;
        } //Изменение пароля
        public async Task<bool> DeleteAccount(string p_login)
        {
            var response = await ExecuteRpc(
                "delete_account",
                new { p_login });

            return response.IsSuccessful;
        } //Удаление аккаунта
        public async Task<bool> CheckPassword(int? p_user_id, string p_password)
        {
            var response = await ExecuteRpc("password_check", new
            {
                p_user_id,
                p_password,
            });
            if (!response.IsSuccessful)
                return false;

            var raw = JObject.Parse(response.Content);

            return raw["success"]?.Value<bool>() ?? false;
        } //Проверка пароля

        //Классы для преобразования
        public class BlockResponse
        {
            public string type { get; set; }
            public object content { get; set; }
        }
        public class CourseProgress
        {
            public int completed_tasks { get; set; }
            public int total_tasks { get; set; }
        }
        public class ResetTokenResult
        {
            public bool success { get; set; }
            public string user_login { get; set; } = "";
        }
        public class ProfileStats
        {
            public int level { get; set; }

            public int total_results { get; set; }

            public int unique_tasks { get; set; }

            public string favorite_course { get; set; }
        }
    }
}
