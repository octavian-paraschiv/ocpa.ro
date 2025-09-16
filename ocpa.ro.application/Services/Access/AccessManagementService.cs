using Microsoft.AspNetCore.Http;
using ocpa.ro.domain.Abstractions;
using ocpa.ro.domain.Abstractions.Access;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Entities;
using ocpa.ro.domain.Exceptions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ocpa.ro.application.Services.Access
{


    public class AccessManagementService : BaseService, IAccessManagementService
    {
        private readonly IApplicationDbContext _dbContext = null;

        public AccessManagementService(IHostingEnvironmentService hostingEnvironment,
           ILogger logger, IApplicationDbContext dbContext)
           : base(hostingEnvironment, logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public IEnumerable<UserType> AllUserTypes()
            => [.. _dbContext.UserTypes];

        public IEnumerable<Application> GetApplications()
        {
            try
            {
                return [.. _dbContext.Applications];
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return [];
        }

        public Application SaveApplication(Application app, out bool inserted)
        {
            Application dbu = null;
            inserted = false;

            try
            {
                var id = app.Id;

                dbu = _dbContext.Applications.FirstOrDefault(a => id == a.Id);
                if (dbu?.Builtin ?? false)
                    throw new ExtendedException("ERR_EDIT_BUILT_IN_APP");

                bool newEntry = dbu == null;

                dbu ??= new Application();

                dbu.LoginRequired = app.LoginRequired;
                dbu.Code = app.Code;
                dbu.Name = app.Name;
                dbu.AdminMode = app.AdminMode;

                if (newEntry)
                {
                    dbu.Builtin = false; // Can't create a new bultin app
                    if (_dbContext.Insert(dbu) > 0)
                        inserted = true;
                    else
                        dbu = null;
                }
                else
                {
                    if (_dbContext.Update(dbu) <= 0)
                        dbu = null;
                }
            }
            catch (ExtendedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);
                dbu = null;
            }

            return dbu;
        }

        public int DeleteApplication(int appId)
        {
            try
            {
                var dbu = _dbContext.Applications.FirstOrDefault(u => u.Id == appId);
                if (dbu == null)
                    return StatusCodes.Status404NotFound;

                if (dbu.Builtin)
                    throw new ExtendedException("ERR_DELETE_BUILTIN_APP");

                if (_dbContext.ApplicationMenus.Any(am => am.ApplicationId == appId))
                    throw new ExtendedException("ERR_DELETE_APP_MENU_ASSOCIATIONS");

                if (_dbContext.ApplicationUsers.Any(am => am.ApplicationId == appId))
                    throw new ExtendedException("ERR_DELETE_APP_USER_ASSOCIATIONS");

                if (_dbContext.Delete(dbu) > 0)
                    return StatusCodes.Status200OK;
            }
            catch (ExtendedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return StatusCodes.Status400BadRequest;
        }

        //------------------------

        public IEnumerable<Menu> GetMenus()
        {
            try
            {
                return [.. _dbContext.Menus];
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return [];
        }

        public Menu SaveMenu(Menu menu, out bool inserted)
        {
            Menu dbu = null;
            inserted = false;

            try
            {
                var id = menu.Id;

                dbu = _dbContext.Menus.FirstOrDefault(a => id == a.Id);
                if (dbu?.Builtin ?? false)
                    throw new ExtendedException("ERR_EDIT_BUILTIN_MENU");

                bool newEntry = dbu == null;

                dbu ??= new Menu();

                dbu.DisplayModeId = menu.DisplayModeId;
                dbu.MenuIcon = menu.MenuIcon;
                dbu.Name = menu.Name;
                dbu.Url = menu.Url;

                if (newEntry)
                {
                    dbu.Builtin = false; // can't create a new builtin menu

                    if (_dbContext.Insert(dbu) > 0)
                        inserted = true;
                    else
                        dbu = null;
                }
                else
                {
                    if (_dbContext.Update(dbu) <= 0)
                        dbu = null;
                }
            }
            catch (ExtendedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);
                dbu = null;
            }

            return dbu;
        }

        public int DeleteMenu(int menuId)
        {
            try
            {
                var dbu = _dbContext.Menus.FirstOrDefault(u => u.Id == menuId);
                if (dbu == null)
                    return StatusCodes.Status404NotFound;

                if (dbu.Builtin)
                    throw new ExtendedException("ERR_DELETE_BUILTIN_MENU");

                if (_dbContext.ApplicationMenus.Any(am => am.MenuId == menuId))
                    throw new ExtendedException("ERR_DELETE_MENU_APP_ASSOCIATIONS");

                if (_dbContext.Delete(dbu) > 0)
                    return StatusCodes.Status200OK;
            }
            catch (ExtendedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return StatusCodes.Status400BadRequest;
        }

        //------------------------

        public IEnumerable<ApplicationMenu> GetApplicationMenus()
        {
            try
            {
                return [.. _dbContext.ApplicationMenus];
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return [];
        }

        public ApplicationMenu SaveApplicationMenu(int appId, int menuId, out bool inserted)
        {
            inserted = false;

            ValidateAppId(appId);
            ValidateMenuId(menuId);

            try
            {
                var dbu = _dbContext.ApplicationMenus.FirstOrDefault(a => a.ApplicationId == appId && a.MenuId == menuId);
                if (dbu != null)
                    return dbu; // Already present

                dbu = new ApplicationMenu
                {
                    ApplicationId = appId,
                    MenuId = menuId,
                };

                if (_dbContext.Insert(dbu) > 0)
                {
                    inserted = true;
                    return dbu;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return null;
        }

        public int DeleteApplicationMenu(int appId, int menuId)
        {
            ValidateAppId(appId);
            ValidateMenuId(menuId);

            try
            {
                var dbu = _dbContext.ApplicationMenus.FirstOrDefault(a => a.ApplicationId == appId && a.MenuId == menuId);
                if (dbu == null)
                    return StatusCodes.Status404NotFound;

                if (_dbContext.Delete(dbu) > 0)
                    return StatusCodes.Status200OK;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return StatusCodes.Status400BadRequest;
        }

        //------------------------

        public IEnumerable<ApplicationUser> GetAppsForUser(int userId)
        {
            ValidateUserId(userId);

            try
            {
                return _dbContext.ApplicationUsers
                    .AsEnumerable()
                    .Where(au => au.UserId == userId)
                    .AsEnumerable();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return Array.Empty<ApplicationUser>();
        }

        public void SaveAppsForUser(int userId, IEnumerable<ApplicationUser> appsForUser)
        {
            DeleteAppsForUser(userId, true);

            if (appsForUser?.Any() ?? false)
            {
                foreach (ApplicationUser au in appsForUser)
                {
                    au.UserId = userId; // should already be set like this, but anyways
                    if (_dbContext.Insert(au) <= 0)
                        throw new ExtendedException("ERR_FAIL_SAVE_APPS_FOR_USER");
                }
            }
        }

        public void DeleteAppsForUser(int userId, bool saveContext)
        {
            try
            {
                var query = $"DELETE from ApplicationUser WHERE UserId={userId}";
                _dbContext.ExecuteSqlRaw(query);
            }
            catch
            {
                var msg = saveContext ? "ERR_FAIL_DELETE_APPS_FOR_USER" : "ERR_FAIL_CLEAR_APPS_FOR_USER";
                throw new ExtendedException(msg);
            }
        }

        //------------------------

        private void ValidateAppId(int appId)
        {
            try
            {
                var app = _dbContext.Applications.FirstOrDefault(a => a.Id == appId);
                if (app == null)
                    throw new ExtendedException("ERR_APP_NOT_FOUND");
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }

        private void ValidateMenuId(int menuId)
        {
            try
            {
                var menu = _dbContext.Menus.FirstOrDefault(a => a.Id == menuId);
                if (menu == null)
                    throw new ExtendedException("ERR_MENU_NOT_FOUND");
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }

        private void ValidateUserId(int userId)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(a => a.Id == userId);
                if (user == null)
                    throw new ExtendedException("ERR_USER_NOT_FOUND");
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }
    }
}
