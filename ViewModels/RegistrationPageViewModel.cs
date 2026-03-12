using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using UP_4.Models;

namespace UP_4.ViewModels
{

    public class MathExample : ViewModelBase
    {
        private string _expression;
        private string _userAnswer;

        public string Expression
        {
            get => _expression;
            set
            {
                _expression = value;
                OnPropertyChanged();
            }
        }

        public string UserAnswer
        {
            get => _userAnswer;
            set
            {
                _userAnswer = value;
                OnPropertyChanged();
            }
        }

        public int CorrectAnswer { get; set; }
    }


    public partial class RegistrationPageViewModel : ViewModelBase
    {

        [ObservableProperty]
        private string email = "";
        [ObservableProperty]
        private string password = "";
        [ObservableProperty]
        private string emailCode = "";
        [ObservableProperty]
        private string franchiseCode = "";
        [ObservableProperty]
        private string message = "";

        private string generatedEmailCode = "";

        public ObservableCollection<MathExample> Examples { get; } = new();

        public RegistrationPageViewModel()
        {
            GenerateExamples();
        }

        private void GenerateEmailCode()
        {
            Random rnd = new Random();
            generatedEmailCode = rnd.Next(100000, 999999).ToString();
        }

        [RelayCommand]
        private void SendEmailCode()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Введите email";
                return;
            }

            if (!IsValidEmail(Email))
            {
                Message = "Некорректный email";
                return;
            }

            GenerateEmailCode();
            Message = $"Код подтверждения (эмуляция email): {generatedEmailCode}";
        }

        private void GenerateExamples()
        {
            Examples.Clear();

            Random rnd = new Random();
            string chars = "+-*";

            int a = rnd.Next(1, 20);
            int b = rnd.Next(1, 20);
            int c = rnd.Next(1, 20);
            int d = rnd.Next(1, 20);

            char op1 = chars[rnd.Next(chars.Length)];
            char op2 = chars[rnd.Next(chars.Length)];
            char op3 = chars[rnd.Next(chars.Length)];

            string expression = $"{a} {op1} {b} {op2} {c} {op3} {d}";

            int result = (int)new System.Data.DataTable().Compute(expression, "");

            Examples.Add(new MathExample
            {
                Expression = expression + " = ",
                CorrectAnswer = result
            });
        }

        private bool CheckAnswers()
        {
            foreach (var example in Examples)
            {
                if (string.IsNullOrWhiteSpace(example.UserAnswer))
                {
                    Message = "Решите CAPTCHA";
                    return false;
                }

                if (!int.TryParse(example.UserAnswer, out int ans))
                {
                    Message = "Введите число";
                    return false;
                }

                if (ans != example.CorrectAnswer)
                {
                    Message = "CAPTCHA решена неверно";
                    return false;
                }
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[0-9])(?=.*[!@#$%^&*]).{8,}$");
        }

        private async Task Reg()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(EmailCode) ||
                    string.IsNullOrWhiteSpace(FranchiseCode))
                {
                    Message = "Заполните все поля";
                    return;
                }

                if (!IsValidEmail(Email))
                {
                    Message = "Некорректный email";
                    return;
                }

                if (!IsValidPassword(Password))
                {
                    Message = "Пароль минимум 8 символов, цифра и спецсимвол";
                    return;
                }

                if (EmailCode != generatedEmailCode)
                {
                    Message = "Неверный код подтверждения email";
                    return;
                }

                if (FranchiseCode != "FRANCH2025")
                {
                    Message = "Неверный код франчайзи";
                    return;
                }

                if (!CheckAnswers())
                    return;

                var exists = await db.Users.AnyAsync(x => x.Email == Email);
                if (exists)
                {
                    Message = "Пользователь уже существует";
                    return;
                }

                var newUser = new User
                {
                    Email = Email,
                    Password = Password,
                    IdRole = 1
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();

                Message = "Регистрация успешна";

                MainWindowViewModel.Instance.CurrentViewModel = new MainPageViewModel(newUser);
            }
            catch (Exception ex)
            {
                Message = $"Ошибка при регистрации: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task RegistrationAdd()
        {
            await Reg();
        }

        [RelayCommand]
        private void Back()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new AuthPageViewModel();
        }
    }
}