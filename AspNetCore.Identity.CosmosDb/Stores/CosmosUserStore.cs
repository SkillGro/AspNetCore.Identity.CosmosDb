﻿using AspNetCore.Identity.CosmosDb.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Identity.CosmosDb.Stores
{
    /// <summary>
    /// Cosmos DB User store
    /// </summary>
    /// <typeparam name="TUserEntity"></typeparam>
    public class CosmosUserStore<TUserEntity> : IdentityStoreBase,
        IUserStore<TUserEntity>,
        IUserRoleStore<TUserEntity>,
        IUserEmailStore<TUserEntity>,
        IUserPasswordStore<TUserEntity>,
        IUserPhoneNumberStore<TUserEntity>,
        IUserLoginStore<TUserEntity> where TUserEntity : IdentityUser, new()
    {
        private readonly IRepository _repo;

        private bool _disposed;

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo"></param>
        public CosmosUserStore(IRepository repo) : base(repo)
        {
            _repo = repo;
        }

        /// <inheritdoc/>
        public async Task<IdentityResult> CreateAsync(TUserEntity user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(user.Email))
                throw new ArgumentNullException(nameof(user.Email));

            if (string.IsNullOrEmpty(user.UserName))
                throw new ArgumentNullException(nameof(user.UserName));

            try
            {
                _repo.Add(user);
                await _repo.SaveChangesAsync();

            } catch (Exception e)
            {
                return ProcessExceptions(e);
            }
            
            return IdentityResult.Success;
        }

        // <inheritdoc />
        public async Task<IdentityResult> DeleteAsync(TUserEntity user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                _repo.Delete(user);
                await _repo.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return ProcessExceptions(e);
            }

            return IdentityResult.Success;
        }

        // <inheritdoc />
        public async Task<TUserEntity> FindByEmailAsync(string normalizedEmailName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(normalizedEmailName))
                throw new ArgumentNullException(nameof(normalizedEmailName));

            var user = await _repo.Table<TUserEntity>()
                .SingleOrDefaultAsync(_ => _.NormalizedEmail == normalizedEmailName, cancellationToken: cancellationToken);

            return user;
        }

        // <inheritdoc />
        public async Task<TUserEntity> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            var user = await _repo.Table<TUserEntity>()
                .WithPartitionKey(userId)
                .SingleOrDefaultAsync(cancellationToken);

            return user;
        }

        // <inheritdoc />
        public async Task<TUserEntity> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(normalizedUserName))
                throw new ArgumentNullException(nameof(normalizedUserName));

            return await _repo.Table<TUserEntity>().SingleOrDefaultAsync(_ => _.NormalizedUserName == normalizedUserName);
        }

        // <inheritdoc />
        public Task<string> GetEmailAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.Email, cancellationToken));
        }

        // <inheritdoc />
        public Task<bool> GetEmailConfirmedAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.EmailConfirmed, cancellationToken));
        }

        // <inheritdoc />
        public Task<string> GetNormalizedEmailAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.NormalizedEmail, cancellationToken));
        }

        // <inheritdoc />
        public Task<string> GetNormalizedUserNameAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                GetUserProperty(user, user => user.NormalizedUserName, cancellationToken));
        }

        // <inheritdoc />
        public Task<string> GetPasswordHashAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.PasswordHash, cancellationToken));
        }

        // <inheritdoc />
        public Task<string> GetPhoneNumberAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.PhoneNumber, cancellationToken));
        }

        // <inheritdoc />
        public Task<bool> GetPhoneNumberConfirmedAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.PhoneNumberConfirmed, cancellationToken));
        }


        // <inheritdoc />
        public Task<string> GetUserIdAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                 GetUserProperty(user, user => user.Id.ToString(), cancellationToken));
        }

        /// <summary>
        /// Get the user name for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetUserNameAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.UserName, cancellationToken));
        }

        /// <summary>
        /// Determine if a user has a password.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> HasPasswordAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => !string.IsNullOrEmpty(user.PasswordHash), cancellationToken));
        }

        /// <summary>
        /// Sets a user's email address
        /// </summary>
        /// <param name="user"></param>
        /// <param name="emailAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetEmailAsync(TUserEntity user, string emailAddress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(emailAddress))
                throw new ArgumentNullException(nameof(emailAddress));

            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, emailAddress, (u, m) => u.Email = emailAddress, cancellationToken);

        }

        /// <summary>
        /// Sets the confirmation status of a user's email address
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetEmailConfirmedAsync(TUserEntity user, bool confirmed, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, confirmed, (u, m) => u.EmailConfirmed = confirmed, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetNormalizedEmailAsync(TUserEntity user, string normalizedEmail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(normalizedEmail))
                throw new ArgumentNullException(nameof(normalizedEmail));

            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, normalizedEmail, (u, m) => u.NormalizedEmail = normalizedEmail, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetNormalizedUserNameAsync(TUserEntity user, string normalizedName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(normalizedName))
                throw new ArgumentNullException(nameof(normalizedName));

            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, normalizedName, (u, m) => u.NormalizedUserName = normalizedName, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetPasswordHashAsync(TUserEntity user, string passwordHash, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, passwordHash, (u, m) => u.PasswordHash = passwordHash, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetPhoneNumberAsync(TUserEntity user, string phoneNumber, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, phoneNumber, (u, v) => user.PhoneNumber = v, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetPhoneNumberConfirmedAsync(TUserEntity user, bool confirmed, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, confirmed, (u, v) => user.PhoneNumberConfirmed = v, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetUserNameAsync(TUserEntity user, string userName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, userName, (u, m) => u.UserName = userName, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public async Task<IdentityResult> UpdateAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            try
            {
                _repo.Update(user);

                await _repo.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return ProcessExceptions(e);
            }

            return IdentityResult.Success;
        }

        private T GetUserProperty<T>(TUserEntity user, Func<TUserEntity, T> accessor, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return accessor(user);
        }

        private void SetUserProperty<T>(TUserEntity user, T value, Action<TUserEntity, T> setter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (value == null) throw new ArgumentNullException(nameof(value));

            setter(user, value);
        }

        // <inheritdoc />
        public async Task AddLoginAsync(TUserEntity user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (login == null) throw new ArgumentNullException(nameof(login));

            try
            {
                IdentityUserLogin<string> loginEntity = new IdentityUserLogin<string>
                {
                    UserId = user.Id,
                    LoginProvider = login.LoginProvider,
                    ProviderKey = login.ProviderKey,
                    ProviderDisplayName = login.ProviderDisplayName
                };

                _repo.Add(loginEntity);
                await _repo.SaveChangesAsync();
            }
            catch (Exception e)
            {
                // Debugging purposes.
                //var x = e;
                throw;
            }
        }

        // <inheritdoc />
        public async Task RemoveLoginAsync(TUserEntity user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey == null) throw new ArgumentNullException(nameof(providerKey));

            try
            {
                var login = await _repo.Table<IdentityUserLogin<string>>()
                    .SingleOrDefaultAsync(l =>
                        l.UserId == user.Id &&
                        l.LoginProvider == loginProvider &&
                        l.ProviderKey == providerKey
                    );

                if (login != null)
                {
                    _repo.Delete(login);
                    await _repo.SaveChangesAsync();
                }
            }
            catch { }
        }

        // <inheritdoc />
        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            IList<UserLoginInfo> res = _repo
                .Table<IdentityUserLogin<string>>()
                .Where(l => l.UserId == user.Id)
                .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, user.UserName)
                {
                    ProviderDisplayName = l.ProviderDisplayName
                })
                .ToList();

            return Task.FromResult(res);
        }

        // <inheritdoc />
        public async Task<TUserEntity> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey == null) throw new ArgumentNullException(nameof(providerKey));

            var userId = (
                await _repo.Table<IdentityUserLogin<string>>().SingleOrDefaultAsync(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)
            )?.UserId;

            return string.IsNullOrEmpty(userId)
                ? default(TUserEntity)
                : await FindByIdAsync(userId, cancellationToken);
        }

        // <inheritdoc />
        public async Task AddToRoleAsync(TUserEntity user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(normalizedRoleName)) throw new ArgumentNullException(nameof(normalizedRoleName));

            var role = await _repo.Table<IdentityRole>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == normalizedRoleName, cancellationToken: cancellationToken);

            if (role == null) throw new InvalidOperationException("Role not found.");

            try
            {
                IdentityUserRole<string> userRole = new IdentityUserRole<string>
                {
                    RoleId = role.Id,
                    UserId = user.Id
                };

                _repo.Add(userRole);
                await _repo.SaveChangesAsync();
            }
            catch { }
        }

        /// <inheritdoc/>>
        public async Task RemoveFromRoleAsync(TUserEntity user, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<IdentityRole>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == roleName, cancellationToken: cancellationToken);

            if (role != null)
            {
                var userRole = await _repo.Table<IdentityUserRole<string>>().SingleOrDefaultAsync(_ => _.RoleId == role.Id, cancellationToken);
                if (userRole != null)
                {
                    _repo.Delete(userRole);
                    await _repo.SaveChangesAsync();
                }
            }
        }

        // <inheritdoc />
        public async Task<IList<string>> GetRolesAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var roleIds = await _repo
                .Table<IdentityUserRole<string>>()
                .Where(m => m.UserId == user.Id)
                .Select(m => m.RoleId)
                .ToListAsync(cancellationToken);

            IList<string> res = await _repo
                .Table<IdentityRole>()
                .Where(m => roleIds.Contains(m.Id))
                .Select(m => m.Name)
                .ToListAsync(cancellationToken);

            return res;
        }

        // <inheritdoc />
        public async Task<bool> IsInRoleAsync(TUserEntity user, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<IdentityRole>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == roleName, cancellationToken: cancellationToken);

            if (role != null)
            {
                var userRole = await _repo.Table<IdentityUserRole<string>>()
                    .SingleOrDefaultAsync(_ => _.RoleId == role.Id && _.UserId == user.Id, cancellationToken);

                return userRole != null;
            }

            return false;
        }

        // <inheritdoc />
        public async Task<IList<TUserEntity>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<IdentityRole>()
                            .SingleOrDefaultAsync(_ => _.NormalizedName == roleName, cancellationToken: cancellationToken);

            if (role != null)
            {
                var userIds = await _repo.Table<IdentityUserRole<string>>()
                    .Where(m => m.RoleId == role.Id)
                    .Select(m => m.UserId)
                    .ToListAsync(cancellationToken);

                var users = await _repo.Table<TUserEntity>()
                    .Where(m => userIds.Contains(m.Id))
                    .ToListAsync(cancellationToken);

                return users;
            }

            return new List<TUserEntity>();
        }

        // <inheritdoc />
        public void Dispose()
        {
            _disposed = true;
        }
    }
}