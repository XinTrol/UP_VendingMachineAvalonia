using Microsoft.EntityFrameworkCore;
using UP_4.Models;
using UP_4.ViewModels;

namespace ViewModelTests
{
    public class UnitTest1 : IDisposable
    {
        private readonly SavukovContext _dbContext;

        public UnitTest1()
        {
            // Создаём синглтон, если его ещё нет
            if (MainWindowViewModel.Instance == null)
                MainWindowViewModel.Instance = new MainWindowViewModel();

            MainWindowViewModel.Instance.CurrentViewModel = null;

            var options = new DbContextOptionsBuilder<SavukovContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new SavukovContext(options);
        }

        public void Dispose() => _dbContext?.Dispose();

        [Fact]
        public void Enter_ValidCredentials_ShouldNavigateToMainPage()
        {
            var role = new Role
            {
                Id = 1,
                Name = "User"
            };
            _dbContext.Roles.Add(role);
            _dbContext.SaveChanges();

            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                Password = "Passw0rd!",
                IdRole = 1
            };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var authVm = new AuthPageViewModel(_dbContext);
            authVm.Email = "test@example.com";
            authVm.Password = "Passw0rd!";

            authVm.EnterCommand.Execute(null);

            Assert.IsType<MainPageViewModel>(MainWindowViewModel.Instance.CurrentViewModel);
        }

        [Fact]
        public void Enter_InvalidPassword_ShouldShowErrorMessage()
        {
            var user = new User { Email = "test@example.com", Password = "Passw0rd!", IdRole = 1 };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            var authVm = new AuthPageViewModel(_dbContext);
            authVm.Email = "test@example.com";
            authVm.Password = "wrong";

            authVm.EnterCommand.Execute(null);

            Assert.Equal("Пользователь не найден", authVm.Message);
            Assert.Null(MainWindowViewModel.Instance.CurrentViewModel);
        }

        [Fact]
        public void Enter_NonExistentEmail_ShouldShowErrorMessage()
        {
            var authVm = new AuthPageViewModel(_dbContext);
            authVm.Email = "notexists@example.com";
            authVm.Password = "any";

            authVm.EnterCommand.Execute(null);

            Assert.Equal("Пользователь не найден", authVm.Message);
        }

        [Fact]
        public void Registration_ShouldNavigateToRegistrationPage()
        {
            var authVm = new AuthPageViewModel(_dbContext);
            authVm.RegistrationCommand.Execute(null);
            Assert.IsType<RegistrationPageViewModel>(MainWindowViewModel.Instance.CurrentViewModel);
        }

        [Fact]
        public void IsValidEmail_ValidEmail_ReturnsTrue()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            Assert.True(regVm.IsValidEmail("test@example.com"));
            Assert.True(regVm.IsValidEmail("user.name@domain.co.uk"));
        }

        [Fact]
        public void IsValidEmail_InvalidEmail_ReturnsFalse()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            Assert.False(regVm.IsValidEmail("test@"));
            Assert.False(regVm.IsValidEmail("test.com"));
            Assert.False(regVm.IsValidEmail("test@domain."));
        }

        [Fact]
        public void IsValidPassword_ValidPassword_ReturnsTrue()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            Assert.True(regVm.IsValidPassword("Qwerty123!"));
            Assert.True(regVm.IsValidPassword("Password1@"));
        }

        [Fact]
        public void IsValidPassword_NoDigits_ReturnsFalse()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            Assert.False(regVm.IsValidPassword("Qwerty!"));
        }

        [Fact]
        public void IsValidPassword_NoSpecialChars_ReturnsFalse()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            Assert.False(regVm.IsValidPassword("Qwerty123"));
        }

        [Fact]
        public void IsValidPassword_TooShort_ReturnsFalse()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            Assert.False(regVm.IsValidPassword("Qwe1!"));
        }

        [Fact]
        public void GenerateEmailCode_Always_Returns6DigitCode()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            regVm.GenerateEmailCode();
            Assert.Equal(6, regVm.GeneratedEmailCode.Length);
            Assert.True(int.TryParse(regVm.GeneratedEmailCode, out _));
        }

        [Fact]
        public void Reg_ValidEmailCode_ShouldNotShowCodeError()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            regVm.Email = "new@example.com";
            regVm.Password = "Qwerty123!";
            regVm.GenerateEmailCode();
            regVm.EmailCode = regVm.GeneratedEmailCode;
            regVm.FranchiseCode = "FRANCH2025";
            regVm.Examples.Clear();
            regVm.Examples.Add(new MathExample { CorrectAnswer = 5, UserAnswer = "5" });

            regVm.RegistrationAddCommand.Execute(null);

            Assert.DoesNotContain("Неверный код подтверждения email", regVm.Message);
        }

        [Fact]
        public void Reg_InvalidFranchiseCode_ShouldShowError()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            regVm.Email = "new@example.com";
            regVm.Password = "Qwerty123!";
            regVm.GenerateEmailCode();
            regVm.EmailCode = regVm.GeneratedEmailCode;
            regVm.FranchiseCode = "WRONG";
            regVm.Examples.Clear();
            regVm.Examples.Add(new MathExample { CorrectAnswer = 5, UserAnswer = "5" });

            regVm.RegistrationAddCommand.Execute(null);

            Assert.Equal("Неверный код франчайзи", regVm.Message);
        }

        [Fact]
        public void Reg_WrongCaptcha_ShouldShowError()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            regVm.Email = "new@example.com";
            regVm.Password = "Qwerty123!";
            regVm.GenerateEmailCode();
            regVm.EmailCode = regVm.GeneratedEmailCode;
            regVm.FranchiseCode = "FRANCH2025";
            regVm.Examples.Clear();
            regVm.Examples.Add(new MathExample { CorrectAnswer = 10, UserAnswer = "5" });

            regVm.RegistrationAddCommand.Execute(null);

            Assert.Equal("CAPTCHA решена неверно", regVm.Message);
        }

        [Fact]
        public async Task Reg_ValidData_ShouldCreateUserAndNavigateToMainPage()
        {
            var regVm = new RegistrationPageViewModel(_dbContext);
            regVm.Email = "newuser@example.com";
            regVm.Password = "Qwerty123!";
            regVm.GenerateEmailCode();
            regVm.EmailCode = regVm.GeneratedEmailCode;
            regVm.FranchiseCode = "FRANCH2025";
            regVm.Examples.Clear();
            regVm.Examples.Add(new MathExample { CorrectAnswer = 7, UserAnswer = "7" });

            await regVm.RegistrationAddCommand.ExecuteAsync(null);

            var createdUser = _dbContext.Users.FirstOrDefault(u => u.Email == "newuser@example.com");
            Assert.NotNull(createdUser);
            Assert.Equal("Qwerty123!", createdUser.Password);
            Assert.Equal(1, createdUser.IdRole);
            Assert.IsType<MainPageViewModel>(MainWindowViewModel.Instance.CurrentViewModel);
        }
    }
}