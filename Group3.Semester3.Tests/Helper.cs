using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Repositories;
using Microsoft.Extensions.Configuration;

namespace Group3.Semester3.WebAppTests
{

    public static class Helper
    {

        private static IUserService _userService;
        private static IFileService _fileService;
        private static IUserRepository _userRepository;
        private static IFileRepository _fileRepository;
        private static IConfiguration _configuration;
        
        public static IUserService GetUserService()
        {
            if (_userService == null)
            {
                _userService = new UserService(GetUserRepository());
            }

            return _userService;
        }

        public static IUserRepository GetUserRepository()
        {
            if (_userRepository == null)
            {
                _userRepository = new UserRepository(ConfigurationRoot());
            }

            return _userRepository;
        }

        public static IFileService GetFileService()
        {
            if (_fileService == null)
            {
                _fileService = new FileService(GetFileRepository());
            }

            return _fileService;
        }

        public static IFileRepository GetFileRepository()
        {
            if (_fileRepository == null)
            {
                _fileRepository = new FileRepository(ConfigurationRoot());
            }

            return _fileRepository;
        }

        public static IConfiguration ConfigurationRoot()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            return _configuration;
        }
    }
}