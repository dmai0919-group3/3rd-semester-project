using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Repositories;
using Moq;

namespace Group3.Semester3.WebAppTests
{

    public class Helper
    {
        
        public readonly Mock<IUserRepository> MockedUserRepository = new Mock<IUserRepository>();
        public readonly Mock<IFileRepository> MockedFileRepository = new Mock<IFileRepository>();
        public readonly Mock<IAccessService> MockedAccessService = new Mock<IAccessService>();
        public readonly Mock<IGroupRepository> MockedGroupRepository = new Mock<IGroupRepository>();
        public readonly Mock<IAzureService> MockedAzureService = new Mock<IAzureService>();
        public readonly Mock<ISharedFilesRepository> MockedSharedFilesRepository = new Mock<ISharedFilesRepository>();

        public IFileService GetFileService()
        {
            return new FileService(MockedFileRepository.Object, MockedAccessService.Object, 
                MockedGroupRepository.Object, MockedSharedFilesRepository.Object, MockedUserRepository.Object, 
                MockedAzureService.Object);
        }

        public IUserService GetUserService()
        {
            return new UserService(MockedUserRepository.Object);
        }
    }
}