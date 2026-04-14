using EventFlow.Application.Exceptions;
using EventFlow.Application.GlobalSettings.Requests;
using EventFlow.Application.Localization;
using EventFlow.Domain.Constansts;
using EventFlow.Domain.GlobalSettings;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalSettingEntity = EventFlow.Domain.GlobalSettings.GlobalSettings;

namespace Eventflow.Application.Tests.GlobalSettings
{
    public class GlobalSettingsServiceTest
    {
        readonly GlobalSettingsServiceFixture _fixture;

        public GlobalSettingsServiceTest()
        {
            _fixture = new GlobalSettingsServiceFixture();
        }


        #region GetByKeyAsync Tests

        [Fact]
        public async Task GetByKeyAsync_ShouldReturnCachedValue_WhenKeyExistsInCache()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            string key = GlobalSettingsKeys.MaxTicketPerUser; 

            int expectedCachedValue = 5;
            object cacheOutput = expectedCachedValue;

            _fixture.MemoryCacheMock
                .Setup(m => m.TryGetValue(key, out cacheOutput))
                .Returns(true);

            var result = await _fixture.Service.GetByKeyAsync(key, token);

            Assert.Equal(expectedCachedValue, result);

            _fixture.SettingsRepoMock.Verify(x => x.GetByKeyAsync(key, token), Times.Never);
        }

        [Fact]
        public async Task GetByKeyAsync_ShouldFetchFromRepoAndSetCache_WhenKeyNotInCache()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            string key = GlobalSettingsKeys.EventEditAllowedDays;

            object nullCacheOutput = null;
            var settingFromRepo = new GlobalSettingEntity { Key = key, Value = 14 };
            _fixture.MemoryCacheMock
                .Setup(m => m.TryGetValue(key, out nullCacheOutput))
                .Returns(false);

            var cacheEntryMock = new Mock<ICacheEntry>();
            _fixture.MemoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntryMock.Object);

            _fixture.SettingsRepoMock
                .Setup(x => x.GetByKeyAsync(key, token))
                .ReturnsAsync(settingFromRepo);

            var result = await _fixture.Service.GetByKeyAsync(key, token);

            Assert.Equal(settingFromRepo.Value, result);
            _fixture.SettingsRepoMock.Verify(x => x.GetByKeyAsync(key, token), Times.Once);

            _fixture.MemoryCacheMock.Verify(m => m.CreateEntry(key), Times.Once);
        }

        [Fact]
        public async Task GetByKeyAsync_ShouldReturnZeroAndSetCache_WhenKeyNotFoundAnywhere()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            string key = "SomeNonExistentKey";

            object nullCacheOutput = null;

            _fixture.MemoryCacheMock
                .Setup(m => m.TryGetValue(key, out nullCacheOutput))
                .Returns(false);

            var cacheEntryMock = new Mock<ICacheEntry>();
            _fixture.MemoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntryMock.Object);

            _fixture.SettingsRepoMock
                .Setup(x => x.GetByKeyAsync(key, token))
                .ReturnsAsync((GlobalSettingEntity?)null);

            var result = await _fixture.Service.GetByKeyAsync(key, token);

            Assert.Equal(0, result);

            _fixture.SettingsRepoMock.Verify(x => x.GetByKeyAsync(key, token), Times.Once);
            _fixture.MemoryCacheMock.Verify(m => m.CreateEntry(key), Times.Once);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFoundException_WhenSettingDoesNotExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int settingId = 1;
            var model = new GlobalSettingsRequestUpdateModel { Value = 10 };

            _fixture.SettingsRepoMock.Setup(x => x.GetAsync(token, settingId)).ReturnsAsync((GlobalSettingEntity?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _fixture.Service.UpdateAsync(settingId, model, token));

            var expectedMessage = ErrorMessages.GlobalSettingNotFound;

            Assert.Equal(expectedMessage, ex.Message);

            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Never);
            _fixture.MemoryCacheMock.Verify(m => m.Remove(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateSettingAndClearCache_WhenSettingExists()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int settingId = 1;

            var model = new GlobalSettingsRequestUpdateModel { Value = 50 };

            var setting = new GlobalSettingEntity
            {
                Id = settingId,
                Key = GlobalSettingsKeys.MaxTicketPerUser, 
                Value = 20 
            };

            _fixture.SettingsRepoMock.Setup(x => x.GetAsync(token, settingId)).ReturnsAsync(setting);

            await _fixture.Service.UpdateAsync(settingId, model, token);

            Assert.Equal(model.Value, setting.Value);

            _fixture.SettingsRepoMock.Verify(x => x.Update(setting), Times.Once);
            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);

            _fixture.MemoryCacheMock.Verify(m => m.Remove(setting.Key), Times.Once);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldCreateSettingAndReturnId_WhenAllDataIsCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var model = new GlobalSettingsRequestCreateModel
            {
                Key = "NewTestKey",
                Value = 100
            };

            var createdSetting = (GlobalSettingEntity?)null;

            _fixture.SettingsRepoMock
                .Setup(x => x.AddAsync(token, It.IsAny<GlobalSettingEntity>()))
                .Callback<CancellationToken, GlobalSettingEntity>((_, setting) =>
                {
                    setting.Id = 1;
                    createdSetting = setting;
                })
                .Returns(Task.CompletedTask);

            var result = await _fixture.Service.CreateAsync(model, token);

            
            Assert.NotNull(createdSetting);
            Assert.Equal(model.Key, createdSetting.Key);
            Assert.Equal(model.Value, createdSetting.Value);

            Assert.Equal(createdSetting.Id, result);

            _fixture.SettingsRepoMock.Verify(x => x.AddAsync(token, createdSetting), Times.Once);
            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);
        }

        #endregion
    }
}
