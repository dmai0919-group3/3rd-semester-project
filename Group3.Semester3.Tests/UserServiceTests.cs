using System;
using Group3.Semester3.WebApp.BusinessLayer;
using Group3.Semester3.WebApp.Entities;
using Group3.Semester3.WebApp.Helpers.Exceptions;
using Group3.Semester3.WebApp.Models.Users;
using Moq;
using NUnit.Framework;

namespace Group3.Semester3.WebAppTests
{
    class UserServiceTests
    {
        IUserService userService;
        private Guid _userId = Guid.NewGuid();
        private string _email = "tester@test.com";
        private string _passwordHash = "mQOpSPAsDzsNxwPZxyzM71czeG0jjSARnvQcVbknCRrjnKUMGnHO6QA8v272lr+eNEqwMiSIcJs3JqwffliSfw==";
        private string _passwordSalt = "mU8h6ntsAEQ94pzInUsBNBqdZrbOQtOHOL7+J0HDheJDStjhmRBfEF5JLVb9l6tJm8D9ReWRlxW2N+7oDIA5lzrHjwzQW8g2KzZKgkNjs4qljIYiHH+pwG92ynSgHYh38vkoJ1ltvm/7Z5vI16er1cc3mmI3Y5Sq3LpVrmhIKvA=";
        private User _user;
        
        [SetUp]
        public void Setup()
        {
            _user = new User()
            {
                Id = _userId,
                Email = _email,
                Name = "tester",
                PasswordHash = Convert.FromBase64String(_passwordHash),
                PasswordSalt = Convert.FromBase64String(_passwordSalt)
            };
            
            Helper helper = new Helper();

            helper.MockedUserRepository.Setup(r => r.GetByEmail(_email)).Returns(_user);
            helper.MockedUserRepository.Setup(r => r.Get(_userId)).Returns(_user);
            helper.MockedUserRepository.Setup(r => r.Insert(It.IsAny<User>())).Returns(true);
            helper.MockedUserRepository.Setup(r => r.Update(It.IsAny<User>())).Returns(true);
            helper.MockedUserRepository.Setup(r => r.Delete(_userId)).Returns(true);

            userService = helper.GetUserService();
        }

        [Test]
        public void Test1()
        {
            if(userService == null) {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
        }

        [Test]
        public void TestRegister()
        {
            var registerModel = new RegisterModel()
            {
                Email = "newemail@test.com",
                Name = "tester",
                Password = "12345678"
            };
            try {
                var result = userService.Register(registerModel);
                
                if(result != null)
                {
                    Assert.AreEqual(registerModel.Email, result.Email);
                    Assert.AreEqual(registerModel.Name, result.Name);
                }
                else
                {
                    Assert.Fail("Register result is null");
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            
            // Check short password

            registerModel.Password = "123456";


            Assert.Throws(typeof(ValidationException), () =>
            {
                userService.Register(registerModel);
            });
            
            // Check incorrect email
            registerModel.Email = "not a email";
            registerModel.Password = "12345678";
            
            Assert.Throws(typeof(ValidationException), () =>
            {
                userService.Register(registerModel);
            });
            
            // Try registering existing user
            registerModel.Email = _email;
            
            Assert.Throws(typeof(ValidationException), () =>
            {
                userService.Register(registerModel);
            });
        }
        
        [Test]
        public void TestLogin()
        {
            var model = new AuthenticateModel()
            {
                Email = _email,
                Password = "12345678"
            };

            var result = userService.Login(model);
            Assert.AreEqual(model.Email, result.Email);

            model.Password = "123456789";
            
            Assert.Throws(typeof(ValidationException), () =>
            {
                userService.Login(model);
            });

            model.Email = "other-mail@mail.com";
            model.Password = "12345678";
            
            Assert.Throws(typeof(ValidationException), () =>
            {
                userService.Login(model);
            });
        }
        [Test]
        public void TestDelete()
        {
            var result = userService.Delete(_userId);
            
            Assert.AreEqual(true, result);

            Assert.Throws(typeof(ValidationException), () =>
            {
                userService.Delete(Guid.Empty);
            });
        }

        [Test]
        public void TestGetByEmail()
        {
            var userModel = userService.GetByEmail(_email);
            
            Assert.AreEqual(_email, userModel.Email);

            var nullModel = userService.GetByEmail("wrong@email.com");
            
            Assert.IsNull(nullModel);
        }

        [Test]
        public void TestGetById()
        {
            var userModel = userService.GetById(_userId);
            
            Assert.AreEqual(_email, userModel.Email);
            
            Assert.Throws(typeof(ValidationException), () =>
            {
                userService.GetById(Guid.Empty);
            });
        }

        [Test]
        public void TestPasswordChange()
        {
            var currentUser = new UserModel()
            {
                Id = _userId
            };
            
            var updateUserModel = new UserUpdateModel()
            {
                Email = _email,
                Name = "tester",
                NewPassword = "87654321",
                NewPasswordCheck = "87654321",
                OldPassword = "12345678",
            };

            var resultUser = userService.Update(updateUserModel, currentUser);
            
            Assert.AreNotEqual(_passwordHash, Convert.ToBase64String(resultUser.PasswordHash));
        }

        [Test]
        public void TestUsernameUpdate()
        {
            var currentUser = new UserModel()
            {
                Id = _userId
            };
            
            var updateUserModel = new UserUpdateModel()
            {
                Email = _email,
                Name = "new name",
                OldPassword = "12345678",
            };
            
            var resultUser = userService.Update(updateUserModel, currentUser);
            
            // Check that password hasn't changed
            Assert.AreEqual(_passwordHash, Convert.ToBase64String(resultUser.PasswordHash));
            Assert.AreEqual("new name", resultUser.Name);
        }

        [Test]
        public void TestUpdateDifferentPasswords()
        {
            var currentUser = new UserModel()
            {
                Id = _userId
            };
            
            var updateUserModel = new UserUpdateModel()
            {
                Email = _email,
                OldPassword = "12345678",
                NewPassword = "123456782",
                NewPasswordCheck = "123456783",
            };

            // Test different passwords
            Assert.Throws(typeof(ValidationException), () =>
            {
                userService.Update(updateUserModel, currentUser);
            });
        }
    }
}