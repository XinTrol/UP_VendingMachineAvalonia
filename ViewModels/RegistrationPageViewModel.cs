using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
        [ObservableProperty] string email = "";
        [ObservableProperty] string password = "";
        [ObservableProperty] string message = "";
        public ObservableCollection<MathExample> Examples { get; } = new();

        public RegistrationPageViewModel()
        {
            GenerateExamples();
        }

        void GenerateExamples()
        {
            Examples.Clear();
            Random rnd = new Random();
            string chars = "-+*";

            for (int i = 0; i < 3; i++)
            {
                int a = rnd.Next(1, 100);
                int b = rnd.Next(1, 100);
                char c = chars[rnd.Next(chars.Length)];

                var example = new MathExample();

                switch (c)
                {
                    case '+':
                        example.CorrectAnswer = a + b;
                        break;
                    case '-':
                        example.CorrectAnswer = a - b;
                        break;
                    case '*':
                        example.CorrectAnswer = a * b;
                        break;
                }

                example.Expression = $"{a} {c} {b} = ";
                Examples.Add(example);
            }

        }

        private bool CheckAnswers()
        {
            foreach (var example in Examples)
            {

                if (string.IsNullOrWhiteSpace(example.UserAnswer))
                {
                    Message = "Решите все примеры!";
                    return false;
                }

                if (!int.TryParse(example.UserAnswer, out int userAnswer))
                {
                    Message = $"В примере '{example.Expression}' введите число!";
                    return false;
                }

                if (userAnswer != example.CorrectAnswer)
                {
                    Message = $"Неверный ответ в примере '{example.Expression}'";
                    return false;
                }
            }

            return true;
        }

        private async Task Reg()
        {
            
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "Заполните все поля!";
                return;
            }

            var exists = await db.Users.AnyAsync(d => d.Email == Email);
            if (exists)
            {
                Message = "Пользователь уже существует.";
                return;
            }

            if (!CheckAnswers())
                return;

            var newUser = new User
            {
                Email = Email,
                Password = Password,
                IdRole = 1
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            Message = "Регистрация успешна!";
            Email = "";
            Password = "";

            GenerateExamples(); // Генерируем новые примеры
        }

        [RelayCommand]
        public async Task RegistrationAdd()
        {
            await Reg();
        }

        [RelayCommand]
        public async Task Back()
        {
            MainWindowViewModel.Instance.CurrentViewModel = new AuthPageViewModel();
        }
    }
}