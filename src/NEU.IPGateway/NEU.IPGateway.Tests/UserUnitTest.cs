using NEU.IPGateway.UI.Services;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace NEU.IPGateway.Tests
{
    public class UserUnitTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCase("20183523", "jijife", "2938")]
        [TestCase("20183873", "fiej23892837", "")]
        [TestCase("2293727632", "afoiewjifjwio", "fsds")]
        public async Task AddUserTest(string username, string password, string pin)
        {
            var service = new UserStorageService();
            await service.SaveUser(username, password, pin);
            var user = await service.GetUser(username);
            if (user.Username != username)
                Assert.Fail("��ȡ���û�����ƥ��");
            if (password != await service.DecryptedUserPassword(username, pin))
                Assert.Fail("��ȡ�����벻ƥ��");
            if (string.IsNullOrEmpty(pin) == await service.CheckUserPinExist(username))
                Assert.Fail("���PIN�����Դ���");


            await service.DeleteUser(username);
            
        }

        [Test]
        public async Task DeleteTest()
        {
            var service = new UserStorageService();
            var users = await service.GetUsers();
            foreach (var item in users)
            {
                await service.DeleteUser(item.Username);
            }

            var newUsers = await service.GetUsers();

            if (newUsers.ToList().Count != 0)
                Assert.Fail("ɾ��ʧ��");
        }

        [Test]
        public async Task DefaultTest()
        {
            var service = new UserStorageService();
            await service.SaveUser("3202348", "432vcsvds", "");
            await service.SaveUser("3234234", "", "");
            await service.SaveUser("322332", "rdfsdfwf", "");

            Assert.AreNotEqual(await service.GetDefaultUser(), null, "Ĭ���û�������");
            await service.SetDefaultUser("322332");

            Assert.AreEqual((await service.GetDefaultUser()).Username, "322332", "�û�Ĭ������ʧ��");

            var users = await service.GetUsers();
            foreach (var item in users)
            {
                await service.DeleteUser(item.Username);
            }


        }

    }
}