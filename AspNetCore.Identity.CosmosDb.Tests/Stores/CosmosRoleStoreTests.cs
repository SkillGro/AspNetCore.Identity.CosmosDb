﻿using AspNetCore.Identity.CosmosDb.Tests;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetCore.Identity.CosmosDb.Stores.Tests
{
    [TestClass()]
    public class CosmosRoleStoreTests : CosmosIdentityTestsBase
    {
        //private static TestUtilities? utils;
        //private static CosmosUserStore<IdentityUser>? _userStore;
        //private static CosmosRoleStore<IdentityRole>? roleStore;
        //private static Random _random;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            InitializeClass();
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityRole"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        private async Task<IdentityRole> GetMockRandomRoleAsync()
        {
            var role = new IdentityRole($"HUB{GetNextRandomNumber(1000, 9999)}");
            role.NormalizedName = role.Name.ToUpper();
            using var roleStore = _testUtilities.GetRoleStore();
            var result = await roleStore.CreateAsync(role);
            Assert.IsTrue(result.Succeeded);
            return role;
        }

        [TestMethod()]
        public async Task CreateAsyncTest()
        {
            // Act
            // Create a bunch of roles in rapid succession
            using var dbContext = _testUtilities.GetDbContext();
            var currentCount = dbContext.Roles.Count();
            for (int i = 0; i < 35; i++)
            {
                var r = await GetMockRandomRoleAsync();
            }

            // Assert
            Assert.AreEqual(35 + currentCount, dbContext.Roles.Count());

        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            using var userStore = _testUtilities.GetUserStore();
            using var dbContext = _testUtilities.GetDbContext();
            var role = await GetMockRandomRoleAsync(roleStore);
            var user = await GetMockRandomUserAsync(userStore);
            var roleClaim = GetMockClaim();
            await roleStore.AddClaimAsync(role, roleClaim);
            await userStore.AddToRoleAsync(user, role.NormalizedName);

            var roleId = role.Id;

            // Act
            var result = await roleStore.DeleteAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(dbContext.Roles.Where(a => a.Name == role.Name).Count() == 0);
            Assert.IsTrue(dbContext.RoleClaims.Where(a => a.RoleId == roleId).Count() == 0);
            Assert.IsTrue(dbContext.UserRoles.Where(a => a.RoleId == roleId).Count() == 0);
        }

        [TestMethod()]
        public async Task FindByIdAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await roleStore.FindByIdAsync(role.Id);

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task FindByNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await roleStore.FindByNameAsync(role.Name.ToUpper());

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task GetNormalizedRoleNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await roleStore.FindByNameAsync(role.Name.ToUpper());

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task GetRoleIdAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync();

            // Act
            var result = await roleStore.GetRoleIdAsync(role);

            // Assert
            Assert.AreEqual(role.Id, result);
        }

        [TestMethod()]
        public async Task GetRoleNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync();

            // Act
            var result = await roleStore.GetRoleNameAsync(role);

            // Assert
            Assert.AreEqual(role.Name, result);
        }

        [TestMethod()]
        public async Task SetNormalizedRoleNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync();
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            // Act
            await roleStore.SetNormalizedRoleNameAsync(role, newName.ToUpper());

            // Assert
            var result = await roleStore.GetNormalizedRoleNameAsync(role);
            Assert.AreEqual(newName.ToUpper(), result);
        }

        [TestMethod()]
        public async Task SetRoleNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync();
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            // Act
            await roleStore.SetRoleNameAsync(role, newName);

            // Assert
            var result1 = await roleStore.GetRoleNameAsync(role);

            Assert.AreEqual(newName, result1);
        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync(roleStore);
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            role.Name = newName;
            role.NormalizedName = newName.ToLower();

            // Act
            var result = await roleStore.UpdateAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded);
            role = await roleStore.FindByIdAsync(role.Id);
            Assert.AreEqual(newName, role.Name);
            Assert.AreEqual(newName.ToLower(), role.NormalizedName);
        }

        [TestMethod()]
        public async Task GetClaimsAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore();
            var claims = new Claim[] { GetMockClaim(), GetMockClaim(), GetMockClaim() };
            var role = await GetMockRandomRoleAsync();
            await roleStore.AddClaimAsync(role, claims[0], default);
            await roleStore.AddClaimAsync(role, claims[1], default);
            await roleStore.AddClaimAsync(role, claims[2], default);

            // Act
            var result2 = await roleStore.GetClaimsAsync(role, default);

            // Assert
            Assert.AreEqual(3, result2.Count);
        }

        [TestMethod()]
        public async Task AddClaimAsyncTest()
        {
            // Assert
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync();
            var claim = GetMockClaim();

            // Act
            await roleStore.AddClaimAsync(role, claim, default);

            // Assert
            var result2 = await roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(1, result2.Count);

        }
        [TestMethod()]
        public async Task RemoveClaimAsyncTest()
        {
            // Assert
            using var roleStore = _testUtilities.GetRoleStore();
            var role = await GetMockRandomRoleAsync();
            var claim = GetMockClaim();
            await roleStore.AddClaimAsync(role, claim, default);
            var result2 = await roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(1, result2.Count);

            // Act
            await roleStore.RemoveClaimAsync(role, claim, default);

            // Assert
            var result3 = await roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(0, result3.Count);
        }

        [TestMethod]
        public async Task QueryRolesTest()
        {
            // Arrange
            using var roletore = _testUtilities.GetRoleStore();
            var user1 = await GetMockRandomRoleAsync(roletore);

            // Act
            var result = roletore.Roles.ToList();

            // Assert
            Assert.IsTrue(result.Count > 0);
        }
    }
}